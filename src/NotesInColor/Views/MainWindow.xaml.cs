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

namespace NotesInColor {
    public sealed partial class MainWindow : Window {
        public MainWindow() {
            this.InitializeComponent();

            this.SystemBackdrop = new MicaBackdrop() { Kind = MicaKind.Base };
            this.ExtendsContentIntoTitleBar = true;

            this.Activated += MainWindow_Activated;

            var manager = WinUIEx.WindowManager.Get(this);
            manager.Width = 1200;
            manager.Height = 800;
            manager.MinWidth = 1200;
            manager.MinHeight = 800;
        }

        private void MainWindow_Activated(object sender, WindowActivatedEventArgs args) {
            if (args.WindowActivationState == WindowActivationState.Deactivated) {
                TitleBarTextBlock.Foreground =
                    (SolidColorBrush)App.Current.Resources["WindowCaptionForegroundDisabled"];
            } else {
                TitleBarTextBlock.Foreground =
                    (SolidColorBrush)App.Current.Resources["WindowCaptionForeground"];
            }
        }

        // The "Open MIDI File" button
        private async void OpenFileButton_Click(object sender, RoutedEventArgs args) {
            // disable the button
            Button senderButton = (sender as Button)!;
            senderButton.IsEnabled = false;

            // open the file picker
            var openPicker = new Windows.Storage.Pickers.FileOpenPicker();

            // retrieve HWND
            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);

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
    }
}