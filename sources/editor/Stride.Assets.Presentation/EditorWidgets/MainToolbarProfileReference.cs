// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using Stride.Core;

namespace Stride.Assets.Presentation.EditorWidgets;

[DataContract("EditorWidgetProfileSource")]
public enum EditorWidgetProfileSource
{
    None,
    Local,
    Project,
}

[DataContract("MainToolbarProfileReference")]
public sealed class MainToolbarProfileReference
{
    [DataMember(10)]
    public EditorWidgetProfileSource Source { get; init; }

    [DataMember(20)]
    public string ProfileId { get; init; } = string.Empty;

    public static MainToolbarProfileReference None() => new();
}
