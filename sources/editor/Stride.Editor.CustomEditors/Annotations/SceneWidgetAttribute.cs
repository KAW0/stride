// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using Stride.Assets.Presentation.SceneEditor.Widgets;

namespace Stride.Editor.CustomEditors.Annotations;

[AttributeUsage(AttributeTargets.Class)]
public sealed class SceneWidgetAttribute : Attribute
{
    public Type ViewModelType { get; }
    public string WidgetName { get; }
    public SceneWidgetPlacement DefaultPlacement { get; set; } = SceneWidgetPlacement.Top;
    public bool AllowMultiple { get; set; } = true;
    public SceneWidgetAttribute(Type viewModelType, string widgetName)
    {
        ViewModelType = viewModelType;
        WidgetName = widgetName;
    }
}
