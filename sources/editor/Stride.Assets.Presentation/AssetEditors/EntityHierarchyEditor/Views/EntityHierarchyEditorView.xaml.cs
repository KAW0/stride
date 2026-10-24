// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net) and Silicon Studio Corp. (https://www.siliconstudio.co.jp)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Stride.Core.Assets.Editor.Services;
using Stride.Core.Assets.Editor.ViewModel;
using Stride.Core.Presentation.Controls;
using Stride.Assets.Presentation.AssetEditors.SceneEditor.SceneInteractions;
using Stride.Assets.Presentation.AssetEditors.EntityHierarchyEditor.ViewModels;
using Stride.Assets.Presentation.SceneEditor.Widgets.ViewModels;

namespace Stride.Assets.Presentation.AssetEditors.EntityHierarchyEditor.Views
{
    /// <summary>
    /// Interaction logic for EntityHierarchyEditorView.xaml
    /// </summary>
    public abstract partial class EntityHierarchyEditorView : IEditorView
    {
        private readonly TaskCompletionSource editorInitializationNotifier = new();

        static EntityHierarchyEditorView()
        {
            FocusOnSelection = new RoutedCommand("FocusOnSelection", typeof(EntityHierarchyEditorView));
        }
        protected EntityHierarchyEditorView()
        {
            InitializeComponent();
            // Ensure we can give the focus to the editor
            Focusable = true;
        }

        public static RoutedCommand FocusOnSelection { get; }

        public Task EditorInitialization => editorInitializationNotifier.Task;

        /// <inheritdoc/>
        public async Task<bool> InitializeEditor(IAssetEditorViewModel editor)
        {
            var hierarchyEditor = (EntityHierarchyEditorViewModel)editor;
            if (!await editor.Initialize())
            {
                editor.Destroy();
                editorInitializationNotifier.SetResult();
                return false;
            }

            SceneView.Content = hierarchyEditor.Controller.EditorHost;
            SceneView.InvalidateVisual();
            hierarchyEditor.NotifySceneViewActivated();

            editorInitializationNotifier.SetResult();
            return true;
        }

        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseDown(e);
            if (DataContext is EntityHierarchyEditorViewModel editor)
                editor.NotifySceneViewActivated();

            // We give the focus to the editor so shortcuts will work
            if (!IsKeyboardFocusWithin && !(e.OriginalSource is GameEngineHost))
            {
                Keyboard.Focus(this);
            }
        }

        protected override void OnGotKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            base.OnGotKeyboardFocus(e);
            if (DataContext is EntityHierarchyEditorViewModel editor)
                editor.NotifySceneViewActivated();
        }

        private void EditorPreviewKeyDown(object sender, KeyEventArgs e)
        {
            // Prevent arrow keys to be involved in arrow navigation. There seems to be no other way - http://bit.ly/1BzM0AC
            switch (e.Key)
            {
                case Key.Up:
                case Key.Down:
                case Key.Left:
                case Key.Right:
                    e.Handled = true;
                    break;
            }
        }

        private void SceneContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            if (DataContext is EntityHierarchyEditorViewModel { ShouldOpenSceneContextMenu: false })
                e.Handled = true;
        }

        private void ScenePreviewMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (IsSceneWidgetPath(e.OriginalSource as DependencyObject))
                return;

            if (DataContext is EntityHierarchyEditorViewModel editor
                && editor.CanCustomToolHandleSceneMouseClick(SceneEditorMouseButton.Right)
                && !editor.IsCustomToolDefaultSceneInputEnabled(SceneEditorDefaultInput.RightClickCameraControl))
                e.Handled = true;
        }

        private bool IsSceneWidgetPath(DependencyObject element)
        {
            while (element != null)
            {
                if (ReferenceEquals(element, LeftSceneWidgetListBox)
                    || ReferenceEquals(element, RightSceneWidgetListBox)
                    || ReferenceEquals(element, BottomSceneWidgetListBox))
                    return true;

                element = VisualTreeHelper.GetParent(element);
            }

            return false;
        }

        private void LoadOrLockButton_OnClick(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            button.CommandParameter = (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control;
        }

        private void SceneWidgetListBox_GiveFeedback(object sender, GiveFeedbackEventArgs e)
        {
            if (DataContext is EntityHierarchyEditorViewModel { SceneWidgets: { } sceneWidgets })
                sceneWidgets.SetWidgetDragInProgress(true);
        }

        private void SceneWidgetListBox_Drop(object sender, DragEventArgs e)
        {
            if (DataContext is EntityHierarchyEditorViewModel { SceneWidgets: { } sceneWidgets })
                sceneWidgets.SetWidgetDragInProgress(false);
        }

        private void SceneWidgetListBox_DragLeave(object sender, DragEventArgs e)
        {
            if (DataContext is EntityHierarchyEditorViewModel { SceneWidgets: { } sceneWidgets })
                sceneWidgets.SetWidgetDragInProgress(false);
        }

        private void SceneWidgetListBox_QueryContinueDrag(object sender, QueryContinueDragEventArgs e)
        {
            if (DataContext is not EntityHierarchyEditorViewModel { SceneWidgets: { } sceneWidgets })
                return;

            if (e.Action == DragAction.Continue)
                return;

            sceneWidgets.SetWidgetDragInProgress(false);
        }

        protected override void OnPreviewMouseUp(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseUp(e);
            if (DataContext is EntityHierarchyEditorViewModel { SceneWidgets: { } sceneWidgets })
                sceneWidgets.SetWidgetDragInProgress(false);
        }
    }
}
