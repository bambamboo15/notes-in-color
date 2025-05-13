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
using System.ComponentModel;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml.Media.Animation;
using WinUIEx;

namespace NotesInColor {
    /**
     * This is the settings page.
     */
    public sealed partial class SettingsPage : Page {
        public SettingsPageViewModel ViewModel { get; private set; } = App.Current.Services.GetRequiredService<SettingsPageViewModel>(); // anti-pattern :(
        private ISettingsManager settingsManager = App.Current.Services.GetRequiredService<ISettingsManager>(); // more anti-pattern :(

        public SettingsPage() {
            this.InitializeComponent();

            // obtain app theme
            themeOption.SelectedIndex = settingsManager["theme"] switch {
                "light" => 0,
                "dark" => 1,
                "auto" => 2,
                _ => throw new NotImplementedException("that wasn't supposed to happen")
            };

            // random XAML binding initialization timing issue caused me to do this
            Loaded += OnLoaded;

            DataContext = this;
        }

        private void OnVMPropertyChanged(object? sender, PropertyChangedEventArgs e) {
            if (e.PropertyName == nameof(ViewModel.StartWhiteKey)) {
                startWhiteKeyComboBox.SelectedIndex = ViewModel.StartWhiteKey;
            } else if (e.PropertyName == nameof(ViewModel.EndWhiteKey)) {
                endWhiteKeyComboBox.SelectedIndex = ViewModel.EndWhiteKey;
            }
        }

        private void OnLoaded(object? sender, object e) {
            startWhiteKeyComboBox.ItemsSource = ViewModel.NoteNames;
            endWhiteKeyComboBox.ItemsSource = ViewModel.NoteNames;

            startWhiteKeyComboBox.SelectedIndex = ViewModel.StartWhiteKey;
            endWhiteKeyComboBox.SelectedIndex = ViewModel.EndWhiteKey;

            // manual binding :/
            ViewModel.PropertyChanged += OnVMPropertyChanged;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e) {
            ViewModel.PropertyChanged -= OnVMPropertyChanged;

            ViewModel = null!;
            settingsManager = null!;
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

                    if (settingsManager["theme"] is string theme && theme != updatedTheme) {
                        settingsManager["theme"] = updatedTheme;

                        if (App.Current.Window is MainWindow mainWindow) {
                            mainWindow.UpdateTheme();
                        }
                    }
                }
            }
        }

        private void Restore88KeyLayout(object sender, RoutedEventArgs e) =>
            ViewModel.Configurations.Restore88KeyLayout();

        private void ApplyNotesInColorTheme(object sender, RoutedEventArgs e) =>
            ViewModel.Configurations.ApplyNotesInColorTheme();

        private void StartWhiteKeyComboBox_DropDownClosed(object sender, object e) {
            ComboBox comboBox = (sender as ComboBox)!;

            ViewModel.StartWhiteKey = comboBox.SelectedIndex;
            comboBox.SelectedIndex = ViewModel.StartWhiteKey;
        }

        private void EndWhiteKeyComboBox_DropDownClosed(object sender, object e) {
            ComboBox comboBox = (sender as ComboBox)!;

            ViewModel.EndWhiteKey = comboBox.SelectedIndex;
            comboBox.SelectedIndex = ViewModel.EndWhiteKey;
        }

        /**
         * ===========================================================================================================
         * 
         *                please ignore whatever is after this section. it's just not MVVM. thank you
         *          
         * ===========================================================================================================
         */

        private Border? lastTappedBorder;
        private Flyout? lastOpenedFlyout;
        private void NoteColorBorderTapped(object sender, TappedRoutedEventArgs args) {
            var border = (Border)sender;
            var flyout = (Flyout)border.ContextFlyout;
            flyout.ShowAt(border);

            lastTappedBorder = border;
            lastOpenedFlyout = flyout;
        }

        private void NoteColorBorderFlyoutClosed(object sender, object args) {
            if (lastTappedBorder is Border border) {
                var animation = new DoubleAnimation { To = 1.0, Duration = new Duration(TimeSpan.FromMilliseconds(200)) };
                var storyboard = new Storyboard();
                Storyboard.SetTarget(animation, border);
                Storyboard.SetTargetProperty(animation, "Opacity");
                storyboard.Children.Add(animation);
                storyboard.Begin();
            }
        }

        private void NoteColorBorderPointerEntered(object sender, PointerRoutedEventArgs args) {
            if (sender is not Border border)
                return;

            var animation = new DoubleAnimation { To = 0.6, Duration = new Duration(TimeSpan.FromMilliseconds(200)) };
            var storyboard = new Storyboard();
            Storyboard.SetTarget(animation, border);
            Storyboard.SetTargetProperty(animation, "Opacity");
            storyboard.Children.Add(animation);
            storyboard.Begin();
        }

        private void NoteColorBorderPointerExited(object sender, PointerRoutedEventArgs args) {
            if (sender is not Border border)
                return;

            if (((Flyout)border.ContextFlyout).IsOpen)
                return;

            var animation = new DoubleAnimation { To = 1.0, Duration = new Duration(TimeSpan.FromMilliseconds(200)) };
            var storyboard = new Storyboard();
            Storyboard.SetTarget(animation, border);
            Storyboard.SetTargetProperty(animation, "Opacity");
            storyboard.Children.Add(animation);
            storyboard.Begin();
        }

        /**
         * Already said bye to good MVVM a while ago
         */
        private void NoteColorAdd(object sender, RoutedEventArgs args) {
            lastOpenedFlyout?.Hide();
            ViewModel.NoteColorAddCommand.Execute((lastOpenedFlyout?.Content as FrameworkElement)?.DataContext);
        }
    }
}