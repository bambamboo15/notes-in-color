﻿/**
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
using Windows.Storage;
using Microsoft.Extensions.DependencyInjection;
using NotesInColor.Services;
using NotesInColor.ViewModel;
using NotesInColor.Core;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;
using System.Runtime.CompilerServices;
using Windows.UI.StartScreen;
using WinUIEx;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Xaml.Media.Animation;

namespace NotesInColor {
    public sealed partial class App : Application {
        public new static App Current => (App)Application.Current;
        public IServiceProvider Services { get; }

        public App() {
            this.InitializeComponent();

            Services = ConfigureServices();
            LoadSettings();
            _ = ManageJumpList();

            // catch unhandled exceptions
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
            TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
        }

        private void OnUnhandledException(object sender, System.UnhandledExceptionEventArgs e) {
            var ex = e.ExceptionObject as Exception;
            Debug.WriteLine($"[UNHANDLED EXCEPTION] {ex?.Message}\n{ex?.StackTrace}");
        }

        private void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e) {
            Debug.WriteLine($"[UNOBSERVED TASK EXCEPTION] {e.Exception?.Message}");
            e.SetObserved();
        }

        private static ServiceProvider ConfigureServices() {
            var services = new ServiceCollection();

            services.AddSingleton<MainWindowViewModel>();
            services.AddSingleton<MainPageViewModel>();
            services.AddSingleton<RendererViewModel>();
            services.AddSingleton<SettingsPageViewModel>();
            services.AddSingleton<CommandsViewModel>();
            services.AddSingleton<PlaythroughViewModel>();
            services.AddSingleton<PlaythroughInfoViewModel>();
            services.AddSingleton<AudioViewModel>();
            services.AddSingleton<PracticeModeViewModel>();

            services.AddSingleton<IRequestMIDIFile, RequestMIDIFile>();
            services.AddSingleton<INavigator, Navigator>();
            services.AddSingleton<ISettingsManager, SettingsManager>();
            services.AddSingleton<INoteAudioPlayer, NoteAudioPlayer>();
            services.AddSingleton<IInputDeviceManager, InputDeviceManager>();
            services.AddSingleton<
                ITryToInitiateAndCompleteTheProcessOfShowingTheDisplayOfSomethingThatCouldBeCalledWhateverContentDialog,
                TryToInitiateAndCompleteTheProcessOfShowingTheDisplayOfSomethingThatCouldBeCalledWhateverContentDialog
            >();

            services.AddSingleton<MIDIPlaythroughData>();
            services.AddSingleton<PracticeModeModel>();
            services.AddSingleton<Configurations>();

            return services.BuildServiceProvider();
        }

        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args) {
            SetUpStyling();

            /*
            var window = new MainWindow(Current.Services.GetService<MainWindowViewModel>()!);

            Frame mainWindowFrame = window.MainWindowFrame;
            mainWindowFrame.NavigationFailed += OnNavigationFailed;
            mainWindowFrame.Navigate(typeof(MainPage), args.Arguments);

            var splash = new SplashScreen(window);
            splash.Completed += (s, e) => Window = e;
            */

            /*
            Window = new MainWindow(Current.Services.GetService<MainWindowViewModel>()!);

            Frame mainWindowFrame = (Window as MainWindow)!.MainWindowFrame;
            mainWindowFrame.NavigationFailed += OnNavigationFailed;
            mainWindowFrame.Navigate(typeof(SplashScreen), args.Arguments);

            Window?.Activate();

            async Task Hello() {
                await Task.Delay(1000);

                mainWindowFrame.Navigate(typeof(MainPage), null, new SuppressNavigationTransitionInfo());
            }
            _ = Hello();
            */

            Window = new MainWindow();

            Frame mainWindowFrame = (Window as MainWindow)!.MainWindowFrame;
            mainWindowFrame.NavigationFailed += OnNavigationFailed;
            mainWindowFrame.Navigate(typeof(MainPage), args.Arguments);

            Window.Activate();
        }

        private void SetUpStyling() {
            // https://github.com/microsoft/microsoft-ui-xaml/issues/9403
            //
            // so many bugs and missing things :/
            var resource = (Style)Resources["DefaultAppBarButtonStyle"];
            resource.Setters[7] = new Setter(FrameworkElement.WidthProperty, 52);
        }

        private void LoadSettings() {
            ISettingsManager settingsManager = Current.Services.GetService<ISettingsManager>()!;
            Configurations configurations = Current.Services.GetService<Configurations>()!;

            settingsManager["theme"] ??= "auto";
        }

        private async Task ManageJumpList() {
            if (JumpList.IsSupported()) {
                JumpList jumpList = await JumpList.LoadCurrentAsync();
                jumpList.Items.Clear();
                await jumpList.SaveAsync();
            }
        }

        void OnNavigationFailed(object sender, NavigationFailedEventArgs args) {
            throw new Exception("Failed to load Page " + args.SourcePageType.FullName);
        }

        public Window? Window { get; private set; }
    }
}