/**
 *                    Notes in Color
 *            A project by Benyamin Bamburac
 * 
 * This project is a modern MIDI file player and visualizer
 * with some MIDI controller support.
 */

using CommunityToolkit.Mvvm.ComponentModel;
using NotesInColor.Core;
using System.ComponentModel;
using System.Diagnostics;

namespace NotesInColor.ViewModel;

/**
 * This is where some forwarding happens. Here's where some conversion
 * happens, but most of the real work happens in the view.
 * 
 * You may notice there's a lot of callback logic. That's because I'm
 * using callback chaining. A lot of it; not something you usually
 * notice in MVVM, but almost necessary.
 */
public partial class RendererViewModel : ObservableObject {
    private readonly MIDIPlaythroughData MIDIPlaythroughData;
    private readonly Configurations Configurations;

    /**
     * The number of white keys
     */
    public int WhiteKeyCount { get; private set; }

    /**
     * The number of pseudo-black keys (1 less because final black key is omitted)
     */
    public int PseudoBlackKeyCount => WhiteKeyCount - 1;

    /**
     * Black key positions
     */
    public bool[] BlackKeyPositions = new bool[128];

    /**
     * White keys and black keys pressed
     */
    private int[] whiteKeysPressed = new int[2];
    private int[] pseudoBlackKeysPressed = new int[1];

    public RendererViewModel(MIDIPlaythroughData MIDIPlaythroughData, Configurations Configurations) {
        this.MIDIPlaythroughData = MIDIPlaythroughData;
        this.Configurations = Configurations;

        RecalculateProperties();

        Configurations.PropertyChanged += (object? sender, PropertyChangedEventArgs e) => {
            if (e.PropertyName == nameof(Configurations.StartKey) ||
                e.PropertyName == nameof(Configurations.EndKey)) {
                RecalculateProperties();
            }
        };
    }

    private void RecalculateProperties() {
        WhiteKeyCount = 1 +
            MIDIKeyHelper.WhiteKeyIndex(Configurations.EndKey) -
            MIDIKeyHelper.WhiteKeyIndex(Configurations.StartKey);

        for (int i = Configurations.StartKey; i < Configurations.EndKey; ++i)
            if (MIDIKeyHelper.IsWhiteKey(i))
                BlackKeyPositions[MIDIKeyHelper.PseudoBlackKeyIndex(i) -
                    MIDIKeyHelper.WhiteKeyIndex(Configurations.StartKey)] =
                        MIDIKeyHelper.IsBlackKey(i + 1);

        whiteKeysPressed = new int[WhiteKeyCount];
        pseudoBlackKeysPressed = new int[PseudoBlackKeyCount];
    }

    /**
     * Forwards some information for piano rendering
     */
    public void PianoForwarder(Action<int[], int[]> callback) {
        for (int i = Configurations.StartKey; i <= Configurations.EndKey; ++i) {
            int keyPlaying = MIDIPlaythroughData.KeysPlaying[i].Track;

            if (MIDIKeyHelper.IsWhiteKey(i)) {
                whiteKeysPressed[
                    MIDIKeyHelper.WhiteKeyIndex(i) -
                    MIDIKeyHelper.WhiteKeyIndex(Configurations.StartKey)
                ] = keyPlaying;
            } else {
                pseudoBlackKeysPressed[
                    MIDIKeyHelper.PseudoBlackKeyIndex(i - 1) -
                    MIDIKeyHelper.PseudoBlackKeyIndex(Configurations.StartKey)
                ] = keyPlaying;
            }
        }

        callback(whiteKeysPressed, pseudoBlackKeysPressed);
    }

    /**
     * Loops through all notes and performs callback operations
     */
    public void AllObservableNotes(Action<double, double, bool, int, int> callback) {
        // I am caching these variables for thread-safety
        int startIndex = MIDIPlaythroughData.ScreenStartIndex;
        int endIndex = MIDIPlaythroughData.ScreenEndIndex;
        long currentMicroseconds = MIDIPlaythroughData.CurrentMicroseconds;
        long screenHeightMicroseconds = MIDIPlaythroughData.ScreenHeightMicroseconds;

        for (int i = startIndex; i < endIndex; ++i) {
            NoteData note = MIDIPlaythroughData.Notes[i];

            if (note.NoteNumber < Configurations.StartKey || note.NoteNumber > Configurations.EndKey)
                continue;

            if (MIDIKeyHelper.IsWhiteKey(note.NoteNumber))
                SendNote(note, currentMicroseconds, screenHeightMicroseconds, callback);
        }

        for (int i = startIndex; i < endIndex; ++i) {
            NoteData note = MIDIPlaythroughData.Notes[i];

            if (note.NoteNumber < Configurations.StartKey || note.NoteNumber > Configurations.EndKey)
                continue;

            if (MIDIKeyHelper.IsBlackKey(note.NoteNumber))
                SendNote(note, currentMicroseconds, screenHeightMicroseconds, callback);
        }
    }

    /**
     * Sends off a note without checking.
     */
    private void SendNote(NoteData note, long currentMicroseconds, long screenHeightMicroseconds, Action<double, double, bool, int, int> callback) {
        double start = (double)(note.Time - currentMicroseconds) / screenHeightMicroseconds;
        double end = (double)(note.EndTime - currentMicroseconds) / screenHeightMicroseconds;

        bool isWhiteKey = MIDIKeyHelper.IsWhiteKey(note.NoteNumber);

        int colorKey = isWhiteKey ? (
            MIDIKeyHelper.WhiteKeyIndex(note.NoteNumber) -
            MIDIKeyHelper.WhiteKeyIndex(Configurations.StartKey)
        ) : (
            MIDIKeyHelper.PseudoBlackKeyIndex(note.NoteNumber - 1) -
            MIDIKeyHelper.PseudoBlackKeyIndex(Configurations.StartKey)
        );

        callback(start, end, isWhiteKey, colorKey, note.Track);
    }
};