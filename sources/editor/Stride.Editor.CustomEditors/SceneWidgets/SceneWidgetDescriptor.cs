// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using Stride.Assets.Presentation.SceneEditor.Widgets;
using Stride.Editor.CustomEditors.Models;

namespace Stride.Editor.CustomEditors.SceneWidgets;

public sealed record SceneWidgetDescriptor(
    string AssemblyId,
    Type ViewType,
    Type ViewModelType,
    string WidgetName,
    SceneWidgetPlacement DefaultPlacement,
    bool AllowMultiple)
    : EditorWidgetDescriptor(AssemblyId, ViewType, ViewModelType, WidgetName, AllowMultiple);
