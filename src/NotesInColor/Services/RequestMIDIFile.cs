/**
 *                    Notes in Color
 *            A project by Benyamin Bamburac
 * 
 * This project is a modern MIDI file player and visualizer
 * with some MIDI controller support.
 */

using System;
using System.Threading.Tasks;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace NotesInColor.Services;

public class RequestMIDIFile : IRequestMIDIFile {
    public async Task<string?> OpenFile() {
        var openPicker = new FileOpenPicker();
        var window = App.Current.Window;
        var hWnd = WindowNative.GetWindowHandle(window);
        InitializeWithWindow.Initialize(openPicker, hWnd);

        openPicker.ViewMode = PickerViewMode.Thumbnail;
        openPicker.FileTypeFilter.Add(".mid");
        openPicker.FileTypeFilter.Add(".midi");

        var file = await openPicker.PickSingleFileAsync();
        return file?.Path;
    }
}