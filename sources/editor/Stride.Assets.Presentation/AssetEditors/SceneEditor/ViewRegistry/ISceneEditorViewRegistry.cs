// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections.Generic;

namespace Stride.Assets.Presentation.AssetEditors.SceneEditor.ViewRegistry;

public interface ISceneEditorViewRegistry
{
    event EventHandler<SceneEditorViewChangedEventArgs> ViewRegistered;

    event EventHandler<SceneEditorViewChangedEventArgs> ViewUnregistered;

    event EventHandler<SceneEditorViewChangedEventArgs> ActiveViewChanged;

    IReadOnlyList<SceneEditorViewRegistration> Views { get; }

    SceneEditorViewRegistration ActiveView { get; }

    void RegisterView(SceneEditorViewRegistration view);

    bool UnregisterView(string viewId);

    bool SetActiveView(string viewId);
}
