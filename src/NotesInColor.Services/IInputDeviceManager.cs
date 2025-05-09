/**
 *                    Notes in Color
 *            A project by Benyamin Bamburac
 * 
 * This project is a modern MIDI file player and visualizer
 * with some MIDI controller support.
 */

using System.Collections.ObjectModel;
using System.Numerics;
using NotesInColor.Core;

namespace NotesInColor.Services;

/**
 * Input device information.
 */
public readonly record struct InputDeviceInfo(string Name, string ID) {
    public readonly string Name = Name;
    public readonly string ID = ID;

    public override string ToString() => Name;
}

/**
 * An interface for MIDI input device management.
 */
public interface IInputDeviceManager {
    /**
     * Observable collection of MIDI input devices.
     */
    ObservableCollection<InputDeviceInfo> InputDevices { get; }

    /**
     * Connect to an input device; only one input device at a time. If unsuccessful,
     * and an input device was already connected, that input device will be disconnected.
     * 
     * If successful, returns true, else returns false.
     */
    public Task<bool> ConnectToInputDevice(InputDeviceInfo info);

    /**
     * Get the input device that is currently connected to.
     * 
     * Note: Whenever an input device is registered, that will automatically be selected.
     */
    public Task<InputDeviceInfo?> GetCurrentInputDevice();

    /**
     * Raised whenever the current input device changes, even when to null.
     */
    event EventHandler? CurrentInputDeviceChanged;

    /**
     * Raised whenever a MIDI input message is recieved.
     * 
     * Only supports note on/off events.
     */
    event EventHandler<MIDIInputMessageArgs> InputMessageReceived;
}