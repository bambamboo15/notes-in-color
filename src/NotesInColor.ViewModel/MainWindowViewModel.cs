﻿/**
 *                    Notes in Color
 *            A project by Benyamin Bamburac
 * 
 * This project is a modern MIDI file player and visualizer
 * with some MIDI controller support.
 */

using CommunityToolkit.Mvvm.ComponentModel;

namespace NotesInColor.ViewModel;

public partial class MainWindowViewModel(
    MainPageViewModel mainPageViewModel,
    SettingsPageViewModel settingsPageViewModel
) : ObservableObject {
    public MainPageViewModel MainPageViewModel { get; } = mainPageViewModel;
    public SettingsPageViewModel SettingsPageViewModel { get; } = settingsPageViewModel;
}