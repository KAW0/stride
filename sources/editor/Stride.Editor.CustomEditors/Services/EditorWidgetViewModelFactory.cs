// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Linq;
using System.Reflection;
using Stride.Core.Presentation.ViewModels;

namespace Stride.Editor.CustomEditors.Services;

internal static class EditorWidgetViewModelFactory
{
    public static TViewModel Create<TViewModel>(Type viewModelType, IViewModelServiceProvider serviceProvider, object context, string widgetName)
        where TViewModel : class
    {
        ArgumentNullException.ThrowIfNull(viewModelType);
        ArgumentNullException.ThrowIfNull(serviceProvider);
        ArgumentNullException.ThrowIfNull(context);

        try
        {
            var constructors = viewModelType.GetConstructors();
            var contextConstructor = constructors.FirstOrDefault(x =>
            {
                var parameters = x.GetParameters();
                return parameters.Length == 2
                    && parameters[0].ParameterType.IsAssignableFrom(serviceProvider.GetType())
                    && parameters[1].ParameterType.IsInstanceOfType(context);
            });

            var serviceProviderConstructor = constructors.FirstOrDefault(x =>
            {
                var parameters = x.GetParameters();
                return parameters.Length == 1 && parameters[0].ParameterType.IsAssignableFrom(serviceProvider.GetType());
            });

            var instance = contextConstructor?.Invoke([serviceProvider, context])
                ?? serviceProviderConstructor?.Invoke([serviceProvider])
                ?? viewModelType.GetConstructor(Type.EmptyTypes)?.Invoke(null)
                ?? Activator.CreateInstance(viewModelType, serviceProvider);

            if (instance is not TViewModel viewModel)
                throw new InvalidOperationException($"Type '{viewModelType.FullName}' is not a {typeof(TViewModel).FullName}.");

            return viewModel;
        }
        catch (Exception exception)
        {
            throw new InvalidOperationException($"Unable to create editor widget view model '{viewModelType.FullName}' for '{widgetName}'.", exception);
        }
    }
}
