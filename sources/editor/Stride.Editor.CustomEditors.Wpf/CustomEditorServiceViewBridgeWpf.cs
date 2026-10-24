// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using System.Diagnostics.CodeAnalysis;
using System.Windows;
using AvalonDock.Layout;
using Stride.Editor.CustomEditors.Infrastructure;
using Stride.Editor.CustomEditors.Models;
using Stride.Editor.CustomEditors.ViewModel;

namespace Stride.Editor.CustomEditors.Wpf;

public sealed class CustomEditorServiceViewBridgeWpf : ICustomEditorServiceViewBridge
{
    private readonly Func<bool> checkAccess;
    private readonly Action<Action> invoke;
    private readonly Func<string, object> getOrCreateDockHandle;
    private readonly Func<object, bool> isDockHandleAttached;
    private readonly Action<object> closeDockHandle;

    public CustomEditorServiceViewBridgeWpf(
        [NotNull] Func<bool> checkAccess,
        [NotNull] Action<Action> invoke,
        [NotNull] Func<string, object> getOrCreateDockHandle,
        [NotNull] Func<object, bool> isDockHandleAttached,
        [NotNull] Action<object> closeDockHandle)
    {
        this.checkAccess = checkAccess ?? throw new ArgumentNullException(nameof(checkAccess));
        this.invoke = invoke ?? throw new ArgumentNullException(nameof(invoke));
        this.getOrCreateDockHandle = getOrCreateDockHandle ?? throw new ArgumentNullException(nameof(getOrCreateDockHandle));
        this.isDockHandleAttached = isDockHandleAttached ?? throw new ArgumentNullException(nameof(isDockHandleAttached));
        this.closeDockHandle = closeDockHandle ?? throw new ArgumentNullException(nameof(closeDockHandle));
    }

    [NotNull]
    public Type ViewType => typeof(FrameworkElement);

    public bool CheckAccess() => checkAccess();

    public void Invoke(Action action) => invoke(action);

    public T Invoke<T>(Func<T> action)
    {
        T result = default;
        invoke(() => result = action());
        return result;
    }

    public object GetOrCreateDockHandle(string editorId) => getOrCreateDockHandle(editorId);

    public bool IsDockHandleAttached(object dockHandle) => isDockHandleAttached(dockHandle);

    public void CloseDockHandle(object dockHandle) => closeDockHandle(dockHandle);
    
    public ICustomEditorInstance CreateEditorInstance([NotNull] object dockHandle, [NotNull] CustomEditorDescriptor descriptor, CustomEditorViewModel viewModel)
    {
        if (dockHandle is not LayoutAnchorable pane)
            throw new ArgumentException("The dock handle must be a LayoutAnchorable.", nameof(dockHandle));

        var view = CreateView(descriptor.ViewType, viewModel);
        pane.Title = viewModel.DisplayName ?? descriptor.EditorName;
        pane.Content = view;
        return new CustomEditorInstanceWpf(pane, view, viewModel, descriptor);
    }
   
    public static FrameworkElement CreateView([NotNull] Type viewType, CustomEditorViewModel viewModel)
    {
        try
        {
            var view = Activator.CreateInstance(viewType) as FrameworkElement;
            if (view == null)
                throw new InvalidOperationException($"Unable to create view '{viewType.FullName}'.");

            view.DataContext ??= viewModel;
            return view;
        }
        catch (Exception exception)
        {
            throw new InvalidOperationException($"Unable to create custom editor view '{viewType.FullName}'.", exception);
        }
    }
}
