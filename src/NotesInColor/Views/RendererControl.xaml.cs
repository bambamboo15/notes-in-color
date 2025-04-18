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
using Microsoft.UI.Xaml.Media.Animation;

namespace NotesInColor {
    /**
     * What did I write here?
     */
    public sealed partial class RendererControl : UserControl {
        private readonly RendererViewModel ViewModel;

        private readonly float whiteKeyHeight = 96.0f;               /* customizable */
        private readonly float whiteKeyGap = 2.0f;                   /* customizable */
        private readonly float blackKeyHeightRatio = 0.58f;          /* customizable */
        private readonly float blackKeyWidthRatio = 0.53f;           /* customizable */
        private readonly float signatureRedLineHeight = 3.75f;       /* customizable */

        private float width;
        private float height;

        private CanvasLinearGradientBrush whiteKeyBrush = null!;
        private CanvasLinearGradientBrush blackKeyBrush = null!;
        private CanvasLinearGradientBrush whiteKeyPressedBrush = null!;
        private CanvasLinearGradientBrush blackKeyPressedBrush = null!;

        public RendererControl() {
            this.InitializeComponent();

            ViewModel = App.Current.Services.GetRequiredService<RendererViewModel>();

            CanvasDevice device = CanvasDevice.GetSharedDevice();
            CanvasSwapChain swapChain = new CanvasSwapChain(device, 100.0f, 100.0f, 96);
            canvasSwapChainPanel.SwapChain = swapChain;
            canvasSwapChainPanel.SizeChanged += OnCanvasResize;

            UpdateBrushes();

            CompositionTarget.Rendering += (_, _) => Draw();
        }

        private void Draw() {
            using (CanvasDrawingSession ds = canvasSwapChainPanel.SwapChain.CreateDrawingSession(Colors.Transparent)) {
                ViewModel.AllObservableNotes((double start, double end, bool isWhiteKey, int colorKey) =>
                    DrawNote(ds, (float)start, (float)end, isWhiteKey, colorKey));
                ViewModel.PianoForwarder((bool[] whiteKeysPressed, bool[] pseudoBlackKeysPressed) =>
                    DrawPiano(ds, whiteKeysPressed, pseudoBlackKeysPressed));
            }

            canvasSwapChainPanel.SwapChain.Present();
        }

        /**
         * Draws a note.
         */
        private void DrawNote(CanvasDrawingSession ds, float start, float end, bool isWhiteKey, int colorKey) {
            float whiteKeyWidthFull = width / ViewModel.WhiteKeyCount;
            float whiteKeyWidth = whiteKeyWidthFull - whiteKeyGap;
            float blackKeyHeight = whiteKeyHeight * blackKeyHeightRatio;
            float blackKeyWidth = whiteKeyWidthFull * blackKeyWidthRatio; 
            float space = height - whiteKeyHeight;

            if (isWhiteKey) {
                ds.FillRectangle(
                    colorKey * whiteKeyWidthFull,
                    (1.0f - end) * space,
                    whiteKeyWidth,
                    (end - start) * space,
                    Color.FromArgb(0xFF, 0xF0, 0x94, 0x13)
                );
            } else {
                ds.FillRectangle(
                    (colorKey + 1) * whiteKeyWidthFull - blackKeyWidth * 0.5f - whiteKeyGap * 0.5f,
                    (1.0f - end) * space,
                    blackKeyWidth,
                    (end - start) * space,
                    Color.FromArgb(0xFF, 0xA0, 0x60, 0x0D)
                );
            }
        }

        /**
         * Draws the piano.
         */
        private void DrawPiano(CanvasDrawingSession ds, bool[] whiteKeysPressed, bool[] pseudoBlackKeysPressed) {
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
                    whiteKeysPressed[i] ? whiteKeyPressedBrush : whiteKeyBrush
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
                        pseudoBlackKeysPressed[i] ? blackKeyPressedBrush : blackKeyBrush
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
            width = (float)canvasSwapChainPanel.ActualWidth;
            height = (float)canvasSwapChainPanel.ActualHeight;

            canvasSwapChainPanel.SwapChain.ResizeBuffers(e.NewSize);

            UpdateBrushes();
        }

        private void UpdateBrushes() {
            whiteKeyBrush?.Dispose();
            blackKeyBrush?.Dispose();
            whiteKeyPressedBrush?.Dispose();
            blackKeyPressedBrush?.Dispose();

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

            var whiteKeyPressed = new CanvasGradientStop[] {
                new() { Position = 0, Color = Color.FromArgb(0xFF, 0xF0, 0x94, 0x13) },
                new() { Position = 1, Color = Color.FromArgb(0xFF, 0xC8, 0x78, 0x10) }
            };
            whiteKeyPressedBrush = new CanvasLinearGradientBrush(canvasSwapChainPanel.SwapChain, whiteKeyPressed);
            whiteKeyPressedBrush.StartPoint = new Vector2(0.0f, height - whiteKeyHeight);
            whiteKeyPressedBrush.EndPoint = new Vector2(0.0f, height);

            var blackKeyPressed = new CanvasGradientStop[] {
                new() { Position = 0, Color = Color.FromArgb(0xFF, 0xA0, 0x60, 0x0D) },
                new() { Position = 1, Color = Color.FromArgb(0xFF, 0x88, 0x50, 0x0B) }
            };
            blackKeyPressedBrush = new CanvasLinearGradientBrush(canvasSwapChainPanel.SwapChain, blackKeyPressed);
            blackKeyPressedBrush.StartPoint = new Vector2(0.0f, height - whiteKeyHeight);
            blackKeyPressedBrush.EndPoint = new Vector2(0.0f, height - whiteKeyHeight + blackKeyHeight);
        }
    }
}