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
using NotesInColor.Core;
using System.Collections;
using System.Threading.Tasks;

namespace NotesInColor {
    /**
     * What did I write here?
     */
    public sealed partial class RendererControl : UserControl {
        private readonly RendererViewModel ViewModel;

        private readonly float whiteKeyMaxHeight = 128.0f;            /* customizable */
        private readonly float whiteKeyGap = 2.0f;                   /* customizable */
        private readonly float blackKeyHeightRatio = 0.58f;          /* customizable */
        private readonly float blackKeyWidthRatio = 0.53f;           /* customizable */
        private readonly float signatureRedLineHeight = 3.75f;       /* customizable */

        private float whiteKeyHeight => whiteKeyMaxHeight * Math.Min(1.0f + Math.Max(height - 500.0f, 0.0f) * 0.002f, width * 0.0338f / ViewModel.WhiteKeyCount);

        private float width;
        private float height;

        private Color[] noteColorLookup = [
            Color.FromArgb(0xFF, 0x00, 0xBC, 0xD4),
            Color.FromArgb(0xFF, 0xF0, 0x94, 0x13), // start
            Color.FromArgb(0xFF, 0x42, 0x87, 0xF5),
            Color.FromArgb(0xFF, 0x4C, 0xAF, 0x50),
            Color.FromArgb(0xFF, 0xE9, 0x1E, 0x63),
            Color.FromArgb(0xFF, 0x9C, 0x27, 0xB0)
        ];

        private Color[] noteDarkColorLookup = [];
        private Color[] noteDarkerColorLookup = [];
        private CanvasLinearGradientBrush[] pianoWhiteKeyBrushes = [];
        private CanvasLinearGradientBrush[] pianoBlackKeyBrushes = [];

        private CanvasLinearGradientBrush whiteKeyBrush = null!;
        private CanvasLinearGradientBrush blackKeyBrush = null!;
        private CanvasLinearGradientBrush fadeBrush = null!;

        //private readonly AutoResetEvent renderRequest = new(false);
        //private readonly AutoResetEvent performUIThreadUpdate = new(false);
        //private bool availableToRequest = true;

        public RendererControl() {
            this.InitializeComponent();

            ViewModel = App.Current.Services.GetRequiredService<RendererViewModel>();

            CanvasDevice device = CanvasDevice.GetSharedDevice();
            CanvasSwapChain swapChain = new CanvasSwapChain(device, 100.0f, 100.0f, 96);
            canvasSwapChainPanel.SwapChain = swapChain;
            canvasSwapChainPanel.SizeChanged += OnCanvasResize;

            UpdateBrushes();

            Loaded += (_, _) => CompositionTarget.Rendering += Update;
            Unloaded += (_, _) => CompositionTarget.Rendering -= Update;

            /*
            Thread renderThread = new Thread(() => {
                performUIThreadUpdate.Set();

                while (true) {
                    renderRequest.WaitOne();
                    Draw();
                }
            }) {
                IsBackground = true,
                Priority = ThreadPriority.AboveNormal,
            };
            renderThread.Start();
            */
        }

        private void Update(object? sender, object e) {
            //if (availableToRequest && !ViewModel.RecomputingNotes) {
            //    availableToRequest = false;
            //    renderRequest.Set();
            //}
            Draw();
        }

        private void Draw() {
            //performUIThreadUpdate.Reset();

            using (CanvasDrawingSession ds = canvasSwapChainPanel.SwapChain.CreateDrawingSession(Colors.Transparent)) {
                using var layer = ds.CreateLayer(fadeBrush);

                //ds.FillRectangle(0, 0, width, height, Color.FromArgb(0xFF, 0x0F, 0x0F, 0x0F));
                ViewModel.AllObservableNotes((double start, double end, bool isWhiteKey, int colorKey, int track) =>
                    DrawNote(ds, (float)start, (float)end, isWhiteKey, colorKey, track));
                ViewModel.PianoForwarder((int[] whiteKeysPressed, int[] pseudoBlackKeysPressed) =>
                    DrawPiano(ds, whiteKeysPressed, pseudoBlackKeysPressed));
            }

            //performUIThreadUpdate.Set();

            //DispatcherQueue.TryEnqueue(() => {
                canvasSwapChainPanel.SwapChain.Present();
            //    availableToRequest = true;
            //});
        }

