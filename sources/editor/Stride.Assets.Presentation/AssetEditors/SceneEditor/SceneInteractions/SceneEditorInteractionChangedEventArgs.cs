// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;

namespace Stride.Assets.Presentation.AssetEditors.SceneEditor.SceneInteractions;

public sealed class SceneEditorInteractionChangedEventArgs : EventArgs
{
    public SceneEditorInteractionSession PreviousInteraction { get; }

    public SceneEditorInteractionSession CurrentInteraction { get; }
    
    public SceneEditorInteractionChangedEventArgs(SceneEditorInteractionSession previousInteraction, SceneEditorInteractionSession currentInteraction)
    {
        PreviousInteraction = previousInteraction;
        CurrentInteraction = currentInteraction;
    }
}
