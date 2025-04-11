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

namespace NotesInColor {
    /**
     * What did I write here?
     */
    public sealed partial class RendererControl : UserControl {
        public RendererViewModel ViewModel { get; }

        public RendererControl() {
            this.InitializeComponent();
            ViewModel = App.Current.Services.GetService<RendererViewModel>()!;
        }

#if FALSE
        private void DrawPiano(CanvasControl sender, CanvasDrawingSession ds) {
            float width = (float)sender.ActualWidth;
            float height = (float)sender.ActualHeight;

            /* customizable */ int whiteKeys = 52;
            /* customizable */ float whiteKeyHeight = 96.0f;
            /* customizable */ float whiteKeyGap = 2.0f;
            /* customizable */ float blackKeyHeightRatio = 0.58f;
            /* customizable */ float blackKeyWidthRatio = 0.53f;
            /* customizable */ float signatureRedLineHeight = 3.75f;

            float whiteKeyWidthFull = width / whiteKeys;
            float whiteKeyWidth = whiteKeyWidthFull - whiteKeyGap;
            float blackKeyHeight = whiteKeyHeight * blackKeyHeightRatio;
            float blackKeyWidth = whiteKeyWidthFull * blackKeyWidthRatio;

            var whiteKey = new CanvasGradientStop[] {
                new() { Position = 0, Color = Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF) },
                new() { Position = 1, Color = Color.FromArgb(0xFF, 0xD8, 0xD8, 0xD8) }
            };
            using var whiteKeyBrush = new CanvasLinearGradientBrush(sender, whiteKey);
            whiteKeyBrush.StartPoint = new System.Numerics.Vector2(0.0f, height - whiteKeyHeight);
            whiteKeyBrush.EndPoint = new System.Numerics.Vector2(0.0f, height);

            var blackKey = new CanvasGradientStop[] {
                new() { Position = 0, Color = Color.FromArgb(0xFF, 0x55, 0x55, 0x55) },
                new() { Position = 1, Color = Color.FromArgb(0xFF, 0x02, 0x02, 0x02) }
            };
            using var blackKeyBrush = new CanvasLinearGradientBrush(sender, blackKey);
            blackKeyBrush.StartPoint = new System.Numerics.Vector2(0.0f, height - whiteKeyHeight);
            blackKeyBrush.EndPoint = new System.Numerics.Vector2(0.0f, height - whiteKeyHeight + blackKeyHeight);

            // draw white keys
            for (int i = 0; i < whiteKeys; ++i) {
                ds.FillRectangle(i * whiteKeyWidthFull, height - whiteKeyHeight, whiteKeyWidth, whiteKeyHeight, whiteKeyBrush);
            }

            // draw black keys over it
            for (int i = 1; i < whiteKeys; ++i) {
                if ((i % 7) == 2 || (i % 7) == 5)
                    continue;
                ds.FillRectangle(i * whiteKeyWidthFull - blackKeyWidth * 0.5f - whiteKeyGap * 0.5f, height - whiteKeyHeight, blackKeyWidth, blackKeyHeight, blackKeyBrush);
            }

            // draw that signature red line
            using var signatureRedLineBrush = new CanvasSolidColorBrush(sender, Color.FromArgb(0xFF, 0x50, 0x00, 0x00));
            ds.FillRectangle(0, height - whiteKeyHeight - signatureRedLineHeight, width, signatureRedLineHeight, signatureRedLineBrush);
        }

        private void DrawNotes(CanvasControl sender, CanvasDrawingSession ds) {
            _rendererViewModel.GenerateNotes((_) => {
                // NOTE: Do nothing.
            });
        }
#endif
    }
}