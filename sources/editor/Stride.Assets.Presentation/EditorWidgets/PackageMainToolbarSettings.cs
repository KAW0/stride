// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using System.Collections.Generic;
using Stride.Core.Assets;
using Stride.Core.Settings;

namespace Stride.Assets.Presentation.EditorWidgets;

public static class PackageMainToolbarSettings
{
    public static SettingsKey<List<MainToolbarProfileData>> LocalProfiles = new(
        "Package/MainToolbarProfiles/LocalProfiles",
        PackageUserSettings.SettingsContainer,
        () => new List<MainToolbarProfileData>());

    public static SettingsKey<MainToolbarProfileReference> DefaultProfile = new(
        "Package/MainToolbarProfiles/DefaultProfile",
        PackageUserSettings.SettingsContainer,
        MainToolbarProfileReference.None);

    public static SettingsKey<List<MainToolbarWidgetLayoutEntry>> CurrentLayout = new(
        "Package/MainToolbarProfiles/CurrentLayout",
        PackageUserSettings.SettingsContainer,
        () => new List<MainToolbarWidgetLayoutEntry>());
}
