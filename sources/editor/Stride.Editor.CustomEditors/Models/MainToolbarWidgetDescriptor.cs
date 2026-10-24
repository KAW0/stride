// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;

namespace Stride.Editor.CustomEditors.Models;

public sealed record MainToolbarWidgetDescriptor(
    string AssemblyId,
    Type ViewType,
    Type ViewModelType,
    string WidgetId,
    string WidgetName,
    bool AllowMultiple)
    : EditorWidgetDescriptor(AssemblyId, ViewType, ViewModelType, WidgetName, AllowMultiple);
