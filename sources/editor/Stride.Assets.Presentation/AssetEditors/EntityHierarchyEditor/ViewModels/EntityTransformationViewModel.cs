// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net) and Silicon Studio Corp. (https://www.siliconstudio.co.jp)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using Stride.Core;
using Stride.Core.Annotations;
using Stride.Core.Mathematics;
using Stride.Assets.Presentation.AssetEditors.EntityHierarchyEditor.Services;
using Stride.Assets.Presentation.AssetEditors.GameEditor;
using Stride.Assets.Presentation.AssetEditors.GameEditor.Services;
using Stride.Assets.Presentation.AssetEditors.GameEditor.ViewModels;
using Stride.Assets.Presentation.AssetEditors.SceneEditor.SceneInteractions;
using Stride.Assets.Presentation.SceneEditor;
using Stride.Core.Presentation.ViewModels;

namespace Stride.Assets.Presentation.AssetEditors.EntityHierarchyEditor.ViewModels
{
    public class EntityTransformationViewModel : DispatcherViewModel, IDestroyable
    {
        private readonly IEditorGameController controller;
        private IReadOnlyCollection<TransformationSpace> availableSpaces;
        private readonly ISceneEditorInteractionService sceneInteractionService;
        private Transformation previousStandardTransformation;
        private SceneEditorInteractionSession lastInteraction;

        public EntityTransformationViewModel([NotNull] IViewModelServiceProvider serviceProvider, [NotNull] IEditorGameController controller,
            [CanBeNull] ISceneEditorInteractionService sceneInteractionService)
            : base(serviceProvider)
        {
            this.controller = controller;
            previousStandardTransformation = Transformation.Translation;
            TranslationSnap = new SnapInfoViewModel(ServiceProvider, controller, Transformation.Translation);
            RotationSnap = new SnapInfoViewModel(ServiceProvider, controller, Transformation.Rotation);
            ScaleSnap = new SnapInfoViewModel(ServiceProvider, controller, Transformation.Scale);
            this.sceneInteractionService = sceneInteractionService;
            lastInteraction = sceneInteractionService?.LastInteraction;
            var service = Service;
            if (service != null)
            {
                previousStandardTransformation = service.ActiveTransformation == Transformation.CustomTool ? Transformation.Translation : service.ActiveTransformation;
            }

            if (sceneInteractionService != null && service != null)
            {
                sceneInteractionService.ActiveInteractionChanged += OnActiveInteractionChanged;
                SyncActiveTransformationWithCustomTool();
            }
        }

        public Transformation ActiveTransformation
        {
            get => Service.ActiveTransformation;
            set
            {
                if (value == Transformation.CustomTool && sceneInteractionService?.ActiveInteraction == null)
                {
                    if (lastInteraction != null && (sceneInteractionService == null || sceneInteractionService.RestoreLastInteraction()))
                        return;

                    lastInteraction = sceneInteractionService?.LastInteraction;
                    return;
                }

                SetValue(Service.ActiveTransformation != value, () =>
                {
                    Service.ActiveTransformation = value;
                    if (value != Transformation.CustomTool)
                    {
                        previousStandardTransformation = value;
                        sceneInteractionService?.DeactivateActiveInteraction();
                    }

                    UpdateSpace();
                });
            }
        }

        public TransformationSpace Space { get { return Service.TransformationSpace; } set { SetValue(Service.TransformationSpace != value, () => Service.TransformationSpace = value); } }

        public IReadOnlyCollection<TransformationSpace> AvailableSpaces { get { return availableSpaces; } private set { SetValue(ref availableSpaces, value); } }

        public SnapInfoViewModel TranslationSnap { get; }

        public SnapInfoViewModel RotationSnap { get; }

        public SnapInfoViewModel ScaleSnap { get; }

        public double GizmoSize { get { return Service.GizmoSize; } set { SetValue(Math.Abs(Service.GizmoSize - (float)value) > MathUtil.ZeroTolerance, () => Service.GizmoSize = value); } }

        private IEditorGameEntityTransformViewModelService Service => controller.GetService<IEditorGameEntityTransformViewModelService>();

        public void LoadSettings([NotNull] SceneSettingsData sceneSettings)
        {
            TranslationSnap.Value = sceneSettings.TranslationSnapValue;
            TranslationSnap.IsActive = sceneSettings.TranslationSnapActive;
            RotationSnap.Value = sceneSettings.RotationSnapValue;
            RotationSnap.IsActive = sceneSettings.RotationSnapActive;
            ScaleSnap.Value = sceneSettings.ScaleSnapValue;
            ScaleSnap.IsActive = sceneSettings.ScaleSnapActive;
            GizmoSize = sceneSettings.TransformationGizmoSize;
            SyncActiveTransformationWithCustomTool();
            UpdateSpace();
        }

        public void SaveSettings([NotNull] SceneSettingsData sceneSettings)
        {
            sceneSettings.TranslationSnapValue = TranslationSnap.Value;
            sceneSettings.TranslationSnapActive = TranslationSnap.IsActive;
            sceneSettings.RotationSnapValue = RotationSnap.Value;
            sceneSettings.RotationSnapActive = RotationSnap.IsActive;
            sceneSettings.ScaleSnapValue = ScaleSnap.Value;
            sceneSettings.ScaleSnapActive = ScaleSnap.IsActive;
            sceneSettings.TransformationGizmoSize = GizmoSize;
        }

        private void UpdateSpace()
        {
            var spaces = new List<TransformationSpace>();
            if (ActiveTransformation == Transformation.CustomTool)
            {
                AvailableSpaces = spaces;
                return;
            }

            if (ActiveTransformation == Transformation.Scale)
            {
                spaces.Add(TransformationSpace.ObjectSpace);
            }
            else
            {
                spaces.Add(TransformationSpace.WorldSpace);
                spaces.Add(TransformationSpace.ObjectSpace);
                spaces.Add(TransformationSpace.ViewSpace);
            }

            AvailableSpaces = spaces;

            if (!spaces.Contains(Space))
            {
                Space = spaces.First();
            }
        }

        private void OnActiveInteractionChanged(object sender, SceneEditorInteractionChangedEventArgs e)
        {
            lastInteraction = e.CurrentInteraction ?? e.PreviousInteraction ?? lastInteraction;
            SyncActiveTransformationWithCustomTool();
        }

        internal void SyncActiveTransformationWithCustomTool()
        {
            Dispatcher.Invoke(() =>
            {
                if (sceneInteractionService?.ActiveInteraction != null)
                {
                    if (ActiveTransformation == Transformation.CustomTool)
                        return;

                    previousStandardTransformation = ActiveTransformation;
                    ActiveTransformation = Transformation.CustomTool;
                    return;
                }

                if (ActiveTransformation != Transformation.CustomTool)
                    return;

                ActiveTransformation = previousStandardTransformation;
            });
        }

        public override void Destroy()
        {
            base.Destroy();
            sceneInteractionService?.ActiveInteractionChanged -= OnActiveInteractionChanged;
        }
    }
}
