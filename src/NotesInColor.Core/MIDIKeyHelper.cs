

using System.Diagnostics;

/**
 *                    Notes in Color
 *            A project by Benyamin Bamburac
 * 
 * This project is a modern MIDI file player and visualizer
 * with some MIDI controller support.
 */
namespace NotesInColor.Core;

/**
 * A helper for manipulating MIDI key values. (0 = C-1, 127 = G9)
 */
public static class MIDIKeyHelper {
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

        return 7 * div + (mod switch {
            0 => 0,
            2 => 1,
            4 => 2,
            5 => 3,
            7 => 4,
            9 => 5,
            11 => 6,
            _ => Trap()
        });
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

        return 7 * div + (mod switch {
            0 => 0,
            2 => 1,
            4 => 2,
            5 => 3,
            7 => 4,
            9 => 5,
            11 => 6,
            _ => Trap()
        });
    }

    /**
     * Find the black key index of the given key.
     *   - Key must be known as black key
     *   - Key must be valid
     */
    public static int BlackKeyIndex(int key) {
        Debug.Assert(IsBlackKey(key), $"This key should've been black: {key}");

        int div = key / 12, mod = key % 12;

        return 5 * div + (mod switch {
            1 => 0,
            3 => 1,
            6 => 3,
            8 => 4,
            10 => 5,
            _ => Trap()
        });
    }

    private static int Trap() {
        Debug.Assert(false, " 5 more minutes of debugging! :D ");
        return 314159;
    }
}