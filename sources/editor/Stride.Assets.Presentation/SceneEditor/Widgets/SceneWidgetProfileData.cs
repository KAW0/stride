// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using Stride.Core;
using Stride.Core.Annotations;

namespace Stride.Assets.Presentation.SceneEditor.Widgets;

[DataContract("SceneWidgetProfileData")]
public sealed class SceneWidgetProfileData
{
    [DataMember(10)]
    public string Id { get; set; } = Guid.NewGuid().ToString("N");

    [DataMember(20)]
    public string Name { get; set; } = "Profile";

    [DataMember(30)]
    public int Version { get; set; } = 1;

    [DataMember(40)]
    public List<SceneWidgetLayoutEntry> Widgets { get; set; } = [];

    [NotNull]
    public SceneWidgetProfileData Clone()
    {
        return new SceneWidgetProfileData
        {
            Id = Id,
            Name = Name,
            Version = Version,
            Widgets = Widgets
                .OrderBy(x => x.Order)
                .Select(x => new SceneWidgetLayoutEntry
                {
                    WidgetId = x.WidgetId,
                    Placement = x.Placement,
                    Order = x.Order,
                    Title = x.Title,
                })
                .ToList(),
        };
    }
}
