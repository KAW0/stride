// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using Stride.Assets.Presentation.SceneEditor.Widgets;
using Stride.Assets.Presentation.SceneEditor.Widgets.ViewModels;

namespace Stride.Editor.CustomEditors.SceneWidgets.ViewModels;

internal sealed class SceneWidgetTypeViewModel : ISceneWidgetTypeViewModel
{
    public SceneWidgetDescriptor Descriptor { get; }

    public string WidgetId => Descriptor.ViewModelType.FullName ?? Descriptor.ViewType.FullName ?? Descriptor.WidgetName;

    public string DisplayName => Descriptor.WidgetName;

    public SceneWidgetPlacement DefaultPlacement => Descriptor.DefaultPlacement;

    public bool AllowMultiple => Descriptor.AllowMultiple;

    public SceneWidgetTypeViewModel(SceneWidgetDescriptor descriptor)
    {
        Descriptor = descriptor;
    }
}
