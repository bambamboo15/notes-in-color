/**
 *                    Notes in Color
 *            A project by Benyamin Bamburac
 * 
 * This project is a modern MIDI file player and visualizer
 * with some MIDI controller support.
 */

using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Interaction;

namespace NotesInColor.Core;

/**
 * Wraps a Note struct but adds some extra data so it can handle microseconds.
 */
public readonly record struct NoteData {
    public readonly long Duration => EndTime - Time;
    public readonly int NoteNumber;
    public readonly int Velocity;
    public readonly int Track;
    public readonly int Channel;
    public readonly long Time;
    public readonly long EndTime;

    public readonly static NoteData Null = new();
    public readonly bool IsNull => Track == -1;

    public NoteData() : this(new Note(new SevenBitNumber()), TempoMap.Default, -1) {}

    public NoteData(Note note, TempoMap tempoMap, int track) {
        Time = TimeConverter.ConvertTo<MetricTimeSpan>(note.Time, tempoMap).TotalMicroseconds;
        EndTime = TimeConverter.ConvertTo<MetricTimeSpan>(note.EndTime, tempoMap).TotalMicroseconds;
        Track = track;
        Velocity = note.Velocity;
        NoteNumber = note.NoteNumber;
        Channel = note.Channel;
    }
}