        /**
         * Draws a note.
         */
        private void DrawNote(CanvasDrawingSession ds, float start, float end, bool isWhiteKey, int colorKey, int track) {
            float whiteKeyWidthFull = width / ViewModel.WhiteKeyCount;
            float whiteKeyWidth = whiteKeyWidthFull - whiteKeyGap;
            float blackKeyHeight = whiteKeyHeight * blackKeyHeightRatio;
            float blackKeyWidth = whiteKeyWidthFull * blackKeyWidthRatio; 
            float space = height - whiteKeyHeight;

            float x = isWhiteKey ?
                (colorKey * whiteKeyWidthFull) :
                ((colorKey + 1) * whiteKeyWidthFull - blackKeyWidth * 0.5f - whiteKeyGap * 0.5f);
            float y = (1.0f - end) * space;
            float w = isWhiteKey ? whiteKeyWidth : blackKeyWidth;
            float h = (end - start) * space;
            float border = w * 0.1f;

            ds.FillRectangle(x, y, w, h, LookupNoteDarkerColor(track));
            ds.FillRectangle(x + border, y + border, w - 2 * border, h - 2 * border, isWhiteKey ? LookupNoteColor(track) : LookupNoteDarkColor(track));
            // ds.FillRoundedRectangle(x, y, w, h, Math.Min(h, 12.0f) / 2.0f, Math.Min(h, 12.0f) / 2.0f, isWhiteKey ? LookupNoteColor(track) : LookupNoteDarkColor(track));
        }

        /**
         * Draws the piano.
         */
        private void DrawPiano(CanvasDrawingSession ds, int[] whiteKeysPressed, int[] pseudoBlackKeysPressed) {
            float whiteKeyWidthFull = width / ViewModel.WhiteKeyCount;
            float whiteKeyWidth = whiteKeyWidthFull - whiteKeyGap;
            float blackKeyHeight = whiteKeyHeight * blackKeyHeightRatio;
            float blackKeyWidth = whiteKeyWidthFull * blackKeyWidthRatio;

            // draw piano background
            ds.FillRectangle(
                0.0f,
                height - whiteKeyHeight,
                width,
                whiteKeyHeight,
                Color.FromArgb(0xFF, 0x00, 0x00, 0x00)
            );

            // draw white keys
            for (int i = 0; i < ViewModel.WhiteKeyCount; ++i) {
                ds.FillRectangle(
                    i * whiteKeyWidthFull,
                    height - whiteKeyHeight,
                    whiteKeyWidth,
                    whiteKeyHeight,
                    LookupWhiteKeyBrush(whiteKeysPressed[i])
                );
            }

            // draw black keys
            for (int i = 0; i < ViewModel.PseudoBlackKeyCount; ++i) {
                if (ViewModel.BlackKeyPositions[i]) {
                    ds.FillRectangle(
                        (i + 1) * whiteKeyWidthFull - blackKeyWidth * 0.5f - whiteKeyGap * 0.5f,
                        height - whiteKeyHeight,
                        blackKeyWidth,
                        blackKeyHeight,
                        LookupBlackKeyBrush(pseudoBlackKeysPressed[i])
                    );
                }
            }

            // draw signature red line
            ds.FillRectangle(
                0.0f,
                height - whiteKeyHeight - signatureRedLineHeight,
                width,
                signatureRedLineHeight,
                Color.FromArgb(0xFF, 0x50, 0x00, 0x00)
            );
        }

        private void OnCanvasResize(object? sender, SizeChangedEventArgs e) {
            width = (float)canvasSwapChainPanel.ActualWidth + whiteKeyGap; // note for myself
            height = (float)canvasSwapChainPanel.ActualHeight;

            canvasSwapChainPanel.SwapChain.ResizeBuffers(e.NewSize);

            UpdateBrushes();
        }

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

        private static Color Darken(Color color, double factor = 0.75) {
            return Color.FromArgb(
                color.A,
                (byte)(color.R * factor),
                (byte)(color.G * factor),
                (byte)(color.B * factor));
        }

        private void UpdateBrushes() {
            whiteKeyBrush?.Dispose();
            blackKeyBrush?.Dispose();
            fadeBrush?.Dispose();
            foreach (var brush in pianoWhiteKeyBrushes)
                brush?.Dispose();
            foreach (var brush in pianoBlackKeyBrushes)
                brush?.Dispose();

            float blackKeyHeight = whiteKeyHeight * blackKeyHeightRatio;

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
        }
    }
}