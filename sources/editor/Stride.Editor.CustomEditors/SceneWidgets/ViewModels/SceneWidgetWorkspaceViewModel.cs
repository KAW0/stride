// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Stride.Assets.Presentation.AssetEditors.EntityHierarchyEditor.ViewModels;
using Stride.Assets.Presentation.SceneEditor;
using Stride.Assets.Presentation.SceneEditor.Widgets;
using Stride.Assets.Presentation.SceneEditor.Widgets.ViewModels;
using Stride.Core;
using Stride.Core.Assets;
using Stride.Core.Assets.Editor.ViewModel;
using Stride.Core.Diagnostics;
using Stride.Core.Extensions;
using Stride.Core.Presentation.Commands;
using Stride.Core.Presentation.Services;
using Stride.Core.Presentation.ViewModels;
using Stride.Editor.CustomEditors.Infrastructure;
using Stride.Editor.CustomEditors.Services;

namespace Stride.Editor.CustomEditors.SceneWidgets.ViewModels;

internal sealed class SceneWidgetWorkspaceViewModel : DispatcherViewModel, ISceneWidgetWorkspaceViewModel
{
    private static readonly Logger Logger = GlobalLogger.GetLogger(nameof(SceneWidgetWorkspaceViewModel));
    private readonly EntityHierarchyEditorViewModel editor;
    private readonly ISceneWidgetService? widgetService;
    private readonly ISceneWidgetServiceViewBridge? widgetViewBridge;
    private readonly ObservableCollection<SceneWidgetTypeViewModel> availableWidgets = [];
    private readonly ObservableCollection<SceneWidgetProfileViewModel> availableProfiles = [];
    private bool isApplyingProfile;
    private bool isWidgetDragInProgress;
    private SceneWidgetProfileViewModel? selectedProfile;

    public bool IsAvailable => widgetService != null;

    public ReadOnlyObservableCollection<SceneWidgetTypeViewModel> AvailableWidgets { get; }

    public ReadOnlyObservableCollection<SceneWidgetProfileViewModel> AvailableProfiles { get; }

    IEnumerable<ISceneWidgetTypeViewModel> ISceneWidgetWorkspaceViewModel.AvailableWidgets => AvailableWidgets;

    IEnumerable<ISceneWidgetProfileViewModel> ISceneWidgetWorkspaceViewModel.AvailableProfiles => AvailableProfiles;

    public SceneWidgetPlacementCollectionViewModel Top { get; }

    public SceneWidgetPlacementCollectionViewModel Left { get; }

    public SceneWidgetPlacementCollectionViewModel Right { get; }

    public SceneWidgetPlacementCollectionViewModel Bottom { get; }

    IEnumerable<ISceneWidgetInstanceViewModel> ISceneWidgetWorkspaceViewModel.TopWidgets => Top.Items;

    IEnumerable<ISceneWidgetInstanceViewModel> ISceneWidgetWorkspaceViewModel.LeftWidgets => Left.Items;

    IEnumerable<ISceneWidgetInstanceViewModel> ISceneWidgetWorkspaceViewModel.RightWidgets => Right.Items;

    IEnumerable<ISceneWidgetInstanceViewModel> ISceneWidgetWorkspaceViewModel.BottomWidgets => Bottom.Items;

    public bool HasTopWidgets => Top.HasItems;

    public bool HasLeftWidgets => Left.HasItems;

    public bool HasRightWidgets => Right.HasItems;

    public bool HasBottomWidgets => Bottom.HasItems;

    public bool ShowTopWidgets => HasTopWidgets || isWidgetDragInProgress;

    public bool ShowLeftWidgets => HasLeftWidgets || isWidgetDragInProgress;

    public bool ShowRightWidgets => HasRightWidgets || isWidgetDragInProgress;

    public bool ShowBottomWidgets => HasBottomWidgets || isWidgetDragInProgress;

