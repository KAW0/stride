// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Stride.Assets.Presentation.EditorWidgets;
using Stride.Assets.Presentation.SceneEditor;
using Stride.Assets.Presentation.SceneEditor.Widgets;
using Stride.Assets.Presentation.SceneEditor.Widgets.ViewModels;
using Stride.Core;
using Stride.Core.Assets;
using Stride.Core.Assets.Editor.ViewModel;
using Stride.Core.Presentation.Commands;
using Stride.Core.Presentation.Services;
using Stride.Core.Presentation.ViewModels;
using Stride.Editor.CustomEditors.Infrastructure;
using Stride.Editor.CustomEditors.SceneWidgets.ViewModels;
using Stride.Editor.CustomEditors.Services;

namespace Stride.Editor.CustomEditors.ViewModel;

public sealed class MainToolbarWidgetWorkspaceViewModel : DispatcherViewModel
{
    private readonly MainToolbarWidgetService widgetService;
    private readonly ISceneWidgetServiceViewBridge viewBridge;
    private readonly ObservableCollection<MainToolbarProfileViewModel> availableProfiles = [];
    private readonly MainToolbarWidgetCollectionViewModel collection;
    private bool applyingProfile;
    private MainToolbarProfileViewModel? selectedProfile;

    public IAddChildViewModel Collection => collection;

    public ObservableCollection<MainToolbarWidgetInstanceViewModel> Items => collection.Items;

    public ReadOnlyObservableCollection<MainToolbarProfileViewModel> AvailableProfiles { get; }

    public MainToolbarProfileViewModel? SelectedProfile
    {
        get => selectedProfile;
        set
        {
            if (!SetValue(ref selectedProfile, value))
                return;

            SaveSelectedProfileCommand.IsEnabled = value != null;
            SetSelectedProfileAsDefaultCommand.IsEnabled = value != null;
            RenameSelectedProfileCommand.IsEnabled = value != null;
            if (!applyingProfile)
                ApplyProfile(value?.Profile);
        }
    }

    public ICommandBase SaveSelectedProfileCommand { get; }
    public ICommandBase SetSelectedProfileAsDefaultCommand { get; }
    public ICommandBase RenameSelectedProfileCommand { get; }
    public ICommandBase SelectProfileWithPromptCommand { get; }
    public ICommandBase CreateLocalProfileCommand { get; }
    public ICommandBase CreateProjectProfileCommand { get; }

    internal MainToolbarWidgetWorkspaceViewModel(
        IViewModelServiceProvider serviceProvider,
        MainToolbarWidgetService widgetService,
        ISceneWidgetServiceViewBridge viewBridge)
        : base(serviceProvider)
    {
        this.widgetService = widgetService;
        this.viewBridge = viewBridge;
        collection = new MainToolbarWidgetCollectionViewModel(widgetService, SaveCurrentLayout);
        widgetService.WidgetOrderChanged += SaveCurrentLayout;
        widgetService.WidgetDescriptorsChanged += OnWidgetDescriptorsChanged;
        AvailableProfiles = new ReadOnlyObservableCollection<MainToolbarProfileViewModel>(availableProfiles);
        SaveSelectedProfileCommand = new AnonymousCommand(serviceProvider, SaveSelectedProfile, () => SelectedProfile != null);
        SetSelectedProfileAsDefaultCommand = new AnonymousCommand(serviceProvider, SetSelectedProfileAsDefault, () => SelectedProfile != null);
        RenameSelectedProfileCommand = new AnonymousTaskCommand(serviceProvider, RenameSelectedProfile, () => SelectedProfile != null);
        SelectProfileWithPromptCommand = new AnonymousTaskCommand(serviceProvider, SelectProfileWithPrompt, () => AvailableProfiles.Count > 0);
        CreateLocalProfileCommand = new AnonymousTaskCommand(serviceProvider, CreateLocalProfile);
        CreateProjectProfileCommand = new AnonymousTaskCommand(serviceProvider, CreateProjectProfile);
        RefreshProfiles();
    }

