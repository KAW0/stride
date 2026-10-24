// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Stride.Editor.CustomEditors.ViewModel;

namespace Stride.Editor.CustomEditors.Services;

public interface ICustomEditorHandle : IAsyncDisposable
{
    string EditorId { get; }

    string InstanceId { get; }

    CustomEditorViewModel ViewModel { get; }

    bool IsVisible { get; }

    void Activate();

    Task<bool> CloseAsync(CancellationToken cancellationToken = default);
}
