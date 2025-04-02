/**
 *                    Notes in Color
 *            A project by Benyamin Bamburac
 * 
 * This project is a modern MIDI file player and visualizer
 * with some MIDI controller support.
 */

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NotesInColor.Services;

namespace NotesInColor.ViewModel;

public partial class OpenFileViewModel(
    IRequestMIDIFile RequestMIDIFile
) : ObservableObject {
    [ObservableProperty]
    private bool isOpenFileButtonEnabled = true;

    [RelayCommand]
    private async Task OpenFile() {
        IsOpenFileButtonEnabled = false;

        string? path = await RequestMIDIFile.OpenFile();
        System.Diagnostics.Debug.WriteLine(path);

        // do something like conversion to byte[]
        // somewhere in a model...

        IsOpenFileButtonEnabled = true;
    }
}