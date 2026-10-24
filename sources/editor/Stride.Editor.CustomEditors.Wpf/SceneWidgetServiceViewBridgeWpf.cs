// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Stride.Assets.Presentation.SceneEditor.Widgets.ViewModels;
using Stride.Core.Presentation.Controls;
using Stride.Core.Presentation.Services;
using Stride.Editor.CustomEditors.Infrastructure;
using Stride.Editor.CustomEditors.SceneWidgets;
using Stride.Editor.CustomEditors.ViewModel;

namespace Stride.Editor.CustomEditors.Wpf;

internal sealed class SceneWidgetServiceViewBridgeWpf : ISceneWidgetServiceViewBridge
{
    public Type ViewType => typeof(FrameworkElement);
   
    public object CreateView(Type viewType, object viewModel)
    {
        try
        {
            if (Activator.CreateInstance(viewType) is not FrameworkElement view)
                throw new InvalidOperationException($"Unable to create view '{viewType.FullName}'.");

            view.DataContext ??= viewModel;
            return view;
        }
        catch (Exception exception)
        {
            throw new InvalidOperationException($"Unable to create scene widget view '{viewType.FullName}'.", exception);
        }
    }

    public async Task<string?> PromptForProfileNameAsync(string title, string initialName)
    {
        var window = new SceneWidgetProfilePromptWindow(title, initialName);
        await window.ShowModal();
        return window.Result == Stride.Core.Presentation.Services.DialogResult.Ok ? window.ProfileName : null;
    }

    public async Task<ISceneWidgetProfileViewModel?> SelectProfileAsync(IReadOnlyList<ISceneWidgetProfileViewModel> profiles, ISceneWidgetProfileViewModel? selectedProfile)
    {
        if (profiles == null || profiles.Count == 0)
            return null;

        var window = new SceneWidgetProfileSelectionWindow(profiles, selectedProfile);
        await window.ShowModal();
        return window.Result == Stride.Core.Presentation.Services.DialogResult.Ok ? window.SelectedProfile : null;
    }

    private sealed class SceneWidgetProfilePromptWindow : ModalWindow
    {
        private readonly Stride.Core.Presentation.Controls.TextBox profileNameTextBox;

        public SceneWidgetProfilePromptWindow(string title, string initialName)
        {
            ApplyEditorTheme();
            Title = title;
            Width = 420;
            SizeToContent = SizeToContent.Height;
            ResizeMode = ResizeMode.NoResize;

            var root = new DockPanel();

            var form = new StackPanel { Margin = new Thickness(20) };
            form.Children.Add(new TextBlock { Text = "Profile name:", Margin = new Thickness(0, 0, 0, 6) });
            profileNameTextBox = new Stride.Core.Presentation.Controls.TextBox { Text = initialName ?? string.Empty };
            form.Children.Add(profileNameTextBox);
            DockPanel.SetDock(form, Dock.Top);
            root.Children.Add(form);

            var buttons = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(20),
            };

            var okButton = new Button
            {
                Content = "OK",
                IsDefault = true,
                Padding = new Thickness(20, 6, 20, 6),
                Margin = new Thickness(0, 0, 10, 0),
            };
            okButton.Click += (_, _) =>
            {
                if (string.IsNullOrWhiteSpace(profileNameTextBox.Text))
                    return;

                Result = Stride.Core.Presentation.Services.DialogResult.Ok;
                Close();
            };
            buttons.Children.Add(okButton);

            var cancelButton = new Button
            {
                Content = "Cancel",
                IsCancel = true,
                Padding = new Thickness(20, 6, 20, 6),
            };
            cancelButton.Click += (_, _) =>
            {
                Result = Stride.Core.Presentation.Services.DialogResult.Cancel;
                Close();
            };
            buttons.Children.Add(cancelButton);

            DockPanel.SetDock(buttons, Dock.Bottom);
            root.Children.Add(buttons);
            Content = root;

            Loaded += (_, _) =>
            {
                profileNameTextBox.Focus();
                profileNameTextBox.SelectAll();
            };
        }

        public string ProfileName => profileNameTextBox.Text?.Trim();

        private void ApplyEditorTheme()
        {
            SetResourceReference(StyleProperty, "WindowChromeStyle");
            Resources.MergedDictionaries.Add((ResourceDictionary)Application.LoadComponent(new Uri("/Stride.Core.Assets.Editor;component/View/CommonResources.xaml", UriKind.RelativeOrAbsolute)));
        }
    }

    private sealed class SceneWidgetProfileSelectionWindow : ModalWindow
    {
        private readonly ListBox profilesListBox;

        public SceneWidgetProfileSelectionWindow(IReadOnlyList<ISceneWidgetProfileViewModel> profiles, ISceneWidgetProfileViewModel? selectedProfile)
        {
            ApplyEditorTheme();
            Title = "Select Widget Profile";
            Width = 420;
            MaxHeight = 480;
            SizeToContent = SizeToContent.Height;
            ResizeMode = ResizeMode.NoResize;

            var root = new DockPanel();

            var form = new StackPanel { Margin = new Thickness(20) };
            form.Children.Add(new TextBlock { Text = "Available profiles:", Margin = new Thickness(0, 0, 0, 6) });
            profilesListBox = new ListBox
            {
                ItemsSource = profiles,
                DisplayMemberPath = nameof(ISceneWidgetProfileViewModel.FullDisplayName),
                SelectedItem = selectedProfile,
                MinHeight = 220,
            };
            form.Children.Add(profilesListBox);
            DockPanel.SetDock(form, Dock.Top);
            root.Children.Add(form);

            var buttons = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(20),
            };

            var okButton = new Button
            {
                Content = "OK",
                IsDefault = true,
                Padding = new Thickness(20, 6, 20, 6),
                Margin = new Thickness(0, 0, 10, 0),
            };
            okButton.Click += (_, _) =>
            {
                if (profilesListBox.SelectedItem == null)
                    return;

                Result = Stride.Core.Presentation.Services.DialogResult.Ok;
                Close();
            };
            buttons.Children.Add(okButton);

            var cancelButton = new Button
            {
                Content = "Cancel",
                IsCancel = true,
                Padding = new Thickness(20, 6, 20, 6),
            };
            cancelButton.Click += (_, _) =>
            {
                Result = Stride.Core.Presentation.Services.DialogResult.Cancel;
                Close();
            };
            buttons.Children.Add(cancelButton);

            DockPanel.SetDock(buttons, Dock.Bottom);
            root.Children.Add(buttons);
            Content = root;
        }

        public ISceneWidgetProfileViewModel? SelectedProfile => profilesListBox.SelectedItem as ISceneWidgetProfileViewModel;

        private void ApplyEditorTheme()
        {
            SetResourceReference(StyleProperty, "WindowChromeStyle");
            Resources.MergedDictionaries.Add((ResourceDictionary)Application.LoadComponent(new Uri("/Stride.Core.Assets.Editor;component/View/CommonResources.xaml", UriKind.RelativeOrAbsolute)));
        }
    }
}
