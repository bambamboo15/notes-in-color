/**
 *                    Notes in Color
 *            A project by Benyamin Bamburac
 * 
 * This project is a modern MIDI file player and visualizer
 * with some MIDI controller support.
 */

using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using Windows.UI;
using System;
using NotesInColor.ViewModel;

namespace NotesInColor.Converters;

public partial class ColorRGBToColorConverter : IValueConverter {
    public object Convert(object value, Type _, object __, string ___) {
        ColorRGB color = (ColorRGB)value;
        return Color.FromArgb(255, color.R, color.G, color.B);
    }

    public object ConvertBack(object value, Type _, object __, string ___) {
        Color color = (Color)value;
        return new ColorRGB(color.R, color.G, color.B);
    }
}