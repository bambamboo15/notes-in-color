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
using System.Diagnostics;

namespace NotesInColor.ViewModel;

public partial class CommandsViewModel(
    IRequestMIDIFile RequestMIDIFile,
    INavigator Navigator,
    MIDIPlaythroughData MIDIPlaythroughData,
    INoteAudioPlayer NoteAudioPlayer,
    IInputDeviceManager InputDeviceManager,
    ITryToInitiateAndCompleteTheProcessOfShowingTheDisplayOfSomethingThatCouldBeCalledWhateverContentDialog TryToInitiateAndCompleteTheProcessOfShowingTheDisplayOfSomethingThatCouldBeCalledWhateverContentDialog
) : ObservableObject {
    [ObservableProperty]
    private bool isOpenFileButtonEnabled = true;

    [ObservableProperty]
    private bool isPracticeFileButtonEnabled = true;

    /**
     * Opens a file.
     */
    [RelayCommand]
    private async Task OpenFile() {
        if (!IsOpenFileButtonEnabled)
            return;

        IsOpenFileButtonEnabled = false;

        string? path = await RequestMIDIFile.OpenFile();
        if (path != null) {
            NoteAudioPlayer.AllNotesOff();
            try {
                MIDIPlaythroughData.Load(MIDIFileData.Parse(path));
            } catch {
                await TryToInitiateAndCompleteTheProcessOfShowingTheDisplayOfSomethingThatCouldBeCalledWhateverContentDialog.ShowSorrySomethingWentWrongFileLoadingDialog();
            }
        }

        IsOpenFileButtonEnabled = true;
    }

    /**
     * Opens a file but with practice mode.
     */
    [RelayCommand]
    private async Task PracticeFile() {
        if (!IsPracticeFileButtonEnabled)
            return;

        IsPracticeFileButtonEnabled = false;

        if (InputDeviceManager.InputDevices.Count > 0) {
            string? path = await RequestMIDIFile.OpenFile();
            if (path != null) {
                NoteAudioPlayer.AllNotesOff();
                try {
                    MIDIPlaythroughData.Load(MIDIFileData.Parse(path), true, false);
                } catch {
                    await TryToInitiateAndCompleteTheProcessOfShowingTheDisplayOfSomethingThatCouldBeCalledWhateverContentDialog.ShowSorrySomethingWentWrongFileLoadingDialog();
                } finally {
                    await TryToInitiateAndCompleteTheProcessOfShowingTheDisplayOfSomethingThatCouldBeCalledWhateverContentDialog.ShowAsyncJustBeforePracticeModeContentDialog();
                    MIDIPlaythroughData.Playing = true;
                }
            }
        } else {
            await TryToInitiateAndCompleteTheProcessOfShowingTheDisplayOfSomethingThatCouldBeCalledWhateverContentDialog.ShowAsyncSorryNoInputMIDIDeviceContentDialog();
        }

        IsPracticeFileButtonEnabled = true;
    }

    /**
     * Opens the Input Device Properties dialog.
     */
    [RelayCommand]
    private async Task OpenInputDeviceProperties() {
        await TryToInitiateAndCompleteTheProcessOfShowingTheDisplayOfSomethingThatCouldBeCalledWhateverContentDialog.ShowAsyncInputDevicePropertiesContentDialog();
    }

    /**
     * Opens the settings page.
     */
    [RelayCommand]
    private void OpenSettings() {
        NoteAudioPlayer.AllNotesOff();
        Navigator.NavigateTo(PageType.SettingsPage);
    }
}