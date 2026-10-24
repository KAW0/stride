// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using System.Collections.Generic;
using System.Threading.Tasks;
using Stride.Assets.Presentation.SceneEditor.Widgets.ViewModels;
using Stride.Editor.CustomEditors.SceneWidgets;
using Stride.Editor.CustomEditors.ViewModel;

namespace Stride.Editor.CustomEditors.Infrastructure;

public interface ISceneWidgetServiceViewBridge : IEditorWidgetServiceViewBridge
{
    Task<string> PromptForProfileNameAsync(string title, string initialName);

    Task<ISceneWidgetProfileViewModel> SelectProfileAsync(IReadOnlyList<ISceneWidgetProfileViewModel> profiles, ISceneWidgetProfileViewModel? selectedProfile);
}
