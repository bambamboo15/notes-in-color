/**
 *                    Notes in Color
 *            A project by Benyamin Bamburac
 * 
 * This project is a modern MIDI file player and visualizer
 * with some MIDI controller support.
 */

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using NotesInColor.Core;
using NotesInColor.Services;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;

namespace NotesInColor.ViewModel;

public partial class CommandsViewModel(
    IRequestMIDIFile RequestMIDIFile,
    INavigator Navigator,
    MIDIPlaythroughData MIDIPlaythroughData
) : ObservableObject {
    [ObservableProperty]
    private bool isOpenFileButtonEnabled = true;

    /**
     * Opens a file.
     */
    [RelayCommand]
    private async Task OpenFile() {
        if (!IsOpenFileButtonEnabled)
            return;

        IsOpenFileButtonEnabled = false;

        string? path = await RequestMIDIFile.OpenFile();
        if (path != null)
            MIDIPlaythroughData.Load(MIDIFileData.Parse(path));

        IsOpenFileButtonEnabled = true;
    }

    /**
     * Opens the settings page.
     */
    [RelayCommand]
    private void OpenSettings() {
        Navigator.NavigateTo(PageType.SettingsPage);
    }
}