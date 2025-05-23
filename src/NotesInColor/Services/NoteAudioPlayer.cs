﻿/**
 *                    Notes in Color
 *            A project by Benyamin Bamburac
 * 
 * This project is a modern MIDI file player and visualizer
 * with some MIDI controller support.
 */

using ManagedBass;
using ManagedBass.Midi;
using System;
using System.IO;
using System.Threading.Tasks;
using NotesInColor.Services;
using System.Diagnostics;
using System.Threading.Channels;
using NotesInColor.Core;
using DryWetMidiCore = Melanchall.DryWetMidi.Core;

namespace NotesInColor.Services;

public class NoteAudioPlayer : INoteAudioPlayer {
    private readonly int sfHandle;
    private int midiStream;

    public double Volume {
        get => volume;
        set {
            Debug.Assert(value >= 0.0 && value <= 1.0);

            if (volume != value) {
                volume = value;
                Bass.GlobalStreamVolume = (int)(value * 10000.0);
            }
        }
    }
    public double volume;

    public NoteAudioPlayer() {
        if (!Bass.Init(-1, 44100, DeviceInitFlags.Latency, IntPtr.Zero))
            throw new Exception("BASS_Init failed");

        string fontPath = Path.Combine(AppContext.BaseDirectory, "arachno.sf2");
        sfHandle = BassMidi.FontInit(fontPath, FontInitFlags.MemoryMap);
        if (sfHandle == 0)
            throw new Exception("Failed to load SoundFont");

        if (!BassMidi.FontLoad(sfHandle, -1, -1))
            throw new Exception("BassMidi.FontLoad failed");

        var fontInfo = BassMidi.FontGetInfo(sfHandle);
        Debug.WriteLine($"SoundFont info: Presets = {fontInfo.Presets}");

        Bass.UpdatePeriod = 0;
        Bass.PlaybackBufferLength = new BassInfo().MinBufferLength;
    }

    /**
     * Creates a stream and plays it.
     */
    private int CreateStream() {
        int midiStream = BassMidi.CreateStream(16, BassFlags.Float, 44100);
        if (midiStream == 0)
            throw new Exception("MIDI Stream creation failed");

        int v = BassMidi.StreamSetFonts(midiStream, [new MidiFont {
            Handle = sfHandle,
            Preset = -1,
            Bank = 0
        }], 1);
        if (v == 0)
            throw new Exception("Could not set SoundFont of stream");

        if (!Bass.ChannelPlay(midiStream, true))
            throw new Exception("Channel could not start playing");

        Bass.ChannelSetAttribute(midiStream, ChannelAttribute.Buffer, 0);
        Bass.ChannelSetAttribute(midiStream, ChannelAttribute.MidiVoices, 1000);

        return midiStream;
    }

    public void Restart() {
        Bass.ChannelStop(midiStream);
        Bass.StreamFree(midiStream);
        midiStream = CreateStream();
    }

    public void Play(int noteNumber, int channel, int velocity) {
        BassMidi.StreamEvent(midiStream, channel, MidiEventType.Note, noteNumber | (velocity << 8));
    }

    /**
     * It seems like BASSMIDI has many controllers unsupported, and this code is weird at times, but it's the best I can do now,
     * and the project deadline is approaching. This is why some compositions sound weird. Sorry :(
     */
    public void PlayNonNoteTimedEvent(NonNoteTimedEventData nonNoteTimedEventData) {
        switch (nonNoteTimedEventData.TimedEvent.Event) {
            case DryWetMidiCore.ProgramChangeEvent programChangeEvent:
                BassMidi.StreamEvent(midiStream, programChangeEvent.Channel, MidiEventType.Program, programChangeEvent.ProgramNumber);
                break;
            case DryWetMidiCore.ControlChangeEvent controlChangeEvent:
                BassMidi.StreamEvent(midiStream, controlChangeEvent.Channel, (int)controlChangeEvent.ControlNumber switch {
                    0 => MidiEventType.Bank,
                    1 => MidiEventType.Modulation,
                    5 => MidiEventType.PortamentoTime,
                    7 => MidiEventType.Volume,
                    10 => MidiEventType.Pan,
                    11 => MidiEventType.Expression,
                    32 => MidiEventType.BankLSB,
                    64 => MidiEventType.Sustain,
                    65 => MidiEventType.Portamento,
                    66 => MidiEventType.Sostenuto,
                    67 => MidiEventType.Soft,
                    71 => MidiEventType.Resonance,
                    72 => MidiEventType.Release,
                    73 => MidiEventType.Attack,
                    74 => MidiEventType.CutOff,
                    75 => MidiEventType.Decay,
                    84 => MidiEventType.PortamentoNote,
                    91 => MidiEventType.Reverb,
                    93 => MidiEventType.Chorus,
                    94 => MidiEventType.UserFX,
                    120 => MidiEventType.SoundOff,
                    121 => MidiEventType.Reset,
                    123 => MidiEventType.NotesOff,
                    126 => MidiEventType.Mode,
                    _ => MidiEventType.Control,
                }, controlChangeEvent.ControlValue);
                break;
        }
    }

    public void ProgramChange(byte channel, byte programNumber) {
        Debug.WriteLine("program change");
        if (!BassMidi.StreamEvent(midiStream, channel, MidiEventType.Program, programNumber))
            throw new Exception("ProgramChange failed");
    }

    public void AllNotesOff() {
        for (int channel = 0; channel < 16; ++channel)
            BassMidi.StreamEvent(midiStream, channel, MidiEventType.NotesOff, 0);
    }

    public void Update() {
        Bass.Update(16);
    }
}