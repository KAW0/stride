// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Media;
using Stride.Core.Annotations;
using Stride.Core.Assets.Editor.View.Behaviors;
using Stride.Core.Assets.Editor.ViewModel;

namespace Stride.Assets.Presentation.View.Behaviors;

public class SceneWidgetListBoxDragDropBehavior : DragDropBehavior<ListBox, FrameworkElement>
{
    protected override bool CanInitializeDrag(object originalSource)
    {
        return IsDragHandlePath(originalSource as DependencyObject);
    }

    protected override FrameworkElement GetContainer(object source)
    {
        var frameworkElement = source as FrameworkElement;
        if (frameworkElement is ListBoxItem itemContainer)
            return itemContainer;

        var parentItem = FindVisualParent<ListBoxItem>(frameworkElement);
        if (parentItem != null)
            return parentItem;

        return AssociatedObject;
    }

    protected override IEnumerable<object> GetItemsToDrag(FrameworkElement container)
    {
        if (container is ListBoxItem itemContainer)
        {
            var sourceItem = itemContainer.DataContext;
            return AssociatedObject.SelectedItems.Contains(sourceItem) ? AssociatedObject.SelectedItems.Cast<object>() : [sourceItem];
        }

        return Enumerable.Empty<object>();
    }

    protected override IAddChildViewModel GetDropTargetItem(FrameworkElement container)
    {
        return ReferenceEquals(container, AssociatedObject) ? AssociatedObject.Tag as IAddChildViewModel : null;
    }

    protected override IInsertChildViewModel GetInsertTargetItem(FrameworkElement container, Point mousePosition, out InsertPosition insertPosition)
    {
        insertPosition = InsertPosition.Before;

        if (container is not ListBoxItem itemContainer)
            return null;

        if (mousePosition.Y < 0 || mousePosition.Y > itemContainer.ActualHeight)
            return null;

        insertPosition = mousePosition.Y <= itemContainer.ActualHeight / 2 ? InsertPosition.Before : InsertPosition.After;
        return itemContainer.DataContext as IInsertChildViewModel;
    }

    [CanBeNull]
    private static T FindVisualParent<T>(DependencyObject element)
        where T : DependencyObject
    {
        while (element != null)
        {
            element = VisualTreeHelper.GetParent(element);
            if (element is T match)
                return match;
        }

        return null;
    }

    private static bool IsDragHandlePath(DependencyObject element)
    {
        while (element != null)
        {
            if (element is FrameworkElement { Tag: "SceneWidgetDragHandle" })
                return true;

            if (element is ButtonBase or TextBox or PasswordBox or ComboBox or Selector or Hyperlink)
                return false;

            element = VisualTreeHelper.GetParent(element);
        }

        return false;
    }
}
