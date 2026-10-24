// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Diagnostics.CodeAnalysis;
using Stride.Assets.Presentation.AssetEditors.EntityHierarchyEditor.ViewModels;
using Stride.Assets.Presentation.AssetEditors.SceneEditor.ViewModels;
using Stride.Assets.Presentation.AssetEditors.SceneEditor.ViewRegistry;
using Stride.Assets.Presentation.ViewModel;

namespace Stride.Editor.CustomEditors.CustomTools;

public sealed class SceneToolContext
{
    public string OwnerId { get; }
    public SceneEditorViewRegistration View { get; }
    public string ViewId => View.ViewId;
    public SceneViewModel SceneAsset => View.SceneAsset;
    public SceneEditorViewModel SceneEditor => View.SceneEditor;
    public EntityHierarchyEditorViewModel EntityHierarchyEditor => View.EntityHierarchyEditor;

    internal SceneToolContext(string ownerId, SceneEditorViewRegistration view)
    {
        OwnerId = ownerId ?? string.Empty;
        ArgumentNullException.ThrowIfNull(view);
        View = view;
    }
}
