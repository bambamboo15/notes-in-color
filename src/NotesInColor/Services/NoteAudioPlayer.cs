/**
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

namespace NotesInColor.Services;

public class NoteAudioPlayer : INoteAudioPlayer {
    private int[] midiStreams = new int[1];
    private readonly int sfHandle;

    public double Volume {
        get => volume;
        set {
            Debug.Assert(value >= 0.0 && value <= 1.0);

            if (volume != value) {
                volume = value;
                Bass.GlobalStreamVolume = (int)(volume * 10000.0);
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

        midiStreams[0] = CreateStream();

        BassInfo bassInfo = new BassInfo();
        Bass.UpdatePeriod = 0;
        Bass.PlaybackBufferLength = bassInfo.MinBufferLength;
    }

    /**
     * Creates a stream and plays it.
     */
    private int CreateStream() {
        int midiStream = BassMidi.CreateStream(16, BassFlags.Float, 44100);
        if (midiStream == 0)
            throw new Exception("MIDI Stream creation failed");

        int v = BassMidi.StreamSetFonts(midiStream, [new MidiFont { Handle = sfHandle }], 1);
        if (v == 0)
            throw new Exception("Could not set SoundFont of stream");

        if (!Bass.ChannelPlay(midiStream, true))
            throw new Exception("Channel could not start playing");

        Bass.ChannelSetAttribute(midiStream, ChannelAttribute.Buffer, 0);
        Bass.ChannelSetAttribute(midiStream, ChannelAttribute.MidiVoices, 1000);

        return midiStream;
    }

    /**
     * Allocates another stream.
     */
    private void AllocateStream() {
        int[] midiStreamsNew = new int[midiStreams.Length + 1];

        for (int i = 0; i < midiStreams.Length; ++i)
            midiStreamsNew[i] = midiStreams[i];

        midiStreamsNew[^1] = CreateStream();
        midiStreams = midiStreamsNew;
    }

    /**
     * Gets a stream.
     */
    private int GetStream(int streamIndex) {
        while (streamIndex >= midiStreams.Length)
            AllocateStream();

        return midiStreams[streamIndex];
    }

    /**
     * Plays a note (or pauses).
     * 
     * Velocity: 0 = RELEASE, 1-127 = PRESS
     */
    public void Play(int noteNumber, int track, int velocity) {
        int stream = track / 15;
        int channel = track % 15;

        // channel 9 is drum channel, why?! >:(
        channel += (channel >= 9 ? 1 : 0);

        BassMidi.StreamEvent(GetStream(stream), channel, MidiEventType.Note, noteNumber | (velocity << 8));
    }

    public void AllNotesOff() {
        for (int note = 0; note < 128; ++note)
            for (int streamIndex = 0; streamIndex < midiStreams.Length; ++streamIndex)
                for (int channel = 0; channel < 16; ++channel)
                    if (channel != 9)
                        BassMidi.StreamEvent(midiStreams[streamIndex], channel, MidiEventType.Note, note);
    }

    public void Update() {
        Bass.Update(16);
    }
}