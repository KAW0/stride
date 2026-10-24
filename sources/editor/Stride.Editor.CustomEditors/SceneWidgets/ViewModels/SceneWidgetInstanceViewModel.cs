// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using Stride.Assets.Presentation.SceneEditor.Widgets;
using Stride.Assets.Presentation.SceneEditor.Widgets.ViewModels;
using Stride.Core.Assets.Editor.View.Behaviors;
using Stride.Core.Assets.Editor.ViewModel;
using Stride.Core.Presentation.Commands;
using Stride.Core.Presentation.ViewModels;
using Stride.Editor.CustomEditors.ViewModel;

namespace Stride.Editor.CustomEditors.SceneWidgets.ViewModels;

internal sealed class SceneWidgetInstanceViewModel : DispatcherViewModel, IInsertChildViewModel, ISceneWidgetInstanceViewModel
{
    private readonly Action<SceneWidgetInstanceViewModel> close;

    private SceneWidgetPlacement placement;

    private int order;
    public SceneWidgetDescriptor Descriptor { get; }
    public SceneWidgetViewModel ViewModel { get; }
    public object View { get; }

    public string WidgetId => ViewModel.WidgetId;

    public string DisplayName => ViewModel.DisplayName;

    public SceneWidgetPlacement Placement
    {
        get => placement;
        set
        {
            if (!SetValue(ref placement, value))
                return;

            ViewModel.UpdatePlacement(value);
        }
    }

    public int Order
    {
        get => order;
        set => SetValue(ref order, value);
    }

    public SceneWidgetPlacementCollectionViewModel? PlacementCollection { get; internal set; }

    public ICommandBase CloseCommand { get; }

    public IReadOnlyList<SceneWidgetMenuItem> MenuItems
    {
        get
        {
            var items = ViewModel.GetMenuItems()?.ToList() ?? [];
            items.Add(new SceneWidgetMenuItem("Close", CloseCommand));
            return items;
        }
    }

    internal SceneWidgetInstanceViewModel(IViewModelServiceProvider serviceProvider, SceneWidgetDescriptor descriptor, SceneWidgetViewModel viewModel, object view, Action<SceneWidgetInstanceViewModel> close)
        : base(serviceProvider)
    {
        Descriptor = descriptor;
        ViewModel = viewModel;
        View = view;
        this.close = close;
        CloseCommand = new AnonymousCommand(serviceProvider, () => this.close(this));
        ViewModel.UpdatePlacement(placement);
    }

    bool IInsertChildViewModel.CanInsertChildren(IReadOnlyCollection<object> children, InsertPosition position, AddChildModifiers modifiers, out string message)
    {
        message = DragDropBehavior.InvalidDropAreaMessage;
        var child = children.OfType<SceneWidgetInstanceViewModel>().SingleOrDefault();
        if (child == null)
            return false;

        message = position == InsertPosition.Before ? $"Insert before {DisplayName}" : $"Insert after {DisplayName}";
        return true;
    }

    void IInsertChildViewModel.InsertChildren(IReadOnlyCollection<object> children, InsertPosition position, AddChildModifiers modifiers)
    {
        var child = children.OfType<SceneWidgetInstanceViewModel>().SingleOrDefault();
        if (child == null || PlacementCollection == null)
            return;

        var sourceCollection = child.PlacementCollection;
        var sameCollection = ReferenceEquals(sourceCollection, PlacementCollection);
        child.PlacementCollection?.Remove(child);
        var targetIndex = PlacementCollection.Items.IndexOf(this);
        if (sameCollection && child.Order < targetIndex)
            targetIndex--;
        if (position == InsertPosition.After)
            targetIndex++;
        targetIndex = System.Math.Max(0, System.Math.Min(targetIndex, PlacementCollection.Items.Count));
        PlacementCollection.Insert(targetIndex, child);
    }
}
