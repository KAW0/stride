// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections.Generic;
using Stride.Core.Assets;

namespace Stride.Editor.CustomEditors.Events;

public static class CustomEditorEvents
{
    public static event EventHandler<AssembliesReloadedEventArgs>? AssembliesReloaded;

    internal static void RaiseAssembliesReloaded(IReadOnlyCollection<PackageLoadedAssembly> assemblies)
    {
        ArgumentNullException.ThrowIfNull(assemblies);
        AssembliesReloaded?.Invoke(null, new AssembliesReloadedEventArgs(assemblies));
    }
}
