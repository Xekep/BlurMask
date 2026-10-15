using System.Buffers.Binary;

namespace InkRecognition;

/// <summary>Already produced 206D30 input item: symbol/kind from the twelve-byte
/// descriptor and the selected coordinate qword from its separate table.</summary>
public readonly record struct LegacyPreparedCompactWordSymbol(ushort Symbol, byte Kind, ulong Coordinates);

public readonly record struct LegacyPreparedCompactWordOptions(ulong Field00, int ParentRecord,
    int Field14, int Field18, ushort TerminalField1e, int TerminalField24,
    bool TerminalBit7, bool TerminalBit3, bool TerminalBit4, byte Field21,
    ushort Special251c, ushort Special251e, ushort Special24e2, ushort Special24dc,
    ushort KnowledgeSymbol0e, uint SourceNode);

public readonly record struct LegacyPreparedCompactWordAppend(int RecordCount, int TerminalRecord,
    bool TerminalBit6);

/// <summary>206D30's compact forty-byte record publication with detailed-cost
/// mode2368 disabled. Input sequence, coordinates and caller options remain
/// explicit. This neither selects symbols nor constructs a backtrack path.</summary>
public static class LegacyPreparedCompactWordRecords
{
    public static LegacyPreparedCompactWordAppend Append(Span<byte> storage, int recordCount,
        ReadOnlySpan<LegacyPreparedCompactWordSymbol> symbols, ReadOnlySpan<byte> sourceBeamRecord,
        LegacyPreparedCompactWordOptions options, Func<ushort, byte> sourceSymbolCategory,
        Span<int> publishedIndices, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(sourceSymbolCategory);
        cancellationToken.ThrowIfCancellationRequested();
        if (storage.Length % 40 != 0 || recordCount < 0 || symbols.Length == 0 ||
            recordCount > storage.Length / 40 - symbols.Length || sourceBeamRecord.Length != 88 ||
            publishedIndices.Length < symbols.Length || sourceBeamRecord.Overlaps(storage))
            throw new ArgumentException("Complete disjoint beam/storage rows and nonempty prepared symbols required.");

        ushort last = symbols[^1].Symbol;
        bool terminalBit6 = true;
        if (last != options.Special251c && last != options.Special251e)
        {
            //The native comparison sign-extends the beam flag byte, but compares
            //the zero-extended ushort node against the full uint source node.
            terminalBit6 = !(U16(sourceBeamRecord, 0x18) != options.SourceNode &&
                (sbyte)sourceBeamRecord[5] < 0) && last != options.Special24e2 &&
                last != options.Special24dc && last != options.KnowledgeSymbol0e &&
                last < 0xffeb && (sourceBeamRecord[0x1a] & 0x3f) != sourceSymbolCategory(last);
        }
        int parent = options.ParentRecord;
        for (int i = 0; i < symbols.Length; i++)
        {
            var symbol = symbols[i];
            int index = recordCount + i;
            var row = storage.Slice(index * 40, 40);
            BinaryPrimitives.WriteUInt64LittleEndian(row, options.Field00);
            BinaryPrimitives.WriteUInt64LittleEndian(row[8..], symbol.Coordinates);
            BinaryPrimitives.WriteInt32LittleEndian(row[0x10..], parent);
            BinaryPrimitives.WriteInt32LittleEndian(row[0x14..], options.Field14);
            BinaryPrimitives.WriteInt32LittleEndian(row[0x18..], options.Field18);
            BinaryPrimitives.WriteUInt16LittleEndian(row[0x1c..], symbol.Symbol);
            bool final = i == symbols.Length - 1;
            BinaryPrimitives.WriteUInt16LittleEndian(row[0x1e..], final ? options.TerminalField1e : ushort.MaxValue);
            //Intermediate records clear all five upper bits. Final records
            //replace bits3/4/6/7, clear bit5 and replace the lower three bits.
            row[0x20] = (byte)((symbol.Kind & 7) | (final ?
                (options.TerminalBit3 ? 8 : 0) | (options.TerminalBit4 ? 16 : 0) |
                (terminalBit6 ? 64 : 0) | (options.TerminalBit7 ? 128 : 0) : 0));
            row[0x21] = options.Field21;
            //Bytes22/23 are allocator residue and are deliberately untouched.
            BinaryPrimitives.WriteInt32LittleEndian(row[0x24..], final ? options.TerminalField24 : -1);
            publishedIndices[i] = index;
            parent = index;
        }
        return new(recordCount + symbols.Length, parent, terminalBit6);
    }

    private static ushort U16(ReadOnlySpan<byte> bytes, int offset) =>
        BinaryPrimitives.ReadUInt16LittleEndian(bytes[offset..]);
}
