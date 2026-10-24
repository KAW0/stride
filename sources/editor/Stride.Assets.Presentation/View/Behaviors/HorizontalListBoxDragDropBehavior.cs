// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Stride.Core.Annotations;
using Stride.Core.Assets.Editor.View.Behaviors;
using Stride.Core.Assets.Editor.ViewModel;
using Stride.Core.Extensions;

namespace Stride.Assets.Presentation.View.Behaviors;

public class HorizontalListBoxDragDropBehavior : DragDropBehavior<ListBox, ListBoxItem>
{
    protected override IEnumerable<object> GetItemsToDrag(ListBoxItem container)
    {
        if (container != null)
        {
            var sourceItem = container.DataContext;
            return AssociatedObject.SelectedItems.Contains(sourceItem) ? AssociatedObject.SelectedItems.Cast<object>() : sourceItem.ToEnumerable<object>();
        }

        return Enumerable.Empty<object>();
    }

    protected override IInsertChildViewModel GetInsertTargetItem([NotNull] ListBoxItem container, Point mousePosition, out InsertPosition insertPosition)
    {
        insertPosition = InsertPosition.Before;

        if (mousePosition.X >= 0 && mousePosition.X <= InsertThreshold)
        {
            insertPosition = InsertPosition.Before;
            return container.DataContext as IInsertChildViewModel;
        }

        if (mousePosition.X >= container.ActualWidth - InsertThreshold && mousePosition.X <= container.ActualWidth)
        {
            insertPosition = InsertPosition.After;
            return container.DataContext as IInsertChildViewModel;
        }

        return null;
    }
}
