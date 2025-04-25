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

    private static readonly string volume0 = "\xE74F";
    private static readonly string volume1 = "\xE993";
    private static readonly string volume2 = "\xE994";
    private static readonly string volume3 = "\xE995";

    public static FontIcon AdjustSpeedGlyph(double value) =>
         value < 0.48 ? speedOffIcon : value > 0.52 ? speedHighIcon : speedMediumIcon;

    public static string AdjustVolumeGlyph(double value) =>
         value > 0.66 ? volume3 : value > 0.33 ? volume2 : value > 0.05 ? volume1 : volume0;

    public static string VolumeReverseNormalizerAsString(double value) =>
        ((int)(value * 100.0)).ToString();
}
