// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Stride.Core;
using Stride.Core.Diagnostics;
using Stride.Core.Presentation.ViewModels;
using Stride.Editor.CustomEditors.Annotations;
using Stride.Editor.CustomEditors.Infrastructure;
using Stride.Editor.CustomEditors.Models;
using Stride.Editor.CustomEditors.ViewModel;

namespace Stride.Editor.CustomEditors.Services;

internal sealed class CustomEditorsService : ReloadableDescriptorServiceBase<CustomEditorDescriptor>, ICustomEditorsService
{
    private static readonly Logger Logger = GlobalLogger.GetLogger(nameof(CustomEditorsService));
    private readonly ICustomEditorServiceViewBridge bridge;
    private readonly IViewModelServiceProvider serviceProvider;
    private readonly Dictionary<string, ICustomEditorInstance> editorInstances = new(StringComparer.OrdinalIgnoreCase);

    public event EventHandler<CustomEditorDescriptorEventArgs>? EditorDescriptorAdded;
    public event EventHandler<CustomEditorDescriptorEventArgs>? EditorDescriptorUpdated;
    public event EventHandler<CustomEditorDescriptorEventArgs>? EditorDescriptorRemoved;
    public event EventHandler<CustomEditorVisibilityChangedEventArgs>? EditorVisibilityChanged;
    public event EventHandler<CustomEditorInstanceEventArgs>? EditorOpened;
    public event EventHandler<CustomEditorInstanceEventArgs>? EditorClosed;

    public IReadOnlyDictionary<string, string> AvailableEditorTypes =>
        Descriptors.ToDictionary(x => x.Key, x => x.Value.EditorName, StringComparer.OrdinalIgnoreCase);

    public IReadOnlyCollection<string> OpenEditorIds => OpenInstances
        .Select(x => x.EditorId)
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .ToList();

    public IReadOnlyCollection<CustomEditorInstanceInfo> OpenInstances => editorInstances.Values
        .Where(x => x.IsDockVisible && bridge.IsDockHandleAttached(x.DockHandle))
        .Select(x => new CustomEditorInstanceInfo(x.Descriptor.EditorId, x.ViewModel.InstanceId, x.IsDockVisible))
        .ToList();

    public CustomEditorsService(ICustomEditorServiceViewBridge bridge, IViewModelServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(bridge);
        ArgumentNullException.ThrowIfNull(serviceProvider);
        this.bridge = bridge;
        this.serviceProvider = serviceProvider;
        InitializeDescriptorRegistry();
    }

    public async Task<ICustomEditorHandle> OpenAsync(string editorId, string instanceId = "default", CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(editorId);
        ArgumentException.ThrowIfNullOrWhiteSpace(instanceId);
        cancellationToken.ThrowIfCancellationRequested();

        if (!Descriptors.TryGetValue(editorId, out var descriptor))
            throw new InvalidOperationException($"Editor '{editorId}' is not registered.");

        if (!bridge.CheckAccess())
            return await bridge.Invoke(() => OpenAsync(editorId, instanceId, cancellationToken));

        var instanceKey = GetInstanceKey(editorId, instanceId);
        if (editorInstances.TryGetValue(instanceKey, out var existingInstance))
        {
            if (!bridge.IsDockHandleAttached(existingInstance.DockHandle))
                DetachEditorInstance(instanceKey, existingInstance, true);
            else
            {
                existingInstance.ActivateDock();
                return CreateHandle(instanceKey);
            }
        }

        var dockHandle = bridge.GetOrCreateDockHandle(instanceKey);
        var context = new CustomEditorContext(descriptor.EditorId, instanceId, descriptor.EditorName);
        var viewModel = CreateViewModel(descriptor.ViewModelType, context);
        ICustomEditorInstance? instance = null;
        try
        {
            instance = bridge.CreateEditorInstance(dockHandle, descriptor, viewModel);
            AttachEditorInstance(instanceKey, instance);
            await viewModel.InitializeAsync(cancellationToken);
            instance.ActivateDock();
            EditorOpened?.Invoke(this, new CustomEditorInstanceEventArgs(editorId, instanceId, viewModel));
            return CreateHandle(instanceKey);
        }
        catch
        {
            if (instance != null)
                DetachEditorInstance(instanceKey, instance, true, false);
            else
                viewModel.Destroy();
            bridge.CloseDockHandle(dockHandle);
            throw;
        }
    }

    public bool IsEditorVisible(string editorId) => IsEditorVisible(editorId, "default");

    public bool IsEditorVisible(string editorId, string instanceId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(editorId);
        ArgumentException.ThrowIfNullOrWhiteSpace(instanceId);
        if (!bridge.CheckAccess())
            return bridge.Invoke(() => IsEditorVisible(editorId, instanceId));

        return editorInstances.TryGetValue(GetInstanceKey(editorId, instanceId), out var instance) &&
               bridge.IsDockHandleAttached(instance.DockHandle) && instance.IsDockVisible;
    }

