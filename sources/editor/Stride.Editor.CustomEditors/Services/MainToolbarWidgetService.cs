// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using Stride.Core.Diagnostics;
using Stride.Core.Presentation.ViewModels;
using Stride.Editor.CustomEditors.Annotations;
using Stride.Editor.CustomEditors.Infrastructure;
using Stride.Editor.CustomEditors.Models;
using Stride.Editor.CustomEditors.SceneWidgets.ViewModels;
using Stride.Editor.CustomEditors.ViewModel;

namespace Stride.Editor.CustomEditors.Services;

internal sealed class MainToolbarWidgetService : EditorWidgetServiceBase<MainToolbarWidgetDescriptor, MainToolbarWidgetViewModel>, IMainToolbarService
{
    private static readonly Logger Logger = GlobalLogger.GetLogger(nameof(MainToolbarWidgetService));
    private readonly IEditorWidgetServiceViewBridge bridge;
    private readonly IViewModelServiceProvider serviceProvider;
    private readonly ObservableCollection<MainToolbarWidgetInstanceViewModel> widgets = [];
    private readonly ReadOnlyObservableCollection<MainToolbarWidgetInstanceViewModel> readOnlyWidgets;

    internal ObservableCollection<MainToolbarWidgetInstanceViewModel> MutableWidgets => widgets;

    internal event Action? WidgetOrderChanged;

    public MainToolbarWidgetWorkspaceViewModel Workspace { get; }

    public event EventHandler? WidgetDescriptorsChanged;

    public IReadOnlyDictionary<string, MainToolbarWidgetDescriptor> WidgetDescriptors => Descriptors;

    public ReadOnlyObservableCollection<MainToolbarWidgetInstanceViewModel> Widgets => readOnlyWidgets;

    public MainToolbarWidgetService(ISceneWidgetServiceViewBridge bridge, IViewModelServiceProvider serviceProvider)
        : base(bridge)
    {
        ArgumentNullException.ThrowIfNull(bridge);
        ArgumentNullException.ThrowIfNull(serviceProvider);
        this.bridge = bridge;
        this.serviceProvider = serviceProvider;
        readOnlyWidgets = new ReadOnlyObservableCollection<MainToolbarWidgetInstanceViewModel>(widgets);
        InitializeDescriptorRegistry();
        RebuildWidgets();
        Workspace = new MainToolbarWidgetWorkspaceViewModel(serviceProvider, this, bridge);
    }

    protected override Assembly DescriptorAssembly => typeof(MainToolbarWidgetService).Assembly;

    protected override Type ViewType => bridge.ViewType;

    protected override string GetAssemblyId(MainToolbarWidgetDescriptor descriptor) => descriptor.AssemblyId;

    protected override bool TryCreateDescriptor(Type type, string assemblyKey, [NotNullWhen(true)] out string? descriptorId, [NotNullWhen(true)] out MainToolbarWidgetDescriptor? descriptor)
    {
        descriptorId = null;
        descriptor = null;

        var attribute = type.GetCustomAttributes().OfType<IMainToolbarWidgetRegistration>().FirstOrDefault();
        if (attribute == null || string.IsNullOrWhiteSpace(attribute.WidgetId) || string.IsNullOrWhiteSpace(attribute.WidgetName))
            return false;

        if (!typeof(MainToolbarWidgetViewModel).IsAssignableFrom(attribute.ViewModelType) || attribute.ViewModelType.IsAbstract)
            return false;

        descriptorId = attribute.WidgetId;
        descriptor = new MainToolbarWidgetDescriptor(
            assemblyKey,
            type,
            attribute.ViewModelType,
            attribute.WidgetId,
            attribute.WidgetName,
            attribute.AllowMultiple);
        return true;
    }

    protected override void OnDescriptorsReloaded(bool descriptorsChanged)
    {
        if (!descriptorsChanged)
            return;

        RebuildWidgets();
        WidgetDescriptorsChanged?.Invoke(this, EventArgs.Empty);
    }

    public void Dispose()
    {
        ClearWidgets();
        DisposeDescriptorRegistry();
    }

    private void RebuildWidgets()
    {
        ClearWidgets();
        foreach (var descriptor in Descriptors.Values.OrderBy(x => x.WidgetId, StringComparer.OrdinalIgnoreCase))
            CreateWidget(descriptor);
    }

    private void CreateWidget(MainToolbarWidgetDescriptor descriptor)
    {
        try
        {
            var context = new MainToolbarWidgetContext(descriptor.WidgetId, descriptor.WidgetName);
            var viewModel = CreateWidgetViewModel(descriptor.WidgetId, serviceProvider, context, "Main toolbar widget");
            var view = CreateWidgetView(descriptor.WidgetId, viewModel, "Main toolbar widget");
            widgets.Add(new MainToolbarWidgetInstanceViewModel(serviceProvider, descriptor, viewModel, view, MoveWidget));
        }
        catch (Exception exception)
        {
            Logger.Error($"Unable to create main toolbar widget '{descriptor.WidgetName}'.", exception);
        }
    }

    private void ClearWidgets()
    {
        foreach (var widget in widgets.ToList())
        {
            widgets.Remove(widget);
            widget.Destroy();
        }
    }

    internal void MoveWidget(MainToolbarWidgetInstanceViewModel widget, int index)
    {
        var oldIndex = widgets.IndexOf(widget);
        if (oldIndex < 0)
            return;

        widgets.RemoveAt(oldIndex);
        index = Math.Max(0, Math.Min(index, widgets.Count));
        widgets.Insert(index, widget);
        for (var i = 0; i < widgets.Count; i++)
            widgets[i].Order = i;
        WidgetOrderChanged?.Invoke();
    }

    internal void ApplyOrder(IEnumerable<string> widgetIds)
    {
        var ordered = widgetIds
            .Select(id => widgets.FirstOrDefault(x => string.Equals(x.WidgetId, id, StringComparison.OrdinalIgnoreCase)))
            .OfType<MainToolbarWidgetInstanceViewModel>()
            .Distinct()
            .ToList();
        ordered.AddRange(widgets.Where(x => !ordered.Contains(x)));
        for (var i = 0; i < ordered.Count; i++)
        {
            var currentIndex = widgets.IndexOf(ordered[i]);
            if (currentIndex != i)
                widgets.Move(currentIndex, i);
            ordered[i].Order = i;
        }
    }
}
