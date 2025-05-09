/**
 *                    Notes in Color
 *            A project by Benyamin Bamburac
 * 
 * This project is a modern MIDI file player and visualizer
 * with some MIDI controller support.
 */

using NotesInColor.Shared;
using System.Diagnostics;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;

namespace NotesInColor.Shared {
    /**
     * Determines the type of feedback that is sent (see context below).
     */
    public enum PracticeModeFeedbackType {
        None = 0,
        Perfect = 1,
        Good = 2,
        Miss = 3
    };

    /**
     * A piece of feedback that is sent during practice mode (see context below).
     */
    public readonly record struct PracticeModeFeedback(int Key, PracticeModeFeedbackType FeedbackType);
}

namespace NotesInColor.Core {
    /**
     * An extension to MIDIPlaythroughData that is dedicated entirely to practice mode. I tried to make
     * this really detailed and understandable, because I like neat code.
     */
    public class PracticeModeModel {
        private readonly MIDIPlaythroughData MIDIPlaythroughData;

        private readonly int[] StartBottomIndices = new int[128];         /* lowest index of note with its bottom considered for assessment of player input, for each key */
        private readonly int[] EndBottomIndices = new int[128];           /* highest index of note with its bottom considered for assessment of player input, for each key */
        private readonly long PerfectVariability = 100000;                 /* the maximum allowed variability in microseconds for PERFECT */
        private readonly long GoodVariability = 300000;                   /* the maximum allowed variability in microseconds for GOOD */
        private readonly long MissVariability = 300000;                   /* the minumum variability in microseconds for MISS */

        public float Score { get; private set; } = 0.0f;
        public float MaximumScore => ComputeMaximumScore();
        public float Accuracy => ComputeAccuracy();
        public string Rank => ComputeRank();
        public int PerfectCount { get; private set; } = 0;
        public int GoodCount { get; private set; } = 0;
        public int MissCount { get; private set; } = 0;

        /**
         * Is the application currently in practice mode?
         */
        public bool PracticeMode => MIDIPlaythroughData.PracticeMode;

        public PracticeModeModel(MIDIPlaythroughData MIDIPlaythroughData) {
            this.MIDIPlaythroughData = MIDIPlaythroughData;

            this.MIDIPlaythroughData.OnLoaded += OnLoaded;
        }

        /**
         * This method is triggered when a composition is loaded. (Only does something in practice mode.)
         */
        private void OnLoaded() {
            if (!MIDIPlaythroughData.PracticeMode)
                return;

            // TODO: Make this cleaner MVVM, and enhance separation of concerns by moving
            //       practice mode code to here
            MIDIPlaythroughData.Tempo = 1.0;

            // Initialize stats
            Score = 0.0f;
            PerfectCount = 0;
            GoodCount = 0;
            MissCount = 0;

            // Determine the inclusive [StartIndex, EndIndex] range of notes for each key,
            // where all notes are within the time window [0, Variability], and the note at
            // StartIndex is of the current key.
            for (int key = 0; key < 128; ++key) {
                ref int StartIndex = ref StartBottomIndices[key], EndIndex = ref EndBottomIndices[key];

                for (StartIndex = 0; StartIndex < MIDIPlaythroughData.Notes.Length &&
                    MIDIPlaythroughData.Notes[StartIndex].NoteNumber != key; ++StartIndex) ;

                EndIndex = StartIndex;
                for (int endIndex = StartIndex; endIndex < MIDIPlaythroughData.Notes.Length &&
                            MIDIPlaythroughData.Notes[endIndex].Time <= MissVariability; ++endIndex)
                    if (MIDIPlaythroughData.Notes[EndIndex].NoteNumber == key)
                        EndIndex = endIndex;
            }
        }

