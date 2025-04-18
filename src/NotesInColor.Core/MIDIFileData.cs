/**
 *                    Notes in Color
 *            A project by Benyamin Bamburac
 * 
 * This project is a modern MIDI file player and visualizer
 * with some MIDI controller support.
 */

using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;

namespace NotesInColor.Core;

/**
 * A utility for MIDI file parsing, powered by DryWetMidi.
 * 
 * This stores immutable information of MIDI file data.
 */
public record MIDIFileData(
    List<Note> Notes,
    TempoMap TempoMap,
    double Duration,
    string Name
) {
    /**
     * Returns an instance of MIDIFileData that contains relevant MIDI file data
     * from path parameter.
     */
    public static MIDIFileData Parse(string path) {
        MidiFile midiFile = MidiFile.Read(path, new ReadingSettings {
            InvalidMetaEventParameterValuePolicy = InvalidMetaEventParameterValuePolicy.SnapToLimits
        });

        return new MIDIFileData(
            new List<Note>(midiFile.GetNotes()),
            midiFile.GetTempoMap(),
            midiFile.GetDuration<MetricTimeSpan>().TotalSeconds,
            Path.GetFileNameWithoutExtension(path));
    }
}