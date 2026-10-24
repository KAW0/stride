// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

namespace Stride.Editor.CustomEditors.Models;

public sealed record CustomEditorDescriptor(string AssemblyId, string EditorId, Type ViewType, Type ViewModelType, string EditorName);