    public SceneWidgetProfileViewModel? SelectedProfile
    {
        get => selectedProfile;
        set
        {
            if (!SetValue(ref selectedProfile, value))
                return;

            SaveSelectedProfileCommand.IsEnabled = value != null;
            SetSelectedProfileAsDefaultCommand.IsEnabled = value != null;
            RenameSelectedProfileCommand.IsEnabled = value != null;
            if (!isApplyingProfile && value != null)
                ApplyProfileSelection(value);
        }
    }

    ISceneWidgetProfileViewModel ISceneWidgetWorkspaceViewModel.SelectedProfile
    {
        get => SelectedProfile!;
        set => SelectedProfile = (value as SceneWidgetProfileViewModel)!;
    }

    public ICommandBase AddWidgetCommand { get; }

    public ICommandBase CreateLocalProfileCommand { get; }

    public ICommandBase CreateProjectProfileCommand { get; }

    public ICommandBase SaveSelectedProfileCommand { get; }

    public ICommandBase SetSelectedProfileAsDefaultCommand { get; }

    public ICommandBase RenameSelectedProfileCommand { get; }

    public ICommandBase SelectProfileWithPromptCommand { get; }

    public SceneWidgetWorkspaceViewModel(IViewModelServiceProvider serviceProvider, EntityHierarchyEditorViewModel editor)
        : base(serviceProvider)
    {
        this.editor = editor;
        widgetService = serviceProvider.TryGet<ISceneWidgetService>();
        widgetViewBridge = serviceProvider.TryGet<ISceneWidgetServiceViewBridge>();
        Top = new SceneWidgetPlacementCollectionViewModel(SceneWidgetPlacement.Top, NotifyCollectionStateChanged);
        Left = new SceneWidgetPlacementCollectionViewModel(SceneWidgetPlacement.Left, NotifyCollectionStateChanged);
        Right = new SceneWidgetPlacementCollectionViewModel(SceneWidgetPlacement.Right, NotifyCollectionStateChanged);
        Bottom = new SceneWidgetPlacementCollectionViewModel(SceneWidgetPlacement.Bottom, NotifyCollectionStateChanged);
        AvailableWidgets = new ReadOnlyObservableCollection<SceneWidgetTypeViewModel>(availableWidgets);
        AvailableProfiles = new ReadOnlyObservableCollection<SceneWidgetProfileViewModel>(availableProfiles);

        AddWidgetCommand = new AnonymousCommand<SceneWidgetTypeViewModel>(serviceProvider, AddWidget);
        CreateLocalProfileCommand = new AnonymousTaskCommand(serviceProvider, CreateLocalProfile);
        CreateProjectProfileCommand = new AnonymousTaskCommand(serviceProvider, CreateProjectProfile);
        SaveSelectedProfileCommand = new AnonymousCommand(serviceProvider, SaveSelectedProfile, () => SelectedProfile != null);
        SetSelectedProfileAsDefaultCommand = new AnonymousCommand(serviceProvider, SetSelectedProfileAsDefault, () => SelectedProfile != null);
        RenameSelectedProfileCommand = new AnonymousTaskCommand(serviceProvider, RenameSelectedProfile, () => SelectedProfile != null);
        SelectProfileWithPromptCommand = new AnonymousTaskCommand(serviceProvider, SelectProfileWithPrompt, () => AvailableProfiles.Count > 0);

        RefreshAvailableWidgets();
        RefreshProfiles();
        if (widgetService != null)
            widgetService.WidgetDescriptorsChanged += OnWidgetDescriptorsChanged;
    }

    public override void Destroy()
    {
        if (widgetService != null)
            widgetService.WidgetDescriptorsChanged -= OnWidgetDescriptorsChanged;

        ClearAll();
        base.Destroy();
    }

    public void SetWidgetDragInProgress(bool isDragging)
    {
        if (isWidgetDragInProgress == isDragging)
            return;

        isWidgetDragInProgress = isDragging;
        OnPropertyChanged(nameof(ShowTopWidgets));
        OnPropertyChanged(nameof(ShowLeftWidgets));
        OnPropertyChanged(nameof(ShowRightWidgets));
        OnPropertyChanged(nameof(ShowBottomWidgets));
    }

