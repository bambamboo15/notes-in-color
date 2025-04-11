/**
 *                    Notes in Color
 *            A project by Benyamin Bamburac
 * 
 * This project is a modern MIDI file player and visualizer
 * with some MIDI controller support.
 */

using Melanchall.DryWetMidi.Core;
using System.Diagnostics;

namespace NotesInColor.Core;

/**
 * This is the class that is responsible for holding MIDI playthrough data
 * and relevant state, such as currently playing or current tempo.
 */
public class MIDIPlaythroughData {
    public required bool Playing { get; set; } = false;
    public required float Tempo { get; set; } = 1.0f;
    public MIDIFileData? Data => data;
    public bool IsLoaded => isLoaded;

    private MIDIFileData? data;
    private bool isLoaded = false;

    // This isn't good MVVM, but what can I do? Messaging??? :/
    public delegate void OnLoaded();
    public event OnLoaded Loaded = delegate { };

    /**
     * Steps by the amount of delta time, in seconds.
     */
    public void Next(float seconds) {
        Debug.Assert(seconds > 0, "What are you trying to do?");
    }

    /**
     * Loads a song from MIDIFileData and starts playing it.
     */
    public void Load(MIDIFileData data) {
        this.data = data;
        Playing = true;
        isLoaded = true;

        Loaded.Invoke();
    }
}