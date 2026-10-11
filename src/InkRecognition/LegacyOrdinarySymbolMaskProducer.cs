using System.Buffers.Binary;

namespace InkRecognition;

/// <summary>Explicit result of runtime190.virtualA8; no controller binding is inferred.</summary>
public enum LegacySymbolMaskControllerOutcome { UnboundOrBypassed, Failed, Succeeded }

/// <summary>One native ordered knowledge-group range, with its stored coefficient.</summary>
public sealed record LegacySymbolMaskKnowledgeGroup(ReadOnlyMemory<ushort> Symbols, float Weight);

/// <summary>Native alphabet.virtual70 initial/medial/final forms; not serialized case triplets.</summary>
public readonly record struct LegacyContextualSymbolForms(ushort Initial, ushort Medial, ushort Final);

/// <summary>Two separately retained acoustic symbol masks.</summary>
public sealed record LegacyAcousticSymbolMasks(ReadOnlyMemory<byte> Primary, ReadOnlyMemory<byte> Secondary)
{
    /// <summary>
    /// Text1b8eb0 inactive-preselection copy, followed by1b95a0. The supplied
    /// physical source identifiers and added template count produce its byte
    /// argument via1b4e40/1b5cf0. Active neural class preselection is outside this
    /// method; invoke it only when that branch is explicitly inactive.
    /// </summary>
    public LegacyAcousticSymbolMasks CopyWithoutClassPreselectionForSources(
        ReadOnlySpan<int> orderedSourceIdentifiers, int addedTemplateStrokes,
        ReadOnlySpan<byte> minimumSourceCounts, ReadOnlySpan<byte> maximumSourceCounts,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (Primary.Length != Secondary.Length || Primary.Length > ushort.MaxValue)
            throw new ArgumentException("Prepared masks must cover the same bounded acoustic alphabet.");
        byte count = LegacyPreparedSourceStrokeCount.Count(orderedSourceIdentifiers,
            addedTemplateStrokes, cancellationToken);
        byte[] primary = Primary.ToArray(), secondary = Secondary.ToArray();
        LegacyAcousticSymbolRangeFilter.Apply(primary, secondary, minimumSourceCounts,
            maximumSourceCounts, count, cancellationToken);
        return new(primary, secondary);
    }
}

/// <summary>
/// Bounded Text1b8850 producer. Language-controller seed, generated Arabic
/// contextual forms and wide pairs are explicit upstream inputs. No script
/// alias, case correction or default linguistic controller is synthesized.
/// </summary>
public sealed class LegacyOrdinarySymbolMaskProducer
{
    private const ushort Absent = ushort.MaxValue;
    private readonly bool[] virtualSymbols;
    private readonly ushort[] header, dashMembers;
    private readonly ushort hyphen, softHyphen, closedPrimitive;

    /// <summary>Resolve the checked1b0760 exact UTF8 labels and seven header fields.</summary>
    public LegacyOrdinarySymbolMaskProducer(LegacyInkAlphabet alphabet,
        ReadOnlySpan<byte> serializedAlphabetHeader)
    {
        ArgumentNullException.ThrowIfNull(alphabet);
        if (serializedAlphabetHeader.Length != 16)
            throw new InvalidDataException("Expected eight uint16 alphabet header fields.");
        header = new ushort[7];
        if (BinaryPrimitives.ReadUInt16LittleEndian(serializedAlphabetHeader[14..]) != alphabet.LabelCount)
            throw new InvalidDataException("Alphabet header symbol count differs.");
        for (int i = 0; i < header.Length; i++)
        {
            header[i] = BinaryPrimitives.ReadUInt16LittleEndian(serializedAlphabetHeader[(i * 2)..]);
            CheckLabel(header[i], alphabet.LabelCount);
        }
        virtualSymbols = new bool[alphabet.LabelCount];
        for (int i = 0; i < virtualSymbols.Length; i++) virtualSymbols[i] = alphabet.IsVirtual((ushort)i);
        ushort Label(string text) => alphabet.TryGetLabel(text, out ushort id) ? id : Absent;
        hyphen = Label("-"); softHyphen = Label("\u00ad"); closedPrimitive = Label("\U000f0001");
        dashMembers = [hyphen, Label("_"), Label("\U000f0002"), Label("\U000f0003"), Label("\u30fc"), Label("\u4e00")];
    }

