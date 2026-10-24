// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using Stride.Assets.Presentation.AssetEditors.SceneEditor.SceneInteractions;
using Stride.Assets.Presentation.AssetEditors.SceneEditor.ViewRegistry;
using Stride.Core.Extensions;
using Stride.Editor.CustomEditors.CustomTools;
using Stride.Editor.CustomEditors.Models;

namespace Stride.Editor.CustomEditors.Services;

internal sealed class CustomToolService : ICustomToolService
{
    private readonly ISceneEditorViewRegistry sceneViewRegistry;

    private readonly List<SceneToolHandle> activeTools = new();

    public event EventHandler<CustomToolChangedEventArgs>? ActiveToolChanged;

    public event EventHandler<CustomToolChangedEventArgs>? RequestedToolChanged;

    public SceneToolHandle? RequestedTool => sceneViewRegistry.ActiveView == null ? null : GetRequestedTool(sceneViewRegistry.ActiveView.ViewId);

    public SceneToolHandle? EffectiveTool => sceneViewRegistry.ActiveView == null ? null : GetEffectiveTool(sceneViewRegistry.ActiveView.ViewId);

    public SceneToolHandle? ActiveTool => EffectiveTool;

    public CustomToolService(ISceneEditorViewRegistry sceneViewRegistry)
    {
        ArgumentNullException.ThrowIfNull(sceneViewRegistry);
        this.sceneViewRegistry = sceneViewRegistry;
        this.sceneViewRegistry.ViewRegistered += OnSceneViewRegistered;
        this.sceneViewRegistry.ViewUnregistered += OnSceneViewUnregistered;
        this.sceneViewRegistry.ActiveViewChanged += OnActiveViewChanged;
    }

    public SceneToolHandle? GetActiveTool(string viewId)
    {
        return GetEffectiveTool(viewId);
    }

    public SceneToolHandle? GetRequestedTool(string viewId)
    {
        if (string.IsNullOrWhiteSpace(viewId))
            return null;

        var sceneView = sceneViewRegistry.Views.FirstOrDefault(x => string.Equals(x.ViewId, viewId, StringComparison.OrdinalIgnoreCase));
        if (sceneView == null)
            return null;

        return activeTools.LastOrDefault(x => IsTargetView(x, sceneView));
    }

    public SceneToolHandle? GetEffectiveTool(string viewId)
    {
        if (string.IsNullOrWhiteSpace(viewId))
            return null;

        var sceneView = sceneViewRegistry.Views.FirstOrDefault(x => string.Equals(x.ViewId, viewId, StringComparison.OrdinalIgnoreCase));
        var interaction = sceneView?.InteractionService.ActiveInteraction;
        if (interaction == null)
            return null;

        return activeTools.FirstOrDefault(x =>
            string.Equals(x.OwnerId, interaction.OwnerId, StringComparison.OrdinalIgnoreCase)
            && string.Equals(x.Definition.Id, interaction.Id, StringComparison.OrdinalIgnoreCase)
            && x.Definition.Hierarchy == interaction.Hierarchy);
    }

    public SceneToolContext? GetActiveSceneContext()
    {
        return sceneViewRegistry.ActiveView == null
            ? null
            : new SceneToolContext(ActiveTool?.OwnerId ?? string.Empty, sceneViewRegistry.ActiveView);
    }

    public void ActivateTool(string editorId, SceneToolDefinition tool)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(editorId);
        ArgumentNullException.ThrowIfNull(tool);

        var activation = new SceneToolHandle(editorId, tool);
        var previousRequestedTool = RequestedTool;
        var previousEffectiveTool = EffectiveTool;
        var replacedActivations = activeTools
            .Where(x =>
                string.Equals(x.OwnerId, activation.OwnerId, StringComparison.OrdinalIgnoreCase)
                && x.Definition.Hierarchy == activation.Definition.Hierarchy
                && !AreEquivalent(x, activation))
            .ToList();

        foreach (var replacedActivation in replacedActivations)
        {
            activeTools.RemoveAll(x => AreEquivalent(x, replacedActivation));
            DeactivateActivationFromViews(replacedActivation, forgetLastInteraction: false);
        }

        activeTools.RemoveAll(x => AreEquivalent(x, activation));
        activeTools.Add(activation);

