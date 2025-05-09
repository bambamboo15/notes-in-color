/**
 *                    Notes in Color
 *            A project by Benyamin Bamburac
 * 
 * This project is a modern MIDI file player and visualizer
 * with some MIDI controller support.
 */

using NotesInColor.Core;

namespace NotesInColor.Services;

public interface INoteAudioPlayer {
    /**
     * The volume (as a double from 0 to 1).
     */
    public double Volume { get; set; }

    /**
     * Presses or releases a note.
     * 
     * Velocity: 0 = RELEASE, 1-127 = PRESS
     */
    public void Play(int noteNumber, int track, int velocity);

    /**
     * Executes a NonNoteTimedEvent event.
     */
    public void PlayNonNoteTimedEvent(NonNoteTimedEventData nonNoteTimedEvent);

    /**
     * Kaboom!!! (Call this on every song load)
     */
    public void Restart();

    /**
     * Update. This has to be called at least once per frame.
     */
    public void Update();

    /**
     * Send a NoteOff event to all notes.
     */
    public void AllNotesOff();
}