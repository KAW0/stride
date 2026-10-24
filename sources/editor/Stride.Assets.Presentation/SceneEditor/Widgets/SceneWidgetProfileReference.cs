// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using Stride.Core;
using Stride.Core.Annotations;

namespace Stride.Assets.Presentation.SceneEditor.Widgets;

[DataContract("SceneWidgetProfileReference")]
public sealed class SceneWidgetProfileReference
{
    [DataMember(10)]
    public SceneWidgetProfileSource Source { get; init; }

    [DataMember(20)]
    public string ProfileId { get; init; } = string.Empty;

    [NotNull]
    public static SceneWidgetProfileReference None()
    {
        return new SceneWidgetProfileReference
        {
            Source = SceneWidgetProfileSource.None,
            ProfileId = string.Empty,
        };
    }
}
