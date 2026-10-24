// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using Stride.Editor.CustomEditors.ViewModel;

namespace Stride.Editor.CustomEditors.Annotations;

[AttributeUsage(AttributeTargets.Class)]
public sealed class MainToolbarWidgetAttribute<TViewModel> : Attribute, IMainToolbarWidgetRegistration
    where TViewModel : MainToolbarWidgetViewModel
{
    public Type ViewModelType => typeof(TViewModel);
    public string WidgetName { get; }

    public bool AllowMultiple { get; set; }
    public string WidgetId { get; }

    public MainToolbarWidgetAttribute(string widgetId, string widgetName)
    {
        if (string.IsNullOrWhiteSpace(widgetId))
            throw new ArgumentException("The widget id cannot be null or whitespace.", nameof(widgetId));

        WidgetId = widgetId;
        WidgetName = widgetName;
    }
}
