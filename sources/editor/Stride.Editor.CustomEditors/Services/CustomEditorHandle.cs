// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using Stride.Editor.CustomEditors.Infrastructure;
using Stride.Editor.CustomEditors.ViewModel;

namespace Stride.Editor.CustomEditors.Services;

internal sealed class CustomEditorHandle : ICustomEditorHandle
{
    private readonly CustomEditorsService owner;
    private readonly string key;

    private ICustomEditorInstance Instance => owner.GetRequiredInstance(key);

    public string EditorId => Instance.Descriptor.EditorId;

    public string InstanceId => Instance.ViewModel.InstanceId;

    public CustomEditorViewModel ViewModel => Instance.ViewModel;

    public bool IsVisible => owner.IsEditorVisible(EditorId, InstanceId);

    public CustomEditorHandle(CustomEditorsService owner, string key)
    {
        this.owner = owner;
        this.key = key;
    }

    public void Activate()
    {
        if (!owner.TryActivate(key))
            throw new ObjectDisposedException(nameof(ICustomEditorHandle));
    }

    public Task<bool> CloseAsync(CancellationToken cancellationToken = default) =>
        owner.CloseAsync(EditorId, InstanceId, cancellationToken);

    public async ValueTask DisposeAsync()
    {
        await CloseAsync();
    }
}
