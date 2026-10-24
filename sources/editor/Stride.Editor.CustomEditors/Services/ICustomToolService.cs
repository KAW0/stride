// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using Stride.Editor.CustomEditors.CustomTools;
using Stride.Editor.CustomEditors.Models;

namespace Stride.Editor.CustomEditors.Services;

public interface ICustomToolService
{
    event EventHandler<CustomToolChangedEventArgs>? ActiveToolChanged;

    event EventHandler<CustomToolChangedEventArgs>? RequestedToolChanged;

    SceneToolHandle? RequestedTool { get; }

    SceneToolHandle? EffectiveTool { get; }

    SceneToolHandle? ActiveTool { get; }

    SceneToolHandle? GetRequestedTool(string viewId);

    SceneToolHandle? GetEffectiveTool(string viewId);

    SceneToolHandle? GetActiveTool(string viewId);

    SceneToolContext? GetActiveSceneContext();

    void ActivateTool(string editorId, SceneToolDefinition tool);

    bool IsToolActive(string editorId, string toolId);

    bool IsToolActive(string editorId, string toolId, string viewId);

    bool IsToolRequested(string editorId, string toolId);

    bool IsToolRequested(string editorId, string toolId, string viewId);

    bool DeactivateTool(string editorId, string toolId);

    bool DeactivateActiveTool();

    bool DeactivateEditorTools(string editorId);

    bool CloseEditorTools(string editorId);
}
