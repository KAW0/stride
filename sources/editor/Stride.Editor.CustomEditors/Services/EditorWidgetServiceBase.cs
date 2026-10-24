// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using Stride.Core.Presentation.ViewModels;
using Stride.Editor.CustomEditors.Infrastructure;
using Stride.Editor.CustomEditors.Models;

namespace Stride.Editor.CustomEditors.Services;

internal abstract class EditorWidgetServiceBase<TDescriptor, TViewModel> : ReloadableDescriptorServiceBase<TDescriptor>
    where TDescriptor : EditorWidgetDescriptor
    where TViewModel : class
{
    protected IEditorWidgetServiceViewBridge WidgetBridge { get; }

    protected EditorWidgetServiceBase(IEditorWidgetServiceViewBridge widgetBridge)
    {
        ArgumentNullException.ThrowIfNull(widgetBridge);
        WidgetBridge = widgetBridge;
    }

    protected TDescriptor GetWidgetDescriptor(string widgetId, string widgetTypeName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(widgetId);
        if (!Descriptors.TryGetValue(widgetId, out var descriptor))
            throw new InvalidOperationException($"{widgetTypeName} '{widgetId}' is not registered.");

        return descriptor;
    }

    protected TViewModel CreateWidgetViewModel(string widgetId, IViewModelServiceProvider serviceProvider, object context, string widgetTypeName)
    {
        var descriptor = GetWidgetDescriptor(widgetId, widgetTypeName);
        return EditorWidgetViewModelFactory.Create<TViewModel>(descriptor.ViewModelType, serviceProvider, context, descriptor.WidgetName);
    }

    protected object CreateWidgetView(string widgetId, TViewModel viewModel, string widgetTypeName)
    {
        ArgumentNullException.ThrowIfNull(viewModel);
        var descriptor = GetWidgetDescriptor(widgetId, widgetTypeName);
        return WidgetBridge.CreateView(descriptor.ViewType, viewModel);
    }
}
