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
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;

namespace NotesInColor.Views.Dialogs {
    public sealed partial class JustBeforePracticeModeDialog : UserControl, ICreateContentDialog {
        public JustBeforePracticeModeDialog() {
            this.InitializeComponent();
        }

        public static ContentDialog CreateContentDialog() {
            return new ContentDialog {
                Title = "About to start Practice Mode!",
                PrimaryButtonText = "OK",
                DefaultButton = ContentDialogButton.Primary,
                Content = new JustBeforePracticeModeDialog(),
                XamlRoot = (App.Current.Window as MainWindow)?.MainWindowFrame?.XamlRoot,
                Style = (Style)Application.Current.Resources["DefaultContentDialogStyle"]
            };
        }
    }
}
