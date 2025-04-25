/**
 *                    Notes in Color
 *            A project by Benyamin Bamburac
 * 
 * This project is a modern MIDI file player and visualizer
 * with some MIDI controller support.
 */

namespace NotesInColor.Services;

public interface ISettingsManager {
    /**
     * Manages settings in internal application-specific storage.
     */
    public object this[string key] { get; set; }
}