// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using Stride.Core.Assets;
using Stride.Editor.CustomEditors.Events;

namespace Stride.Editor.CustomEditors.Services;

internal abstract class ReloadableDescriptorServiceBase<TDescriptor> : IReloadableDescriptorService<TDescriptor>
    where TDescriptor : class
{
    private readonly Dictionary<string, TDescriptor> descriptors = new(StringComparer.OrdinalIgnoreCase);
    private bool initialized;

    public event EventHandler? DescriptorsChanged;

    public IReadOnlyDictionary<string, TDescriptor> Descriptors => descriptors;

    protected void InitializeDescriptorRegistry()
    {
        if (initialized)
            return;

        RegisterAvailableDescriptors();
        CustomEditorEvents.AssembliesReloaded += ReloadAssemblies;
        initialized = true;
    }

    protected void DisposeDescriptorRegistry()
    {
        if (!initialized)
            return;

        CustomEditorEvents.AssembliesReloaded -= ReloadAssemblies;
        initialized = false;
    }

    protected abstract Assembly DescriptorAssembly { get; }

    protected abstract Type ViewType { get; }

    protected virtual bool CheckAccess() => true;

    protected virtual void Invoke(Action action) => action();

    protected abstract string GetAssemblyId(TDescriptor descriptor);

    protected abstract bool TryCreateDescriptor(Type type, string assemblyKey, [NotNullWhen(true)] out string? descriptorId, [NotNullWhen(true)] out TDescriptor? descriptor);

    protected virtual void OnDescriptorAdded(string descriptorId, TDescriptor descriptor)
    {
    }

    protected virtual void OnDescriptorUpdated(string descriptorId, TDescriptor previousDescriptor, TDescriptor descriptor)
    {
    }

    protected virtual void OnDescriptorRemoved(string descriptorId, TDescriptor descriptor)
    {
    }

    protected virtual void OnDescriptorsReloaded(bool descriptorsChanged)
    {
    }

    protected virtual void OnTypeLoadException(Assembly assembly, ReflectionTypeLoadException exception)
    {
    }

    protected virtual void OnDescriptorCreateException(Type type, Exception exception)
    {
    }

    private void RegisterAvailableDescriptors()
    {
        var descriptorAssembly = DescriptorAssembly;
        var descriptorAssemblyName = descriptorAssembly.GetName();

        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            if (assembly.IsDynamic)
                continue;

            if (assembly != descriptorAssembly)
            {
                var referencesDescriptorAssembly = assembly
                    .GetReferencedAssemblies()
                    .Any(reference => AssemblyName.ReferenceMatchesDefinition(reference, descriptorAssemblyName));
                if (!referencesDescriptorAssembly)
                    continue;
            }

            AddDerivedTypes(assembly, GetAssemblyKey(assembly), descriptors);
        }
    }

    private void ReloadAssemblies(object? sender, AssembliesReloadedEventArgs e)
    {
        if (!CheckAccess())
        {
            Invoke(() => ReloadAssemblies(sender, e));
            return;
        }

        var changedAssemblyIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var refreshedDescriptors = new Dictionary<string, TDescriptor>(StringComparer.OrdinalIgnoreCase);

        foreach (var assemblyInfo in e.Assemblies)
        {
            var assembly = assemblyInfo.Assembly;
            if (assembly == null)
                continue;

            var assemblyKey = GetAssemblyKey(assembly);
            changedAssemblyIds.Add(assemblyKey);
            AddDerivedTypes(assembly, assemblyKey, refreshedDescriptors);
        }

        var descriptorsChanged = false;
        foreach (var existingEntry in descriptors.ToList())
        {
            if (!changedAssemblyIds.Contains(GetAssemblyId(existingEntry.Value)))
                continue;

            if (!refreshedDescriptors.TryGetValue(existingEntry.Key, out var replacement))
            {
                descriptors.Remove(existingEntry.Key);
                OnDescriptorRemoved(existingEntry.Key, existingEntry.Value);
                descriptorsChanged = true;
                continue;
            }

            if (!EqualityComparer<TDescriptor>.Default.Equals(existingEntry.Value, replacement))
            {
                descriptors[existingEntry.Key] = replacement;
                OnDescriptorUpdated(existingEntry.Key, existingEntry.Value, replacement);
                descriptorsChanged = true;
            }
        }

        foreach (var refreshedEntry in refreshedDescriptors)
        {
            if (descriptors.ContainsKey(refreshedEntry.Key))
                continue;

            descriptors[refreshedEntry.Key] = refreshedEntry.Value;
            OnDescriptorAdded(refreshedEntry.Key, refreshedEntry.Value);
            descriptorsChanged = true;
        }

        if (descriptorsChanged)
            DescriptorsChanged?.Invoke(this, EventArgs.Empty);

        OnDescriptorsReloaded(descriptorsChanged);
    }

    private void AddDerivedTypes(Assembly assembly, string assemblyKey, IDictionary<string, TDescriptor> target)
    {
        foreach (var type in GetLoadableTypes(assembly))
        {
            if (!type.IsClass || type.IsAbstract || !ViewType.IsAssignableFrom(type))
                continue;

            try
            {
                if (!TryCreateDescriptor(type, assemblyKey, out var descriptorId, out var descriptor))
                    continue;

                if (string.IsNullOrWhiteSpace(descriptorId) || target.ContainsKey(descriptorId))
                    continue;

                target.Add(descriptorId, descriptor);
            }
            catch (Exception exception)
            {
                OnDescriptorCreateException(type, exception);
            }
        }
    }

    private IEnumerable<Type> GetLoadableTypes(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException exception)
        {
            OnTypeLoadException(assembly, exception);
            return exception.Types.Where(x => x != null)!;
        }
    }

    private static string GetAssemblyKey(Assembly assembly)
    {
        return assembly.FullName ?? assembly.GetName().Name ?? assembly.Location;
    }
}