    public bool TryGetInstance(string editorId, string instanceId, [NotNullWhen(true)] out ICustomEditorHandle? handle)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(editorId);
        ArgumentException.ThrowIfNullOrWhiteSpace(instanceId);
        if (!bridge.CheckAccess())
        {
            var result = bridge.Invoke(() => TryGetInstanceCore(GetInstanceKey(editorId, instanceId)));
            handle = result;
            return result != null;
        }

        var key = GetInstanceKey(editorId, instanceId);
        handle = TryGetInstanceCore(key);
        return handle != null;
    }

    private ICustomEditorHandle? TryGetInstanceCore(string key)
    {
        if (editorInstances.TryGetValue(key, out var instance) && bridge.IsDockHandleAttached(instance.DockHandle))
        {
            return CreateHandle(key);
        }

        return null;
    }

    public async Task<ICustomEditorHandle?> SetVisibleAsync(string editorId, bool isVisible, string instanceId = "default", CancellationToken cancellationToken = default)
    {
        return isVisible
            ? await OpenAsync(editorId, instanceId, cancellationToken)
            : await CloseAndReturnHandleAsync(editorId, instanceId, cancellationToken);
    }

    public async Task<ICustomEditorHandle?> ToggleAsync(string editorId, string instanceId = "default", CancellationToken cancellationToken = default)
    {
        return await SetVisibleAsync(editorId, !IsEditorVisible(editorId, instanceId), instanceId, cancellationToken);
    }

    public bool TryActivate(string contentId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(contentId);
        if (!bridge.CheckAccess())
            return bridge.Invoke(() => TryActivate(contentId));
        return editorInstances.TryGetValue(contentId, out var instance) && ActivateIfAttached(contentId, instance);
    }

    public async Task<bool> CloseAsync(string editorId, string instanceId = "default", CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(editorId);
        ArgumentException.ThrowIfNullOrWhiteSpace(instanceId);
        cancellationToken.ThrowIfCancellationRequested();
        var key = GetInstanceKey(editorId, instanceId);
        if (!bridge.CheckAccess())
            return await bridge.Invoke(() => CloseAsync(editorId, instanceId, cancellationToken));
        if (!editorInstances.TryGetValue(key, out var instance))
            return false;
        if (!bridge.IsDockHandleAttached(instance.DockHandle))
        {
            DetachEditorInstance(key, instance, true);
            return false;
        }
        if (!await instance.ViewModel.CanCloseAsync(cancellationToken))
            return false;
        await instance.ViewModel.CloseAsync(cancellationToken);
        CloseEditorTools(key);
        instance.HideDock();
        return true;
    }

    private async Task<ICustomEditorHandle?> CloseAndReturnHandleAsync(string editorId, string instanceId, CancellationToken cancellationToken)
    {
        TryGetInstance(editorId, instanceId, out var handle);
        return await CloseAsync(editorId, instanceId, cancellationToken) ? handle : null;
    }

    public void Dispose()
    {
        if (!bridge.CheckAccess())
        {
            bridge.Invoke(Dispose);
            return;
        }
        foreach (var key in editorInstances.Keys.ToList())
            RemoveEditorInstance(key);
        DisposeDescriptorRegistry();
    }

    private bool ActivateIfAttached(string key, ICustomEditorInstance instance)
    {
        if (!bridge.IsDockHandleAttached(instance.DockHandle))
        {
            DetachEditorInstance(key, instance, true);
            return false;
        }
        instance.ActivateDock();
        return true;
    }

    private CustomEditorViewModel CreateViewModel(Type viewModelType, CustomEditorContext context)
    {
        try
        {
            var constructor = viewModelType.GetConstructor(new[] { typeof(IViewModelServiceProvider), typeof(CustomEditorContext) });
            if (constructor != null)
                return (CustomEditorViewModel)constructor.Invoke(new object[] { serviceProvider, context });

            throw new InvalidOperationException($"Custom editor view model '{viewModelType.FullName}' must expose a constructor accepting IViewModelServiceProvider and CustomEditorContext.");
        }
        catch (Exception exception)
        {
            throw new InvalidOperationException($"Unable to create custom editor view model '{viewModelType.FullName}'.", exception);
        }
    }

    private void AttachEditorInstance(string key, ICustomEditorInstance instance)
    {
        if (editorInstances.TryGetValue(key, out var previous))
        {
            previous.DockClosed -= OnDockClosed;
            previous.DockIsVisibleChanged -= OnDockIsVisibleChanged;
        }
        instance.DockClosed += OnDockClosed;
        instance.DockIsVisibleChanged += OnDockIsVisibleChanged;
        editorInstances[key] = instance;
    }

    private void RemoveEditorInstance(string key)
    {
        if (!editorInstances.TryGetValue(key, out var instance))
            return;
        DetachEditorInstance(key, instance, true);
        bridge.CloseDockHandle(instance.DockHandle);
    }

    private void DetachEditorInstance(string key, ICustomEditorInstance instance, bool clearPaneContent, bool raiseClosed = true)
    {
        editorInstances.Remove(key);
        CloseEditorTools(key);
        if (raiseClosed)
            EditorClosed?.Invoke(this, new CustomEditorInstanceEventArgs(instance.Descriptor.EditorId, instance.ViewModel.InstanceId, instance.ViewModel));
        instance.DockClosed -= OnDockClosed;
        instance.DockIsVisibleChanged -= OnDockIsVisibleChanged;
        if (clearPaneContent)
            instance.ClearContent();
        if (instance.ViewModel is IDestroyable destroyable)
            destroyable.Destroy();
        instance.Dispose();
    }

    private void CloseEditorTools(string ownerId) => serviceProvider.TryGet<ICustomToolService>()?.CloseEditorTools(ownerId);

    private ICustomEditorHandle CreateHandle(string key) => new CustomEditorHandle(this, key);

    internal ICustomEditorInstance GetRequiredInstance(string key)
    {
        if (!editorInstances.TryGetValue(key, out var instance))
            throw new ObjectDisposedException(nameof(ICustomEditorHandle));
        return instance;
    }

    private void OnDockClosed(object? sender, EventArgs e)
    {
        if (sender is not ICustomEditorInstance instance)
            return;
        var key = editorInstances.FirstOrDefault(x => ReferenceEquals(x.Value, instance)).Key;
        if (string.IsNullOrEmpty(key))
            return;
        DetachEditorInstance(key, instance, true);
        EditorVisibilityChanged?.Invoke(this, new CustomEditorVisibilityChangedEventArgs(instance.Descriptor.EditorId, instance.ViewModel.InstanceId, false));
    }

    private void OnDockIsVisibleChanged(object? sender, EventArgs e)
    {
        if (sender is not ICustomEditorInstance instance)
            return;
        var key = editorInstances.FirstOrDefault(x => ReferenceEquals(x.Value, instance)).Key;
        if (string.IsNullOrEmpty(key))
            return;
        if (!instance.IsDockVisible)
            CloseEditorTools(key);
        EditorVisibilityChanged?.Invoke(this, new CustomEditorVisibilityChangedEventArgs(instance.Descriptor.EditorId, instance.ViewModel.InstanceId, instance.IsDockVisible));
    }

    protected override Assembly DescriptorAssembly => bridge.ViewType.Assembly;
    protected override Type ViewType => bridge.ViewType;
    protected override bool CheckAccess() => bridge.CheckAccess();
    protected override void Invoke(Action action) => bridge.Invoke(action);
    protected override string GetAssemblyId(CustomEditorDescriptor descriptor) => descriptor.AssemblyId;

    protected override bool TryCreateDescriptor(Type type, string assemblyKey, [NotNullWhen(true)] out string? descriptorId, [NotNullWhen(true)] out CustomEditorDescriptor? descriptor)
    {
        descriptorId = null;
        descriptor = null;
        var attribute = type.GetCustomAttributes(inherit: false)
            .OfType<ICustomEditor>()
            .SingleOrDefault();
        if (attribute == null || !typeof(CustomEditorViewModel).IsAssignableFrom(attribute.ViewModelType) || attribute.ViewModelType.IsAbstract)
            return false;
        descriptorId = attribute.EditorId;
        descriptor = new CustomEditorDescriptor(assemblyKey, attribute.EditorId, type, attribute.ViewModelType, attribute.EditorName);
        return true;
    }

    protected override void OnDescriptorAdded(string id, CustomEditorDescriptor descriptor) =>
        EditorDescriptorAdded?.Invoke(this, new CustomEditorDescriptorEventArgs(id, descriptor.EditorName));

    protected override void OnDescriptorUpdated(string id, CustomEditorDescriptor previous, CustomEditorDescriptor descriptor)
    {
        foreach (var pair in editorInstances.Where(x => string.Equals(x.Value.Descriptor.EditorId, id, StringComparison.OrdinalIgnoreCase)).ToList())
        {
            try
            {
                var replacementContext = new CustomEditorContext(
                    descriptor.EditorId,
                    pair.Value.ViewModel.InstanceId,
                    descriptor.EditorName);
                var replacementVm = CreateViewModel(descriptor.ViewModelType, replacementContext);
                var replacement = bridge.CreateEditorInstance(pair.Value.DockHandle, descriptor, replacementVm);
                AttachEditorInstance(pair.Key, replacement);
                DetachEditorInstance(pair.Key, pair.Value, false);
            }
            catch (Exception exception)
            {
                Logger.Error($"Unable to reload custom editor '{id}'.", exception);
            }
        }
        EditorDescriptorUpdated?.Invoke(this, new CustomEditorDescriptorEventArgs(id, descriptor.EditorName));
    }

    protected override void OnDescriptorRemoved(string id, CustomEditorDescriptor descriptor)
    {
        foreach (var key in editorInstances.Where(x => string.Equals(x.Value.Descriptor.EditorId, id, StringComparison.OrdinalIgnoreCase)).Select(x => x.Key).ToList())
            RemoveEditorInstance(key);
        EditorDescriptorRemoved?.Invoke(this, new CustomEditorDescriptorEventArgs(id, descriptor.EditorName));
    }

    protected override void OnDescriptorCreateException(Type type, Exception exception)
    {
        Logger.Error($"Unable to create custom editor descriptor for '{type.FullName}'.", exception);
    }

    private static string GetInstanceKey(string editorId, string instanceId) => $"{editorId}::{instanceId}";
}
