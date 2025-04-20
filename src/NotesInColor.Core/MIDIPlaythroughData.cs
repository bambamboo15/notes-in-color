/**
 *                    Notes in Color
 *            A project by Benyamin Bamburac
 * 
 * This project is a modern MIDI file player and visualizer
 * with some MIDI controller support.
 */

using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using System;
using System.ComponentModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;

namespace NotesInColor.Core;

/**
 * This is the class that is responsible for holding MIDI playthrough data
 * and relevant state, such as currently playing or current tempo.
 * 
 * I like to think of this as a "batteries-included" model
 */
public partial class MIDIPlaythroughData : ObservableObject {
    /**
     * Is the composition currently playing?
     */
    [ObservableProperty]
    private bool playing = false;

    /**
     * What is the tempo (relative speed) of composition?
     */
    [ObservableProperty]
    public double tempo = 1.0;

    /**
     * What is the progress (in seconds) the song has come to?
     */
    [ObservableProperty]
    private double progress = 0.0;

    /**
     * What is the duration of the song?
     */
    [ObservableProperty]
    private double duration = 0.0;

    /**
     * What is the current tick count?
     */
    public long CurrentMicroseconds => currentMicroseconds;

    /**
     * The "screen height" in seconds
     */
    [ObservableProperty]
    private double screenHeightSeconds = 1.0;

    /**
     * The "screen height" in microseconds
     */
    public long ScreenHeightMicroseconds { get; private set; } = 1000000;

    /**
     * The warmup time in seconds
     */
    public double WarmupTimeSeconds { get; private set; } = 2.0;

    /**
     * The list of currently observable notes
     */
    public Queue<NoteData> notes = [];

    /**
     * An array of if a key is currently playing (nullable notes)
     */
    public NoteData[] KeysPlaying { get; private set; } = Enumerable.Repeat(NoteData.Null, 128).ToArray();

    /**
     * Event for when a composition is loaded
     */
    public event Action? OnLoaded;

    /**
     * The MIDI file data
     */
    public MIDIFileData? Data => data;
    private MIDIFileData? data;

    /**
     * Has the composition been loaded yet?
     */
    [ObservableProperty]
    private bool isLoaded = false;

    /**
     * Private properties
     */
    private long currentMicroseconds = 0;            // current number of microseconds during playthrough
    private long currentMicrosecondsFromQueue = 0;   // current number of microseconds synced with queue
    private int currentIndex = 0;                    // note index of last note actually

    /**
     * Step FORWARDS by the amount of delta time, in seconds, IF PLAYING.
     * 
     * This is affected by tempo.
     */
    public void Next(double deltaTime) {
        Debug.Assert(deltaTime >= 0.0, "You're not stepping backwards");

        if (Playing)
            Progress += deltaTime * Tempo;
    }

    /**
     * Jump to a certain time, even when not playing.
     */
    public void Jump(double time) {
        Progress = Math.Clamp(time, -WarmupTimeSeconds, Duration);
    }
    
    /**
     * Loads a composition from MIDIFileData and starts playing.
     */
    public void Load(MIDIFileData data) {
        this.data = data;
        Playing = true;
        IsLoaded = true;
        currentIndex = 0;
        notes.Clear();
        Duration = data.Duration;
        Progress = -WarmupTimeSeconds;

        OnLoaded?.Invoke();
    }

    /**
     * This function processes notes!
     * 
     *                   ||    ||  ||
     *                 ||    ||    ||
     *               ||  ||  ||
     *               ||  ||  ||
     * ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
     * 
     * Specifically, we start actually processing notes when we reach
     * a note whose top (EndTime) exceeds the bottom of the screen,
     * (currentTicks) implying that a note is currently visible.
     * 
     * We stop when we reach a note whose bottom (Time) exceeds the top
     * of the screen (currentTicks + screenHeightTicks).
     * 
     * I hope this works :)
     * 
     * LIMITATION:
     *   Visible speedup/slowdown when tempo changes. This is because all
     *   calculations are done with ticks instead of microseconds.
     * 
     * TODO:
     *   Fix it.
     *   
     * LIMITATION:
     *   Playthrough is abnormally fast.
     */
    private void RecomputeNotes() {
        // If we went backwards, then redo everything
        if (currentMicroseconds < currentMicrosecondsFromQueue) {
            notes.Clear();
            currentIndex = 0;
        }
        currentMicrosecondsFromQueue = currentMicroseconds;

        // Discard all notes under bottom of screen
        //
        // LIMITATION:
        //   ...but not all the time. Imagine the following scenario:
        //
        //                            ||
        //               ^^^^^^^^^^^^^||^^^^^^^^^^^^^   (bottom of screen)
        //                            ||
        //                        ||  ||
        //                        ||  ||
        //                            ||
        //
        //   Obviously, the right note will be looked at first,
        //   since notes are ordered by their bottom. Since its top
        //   is not lower than the bottom of screen, the discard loop
        //   ends before the left note can be discarded.
        //
        // HOWEVER:
        //   These will be discarded eventually.
        while (notes.Count > 0) {
            NoteData note = notes.Peek();
            if (note.EndTime > currentMicroseconds)
                break;

            notes.Dequeue();
        }

        // Add notes under top of screen
        for (int index = currentIndex; index < Data!.Notes.Count; ++index) {
            NoteData note = Data!.Notes[index];
            if (note.EndTime < currentMicroseconds)
                continue;
            else if (note.Time > currentMicroseconds + ScreenHeightMicroseconds)
                break;

            notes.Enqueue(note);
            currentIndex = index;
        }

        // Update current keys playing
        for (int i = 0; i < 128; ++i)
            KeysPlaying[i] = NoteData.Null;

        foreach (NoteData note in notes) {
            if (note.EndTime < currentMicroseconds)
                continue;
            else if (note.Time > currentMicroseconds)
                break;
            else if (!KeysPlaying[note.NoteNumber].IsNull)
                continue;

            KeysPlaying[note.NoteNumber] = note;
        }
    }

    partial void OnScreenHeightSecondsChanging(double oldValue, double newValue) {
        ScreenHeightMicroseconds = (long)(newValue * 1000000.0);

        Next(0.0);
    }

    partial void OnProgressChanging(double oldValue, double newValue) {
        currentMicroseconds = (long)(newValue * 1000000.0);

        RecomputeNotes();

        if (newValue >= Duration)
            Playing = false;
    }
}