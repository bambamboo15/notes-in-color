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
 * Performs divisions
 */
public partial class ThousandReductionConverter : IValueConverter {
    public object Convert(object value, Type _, object __, string ___) =>
        (double)value * 1000.0;

    public object ConvertBack(object value, Type _, object __, string ___) =>
        (double)value / 1000.0;
}