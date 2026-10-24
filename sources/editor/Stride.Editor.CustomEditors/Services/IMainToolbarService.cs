// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Stride.Editor.CustomEditors.Models;
using Stride.Editor.CustomEditors.ViewModel;

namespace Stride.Editor.CustomEditors.Services;

public interface IMainToolbarService : IDisposable
{
    event EventHandler? WidgetDescriptorsChanged;

    IReadOnlyDictionary<string, MainToolbarWidgetDescriptor> WidgetDescriptors { get; }

    ReadOnlyObservableCollection<MainToolbarWidgetInstanceViewModel> Widgets { get; }

    MainToolbarWidgetWorkspaceViewModel Workspace { get; }
}
