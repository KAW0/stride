// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using Stride.Assets.Presentation.AssetEditors.EntityHierarchyEditor.ViewModels;

namespace Stride.Assets.Presentation.SceneEditor.Widgets.ViewModels;

public interface ISceneWidgetWorkspaceViewModelFactory
{
    ISceneWidgetWorkspaceViewModel Create(EntityHierarchyEditorViewModel editor);
}
