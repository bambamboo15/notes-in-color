/**
 *                    Notes in Color
 *            A project by Benyamin Bamburac
 * 
 * This project is a modern MIDI file player and visualizer
 * with some MIDI controller support.
 */

using Melanchall.DryWetMidi.Core;

namespace NotesInColor.Core;

/**
 * A utility for MIDI file parsing, powered by DryWetMidi.
 * 
 * This stores immutable information of MIDI file data.
 */
public class MIDIFileData(MidiFile midiFile, string name) {
    public readonly MidiFile? midiFile = midiFile;
    public readonly string name = name;

    /**
     * Returns an instance of MIDIFileData that contains relevant MIDI file data
     * from path parameter.
     */
    public static MIDIFileData Parse(string path) {
        return new MIDIFileData(MidiFile.Read(path),
            Path.GetFileNameWithoutExtension(path));
    }
}