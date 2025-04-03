/**
 *                    Notes in Color
 *            A project by Benyamin Bamburac
 * 
 * This project is a modern MIDI file player and visualizer
 * with some MIDI controller support.
 */

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NotesInColor.Core;
using NotesInColor.Services;

namespace NotesInColor.ViewModel;

public partial class OpenFileViewModel(
    IRequestMIDIFile RequestMIDIFile,
    MIDIFileParser MIDIFileParser
) : ObservableObject {
    [ObservableProperty]
    private bool isOpenFileButtonEnabled = true;

    [RelayCommand]
    private async Task OpenFile() {
        IsOpenFileButtonEnabled = false;

        string? path = await RequestMIDIFile.OpenFile();

        if (path != null) {
            System.Diagnostics.Debug.WriteLine(
                $"[OPENFILEVIEWMODEL] Opened file '{path}'"
            );

            // yay! parse it! now!
            using Stream stream = File.OpenRead(path);
            MIDIFileParser.Parse(stream);
        }

        IsOpenFileButtonEnabled = true;
    }
}