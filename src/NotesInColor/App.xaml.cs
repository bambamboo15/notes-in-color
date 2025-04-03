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
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.Extensions.DependencyInjection;
using NotesInColor.Services;
using NotesInColor.ViewModel;
using NotesInColor.Core;

namespace NotesInColor {
    public sealed partial class App : Application {
        public new static App Current => (App)Application.Current;
        public IServiceProvider Services { get; }

        public App() {
            this.InitializeComponent();

            Services = ConfigureServices();
        }

        private static ServiceProvider ConfigureServices() {
            var services = new ServiceCollection();

            services.AddSingleton<MainWindow>();
            services.AddSingleton<RendererControl>();
            //services.AddSingleton<MainPage>();
            //services.AddSingleton<SettingsPage>();

            services.AddSingleton<MainWindowViewModel>();
            services.AddSingleton<MainPageViewModel>();
            services.AddSingleton<RendererViewModel>();
            services.AddSingleton<SettingsPageViewModel>();
            services.AddSingleton<OpenFileViewModel>();

            services.AddSingleton<IRequestMIDIFile, RequestMIDIFile>();

            services.AddSingleton<MIDIFileParser>();

            return services.BuildServiceProvider();
        }

        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args) {
            Window = new MainWindow(Current.Services.GetService<MainWindowViewModel>()!);

            Frame mainWindowFrame = (Window as MainWindow)!.MainWindowFrame;
            mainWindowFrame.NavigationFailed += OnNavigationFailed;
            mainWindowFrame.Navigate(typeof(MainPage), args.Arguments);

            Window.Activate();
        }

        void OnNavigationFailed(object sender, NavigationFailedEventArgs args) {
            throw new Exception("Failed to load Page " + args.SourcePageType.FullName);
        }

        static public Window? Window;
    }
}