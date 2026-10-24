// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using Stride.Core;
using Stride.Core.Assets;

namespace Stride.Assets.Presentation.EditorWidgets;

[DataContract("MainToolbarProfileAsset")]
[AssetDescription(FileExtension)]
[Display("Main Toolbar Profile")]
public sealed class MainToolbarProfileAsset : Asset
{
    public const string FileExtension = ".sdtoolbarprofile";

    [DataMember(10)]
    public MainToolbarProfileData Profile { get; set; } = new();
}
