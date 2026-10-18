using System.Text;

namespace InkRecognition.Math;

/// <summary>Restricted EE4 no-rule-hit domain of the authenticated cold source.
/// A formatter owner over the SAME immutable AL, not a caller grammar flag.</summary>
internal sealed class Math2Mode0RawOnlyOutputPolicy
{
    private readonly LegacyInkAlphabet alphabet;
    private readonly HashSet<ushort> grammar;
    internal LegacyMath2ObservableTokens Gate { get; }
    internal const int DecoderLabels = 297;
    internal ushort Dot { get; }
    internal ushort Period { get; }
    internal ushort Zero { get; }
    private Math2Mode0RawOnlyOutputPolicy(LegacyMath2ObservableTokens gate)
    {
        Gate = gate; alphabet = gate.ReadOwnedAlphabet();
        if (alphabet.LabelCount != 299) throw new InvalidDataException("Actual source AL required.");
        string[] names = ["√", "</sqrt>", "<frac>", "<fracbar>", "</frac>",
            "<slantedfrac>", "<slantedfracbar>", "</slantedfrac>", "<sub>", "</sub>",
            "<sup>", "</sup>", "<subsup>", "<subsup2>", "</subsup>",
            "<under>", "<under2>", "</under>", "<over>", "<over2>", "</over>",
            "<underover>", "<underover2>", "<underover3>", "</underover>",
            "<presup>", "</presup>", "<table>", "<&>", "<\\>", "</table>",
            "<vop>", "<vopbar>", "</vop>", "<fence>", "</fence>", "\u2402", "\u2403"];
        grammar = names.Select(Find).ToHashSet();
        Dot = Find("⋅"); Period = Find("."); Zero = Find("0");
        for (int digit = 0; digit < 10; digit++)
            if (Find(digit.ToString(System.Globalization.CultureInfo.InvariantCulture)) != Zero + digit)
                throw new InvalidDataException("Actual contiguous digit labels required.");
    }
    internal static Math2Mode0RawOnlyOutputPolicy Bind(Math2Mode0RawOnlyEf4 source,
        LegacyMath2ObservableTokens gate)
    {
        if (source.Source.Policy.OptionalCount != 0 || source.Source.Policy.CorrectionByte != 1 ||
            !source.Source.Policy.Bundle.ForceNonobservablePositions)
            throw new NotSupportedException("The actual cold no-constraint/no-detection profile is required.");
        return new(gate);
    }
    private ushort Find(string text) => alphabet.TryGetLabel(text, out ushort label) ? label
        : throw new InvalidDataException("Missing actual formatter symbol.");
    internal string ReadText(ushort label) => alphabet.TryGetText(label, out string text) ? text
        : throw new InvalidDataException("Actual ordinary AL interval missing.");
    internal int ByteCount(ushort label) => Encoding.UTF8.GetByteCount(ReadText(label));
    internal byte[] CopyUtf8(ushort label) => Encoding.UTF8.GetBytes(ReadText(label));
    internal void ValidateUnit(uint unit)
    {
        ushort label = (ushort)unit;
        if ((unit & 0x00ff0000u) != 0 || label == 0 || label >= DecoderLabels || grammar.Contains(label))
            throw new NotSupportedException("Whole-vector ordinary no-rule-hit output only; structured/existing/zero/control units remain unsupported.");
        _ = ReadText(label);
    }
    internal ushort[] CopyIdentityMapPrepared(ReadOnlySpan<uint> units)
    {
        if (units.Length > 1000) throw new NotSupportedException("Actual bounded prefix extent required.");
        foreach (uint unit in units) ValidateUnit(unit);
        return Enumerable.Range(0, units.Length).Select(value => checked((ushort)value)).ToArray();
    }
    // Scalar seam is explicit prepared data, not an owned result or NN claim.
    internal uint[] NormalizeDecimalPrepared(ReadOnlySpan<uint> units, CancellationToken token = default)
    {
        uint[] result = units.ToArray();
        for (int index = 1; index + 1 < result.Length; index++)
        {
            token.ThrowIfCancellationRequested();
            if ((ushort)result[index] == Dot &&
                ((result[index - 1] | result[index] | result[index + 1]) & 0x00ff0000u) == 0 &&
                (ushort)result[index - 1] >= Zero && (ushort)result[index - 1] <= Zero + 9 &&
                (ushort)result[index + 1] >= Zero && (ushort)result[index + 1] <= Zero + 9)
                result[index] = (result[index] & 0xffff0000u) | Period;
        }
        token.ThrowIfCancellationRequested(); return result;
    }
}
