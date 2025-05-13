/**
 *                    Notes in Color
 *            A project by Benyamin Bamburac
 * 
 * This project is a modern MIDI file player and visualizer
 * with some MIDI controller support.
 */

using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Multimedia;
using System.Diagnostics;
using System.IO;

namespace NotesInColor.Core;

/**
 * A utility for MIDI file parsing, powered by DryWetMidi.
 * 
 * This stores immutable information of MIDI file data.
 */
public record MIDIFileData(
    NoteData[] Notes,
    NonNoteTimedEventData[] NonNoteTimedEvents,
    double Duration,
    string Name,
    int Tracks
) {
    /**
     * Returns an instance of MIDIFileData that contains relevant MIDI file data
     * from path parameter.
     */
    public static MIDIFileData Parse(string path) {
        MidiFile midiFile = MidiFile.Read(path, new ReadingSettings {
            InvalidMetaEventParameterValuePolicy = InvalidMetaEventParameterValuePolicy.SnapToLimits
        });

        return Parse(midiFile, Path.GetFileNameWithoutExtension(path));
    }
    
    public static MIDIFileData Parse(MidiFile midiFile, string name) {
        TempoMap tempoMap = midiFile.GetTempoMap();

        int trackIndex = 0;
        List<NoteData> notes = [];
        List<NonNoteTimedEventData> nonNoteTimedEvents = [];
        foreach (var trackChunk in midiFile.GetTrackChunks()) {
            foreach (var note in trackChunk.GetNotes())
                notes.Add(new NoteData(note, tempoMap, trackIndex));
            ++trackIndex;

            foreach (var midiEvent in trackChunk.GetTimedEvents())
                if (midiEvent.Event is not (NoteOnEvent or NoteOnEvent))
                    nonNoteTimedEvents.Add(new NonNoteTimedEventData(midiEvent, tempoMap));
        }

        return new MIDIFileData(
            [.. notes.OrderBy(n => n.Time)],
            [.. nonNoteTimedEvents.OrderBy(e => e.Time)],
            midiFile.GetDuration<MetricTimeSpan>().TotalSeconds,
            name,
            trackIndex);
    }
}