// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Stride.Core.Assets.Editor.ViewModel;
using Stride.Core.Presentation.ViewModels;
using Stride.Editor.CustomEditors;
using Stride.Editor.CustomEditors.Models;

namespace Stride.Editor.CustomEditors.ViewModel;

public abstract class CustomEditorViewModel : StrideEditorExtensionViewModel
{
    public virtual string DisplayName => Context.DisplayName;

    public CustomEditorContext Context { get; }

    public string InstanceId => Context.InstanceId;

    public string EditorId => Context.EditorId;

    protected override string ToolOwnerId => EditorId;

    protected CustomEditorViewModel(IViewModelServiceProvider serviceProvider, CustomEditorContext context)
        : base(serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(context);
        Context = context;
    }

    public virtual Task InitializeAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

    public virtual Task<bool> CanCloseAsync(CancellationToken cancellationToken = default) => Task.FromResult(true);

    public virtual Task CloseAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
}
