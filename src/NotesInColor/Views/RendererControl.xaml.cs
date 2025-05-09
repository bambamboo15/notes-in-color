/**
 *                    Notes in Color
 *            A project by Benyamin Bamburac
 * 
 * This project is a modern MIDI file player and visualizer
 * with some MIDI controller support.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Composition.SystemBackdrops;
using WinRT.Interop;
using Windows.UI.ViewManagement;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Threading;
using Windows.Storage.Pickers;
using Windows.UI;
using System.Numerics;
using NotesInColor.ViewModel;
using Microsoft.Extensions.DependencyInjection;
using Windows.Devices.PointOfService;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Brushes;
using System.Diagnostics;
using NotesInColor.Shared;
using System.Collections;
using System.Threading.Tasks;
using NotesInColor.Converters;
using Microsoft.Graphics.Canvas.Text;
using System.Collections.Specialized;
using Microsoft.Graphics.Canvas.Geometry;
using static System.Net.Mime.MediaTypeNames;
using System.Reflection;

namespace NotesInColor {
    /**
     * This class deals with rendering, in what hopefully is the simplest way possible.
     * 
     * This violates separation of concerns in many ways. This also violates some MVVM as well,
     * and has the added bonus of reducing testability.
     * 
     * IMPORTANT TODO:
     *        - Do not depend on MIDIKeyHelper
     *        - Do not depend on Configurations.StartKey and Configurations.EndKey
     */
    public sealed partial class RendererControl : UserControl {
        private readonly RendererViewModel RendererViewModel;
        private readonly PracticeModeViewModel PracticeModeViewModel;
        private readonly Configurations Configurations;
        private readonly ColorRGBToColorConverter ColorRGBToColorConverter;

        /**
         * Rendering properties
         */
        private float width;
        private float height;

        private float emptySpace;
        private float signatureRedLineHeight;

        private float whiteKeyHeight;
        private float whiteKeyWidth;
        private float whiteKeyWidthFull;
        private float blackKeyWidth;
        private float blackKeyHeight;

        private readonly float whiteKeyMaxHeight = 128.0f;
        private readonly float whiteKeyGap = 2.0f;
        private readonly float blackKeyHeightRatio = 0.58f;
        private readonly float blackKeyWidthRatio = 0.53f;

        /**
         * Precomputed note color, brush, and format properties
         */
        private Color[] noteColorLookup = [];
        private Color[] noteDarkColorLookup = [];
        private Color[] noteDarkerColorLookup = [];
        private CanvasLinearGradientBrush[] pianoWhiteKeyBrushes = [];
        private CanvasLinearGradientBrush[] pianoBlackKeyBrushes = [];

        private CanvasLinearGradientBrush whiteKeyBrush = null!;
        private CanvasLinearGradientBrush blackKeyBrush = null!;
        private CanvasLinearGradientBrush fadeBrush = null!;

        private CanvasTextFormat canvasTextFormat = null!;
        private CanvasTextFormat feedbackTextFormat = null!;

        private CanvasGeometry perfectFeedbackTextGeometry = null!;
        private CanvasGeometry goodFeedbackTextGeometry = null!;
        private CanvasGeometry missFeedbackTextGeometry = null!;

        /**
         * Stopwatches
         */
        private Stopwatch deltaTimeStopwatch = Stopwatch.StartNew();
        private double deltaTime;

        private Stopwatch fpsStopwatch = Stopwatch.StartNew();
        private int frameCount = 0;
        private int fps;

        /**
         * Easing variables
         */
        private float fpsEase = 0.0f;
        private float fpsEaseFactor = 0.1f;
        private bool fpsEaseUpwardsCondition => RendererViewModel.ShowFPS;

        private float pianoEase = 0.0f;
        private float pianoEaseFactor = 0.1f;
        private bool pianoEaseUpwardsCondition => RendererViewModel.ShowPiano;

        /**
         * Number of white keys
         */
        public int whiteKeyCount;

        /**
         * All on-screen feedback
         */
        public (PracticeModeFeedback feedback, double elapsedTime)[] visibleFeedback = Enumerable.Repeat((new PracticeModeFeedback(), 0.0), 128).ToArray();

        /**
         * Initialize everything
         */
        public RendererControl() {
            this.InitializeComponent();

            // dependency injection but it's not because of default constructor
            RendererViewModel = App.Current.Services.GetRequiredService<RendererViewModel>();
            PracticeModeViewModel = App.Current.Services.GetRequiredService<PracticeModeViewModel>();
            Configurations = App.Current.Services.GetRequiredService<Configurations>();
            ColorRGBToColorConverter = new ColorRGBToColorConverter();

            // initialize the swap chain
            CanvasDevice device = CanvasDevice.GetSharedDevice();
            CanvasSwapChain swapChain = new CanvasSwapChain(device, 100.0f, 100.0f, 96);
            canvasSwapChainPanel.SwapChain = swapChain;
            canvasSwapChainPanel.SizeChanged += OnCanvasResize;

            // trigger Update on every render, and load/unload it dynamically to avoid bugs
            Loaded += (_, _) => CompositionTarget.Rendering += Update;
            Unloaded += (_, _) => CompositionTarget.Rendering -= Update;

            // trigger OnNoteColorsCollectionChanged on every note color collection change, and load/unload it dynamically to avoid bugs
            Loaded += (_, _) => Configurations.NoteColors.CollectionChanged += OnNoteColorsCollectionChanged;
            Unloaded += (_, _) => Configurations.NoteColors.CollectionChanged -= OnNoteColorsCollectionChanged;

            // on every feedback recieved, do something
            PracticeModeViewModel.Feedback += OnFeedback;

            // initialize dimensions
            Loaded += (_, _) => UpdateDimensions(new(canvasSwapChainPanel.ActualWidth, canvasSwapChainPanel.ActualHeight));

            // initialize expensive properties
            UpdateNoteColorBrushFormatProperties();
        }

        /**
         * This function is intended to be called every rendering frame.
         */
        private void Update(object? sender, object args) {
            // stopwatches
            RecomputeDeltaTime();
            RecomputeFPS();

            // recompute other properties
            RecomputeProperties();

            // logic
            AssessPlayerInput();

            // drawing
            ApplyEasings();
            Draw();
        }

        /**
         * Recomputes the delta time.
         */
        private void RecomputeDeltaTime() {
            deltaTime = deltaTimeStopwatch.ElapsedMilliseconds / 1000.0;
            deltaTimeStopwatch.Restart();
        }

        /**
         * Recomputes the FPS.
         */
        private void RecomputeFPS() {
            ++frameCount;
            if (fpsStopwatch.ElapsedMilliseconds >= 1000) {
                fps = frameCount;
                frameCount = 0;

                fpsStopwatch.Restart();
            }
        }

        /**
         * This function assesses player input for rhythm game feedback.
         */
        private void AssessPlayerInput() {
            for (int i = 0; i < 128; ++i)
                visibleFeedback[i] = (
                    visibleFeedback[i].elapsedTime > 0.75f
                        ? new()
                        : visibleFeedback[i].feedback,
                    visibleFeedback[i].elapsedTime + deltaTime);

            PracticeModeViewModel.AssessPlayerInput();
        }

        /**
         * Recomputes various properties. Must be called every frame.
         */
        private void RecomputeProperties() {
            whiteKeyCount = MIDIKeyHelper.WhiteKeyIndex(Configurations.EndKey) - MIDIKeyHelper.WhiteKeyIndex(Configurations.StartKey);

            float oldWhiteKeyWidthFull = whiteKeyWidthFull;

            whiteKeyHeight = (whiteKeyMaxHeight * Math.Min(1.0f + Math.Max(height - 500.0f, 0.0f) * 0.002f, width * 0.0338f / whiteKeyCount));
            whiteKeyWidthFull = width / whiteKeyCount;
            whiteKeyWidth = whiteKeyWidthFull - whiteKeyGap;
            blackKeyHeight = whiteKeyHeight * blackKeyHeightRatio;
            blackKeyWidth = whiteKeyWidthFull * blackKeyWidthRatio;

            signatureRedLineHeight = pianoEase * 3.75f;
            emptySpace = Math.Clamp((1.0f - pianoEase) * (whiteKeyHeight + 16) + height - whiteKeyHeight, 0.0f, height);

            // If the white key width changed, then only then do all of these allocations
            if (oldWhiteKeyWidthFull != whiteKeyWidthFull) {
                feedbackTextFormat?.Dispose();
                feedbackTextFormat = new() { FontFamily = "Segoe UI Variable", FontSize = whiteKeyWidthFull / 1.4f, FontWeight = new Windows.UI.Text.FontWeight(500), HorizontalAlignment = CanvasHorizontalAlignment.Center, VerticalAlignment = CanvasVerticalAlignment.Center };

                perfectFeedbackTextGeometry?.Dispose();
                perfectFeedbackTextGeometry = CanvasGeometry.CreateText(new(canvasSwapChainPanel.SwapChain, "Perfect!", feedbackTextFormat, 256, 0));

                missFeedbackTextGeometry?.Dispose();
                missFeedbackTextGeometry = CanvasGeometry.CreateText(new(canvasSwapChainPanel.SwapChain, "Miss!", feedbackTextFormat, 256, 0));

                goodFeedbackTextGeometry?.Dispose();
                goodFeedbackTextGeometry = CanvasGeometry.CreateText(new(canvasSwapChainPanel.SwapChain, "Good!", feedbackTextFormat, 256, 0));
            }
        }

        /**
         * Apply one discrete time step of various easings. This depends on deltaTime.
         */
        private void ApplyEasings() {
            fpsEase = Math.Clamp((1.0f - fpsEaseFactor) * fpsEase + (fpsEaseUpwardsCondition ? fpsEaseFactor : 0.0f), 0.0f, 1.0f);
            pianoEase = Math.Clamp((1.0f - pianoEaseFactor) * pianoEase + (pianoEaseUpwardsCondition ? pianoEaseFactor : 0.0f), 0.0f, 1.0f);
        }

        /**
         * Draws everything.
         */
        private void Draw() {
            using (CanvasDrawingSession ds = canvasSwapChainPanel.SwapChain.CreateDrawingSession(Colors.Transparent)) {
                // upper gradient
                using var layer = ds.CreateLayer(fadeBrush);

                // draw all white notes
                RendererViewModel.AllObservableNotes((double start, double end, int key, int track) => {
                    if (MIDIKeyHelper.IsWhiteKey(key))
                        DrawNote(ds, (float)start, (float)end, key, track);
                });

                // draw all black notes
                RendererViewModel.AllObservableNotes((double start, double end, int key, int track) => {
                    if (MIDIKeyHelper.IsBlackKey(key))
                        DrawNote(ds, (float)start, (float)end, key, track);
                });

                // draw feedback
                DrawFeedback(ds);

                // draw piano
                RendererViewModel.ComputeKeysPressed();
                DrawPiano(ds);

                // draw FPS
                DrawFPS(ds);
            }

            canvasSwapChainPanel.SwapChain.Present();
        }

        /**
         * Draws a note.
         */
        private void DrawNote(CanvasDrawingSession ds, float start, float end, int key, int track) {
            // don't draw unnecessary notes
            if (key < Configurations.StartKey || key >= Configurations.EndKey)
                return;

            bool isWhiteKey = MIDIKeyHelper.IsWhiteKey(key);
            int colorKey = isWhiteKey ? (
                MIDIKeyHelper.WhiteKeyIndex(key) -
                MIDIKeyHelper.WhiteKeyIndex(Configurations.StartKey)
            ) : (
                MIDIKeyHelper.PseudoBlackKeyIndex(key - 1) -
                MIDIKeyHelper.PseudoBlackKeyIndex(Configurations.StartKey)
            );

            float x = isWhiteKey ?
                (colorKey * whiteKeyWidthFull) :
                ((colorKey + 1) * whiteKeyWidthFull - blackKeyWidth * 0.5f - whiteKeyGap * 0.5f);
            float y = (1.0f - end) * emptySpace;
            float w = isWhiteKey ? whiteKeyWidth : blackKeyWidth;
            float h = (end - start) * emptySpace;
            float border = w * 0.1f;

            ds.FillRectangle(x, y, w, h, LookupNoteDarkerColor(track));
            ds.FillRectangle(x + border, y + border, w - 2 * border, h - 2 * border, isWhiteKey ? LookupNoteColor(track) : LookupNoteDarkColor(track));
        }

        /**
         * Draws the piano.
         */
        private void DrawPiano(CanvasDrawingSession ds) {
            // draw piano background
            ds.FillRectangle(
                0.0f,
                emptySpace,
                width,
                whiteKeyHeight,
                Color.FromArgb(0xFF, 0x00, 0x00, 0x00) // TRANSPARENT FOR NOW
            );

            // draw white keys
            for (int i = Configurations.StartKey; i < Configurations.EndKey; ++i) {
                if (MIDIKeyHelper.IsWhiteKey(i)) {
                    int whiteKeyOffsetIndex =
                        MIDIKeyHelper.WhiteKeyIndex(i) -
                        MIDIKeyHelper.WhiteKeyIndex(Configurations.StartKey);

                    ds.FillRectangle(
                        whiteKeyOffsetIndex * whiteKeyWidthFull,
                        emptySpace,
                        whiteKeyWidth,
                        whiteKeyHeight,
                        LookupWhiteKeyBrush(RendererViewModel.KeysPressed[i])
                    );
                }
            }

            // draw black keys
            for (int i = Configurations.StartKey; i < Configurations.EndKey - 1; ++i) {
                if (MIDIKeyHelper.IsBlackKey(i)) {
                    int blackKeyOffsetIndex =
                        MIDIKeyHelper.PseudoBlackKeyIndex(i - 1) -
                        MIDIKeyHelper.PseudoBlackKeyIndex(Configurations.StartKey);

                    ds.FillRectangle(
                        (blackKeyOffsetIndex + 1) * whiteKeyWidthFull - blackKeyWidth * 0.5f - whiteKeyGap * 0.5f,
                        emptySpace,
                        blackKeyWidth,
                        blackKeyHeight,
                        LookupBlackKeyBrush(RendererViewModel.KeysPressed[i])
                    );
                }
            }

            // draw signature red line
            ds.FillRectangle(
                0.0f,
                emptySpace - signatureRedLineHeight,
                width,
                signatureRedLineHeight,
                Color.FromArgb(0xFF, 0x50, 0x00, 0x00)
            );
        }

        /**
         * Draws the rhythm game feedback.
         */
        private void DrawFeedback(CanvasDrawingSession ds) {
            float y = emptySpace - signatureRedLineHeight - 16;

            for (int i = 0; i < 128; ++i) {
                var time = (float)visibleFeedback[i].elapsedTime;
                var key = visibleFeedback[i].feedback.Key;
                var type = visibleFeedback[i].feedback.FeedbackType;

                if (type == PracticeModeFeedbackType.None)
                    continue;

                bool isWhiteKey = MIDIKeyHelper.IsWhiteKey(key);
                int colorKey = isWhiteKey ? (
                    MIDIKeyHelper.WhiteKeyIndex(key) -
                    MIDIKeyHelper.WhiteKeyIndex(Configurations.StartKey)
                ) : (
                    MIDIKeyHelper.PseudoBlackKeyIndex(key - 1) -
                    MIDIKeyHelper.PseudoBlackKeyIndex(Configurations.StartKey)
                );

                float x = isWhiteKey
                    ? (0.5f + colorKey) * whiteKeyWidthFull - 0.5f * whiteKeyGap
                    : (1.0f + colorKey) * whiteKeyWidthFull - 0.5f * whiteKeyGap;

                float dy = 0;
                if (time < 0.2f) {
                    dy = (time - 0.1f) / 0.1f;
                    dy = (dy * dy - 1.0f) * 4.0f;
                } else if (time > 0.7f) {
                    dy = (time - 0.7f) / 0.05f;
                    dy *= dy * 16.0f;
                }

                float hw = 128.0f;

                Color color = type switch {
                    PracticeModeFeedbackType.Perfect => Color.FromArgb(255, 61, 177, 255),
                    PracticeModeFeedbackType.Good => Color.FromArgb(255, 0, 255, 0),
                    PracticeModeFeedbackType.Miss => Color.FromArgb(255, 255, 0, 0),
                    _ => throw new NotImplementedException("bug")
                };

                CanvasGeometry geometry = type switch {
                    PracticeModeFeedbackType.Perfect => perfectFeedbackTextGeometry,
                    PracticeModeFeedbackType.Good => goodFeedbackTextGeometry,
                    PracticeModeFeedbackType.Miss => missFeedbackTextGeometry,
                    _ => throw new NotImplementedException("bug")
                };

                ds.DrawGeometry(
                    geometry,
                    x - hw,
                    y + dy,
                    Colors.Black,
                    4
                );
                ds.FillGeometry(
                    geometry,
                    x - hw,
                    y + dy,
                    color
                );
            }
        }

        /**
         * Draws the FPS.
         */
        private void DrawFPS(CanvasDrawingSession ds) {
            //ds.FillRoundedRectangle(10, 25 - (fpsEase - 1.0f) * 4.0f, 150, 40, 4, 4, Color.FromArgb((byte)(fpsEase * 64.0f), 0, 0, 0));
            //ds.DrawText($"FPS: {fps}", 20, 30 - (fpsEase - 1.0f) * 4.0f, Color.FromArgb((byte)(fpsEase * 255.0f), 255, 255, 255), canvasTextFormat);

            if (fpsEase < 0.01f)
                return;

            // create geometry
            using var layout = new CanvasTextLayout(ds, string.Intern($"FPS: {fps}"), canvasTextFormat, 512, 0);
            using var geometry = CanvasGeometry.CreateText(layout);

            ds.DrawGeometry(geometry, 20, 30 - (fpsEase - 1.0f) * 4.0f, Color.FromArgb((byte)(fpsEase * 255.0f), 0, 0, 0), 2);
            ds.FillGeometry(geometry, 20, 30 - (fpsEase - 1.0f) * 4.0f, Color.FromArgb((byte)(fpsEase * 255.0f), 255, 255, 255));
        }

        /**
         * Whenever feedback is obtained...
         */
        private void OnFeedback(PracticeModeFeedback feedback) {
            visibleFeedback[feedback.Key] = (feedback, 0.0);
        }

        /**
         * Updates necessary things on canvas resize. This recomputes width- and height-based properties.
         */
        private void OnCanvasResize(object? sender, SizeChangedEventArgs args) {
            UpdateDimensions(args.NewSize);
            UpdateNoteColorBrushFormatProperties();
        }

        /**
         * Updates dimensions.
         */
        private void UpdateDimensions(Size newSize) {
            // add the whiteKeyGap here to eliminate the rightmost gap
            width = (float)newSize.Width + whiteKeyGap;
            height = (float)newSize.Height;

            canvasSwapChainPanel.SwapChain.ResizeBuffers(newSize);
        }

        /**
         * This function should be invoked when anything about the note colors changes.
         */
        private void OnNoteColorsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs args) {
            UpdateNoteColorBrushFormatProperties();
        }

        /**
         * Note color lookup functions.
         */
        private Color LookupNoteColor(int track) =>
            noteColorLookup[track % noteColorLookup.Length];

        private Color LookupNoteDarkColor(int track) =>
            noteDarkColorLookup[track % noteDarkColorLookup.Length];

        private Color LookupNoteDarkerColor(int track) =>
            noteDarkerColorLookup[track % noteDarkerColorLookup.Length];

        private CanvasLinearGradientBrush LookupWhiteKeyBrush(int track) =>
            track == -1 ? whiteKeyBrush : pianoWhiteKeyBrushes[track % pianoWhiteKeyBrushes.Length];

        private CanvasLinearGradientBrush LookupBlackKeyBrush(int track) =>
            track == -1 ? blackKeyBrush : pianoBlackKeyBrushes[track % pianoBlackKeyBrushes.Length];

        /**
         * Darkens a color.
         */
        private static Color Darken(Color color, double factor = 0.75) {
            return Color.FromArgb(
                color.A,
                (byte)(color.R * factor),
                (byte)(color.G * factor),
                (byte)(color.B * factor));
        }

        /**
         * Updates note colors, brushes, and formats.
         */
        private void UpdateNoteColorBrushFormatProperties() {
            whiteKeyBrush?.Dispose();
            blackKeyBrush?.Dispose();
            fadeBrush?.Dispose();
            foreach (var brush in pianoWhiteKeyBrushes)
                brush?.Dispose();
            foreach (var brush in pianoBlackKeyBrushes)
                brush?.Dispose();
            canvasTextFormat?.Dispose();

            var whiteKey = new CanvasGradientStop[] {
                new() { Position = 0, Color = Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF) },
                new() { Position = 1, Color = Color.FromArgb(0xFF, 0xD5, 0xD5, 0xD5) }
            };
            whiteKeyBrush = new CanvasLinearGradientBrush(canvasSwapChainPanel.SwapChain, whiteKey);
            whiteKeyBrush.StartPoint = new Vector2(0.0f, height - whiteKeyHeight);
            whiteKeyBrush.EndPoint = new Vector2(0.0f, height);

            var blackKey = new CanvasGradientStop[] {
                new() { Position = 0, Color = Color.FromArgb(0xFF, 0x44, 0x44, 0x44) },
                new() { Position = 1, Color = Color.FromArgb(0xFF, 0x02, 0x02, 0x02) }
            };
            blackKeyBrush = new CanvasLinearGradientBrush(canvasSwapChainPanel.SwapChain, blackKey);
            blackKeyBrush.StartPoint = new Vector2(0.0f, height - whiteKeyHeight);
            blackKeyBrush.EndPoint = new Vector2(0.0f, height - whiteKeyHeight + blackKeyHeight);

            fadeBrush = new CanvasLinearGradientBrush(canvasSwapChainPanel.SwapChain, [
                    new CanvasGradientStop { Position = 0f, Color = Color.FromArgb(255, 0, 0, 0) },
                    new CanvasGradientStop { Position = 1f, Color = Color.FromArgb(0, 0, 0, 0) },
                ]) {
                StartPoint = new Vector2(0, 32),
                EndPoint = new Vector2(0, 0)
            };

            noteColorLookup = new Color[Configurations.NoteColors.Count];
            for (int i = 0; i < Configurations.NoteColors.Count; ++i) {
                noteColorLookup[i] = (Color)ColorRGBToColorConverter.Convert(Configurations.NoteColors[i].ColorRGB, null!, null!, null!);
            }

            noteDarkColorLookup = new Color[noteColorLookup.Length];
            noteDarkerColorLookup = new Color[noteColorLookup.Length];
            pianoWhiteKeyBrushes = new CanvasLinearGradientBrush[noteColorLookup.Length];
            pianoBlackKeyBrushes = new CanvasLinearGradientBrush[noteColorLookup.Length];
            for (int i = 0; i < noteColorLookup.Length; ++i) {
                noteDarkColorLookup[i] = Darken(noteColorLookup[i], 0.75);
                noteDarkerColorLookup[i] = Darken(noteColorLookup[i], 0.4);

                pianoWhiteKeyBrushes[i] = new CanvasLinearGradientBrush(canvasSwapChainPanel.SwapChain, [
                    new() { Position = 0, Color = noteColorLookup[i] },
                    new() { Position = 1, Color = Darken(noteColorLookup[i], 0.8) }
                ]);
                pianoWhiteKeyBrushes[i].StartPoint = new Vector2(0.0f, height - whiteKeyHeight);
                pianoWhiteKeyBrushes[i].EndPoint = new Vector2(0.0f, height);

                pianoBlackKeyBrushes[i] = new CanvasLinearGradientBrush(canvasSwapChainPanel.SwapChain, [
                    new() { Position = 0, Color = noteDarkColorLookup[i] },
                    new() { Position = 1, Color = Darken(noteDarkColorLookup[i], 0.8) }
                ]);
                pianoBlackKeyBrushes[i].StartPoint = new Vector2(0.0f, height - whiteKeyHeight);
                pianoBlackKeyBrushes[i].EndPoint = new Vector2(0.0f, height - whiteKeyHeight + blackKeyHeight);
            }

            canvasTextFormat = new() { FontFamily = "Segoe UI Variable", FontSize = 20, FontWeight = new Windows.UI.Text.FontWeight(500) };
        }
    }
}