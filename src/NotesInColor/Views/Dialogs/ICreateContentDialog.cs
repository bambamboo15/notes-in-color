/**
 *                    Notes in Color
 *            A project by Benyamin Bamburac
 * 
 * This project is a modern MIDI file player and visualizer
 * with some MIDI controller support.
 */

using Microsoft.UI.Xaml.Controls;
using System.Threading.Tasks;

namespace NotesInColor.Views.Dialogs;

internal interface ICreateContentDialog {
    /**
     * Creates an instance of this inside a ContentDialog.
     */
    static abstract ContentDialog CreateContentDialog();
}