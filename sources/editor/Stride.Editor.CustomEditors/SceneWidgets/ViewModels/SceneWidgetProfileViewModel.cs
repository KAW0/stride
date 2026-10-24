// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using Stride.Assets.Presentation.SceneEditor.Widgets;
using Stride.Assets.Presentation.SceneEditor.Widgets.ViewModels;
using Stride.Core.Assets.Editor.ViewModel;

namespace Stride.Editor.CustomEditors.SceneWidgets.ViewModels;

internal sealed class SceneWidgetProfileViewModel : ISceneWidgetProfileViewModel
{
    public SceneWidgetProfileSource Source { get; }

    public string ProfileId { get; }

    public string DisplayName { get; }

    public SceneWidgetProfileData Profile { get; }

    public AssetViewModel? Asset { get; }

    public SceneWidgetProfileViewModel(SceneWidgetProfileSource source, string profileId, string displayName, SceneWidgetProfileData profile, AssetViewModel? asset = null)
    {
        Source = source;
        ProfileId = profileId;
        DisplayName = displayName;
        Profile = profile;
        Asset = asset;
    }

    public string FullDisplayName => Source switch
    {
        SceneWidgetProfileSource.Local => $"Local: {DisplayName}",
        SceneWidgetProfileSource.Project => $"Project: {DisplayName}",
        _ => DisplayName,
    };

    public SceneWidgetProfileReference ToReference()
    {
        return new SceneWidgetProfileReference
        {
            Source = Source,
            ProfileId = ProfileId,
        };
    }
}
