// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Stride.Core.Annotations;

namespace Stride.Assets.Presentation.AssetEditors.SceneEditor.SceneInteractions;

public sealed class SceneEditorInteractionService : ISceneEditorInteractionService
{
    private readonly Lock syncRoot = new();
    private readonly List<SceneEditorInteractionSession> interactions = new();
    private SceneEditorInteractionSession activeInteraction;
    private SceneEditorInteractionSession lastInteraction;

    public event EventHandler<SceneEditorInteractionChangedEventArgs> ActiveInteractionChanged;

    public SceneEditorInteractionSession ActiveInteraction
    {
        get
        {
            lock (syncRoot)
                return activeInteraction;
        }
    }

    public SceneEditorInteractionSession LastInteraction
    {
        get
        {
            lock (syncRoot)
                return lastInteraction;
        }
    }

    public void ActivateInteraction(SceneEditorInteractionSession interaction)
    {
        ArgumentNullException.ThrowIfNull(interaction);

        SceneEditorInteractionChangedEventArgs eventArgs = null;
        lock (syncRoot)
        {
            var existingIndex = interactions.FindIndex(x =>
                string.Equals(x.OwnerId, interaction.OwnerId, StringComparison.OrdinalIgnoreCase)
                && string.Equals(x.Id, interaction.Id, StringComparison.OrdinalIgnoreCase)
                && x.Hierarchy == interaction.Hierarchy);

            if (existingIndex >= 0)
            {
                interactions.RemoveAt(existingIndex);
                interactions.Add(interaction);
            }
            else
                interactions.Add(interaction);

            eventArgs = UpdateActiveInteraction_NoLock();
        }

        RaiseChanged(eventArgs);
    }

    public bool IsInteractionActive([NotNull] string ownerId, [NotNull] string interactionId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(ownerId);
        ArgumentException.ThrowIfNullOrWhiteSpace(interactionId);

        lock (syncRoot)
        {
            return activeInteraction != null
                && string.Equals(activeInteraction.OwnerId, ownerId, StringComparison.OrdinalIgnoreCase)
                && string.Equals(activeInteraction.Id, interactionId, StringComparison.OrdinalIgnoreCase);
        }
    }

    public bool DeactivateInteraction([NotNull] string ownerId, [NotNull] string interactionId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(ownerId);
        ArgumentException.ThrowIfNullOrWhiteSpace(interactionId);

        SceneEditorInteractionChangedEventArgs eventArgs = null;
        bool removed;
        lock (syncRoot)
        {
            removed = interactions.RemoveAll(x =>
                string.Equals(x.OwnerId, ownerId, StringComparison.OrdinalIgnoreCase)
                && string.Equals(x.Id, interactionId, StringComparison.OrdinalIgnoreCase)) > 0;

            if (removed)
                eventArgs = UpdateActiveInteraction_NoLock();
        }

        RaiseChanged(eventArgs);
        return removed;
    }

    public bool DeactivateInteraction([NotNull] string ownerId, [NotNull] string interactionId, SceneToolHierarchy hierarchy)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(ownerId);
        ArgumentException.ThrowIfNullOrWhiteSpace(interactionId);

        SceneEditorInteractionChangedEventArgs eventArgs = null;
        bool removed;
        lock (syncRoot)
        {
            removed = interactions.RemoveAll(x =>
                string.Equals(x.OwnerId, ownerId, StringComparison.OrdinalIgnoreCase)
                && string.Equals(x.Id, interactionId, StringComparison.OrdinalIgnoreCase)
                && x.Hierarchy == hierarchy) > 0;

            if (removed)
                eventArgs = UpdateActiveInteraction_NoLock();
        }

