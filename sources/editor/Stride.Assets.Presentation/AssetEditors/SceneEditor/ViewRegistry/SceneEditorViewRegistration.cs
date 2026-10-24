// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using Stride.Assets.Presentation.AssetEditors.EntityHierarchyEditor.ViewModels;
using Stride.Assets.Presentation.AssetEditors.SceneEditor.SceneInteractions;
using Stride.Assets.Presentation.AssetEditors.SceneEditor.ViewModels;
using Stride.Assets.Presentation.ViewModel;
using Stride.Core.Annotations;

namespace Stride.Assets.Presentation.AssetEditors.SceneEditor.ViewRegistry;

public sealed class SceneEditorViewRegistration
{
    public string ViewId { get; }

    public ISceneEditorInteractionService InteractionService { get; }

    public bool IsActive { get; internal set; }

    public SceneViewModel SceneAsset { get; }

    public SceneEditorViewModel SceneEditor { get; }

    public EntityHierarchyEditorViewModel EntityHierarchyEditor { get; }

    public SceneEditorViewRegistration([NotNull] string viewId, [NotNull] ISceneEditorInteractionService interactionService, SceneViewModel sceneAsset = null, SceneEditorViewModel sceneEditor = null, EntityHierarchyEditorViewModel entityHierarchyEditor = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(viewId);
        ViewId = viewId;
        InteractionService = interactionService ?? throw new ArgumentNullException(nameof(interactionService));
        SceneAsset = sceneAsset;
        SceneEditor = sceneEditor;
        EntityHierarchyEditor = entityHierarchyEditor ?? sceneEditor;
    }
}