    /// <summary>
    /// Native order is intentional: each group/pair/label is visited once.
    /// Closure may re-enable packed-restricted symbols; virtual stripping and
    /// primitive restoration occur afterwards. Binary callback seeds are the
    /// checked public boundary; an unbound/failed controller supplies no seed.
    /// </summary>
    public LegacyAcousticSymbolMasks Build(LegacySymbolMaskControllerOutcome outcome,
        ReadOnlySpan<byte> controllerSeed, IReadOnlyList<LegacySymbolMaskKnowledgeGroup> knowledgeGroups,
        ReadOnlySpan<byte> packedUserRestriction, ReadOnlySpan<LegacyContextualSymbolForms> contextualForms,
        ReadOnlySpan<LegacyAcousticCachePair> widePairs, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(knowledgeGroups); cancellationToken.ThrowIfCancellationRequested();
        int count = virtualSymbols.Length;
        if (!Enum.IsDefined(outcome) || knowledgeGroups.Count > 512 || widePairs.Length > 4096 ||
            (outcome == LegacySymbolMaskControllerOutcome.Succeeded ? controllerSeed.Length != count : !controllerSeed.IsEmpty) ||
            (!packedUserRestriction.IsEmpty && packedUserRestriction.Length != (count + 7) / 8) ||
            (!contextualForms.IsEmpty && contextualForms.Length != count))
            throw new ArgumentException("Explicit bounded controller/group/restriction/form/pair inputs required.");
        foreach (byte value in controllerSeed) if (value > 1) throw new ArgumentException("Controller seed must be binary.");
        int groupEntries = 0;
        foreach (var group in knowledgeGroups)
        {
            ArgumentNullException.ThrowIfNull(group); groupEntries = checked(groupEntries + group.Symbols.Length);
            if (!float.IsFinite(group.Weight) || groupEntries > 65535)
                throw new ArgumentException("Knowledge groups require finite bounded stored values.");
            foreach (ushort symbol in group.Symbols.Span) CheckLabel(symbol, count, allowAbsent: false);
        }
        foreach (var forms in contextualForms)
        { CheckLabel(forms.Initial, count); CheckLabel(forms.Medial, count); CheckLabel(forms.Final, count); }
        foreach (var pair in widePairs)
        { CheckLabel(pair.First, count, allowAbsent: false); CheckLabel(pair.Second, count, allowAbsent: false); }

        var primary = new byte[count]; var secondary = new byte[count]; secondary.AsSpan().Fill(1);
        if (outcome == LegacySymbolMaskControllerOutcome.Succeeded)
        {
            controllerSeed.CopyTo(primary);
            foreach (var group in knowledgeGroups)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (group.Weight <= 0) continue;
                bool active = false;
                foreach (ushort symbol in group.Symbols.Span) if (primary[symbol] != 0) { active = true; break; }
                if (active) foreach (ushort symbol in group.Symbols.Span) primary[symbol] = 1;
            }
        }
        else primary.AsSpan().Fill(1);
        if (outcome != LegacySymbolMaskControllerOutcome.UnboundOrBypassed)
        { Enable(primary, hyphen); Enable(primary, softHyphen); }
        if (!packedUserRestriction.IsEmpty)
            for (int i = 0; i < count; i++)
            {
                byte permission = (byte)((packedUserRestriction[i >> 3] >> (i & 7)) & 1);
                primary[i] &= permission; secondary[i] &= permission;
            }
        if (!contextualForms.IsEmpty)
            for (int i = 0; i < count; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (secondary[i] == 0) continue;
                var forms = contextualForms[i];
                Enable(secondary, forms.Initial); Enable(secondary, forms.Medial); Enable(secondary, forms.Final);
                if (primary[i] != 0)
                { Enable(primary, forms.Initial); Enable(primary, forms.Medial); Enable(primary, forms.Final); }
            }
        foreach (var pair in widePairs)
            if (primary[pair.First] != 0 || primary[pair.Second] != 0)
                primary[pair.First] = primary[pair.Second] = 1;
        for (int i = 0; i < count; i++) if (virtualSymbols[i]) primary[i] = secondary[i] = 0;
        Aggregate(primary); Aggregate(secondary);
        return new(primary, secondary);
    }

    /// <summary>
    /// Read the v110 ordered c8/d0/d8/e0 group tail after the checked acoustic
    /// prefix. This version stores one common coefficient for all groups;
    /// later per-group coefficient versions are not silently accepted.
    /// </summary>
    public static LegacySymbolMaskKnowledgeGroup[] ReadV110KnowledgeGroups(ReadOnlySpan<byte> serialized,
        int symbolCount, out int consumedBytes, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (symbolCount is < 1 or > ushort.MaxValue) throw new ArgumentOutOfRangeException(nameof(symbolCount));
        _ = LegacyDirectionalAcousticWeightConfiguration.LoadV110Prefix(serialized, out int prefix, cancellationToken);
        if (BinaryPrimitives.ReadUInt16LittleEndian(serialized[12..]) != 0)
            throw new InvalidDataException("Compressed acoustic objects unsupported.");
        int length = checked((int)BinaryPrimitives.ReadUInt32LittleEndian(serialized[4..]) + 8);
        var r = new Reader(serialized[prefix..length]);
        r.Skip(1 + 4 + 1 + 8);
        int lists = r.U16(); if (lists > 512) throw new InvalidDataException("Too many acoustic lists.");
        for (int i = 0; i < lists; i++)
        { cancellationToken.ThrowIfCancellationRequested(); int n = r.U16(); if (n > 4096) throw new InvalidDataException("Acoustic list too long."); r.Skip(n * 2); }
        r.Skip(6 * 4);
        if (r.Byte() != 0) throw new InvalidDataException("Native size penalty branch unsupported."); r.Skip(8);
        if (r.Byte() != 0) throw new InvalidDataException("Obsolete nested conversion unsupported.");
        int count = r.I16(); if (count is < 0 or > 512) throw new InvalidDataException("Invalid acoustic group count.");
        var groups = new LegacySymbolMaskKnowledgeGroup[count];
        if (count != 0)
        {
            var ends = new ushort[count + 1];
            for (int i = 0; i < ends.Length; i++)
            { ends[i] = r.U16(); if (i > 0 && ends[i] < ends[i - 1]) throw new InvalidDataException("Nonmonotonic group ranges."); }
            var symbols = new ushort[ends[^1]];
            for (int i = 0; i < symbols.Length; i++)
            { symbols[i] = r.U16(); if (symbols[i] >= symbolCount) throw new InvalidDataException("Group symbol outside alphabet."); }
            float weight = r.Float();
            for (int i = 0; i < count; i++) groups[i] = new(symbols.AsMemory(ends[i], ends[i + 1] - ends[i]), weight);
        }
        consumedBytes = prefix + r.Position; return groups;
    }

    private void Aggregate(byte[] mask)
    {
        if (header[1] != Absent)
        {
            byte enabled = 0; foreach (ushort symbol in dashMembers) if (symbol != Absent && mask[symbol] != 0) { enabled = 1; break; }
            mask[header[1]] = enabled;
        }
        if (header[2] != Absent) mask[header[2]] = (byte)(closedPrimitive != Absent && mask[closedPrimitive] != 0 ? 1 : 0);
        foreach (int slot in new[] { 3, 0, 4, 5, 6 }) Enable(mask, header[slot]);
    }
    private static void Enable(byte[] mask, ushort label) { if (label != Absent) mask[label] = 1; }
    private static void CheckLabel(ushort symbol, int count, bool allowAbsent = true)
    { if (!(allowAbsent && symbol == Absent) && symbol >= count) throw new ArgumentException("Symbol outside acoustic alphabet."); }
    private ref struct Reader(ReadOnlySpan<byte> bytes)
    {
        private readonly ReadOnlySpan<byte> bytes = bytes; public int Position { get; private set; }
        private ReadOnlySpan<byte> Take(int n) { if (n < 0 || n > bytes.Length - Position) throw new InvalidDataException("Truncated acoustic mask tail."); var result = bytes.Slice(Position, n); Position += n; return result; }
        public void Skip(int n) => Take(n); public byte Byte() => Take(1)[0];
        public ushort U16() => BinaryPrimitives.ReadUInt16LittleEndian(Take(2)); public short I16() => BinaryPrimitives.ReadInt16LittleEndian(Take(2));
        public float Float() { float value = BinaryPrimitives.ReadSingleLittleEndian(Take(4)); if (!float.IsFinite(value)) throw new InvalidDataException("Nonfinite knowledge-group coefficient."); return value; }
    }
}
