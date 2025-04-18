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
using System.Diagnostics;

namespace NotesInColor.ViewModel;

public partial class PlaythroughViewModel : ObservableObject {
    [ObservableProperty]
    private bool playing = false;

    [ObservableProperty]
    private double progress = 0.0;

    [ObservableProperty]
    private bool enabled = false;

    private readonly MIDIPlaythroughData MIDIPlaythroughData;

    public PlaythroughViewModel(MIDIPlaythroughData MIDIPlaythroughData) {
        this.MIDIPlaythroughData = MIDIPlaythroughData;

        // manual binding...
        MIDIPlaythroughData.PropertyChanged += (s, e) => {
            if (e.PropertyName == nameof(MIDIPlaythroughData.Playing)) {
                Playing = MIDIPlaythroughData.Playing;
            } else if (e.PropertyName == nameof(MIDIPlaythroughData.Progress)) {
                Progress = MIDIPlaythroughData.Progress /
                    MIDIPlaythroughData.Data!.Duration;
            } else if (e.PropertyName == nameof(MIDIPlaythroughData.IsLoaded)) {
                Enabled = MIDIPlaythroughData.IsLoaded;
            }
        };
    }

    partial void OnProgressChanging(double value) {
        // if floating-point math goes wrong here, this will cause an infinite loop :(
        //
        //     Jump => Progress => Progress => OnProgressChanging => Jump => ...
        MIDIPlaythroughData.Jump(value * MIDIPlaythroughData.Data!.Duration);
    }

    /**
     * Advances playthrough by the specified number of seconds.
     */
    public void Next(double seconds) {
        MIDIPlaythroughData.Next(seconds);
    }

    /**
     * Toggle play/pause
     */
    [RelayCommand]
    private void TogglePlayPause() {
        MIDIPlaythroughData.Playing = (Playing = !Playing);
    }
}