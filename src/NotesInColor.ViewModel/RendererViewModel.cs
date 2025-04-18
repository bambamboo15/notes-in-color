/**
 *                    Notes in Color
 *            A project by Benyamin Bamburac
 * 
 * This project is a modern MIDI file player and visualizer
 * with some MIDI controller support.
 */

using CommunityToolkit.Mvvm.ComponentModel;
using Melanchall.DryWetMidi.Interaction;
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
    }

    /**
     * Forwards some information for piano rendering
     */
    public void PianoForwarder(Action<bool[], bool[]> callback) {
        bool[] whiteKeysPressed = new bool[WhiteKeyCount];
        bool[] pseudoBlackKeysPressed = new bool[PseudoBlackKeyCount];

        for (int i = Configurations.StartKey; i <= Configurations.EndKey; ++i) {
            bool keyPlaying = MIDIPlaythroughData.KeysPlaying[i];

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
     * 
     * LIMITATION:
     *   Black keys do not appear over white keys.
     */
    public void AllObservableNotes(Action<double, double, bool, int> callback) {
        foreach (Note note in MIDIPlaythroughData.notes) {
            if (note.NoteNumber < Configurations.StartKey || note.NoteNumber > Configurations.EndKey)
                continue;

            double start = (double)(note.Time - MIDIPlaythroughData.CurrentTicks)
                / MIDIPlaythroughData.screenHeightTicks;
            double end = (double)(note.EndTime - MIDIPlaythroughData.CurrentTicks)
                / MIDIPlaythroughData.screenHeightTicks;

            bool isWhiteKey = MIDIKeyHelper.IsWhiteKey(note.NoteNumber);
            int colorKey = isWhiteKey ? (
                MIDIKeyHelper.WhiteKeyIndex(note.NoteNumber) -
                MIDIKeyHelper.WhiteKeyIndex(Configurations.StartKey)
            ) : (
                MIDIKeyHelper.PseudoBlackKeyIndex(note.NoteNumber - 1) -
                MIDIKeyHelper.PseudoBlackKeyIndex(Configurations.StartKey)
            );
            callback(start, end, isWhiteKey, colorKey);
        }
    }
};