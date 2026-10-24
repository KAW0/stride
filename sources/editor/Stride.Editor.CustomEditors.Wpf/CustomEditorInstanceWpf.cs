// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using System.Diagnostics.CodeAnalysis;
using System.Windows;
using AvalonDock.Layout;
using Stride.Editor.CustomEditors.Infrastructure;
using Stride.Editor.CustomEditors.Models;
using Stride.Editor.CustomEditors.ViewModel;

namespace Stride.Editor.CustomEditors.Wpf;

internal class CustomEditorInstanceWpf : ICustomEditorInstance
{
    public LayoutAnchorable Pane { get; }
    
    public FrameworkElement View { get; }
    
    public object DockHandle => Pane;
    
    public CustomEditorViewModel ViewModel { get; }
    
    public CustomEditorDescriptor Descriptor { get; }

    public bool IsDockVisible => Pane.IsVisible;
    
    public event EventHandler<EventArgs> DockClosed;
    
    public event EventHandler<EventArgs> DockIsVisibleChanged;
    
    public CustomEditorInstanceWpf([NotNull] LayoutAnchorable pane, FrameworkElement view, CustomEditorViewModel viewModel, CustomEditorDescriptor descriptor)
    {
        Pane = pane;
        View = view;
        ViewModel = viewModel;
        Descriptor = descriptor;
        Pane.Closed += OnDockClosed;
        Pane.IsVisibleChanged += OnDockIsVisibleChanged;
    }

    public void Dispose()
    {
        Pane.Closed -= OnDockClosed;
        Pane.IsVisibleChanged -= OnDockIsVisibleChanged;
    }

    public bool IsAttachedToLayout()
    {
        return Pane.Root != null;
    }

    private void OnDockClosed(object sender, EventArgs e)
    {
        DockClosed?.Invoke(this, EventArgs.Empty);
    }

    private void OnDockIsVisibleChanged(object sender, EventArgs e)
    {
        DockIsVisibleChanged?.Invoke(this, EventArgs.Empty);
    }

    public void ShowDock()
    {
        Pane.Show();
    }

    public void HideDock()
    {
        Pane.Hide();
    }

    public void ActivateDock()
    {
        Pane.IsActive = true;
        Pane.Show();
    }

    public void ClearContent()
    {
        Pane.Content = null;
        Pane.Title = null;
    }
}
