/**
 *                    Notes in Color
 *            A project by Benyamin Bamburac
 * 
 * This project is a modern MIDI file player and visualizer
 * with some MIDI controller support.
 */

using Microsoft.UI.Xaml.Data;
using System;

namespace NotesInColor.Converters;

/**
 * Converts 1000 -> 1, and 1 -> 1000. Does nothing else.
 */
public partial class ThousandReductionConverter : IValueConverter {
    public object Convert(object value, Type _, object __, string ___) =>
        (double)value * 1000.0;

    public object ConvertBack(object value, Type _, object __, string ___) =>
        (double)value / 1000.0;
}