    public void LoadSettings(SceneSettingsData settings)
    {
        if (!IsAvailable || settings == null)
            return;

        RefreshProfiles(settings);
        var profileReference = settings.SelectedWidgetProfile ?? SceneWidgetProfileReference.None();
        if (profileReference.Source == SceneWidgetProfileSource.None)
            profileReference = editor.Asset.Directory.Package.UserSettings.GetValue(PackageSceneSettings.DefaultWidgetProfile) ?? SceneWidgetProfileReference.None();
        if (profileReference.Source != SceneWidgetProfileSource.None)
        {
            var profile = availableProfiles.FirstOrDefault(x => x.Source == profileReference.Source && string.Equals(x.ProfileId, profileReference.ProfileId, StringComparison.OrdinalIgnoreCase));
            if (profile != null)
            {
                SetSelectedProfileSilently(profile);
                ApplyProfile(profile.Profile);
                return;
            }
        }

        SetSelectedProfileSilently(null);
        ApplyProfile(new SceneWidgetProfileData { Widgets = settings.CurrentWidgetLayout?.ToList() ?? [] });
    }

    public void SaveSettings(SceneSettingsData settings)
    {
        if (!IsAvailable || settings == null)
            return;

        settings.CurrentWidgetLayout = CaptureCurrentLayout().Widgets;
        settings.SelectedWidgetProfile = SelectedProfile?.ToReference() ?? SceneWidgetProfileReference.None();
    }

    public void RefreshProfiles(SceneSettingsData? settings = null)
    {
        availableProfiles.Clear();
        var packageSettings = editor.Asset.Directory.Package.UserSettings.GetValue(PackageSceneSettings.LocalWidgetProfiles);

        foreach (var profile in packageSettings ?? Enumerable.Empty<SceneWidgetProfileData>())
        {
            availableProfiles.Add(new SceneWidgetProfileViewModel(SceneWidgetProfileSource.Local, profile.Id, profile.Name, profile.Clone()));
        }

        foreach (var asset in editor.Asset.Directory?.Package?.AllAssets.Where(x => x.Asset is SceneWidgetProfileAsset) ?? Enumerable.Empty<AssetViewModel>())
        {
            if (asset.Asset is not SceneWidgetProfileAsset profileAsset)
                continue;

            var profile = profileAsset.Profile?.Clone() ?? new SceneWidgetProfileData();
            if (string.IsNullOrWhiteSpace(profile.Id))
                profile.Id = asset.Id.ToString();
            availableProfiles.Add(new SceneWidgetProfileViewModel(SceneWidgetProfileSource.Project, profile.Id, asset.Name, profile, asset));
        }

        SelectProfileWithPromptCommand.IsEnabled = availableProfiles.Count > 0;
    }

    private void RefreshAvailableWidgets()
    {
        availableWidgets.Clear();
        if (widgetService != null)
        {
            foreach (var descriptor in widgetService.WidgetDescriptors.Values.OrderBy(x => x.WidgetName, StringComparer.OrdinalIgnoreCase))
            {
                availableWidgets.Add(new SceneWidgetTypeViewModel(descriptor));
            }
        }
    }

    private void AddWidget(SceneWidgetTypeViewModel widgetType)
    {
        if (widgetType == null || widgetService == null)
            return;

        if (!widgetType.AllowMultiple && GetAllItems().Any(x => string.Equals(x.WidgetId, widgetType.WidgetId, StringComparison.OrdinalIgnoreCase)))
            return;

        var item = CreateWidgetInstance(widgetType.WidgetId, widgetType.DefaultPlacement);
        if (item == null)
            return;

        GetCollection(item.Placement).Add(item);
    }

