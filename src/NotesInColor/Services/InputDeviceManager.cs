/**
 *                    Notes in Color
 *            A project by Benyamin Bamburac
 * 
 * This project is a modern MIDI file player and visualizer
 * with some MIDI controller support.
 */

using ManagedBass;
using Microsoft.UI.Dispatching;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.Midi;
using NotesInColor.Core;

namespace NotesInColor.Services;

public class InputDeviceManager : IInputDeviceManager {
    private MidiInPort? MidiInPort {
        get => midiInPort;
        set {
            if (midiInPort != value) {
                if (midiInPort != null)
                    midiInPort.MessageReceived -= OnMIDIMessageRecieved;
                midiInPort = value;
                if (midiInPort != null)
                    midiInPort.MessageReceived += OnMIDIMessageRecieved;

                CurrentInputDeviceChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    public ObservableCollection<InputDeviceInfo> InputDevices { get; } = new();

    private MidiInPort? midiInPort;

    private DispatcherQueue dispatcherQueue;
    private DeviceWatcher watcher;

    public InputDeviceManager() {
        dispatcherQueue = DispatcherQueue.GetForCurrentThread();

        string selector = MidiInPort.GetDeviceSelector();
        watcher = DeviceInformation.CreateWatcher(selector);

        watcher.Added += (sender, deviceInfo) => {
            dispatcherQueue.TryEnqueue(async () => {
                InputDevices.Add(new InputDeviceInfo(deviceInfo.Name, deviceInfo.Id));

                MidiInPort = await MidiInPort.FromIdAsync(deviceInfo.Id);
            });
        };

        watcher.Removed += (sender, deviceUpdate) => {
            dispatcherQueue.TryEnqueue(async () => {
                for (int i = 0; i < InputDevices.Count; ++i) {
                    if (InputDevices[i].ID == deviceUpdate.Id) {
                        InputDevices.RemoveAt(i);
                        break;
                    }
                }

                if (MidiInPort != null) {
                    var devices = await DeviceInformation.FindAllAsync(MidiInPort.GetDeviceSelector());
                    foreach (var device in devices) {
                        if (device.Id == MidiInPort!.DeviceId) {
                            return;
                        }
                    }

                    if (devices.Count > 0) {
                        MidiInPort = await MidiInPort.FromIdAsync(devices[0].Id);
                    } else {
                        MidiInPort = null;
                    }
                }
            });
        };

        watcher.Start();
    }

    public async Task<bool> ConnectToInputDevice(InputDeviceInfo info) {
        if (MidiInPort?.DeviceId == info.ID) {
            return true;
        }

        MidiInPort = await MidiInPort.FromIdAsync(info.ID);
        return MidiInPort != null;
    }

    public async Task<InputDeviceInfo?> GetCurrentInputDevice() {
        return await InputDeviceFromMidiInPort(MidiInPort);
    }

    private async Task<InputDeviceInfo?> InputDeviceFromMidiInPort(MidiInPort? midiInPort) {
        if (midiInPort == null) {
            return null;
        }

        var devices = await DeviceInformation.FindAllAsync(MidiInPort.GetDeviceSelector());
        foreach (var device in devices) {
            if (device.Id == midiInPort.DeviceId) {
                return new InputDeviceInfo(device.Name, device.Id);
            }
        }

        return null;
    }

    public event EventHandler? CurrentInputDeviceChanged;
    public event EventHandler<MIDIInputMessageArgs>? InputMessageReceived;

    private void OnMIDIMessageRecieved(MidiInPort sender, MidiMessageReceivedEventArgs args) {
        var message = args.Message;

        if (message is MidiNoteOnMessage noteOnMessage) {
            if (noteOnMessage.Velocity == 0) {
                InputMessageReceived?.Invoke(this, new MIDIInputMessageArgs(new MIDIInputNoteOffMessage(noteOnMessage.Channel, noteOnMessage.Note)));
            } else {
                InputMessageReceived?.Invoke(this, new MIDIInputMessageArgs(new MIDIInputNoteOnMessage(noteOnMessage.Channel, noteOnMessage.Note, noteOnMessage.Velocity)));
            }
        } else if (message is MidiNoteOffMessage noteOffMessage) {
            InputMessageReceived?.Invoke(this, new MIDIInputMessageArgs(new MIDIInputNoteOffMessage(noteOffMessage.Channel, noteOffMessage.Note)));
        }
    }
}