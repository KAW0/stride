// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections.Generic;
using Stride.Core.Assets;

namespace Stride.Editor.CustomEditors.Events;

public sealed class AssembliesReloadedEventArgs : EventArgs
{
    public IReadOnlyCollection<PackageLoadedAssembly> Assemblies { get; }

    public AssembliesReloadedEventArgs(IReadOnlyCollection<PackageLoadedAssembly> assemblies)
    {
        ArgumentNullException.ThrowIfNull(assemblies);
        Assemblies = assemblies;
    }
}
