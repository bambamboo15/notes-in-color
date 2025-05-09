/**
 *                    Notes in Color
 *            A project by Benyamin Bamburac
 * 
 * This project is a modern MIDI file player and visualizer
 * with some MIDI controller support.
 */

using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;
using System.Diagnostics;

namespace NotesInColor.Core;

/**
 * exactly what it says
 */
public readonly record struct NonNoteTimedEventData {
    public readonly static NonNoteTimedEventData Null = new();
    public readonly bool IsNull => TimedEvent.Event is NoteOffEvent;
    public readonly long Time;
    public readonly TimedEvent TimedEvent;

    public NonNoteTimedEventData() : this(new TimedEvent(new NoteOffEvent()), TempoMap.Default) { }

    public NonNoteTimedEventData(TimedEvent timedEvent, TempoMap tempoMap) {
        Debug.Assert(timedEvent.Event is not (NoteOnEvent or NoteOnEvent));

        Time = TimeConverter.ConvertTo<MetricTimeSpan>(timedEvent.Time, tempoMap).TotalMicroseconds;
        TimedEvent = timedEvent;
    }
}