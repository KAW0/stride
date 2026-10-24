// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using System.Collections.Generic;
using System.Linq;
using Stride.Assets.Presentation.SceneEditor.Widgets;
using Stride.Core.Assets.Editor.View.Behaviors;
using Stride.Core.Assets.Editor.ViewModel;
using Stride.Core.Presentation.Collections;

namespace Stride.Editor.CustomEditors.SceneWidgets.ViewModels;

internal sealed class SceneWidgetPlacementCollectionViewModel : IAddChildViewModel
{
    private readonly System.Action? changed;

    public SceneWidgetPlacement Placement { get; }

    public ObservableList<SceneWidgetInstanceViewModel> Items { get; } = [];

    public bool HasItems => Items.Count > 0;

    public SceneWidgetPlacementCollectionViewModel(SceneWidgetPlacement placement, System.Action? changed = null)
    {
        Placement = placement;
        this.changed = changed;
    }
    public void Add(SceneWidgetInstanceViewModel item)
    {
        item.PlacementCollection = this;
        item.Placement = Placement;
        Items.Add(item);
        RefreshOrder();
        changed?.Invoke();
    }

    public void Insert(int index, SceneWidgetInstanceViewModel item)
    {
        item.PlacementCollection = this;
        item.Placement = Placement;
        Items.Insert(index, item);
        RefreshOrder();
        changed?.Invoke();
    }

    public bool Remove(SceneWidgetInstanceViewModel item)
    {
        var removed = Items.Remove(item);
        if (removed)
        {
            RefreshOrder();
            changed?.Invoke();
        }
        return removed;
    }

    public IEnumerable<SceneWidgetLayoutEntry> CreateLayoutEntries()
    {
        return Items.Select((x, index) => new SceneWidgetLayoutEntry
        {
            WidgetId = x.WidgetId,
            Placement = Placement,
            Order = index,
            Title = x.DisplayName,
        });
    }

    private void RefreshOrder()
    {
        for (var index = 0; index < Items.Count; index++)
        {
            Items[index].Order = index;
        }
    }

    bool IAddChildViewModel.CanAddChildren(IReadOnlyCollection<object> children, AddChildModifiers modifiers, out string message)
    {
        message = DragDropBehavior.InvalidDropAreaMessage;
        var child = children.OfType<SceneWidgetInstanceViewModel>().SingleOrDefault();
        if (child == null)
            return false;

        message = $"Move to {Placement}";
        return true;
    }

    void IAddChildViewModel.AddChildren(IReadOnlyCollection<object> children, AddChildModifiers modifiers)
    {
        var child = children.OfType<SceneWidgetInstanceViewModel>().SingleOrDefault();
        if (child == null)
            return;

        child.PlacementCollection?.Remove(child);
        Add(child);
    }
}
