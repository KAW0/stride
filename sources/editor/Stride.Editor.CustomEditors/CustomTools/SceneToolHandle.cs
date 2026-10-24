// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Diagnostics.CodeAnalysis;

namespace Stride.Editor.CustomEditors.CustomTools;

public sealed class SceneToolHandle
{
    public string OwnerId { get; }

    public SceneToolDefinition Definition { get; }

    internal SceneToolHandle(string ownerId, SceneToolDefinition definition)
    {
        if (string.IsNullOrWhiteSpace(ownerId))
            throw new ArgumentException("The owner id cannot be null or whitespace.", nameof(ownerId));

        OwnerId = ownerId;
        ArgumentNullException.ThrowIfNull(definition);
        Definition = definition;
    }
}
