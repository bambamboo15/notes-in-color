﻿/**
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
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;

namespace NotesInColor {
    public partial class App : Application {
        public App() {
            this.InitializeComponent();
        }

        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args) {
            m_window = new MainWindow();

            Frame mainWindowFrame = (m_window as MainWindow)!.MainWindowFrame;
            mainWindowFrame.NavigationFailed += OnNavigationFailed;
            mainWindowFrame.Navigate(typeof(MainPage), args.Arguments);

            m_window.Activate();
        }

        void OnNavigationFailed(object sender, NavigationFailedEventArgs args) {
            throw new Exception("Failed to load Page " + args.SourcePageType.FullName);
        }

        private Window? m_window;
        public Window? Window => m_window;
    }
}