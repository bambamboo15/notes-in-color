/**
 *                    Notes in Color
 *            A project by Benyamin Bamburac
 * 
 * This project is a modern MIDI file player and visualizer
 * with some MIDI controller support.
 */

using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using NotesInColor.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;

namespace NotesInColor.Views.Dialogs {
    public sealed partial class InputDevicePropertiesDialog : UserControl, ICreateContentDialog {
        private readonly IInputDeviceManager InputDeviceManager;

        public InputDevicePropertiesDialog() {
            InputDeviceManager = App.Current.Services.GetRequiredService<IInputDeviceManager>();

            this.InitializeComponent();

            Loaded += OnLoaded;
        }

        private async void OnLoaded(object sender, RoutedEventArgs e) {
            var currentDevice = await InputDeviceManager.GetCurrentInputDevice();
            for (int i = 0; i < InputDeviceManager.InputDevices.Count; ++i) {
                if (InputDeviceManager.InputDevices[i].ID == currentDevice?.ID) {
                    InputDevicesListView.SelectedIndex = i;
                    break;
                }
            }

            InputDevicesListView.SelectionChanged += (_, _) => {
                // for some reason this check is super essential
                if (InputDevicesListView.SelectedIndex >= 0 && InputDevicesListView.SelectedIndex < InputDeviceManager.InputDevices.Count) {
                    // we don't make this async/await because we don't want to wait for this
                    _ = InputDeviceManager.ConnectToInputDevice(InputDeviceManager.InputDevices[InputDevicesListView.SelectedIndex]);
                }
            };

            InputDeviceManager.CurrentInputDeviceChanged += async (_, _) => {
                var currentDevice = await InputDeviceManager.GetCurrentInputDevice();

                for (int i = 0; i < InputDeviceManager.InputDevices.Count; ++i) {
                    if (InputDeviceManager.InputDevices[i].ID == currentDevice?.ID) {
                        InputDevicesListView.SelectedIndex = i;
                    }
                }
            };

            InputDeviceManager.InputDevices.CollectionChanged += (_, _) =>
                UpdateNoInputDevicesConnectedTextBlockOpacity();

            NoInputDevicesConnectedTextBlock.Loaded += (_, _) => {
                async void OnRendering(object? sender, object? e) {
                    await Task.Delay(50); // could be inconsistent

                    CompositionTarget.Rendering -= OnRendering;
                    UpdateNoInputDevicesConnectedTextBlockOpacity();
                }

                CompositionTarget.Rendering += OnRendering;
            };
        }

        private void UpdateNoInputDevicesConnectedTextBlockOpacity() {
            float opacity = InputDeviceManager.InputDevices.Count > 0 ? 0.0f : 0.5f;

            var visual = ElementCompositionPreview.GetElementVisual(NoInputDevicesConnectedTextBlock);
            var compositor = visual.Compositor;

            var animation = compositor.CreateScalarKeyFrameAnimation();
            animation.InsertKeyFrame(1.0f, opacity);
            animation.Duration = TimeSpan.FromMilliseconds(300);

            visual.StartAnimation("Opacity", animation);
        }

        public static ContentDialog CreateContentDialog() {
            return new ContentDialog {
                Title = "Input device properties",
                PrimaryButtonText = "OK",
                DefaultButton = ContentDialogButton.Primary,
                Content = new InputDevicePropertiesDialog(),
                XamlRoot = ((App.Current.Window as MainWindow)?.MainWindowFrame.Content as MainPage)?.XamlRoot,
                Style = (Style)Application.Current.Resources["DefaultContentDialogStyle"]
            };
        }
    }
}