    private SceneWidgetInstanceViewModel? CreateWidgetInstance(string widgetId, SceneWidgetPlacement placement)
    {
        var service = widgetService;
        if (service == null)
            return null;

        try
        {
            var context = new SceneWidgetViewModelContext(editor, editor.SceneViewId);
            var viewModel = service.CreateViewModel(widgetId, ServiceProvider, context);
            var view = service.CreateView(widgetId, viewModel);
            var descriptor = service.WidgetDescriptors[widgetId];
            return new SceneWidgetInstanceViewModel(ServiceProvider, descriptor, viewModel, view, CloseWidget)
            {
                Placement = placement,
            };
        }
        catch (Exception exception)
        {
            Logger.Error($"Unable to create scene widget '{widgetId}'.", exception);
            ServiceProvider.Get<IDialogService>().MessageBoxAsync($"Unable to create scene widget '{widgetId}'.\r\n\r\n{exception.Message}", MessageBoxButton.OK, MessageBoxImage.Warning).Forget();
            return null;
        }
    }

    private void CloseWidget(SceneWidgetInstanceViewModel instance)
    {
        if (instance == null)
            return;

        instance.PlacementCollection?.Remove(instance);
        DestroyInstance(instance);
    }

    private async Task CreateLocalProfile()
    {
        var settings = LoadSceneSettings();
        var profile = CaptureCurrentLayout();
        profile.Id = Guid.NewGuid().ToString("N");
        profile.Name = ComputeLocalProfileName();
        profile.Name = await PromptForProfileName(profile.Name, "Create Local Widget Profile");
        if (string.IsNullOrWhiteSpace(profile.Name))
            return;

        availableProfiles.Add(new SceneWidgetProfileViewModel(SceneWidgetProfileSource.Local, profile.Id, profile.Name, profile));
        SetSelectedProfileSilently(availableProfiles.Last());
        SaveSceneSettings();
        await Task.CompletedTask;
    }

    private async Task CreateProjectProfile()
    {
        var package = editor.Asset.Directory?.Package;
        var directory = editor.Asset.Directory;
        if (package == null || directory == null)
            return;

        var profile = CaptureCurrentLayout();
        profile.Id = Guid.NewGuid().ToString("N");
        profile.Name = ComputeProjectProfileName(package);
        profile.Name = await PromptForProfileName(profile.Name, "Create Project Widget Profile");
        if (string.IsNullOrWhiteSpace(profile.Name))
            return;

        var asset = new SceneWidgetProfileAsset
        {
            Profile = profile.Clone(),
        };
        var assetItem = new AssetItem(profile.Name, asset)
        {
            IsDirty = true,
        };
        var assetViewModel = package.CreateAsset(directory, assetItem, true, null);
        editor.Session.NotifyAssetPropertiesChanged([assetViewModel]);
        RefreshProfiles();
        SetSelectedProfileSilently(availableProfiles.FirstOrDefault(x => x.Source == SceneWidgetProfileSource.Project && x.Asset?.Id == assetViewModel.Id));
        SaveSceneSettings();
        await Task.CompletedTask;
    }

    private void SaveSelectedProfile()
    {
        if (SelectedProfile == null)
            return;

        var captured = CaptureCurrentLayout();
        SelectedProfile.Profile.Widgets = captured.Widgets;
        SelectedProfile.Profile.Version = captured.Version;
        SelectedProfile.Profile.Name = SelectedProfile.DisplayName;

        if (SelectedProfile.Source == SceneWidgetProfileSource.Project && SelectedProfile.Asset?.Asset is SceneWidgetProfileAsset projectAsset)
        {
            projectAsset.Profile = SelectedProfile.Profile.Clone();
            SelectedProfile.Asset.AssetItem.IsDirty = true;
            editor.Session.NotifyAssetPropertiesChanged([SelectedProfile.Asset]);
        }

        SaveSceneSettings();
    }

    private async Task RenameSelectedProfile()
    {
        if (SelectedProfile == null)
            return;

        var previousSource = SelectedProfile.Source;
        var previousProfileId = SelectedProfile.ProfileId;
        var newName = await PromptForProfileName(SelectedProfile.DisplayName, "Rename Widget Profile");
        if (string.IsNullOrWhiteSpace(newName) || string.Equals(newName, SelectedProfile.DisplayName, StringComparison.Ordinal))
            return;

        if (previousSource == SceneWidgetProfileSource.Project && SelectedProfile.Asset != null)
        {
            SelectedProfile.Asset.Name = newName;
        }
        else
        {
            SelectedProfile.Profile.Name = newName;
        }

        RefreshProfiles();
        SetSelectedProfileSilently(availableProfiles.FirstOrDefault(x => x.Source == previousSource && string.Equals(x.ProfileId, previousProfileId, StringComparison.OrdinalIgnoreCase)));
        SaveSceneSettings();
        await Task.CompletedTask;
    }

