// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections.Generic;
using Stride.Core.Assets.Editor.ViewModel;
using Stride.Core.Presentation.ViewModels;
using Stride.Editor.CustomEditors.SceneWidgets;
using Stride.Editor.CustomEditors.ViewModel;

namespace Stride.Editor.CustomEditors.Services;

public interface ISceneWidgetService:IDisposable
{
    event EventHandler? WidgetDescriptorsChanged;

    IReadOnlyDictionary<string, SceneWidgetDescriptor> WidgetDescriptors { get; }

    object CreateView(string widgetId, SceneWidgetViewModel viewModel);

    SceneWidgetViewModel CreateViewModel(string widgetId, IViewModelServiceProvider serviceProvider, SceneWidgetViewModelContext context);
}