        RaiseChanged(eventArgs);
        return removed;
    }

    public bool DeactivateActiveInteraction()
    {
        SceneEditorInteractionChangedEventArgs eventArgs = null;
        bool removed;
        lock (syncRoot)
        {
            if (activeInteraction == null)
                return false;

            removed = interactions.RemoveAll(x =>
                string.Equals(x.OwnerId, activeInteraction.OwnerId, StringComparison.OrdinalIgnoreCase)
                && string.Equals(x.Id, activeInteraction.Id, StringComparison.OrdinalIgnoreCase)
                && x.Hierarchy == activeInteraction.Hierarchy) > 0;

            if (removed)
                eventArgs = UpdateActiveInteraction_NoLock();
        }

        RaiseChanged(eventArgs);
        return removed;
    }

    public bool DeactivateOwnerInteractions([NotNull] string ownerId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(ownerId);

        SceneEditorInteractionChangedEventArgs eventArgs = null;
        bool removed;
        lock (syncRoot)
        {
            removed = interactions.RemoveAll(x => string.Equals(x.OwnerId, ownerId, StringComparison.OrdinalIgnoreCase)) > 0;
            if (removed)
                eventArgs = UpdateActiveInteraction_NoLock();
        }

        RaiseChanged(eventArgs);
        return removed;
    }

    public bool RestoreLastInteraction()
    {
        SceneEditorInteractionSession interaction;
        lock (syncRoot)
        {
            interaction = lastInteraction;
        }

        if (interaction == null)
            return false;

        ActivateInteraction(interaction);
        return true;
    }

    public bool ForgetLastInteraction([NotNull] string ownerId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(ownerId);

        lock (syncRoot)
        {
            if (lastInteraction == null
                || !string.Equals(lastInteraction.OwnerId, ownerId, StringComparison.OrdinalIgnoreCase))
                return false;

            lastInteraction = null;
            return true;
        }
    }

    public bool IsDefaultInputEnabled(SceneEditorDefaultInput input)
    {
        try
        {
            return ActiveInteraction?.IsDefaultInputEnabled?.Invoke(input) ?? true;
        }
        catch
        {
            return false;
        }
    }

    public bool CanHandleMouseClick([NotNull] SceneEditorClickContext clickContext)
    {
        ArgumentNullException.ThrowIfNull(clickContext);

        try
        {
            return ActiveInteraction?.CanHandleMouseClick?.Invoke(clickContext) ?? false;
        }
        catch
        {
            return false;
        }
    }

    public bool HandleMouseClick([NotNull] SceneEditorClickContext clickContext)
    {
        ArgumentNullException.ThrowIfNull(clickContext);

        try
        {
            return ActiveInteraction?.CanHandleMouseClick?.Invoke(clickContext) == true
                && ActiveInteraction?.HandleMouseClick?.Invoke(clickContext) == true;
        }
        catch
        {
            return false;
        }
    }

    [NotNull]
    public IReadOnlyList<SceneEditorContextMenuItem> GetContextMenuItems()
    {
        try
        {
            return ActiveInteraction?.GetContextMenuItems?.Invoke()?.Where(x => x != null).ToList()
                   ?? (IReadOnlyList<SceneEditorContextMenuItem>)Array.Empty<SceneEditorContextMenuItem>();
        }
        catch
        {
            return Array.Empty<SceneEditorContextMenuItem>();
        }
    }

    public bool ShouldExpandDefaultContextMenu()
    {
        try
        {
            return ActiveInteraction?.ExpandDefaultContextMenu ?? false;
        }
        catch
        {
            return false;
        }
    }

    [CanBeNull]
    private SceneEditorInteractionChangedEventArgs UpdateActiveInteraction_NoLock()
    {
        var previousInteraction = activeInteraction;
        activeInteraction = interactions
            .OrderByDescending(x => x.Hierarchy)
            .ThenByDescending(x => x.Priority)
            .ThenByDescending(x => interactions.IndexOf(x))
            .FirstOrDefault();

        if (activeInteraction != null)
            lastInteraction = activeInteraction;

        return ReferenceEquals(previousInteraction, activeInteraction)
            ? null
            : new SceneEditorInteractionChangedEventArgs(previousInteraction, activeInteraction);
    }

    private void RaiseChanged(SceneEditorInteractionChangedEventArgs eventArgs)
    {
        if (eventArgs != null)
            ActiveInteractionChanged?.Invoke(this, eventArgs);
    }
}
