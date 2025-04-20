/**
 *                    Notes in Color
 *            A project by Benyamin Bamburac
 * 
 * This project is a modern MIDI file player and visualizer
 * with some MIDI controller support.
 */

using System.ComponentModel;
using System.Diagnostics;
using NotesInColor.Services;

namespace NotesInColor.Core;

/**
 * Stores configurations that can be considered as part of persistent application state.
 */
public class Configurations(ISettingsManager SettingsManager) : INotifyPropertyChanged {
    /**
     * The start key of the on-screen piano. [INCLUSIVE]
     */
    public int StartKey {
        get => startKey;
        set {
            if (startKey != value) {
                startKey = value;

                // this shoudn't happen
                if (MIDIKeyHelper.IsBlackKey(startKey))
                    --startKey;

                // this also shouldn't happen
                if (startKey > EndKey)
                    (startKey, EndKey) = (EndKey, startKey);

                SettingsManager["startKey"] = startKey;
                OnPropertyChanged(nameof(StartKey));
            }
        }
    }
    private int startKey = (int)(SettingsManager["startKey"] ??= 21);

    /**
     * The end key of the on-screen piano. [INCLUSIVE]
     */
    public int EndKey {
        get => endKey;
        set {
            if (endKey != value) {
                endKey = value;

                // this shoudn't happen
                if (MIDIKeyHelper.IsBlackKey(endKey))
                    ++endKey;

                // this also shouldn't happen
                if (StartKey > endKey)
                    (endKey, StartKey) = (StartKey, endKey);

                SettingsManager["endKey"] = endKey;
                OnPropertyChanged(nameof(EndKey));
            }
        }
    }
    private int endKey = (int)(SettingsManager["endKey"] ??= 108);

    /**
     * Restore the default 88-key layout.
     */
    public void Restore88KeyLayout() {
        StartKey = 21;
        EndKey = 108;
    }

    /**
     * Property changed event
     */
    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged(string name) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}