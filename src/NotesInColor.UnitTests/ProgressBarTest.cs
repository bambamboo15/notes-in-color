/**
 *                    Notes in Color
 *            A project by Benyamin Bamburac
 * 
 * This project is a modern MIDI file player and visualizer
 * with some MIDI controller support.
 */

using NotesInColor.ViewModel;
using NotesInColor.Core;
using NotesInColor.Services;
using Melanchall.DryWetMidi.Core;
using Moq;
using System.Diagnostics;

namespace NotesInColor.UnitTest;

[TestClass]
public sealed class ProgressBarTest {
    private readonly MIDIPlaythroughData MIDIPlaythroughData;
    private readonly INoteAudioPlayer NoteAudioPlayer;
    private readonly PlaythroughViewModel PlaythroughViewModel;

    public ProgressBarTest() {
        MIDIPlaythroughData = new MIDIPlaythroughData();
        NoteAudioPlayer = Mock.Of<INoteAudioPlayer>();
        PlaythroughViewModel = new PlaythroughViewModel(MIDIPlaythroughData, NoteAudioPlayer);
    }

    /**
     * Loads the MIDIPlaythroughData with a 1 minute blank MIDI file.
     */
    public void LoadOneMinuteBlankMIDIFile() {
        var MidiFile = new MidiFile(new TrackChunk(
            new SetTempoEvent(500000) { DeltaTime = 0 },
            new TextEvent("Placeholder") { DeltaTime = 480 * 120 }
        )) { TimeDivision = new TicksPerQuarterNoteTimeDivision(480) };

        try {
            MIDIPlaythroughData.Load(MIDIFileData.Parse(MidiFile, "Placeholder"));
        } catch {
            Assert.Fail("Loading a one minute blank MIDI file failed...");
        }
    }

    /**
     * Tests the progress bar to see if the reported progress matches the MIDIPlaythroughData progress
     */
    [TestMethod]
    public void TestProgressBarElapsedTime() {
        LoadOneMinuteBlankMIDIFile();
        MIDIPlaythroughData.Jump(0.0);

        // test elapsed time
        for (int i = 0; i < 10; ++i) {
            PlaythroughViewModel.Next(6.0);

            // make sure it is accurate
            Assert.AreEqual(PlaythroughViewModel.Progress, MIDIPlaythroughData.Progress, 0.00001);
        }
    }

    /**
     * Tests the progress bar to see if jumping works
     */
    [TestMethod]
    public void TestProgressBarJump() {
        LoadOneMinuteBlankMIDIFile();

        // "move the slider" to the left, middle, and right
        foreach (double location in new Span<double>([0.0, 0.5, 1.0])) {
            PlaythroughViewModel.NormalizedProgress = location;

            // calculate actual progress
            double calculatedProgress =
                location * (MIDIPlaythroughData.Duration + MIDIPlaythroughData.WarmupTimeSeconds) -
                MIDIPlaythroughData.WarmupTimeSeconds;
            Assert.AreEqual(MIDIPlaythroughData.Progress, calculatedProgress, 0.01);
        }
    }
}