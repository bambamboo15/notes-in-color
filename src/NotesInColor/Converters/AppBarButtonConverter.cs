/**
 *                    Notes in Color
 *            A project by Benyamin Bamburac
 * 
 * This project is a modern MIDI file player and visualizer
 * with some MIDI controller support.
 */

using Microsoft.UI.Xaml.Controls;
using System;

namespace NotesInColor.Converters;

public class AppBarButtonConverter {
    private static readonly FontIcon speedOffIcon = new() { Glyph = "\xEC48" };
    private static readonly FontIcon speedMediumIcon = new() { Glyph = "\xEC49" };
    private static readonly FontIcon speedHighIcon = new() { Glyph = "\xEC4A" };

    private static readonly string volume0 = "\xE992";
    private static readonly string volume1 = "\xE993";
    private static readonly string volume2 = "\xE994";
    private static readonly string volume3 = "\xE995";

    public static FontIcon AdjustSpeedGlyph(double value) =>
         (int)value < 8 ? speedOffIcon : (int)value > 8 ? speedHighIcon : speedMediumIcon;

    public static string AdjustVolumeGlyph(double value) =>
         value > 66.0 ? volume3 : value > 33.0 ? volume2 : value > 0.5 ? volume1 : volume0;
}
