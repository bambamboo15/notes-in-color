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
using System.Threading.Tasks;

namespace NotesInColor;

public sealed partial class SplashScreen : Page {
    public SplashScreen() {
        this.InitializeComponent();
    }
}