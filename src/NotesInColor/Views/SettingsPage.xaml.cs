/**
 *                    Notes in Color
 *            A project by Benyamin Bamburac
 * 
 * This project is a modern MIDI file player and visualizer
 * with some MIDI controller support.
 */

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using NotesInColor.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.Extensions.DependencyInjection;
using NotesInColor.Services;
using Microsoft.Windows.AppLifecycle;
using NotesInColor.Core;

namespace NotesInColor {
    /**
     * This is the settings page.
     */
    public sealed partial class SettingsPage : Page {
        private readonly SettingsPageViewModel ViewModel;
        private readonly ISettingsManager settingsManager;

        public SettingsPage() {
            this.InitializeComponent();

            ViewModel = App.Current.Services.GetRequiredService<SettingsPageViewModel>(); // anti-pattern :(
            settingsManager = App.Current.Services.GetRequiredService<ISettingsManager>(); // more anti-pattern :(

            // obtain app theme
            themeOption.SelectedIndex = settingsManager["theme"] switch {
                "light" => 0,
                "dark" => 1,
                "auto" => 2,
                _ => throw new NotImplementedException("that wasn't supposed to happen")
            };
        }

        // user wants to change app theme
        private void OnThemeSelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (sender is RadioButtons radioButtons) {
                var selected = radioButtons.SelectedIndex;
                if (selected != -1) {
                    string updatedTheme = selected switch {
                        0 => "light",
                        1 => "dark",
                        2 => "auto",
                        _ => throw new NotImplementedException("that wasn't supposed to happen")
                    };

                    if ((string)settingsManager["theme"] != updatedTheme) {
                        settingsManager["theme"] = updatedTheme;

                        // close and reopen application
                        AppInstance.Restart("");
                    }
                }
            }
        }

        private void Restore88KeyLayout(object sender, RoutedEventArgs e) =>
            ViewModel.Configurations.Restore88KeyLayout();

        private void StartWhiteKeyComboBox_DropDownClosed(object sender, object e) =>
            (sender as ComboBox)!.SelectedIndex = ViewModel.StartWhiteKey;

        private void EndWhiteKeyComboBox_DropDownClosed(object sender, object e) =>
            (sender as ComboBox)!.SelectedIndex = ViewModel.EndWhiteKey;
    }
}