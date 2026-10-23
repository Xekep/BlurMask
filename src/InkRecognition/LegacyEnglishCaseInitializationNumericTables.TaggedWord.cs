using System.Buffers.Binary;

namespace InkRecognition;

internal sealed partial class LegacyEnglishCaseInitializationNumericTables
{
    // Explicit numerical transport. These values cannot issue source70, a
    // query lease, a graph history receipt or a recognition candidate.
    internal readonly record struct TaggedWordInput(int Word, int Category,
        int HistoryCount, uint History0, uint History1, bool Boundary,
        bool ClearFirstTag, bool SuppressCategory, int LexicalLimit);

    internal readonly record struct TaggedWordResult(int Cost, int MatchedOrder,
        int HistoryCount, uint History0, uint History1, int Word)
    {
        internal ulong WrittenByteMask => 0xff0001ffUL |
            ((HistoryCount >= 1 ? 15UL : 0) << 12) |
            ((HistoryCount == 2 ? 15UL : 0) << 16);

        // Preserve allocator residue outside the native-known write set.
        internal void WriteRecord(Span<byte> destination)
        {
            if (destination.Length != 40) throw new ArgumentException("Exact40-byte destination required.");
            BinaryPrimitives.WriteInt32LittleEndian(destination, Cost);
            BinaryPrimitives.WriteInt32LittleEndian(destination[4..], MatchedOrder);
            destination[8] = (byte)HistoryCount;
            if (HistoryCount >= 1) BinaryPrimitives.WriteUInt32LittleEndian(destination[12..], History0);
            if (HistoryCount == 2) BinaryPrimitives.WriteUInt32LittleEndian(destination[16..], History1);
            BinaryPrimitives.WriteInt32LittleEndian(destination[24..], -1);
            BinaryPrimitives.WriteInt32LittleEndian(destination[28..], Word);
        }
    }

    // Retains references to the same immutable initializer arrays. The two
    // tiny reference vectors are allocated once; no model payload is recopied.
    internal sealed class TaggedWordTables
    {
        private readonly LegacyEnglishCaseInitializationNumericTables owner;
        private readonly byte[] words;
        private readonly byte[][] classCodes, backoffCodes;

        private TaggedWordTables(LegacyEnglishCaseInitializationNumericTables owner)
        {
            Preflight(owner.Numeric);
            this.owner = owner;
            words = owner.codes["98.wordCodes"];
            classCodes = [owner.codes["C0.0"], owner.codes["C0.1"], owner.codes["C0.2"]];
            backoffCodes = [owner.codes["C8.0"], owner.codes["C8.1"]];
        }

        internal static TaggedWordTables Prepare(LegacyEnglishCaseInitializationNumericTables owner)
        {
            ArgumentNullException.ThrowIfNull(owner);
            return new(owner);
        }

        internal bool IsFor(LegacyEnglishCaseInitializationNumericTables actual) => ReferenceEquals(owner, actual);

        // Pinned known-word/no-option/order3 only. Actual caller policy and
        // source/history identity remain the live graph wrapper's responsibility.
        internal TaggedWordResult? EvaluateTaggedCore(TaggedWordInput input,
            CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if ((uint)input.Word >= 260074 || (uint)input.Category > 12 || (uint)input.HistoryCount > 2)
                throw new ArgumentOutOfRangeException(nameof(input));
            Span<uint> history = stackalloc uint[2];
            history[0] = input.History0; history[1] = input.History1;
            Span<int> classes = stackalloc int[2];
            for (int i = 0; i < input.HistoryCount; i++)
            {
                if ((history[i] & 0xffffff) >= 1024 || ((history[i] >> 24) & 15) > 12)
                    throw new ArgumentException("Bounded actual class/category history required.", nameof(input));
                classes[i] = (int)(history[i] & 0xffffff);
            }
            if (input.ClearFirstTag && input.HistoryCount > 0) history[0] &= 0xf0ffffff;
            int tagged = 0;
            // The pinned language reader requires casePathVersion0200, so
            // model150 is nonzero. Category0 disables tagged history selection.
            if (input.Category != 0)
                while (tagged < input.HistoryCount && (history[tagged] & 0x0f000000) != 0) tagged++;

            int categoryCost = 0;
            if (!input.SuppressCategory)
            {
                int first = (int)((history[0] >> 24) & 15);
                int second = (int)((history[1] >> 24) & 15);
                categoryCost = tagged switch
                {
                    2 => owner.Numeric.GetCaseCost(first, second, input.Category),
                    1 => owner.Numeric.GetCaseCost(first, input.Category),
                    _ => input.Category switch
                    {
                        3 => input.ClearFirstTag ? -2 : 1,
                        5 or 6 => 2,
                        10 or 11 or 12 => 3,
                        _ => 0
                    }
                };
            }

            int currentClass = owner.Numeric.LanguageOwner.GetPackedValue(
                LegacyEnglishSparsePackedField.WordClasses, input.Word);
            var lexical = LegacyEnglishSparseTerminalQuery.EvaluateCore(owner.Numeric,
                words, classCodes, backoffCodes, owner.word, owner.classes, owner.backoff,
                classes[..input.HistoryCount], currentClass, input.Word, input.Boundary, token);
            int cost = checked(lexical.Cost + categoryCost);
            if (cost >= 0x4000000 || cost > input.LexicalLimit) return null;

            int count = Math.Min(input.HistoryCount + 1, 2);
            Span<uint> result = stackalloc uint[2];
            result[0] = input.HistoryCount == 2 ? history[1] & 0xffffff :
                input.HistoryCount == 1 ? history[0] & 0xffffff : (uint)currentClass;
            if (count == 2) result[1] = (uint)currentClass;
            if (input.SuppressCategory)
            {
                for (int i = 0; i < tagged; i++) result[i] |= history[i] & 0x0f000000;
            }
            else
            {
                int priorTags = tagged < 2 ? tagged : tagged - 1;
                int skip = tagged < 2 ? 0 : 1;
                for (int i = 0; i < priorTags; i++) result[i] |= history[i + skip] & 0x0f000000;
                result[priorTags] |= (uint)input.Category << 24;
            }
            token.ThrowIfCancellationRequested();
            return new(cost, lexical.MatchedOrder, count, result[0], count == 2 ? result[1] : 0, input.Word);
        }
    }
}
