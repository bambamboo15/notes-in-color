/**
 *                    Notes in Color
 *            A project by Benyamin Bamburac
 * 
 * This project is a modern MIDI file player and visualizer
 * with some MIDI controller support.
 */

using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.Pickers;

namespace NotesInColor {
    public sealed partial class MainPage : Page {
        public MainPage() {
            this.InitializeComponent();
        }

        // The "Open MIDI File" button
        private async void OpenFileButton_Click(object sender, RoutedEventArgs args) {
            // disable the button
            Button senderButton = (sender as Button)!;
            senderButton.IsEnabled = false;

            // open the file picker
            var openPicker = new Windows.Storage.Pickers.FileOpenPicker();

            // retrieve HWND
            var actualWindow = (App.Current as App)!.Window!;
            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(actualWindow);

            // initialize file picker with HWND
            WinRT.Interop.InitializeWithWindow.Initialize(openPicker, hWnd);

            // set options for file picker
            //   plan to support .mid, .midi
            openPicker.ViewMode = PickerViewMode.Thumbnail;
            openPicker.FileTypeFilter.Add(".midi");
            openPicker.FileTypeFilter.Add(".mid");

            // open the picker for the user to pick a file
            var file = await openPicker.PickSingleFileAsync();
            if (file != null) {
                // picked a file
            } else {
                // operation cancelled
            }

            // enable the button
            senderButton.IsEnabled = true;
        }

        // Open settings
        private void OpenSettings_Click(object sender, RoutedEventArgs args) {
            Frame.Navigate(typeof(SettingsPage));
        }
    }
}
