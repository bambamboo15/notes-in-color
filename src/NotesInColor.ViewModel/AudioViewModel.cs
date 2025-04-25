/**
 *                    Notes in Color
 *            A project by Benyamin Bamburac
 * 
 * This project is a modern MIDI file player and visualizer
 * with some MIDI controller support.
 */

using CommunityToolkit.Mvvm.ComponentModel;
using NotesInColor.Core;
using NotesInColor.Services;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Diagnostics;

namespace NotesInColor.ViewModel;

public partial class AudioViewModel : ObservableObject {
    private readonly INoteAudioPlayer NoteAudioPlayer;
    private readonly MIDIPlaythroughData MIDIPlaythroughData;

    private readonly BlockingCollection<(int note, int track, int velocity)> argsQueue
        = new(new ConcurrentQueue<(int, int, int)>());
    private readonly Thread audioThread0;
    private readonly Thread audioThread1;

    public AudioViewModel(INoteAudioPlayer NoteAudioPlayer, MIDIPlaythroughData MIDIPlaythroughData) {
        this.NoteAudioPlayer = NoteAudioPlayer;
        this.MIDIPlaythroughData = MIDIPlaythroughData;

        audioThread0 = new Thread(AudioThreadLoop) {
            IsBackground = true,
            Priority = ThreadPriority.Normal
        };
        audioThread0.Start();

        audioThread1 = new Thread(AudioThreadLoop) {
            IsBackground = true,
            Priority = ThreadPriority.Normal
        };
        audioThread1.Start();

        this.MIDIPlaythroughData.NoteChanged += OnNoteChanged;

        this.MIDIPlaythroughData.PropertyChanged += (object? sender, PropertyChangedEventArgs e) => {
            if (e.PropertyName == nameof(this.MIDIPlaythroughData.Progress)) {
                NoteAudioPlayer.Update();
            }
        };

        this.MIDIPlaythroughData.OnLoaded += NoteAudioPlayer.AllNotesOff;
    }

    private void OnNoteChanged(NoteData note, bool pressed) {
        if (MIDIPlaythroughData.Playing)
            argsQueue.Add((note.NoteNumber, note.Track, pressed ? note.Velocity : 0));
    }

    private void AudioThreadLoop() {
        foreach (var (note, track, velocity) in argsQueue.GetConsumingEnumerable())
            NoteAudioPlayer.Play(note, track, velocity);
    }
}