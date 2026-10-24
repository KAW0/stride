// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Threading;
using Stride.Core.Diagnostics;

namespace Stride.Editor.CustomEditors.Wpf;

/// <summary>
/// This class filters out some exceptions caused by bugs in AvalonDock that would otherwise crash the application.
/// </summary>
internal sealed class AvalonDockExceptionFilter : IDisposable
{
    private static readonly Logger Logger = GlobalLogger.GetLogger(nameof(AvalonDockExceptionFilter));
    private readonly Dispatcher dispatcher;

    public AvalonDockExceptionFilter([NotNull] Dispatcher dispatcher)
    {
        this.dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
        this.dispatcher.UnhandledException += OnUnhandledException;
    }

    public void Dispose()
    {
        dispatcher.UnhandledException -= OnUnhandledException;
    }

    private static void OnUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        if (IsFloatingWindowActivationException(e.Exception))
        {
            e.Handled = true;
            return;
        }

        if (IsReadOnlyBindingException(e.Exception))
        {
            Logger.Error("A WPF binding targeted a read-only property. The exception was handled to keep Game Studio running.", e.Exception);
            e.Handled = true;
        }
    }

    private static bool IsFloatingWindowActivationException(Exception exception)
    {
        if (exception is not InvalidOperationException)
            return false;

        var message = exception.Message;
        var stackTrace = exception.StackTrace;
        if (string.IsNullOrEmpty(stackTrace) && string.IsNullOrEmpty(message))
            return false;

        var isActivationPath = !string.IsNullOrEmpty(stackTrace)
                               && stackTrace.Contains("AvalonDock.Controls.LayoutFloatingWindowControl.OnActivated", StringComparison.Ordinal);

        if (!isActivationPath)
            return false;

        return (!string.IsNullOrEmpty(stackTrace)
                && (stackTrace.Contains("System.Windows.Media.Visual.PointToScreen", StringComparison.Ordinal)
                    || stackTrace.Contains("AvalonDock.Controls.TransformExtensions.PointToScreenDPI", StringComparison.Ordinal)))
               || (!string.IsNullOrEmpty(message)
                   && message.Contains("PresentationSource", StringComparison.Ordinal));
    }

    private static bool IsReadOnlyBindingException(Exception exception)
    {
        if (exception is not InvalidOperationException)
            return false;

        var stackTrace = exception.StackTrace;
        if (string.IsNullOrEmpty(stackTrace))
            return false;

        return stackTrace.Contains("MS.Internal.Data.PropertyPathWorker.CheckReadOnly", StringComparison.Ordinal)
               && stackTrace.Contains("System.Windows.Data.BindingExpression.AttachToContext", StringComparison.Ordinal);
    }
}
