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
using System.Reflection;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
    public double Progress {
        get => progress;
        private set {
            if (value >= Duration)
                Playing = false;

            if (SetProperty(ref progress, Math.Clamp(value, -WarmupTimeSeconds, Duration))) {
                currentMicroseconds = (long)(progress * 1000000.0);

                RecomputeNotes();
            }
        }
    }
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
     * The list of notes
     */
    public NoteData[] Notes => Data!.Notes;

    /**
     * The number of tracks
     */
    public int Tracks => Data!.Tracks;

    /**
     * Arrays of whatever is currently playing
     */
    public NoteData[] KeysPlaying { get; private set; } = Enumerable.Repeat(NoteData.Null, 128).ToArray();
    private NoteData[,] NotesPlaying = NotesPlayingTracks(1);
    private NoteData[,] NotesPlayingPrev = NotesPlayingTracks(1);

    /**
     * An event for when a note is pressed or released (essentially observable KeysPlaying)
     */
    public event Action<NoteData, bool>? NoteChanged;

    /**
     * Event for when a composition is loaded
     */
    public event Action? OnLoaded;

    /**
     * The MIDI file data
     */
    public MIDIFileData? Data => data;
    private MIDIFileData? data; // for thread safety across loads

    /**
     * Has the composition been loaded yet?
     */
    [ObservableProperty]
    private bool isLoaded = false;

    /**
     * Sliding window start and end indices
     */
    public int ScreenStartIndex { get; private set; } = 0;          // note index of first note in sliding window
    public int ScreenEndIndex { get; private set; } = 0;            // note index of last note in sliding window

    /**
     * Private properties
     */
    private long currentMicroseconds = 0;                           // current number of microseconds during playthrough
    private long currentMicrosecondsFromQueue = 0;                  // current number of microseconds synced with queue

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
        Progress = time;
    }
    
    /**
     * Loads a composition from MIDIFileData and starts playing.
     */
    public void Load(MIDIFileData data) {
        this.data = data;
        NotesPlaying = NotesPlayingTracks(data.Tracks);
        NotesPlayingPrev = NotesPlayingTracks(data.Tracks);
        ScreenStartIndex = 0;
        ScreenEndIndex = 0;
        Duration = data.Duration;
        Progress = -WarmupTimeSeconds;
        Playing = true;
        IsLoaded = true;

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
     * ================================================================
     * 
     * I have recently optimized this so it uses two indices instead
     * of a Queue, and it provides the perfect sliding window functionality
     * without any allocations.
     */
    private void RecomputeNotes() {
        // If we went backwards, then redo everything
        if (currentMicroseconds < currentMicrosecondsFromQueue) {
            ScreenStartIndex = 0;
            ScreenEndIndex = 0;
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
        for (; ScreenStartIndex < ScreenEndIndex; ++ScreenStartIndex) {
            NoteData note = Notes[ScreenStartIndex];
            if (note.EndTime > currentMicroseconds)
                break;
        }

        // Add notes under top of screen
        for (; ScreenEndIndex < Notes.Length; ++ScreenEndIndex) {
            NoteData note = Notes[ScreenEndIndex];
            if (note.EndTime < currentMicroseconds)
                continue;
            else if (note.Time > currentMicroseconds + ScreenHeightMicroseconds)
                break;
        }

        // Update current notes playing
        //
        // =============================================
        //
        // This used to completely collapse for notes played across several tracks.
        // You may think that this plays normally, but it becomes much more obvious
        // when audio comes into play.
        //
        // Therefore, I am using a 2D array. However, this still assumes that in an
        // individual track, no notes are overlapping.
        for (int i = 0; i < 128; ++i)
            for (int j = 0; j < Tracks; ++j)
                NotesPlaying[i, j] = NoteData.Null;

        for (int i = ScreenStartIndex; i < ScreenEndIndex; ++i) {
            NoteData note = Notes[i];
            if (note.EndTime <= currentMicroseconds)
                continue;
            else if (note.Time > currentMicroseconds)
                break;

            NotesPlaying[note.NoteNumber, note.Track] = note;
        }

        // Update current keys playing
        for (int i = 0; i < 128; ++i) {
            NoteData maxTimeNote = NotesPlaying[i, 0];

            for (int j = 1; j < Tracks; ++j) {
                NoteData note = NotesPlaying[i, j];
                if (note.Time >= maxTimeNote.Time)
                    maxTimeNote = note;
            }

            KeysPlaying[i] = maxTimeNote;
        }

        // Audio events for changes in notes playing
        for (int i = 0; i < 128; ++i) {
            for (int j = 0; j < Tracks; ++j) {
                ref NoteData prev = ref NotesPlayingPrev[i, j];
                ref NoteData cur = ref NotesPlaying[i, j];

                if (!prev.Equals(cur)) {
                    if (!prev.IsNull) {
                        // note off
                        NoteChanged?.Invoke(prev, false);
                    }

                    if (prev.IsNull || !cur.IsNull) {
                        // note on
                        NoteChanged?.Invoke(cur, true);
                    }
                }

                prev = cur;
            }
        }
    }

    private static NoteData[,] NotesPlayingTracks(int tracks) {
        NoteData[,] output = new NoteData[128, tracks];

        for (int i = 0; i < 128; ++i)
            for (int j = 0; j < tracks; ++j)
                output[i, j] = NoteData.Null;

        return output;
    }

    partial void OnScreenHeightSecondsChanged(double oldValue, double newValue) {
        ScreenHeightMicroseconds = (long)(newValue * 1000000.0);

        if (newValue > oldValue) {
            RecomputeNotes();
        }
    }
}