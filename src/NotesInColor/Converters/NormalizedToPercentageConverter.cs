/**
 *                    Notes in Color
 *            A project by Benyamin Bamburac
 * 
 * This project is a modern MIDI file player and visualizer
 * with some MIDI controller support.
 */

using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using NotesInColor.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotesInColor.Converters;

public class NormalizedToPercentageConverter : IValueConverter {
    public object Convert(object value, Type _, object __, string ___) {
        if (value is float norm)
            return string.Format("{0:0.##}%", norm * 100.0f);
        return value;
    }

    public object ConvertBack(object value, Type _, object __, string ___) =>
        throw new NotImplementedException();
}