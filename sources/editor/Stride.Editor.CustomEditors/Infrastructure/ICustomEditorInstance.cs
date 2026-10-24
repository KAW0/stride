// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using Stride.Editor.CustomEditors.ViewModel;
using Stride.Editor.CustomEditors.Models;

namespace Stride.Editor.CustomEditors.Infrastructure;

public interface ICustomEditorInstance : IDisposable
{
    object DockHandle { get; }

    bool IsDockVisible { get; }

    bool IsAttachedToLayout();

    void ShowDock();

    void HideDock();

    void ActivateDock();

    void ClearContent();

    event EventHandler<EventArgs>? DockClosed;

    event EventHandler<EventArgs>? DockIsVisibleChanged;

    CustomEditorViewModel ViewModel { get; }

    CustomEditorDescriptor Descriptor { get; }
}
