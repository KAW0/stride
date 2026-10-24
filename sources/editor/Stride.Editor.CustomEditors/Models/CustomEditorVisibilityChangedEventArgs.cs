// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;

namespace Stride.Editor.CustomEditors.Models;

public sealed class CustomEditorVisibilityChangedEventArgs : EventArgs
{
    public string EditorId { get; }

    public string InstanceId { get; }

    public bool IsVisible { get; }

    public CustomEditorVisibilityChangedEventArgs(string editorId, string instanceId, bool isVisible)
    {
        EditorId = editorId;
        InstanceId = instanceId;
        IsVisible = isVisible;
    }
}