        SyncActivationToViews(activation);
        RaiseToolChanged(previousRequestedTool, RequestedTool, previousEffectiveTool, EffectiveTool);
    }

    public bool IsToolActive(string editorId, string toolId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(editorId);
        ArgumentException.ThrowIfNullOrWhiteSpace(toolId);

        var activeTool = ActiveTool;
        return activeTool != null
            && string.Equals(activeTool.OwnerId, editorId, StringComparison.OrdinalIgnoreCase)
            && string.Equals(activeTool.Definition.Id, toolId, StringComparison.OrdinalIgnoreCase);
    }

    public bool IsToolActive(string editorId, string toolId, string viewId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(editorId);
        ArgumentException.ThrowIfNullOrWhiteSpace(toolId);
        ArgumentException.ThrowIfNullOrWhiteSpace(viewId);

        var activeTool = GetActiveTool(viewId);
        return activeTool != null
            && string.Equals(activeTool.OwnerId, editorId, StringComparison.OrdinalIgnoreCase)
            && string.Equals(activeTool.Definition.Id, toolId, StringComparison.OrdinalIgnoreCase);
    }

    public bool IsToolRequested(string editorId, string toolId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(editorId);
        ArgumentException.ThrowIfNullOrWhiteSpace(toolId);

        var requestedTool = RequestedTool;
        return requestedTool != null
            && string.Equals(requestedTool.OwnerId, editorId, StringComparison.OrdinalIgnoreCase)
            && string.Equals(requestedTool.Definition.Id, toolId, StringComparison.OrdinalIgnoreCase);
    }

    public bool IsToolRequested(string editorId, string toolId, string viewId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(editorId);
        ArgumentException.ThrowIfNullOrWhiteSpace(toolId);
        ArgumentException.ThrowIfNullOrWhiteSpace(viewId);

        var requestedTool = GetRequestedTool(viewId);
        return requestedTool != null
            && string.Equals(requestedTool.OwnerId, editorId, StringComparison.OrdinalIgnoreCase)
            && string.Equals(requestedTool.Definition.Id, toolId, StringComparison.OrdinalIgnoreCase);
    }

    public bool DeactivateTool(string editorId, string toolId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(editorId);
        ArgumentException.ThrowIfNullOrWhiteSpace(toolId);

        var activation = activeTools.FirstOrDefault(x =>
            string.Equals(x.OwnerId, editorId, StringComparison.OrdinalIgnoreCase)
            && string.Equals(x.Definition.Id, toolId, StringComparison.OrdinalIgnoreCase));

        var activeTool = ActiveTool;
        if (activeTool != null
            && string.Equals(activeTool.OwnerId, editorId, StringComparison.OrdinalIgnoreCase)
            && string.Equals(activeTool.Definition.Id, toolId, StringComparison.OrdinalIgnoreCase))
        {
            activation = activeTool;
        }

        return activation != null && DeactivateTool(activation, forgetLastInteraction: false);
    }

    public bool DeactivateActiveTool()
    {
        return ActiveTool != null && DeactivateTool(ActiveTool, forgetLastInteraction: false);
    }

    public bool DeactivateEditorTools(string editorId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(editorId);

        var matchingTools = activeTools
            .Where(x => string.Equals(x.OwnerId, editorId, StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (matchingTools.Count == 0)
            return DeactivateOwnerFromViews(editorId, forgetLastInteraction: false);

        var changed = false;
        foreach (var tool in matchingTools)
            changed |= DeactivateTool(tool, forgetLastInteraction: false);

        return changed;
    }

    public bool CloseEditorTools(string editorId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(editorId);

        var matchingTools = activeTools
            .Where(x => string.Equals(x.OwnerId, editorId, StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (matchingTools.Count == 0)
            return DeactivateOwnerFromViews(editorId, forgetLastInteraction: true);

        var changed = false;
        foreach (var tool in matchingTools)
            changed |= DeactivateTool(tool, forgetLastInteraction: true);

        return changed;
    }

    private void OnSceneViewRegistered(object? sender, SceneEditorViewChangedEventArgs e)
    {
        foreach (var tool in activeTools)
            SyncActivationToViews(tool);
    }

    private void OnSceneViewUnregistered(object? sender, SceneEditorViewChangedEventArgs e)
    {
        // The registration is no longer returned by the registry, so clean the
        // interaction state on the detached service before the view can be GC'd.
        foreach (var ownerId in activeTools.Select(x => x.OwnerId).Distinct(StringComparer.OrdinalIgnoreCase))
        {
            e.View.InteractionService.DeactivateOwnerInteractions(ownerId);
            e.View.InteractionService.ForgetLastInteraction(ownerId);
        }
    }

    private void OnActiveViewChanged(object? sender, SceneEditorViewChangedEventArgs e)
    {
        var previousRequestedTool = RequestedTool;
        var previousEffectiveTool = EffectiveTool;
        foreach (var tool in activeTools.Where(x => x.Definition.Scope == SceneToolScope.ActiveView))
            SyncActivationToViews(tool);

        RaiseToolChanged(previousRequestedTool, RequestedTool, previousEffectiveTool, EffectiveTool);
    }

    private static bool AreEquivalent(SceneToolHandle? left, SceneToolHandle? right)
    {
        return left == null
            ? right == null
            : right != null
               && string.Equals(left.OwnerId, right.OwnerId, StringComparison.OrdinalIgnoreCase)
               && string.Equals(left.Definition.Id, right.Definition.Id, StringComparison.OrdinalIgnoreCase)
               && left.Definition.Hierarchy == right.Definition.Hierarchy;
    }

    private bool DeactivateTool(SceneToolHandle? activation, bool forgetLastInteraction)
    {
        if (activation == null)
            return false;

        var previousRequestedTool = RequestedTool;
        var previousEffectiveTool = EffectiveTool;
        activeTools.RemoveAll(x => AreEquivalent(x, activation));
        var changed = DeactivateActivationFromViews(activation, forgetLastInteraction);
        RaiseToolChanged(previousRequestedTool, RequestedTool, previousEffectiveTool, EffectiveTool);
        return changed;
    }

    private bool DeactivateOwnerFromViews(string editorId, bool forgetLastInteraction)
    {
        var changed = false;
        foreach (var sceneView in sceneViewRegistry.Views)
        {
            changed |= sceneView.InteractionService.DeactivateOwnerInteractions(editorId);
            if (forgetLastInteraction)
                changed |= sceneView.InteractionService.ForgetLastInteraction(editorId);
        }

        return changed;
    }

    private bool DeactivateActivationFromViews(SceneToolHandle activation, bool forgetLastInteraction)
    {
        var changed = false;
        foreach (var sceneView in sceneViewRegistry.Views)
        {
            changed |= sceneView.InteractionService.DeactivateInteraction(activation.OwnerId, activation.Definition.Id, activation.Definition.Hierarchy);
            if (forgetLastInteraction)
                changed |= sceneView.InteractionService.ForgetLastInteraction(activation.OwnerId);
        }

        return changed;
    }

    private void SyncActivationToViews(SceneToolHandle activation)
    {
        var targetViewIds = GetTargetViews(activation).Select(x => x.ViewId).ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var sceneView in sceneViewRegistry.Views)
        {
            if (!targetViewIds.Contains(sceneView.ViewId))
            {
                sceneView.InteractionService.DeactivateInteraction(activation.OwnerId, activation.Definition.Id, activation.Definition.Hierarchy);
                continue;
            }

            var context = new SceneToolContext(activation.OwnerId, sceneView);
            sceneView.InteractionService.ActivateInteraction(CreateInteractionSession(activation, context));
        }
    }

    private IEnumerable<SceneEditorViewRegistration> GetTargetViews(SceneToolHandle activation)
    {
        return activation.Definition.Scope switch
        {
            SceneToolScope.AllViews => sceneViewRegistry.Views,
            SceneToolScope.ToolDefined => sceneViewRegistry.Views.Where(view =>
                activation.Definition.IsEnabledForView?.Invoke(new SceneToolContext(activation.OwnerId, view)) == true),
            _ => sceneViewRegistry.ActiveView is { } activeView ? new[] { activeView } : Enumerable.Empty<SceneEditorViewRegistration>(),
        };
    }

    private static bool IsTargetView(SceneToolHandle activation, SceneEditorViewRegistration view)
    {
        return activation.Definition.Scope switch
        {
            SceneToolScope.AllViews => true,
            SceneToolScope.ToolDefined => activation.Definition.IsEnabledForView?.Invoke(new SceneToolContext(activation.OwnerId, view)) == true,
            _ => true,
        };
    }

    private static SceneEditorInteractionSession CreateInteractionSession(SceneToolHandle activation, SceneToolContext context)
    {
        return new SceneEditorInteractionSession(activation.OwnerId, activation.Definition.Id, context, activation.Definition.Priority)
        {
            DisplayName = activation.Definition.DisplayName,
            Hierarchy = activation.Definition.Hierarchy,
            CanHandleMouseClick = clickContext => activation.Definition.CanHandleClick?.Invoke(clickContext) ?? false,
            HandleMouseClick = clickContext => activation.Definition.HandleClick?.Invoke(clickContext) ?? false,
            IsDefaultInputEnabled = input => activation.Definition.IsDefaultInputEnabled?.Invoke(context, input) ?? true,
            GetContextMenuItems = () => activation.Definition.GetContextMenuItems?.Invoke(context)?
                                      .Where(x => x != null)
                                      .ToList()
                                  ?? (IReadOnlyList<SceneEditorContextMenuItem>)Array.Empty<SceneEditorContextMenuItem>(),
            ExpandDefaultContextMenu = activation.Definition.ExpandDefaultContextMenu,
        };
    }

    private void RaiseToolChanged(SceneToolHandle? previousRequestedTool, SceneToolHandle? currentRequestedTool, SceneToolHandle? previousEffectiveTool, SceneToolHandle? currentEffectiveTool)
    {
        if (!AreEquivalent(previousRequestedTool, currentRequestedTool))
            RequestedToolChanged?.Invoke(this, new CustomToolChangedEventArgs(previousRequestedTool, currentRequestedTool));

        if (!AreEquivalent(previousEffectiveTool, currentEffectiveTool))
            ActiveToolChanged?.Invoke(this, new CustomToolChangedEventArgs(previousEffectiveTool, currentEffectiveTool));
    }
}
