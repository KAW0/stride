// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Stride.Core.Assets.Editor.ViewModel;
using Stride.Core.Presentation.ViewModels;
using Stride.Assets.Presentation.AssetEditors.SceneEditor.SceneInteractions;
using Stride.Editor.CustomEditors.CustomTools;
using Stride.Editor.CustomEditors.Services;

namespace Stride.Editor.CustomEditors.ViewModel;

public abstract class StrideEditorExtensionViewModel : DispatcherViewModel
{
    protected abstract string ToolOwnerId { get; }

    protected ICustomToolService CustomToolService => ServiceProvider.Get<ICustomToolService>();

    protected StrideEditorExtensionViewModel(IViewModelServiceProvider serviceProvider)
        : base(serviceProvider)
    {
    }

    protected void ActivateTool(SceneToolDefinition tool)
    {
        ArgumentNullException.ThrowIfNull(tool);
        CustomToolService.ActivateTool(ToolOwnerId, tool);
    }

    protected void ActivateTool(string toolId, Action<SceneToolDefinition>? configure = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(toolId);

        var tool = new SceneToolDefinition(toolId);
        ConfigureDefaultTool(tool);
        configure?.Invoke(tool);
        ActivateTool(tool);
    }

    protected void ActivateTool(string toolId, string viewId, Action<SceneToolDefinition>? configure = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(toolId);
        ArgumentException.ThrowIfNullOrWhiteSpace(viewId);

        ActivateTool(toolId, tool =>
        {
            tool.Hierarchy = SceneToolHierarchy.Scene;
            tool.Scope = SceneToolScope.ToolDefined;
            tool.IsEnabledForView = context => string.Equals(context.ViewId, viewId, StringComparison.OrdinalIgnoreCase);
            configure?.Invoke(tool);
        });
    }

    protected bool IsToolActive(string toolId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(toolId);
        return ServiceProvider.TryGet<ICustomToolService>()?.IsToolActive(ToolOwnerId, toolId) ?? false;
    }

    protected bool IsToolActive(string toolId, string viewId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(toolId);
        ArgumentException.ThrowIfNullOrWhiteSpace(viewId);
        return ServiceProvider.TryGet<ICustomToolService>()?.IsToolActive(ToolOwnerId, toolId, viewId) ?? false;
    }

    protected bool IsToolRequested(string toolId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(toolId);
        return ServiceProvider.TryGet<ICustomToolService>()?.IsToolRequested(ToolOwnerId, toolId) ?? false;
    }

    protected bool IsToolRequested(string toolId, string viewId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(toolId);
        ArgumentException.ThrowIfNullOrWhiteSpace(viewId);
        return ServiceProvider.TryGet<ICustomToolService>()?.IsToolRequested(ToolOwnerId, toolId, viewId) ?? false;
    }

    protected bool DeactivateTool(string toolId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(toolId);
        return ServiceProvider.TryGet<ICustomToolService>()?.DeactivateTool(ToolOwnerId, toolId) ?? false;
    }

    protected SceneToolContext? GetActiveSceneContext()
    {
        return ServiceProvider.TryGet<ICustomToolService>()?.GetActiveSceneContext();
    }

    protected SceneToolHandle? GetActiveTool(string viewId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(viewId);
        return ServiceProvider.TryGet<ICustomToolService>()?.GetActiveTool(viewId);
    }

    protected bool DeactivateAllTools()
    {
        return ServiceProvider.TryGet<ICustomToolService>()?.DeactivateEditorTools(ToolOwnerId) ?? false;
    }

    protected virtual void ConfigureDefaultTool(SceneToolDefinition tool)
    {
        ArgumentNullException.ThrowIfNull(tool);
    }

    protected void RunOnDispatcher(Action action)
    {
        ArgumentNullException.ThrowIfNull(action);
        Dispatcher.Invoke(action);
    }

    protected TResult RunOnDispatcher<TResult>(Func<TResult> action)
    {
        ArgumentNullException.ThrowIfNull(action);
        return Dispatcher.Invoke(action);
    }

    protected Task RunOnDispatcherAsync(Action action, CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(action);
        return Dispatcher.InvokeAsync(action, token);
    }

    protected Task<TResult> RunOnDispatcherAsync<TResult>(Func<TResult> action, CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(action);
        return Dispatcher.InvokeAsync(action, token);
    }

    protected Task RunOnDispatcherTaskAsync(Func<Task> action, CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(action);
        return Dispatcher.InvokeTask(action, token);
    }

    protected Task<TResult> RunOnDispatcherTaskAsync<TResult>(Func<Task<TResult>> action, CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(action);
        return Dispatcher.InvokeTask(action, token);
    }
}
