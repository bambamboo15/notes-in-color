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

namespace NotesInColor.Core;

/**
 * This is the class that is responsible for holding MIDI playthrough data
 * and relevant state, such as currently playing or current tempo.
 * 
 * I like to think of this as a "batteries-included" model
 */
public class MIDIPlaythroughData : INotifyPropertyChanged {
    /**
     * Is the composition currently playing?
     */
    public bool Playing {
        get => playing;
        set {
            if (playing != value) {
                playing = value;
                OnPropertyChanged(nameof(Playing));
            }
        }
    }
    private bool playing = false;

    /**
     * What is the tempo (relative speed) of composition?
     */
    public required double Tempo { get; set; } = 1.0;

    /**
     * What is the progress (in seconds) the song has come to?
     */
    public double Progress {
        get => progress;
        private set {
            if (progress != value) {
                progress = value;

                long microseconds = (long)(value * 1000000.0);
                long ticks = TimeConverter.ConvertFrom(new MetricTimeSpan(microseconds), Data!.TempoMap);
                currentTicks = ticks;

                RecomputeNotes();

                if (value >= Data!.Duration)
                    Playing = false;

                OnPropertyChanged(nameof(Progress));
            }
        }
    }
    private double progress = 1.0;

    /**
     * What is the current tick count?
     */
    public long CurrentTicks => currentTicks;

    /**
     * The "screen height" in ticks
     */
    public readonly long screenHeightTicks = 1000;

    /**
     * The list of currently observable notes
     */
    public Queue<Note> notes = [];

    /**
     * An array of if a key is currently playing
     * 
     * MIDI file keys go from C-1 (0) to G9 (127)
     */
    public bool[] KeysPlaying { get; private set; } = [
        false, false, false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false
    ];

    /**
     * Property changed event
     */
    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged(string name) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

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
    public bool IsLoaded {
        get => isLoaded;
        set {
            if (isLoaded != value) {
                isLoaded = value;
                OnPropertyChanged(nameof(IsLoaded));
            }
        }
    }
    private bool isLoaded = false;

    /**
     * Private properties
     */
    private long currentTicks = 0;            // current number of ticks during playthrough
    private long currentTicksFromQueue = 0;   // current number of ticks synced with queue
    private long keepTicks = 20;             // number of ticks note is kept before queued for disposal
    private int currentIndex = 0;             // note index of last note actually

    /**
     * Step FORWARDS by the amount of delta time, in seconds, IF PLAYING.
     */
    public void Next(double deltaTime) {
        Debug.Assert(deltaTime >= 0.0, "You're not stepping backwards");

        if (Playing)
            Progress += deltaTime;
    }

    /**
     * Jump to a certain time, even when not playing.
     */
    public void Jump(double time) {
        Progress = Math.Clamp(time, 0.0, Data!.Duration);
    }
    
    /**
     * Loads a composition from MIDIFileData. (Stops playing.)
     */
    public void Load(MIDIFileData data) {
        this.data = data;
        Playing = false;
        IsLoaded = true;
        currentTicks = 0;
        currentIndex = 0;
        notes.Clear();
        Progress = 0.0;

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
     * a note whose top (Time + Length) exceeds the bottom of the screen,
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
     */
    private void RecomputeNotes() {
        // If we went backwards, then redo everything
        if (currentTicks < currentTicksFromQueue) {
            notes.Clear();
            currentIndex = 0;
        }
        currentTicksFromQueue = currentTicks;

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
            Note note = notes.Peek();
            if (note.EndTime > currentTicks - keepTicks)
                break;

            notes.Dequeue();
        }

        // Update current keys playing
        for (int i = 0; i < 128; ++i)
            KeysPlaying[i] = false;

        foreach (Note note in notes) {
            if (note.EndTime < currentTicks - keepTicks)
                continue;
            else if (note.Time > currentTicks)
                break;

            KeysPlaying[note.NoteNumber] = true;
        }

        // Add notes under top of screen
        for (int index = currentIndex; index < Data!.Notes.Count; ++index) {
            Note note = Data!.Notes[index];
            if (note.EndTime < currentTicks)
                continue;
            else if (note.Time > currentTicks + screenHeightTicks)
                break;

            notes.Enqueue(note);
            currentIndex = index;
        }
    }
}