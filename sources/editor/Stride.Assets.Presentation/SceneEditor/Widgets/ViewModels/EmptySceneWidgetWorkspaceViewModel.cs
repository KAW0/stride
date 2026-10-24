// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using System.Collections.Generic;
using Stride.Core.Annotations;
using Stride.Core.Presentation.Commands;

namespace Stride.Assets.Presentation.SceneEditor.Widgets.ViewModels;

internal sealed class EmptySceneWidgetWorkspaceViewModel : ISceneWidgetWorkspaceViewModel
{
    public static EmptySceneWidgetWorkspaceViewModel Instance { get; } = new();

    private EmptySceneWidgetWorkspaceViewModel()
    {
    }

    public bool IsAvailable => false;

    [NotNull]
    public IEnumerable<ISceneWidgetTypeViewModel> AvailableWidgets => [];

    [NotNull]
    public IEnumerable<ISceneWidgetProfileViewModel> AvailableProfiles => [];

    [CanBeNull]
    public ISceneWidgetProfileViewModel SelectedProfile
    {
        get => null;
        set
        {
        }
    }

    [NotNull]
    public IEnumerable<ISceneWidgetInstanceViewModel> TopWidgets => [];

    [NotNull]
    public IEnumerable<ISceneWidgetInstanceViewModel> LeftWidgets => [];

    [NotNull]
    public IEnumerable<ISceneWidgetInstanceViewModel> RightWidgets => [];

    [NotNull]
    public IEnumerable<ISceneWidgetInstanceViewModel> BottomWidgets => [];

    public bool HasTopWidgets => false;

    public bool HasLeftWidgets => false;

    public bool HasRightWidgets => false;

    public bool HasBottomWidgets => false;

    public bool ShowTopWidgets => false;

    public bool ShowLeftWidgets => false;

    public bool ShowRightWidgets => false;

    public bool ShowBottomWidgets => false;

    [NotNull]
    public ICommandBase AddWidgetCommand => DisabledCommand.Instance;

    [NotNull]
    public ICommandBase CreateLocalProfileCommand => DisabledCommand.Instance;

    [NotNull]
    public ICommandBase CreateProjectProfileCommand => DisabledCommand.Instance;

    [NotNull]
    public ICommandBase SaveSelectedProfileCommand => DisabledCommand.Instance;

    [NotNull]
    public ICommandBase SetSelectedProfileAsDefaultCommand => DisabledCommand.Instance;

    [NotNull]
    public ICommandBase RenameSelectedProfileCommand => DisabledCommand.Instance;

    [NotNull]
    public ICommandBase SelectProfileWithPromptCommand => DisabledCommand.Instance;

    public void SetWidgetDragInProgress(bool isDragging)
    {
    }

    public void LoadSettings(SceneSettingsData settings)
    {
    }

    public void SaveSettings(SceneSettingsData settings)
    {
    }
}
