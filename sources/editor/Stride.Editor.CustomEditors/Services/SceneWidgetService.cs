// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Stride.Core.Diagnostics;
using Stride.Core.Presentation.ViewModels;
using Stride.Editor.CustomEditors.Annotations;
using Stride.Editor.CustomEditors.Infrastructure;
using Stride.Editor.CustomEditors.Services;
using Stride.Editor.CustomEditors.ViewModel;

namespace Stride.Editor.CustomEditors.SceneWidgets;

internal sealed class SceneWidgetService : EditorWidgetServiceBase<SceneWidgetDescriptor, SceneWidgetViewModel>, ISceneWidgetService
{
    private static readonly Logger Logger = GlobalLogger.GetLogger(nameof(SceneWidgetService));
    private readonly ISceneWidgetServiceViewBridge bridge;

    public event EventHandler? WidgetDescriptorsChanged;

    public IReadOnlyDictionary<string, SceneWidgetDescriptor> WidgetDescriptors => Descriptors;

    public SceneWidgetService(ISceneWidgetServiceViewBridge bridge)
        : base(bridge)
    {
        ArgumentNullException.ThrowIfNull(bridge);
        this.bridge = bridge;
        InitializeDescriptorRegistry();
    }

    public object CreateView(string widgetId, SceneWidgetViewModel viewModel)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(widgetId);
        ArgumentNullException.ThrowIfNull(viewModel);

        return CreateWidgetView(widgetId, viewModel, "Scene widget");
    }

    public SceneWidgetViewModel CreateViewModel(string widgetId, IViewModelServiceProvider serviceProvider, SceneWidgetViewModelContext context)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(widgetId);
        ArgumentNullException.ThrowIfNull(serviceProvider);
        ArgumentNullException.ThrowIfNull(context);

        var viewModel = CreateWidgetViewModel(widgetId, serviceProvider, context, "Scene widget");

        viewModel.Initialize(context);
        return viewModel;
    }

    protected override Assembly DescriptorAssembly => typeof(SceneWidgetService).Assembly;

    protected override Type ViewType => bridge.ViewType;

    protected override string GetAssemblyId(SceneWidgetDescriptor descriptor) => descriptor.AssemblyId;

    protected override bool TryCreateDescriptor(Type type, string assemblyKey, [NotNullWhen(true)] out string? descriptorId, [NotNullWhen(true)] out SceneWidgetDescriptor? descriptor)
    {
        descriptorId = null;
        descriptor = null;

        var attribute = type.GetCustomAttribute<SceneWidgetAttribute>();
        if (attribute?.ViewModelType == null)
            return false;

        if (!typeof(SceneWidgetViewModel).IsAssignableFrom(attribute.ViewModelType) || attribute.ViewModelType.IsAbstract)
            return false;

        descriptorId = attribute.ViewModelType.FullName ?? type.FullName;
        if (string.IsNullOrWhiteSpace(descriptorId))
            return false;

        descriptor = new SceneWidgetDescriptor(
            assemblyKey,
            type,
            attribute.ViewModelType,
            attribute.WidgetName,
            attribute.DefaultPlacement,
            attribute.AllowMultiple);
        return true;
    }

    protected override void OnDescriptorsReloaded(bool descriptorsChanged)
    {
        if (descriptorsChanged)
            WidgetDescriptorsChanged?.Invoke(this, EventArgs.Empty);
    }

    protected override void OnTypeLoadException(Assembly assembly, ReflectionTypeLoadException exception)
    {
        Logger.Warning($"Unable to load all types from assembly '{assembly.FullName}'.");
    }

    public void Dispose()
    {
        DisposeDescriptorRegistry();
    }
}