    public void LoadSettings()
    {
        var package = GetCurrentPackage();
        if (package == null)
            return;

        RefreshProfiles();
        var selectedReference = package.UserSettings.GetValue(PackageMainToolbarSettings.DefaultProfile) ?? MainToolbarProfileReference.None();
        var selected = availableProfiles.FirstOrDefault(x => x.Source == selectedReference.Source && string.Equals(x.ProfileId, selectedReference.ProfileId, StringComparison.OrdinalIgnoreCase));
        if (selected != null)
        {
            SetSelectedProfileSilently(selected);
            ApplyProfile(selected.Profile);
        }
        else
        {
            SetSelectedProfileSilently(null);
            ApplyProfile(new MainToolbarProfileData
            {
                Widgets = package.UserSettings.GetValue(PackageMainToolbarSettings.CurrentLayout)?.ToList() ?? [],
            });
        }
    }

    public override void Destroy()
    {
        widgetService.WidgetOrderChanged -= SaveCurrentLayout;
        widgetService.WidgetDescriptorsChanged -= OnWidgetDescriptorsChanged;
        base.Destroy();
    }

    private void RefreshProfiles()
    {
        availableProfiles.Clear();
        var package = GetCurrentPackage();
        if (package == null)
        {
            SelectProfileWithPromptCommand.IsEnabled = false;
            return;
        }

        foreach (var profile in package.UserSettings.GetValue(PackageMainToolbarSettings.LocalProfiles) ?? [])
        {
            availableProfiles.Add(new MainToolbarProfileViewModel(EditorWidgetProfileSource.Local, profile.Id, profile.Name, profile.Clone()));
        }

        foreach (var asset in package.AllAssets.Where(x => x.Asset is MainToolbarProfileAsset))
        {
            if (asset.Asset is not MainToolbarProfileAsset profileAsset)
                continue;

            var profile = profileAsset.Profile?.Clone() ?? new MainToolbarProfileData();
            if (string.IsNullOrWhiteSpace(profile.Id))
                profile.Id = asset.Id.ToString();
            availableProfiles.Add(new MainToolbarProfileViewModel(EditorWidgetProfileSource.Project, profile.Id, asset.Name, profile, asset));
        }

        SelectProfileWithPromptCommand.IsEnabled = availableProfiles.Count > 0;
    }

    private void ApplyProfile(MainToolbarProfileData? profile)
    {
        applyingProfile = true;
        try
        {
            var order = profile?.Widgets.OrderBy(x => x.Order).Select(x => x.WidgetId) ?? Enumerable.Empty<string>();
            widgetService.ApplyOrder(order);
            collection.RefreshOrder();
        }
        finally
        {
            applyingProfile = false;
        }
    }

    private void SaveCurrentLayout()
    {
        if (!applyingProfile)
            SaveSettings();
    }

    private void SaveSettings()
    {
        var package = GetCurrentPackage();
        if (package == null)
            return;

        package.UserSettings.SetValue(PackageMainToolbarSettings.CurrentLayout, CaptureLayout());
        package.UserSettings.Save();
    }

    private List<MainToolbarWidgetLayoutEntry> CaptureLayout()
    {
        return Items.Select((item, index) => new MainToolbarWidgetLayoutEntry
        {
            WidgetId = item.WidgetId,
            Order = index,
            Title = item.DisplayName,
        }).ToList();
    }

    private async Task CreateLocalProfile()
    {
        var package = GetCurrentPackage();
        if (package == null)
            return;

        var profile = new MainToolbarProfileData
        {
            Id = Guid.NewGuid().ToString("N"),
            Name = $"Main Toolbar Profile {availableProfiles.Count + 1}",
            Widgets = CaptureLayout(),
        };
        profile.Name = await PromptForProfileName(profile.Name, "Create Main Toolbar Profile");
        if (string.IsNullOrWhiteSpace(profile.Name))
            return;

        package.UserSettings.GetValue(PackageMainToolbarSettings.LocalProfiles).Add(profile);
        package.UserSettings.Save();
        RefreshProfiles();
        SetSelectedProfileSilently(availableProfiles.LastOrDefault(x => x.ProfileId == profile.Id));
    }

