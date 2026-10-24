// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections.Generic;
using Stride.Assets.Presentation.AssetEditors.SceneEditor.SceneInteractions;

namespace Stride.Editor.CustomEditors.CustomTools;

public sealed class SceneToolDefinition
{
    public string Id { get; set; }
    public string DisplayName { get; set; }
    public SceneToolScope Scope { get; set; } = SceneToolScope.ActiveView;
    public SceneToolHierarchy Hierarchy { get; set; } = SceneToolHierarchy.Global;
    public int Priority { get; set; } = 100;
    public bool ExpandDefaultContextMenu { get; set; }
    public Func<SceneToolContext, bool>? IsEnabledForView { get; set; }
    public Func<SceneToolContext, IEnumerable<SceneEditorContextMenuItem>>? GetContextMenuItems { get; set; }
    public Func<SceneEditorClickContext, bool>? CanHandleClick { get; set; }
    public Func<SceneEditorClickContext, bool>? HandleClick { get; set; }
    public Func<SceneToolContext, SceneEditorDefaultInput, bool>? IsDefaultInputEnabled { get; set; }

    public SceneToolDefinition(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("The tool id cannot be null or whitespace.", nameof(id));

        Id = id;
        DisplayName = id;
    }
}
