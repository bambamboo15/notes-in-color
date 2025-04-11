/**
 *                    Notes in Color
 *            A project by Benyamin Bamburac
 * 
 * This project is a modern MIDI file player and visualizer
 * with some MIDI controller support.
 */

namespace NotesInColor.Services;

public interface INavigator {
    /**
     * Navigates to the given page.
     */
    public void NavigateTo(PageType pageType);
}
