/**
 *                    Notes in Color
 *            A project by Benyamin Bamburac
 * 
 * This project is a modern MIDI file player and visualizer
 * with some MIDI controller support.
 */
using CommunityToolkit.Mvvm.ComponentModel;

namespace NotesInColor.ViewModel;

/**
 * A bindable ColorRGB
 */
public partial class BindableColor : ObservableObject {
    [ObservableProperty]
    private ColorRGB colorRGB;

    public BindableColor(ColorRGB colorRGB) {
        ColorRGB = colorRGB;
    }

    public BindableColor(byte red, byte green, byte blue)
        : this(new ColorRGB(red, green, blue)) { }

    public BindableColor()
        : this(new ColorRGB()) { }
}

/**
 * Yes, this is a model
 */
public struct ColorRGB {
    public byte R { get; set; }
    public byte G { get; set; }
    public byte B { get; set; }

    public ColorRGB(byte r, byte g, byte b) {
        R = r;
        G = g;
        B = b;
    }

    public ColorRGB() : this(0, 0, 0) { }

    public ColorRGB(BindableColor bindableColor)
        : this(bindableColor.ColorRGB.R, bindableColor.ColorRGB.G, bindableColor.ColorRGB.B) { }
}