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
}
