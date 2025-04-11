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

namespace NotesInColor.ViewModel;

public partial class PlaythroughViewModel(
    MIDIPlaythroughData MIDIPlaythroughData
): ObservableObject {
    [ObservableProperty]
    private bool isPlaying = false;

    /**
     * Toggle play/pause
     */
    [RelayCommand]
    private void TogglePlayPause() {
        MIDIPlaythroughData.Playing = (IsPlaying = !IsPlaying);
    }

    /**
     * Toggle play/pause
     */
}