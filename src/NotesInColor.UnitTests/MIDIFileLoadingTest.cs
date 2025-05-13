/**
 *                    Notes in Color
 *            A project by Benyamin Bamburac
 * 
 * This project is a modern MIDI file player and visualizer
 * with some MIDI controller support.
 */

using NotesInColor.Core;
using System.Diagnostics;

namespace NotesInColor.UnitTest;

[TestClass]
public sealed class MIDIFileLoadingTest {
    public readonly MIDIPlaythroughData MIDIPlaythroughData = new();

    /**
     * Test if the application can load all of the MIDI files in the MIDIFiles directory.
     */
    [TestMethod]
    public void TestMIDIFileLoading() {
        string directoryPath = Path.Combine(Directory.GetCurrentDirectory(), @"..\..\..\MIDIFiles");

        string[] midiFiles;

        try {
            midiFiles = Directory.GetFiles(directoryPath, "*.mid");
        } catch {
            Debug.WriteLine("Could not find MIDIFiles directory!");
            return;
        }

        foreach (var file in midiFiles) {
            try {
                MIDIPlaythroughData.Load(MIDIFileData.Parse(file));
                Console.WriteLine($"Successfully loaded: {file}");
            } catch (Exception ex) {
                Assert.Fail($"Failed to load {file}. Error: {ex.Message}");
            }
        }
    }
}