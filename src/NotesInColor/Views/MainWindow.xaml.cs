/**
 *                    Notes in Color
 *            A project by Benyamin Bamburac
 * 
 * This project is a modern MIDI file player and visualizer
 * with some MIDI controller support.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Composition.SystemBackdrops;
using WinRT.Interop;
using Windows.UI.ViewManagement;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Threading;
using Windows.Storage.Pickers;
using WinUIEx;
using NotesInColor.ViewModel;
using Microsoft.Extensions.DependencyInjection;
using NotesInColor.Services;

namespace NotesInColor {
    /**
     * Represents the main window of the application.
     */
    public sealed partial class MainWindow : WindowEx {
        public MainWindowViewModel ViewModel { get; }
        private ISettingsManager SettingsManager;

        public Frame MainWindowFrame => this.Frame;

        public MainWindow() {
            this.InitializeComponent();

            this.SystemBackdrop = new MicaBackdrop() { Kind = MicaKind.Base };
            this.ExtendsContentIntoTitleBar = true;
            SetTitleBar(AppTitleBar);

            var manager = WindowManager.Get(this);
            manager.Width = 1200;
            manager.Height = 800;
            manager.MinWidth = 800;
            manager.MinHeight = 500;

            ViewModel = App.Current.Services.GetRequiredService<MainWindowViewModel>();
            SettingsManager = App.Current.Services.GetRequiredService<ISettingsManager>();

            MainWindowFrame.Loaded += (_, _) => UpdateTheme();
        }

        /**
         * Updates the window theme to match settings.
         */
        public void UpdateTheme() {
            if (SettingsManager["theme"] is not string theme)
                return;

            ElementTheme elementTheme = theme switch {
                "light" => ElementTheme.Light,
                "dark" => ElementTheme.Dark,
                "auto" => ElementTheme.Default,
                _ => throw new NotImplementedException("bug")
            };

            TitleBarTheme titleBarTheme = theme switch {
                "light" => TitleBarTheme.Light,
                "dark" => TitleBarTheme.Dark,
                "auto" => TitleBarTheme.UseDefaultAppMode,
                _ => throw new NotImplementedException("bug")
            };
            
            if (MainWindowFrame.XamlRoot?.Content is FrameworkElement frameworkElement) {
                frameworkElement.RequestedTheme = elementTheme;
            }
            MainWindowFrame.RequestedTheme = elementTheme;

            AppWindow.TitleBar.PreferredTheme = titleBarTheme;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e) {
            if (Frame.CanGoBack)
                Frame.GoBack();
        }
    }
}