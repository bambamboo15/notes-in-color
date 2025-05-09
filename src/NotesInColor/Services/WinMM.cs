/**
 *                    Notes in Color
 *            A project by Benyamin Bamburac
 * 
 * This project is a modern MIDI file player and visualizer
 * with some MIDI controller support.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Midi;

namespace NotesInColor.Services;

/**
 * 
 *  
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 */
public class WinMM {
    // MIDI Input Functions

    [DllImport("winmm.dll", SetLastError = true)]
    public static extern uint midiInGetNumDevs();

    [DllImport("winmm.dll", SetLastError = true)]
    public static extern uint midiInGetDevCaps(uint deviceID, out MIDIINCAPS caps, uint size);

    [DllImport("winmm.dll", SetLastError = true)]
    public static extern uint midiInOpen(out IntPtr hMidiIn, uint uDeviceID, IntPtr dwCallback, IntPtr dwInstance, uint dwFlags);

    [DllImport("winmm.dll", SetLastError = true)]
    public static extern uint midiInStart(IntPtr hMidiIn);

    [DllImport("winmm.dll", SetLastError = true)]
    public static extern uint midiInStop(IntPtr hMidiIn);

    [DllImport("winmm.dll", SetLastError = true)]
    public static extern uint midiInClose(IntPtr hMidiIn);

    [DllImport("winmm.dll", SetLastError = true)]
    public static extern uint midiInGetErrorText(uint errorCode, [Out] char[] errorText, uint errorTextSize);

    [DllImport("winmm.dll", SetLastError = true)]
    public static extern uint midiInMessage(IntPtr hMidiIn, uint msg, uint dwInstance, uint dwParam1, uint dwParam2);


    // MIDI Output Functions

    [DllImport("winmm.dll", SetLastError = true)]
    public static extern uint midiOutGetNumDevs();

    [DllImport("winmm.dll", SetLastError = true)]
    public static extern uint midiOutGetDevCaps(uint deviceID, out MIDIOUTCAPS caps, uint size);

    [DllImport("winmm.dll", SetLastError = true)]
    public static extern uint midiOutOpen(out IntPtr hMidiOut, uint uDeviceID, IntPtr dwCallback, IntPtr dwInstance, uint dwFlags);

    [DllImport("winmm.dll", SetLastError = true)]
    public static extern uint midiOutShortMsg(IntPtr hMidiOut, uint dwMsg);

    [DllImport("winmm.dll", SetLastError = true)]
    public static extern uint midiOutLongMsg(IntPtr hMidiOut, ref MIDIHDR lpMidiHdr, uint dwSize);

    [DllImport("winmm.dll", SetLastError = true)]
    public static extern uint midiOutReset(IntPtr hMidiOut);

    [DllImport("winmm.dll", SetLastError = true)]
    public static extern uint midiOutClose(IntPtr hMidiOut);

    [DllImport("winmm.dll", SetLastError = true)]
    public static extern uint midiOutGetErrorText(uint errorCode, [Out] char[] errorText, uint errorTextSize);


    // MIDI Device Capabilities Structures

    [StructLayout(LayoutKind.Sequential)]
    public struct MIDIINCAPS {
        public uint midiDeviceID;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string szPname;
        public uint dwSupport;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MIDIOUTCAPS {
        public uint midiDeviceID;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string szPname;
        public uint dwSupport;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MIDIHDR {
        public uint lpData;
        public uint dwBufferLength;
        public uint dwBytesRecorded;
        public uint dwUser;
        public uint dwFlags;
        public uint lpNext;
        public uint reserved;
        public uint dwOffset;
    };
}