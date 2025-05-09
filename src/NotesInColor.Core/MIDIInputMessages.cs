/**
 *                    Notes in Color
 *            A project by Benyamin Bamburac
 * 
 * This project is a modern MIDI file player and visualizer
 * with some MIDI controller support.
 */

namespace NotesInColor.Core;

/**
 * MIDI message arguments.
 * 
 * Only supports note on/off events.
 */
public abstract record MIDIInputMessage;
public record MIDIInputNoteOnMessage(byte Channel, byte Note, byte Velocity) : MIDIInputMessage { };
public record MIDIInputNoteOffMessage(byte Channel, byte Note) : MIDIInputMessage { };
public record struct MIDIInputMessageArgs(MIDIInputMessage Message);