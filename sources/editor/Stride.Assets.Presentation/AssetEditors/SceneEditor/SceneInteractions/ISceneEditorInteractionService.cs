// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections.Generic;

namespace Stride.Assets.Presentation.AssetEditors.SceneEditor.SceneInteractions;

public interface ISceneEditorInteractionService
{
    event EventHandler<SceneEditorInteractionChangedEventArgs> ActiveInteractionChanged;

    SceneEditorInteractionSession ActiveInteraction { get; }

    SceneEditorInteractionSession LastInteraction { get; }

    void ActivateInteraction(SceneEditorInteractionSession interaction);

    bool IsInteractionActive(string ownerId, string interactionId);

    bool DeactivateInteraction(string ownerId, string interactionId, SceneToolHierarchy hierarchy);

    bool DeactivateInteraction(string ownerId, string interactionId);

    bool DeactivateActiveInteraction();

    bool DeactivateOwnerInteractions(string ownerId);

    bool RestoreLastInteraction();

    bool ForgetLastInteraction(string ownerId);

    bool IsDefaultInputEnabled(SceneEditorDefaultInput input);

    bool CanHandleMouseClick(SceneEditorClickContext clickContext);

    bool HandleMouseClick(SceneEditorClickContext clickContext);

    IReadOnlyList<SceneEditorContextMenuItem> GetContextMenuItems();

    bool ShouldExpandDefaultContextMenu();
}
