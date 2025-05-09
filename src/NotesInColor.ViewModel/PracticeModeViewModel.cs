/**
 *                    Notes in Color
 *            A project by Benyamin Bamburac
 * 
 * This project is a modern MIDI file player and visualizer
 * with some MIDI controller support.
 */

using CommunityToolkit.Mvvm.ComponentModel;
using NotesInColor.Core;
using NotesInColor.Shared;
using NotesInColor.Services;
using System.Diagnostics;
using System.ComponentModel;

namespace NotesInColor.ViewModel;

/**
 * Using-directive to shorten enterprise-grade interface names.
 */
using IDialogPresenter = ITryToInitiateAndCompleteTheProcessOfShowingTheDisplayOfSomethingThatCouldBeCalledWhateverContentDialog;

/**
 * Viewmodel that is entirely dedicated to practice mode.
 */
public partial class PracticeModeViewModel : ObservableObject {
    private readonly IDialogPresenter DialogPresenter;
    private readonly IInputDeviceManager InputDeviceManager;
    private readonly PracticeModeModel PracticeModeModel;
    private readonly MIDIPlaythroughData MIDIPlaythroughData;

    /**
     * Is the application currently in practice mode?
     */
    public bool PracticeMode => PracticeModeModel.PracticeMode;

    /**
     * Statistics
     */
    public int Score => (int)PracticeModeModel.Score;
    public float Accuracy => PracticeModeModel.Accuracy;
    public string Rank => PracticeModeModel.Rank;
    public int PerfectCount => PracticeModeModel.PerfectCount;
    public int GoodCount => PracticeModeModel.GoodCount;
    public int MissCount => PracticeModeModel.MissCount;

    public PracticeModeViewModel(IDialogPresenter DialogPresenter, IInputDeviceManager InputDeviceManager, PracticeModeModel PracticeModeModel, MIDIPlaythroughData MIDIPlaythroughData) {
        this.DialogPresenter = DialogPresenter;
        this.InputDeviceManager = InputDeviceManager;
        this.PracticeModeModel = PracticeModeModel;
        this.MIDIPlaythroughData = MIDIPlaythroughData;

        this.InputDeviceManager.InputMessageReceived += OnIDMInputMessageRecieved;
        this.PracticeModeModel.Feedback += OnPMMFeedback;
        this.MIDIPlaythroughData.PropertyChanged += OnMPDPropertyChanged;
    }

    /**
     * This method is triggered when an input message is recieved from an input device.
     */
    private void OnIDMInputMessageRecieved(object? sender, MIDIInputMessageArgs args) {
        PracticeModeModel.PushPlayerInput(args.Message);
    }

    /**
     * This event is fired when a new piece of feedback is determined.
     */
    public delegate void FeedbackDelegate(PracticeModeFeedback feedback);
    public event FeedbackDelegate? Feedback;

    /**
     * This method is triggered when a new piece of feedback is determined.
     */
    private void OnPMMFeedback(PracticeModeFeedback feedback) {
        Feedback?.Invoke(feedback);
    }

    /**
     * This function provides a utility that should be called via polling that is essentially
     * a discrete time step of determining feedback quite similarly to how a rhythm game works;
     * it determines hit/miss and other feedback based on the player's actions and dynamic
     * properties such as the timing of the notes. This only works when practice mode
     * is enabled; otherwise, it does nothing.
     */
    public void AssessPlayerInput() {
        PracticeModeModel.AssessPlayerInput();
    }

    /**
     * This method should be triggered whenever a property changes in MIDIPlaythroughData.
     * More specifically, this method checks when the end of a song is reached, at which
     * the relevant content dialog will be shown.
     */
    private async void OnMPDPropertyChanged(object? sender, PropertyChangedEventArgs args) {
        if (!PracticeMode)
            return;

        if (args.PropertyName == nameof(MIDIPlaythroughData.Progress)) {
            if (MIDIPlaythroughData.Progress >= MIDIPlaythroughData.Duration) {
                await Task.Delay(250); // may not be MVVM-friendly
                await DialogPresenter.ShowAsyncPracticeModeStatsContentDialog();
            }
        }
    }
}