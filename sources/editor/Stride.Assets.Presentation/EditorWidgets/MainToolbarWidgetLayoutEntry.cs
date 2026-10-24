// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using Stride.Core;

namespace Stride.Assets.Presentation.EditorWidgets;

[DataContract("MainToolbarWidgetLayoutEntry")]
public sealed class MainToolbarWidgetLayoutEntry
{
    [DataMember(10)]
    public string WidgetId { get; init; } = string.Empty;

    [DataMember(20)]
    public int Order { get; init; }

    [DataMember(30)]
    public string Title { get; init; } = string.Empty;
}
