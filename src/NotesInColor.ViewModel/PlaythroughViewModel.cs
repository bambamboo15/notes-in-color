/**
 *                    Notes in Color
 *            A project by Benyamin Bamburac
 * 
 * This project is a modern MIDI file player and visualizer
 * with some MIDI controller support.
 */

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NotesInColor.Core;
using NotesInColor.Services;
using System.ComponentModel.Design;
using System.Diagnostics;

namespace NotesInColor.ViewModel;

public partial class PlaythroughViewModel : ObservableObject {
    /**
     * Currently playing?
     */
    [ObservableProperty]
    private bool playing = false;

    /**
     * How much progress? (in seconds)
     */
    [ObservableProperty]
    private double progress = 0.0;

    /**
     * What is the duration of the composition? (in seconds)
     */
    [ObservableProperty]
    private double duration = 0.01;

    /**
     * Has something been loaded?
     */
    [ObservableProperty]
    private bool enabled = false;

    /**
     * Is this enabled, and there has been no practice mode?
     */
    public bool EnabledNoPracticeMode {
        get => enabledNoPracticeMode;
        private set => SetProperty(ref enabledNoPracticeMode, value);
    }
    private bool enabledNoPracticeMode;

    /**
     * The progress normalized between 0.0 and 1.0
     */
    [ObservableProperty]
    public double normalizedProgress = 0.0;

    /**
     * Progress remaining
     */
    private double ProgressRemaining => Duration - Progress;

    /**
     * The formatted progress as M:SS or MM:SS (BINDABLE)
     */
    public string FormattedProgress => FormattedTime(Math.Max(0.0f, Progress));

    /**
     * The formatted progress remaining as M:SS or MM:SS (BINDABLE)
     */
    public string FormattedProgressRemaining => FormattedTime(Math.Min(Duration, ProgressRemaining));

    // some random flag to avoid floating point imprecision errors causing infinite loops through
    // property getters and setters happening randomly causing a stack overflow
    private bool acceptchangesfromprogresstonormalizedprogressforonecallthankyou = true;

    /**
     * The note length slider value
     */
    [ObservableProperty]
    public double normalizedNoteLength = 0.5;

    /**
     * The normalized tempo
     */
    [ObservableProperty]
    public double normalizedTempo = 0.5;

    /**
     * The normalized volume
     */
    [ObservableProperty]
    public double normalizedVolume = 1.0;

    private readonly MIDIPlaythroughData MIDIPlaythroughData;
    private readonly INoteAudioPlayer NoteAudioPlayer;
    public PlaythroughViewModel(MIDIPlaythroughData MIDIPlaythroughData, INoteAudioPlayer NoteAudioPlayer) {
        this.MIDIPlaythroughData = MIDIPlaythroughData;
        this.NoteAudioPlayer = NoteAudioPlayer;

        // this was added because a unit test failed!
        Playing = MIDIPlaythroughData.Playing;
        Progress = MIDIPlaythroughData.Progress;
        Enabled = MIDIPlaythroughData.IsLoaded;
        Duration = MIDIPlaythroughData.Duration;
        EnabledNoPracticeMode = Enabled && !MIDIPlaythroughData.PracticeMode;

        // manual binding...
        MIDIPlaythroughData.PropertyChanged += (s, e) => {
            if (e.PropertyName == nameof(MIDIPlaythroughData.Playing)) {
                Playing = MIDIPlaythroughData.Playing;
            } else if (e.PropertyName == nameof(MIDIPlaythroughData.Progress)) {
                Progress = MIDIPlaythroughData.Progress;
            } else if (e.PropertyName == nameof(MIDIPlaythroughData.IsLoaded)) {
                Enabled = MIDIPlaythroughData.IsLoaded;
            } else if (e.PropertyName == nameof(MIDIPlaythroughData.Duration)) {
                Duration = MIDIPlaythroughData.Duration;
            } else if (e.PropertyName == nameof(MIDIPlaythroughData.PracticeMode)) {
                EnabledNoPracticeMode = Enabled && !MIDIPlaythroughData.PracticeMode;
            }
        };
    }

    partial void OnProgressChanged(double value) {
        acceptchangesfromprogresstonormalizedprogressforonecallthankyou = false;
        UpdateNormalizedProgress();
        acceptchangesfromprogresstonormalizedprogressforonecallthankyou = true;
        OnPropertyChanged(nameof(FormattedProgress));
        OnPropertyChanged(nameof(FormattedProgressRemaining));
        MIDIPlaythroughData.Jump(value);
    }

    partial void OnDurationChanged(double value) {
        UpdateNormalizedProgress();
        OnPropertyChanged(nameof(FormattedProgressRemaining));
    }

    partial void OnNormalizedProgressChanged(double value) {
        if (acceptchangesfromprogresstonormalizedprogressforonecallthankyou)
            Progress = NormalizedProgress * (Duration + MIDIPlaythroughData.WarmupTimeSeconds) - MIDIPlaythroughData.WarmupTimeSeconds;
    }

    private void UpdateNormalizedProgress() {
        NormalizedProgress = (Progress + MIDIPlaythroughData.WarmupTimeSeconds) / (Duration + MIDIPlaythroughData.WarmupTimeSeconds);
    }

    partial void OnPlayingChanged(bool value) {
        MIDIPlaythroughData.Playing = value;
    }

    partial void OnNormalizedNoteLengthChanged(double value) {
        MIDIPlaythroughData.ScreenHeightSeconds = Math.Pow(2.0, 1.0 + ((0.5 - value) * 4.0));
    }

    partial void OnNormalizedTempoChanged(double value) {
        MIDIPlaythroughData.Tempo = 0.1 * value * value + 1.85 * value + 0.05;
    }

    partial void OnNormalizedVolumeChanging(double value) {
        NoteAudioPlayer.Volume = value;
    }

    partial void OnEnabledChanged(bool value) {
        EnabledNoPracticeMode = Enabled && !MIDIPlaythroughData.PracticeMode;
    }

    private static string FormattedTime(double totalSeconds) =>
        string.Intern(totalSeconds >= 0.0
            ? $"{((int)Math.Floor(totalSeconds) / 60)}:{((int)Math.Floor(totalSeconds) % 60):D2}"
            : $"-{((int)Math.Ceiling(-totalSeconds) / 60)}:{((int)Math.Ceiling(-totalSeconds) % 60):D2}");

    /**
     * Advances playthrough by the specified number of seconds.
     */
    public void Next(double seconds) =>
        MIDIPlaythroughData.Next(seconds);

    /**
     * Toggle play/pause
     */
    [RelayCommand]
    private void TogglePlayPause() =>
        Playing = !Playing;
}