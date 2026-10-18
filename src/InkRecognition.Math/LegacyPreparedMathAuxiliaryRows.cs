using System.Buffers.Binary;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("LegacyMathAuxiliaryRows.Probe")]
namespace InkRecognition.Math;

/// <summary>Only defined symbol/score fields and explicitly known padding.
/// Null padding represents the unwritten native stack word, never zero.</summary>
internal readonly record struct LegacyPreparedMathAuxiliaryRow(ushort Symbol, ushort? Padding, uint ScoreBits);

/// <summary>Prepared041EC0 row insertion; no actual auxiliary98/A0 owner.</summary>
internal sealed class LegacyPreparedMathAuxiliaryRows
{
    private readonly LegacyPreparedMathAuxiliaryRow[] rows;
    internal LegacyPreparedMathAuxiliaryRow[] CopyRows() => rows.ToArray();
    private LegacyPreparedMathAuxiliaryRows(LegacyPreparedMathAuxiliaryRow[] rows) { this.rows = rows; }
    internal bool TryCopyNativeImage(out byte[] image)
    {
        if (rows.Any(row => !row.Padding.HasValue)) { image = []; return false; }
        image = new byte[checked(rows.Length * 8)];
        for (int i = 0; i < rows.Length; i++)
        {
            BinaryPrimitives.WriteUInt16LittleEndian(image.AsSpan(i * 8), rows[i].Symbol);
            BinaryPrimitives.WriteUInt16LittleEndian(image.AsSpan(i * 8 + 2), rows[i].Padding!.Value);
            BinaryPrimitives.WriteUInt32LittleEndian(image.AsSpan(i * 8 + 4), rows[i].ScoreBits);
        }
        return true;
    }
    internal static LegacyPreparedMathAuxiliaryRows EvaluatePrepared(ReadOnlySpan<uint> outputUnits,
        ReadOnlySpan<byte> insertionMask, int width, ReadOnlySpan<LegacyPreparedMathAuxiliaryRow> initialRows,
        CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();
        if (outputUnits.Length > 1000 || width is < 0 or > 128 || insertionMask.Length != outputUnits.Length)
            throw new ArgumentException("Bounded prepared units, insertion mask and auxiliary width required.");
        int existingUnits = 0;
        foreach (byte value in insertionMask) if (value == 0) existingUnits++;
        if ((long)existingUnits * width != initialRows.Length)
            throw new ArgumentException("Existing auxiliary blocks must correspond to uninserted units.");
        var result = new List<LegacyPreparedMathAuxiliaryRow>(initialRows.ToArray());
        for (int unit = 0; unit < outputUnits.Length; unit++)
        {
            token.ThrowIfCancellationRequested();
            if (insertionMask[unit] == 0 || width == 0) continue;
            var block = new LegacyPreparedMathAuxiliaryRow[width];
            // Newly inserted block was zeroed. First row writes only its low
            //symbol word and score, preserving the zero padding from memset.
            block[0] = new((ushort)outputUnits[unit], 0, 0);
            //Other rows copy QWORD [rsp+20]: lowword0, score -FMAX, but the
            //word at rsp+22 has no defining write in this function.
            for (int i = 1; i < width; i++) block[i] = new(0, null, 0xff7fffff);
            result.InsertRange(checked(unit * width), block);
        }
        token.ThrowIfCancellationRequested();
        return new(result.ToArray());
    }
}