    private void SetSelectedProfileAsDefault()
    {
        if (SelectedProfile == null)
            return;

        var userSettings = editor.Asset.Directory.Package.UserSettings;
        userSettings.SetValue(PackageSceneSettings.DefaultWidgetProfile, SelectedProfile.ToReference());
        userSettings.Save();
    }

    private async Task SelectProfileWithPrompt()
    {
        if (availableProfiles.Count == 0)
            return;

        if (widgetViewBridge == null)
            return;

        var selected = await widgetViewBridge.SelectProfileAsync(availableProfiles.Cast<ISceneWidgetProfileViewModel>().ToList(), SelectedProfile);
        if (selected is not SceneWidgetProfileViewModel profile)
            return;

        if (SelectedProfile != null &&
            profile.Source == SelectedProfile.Source &&
            string.Equals(profile.ProfileId, SelectedProfile.ProfileId, StringComparison.OrdinalIgnoreCase))
        {
            ApplyProfileSelection(profile);
            return;
        }

        SelectedProfile = profile;
    }

    private void ApplyProfileSelection(SceneWidgetProfileViewModel profile)
    {
        if (profile == null)
            return;

        ApplyProfile(profile.Profile);
        SaveSceneSettings();
    }

    private void ApplyProfile(SceneWidgetProfileData profile)
    {
        var service = widgetService;
        if (service == null)
            return;

        isApplyingProfile = true;
        try
        {
            ClearAll();
            var missingWidgets = new List<string>();
            foreach (var entry in profile.Widgets.OrderBy(x => x.Order))
            {
                if (!service.WidgetDescriptors.ContainsKey(entry.WidgetId))
                {
                    missingWidgets.Add(entry.WidgetId);
                    continue;
                }

                var item = CreateWidgetInstance(entry.WidgetId, entry.Placement);
                if (item != null)
                    GetCollection(entry.Placement).Add(item);
            }

            if (missingWidgets.Count > 0)
            {
                var profileName = string.IsNullOrWhiteSpace(profile.Name) ? "profile" : profile.Name;
                var message = $"Profile '{profileName}' was restored partially.\r\n\r\nMissing widgets:\r\n - {string.Join("\r\n - ", missingWidgets.Distinct(StringComparer.OrdinalIgnoreCase))}";
                ServiceProvider.Get<IDialogService>().MessageBoxAsync(message, MessageBoxButton.OK, MessageBoxImage.Warning).Forget();
            }
        }
        finally
        {
            isApplyingProfile = false;
        }
    }

    private SceneWidgetProfileData CaptureCurrentLayout()
    {
        return new SceneWidgetProfileData
        {
            Widgets = Top.CreateLayoutEntries()
                .Concat(Left.CreateLayoutEntries())
                .Concat(Right.CreateLayoutEntries())
                .Concat(Bottom.CreateLayoutEntries())
                .OrderBy(x => x.Placement)
                .ThenBy(x => x.Order)
                .ToList(),
        };
    }

    private SceneSettingsData LoadSceneSettings()
    {
        try
        {
            var userSettings = editor.Asset.Directory.Package.UserSettings;
            var sceneSettingsCollection = userSettings.GetValue(PackageSceneSettings.SceneSettings);
            if (!sceneSettingsCollection.TryGetValue(editor.Asset.Asset.Id, out var settings))
            {
                settings = SceneSettingsData.CreateDefault();
                sceneSettingsCollection.Add(editor.Asset.Asset.Id, settings);
            }

            return settings;
        }
        catch
        {
            return SceneSettingsData.CreateDefault();
        }
    }

