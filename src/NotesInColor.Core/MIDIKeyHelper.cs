/**
 *                    Notes in Color
 *            A project by Benyamin Bamburac
 * 
 * This project is a modern MIDI file player and visualizer
 * with some MIDI controller support.
 */

using System.Diagnostics;

namespace NotesInColor.Shared;

/**
 * A helper for manipulating MIDI key values. (0 = C-1, 127 = G9)
 * 
 * LIMITATION:
 *   Does not validate key values to see if they are in the [0, 127] range.
 */
public static class MIDIKeyHelper {
    /**
     * A note name lookup table
     */
    private static readonly string[] noteNameLookup = [
        "C-1", "C#-1", "D-1", "D#-1", "E-1", "F-1", "F#-1", "G-1", "G#-1", "A-1", "A#-1", "B-1",
        "C0", "C#0", "D0", "D#0", "E0", "F0", "F#0", "G0", "G#0", "A0", "A#0", "B0",
        "C1", "C#1", "D1", "D#1", "E1", "F1", "F#1", "G1", "G#1", "A1", "A#1", "B1",
        "C2", "C#2", "D2", "D#2", "E2", "F2", "F#2", "G2", "G#2", "A2", "A#2", "B2",
        "C3", "C#3", "D3", "D#3", "E3", "F3", "F#3", "G3", "G#3", "A3", "A#3", "B3",
        "C4", "C#4", "D4", "D#4", "E4", "F4", "F#4", "G4", "G#4", "A4", "A#4", "B4",
        "C5", "C#5", "D5", "D#5", "E5", "F5", "F#5", "G5", "G#5", "A5", "A#5", "B5",
        "C6", "C#6", "D6", "D#6", "E6", "F6", "F#6", "G6", "G#6", "A6", "A#6", "B6",
        "C7", "C#7", "D7", "D#7", "E7", "F7", "F#7", "G7", "G#7", "A7", "A#7", "B7",
        "C8", "C#8", "D8", "D#8", "E8", "F8", "F#8", "G8", "G#8", "A8", "A#8", "B8",
        "C9", "C#9", "D9", "D#9", "E9", "F9", "F#9", "G9"
    ];

    /**
     * Is the key a white key?
     */
    public static bool IsWhiteKey(int key) => (key % 12) switch {
        0 or 2 or 4 or 5 or 7 or 9 or 11 => true,
        _ => false
    };

    /**
     * Is the key a black key?
     */
    public static bool IsBlackKey(int key) => !IsWhiteKey(key);

    /**
     * Find the white key index of the given key.
     *   - Key must be known as white key
     *   - Key must be valid
     */
    public static int WhiteKeyIndex(int key) {
        Debug.Assert(IsWhiteKey(key), $"This key should've been white: {key}");

        int div = key / 12, mod = key % 12;

        return 7 * div + mod switch {
            0 => 0,
            2 => 1,
            4 => 2,
            5 => 3,
            7 => 4,
            9 => 5,
            11 => 6,
            _ => Trap()
        };
    }

    /**
     * Find the pseudo black key index of the key RIGHT AFTER the given key
     *   - Key must be known as white key
     *   - Key must be valid
     * 
     * A pseudo black key counts empty spaces as well.
     */
    public static int PseudoBlackKeyIndex(int key) {
        Debug.Assert(IsWhiteKey(key), $"This key should've been white: {key}");

        int div = key / 12, mod = key % 12;

        return 7 * div + mod switch {
            0 => 0,
            2 => 1,
            4 => 2,
            5 => 3,
            7 => 4,
            9 => 5,
            11 => 6,
            _ => Trap()
        };
    }

    /**
     * Find the black key index of the given key.
     *   - Key must be known as black key
     *   - Key must be valid
     */
    public static int BlackKeyIndex(int key) {
        Debug.Assert(IsBlackKey(key), $"This key should've been black: {key}");

        int div = key / 12, mod = key % 12;

        return 5 * div + mod switch {
            1 => 0,
            3 => 1,
            6 => 2,
            8 => 3,
            10 => 4,
            _ => Trap()
        };
    }

    /**
     * Convert the key to a formatted string.
     */
    public static string ToFormatted(int key) =>
        noteNameLookup[key];

    /**
     * Find the key given the white key index.
     */
    public static int KeyFromWhite(int whiteKey) {
        int div = whiteKey / 7, mod = whiteKey % 7;

        return 12 * div + mod switch {
            0 => 0,
            1 => 2,
            2 => 4,
            3 => 5,
            4 => 7,
            5 => 9,
            6 => 11,
            _ => Trap()
        };
    }

    // I am completely aware that KeyFromBlack and KeyFromPseudoBlack are
    // not implemented, but they are not used anywhere, so...
    public static int KeyFromBlack(int blackKey) =>
        throw new NotImplementedException("KeyFromBlack not implemented");

    public static int KeyFromPseudoBlack(int pseudoBlackKey) =>
        throw new NotImplementedException("KeyFromPseudoBlack not implemented");

    private static int Trap() {
        Debug.Assert(false, " 5 more minutes of debugging! :D ");
        return 314159;
    }
}