// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using Stride.Core;
using Stride.Core.Assets.Editor.View.Behaviors;
using Stride.Core.Assets.Editor.ViewModel;
using Stride.Core.Presentation.ViewModels;
using Stride.Editor.CustomEditors.Models;

namespace Stride.Editor.CustomEditors.ViewModel;

public sealed class MainToolbarWidgetInstanceViewModel : DispatcherViewModel, IInsertChildViewModel
{
    private readonly Action<MainToolbarWidgetInstanceViewModel, int> move;

    public MainToolbarWidgetDescriptor Descriptor { get; }

    public MainToolbarWidgetViewModel ViewModel { get; }

    public object View { get; }

    public string WidgetId => ViewModel.WidgetId;

    public string DisplayName => ViewModel.DisplayName;

    public int Order { get; internal set; }

    public bool IsVisible => ViewModel.IsVisible;

    internal MainToolbarWidgetInstanceViewModel(
        IViewModelServiceProvider serviceProvider,
        MainToolbarWidgetDescriptor descriptor,
        MainToolbarWidgetViewModel viewModel,
        object view,
        Action<MainToolbarWidgetInstanceViewModel, int> move)
        : base(serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(descriptor);
        ArgumentNullException.ThrowIfNull(viewModel);
        ArgumentNullException.ThrowIfNull(view);
        ArgumentNullException.ThrowIfNull(move);
        Descriptor = descriptor;
        ViewModel = viewModel;
        View = view;
        this.move = move;
        ViewModel.PropertyChanged += OnViewModelPropertyChanged;
    }

    public override void Destroy()
    {
        ViewModel.PropertyChanged -= OnViewModelPropertyChanged;
        if (ViewModel is IDestroyable destroyableViewModel)
            destroyableViewModel.Destroy();
        if (View is IDestroyable destroyableView)
            destroyableView.Destroy();
        base.Destroy();
    }

    private void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (string.Equals(e.PropertyName, nameof(MainToolbarWidgetViewModel.IsVisible), StringComparison.Ordinal))
            OnPropertyChanged(nameof(IsVisible));
    }

    bool IInsertChildViewModel.CanInsertChildren(IReadOnlyCollection<object> children, InsertPosition position, AddChildModifiers modifiers, out string message)
    {
        message = DragDropBehavior.InvalidDropAreaMessage;
        var child = children.OfType<MainToolbarWidgetInstanceViewModel>().SingleOrDefault();
        if (child == null)
            return false;

        message = position == InsertPosition.Before ? $"Insert before {DisplayName}" : $"Insert after {DisplayName}";
        return true;
    }

    void IInsertChildViewModel.InsertChildren(IReadOnlyCollection<object> children, InsertPosition position, AddChildModifiers modifiers)
    {
        var child = children.OfType<MainToolbarWidgetInstanceViewModel>().SingleOrDefault();
        if (child == null || child == this)
            return;

        move(child, Order + (position == InsertPosition.After ? 1 : 0));
    }
}
