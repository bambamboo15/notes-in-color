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
 * This is RendererViewModel (ViewModel), a ViewModel that 
 * interacts with the RendererControl (View) but also with
 * various rendering models (private complicated rendering
 * logic). The models and RendererControl are decoupled;
 * they know nothing about each other. This separation of
 * concern is an advantage of MVVM (Model-View-ViewModel).
 */
public partial class RendererViewModel : ObservableObject {
    /**
     * This is a function that generates notes which are
     * intended to be used for rendering. The callback is
     * called for each generated note.
     * 
     * Right now, all it does it send back one 0.
     */
    public void GenerateNotes(Action<int> noteCallback) {
        noteCallback(0);
    }
};