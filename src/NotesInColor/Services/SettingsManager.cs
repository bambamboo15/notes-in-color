/**
 *                    Notes in Color
 *            A project by Benyamin Bamburac
 * 
 * This project is a modern MIDI file player and visualizer
 * with some MIDI controller support.
 */

using Windows.Storage;

namespace NotesInColor.Services;

public class SettingsManager : ISettingsManager {
    private readonly ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

    public object? this[string key] {
        get => localSettings.Values[key];
        set => localSettings.Values[key] = value;
    }
}