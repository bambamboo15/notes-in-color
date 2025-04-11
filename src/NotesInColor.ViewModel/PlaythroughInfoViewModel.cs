/**
 *                    Notes in Color
 *            A project by Benyamin Bamburac
 * 
 * This project is a modern MIDI file player and visualizer
 * with some MIDI controller support.
 */

using CommunityToolkit.Mvvm.ComponentModel;
using NotesInColor.Core;

namespace NotesInColor.ViewModel;

public partial class PlaythroughInfoViewModel : ObservableObject {
    private MIDIPlaythroughData MIDIPlaythroughData;

    [ObservableProperty]
    public string name = "Notes in Color";

    public PlaythroughInfoViewModel(MIDIPlaythroughData MIDIPlaythroughData) {
        this.MIDIPlaythroughData = MIDIPlaythroughData;
        this.MIDIPlaythroughData.Loaded += OnDataLoaded;
    }

    public void OnDataLoaded() {
        Name = MIDIPlaythroughData.Data!.name;
    }
}