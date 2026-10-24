// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using Stride.Core;

namespace Stride.Assets.Presentation.EditorWidgets;

[DataContract("MainToolbarProfileData")]
public sealed class MainToolbarProfileData
{
    [DataMember(10)]
    public string Id { get; set; } = Guid.NewGuid().ToString("N");

    [DataMember(20)]
    public string Name { get; set; } = "Profile";

    [DataMember(30)]
    public int Version { get; set; } = 1;

    [DataMember(40)]
    public List<MainToolbarWidgetLayoutEntry> Widgets { get; set; } = [];

    public MainToolbarProfileData Clone() => new()
    {
        Id = Id,
        Name = Name,
        Version = Version,
        Widgets = Widgets.OrderBy(x => x.Order).Select(x => new MainToolbarWidgetLayoutEntry
        {
            WidgetId = x.WidgetId,
            Order = x.Order,
            Title = x.Title,
        }).ToList(),
    };
}
