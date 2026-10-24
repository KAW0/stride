// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using System.Collections.Generic;
using Stride.Core.Presentation.Commands;

namespace Stride.Assets.Presentation.SceneEditor.Widgets.ViewModels;

public interface ISceneWidgetWorkspaceViewModel
{
    bool IsAvailable { get; }

    IEnumerable<ISceneWidgetTypeViewModel> AvailableWidgets { get; }

    IEnumerable<ISceneWidgetProfileViewModel> AvailableProfiles { get; }

    ISceneWidgetProfileViewModel SelectedProfile { get; set; }

    IEnumerable<ISceneWidgetInstanceViewModel> TopWidgets { get; }

    IEnumerable<ISceneWidgetInstanceViewModel> LeftWidgets { get; }

    IEnumerable<ISceneWidgetInstanceViewModel> RightWidgets { get; }

    IEnumerable<ISceneWidgetInstanceViewModel> BottomWidgets { get; }

    bool HasTopWidgets { get; }

    bool HasLeftWidgets { get; }

    bool HasRightWidgets { get; }

    bool HasBottomWidgets { get; }

    bool ShowTopWidgets { get; }

    bool ShowLeftWidgets { get; }

    bool ShowRightWidgets { get; }

    bool ShowBottomWidgets { get; }

    ICommandBase AddWidgetCommand { get; }

    ICommandBase CreateLocalProfileCommand { get; }

    ICommandBase CreateProjectProfileCommand { get; }

    ICommandBase SaveSelectedProfileCommand { get; }

    ICommandBase SetSelectedProfileAsDefaultCommand { get; }

    ICommandBase RenameSelectedProfileCommand { get; }

    ICommandBase SelectProfileWithPromptCommand { get; }

    void SetWidgetDragInProgress(bool isDragging);

    void LoadSettings(SceneSettingsData settings);

    void SaveSettings(SceneSettingsData settings);
}
