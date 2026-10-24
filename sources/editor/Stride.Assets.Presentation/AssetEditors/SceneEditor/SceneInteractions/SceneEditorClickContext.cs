// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using Stride.Assets.Presentation.AssetEditors.EntityHierarchyEditor.ViewModels;
using Stride.Assets.Presentation.AssetEditors.SceneEditor.ViewModels;
using Stride.Assets.Presentation.AssetEditors.SceneEditor.ViewRegistry;
using Stride.Assets.Presentation.ViewModel;
using Stride.Core.Annotations;

namespace Stride.Assets.Presentation.AssetEditors.SceneEditor.SceneInteractions;

public sealed class SceneEditorClickContext
{
    public string ViewId { get; }

    public SceneEditorMouseButton Button { get; }

    public SceneViewModel SceneAsset { get; }

    public SceneEditorViewModel SceneEditor { get; }

    public EntityHierarchyEditorViewModel EntityHierarchyEditor { get; }

    public SceneEditorViewRegistration SceneView { get; }

    public SceneEditorClickContext(SceneEditorMouseButton button, [NotNull] SceneEditorViewRegistration sceneView)
    {
        ArgumentNullException.ThrowIfNull(sceneView);

        ViewId = sceneView.ViewId;
        Button = button;
        SceneView = sceneView;
        SceneAsset = sceneView.SceneAsset;
        SceneEditor = sceneView.SceneEditor;
        EntityHierarchyEditor = sceneView.EntityHierarchyEditor;
    }
}
