/**
 *                    Notes in Color
 *            A project by Benyamin Bamburac
 * 
 * This project is a modern MIDI file player and visualizer
 * with some MIDI controller support.
 */

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using System;

namespace NotesInColor.Services;

public class Navigator : INavigator {
    private static Type DeterminePage(PageType pageType) =>
        pageType switch {
            PageType.MainPage => typeof(MainPage),
            PageType.SettingsPage => typeof(SettingsPage),
            _ => throw new ArgumentOutOfRangeException(nameof(pageType))
        };

    private static SlideNavigationTransitionEffect DetermineNavigationEffect(PageType pageType) =>
        pageType switch {
            PageType.MainPage => SlideNavigationTransitionEffect.FromLeft,
            PageType.SettingsPage => SlideNavigationTransitionEffect.FromRight,
            _ => throw new ArgumentOutOfRangeException(nameof(pageType))
        };

    public void NavigateTo(PageType pageType) {
        (App.Current.Window as MainWindow)?.MainWindowFrame?
            .Navigate(DeterminePage(pageType), null, new SlideNavigationTransitionInfo() { Effect = DetermineNavigationEffect(pageType) });
    }
}