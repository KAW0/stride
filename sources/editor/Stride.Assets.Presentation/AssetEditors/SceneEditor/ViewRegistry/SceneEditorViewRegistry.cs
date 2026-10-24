// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Stride.Assets.Presentation.AssetEditors.SceneEditor.Services;
using Stride.Core.Annotations;

namespace Stride.Assets.Presentation.AssetEditors.SceneEditor.ViewRegistry;

public sealed class SceneEditorViewRegistry : ISceneEditorViewRegistry
{
    private readonly Lock syncRoot = new();
    private readonly List<SceneEditorViewRegistration> views = [];
    private SceneEditorViewRegistration activeView;

    public event EventHandler<SceneEditorViewChangedEventArgs> ViewRegistered;

    public event EventHandler<SceneEditorViewChangedEventArgs> ViewUnregistered;

    public event EventHandler<SceneEditorViewChangedEventArgs> ActiveViewChanged;

    [NotNull]
    public IReadOnlyList<SceneEditorViewRegistration> Views
    {
        get
        {
            lock (syncRoot)
                return views.ToList();
        }
    }

    public SceneEditorViewRegistration ActiveView
    {
        get
        {
            lock (syncRoot)
                return activeView;
        }
    }

    public void RegisterView([NotNull] SceneEditorViewRegistration view)
    {
        ArgumentNullException.ThrowIfNull(view);

        SceneEditorViewChangedEventArgs registeredArgs;
        SceneEditorViewChangedEventArgs unregisteredArgs = null;
        SceneEditorViewChangedEventArgs activeChangedArgs = null;
        lock (syncRoot)
        {
            var existingIndex = views.FindIndex(x => string.Equals(x.ViewId, view.ViewId, StringComparison.OrdinalIgnoreCase));
            if (existingIndex >= 0)
            {
                var previous = views[existingIndex];
                views[existingIndex] = view;
                previous.IsActive = false;
                view.IsActive = previous.IsActive;
                if (ReferenceEquals(activeView, previous) || string.Equals(activeView?.ViewId, view.ViewId, StringComparison.OrdinalIgnoreCase))
                {
                    activeView = view;
                    view.IsActive = true;
                    activeChangedArgs = new SceneEditorViewChangedEventArgs(view);
                }
                unregisteredArgs = new SceneEditorViewChangedEventArgs(previous);
            }
            else
            {
                views.Add(view);
                if (activeView == null)
                {
                    view.IsActive = true;
                    activeView = view;
                    activeChangedArgs = new SceneEditorViewChangedEventArgs(view);
                }
            }

            registeredArgs = new SceneEditorViewChangedEventArgs(view);
        }

        if (unregisteredArgs != null)
            ViewUnregistered?.Invoke(this, unregisteredArgs);
        ViewRegistered?.Invoke(this, registeredArgs);
        if (activeChangedArgs != null)
            ActiveViewChanged?.Invoke(this, activeChangedArgs);
    }

    public bool UnregisterView(string viewId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(viewId);

        SceneEditorViewRegistration removedView = null;
        SceneEditorViewChangedEventArgs activeChangedArgs = null;
        lock (syncRoot)
        {
            var index = views.FindIndex(x => string.Equals(x.ViewId, viewId, StringComparison.OrdinalIgnoreCase));
            if (index < 0)
                return false;

            removedView = views[index];
            views.RemoveAt(index);
            removedView.IsActive = false;

            if (ReferenceEquals(activeView, removedView) || string.Equals(activeView?.ViewId, viewId, StringComparison.OrdinalIgnoreCase))
            {
                activeView = views.LastOrDefault();
                if (activeView != null)
                {
                    activeView.IsActive = true;
                    activeChangedArgs = new SceneEditorViewChangedEventArgs(activeView);
                }
            }
        }

        ViewUnregistered?.Invoke(this, new SceneEditorViewChangedEventArgs(removedView));
        if (activeChangedArgs != null)
            ActiveViewChanged?.Invoke(this, activeChangedArgs);
        return true;
    }

    public bool SetActiveView([NotNull] string viewId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(viewId);

        SceneEditorViewRegistration changedView;
        lock (syncRoot)
        {
            changedView = views.FirstOrDefault(x => string.Equals(x.ViewId, viewId, StringComparison.OrdinalIgnoreCase));
            if (changedView == null || ReferenceEquals(activeView, changedView))
                return false;

            activeView?.IsActive = false;

            changedView.IsActive = true;
            activeView = changedView;
        }

        ActiveViewChanged?.Invoke(this, new SceneEditorViewChangedEventArgs(changedView));
        return true;
    }
}
