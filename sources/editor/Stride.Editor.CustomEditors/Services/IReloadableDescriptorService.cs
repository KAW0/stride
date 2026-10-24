// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections.Generic;

namespace Stride.Editor.CustomEditors.Services;

public interface IReloadableDescriptorService<TDescriptor>
{
    event EventHandler? DescriptorsChanged;

    IReadOnlyDictionary<string, TDescriptor> Descriptors { get; }
}
