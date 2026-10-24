// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using Stride.Editor.CustomEditors.ViewModel;

namespace Stride.Editor.CustomEditors.Annotations;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class CustomEditorAttribute<TViewModel> : Attribute, ICustomEditor
    where TViewModel : CustomEditorViewModel
{
    public string EditorId { get; }
    public Type ViewModelType => typeof(TViewModel);
    public string EditorName { get; }

    public CustomEditorAttribute(string editorId, string editorName)
    {
        if (string.IsNullOrWhiteSpace(editorId))
            throw new ArgumentException("An editor id is required.", nameof(editorId));
        if (string.IsNullOrWhiteSpace(editorName))
            throw new ArgumentException("An editor name is required.", nameof(editorName));

        EditorId = editorId;
        EditorName = editorName;
    }
}
