/**
 *                    Notes in Color
 *            A project by Benyamin Bamburac
 * 
 * This project is a modern MIDI file player and visualizer
 * with some MIDI controller support.
 */

using CommunityToolkit.Mvvm.ComponentModel;

namespace NotesInColor.ViewModel;

public partial class MainPageViewModel(
    RendererViewModel rendererViewModel,
    OpenFileViewModel openFileViewModel
) : ObservableObject {
    public OpenFileViewModel OpenFileViewModel { get; } = openFileViewModel;
    public RendererViewModel RendererViewModel { get; } = rendererViewModel;
}
