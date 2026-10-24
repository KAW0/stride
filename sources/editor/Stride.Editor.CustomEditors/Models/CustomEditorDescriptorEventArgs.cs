// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;

namespace Stride.Editor.CustomEditors.Models;

public sealed class CustomEditorDescriptorEventArgs : EventArgs
{
    public string EditorId { get; }

    public string EditorName { get; }

    public CustomEditorDescriptorEventArgs(string editorId, string editorName)
    {
        EditorId = editorId;
        EditorName = editorName;
    }
}
