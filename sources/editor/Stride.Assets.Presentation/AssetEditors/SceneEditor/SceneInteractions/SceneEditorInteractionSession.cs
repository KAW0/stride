// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections.Generic;
using Stride.Core.Annotations;

namespace Stride.Assets.Presentation.AssetEditors.SceneEditor.SceneInteractions;

public sealed class SceneEditorInteractionSession
{
    public string Id { get; }

    public string OwnerId { get; }

    public int Priority { get; }

    public string DisplayName { get; init; }

    public object Context { get; }

    public SceneToolHierarchy Hierarchy { get; init; } = SceneToolHierarchy.Global;

    public Func<SceneEditorClickContext, bool> CanHandleMouseClick { get; init; }

    public Func<SceneEditorClickContext, bool> HandleMouseClick { get; init; }

    public Func<SceneEditorDefaultInput, bool> IsDefaultInputEnabled { get; init; }

    public Func<IReadOnlyList<SceneEditorContextMenuItem>> GetContextMenuItems { get; init; }

    public bool ExpandDefaultContextMenu { get; init; }

    public SceneEditorInteractionSession([NotNull] string ownerId, [NotNull] string id, object context = null, int priority = 0)
    {
        if (string.IsNullOrWhiteSpace(ownerId))
            throw new ArgumentException("The owner id cannot be null or whitespace.", nameof(ownerId));
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("The interaction id cannot be null or whitespace.", nameof(id));

        OwnerId = ownerId;
        Id = id;
        Context = context;
        Priority = priority;
    }
}