        /**
         * This function should be called whenever player input is recieved. (Only does something in practice mode.)
         */
        public void PushPlayerInput(MIDIInputMessage inputMessage) {
            if (!MIDIPlaythroughData.PracticeMode)
                return;

            if (inputMessage is MIDIInputNoteOnMessage noteOnMessage) {
                int note = noteOnMessage.Note;

                ref int StartIndex = ref StartBottomIndices[note];

                // Give the user some freedom at the start and end points
                if (MIDIPlaythroughData.Progress < (-MissVariability / 1000000.0) || MIDIPlaythroughData.Progress >= MIDIPlaythroughData.Duration) {
                    return;
                }

                // Okay, there's like nothing there
                if (StartIndex >= MIDIPlaythroughData.Notes.Length) {
                    SendFeedback(note, PracticeModeFeedbackType.Miss);
                    return;
                }

                // Has the user pressed the note in Variability seconds around the note?
                long noteBottomTime = MIDIPlaythroughData.Notes[StartIndex].Time;
                long currentTime = MIDIPlaythroughData.CurrentMicroseconds;
                long variability = Math.Abs(noteBottomTime - currentTime);
                if (variability > MissVariability) {
                    SendFeedback(note, PracticeModeFeedbackType.Miss);
                    return;
                }

                if (variability < PerfectVariability) {
                    SendFeedback(note, PracticeModeFeedbackType.Perfect);
                } else if (variability < GoodVariability) {
                    SendFeedback(note, PracticeModeFeedbackType.Good);
                }

                // Alright, now that we have provided feedback for that note, it should no longer be considered.
                // We will more on to the next one with a matching key, if there is any.
                for (++StartIndex; StartIndex < MIDIPlaythroughData.Notes.Length &&
                    MIDIPlaythroughData.Notes[StartIndex].NoteNumber != note; ++StartIndex) ;
            } else if (inputMessage is MIDIInputNoteOffMessage noteOffMessage) {
                // nothing for now
            }
        }

        /**
         * This event is fired when a new piece of feedback is determined.
         */
        public delegate void FeedbackDelegate(PracticeModeFeedback feedback);
        public event FeedbackDelegate? Feedback;

        /**
         * This function provides a utility that should be called via polling that is essentially
         * a discrete time step of determining feedback quite similarly to how a rhythm game works;
         * it determines hit/miss and other feedback based on the player's actions and dynamic
         * properties such as the timing of the notes. This only runs when practice mode
         * is enabled; otherwise, it does nothing.
         */
        public void AssessPlayerInput() {
            if (!MIDIPlaythroughData.PracticeMode)
                return;

            for (int key = 0; key < 128; ++key) {
                ref int StartIndex = ref StartBottomIndices[key], EndIndex = ref EndBottomIndices[key];

                // Move forward the range of notes considered for player input assessment, from the bottommost note.
                // This ensures that only notes whose time is greater than the current time minus the variability
                // window are considered.
                while (StartIndex < MIDIPlaythroughData.Notes.Length &&
                        MIDIPlaythroughData.Notes[StartIndex].Time <= MIDIPlaythroughData.CurrentMicroseconds - MissVariability) {

                    // It seems like a note went below the screen, below any variability, so we should register
                    // that as a MISS.
                    if (MIDIPlaythroughData.Notes[StartIndex].NoteNumber == key) {
                        SendFeedback(key, PracticeModeFeedbackType.Miss);
                    }

                    ++StartIndex;
                }

                // Move the start of range forward until the note is of the key.
                while (StartIndex < MIDIPlaythroughData.Notes.Length &&
                        MIDIPlaythroughData.Notes[StartIndex].NoteNumber != key)
                    ++StartIndex;

                // Move forward the range of notes considered for player input assessment, from the topmost note.
                // This ensures that only notes whose time is greater than the current time plus the variability
                // window are considered.
                while (EndIndex < MIDIPlaythroughData.Notes.Length &&
                        MIDIPlaythroughData.Notes[EndIndex].Time <= MIDIPlaythroughData.CurrentMicroseconds + MissVariability)
                    ++EndIndex;
            }
        }

        /**
         * Send feedback.
         */
        private void SendFeedback(int key, PracticeModeFeedbackType feedbackType) {
            if (feedbackType == PracticeModeFeedbackType.Perfect) {
                Score += 1.0f;
                ++PerfectCount;
            } else if (feedbackType == PracticeModeFeedbackType.Good) {
                Score += 0.9f;
                ++GoodCount;
            } else if (feedbackType == PracticeModeFeedbackType.Miss) {
                ++MissCount;
            }

            Feedback?.Invoke(new PracticeModeFeedback(key, feedbackType));
        }

        /**
         * Computes the maximum score.
         */
        private float ComputeMaximumScore() =>
            MIDIPlaythroughData.Notes.Length;

        /**
         * Computes the accuracy.
         */
        private float ComputeAccuracy() =>
            1.0f - (float)MissCount / (MissCount + GoodCount + PerfectCount);

        /**
         * Computes the rank.
         */
        private string ComputeRank() =>
            (Score / MaximumScore) switch {
                >= 0.95f => "S",
                >= 0.9f => "A",
                >= 0.75f => "B",
                >= 0.6f => "C",
                >= 0.3f => "D",
                _ => "F",
            };
    }
}