/**
 *                    Notes in Color
 *            A project by Benyamin Bamburac
 * 
 * This project is a modern MIDI file player and visualizer
 * with some MIDI controller support.
 */

using CommunityToolkit.Mvvm.ComponentModel;
using NotesInColor.Core;
using System;

namespace NotesInColor.ViewModel;

public partial class PlaythroughInfoViewModel : ObservableObject {
    private readonly MIDIPlaythroughData MIDIPlaythroughData;

    [ObservableProperty]
    public string name = "Notes in Color";

    [ObservableProperty]
    public bool loaded = false;

    public PlaythroughInfoViewModel(MIDIPlaythroughData MIDIPlaythroughData) {
        this.MIDIPlaythroughData = MIDIPlaythroughData;
        this.MIDIPlaythroughData.OnLoaded += OnDataLoaded;
    }

    /**
     * When the MIDI file loads, now we begin...
     */
    public void OnDataLoaded() {
        Name = MIDIPlaythroughData.Data!.Name;
        Loaded = true;
    }
}