// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Diagnostics.CodeAnalysis;
using Stride.Core.Assets.Editor.ViewModel;
using Stride.Core.Presentation.ViewModels;
using Stride.Editor.CustomEditors.Models;

namespace Stride.Editor.CustomEditors.ViewModel;

public abstract class MainToolbarWidgetViewModel : StrideEditorExtensionViewModel
{
    public MainToolbarWidgetContext Context { get; }

    public string WidgetId => Context.WidgetId;

    public virtual string DisplayName => Context.DisplayName;

    public virtual bool IsVisible => true;

    protected override string ToolOwnerId => WidgetId;

    protected MainToolbarWidgetViewModel(
        IViewModelServiceProvider serviceProvider,
        MainToolbarWidgetContext context)
        : base(serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(context);
        Context = context;
    }
}
