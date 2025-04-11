/**
 *                    Notes in Color
 *            A project by Benyamin Bamburac
 * 
 * This project is a modern MIDI file player and visualizer
 * with some MIDI controller support.
 */

namespace NotesInColor.Services;

public interface IRequestMIDIFile {
    /**
     * Requests a MIDI file, and outputs the path, null otherwise.
     */
    public Task<string?> OpenFile();
}