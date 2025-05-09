/**
 *                    Notes in Color
 *            A project by Benyamin Bamburac
 * 
 * This project is a modern MIDI file player and visualizer
 * with some MIDI controller support.
 */

using CommunityToolkit.Mvvm.ComponentModel;
using NotesInColor.Core;
using System.ComponentModel;
using System.Diagnostics;

namespace NotesInColor.ViewModel;

/**
 * This is where some forwarding happens. Here's where some conversion
 * happens, but most of the real work happens in the view.
 * 
 * This class has a huge limitation: it does not deal with Configurations.StartKey
 * and Configurations.EndKey for the view, which means that horizontal screen bounds
 * are not managed here.
 */
public partial class RendererViewModel(MIDIPlaythroughData MIDIPlaythroughData) : ObservableObject {
    /**
     * Random observable properties that the View reads out
     */
    [ObservableProperty]
    private bool showFPS = false;

    [ObservableProperty]
    private bool showPiano = true;

    /**
     * Keys pressed (only updated by ComputeKeysPressed)
     */
    public int[] KeysPressed { get; private set; } = new int[128];

    /**
     * This function computes the current playing track of each note (-1 if nothing is playing on that note).
     */
    public void ComputeKeysPressed() {
        for (int i = 0; i < 128; ++i)
            KeysPressed[i] = MIDIPlaythroughData.KeysPlaying[i].Track;
    }

    /**
     * Delegate that recieves note data from AllNotes.
     */
    public delegate void AllNotesCallback(double startOnScreen, double endOnScreen, int key, int track);

    /**
     * Loops through all observable notes and performs callback operations
     */
    public void AllObservableNotes(AllNotesCallback callback) {
        for (int i = MIDIPlaythroughData.ScreenStartIndex; i < MIDIPlaythroughData.ScreenEndIndex; ++i) {
            NoteData note = MIDIPlaythroughData.Notes[i];

            double start = (double)(note.Time - MIDIPlaythroughData.CurrentMicroseconds) / MIDIPlaythroughData.ScreenHeightMicroseconds;
            double end = (double)(note.EndTime - MIDIPlaythroughData.CurrentMicroseconds) / MIDIPlaythroughData.ScreenHeightMicroseconds;

            callback(start, end, note.NoteNumber, note.Track);
        }
    }
};