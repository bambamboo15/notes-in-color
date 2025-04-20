/**
 *                    Notes in Color
 *            A project by Benyamin Bamburac
 * 
 * This project is a modern MIDI file player and visualizer
 * with some MIDI controller support.
 */

using Melanchall.DryWetMidi.Interaction;

namespace NotesInColor.Core;

/**
 * Wraps a Note struct but adds some extra data so it can handle microseconds.
 */
public readonly record struct NoteData {
    private readonly Note note;

    public readonly int NoteNumber => note.NoteNumber;
    public readonly int Track;
    public readonly long Time;
    public readonly long EndTime;

    public static NoteData Null = new NoteData();
    public bool IsNull => Track == -1;

    public NoteData() : this(new Note(new()), TempoMap.Default, -1) {}

    public NoteData(Note note, TempoMap tempoMap, int track) {
        Time = TimeConverter.ConvertTo<MetricTimeSpan>(note.Time, tempoMap).TotalMicroseconds;
        EndTime = TimeConverter.ConvertTo<MetricTimeSpan>(note.EndTime, tempoMap).TotalMicroseconds;
        Track = track;

        this.note = note;
    }
}