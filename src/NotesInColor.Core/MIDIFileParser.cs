/**
 *                    Notes in Color
 *            A project by Benyamin Bamburac
 * 
 * This project is a modern MIDI file player and visualizer
 * with some MIDI controller support.
 */

namespace NotesInColor.Core;

/**
 * A utility for MIDI file parsing.
 * 
 * TODO: Implement actual error handling.
 * TODO: Improve error detection (blindly believing lengths)
 */
public class MIDIFileParser {
    private static readonly byte[] MThd = [0x4D, 0x54, 0x68, 0x64];
    private static readonly byte[] MTrk = [0x4D, 0x54, 0x72, 0x6B];

    private static uint Read32(Stream stream) {
        return (uint)((stream.ReadByte() << 24) |
                      (stream.ReadByte() << 16) |
                      (stream.ReadByte() << 8) |
                      stream.ReadByte());
    }

    private static ushort Read16(Stream stream) {
        return (ushort)((stream.ReadByte() << 8) |
                        stream.ReadByte());
    }

    public object? Parse(Stream stream) {
        // Let's begin :D
        MIDIFileData data = new();

        // MIDI files always start with the four bytes "MThd",
        // so we'll try to get those.
        Span<byte> chunkTypeBuffer = stackalloc byte[4];
        if (stream.Read(chunkTypeBuffer) != 4 || !chunkTypeBuffer.SequenceEqual(MThd)) {
            System.Diagnostics.Debug.WriteLine(
                "[PARSER ERROR] First four bytes are not 'MThd'"
            );
            return null;
        }

        // Now let's read the header chunk.
        uint headerLength = Read32(stream);
        if (headerLength < 6) {
            System.Diagnostics.Debug.WriteLine(
                "[PARSER ERROR] Header length is less than 6"
            );
            return null;
        }
        data.format = Read16(stream);
        data.ntrks = Read16(stream);
        data.division = Read16(stream);
        stream.Seek(headerLength - 6, SeekOrigin.Current);

        // Some validation
        if (data.format != 1) {
            System.Diagnostics.Debug.WriteLine(
                $"[PARSER ERROR] Format {data.format} is unsupported"
            );
            return null;
        }

        System.Diagnostics.Debug.WriteLine("[PARSER] Format: " + data.format);
        System.Diagnostics.Debug.WriteLine("[PARSER] Number of tracks: " + data.ntrks);
        System.Diagnostics.Debug.WriteLine("[PARSER] Division: " + data.division);

        // Initialize data tracks
        data.tracks = new MIDITrack[data.ntrks];

        // Repeatedly read tracks
        for (int trackCount = 0; trackCount < data.ntrks; ++trackCount) {
            System.Diagnostics.Debug.WriteLine(
                $"[PARSER] ============ Track #{trackCount + 1}"
            );

            // Read track chunk type
            if (stream.Read(chunkTypeBuffer) != 4 || !chunkTypeBuffer.SequenceEqual(MTrk)) {
                System.Diagnostics.Debug.WriteLine(
                    "[PARSER ERROR] Start of track bytes are not 'MTrk'"
                );
                return null;
            }

            // Read track length
            uint trackLength = Read32(stream);
            long trackEnd = stream.Position + trackLength;

            while (stream.Position < trackEnd) {
                // TODO: Actually do something here
                stream.ReadByte();
            }
        }

        return null;
    }
}