// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Stride.Editor.CustomEditors.Models;

namespace Stride.Editor.CustomEditors.Services;

public interface ICustomEditorsService : IReloadableDescriptorService<CustomEditorDescriptor>, IDisposable
{
    IReadOnlyDictionary<string, string> AvailableEditorTypes { get; }

    IReadOnlyCollection<string> OpenEditorIds { get; }

    IReadOnlyCollection<CustomEditorInstanceInfo> OpenInstances { get; }

    event EventHandler<CustomEditorDescriptorEventArgs>? EditorDescriptorAdded;

    event EventHandler<CustomEditorDescriptorEventArgs>? EditorDescriptorUpdated;

    event EventHandler<CustomEditorDescriptorEventArgs>? EditorDescriptorRemoved;

    event EventHandler<CustomEditorVisibilityChangedEventArgs>? EditorVisibilityChanged;

    event EventHandler<CustomEditorInstanceEventArgs>? EditorOpened;

    event EventHandler<CustomEditorInstanceEventArgs>? EditorClosed;

    Task<ICustomEditorHandle> OpenAsync(string editorId, string instanceId = "default", CancellationToken cancellationToken = default);

    Task<ICustomEditorHandle?> SetVisibleAsync(string editorId, bool isVisible, string instanceId = "default", CancellationToken cancellationToken = default);

    Task<ICustomEditorHandle?> ToggleAsync(string editorId, string instanceId = "default", CancellationToken cancellationToken = default);

    bool IsEditorVisible(string editorId);

    bool IsEditorVisible(string editorId, string instanceId);

    bool TryGetInstance(string editorId, string instanceId, [NotNullWhen(true)] out ICustomEditorHandle? handle);

    bool TryActivate(string contentId);

    Task<bool> CloseAsync(string editorId, string instanceId = "default", CancellationToken cancellationToken = default);
}
