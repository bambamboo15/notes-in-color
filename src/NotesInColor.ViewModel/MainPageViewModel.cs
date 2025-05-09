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
    PlaythroughInfoViewModel playthroughInfoViewModel,
    AudioViewModel audioViewModel,
    PracticeModeViewModel practiceModeViewModel
) : ObservableObject {
    public readonly CommandsViewModel CommandsViewModel = commandsViewModel;
    public readonly RendererViewModel RendererViewModel = rendererViewModel;
    public readonly PlaythroughViewModel PlaythroughViewModel = playthroughViewModel;
    public readonly PlaythroughInfoViewModel PlaythroughInfoViewModel = playthroughInfoViewModel;
    public readonly AudioViewModel AudioViewModel = audioViewModel;
    public readonly PracticeModeViewModel PracticeModeViewModel = practiceModeViewModel;
}