// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections.Generic;
using Stride.Assets.Presentation.AssetEditors.EntityHierarchyEditor.ViewModels;
using Stride.Assets.Presentation.AssetEditors.SceneEditor.SceneInteractions;
using Stride.Assets.Presentation.SceneEditor.Widgets;
using Stride.Core.Assets.Editor.ViewModel;
using Stride.Core.Presentation.ViewModels;
using Stride.Editor.CustomEditors;
using Stride.Editor.CustomEditors.CustomTools;
using Stride.Editor.CustomEditors.SceneWidgets;

namespace Stride.Editor.CustomEditors.ViewModel;

public abstract class SceneWidgetViewModel : StrideEditorExtensionViewModel
{
    private SceneWidgetViewModelContext? context;
    private SceneWidgetPlacement currentPlacement;
    public abstract string DisplayName { get; set; }

    public virtual string WidgetId => GetType().FullName ?? GetType().Name;

    protected override string ToolOwnerId => WidgetId;

    public SceneWidgetViewModelContext? Context => context;

    public bool IsInitialized => context != null;

    public SceneWidgetPlacement CurrentPlacement
    {
        get => currentPlacement;
        private set => SetValue(ref currentPlacement, value);
    }

    public bool IsVerticalLayout => CurrentPlacement is SceneWidgetPlacement.Left or SceneWidgetPlacement.Right;
    protected EntityHierarchyEditorViewModel Editor => Context!.Editor;

    protected SceneWidgetViewModel(IViewModelServiceProvider serviceProvider)
        : base(serviceProvider)
    {
    }

    public void Initialize(SceneWidgetViewModelContext widgetContext)
    {
        ArgumentNullException.ThrowIfNull(widgetContext);
        context = widgetContext;
        OnInitialized();
    }

    internal void UpdatePlacement(SceneWidgetPlacement placement)
    {
        if (CurrentPlacement == placement)
            return;

        CurrentPlacement = placement;
        OnPropertyChanged(nameof(IsVerticalLayout));
    }

    protected override void ConfigureDefaultTool(SceneToolDefinition tool)
    {
        base.ConfigureDefaultTool(tool);

        if (!string.IsNullOrWhiteSpace(Context?.SceneViewId))
        {
            tool.Hierarchy = SceneToolHierarchy.Scene;
            tool.Scope = SceneToolScope.ToolDefined;
            tool.IsEnabledForView = sceneToolContext => string.Equals(sceneToolContext.ViewId, Context.SceneViewId, StringComparison.OrdinalIgnoreCase);
        }
    }

    protected virtual void OnInitialized()
    {
    }

    public virtual IReadOnlyList<SceneWidgetMenuItem> GetMenuItems()
    {
        return Array.Empty<SceneWidgetMenuItem>();
    }
}
