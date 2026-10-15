using System.Buffers.Binary;

namespace InkRecognition;

/// <summary>Ordinary21B.VC8/19A1D0's single sixteen-byte source coordinate.
/// This packs a source-node/arc reference, not screen X/Y or glyph bounds.</summary>
public static class LegacyNativeMainSourceCoordinates
{
    /// <summary>Return the required item count1 even for empty storage. With
    /// room for an item, overwrite its qword and flag byte only. The native
    /// glyph-difference argument does not affect this ordinary source method.
    /// Input node bits are truncated to24, without signed-range inference.</summary>
    public static int WriteSingle(uint sourceNode, ushort sourceArc, Span<byte> coordinateStorage)
    {
        if (coordinateStorage.Length % 16 != 0)
            throw new ArgumentException("Complete sixteen-byte coordinate storage required.", nameof(coordinateStorage));
        if (!coordinateStorage.IsEmpty)
        {
            ulong packed = (sourceNode & 0xffffffU) | ((ulong)sourceArc << 24);
            BinaryPrimitives.WriteUInt64LittleEndian(coordinateStorage, packed);
            coordinateStorage[8] = 0;
        }
        return 1;
    }

    /// <summary>2075E0's callback-present counters. The supplied frame already
    /// contains count25C and sixteen-byte coordinate items260. Count excludes
    /// the last item's flag; field3A4 is a signed difference, without clamping.
    /// This is not the distinct null-callback fallback, which zeros all three.</summary>
    public static void WriteCallbackCounts(byte glyphDifference, Span<byte> sequenceFrame)
    {
        if (sequenceFrame.Length != 0x3b0)
            throw new ArgumentException("Complete944-byte sequence frame required.", nameof(sequenceFrame));
        int count = BinaryPrimitives.ReadInt32LittleEndian(sequenceFrame[0x25c..]);
        if ((uint)count > 20)
            throw new ArgumentException("Callback count must fit the native twenty-item buffer.", nameof(sequenceFrame));
        int unflagged = 0, flagged = 0;
        for (int item = count - 2; item >= 0; item--)
        {
            if ((sequenceFrame[0x268 + item * 16] & 1) == 0) unflagged++;
            else flagged++;
        }
        BinaryPrimitives.WriteInt32LittleEndian(sequenceFrame[0x3a0..], unflagged);
        BinaryPrimitives.WriteInt32LittleEndian(sequenceFrame[0x3a4..], glyphDifference - unflagged);
        BinaryPrimitives.WriteInt32LittleEndian(sequenceFrame[0x3a8..], flagged);
    }
}
