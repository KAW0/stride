// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using Stride.Assets.Presentation.AssetEditors.EntityHierarchyEditor.ViewModels;
using Stride.Assets.Presentation.SceneEditor.Widgets.ViewModels;
using Stride.Core.Assets.Editor.ViewModel;
using Stride.Core.Presentation.ViewModels;

namespace Stride.Editor.CustomEditors.SceneWidgets.ViewModels;

internal sealed class SceneWidgetWorkspaceViewModelFactory : ISceneWidgetWorkspaceViewModelFactory
{
    private readonly IViewModelServiceProvider serviceProvider;

    public SceneWidgetWorkspaceViewModelFactory(IViewModelServiceProvider serviceProvider)
    {
        this.serviceProvider = serviceProvider;
    }

    public ISceneWidgetWorkspaceViewModel Create(EntityHierarchyEditorViewModel editor)
    {
        return new SceneWidgetWorkspaceViewModel(serviceProvider, editor);
    }
}
