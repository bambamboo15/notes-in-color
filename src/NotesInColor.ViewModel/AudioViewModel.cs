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

public record class ArgsQueueArgs(int NoteNumber, int Channel, int Velocity) { };

public class ArgsQueueArgsPool {
    private readonly ConcurrentQueue<ArgsQueueArgs> _pool = new ConcurrentQueue<ArgsQueueArgs>();

    public ArgsQueueArgs Rent(int noteNumber, int channel, int velocity) {
        if (_pool.TryDequeue(out var args)) {
            return args with { NoteNumber = noteNumber, Channel = channel, Velocity = velocity };
        } else {
            return new ArgsQueueArgs(noteNumber, channel, velocity);
        }
    }

    public void Return(ArgsQueueArgs args) {
        _pool.Enqueue(args);
    }
}

public partial class AudioViewModel : ObservableObject {
    private readonly INoteAudioPlayer NoteAudioPlayer;
    private readonly MIDIPlaythroughData MIDIPlaythroughData;
    private readonly ArgsQueueArgsPool ArgsQueueArgsPool;

    private readonly BlockingCollection<ArgsQueueArgs> argsQueue
        = new(new ConcurrentQueue<ArgsQueueArgs>());
    private readonly Thread audioThread0;
    private readonly Thread audioThread1;

    // this is unused for now
    public bool PlaySounds { get; set; } = true;

    public AudioViewModel(INoteAudioPlayer NoteAudioPlayer, MIDIPlaythroughData MIDIPlaythroughData) {
        this.NoteAudioPlayer = NoteAudioPlayer;
        this.MIDIPlaythroughData = MIDIPlaythroughData;
        this.ArgsQueueArgsPool = new ArgsQueueArgsPool();

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

        this.MIDIPlaythroughData.NoteChanged += OnMPDNoteChanged;
        this.MIDIPlaythroughData.NonNoteTimedEvent += OnMPDNonNoteTimedEvent;
        this.MIDIPlaythroughData.PropertyChanged += OnMPDPropertyChanged;
        this.MIDIPlaythroughData.OnLoaded += OnMPDLoaded;
    }

    private void OnMPDNoteChanged(NoteData note, bool pressed) {
        if (MIDIPlaythroughData.Playing)
            argsQueue.Add(ArgsQueueArgsPool.Rent(note.NoteNumber, note.Channel, pressed ? note.Velocity : 0));
    }

    private void OnMPDNonNoteTimedEvent(NonNoteTimedEventData nonNoteTimedEventData) {
        NoteAudioPlayer.PlayNonNoteTimedEvent(nonNoteTimedEventData);
    }

    private void OnMPDPropertyChanged(object? sender, PropertyChangedEventArgs e) {
        if (e.PropertyName == nameof(this.MIDIPlaythroughData.Progress)) {
            NoteAudioPlayer.Update();
        } else if (e.PropertyName == nameof(this.MIDIPlaythroughData.Playing)) {
            if (!this.MIDIPlaythroughData.Playing)
                NoteAudioPlayer.AllNotesOff();
        }
    }

    private void OnMPDLoaded() {
        NoteAudioPlayer.Restart();
        NoteAudioPlayer.AllNotesOff();
    }

    private void AudioThreadLoop() {
        foreach (ArgsQueueArgs args in argsQueue.GetConsumingEnumerable()) {
            if (args.Velocity == 0 || PlaySounds)
                NoteAudioPlayer.Play(args.NoteNumber, args.Channel, args.Velocity);
            ArgsQueueArgsPool.Return(args);
        }
    }
}