// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using System.Windows.Input;

namespace Stride.Assets.Presentation.SceneEditor.Widgets;

public sealed class SceneWidgetMenuItem
{
    public string Header { get; }

    public ICommand Command { get; }

    public object CommandParameter { get; }

    public bool IsEnabled { get; }

    public SceneWidgetMenuItem(string header, ICommand command, object commandParameter = null, bool isEnabled = true)
    {
        Header = header;
        Command = command;
        CommandParameter = commandParameter;
        IsEnabled = isEnabled;
    }
}
