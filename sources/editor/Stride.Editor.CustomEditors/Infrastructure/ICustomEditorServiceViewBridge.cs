// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using Stride.Editor.CustomEditors.ViewModel;
using Stride.Editor.CustomEditors.Models;

namespace Stride.Editor.CustomEditors.Infrastructure;

public interface ICustomEditorServiceViewBridge
{
    Type ViewType { get; }

    bool CheckAccess();

    void Invoke(Action action);

    T Invoke<T>(Func<T> action);

    object GetOrCreateDockHandle(string editorId);

    bool IsDockHandleAttached(object dockHandle);

    void CloseDockHandle(object dockHandle);

    ICustomEditorInstance CreateEditorInstance(object dockHandle, CustomEditorDescriptor descriptor, CustomEditorViewModel viewModel);
}
