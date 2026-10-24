// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using Stride.Assets.Presentation.EditorWidgets;
using Stride.Assets.Presentation.SceneEditor.Widgets.ViewModels;
using Stride.Core.Assets.Editor.ViewModel;

namespace Stride.Editor.CustomEditors.ViewModel;

public sealed class MainToolbarProfileViewModel : ISceneWidgetProfileViewModel
{
    public EditorWidgetProfileSource Source { get; }
    public string ProfileId { get; }
    public string DisplayName { get; }
    public MainToolbarProfileData Profile { get; }
    public AssetViewModel? Asset { get; }

    public MainToolbarProfileViewModel(EditorWidgetProfileSource source, string profileId, string displayName, MainToolbarProfileData profile, AssetViewModel? asset = null)
    {
        Source = source;
        ProfileId = profileId;
        DisplayName = displayName;
        Profile = profile;
        Asset = asset;
    }

    public string FullDisplayName => Source switch
    {
        EditorWidgetProfileSource.Local => $"Local: {DisplayName}",
        EditorWidgetProfileSource.Project => $"Project: {DisplayName}",
        _ => DisplayName,
    };

    public MainToolbarProfileReference ToReference() => new()
    {
        Source = Source,
        ProfileId = ProfileId,
    };
}
