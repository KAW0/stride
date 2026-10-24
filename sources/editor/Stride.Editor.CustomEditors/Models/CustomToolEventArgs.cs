// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using Stride.Editor.CustomEditors.CustomTools;

namespace Stride.Editor.CustomEditors.Models;

public sealed class CustomToolChangedEventArgs : EventArgs
{
    public SceneToolHandle? PreviousTool { get; }
    public SceneToolHandle? CurrentTool { get; }

    internal CustomToolChangedEventArgs(SceneToolHandle? previousTool, SceneToolHandle? currentTool)
    {
        PreviousTool = previousTool;
        CurrentTool = currentTool;
    }
}
