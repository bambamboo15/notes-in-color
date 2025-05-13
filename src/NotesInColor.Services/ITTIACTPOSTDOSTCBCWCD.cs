/**
 *                    Notes in Color
 *            A project by Benyamin Bamburac
 * 
 * This project is a modern MIDI file player and visualizer
 * with some MIDI controller support.
 */

namespace NotesInColor.Services;

//
// Trust me, this is ready for production. Write once, read many.
//
public interface ITryToInitiateAndCompleteTheProcessOfShowingTheDisplayOfSomethingThatCouldBeCalledWhateverContentDialog {
    public Task ShowAsyncSorryNoInputMIDIDeviceContentDialog();
    public Task ShowAsyncInputDevicePropertiesContentDialog();
    public Task<bool> ShowAsyncJustBeforePracticeModeContentDialog();
    public Task ShowAsyncPracticeModeStatsContentDialog();
    public Task ShowSorrySomethingWentWrongFileLoadingDialog();
}