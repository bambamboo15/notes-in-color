/**
 *                    Notes in Color
 *            A project by Benyamin Bamburac
 * 
 * This project is a modern MIDI file player and visualizer
 * with some MIDI controller support.
 */

namespace NotesInColor.Services;

public interface INoteAudioPlayer {
    /**
     * The volume (as a double from 0 to 1).
     */
    public double Volume { get; set; }

    /**
     * Plays a note.
     */
    public void Play(int noteNumber, int track, int velocity);

    /**
     * Update. This has to be called at least once per frame.
     */
    public void Update();

    /**
     * Send a NoteOff event to all notes.
     */
    public void AllNotesOff();
}