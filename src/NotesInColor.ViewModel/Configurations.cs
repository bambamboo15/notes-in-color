/**
 *                    Notes in Color
 *            A project by Benyamin Bamburac
 * 
 * This project is a modern MIDI file player and visualizer
 * with some MIDI controller support.
 */

using System.ComponentModel;
using System.Diagnostics;
using System.Text.Json;
using NotesInColor.Services;
using NotesInColor.Shared;
using System.Collections.ObjectModel;
using System.Drawing;

namespace NotesInColor.ViewModel;

/**
 * Stores configurations that can be considered as part of persistent application state.
 * 
 * Originally the strangest model, but now the strangest viewmodel, if it can even be considered as one.
 */
public class Configurations : INotifyPropertyChanged {
    private readonly ISettingsManager SettingsManager;
    public Configurations(ISettingsManager SettingsManager) {
        this.SettingsManager = SettingsManager;

        // subscribe to collection changed
        NoteColors.CollectionChanged += (s, e) => {
            SettingsManager["noteColors"] = JsonSerializer.Serialize<ColorRGB[]>([.. NoteColors.Select(bc => new ColorRGB(bc))]);

            if (e.NewItems is not null)
                foreach (BindableColor bindableColor in e.NewItems)
                    bindableColor.PropertyChanged += (_, _) =>
                        SettingsManager["noteColors"] = JsonSerializer.Serialize<ColorRGB[]>([.. NoteColors.Select(bc => new ColorRGB(bc))]);
        };

        StartKey = (int)(SettingsManager["startKey"] ??= StartKey);
        EndKey = (int)(SettingsManager["endKey"] ??= EndKey);

        if (SettingsManager["noteColors"] is string json) {
            foreach (ColorRGB colorRGB in JsonSerializer.Deserialize<ColorRGB[]>(json)!)
                NoteColors.Add(new BindableColor(colorRGB));
        } else {
            ApplyNotesInColorTheme();
        }
    }

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
    private int startKey = 21;

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
    private int endKey = 108;

    /**
     * Note colors.
     */
    public ObservableCollection<BindableColor> NoteColors = [];

    /**
     * Restore the default 88-key layout.
     */
    public void Restore88KeyLayout() {
        StartKey = 21;
        EndKey = 108;
    }

    /**
     * Apply the "Notes in Color" theme
     */
    public void ApplyNotesInColorTheme() {
        ColorRGB[] colors = [new ColorRGB(0x00, 0xBC, 0xD4), new ColorRGB(0xF0, 0x94, 0x13), new ColorRGB(0x42, 0x87, 0xF5),
                             new ColorRGB(0x4C, 0xAF, 0x50), new ColorRGB(0xE9, 0x1E, 0x63), new ColorRGB(0x9C, 0x27, 0xB0)];

        // the reason why we have to go through all this code is to prevent the situation of there being
        // zero colors at all
        while (NoteColors.Count < colors.Length)
            NoteColors.Add(new BindableColor(255, 255, 255));
        for (int i = 0; i < colors.Length; ++i)
            NoteColors[i].ColorRGB = colors[i];
        while (NoteColors.Count > colors.Length)
            NoteColors.RemoveAt(NoteColors.Count - 1);
    }

    /**
     * Property changed event
     */
    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged(string name) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}