/**
 *                    Notes in Color
 *            A project by Benyamin Bamburac
 * 
 * This project is a modern MIDI file player and visualizer
 * with some MIDI controller support.
 */

using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using NotesInColor.ViewModel;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

namespace NotesInColor.Views.Dialogs {
    public sealed partial class PracticeModeStatsDialog : UserControl, ICreateContentDialog {
        private readonly PracticeModeViewModel PracticeModeViewModel;
        private readonly PlaythroughViewModel PlaythroughViewModel;

        public PracticeModeStatsDialog() {
            PracticeModeViewModel = App.Current.Services.GetRequiredService<PracticeModeViewModel>();
            PlaythroughViewModel = App.Current.Services.GetRequiredService<PlaythroughViewModel>();

            this.InitializeComponent();
        }

        public static ContentDialog CreateContentDialog() {
            return new ContentDialog {
                Title = "Post-practice stats",
                PrimaryButtonText = "OK",
                DefaultButton = ContentDialogButton.Primary,
                Content = new PracticeModeStatsDialog(),
                XamlRoot = ((App.Current.Window as MainWindow)?.MainWindowFrame.Content as MainPage)?.XamlRoot,
                Style = (Style)Application.Current.Resources["DefaultContentDialogStyle"]
            };
        }
    }
}
