/**
 *                    Notes in Color
 *            A project by Benyamin Bamburac
 * 
 * This project is a modern MIDI file player and visualizer
 * with some MIDI controller support.
 */

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NotesInColor.Shared;
using NotesInColor.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;

namespace NotesInColor.ViewModel;

/**
 * Settings page.
 */
public partial class SettingsPageViewModel : ObservableObject {
    public ObservableCollection<string> NoteNames { get; private set; } = [];

    [ObservableProperty]
    private int startWhiteKey;

    [ObservableProperty]
    private int endWhiteKey;

    [ObservableProperty]
    private bool canRemove;

    public readonly Configurations Configurations;

    public SettingsPageViewModel(Configurations Configurations) {
        this.Configurations = Configurations;

        StartWhiteKey = MIDIKeyHelper.WhiteKeyIndex(Configurations.StartKey);
        EndWhiteKey = MIDIKeyHelper.WhiteKeyIndex(Configurations.EndKey);
        CanRemove = Configurations.NoteColors.Count > 1;

        for (int i = 0; i < 128; ++i)
            if (MIDIKeyHelper.IsWhiteKey(i))
                NoteNames.Add(MIDIKeyHelper.ToFormatted(i));

        Configurations.PropertyChanged += OnConfigurationsPropertyChanged;
        Configurations.NoteColors.CollectionChanged += (_, _) => {
            CanRemove = Configurations.NoteColors.Count > 1;
        };
    }

    partial void OnStartWhiteKeyChanged(int value) =>
        Configurations.StartKey = MIDIKeyHelper.KeyFromWhite(value);

    partial void OnEndWhiteKeyChanged(int value) =>
        Configurations.EndKey = MIDIKeyHelper.KeyFromWhite(value);

    [RelayCommand]
    private void NoteColorAdd(BindableColor color) {
        int index = Configurations.NoteColors.IndexOf(color);
        if (index != -1)
            Configurations.NoteColors.Insert(index + 1, new BindableColor(255, 255, 255));
    }

    [RelayCommand]
    private void NoteColorRemove(BindableColor color) {
        if (CanRemove)
            Configurations.NoteColors.Remove(color);
    }

    private void OnConfigurationsPropertyChanged(object? sender, PropertyChangedEventArgs e) {
        if (e.PropertyName == nameof(Configurations.StartKey)) {
            StartWhiteKey = MIDIKeyHelper.WhiteKeyIndex(Configurations.StartKey);
        } else if (e.PropertyName == nameof(Configurations.EndKey)) {
            EndWhiteKey = MIDIKeyHelper.WhiteKeyIndex(Configurations.EndKey);
        }
    }
}