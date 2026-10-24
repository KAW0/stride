// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using Stride.Core.Annotations;

namespace Stride.Assets.Presentation.AssetEditors.SceneEditor.ViewRegistry;

public sealed class SceneEditorViewChangedEventArgs : EventArgs
{
    public SceneEditorViewRegistration View { get; }

    public SceneEditorViewChangedEventArgs([NotNull] SceneEditorViewRegistration view)
    {
        View = view ?? throw new ArgumentNullException(nameof(view));
    }
}
