/**
 *                    Notes in Color
 *            A project by Benyamin Bamburac
 * 
 * This project is a modern MIDI file player and visualizer
 * with some MIDI controller support.
 */

using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using System;
using System.Threading.Tasks;
using Microsoft.UI.Dispatching;
using Windows.UI.Text;
using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Composition;
using Windows.Globalization.NumberFormatting;
using NotesInColor.Views.Dialogs;

namespace NotesInColor.Services;

//
// If you're confused, just refer to src/NotesInColor/NotesInColor.Services/ITTIACTPOSTDOSTCBCWCD.cs
//
public class TryToInitiateAndCompleteTheProcessOfShowingTheDisplayOfSomethingThatCouldBeCalledWhateverContentDialog : ITryToInitiateAndCompleteTheProcessOfShowingTheDisplayOfSomethingThatCouldBeCalledWhateverContentDialog {
    private ContentDialog? current;
    
    public async Task ShowAsyncSorryNoInputMIDIDeviceContentDialog() {
        _ = await Show(SorryNoInputMIDIDeviceDialog.CreateContentDialog());
    }

    public async Task ShowAsyncInputDevicePropertiesContentDialog() {
        _ = await Show(InputDevicePropertiesDialog.CreateContentDialog());
    }

    public async Task<bool> ShowAsyncJustBeforePracticeModeContentDialog() {
        ContentDialogResult result = await Show(JustBeforePracticeModeDialog.CreateContentDialog());
        return result == ContentDialogResult.Primary;
    }

    public async Task ShowAsyncPracticeModeStatsContentDialog() {
        _ = await Show(PracticeModeStatsDialog.CreateContentDialog());
    }

    public async Task ShowSorrySomethingWentWrongFileLoadingDialog() {
        _ = await Show(SorrySomethingWentWrongFileLoadingDialog.CreateContentDialog());
    }

    /**
     * Shows a ContentDialog.
     */
    private async Task<ContentDialogResult> Show(ContentDialog contentDialog) {
        current?.Hide();

        current = contentDialog;
        current.RequestedTheme = (App.Current.Window as MainWindow)!.MainWindowFrame.RequestedTheme;

        return await current.ShowAsync();
    }
}