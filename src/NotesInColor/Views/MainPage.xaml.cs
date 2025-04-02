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
using NotesInColor.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.Pickers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Graphics.Canvas.UI.Xaml;

namespace NotesInColor {
    public sealed partial class MainPage : Page {
        public readonly MainPageViewModel ViewModel;

        public MainPage() {
            this.InitializeComponent();
            ViewModel = App.Current.Services.GetRequiredService<MainPageViewModel>(); // anti-pattern :(
            DataContext = ViewModel;
        }
    }
}
