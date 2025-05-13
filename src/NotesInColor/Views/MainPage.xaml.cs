/**
 *                    Notes in Color
 *            A project by Benyamin Bamburac
 * 
 * This project is a modern MIDI file player and visualizer
 * with some MIDI controller support.
 */

using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using NotesInColor.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.Pickers;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using NotesInColor.Services;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml.Media.Animation;
using ManagedBass.Midi;
using Windows.UI.ViewManagement;

namespace NotesInColor {
    public sealed partial class MainPage : Page {
        public MainPageViewModel ViewModel;
        private readonly UISettings UISettings = new();

        private readonly Stopwatch stopwatch = Stopwatch.StartNew();
        private TimeSpan lastTime = TimeSpan.Zero;

        private bool ProgressBarThumbDragged {
            get => progressBarThumbDragged;
            set {
                progressBarThumbDragged = value;
                ViewModel.AudioViewModel.PlaySounds = !value;
            }
        }
        private bool progressBarThumbDragged = false;
        private readonly double minFPS = 10.0;

        public MainPage() {
            this.InitializeComponent();
            ViewModel = App.Current.Services.GetRequiredService<MainPageViewModel>(); // anti-pattern :(

            // I put this here because I wanted to decouple playthrough from Renderer. The playthrough
            // slider is here in the main page, so I put playthrough advancement code here.
            //
            // A huge issue, that I only realize now, is that the application pauses when dragged, and fixing
            // that requires putting everything in a separate thread. The application is way too big for that,
            // and the project deadline is too close, so that issue will have to stay.
            CompositionTarget.Rendering += OnRendering;

            // Detect slider thumb dragged
            progressBarSlider.Loaded += (_, _) => {
                Thumb progressBarThumb = FindVisualChild<Thumb>(progressBarSlider)!;
                progressBarThumb.DragStarted += (_, _) => ProgressBarThumbDragged = true;
                progressBarThumb.DragCompleted += (_, _) => ProgressBarThumbDragged = false;

                progressBarSlider.AddHandler(
                    UIElement.PointerPressedEvent,
                    new PointerEventHandler((_, _) => ProgressBarThumbDragged = true),
                    handledEventsToo: true
                );
                progressBarSlider.AddHandler(
                    UIElement.PointerReleasedEvent,
                    new PointerEventHandler((_, _) => ProgressBarThumbDragged = false),
                    handledEventsToo: true
                );
            };

            // slider snapping functionality
            noteLengthSlider.RegisterPropertyChangedCallback(Slider.IntermediateValueProperty, (DependencyObject obj, DependencyProperty prop) => {
                double value = noteLengthSlider.IntermediateValue;
                noteLengthSlider.IntermediateValue = Math.Abs(value - 0.5) < 0.02 ? 0.5 : value;
            });
            adjustSpeedSlider.RegisterPropertyChangedCallback(Slider.IntermediateValueProperty, (DependencyObject obj, DependencyProperty prop) => {
                double value = adjustSpeedSlider.IntermediateValue;
                adjustSpeedSlider.IntermediateValue = Math.Abs(value - 0.5) < 0.02 ? 0.5 : value;
            });

            // spacebar pressed means play/pause button
            PreviewKeyDown += (object sender, KeyRoutedEventArgs e) => {
                if (e.Key == Windows.System.VirtualKey.Space) {
                    ViewModel.PlaythroughViewModel.TogglePlayPauseCommand.Execute(null);

                    e.Handled = true;
                }
            };
        }

        // spacebar pressed means play/pause button
        private void SpacePressed(object sender, KeyboardAcceleratorInvokedEventArgs e) {
            ViewModel.PlaythroughViewModel.TogglePlayPauseCommand.Execute(null);

            e.Handled = true;

        }

        protected override void OnNavigatedFrom(NavigationEventArgs e) {
            CompositionTarget.Rendering -= OnRendering;

            // microsoft
            //ViewModel = null!;
        }

        private void OnRendering(object? sender, object e) {
            var now = stopwatch.Elapsed;
            var deltaTimeSpan = now - lastTime;
            lastTime = now;
            double deltaTime = Math.Min(1.0 / minFPS, deltaTimeSpan.TotalSeconds);

            if (!ProgressBarThumbDragged)
                ViewModel.PlaythroughViewModel.Next(deltaTime);
        }

        // NOTE:
        //   This should be in a separate class, but I'm leaving it here because it's only required here
        public static T? FindVisualChild<T>(DependencyObject parent) where T : DependencyObject {
            int count = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < count; i++) {
                DependencyObject child = VisualTreeHelper.GetChild(parent, i);
                if (child is T typedChild)
                    return typedChild;

                T? childOfChild = FindVisualChild<T>(child);
                if (childOfChild != null)
                    return childOfChild;
            }
            return null;
        }
    }
}
