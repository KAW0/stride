// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using Stride.Editor.CustomEditors.ViewModel;

namespace Stride.Editor.CustomEditors.Models;

public sealed class CustomEditorInstanceEventArgs : EventArgs
{
    public string EditorId { get; }

    public string InstanceId { get; }

    public CustomEditorViewModel ViewModel { get; }

    public CustomEditorInstanceEventArgs(string editorId, string instanceId, CustomEditorViewModel viewModel)
    {
        EditorId = editorId;
        InstanceId = instanceId;
        ViewModel = viewModel;
    }
}