    private async Task CreateProjectProfile()
    {
        var package = GetCurrentPackage();
        var directory = package?.AssetMountPoint;
        if (package == null || directory == null)
            return;

        var profile = new MainToolbarProfileData
        {
            Id = Guid.NewGuid().ToString("N"),
            Name = $"Main Toolbar Profile {availableProfiles.Count + 1}",
            Widgets = CaptureLayout(),
        };
        profile.Name = await PromptForProfileName(profile.Name, "Create Project Main Toolbar Profile");
        if (string.IsNullOrWhiteSpace(profile.Name))
            return;

        var asset = new MainToolbarProfileAsset { Profile = profile.Clone() };
        var assetItem = new AssetItem(profile.Name, asset) { IsDirty = true };
        var assetViewModel = package.CreateAsset(directory, assetItem, true, null);
        SessionViewModel.Instance.NotifyAssetPropertiesChanged([assetViewModel]);
        RefreshProfiles();
        SetSelectedProfileSilently(availableProfiles.FirstOrDefault(x => x.Asset?.Id == assetViewModel.Id));
    }

    private void SaveSelectedProfile()
    {
        if (SelectedProfile == null)
            return;

        SelectedProfile.Profile.Widgets = CaptureLayout();
        if (SelectedProfile.Source == EditorWidgetProfileSource.Project && SelectedProfile.Asset?.Asset is MainToolbarProfileAsset asset)
        {
            asset.Profile = SelectedProfile.Profile.Clone();
            SelectedProfile.Asset.AssetItem.IsDirty = true;
            SessionViewModel.Instance.NotifyAssetPropertiesChanged([SelectedProfile.Asset]);
        }
        else if (SelectedProfile.Source == EditorWidgetProfileSource.Local)
        {
            var package = GetCurrentPackage();
            var profiles = package?.UserSettings.GetValue(PackageMainToolbarSettings.LocalProfiles);
            var stored = profiles?.FirstOrDefault(x => x.Id == SelectedProfile.ProfileId);
            if (stored != null)
                stored.Widgets = SelectedProfile.Profile.Widgets;
        }
        SaveSettings();
    }

    private async Task RenameSelectedProfile()
    {
        if (SelectedProfile == null)
            return;

        var name = await PromptForProfileName(SelectedProfile.DisplayName, "Rename Main Toolbar Profile");
        if (string.IsNullOrWhiteSpace(name) || name == SelectedProfile.DisplayName)
            return;

        if (SelectedProfile.Source == EditorWidgetProfileSource.Project && SelectedProfile.Asset != null)
            SelectedProfile.Asset.Name = name;
        else
        {
            SelectedProfile.Profile.Name = name;
            var package = GetCurrentPackage();
            var stored = package?.UserSettings.GetValue(PackageMainToolbarSettings.LocalProfiles)?.FirstOrDefault(x => x.Id == SelectedProfile.ProfileId);
            if (stored != null)
                stored.Name = name;
        }

        RefreshProfiles();
    }

    private void SetSelectedProfileAsDefault()
    {
        var package = GetCurrentPackage();
        if (package != null && SelectedProfile != null)
        {
            package.UserSettings.SetValue(PackageMainToolbarSettings.DefaultProfile, SelectedProfile.ToReference());
            package.UserSettings.Save();
        }
    }

    private async Task SelectProfileWithPrompt()
    {
        if (availableProfiles.Count == 0)
            return;

        var selected = await viewBridge.SelectProfileAsync(availableProfiles.Cast<ISceneWidgetProfileViewModel>().ToList(), SelectedProfile);
        if (selected is MainToolbarProfileViewModel profile)
            SelectedProfile = profile;
    }

    private async Task<string?> PromptForProfileName(string initialName, string title)
    {
        return await viewBridge.PromptForProfileNameAsync(title, initialName);
    }

    private void SetSelectedProfileSilently(MainToolbarProfileViewModel? profile)
    {
        applyingProfile = true;
        try
        {
            SelectedProfile = profile;
        }
        finally
        {
            applyingProfile = false;
        }
    }

    private PackageViewModel? GetCurrentPackage() => SessionViewModel.Instance?.CurrentProject;

    private void OnWidgetDescriptorsChanged(object? sender, EventArgs e)
    {
        LoadSettings();
    }
}
