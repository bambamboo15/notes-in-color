/**
 *                    Notes in Color
 *            A project by Benyamin Bamburac
 * 
 * This project is a modern MIDI file player and visualizer
 * with some MIDI controller support.
 */

using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml;

namespace NotesInColor.Converters;

/**
 * Handles view-specific conversion of text.
 */
public class PlaythroughUIConverter {
    private static readonly FontIcon playIcon = new() { Glyph = "\xF5B0" };
    private static readonly FontIcon pauseIcon = new() { Glyph = "\xF8AE" };

    private static SolidColorBrush EnabledBrush => (SolidColorBrush)Application.Current.Resources["TextFillColorPrimaryBrush"];
    private static SolidColorBrush DisabledBrush => (SolidColorBrush)Application.Current.Resources["AccentTextFillColorDisabledBrush"];

    private static readonly string playToolTip = "Play (Space)";
    private static readonly string pauseToolTip = "Pause (Space)";

    public static FontIcon PlayButton(bool value) =>
        value ? pauseIcon : playIcon;

    public static string PlayButtonToolTip(bool value) =>
        value ? playToolTip : pauseToolTip;

    public static SolidColorBrush BoolToBrush(bool value) =>
        value ? EnabledBrush : DisabledBrush;

    public static double BoolToEnabledOpacity(bool value) =>
        value ? 1.0 : 0.4;
}