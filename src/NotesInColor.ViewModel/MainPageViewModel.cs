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
    CommandsViewModel commandsViewModel,
    PlaythroughViewModel playthroughViewModel,
    PlaythroughInfoViewModel playthroughInfoViewModel
) : ObservableObject {
    public CommandsViewModel CommandsViewModel { get; } = commandsViewModel;
    public RendererViewModel RendererViewModel { get; } = rendererViewModel;
    public PlaythroughViewModel PlaythroughViewModel { get; } = playthroughViewModel;
    public PlaythroughInfoViewModel PlaythroughInfoViewModel { get; } = playthroughInfoViewModel;
}