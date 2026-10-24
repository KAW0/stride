// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using Stride.Core;
using Stride.Core.Assets;

namespace Stride.Assets.Presentation.SceneEditor.Widgets;

[DataContract("SceneWidgetProfileAsset")]
[AssetDescription(FileExtension)]
[Display("Scene Widget Profile")]
public sealed class SceneWidgetProfileAsset : Asset
{
    public const string FileExtension = ".sdwidgetprofile";

    [DataMember(10)]
    public SceneWidgetProfileData Profile { get; set; } = new();
}