    private void SaveSceneSettings()
    {
        try
        {
            var userSettings = editor.Asset.Directory.Package.UserSettings;
            var sceneSettingsCollection = userSettings.GetValue(PackageSceneSettings.SceneSettings);
            userSettings.SetValue(PackageSceneSettings.LocalWidgetProfiles, availableProfiles.Where(x => x.Source == SceneWidgetProfileSource.Local).Select(x => x.Profile.Clone()).ToList());
            if (!sceneSettingsCollection.TryGetValue(editor.Asset.Asset.Id, out var settings))
            {
                settings = SceneSettingsData.CreateDefault();
                sceneSettingsCollection.Add(editor.Asset.Asset.Id, settings);
            }

            SaveSettings(settings);
            userSettings.SetValue(PackageSceneSettings.SceneSettings, sceneSettingsCollection);
            userSettings.Save();
        }
        catch (Exception exception)
        {
            Logger.Warning("Unable to save scene widget settings.", exception);
        }
    }

    private void SetSelectedProfileSilently(SceneWidgetProfileViewModel? profile)
    {
        isApplyingProfile = true;
        try
        {
            SelectedProfile = profile;
        }
        finally
        {
            isApplyingProfile = false;
        }
    }

    private void ClearAll()
    {
        foreach (var item in GetAllItems().ToList())
        {
            item.PlacementCollection?.Remove(item);
            DestroyInstance(item);
        }
    }

    private SceneWidgetPlacementCollectionViewModel GetCollection(SceneWidgetPlacement placement)
    {
        return placement switch
        {
            SceneWidgetPlacement.Left => Left,
            SceneWidgetPlacement.Right => Right,
            SceneWidgetPlacement.Bottom => Bottom,
            _ => Top,
        };
    }

    private IEnumerable<SceneWidgetInstanceViewModel> GetAllItems()
    {
        return Top.Items.Concat(Left.Items).Concat(Right.Items).Concat(Bottom.Items);
    }

    private void NotifyCollectionStateChanged()
    {
        OnPropertyChanged(nameof(HasTopWidgets));
        OnPropertyChanged(nameof(HasLeftWidgets));
        OnPropertyChanged(nameof(HasRightWidgets));
        OnPropertyChanged(nameof(HasBottomWidgets));
        OnPropertyChanged(nameof(ShowTopWidgets));
        OnPropertyChanged(nameof(ShowLeftWidgets));
        OnPropertyChanged(nameof(ShowRightWidgets));
        OnPropertyChanged(nameof(ShowBottomWidgets));
        if (!isApplyingProfile)
            SaveSceneSettings();
    }

    private void OnWidgetDescriptorsChanged(object? sender, EventArgs e)
    {
        Dispatcher.Invoke(() =>
        {
            var currentLayout = CaptureCurrentLayout();
            RefreshAvailableWidgets();
            ApplyProfile(currentLayout);
        });
    }

    private static void DestroyInstance(SceneWidgetInstanceViewModel instance)
    {
        if (instance.ViewModel is IDestroyable destroyableViewModel)
            destroyableViewModel.Destroy();

        if (instance.View is IDestroyable destroyableView)
            destroyableView.Destroy();
    }

    private string ComputeLocalProfileName()
    {
        var index = 1;
        var existing = new HashSet<string>((editor.Asset.Directory.Package.UserSettings.GetValue(PackageSceneSettings.LocalWidgetProfiles) ?? []).Select(x => x.Name), StringComparer.OrdinalIgnoreCase);
        string candidate;
        do
        {
            candidate = $"Local Profile {index++}";
        }
        while (existing.Contains(candidate));

        return candidate;
    }

    private string ComputeProjectProfileName(PackageViewModel package)
    {
        var index = 1;
        var baseName = "Widget Profile";
        var existing = new HashSet<string>(package.Assets.Select(x => x.Name), StringComparer.OrdinalIgnoreCase);
        string candidate;
        do
        {
            candidate = $"{baseName} {index++}";
        }
        while (existing.Contains(candidate));

        return candidate;
    }

    private async Task<string?> PromptForProfileName(string initialValue, string title)
    {
        if (widgetViewBridge == null)
            return null;

        return await widgetViewBridge.PromptForProfileNameAsync(title, initialValue);
    }
}
