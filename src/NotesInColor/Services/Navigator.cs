/**
 *                    Notes in Color
 *            A project by Benyamin Bamburac
 * 
 * This project is a modern MIDI file player and visualizer
 * with some MIDI controller support.
 */

using System;

namespace NotesInColor.Services;

public class Navigator : INavigator {
    private static Type DeterminePage(PageType pageType)
        => pageType switch {
            PageType.MainPage => typeof(MainPage),
            PageType.SettingsPage => typeof(SettingsPage),
            _ => throw new ArgumentOutOfRangeException(nameof(pageType))
        };

    public void NavigateTo(PageType pageType)
        => (App.Window as MainWindow)!.MainWindowFrame.Navigate(DeterminePage(pageType));
}