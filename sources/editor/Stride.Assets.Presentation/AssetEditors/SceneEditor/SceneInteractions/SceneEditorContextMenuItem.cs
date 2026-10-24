// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Windows.Input;
using Stride.Core.Annotations;

namespace Stride.Assets.Presentation.AssetEditors.SceneEditor.SceneInteractions;

public sealed class SceneEditorContextMenuItem
{
    public string Header { get; }

    public ICommand Command { get; }

    public object CommandParameter { get; }

    public bool IsEnabled { get; }

    public SceneEditorContextMenuItem([NotNull] string header, [NotNull] ICommand command, object commandParameter = null, bool isEnabled = true)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(header, nameof(header));
        Header = header;
        Command = command ?? throw new ArgumentNullException(nameof(command));
        CommandParameter = commandParameter;
        IsEnabled = isEnabled;
    }
}
