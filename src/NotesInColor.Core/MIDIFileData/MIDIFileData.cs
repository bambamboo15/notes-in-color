/**
 *                    Notes in Color
 *            A project by Benyamin Bamburac
 * 
 * This project is a modern MIDI file player and visualizer
 * with some MIDI controller support.
 */

namespace NotesInColor.Core;

public class MIDIFileData {
    public ushort format = 0;
    public ushort ntrks = 0;
    public ushort division = 0;
    public MIDITrack[] tracks = [];
}