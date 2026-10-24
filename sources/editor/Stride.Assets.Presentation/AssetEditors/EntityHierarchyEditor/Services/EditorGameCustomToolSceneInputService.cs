// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Threading.Tasks;
using Stride.Core.Annotations;
using Stride.Core.Mathematics;
using Stride.Assets.Presentation.AssetEditors.GameEditor.Game;
using Stride.Assets.Presentation.AssetEditors.EntityHierarchyEditor.ViewModels;
using Stride.Assets.Presentation.AssetEditors.Gizmos;
using Stride.Assets.Presentation.AssetEditors.SceneEditor.SceneInteractions;
using Stride.Editor.EditorGame.Game;
using Stride.Input;

namespace Stride.Assets.Presentation.AssetEditors.EntityHierarchyEditor.Game
{
    public class EditorGameCustomToolSceneInputService : EditorGameServiceBase
    {
        private readonly EntityHierarchyEditorViewModel editor;
        private EntityHierarchyEditorGame game;
        private Vector2 leftMouseMoveAccumulator;
        private Vector2 middleMouseMoveAccumulator;
        private Vector2 rightMouseMoveAccumulator;

        public EditorGameCustomToolSceneInputService([NotNull] EntityHierarchyEditorViewModel editor)
        {
            this.editor = editor ?? throw new ArgumentNullException(nameof(editor));
        }

        protected override Task<bool> Initialize(EditorServiceGame editorGame)
        {
            game = (EntityHierarchyEditorGame)editorGame;
            game.Script.AddTask(Execute);
            return Task.FromResult(true);
        }

        private async Task Execute()
        {
            while (game.IsRunning)
            {
                await game.Script.NextFrame();

                if (!IsActive || !editor.IsCustomToolModeActive())
                    continue;

                var screenSize = new Vector2(game.GraphicsDevice.Presenter.BackBuffer.Width, game.GraphicsDevice.Presenter.BackBuffer.Height);
                var mouseDelta = new Vector2(Math.Abs(game.Input.MouseDelta.X * screenSize.X), Math.Abs(game.Input.MouseDelta.Y * screenSize.Y));

                UpdateMouseAccumulator(MouseButton.Left, ref leftMouseMoveAccumulator, mouseDelta);
                UpdateMouseAccumulator(MouseButton.Middle, ref middleMouseMoveAccumulator, mouseDelta);
                UpdateMouseAccumulator(MouseButton.Right, ref rightMouseMoveAccumulator, mouseDelta);

                TryHandleMouseClick(MouseButton.Left, SceneEditorMouseButton.Left, leftMouseMoveAccumulator);
                TryHandleMouseClick(MouseButton.Middle, SceneEditorMouseButton.Middle, middleMouseMoveAccumulator);
                TryHandleMouseClick(MouseButton.Right, SceneEditorMouseButton.Right, rightMouseMoveAccumulator);
            }
        }

        private void UpdateMouseAccumulator(MouseButton button, ref Vector2 accumulator, Vector2 mouseDelta)
        {
            if (game.Input.IsMouseButtonPressed(button))
                accumulator = Vector2.Zero;

            if (game.Input.IsMouseButtonDown(button))
                accumulator += mouseDelta;
        }

        private void TryHandleMouseClick(MouseButton button, SceneEditorMouseButton customButton, Vector2 mouseMoveAccumulator)
        {
            if (!game.Input.IsMouseButtonReleased(button))
                return;

            if (mouseMoveAccumulator.Length() >= TransformationGizmo.TransformationStartPixelThreshold)
                return;

            if (customButton == SceneEditorMouseButton.Left && editor.IsCustomToolDefaultSceneInputEnabled(SceneEditorDefaultInput.LeftClickSelection))
                return;

            if (customButton == SceneEditorMouseButton.Middle && editor.IsCustomToolDefaultSceneInputEnabled(SceneEditorDefaultInput.MiddleClickCameraControl))
                return;

            if (customButton == SceneEditorMouseButton.Right && editor.IsCustomToolDefaultSceneInputEnabled(SceneEditorDefaultInput.RightClickCameraControl))
                return;

            editor.HandleCustomSceneMouseClick(customButton);
        }
    }
}
