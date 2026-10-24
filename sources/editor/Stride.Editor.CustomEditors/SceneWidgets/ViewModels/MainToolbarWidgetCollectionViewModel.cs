// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Stride.Core.Assets.Editor.View.Behaviors;
using Stride.Core.Assets.Editor.ViewModel;
using Stride.Core.Presentation.ViewModels;
using Stride.Editor.CustomEditors.Services;
using Stride.Editor.CustomEditors.ViewModel;

namespace Stride.Editor.CustomEditors.SceneWidgets.ViewModels;

internal sealed class MainToolbarWidgetCollectionViewModel : IAddChildViewModel
{
    private readonly MainToolbarWidgetService service;
    private readonly System.Action changed;

    public ObservableCollection<MainToolbarWidgetInstanceViewModel> Items => service.MutableWidgets;

    public MainToolbarWidgetCollectionViewModel(MainToolbarWidgetService service, System.Action changed)
    {
        this.service = service;
        this.changed = changed;
    }

    public void RefreshOrder()
    {
        for (var index = 0; index < Items.Count; index++)
            Items[index].Order = index;
    }

    bool IAddChildViewModel.CanAddChildren(IReadOnlyCollection<object> children, AddChildModifiers modifiers, out string message)
    {
        message = DragDropBehavior.InvalidDropAreaMessage;
        var child = children.OfType<MainToolbarWidgetInstanceViewModel>().SingleOrDefault();
        if (child == null)
            return false;

        message = "Move to main toolbar";
        return true;
    }

    void IAddChildViewModel.AddChildren(IReadOnlyCollection<object> children, AddChildModifiers modifiers)
    {
        var child = children.OfType<MainToolbarWidgetInstanceViewModel>().SingleOrDefault();
        if (child == null)
            return;

        service.MoveWidget(child, Items.Count);
        RefreshOrder();
        changed();
    }
}
