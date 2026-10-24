namespace InkRecognition;

/// <summary>
/// Source-bound base-tail1F numerical path only. It consumes privately issued
/// temporal222 features; it is not the selected-five259-label13A5 classifier.
/// No grammar candidate/cache/workspace authority follows from these outputs.
/// </summary>
public sealed class LegacyMathAkTemporalNetwork
{
    private readonly LegacyMathAkSourceGeometry source;
    private readonly LegacyMathAkSelectedModels models;
    //Exact48-entry Text180260FD0 table, independently checked against PE bytes.
    private static readonly int[] HiddenTable =
    {
        0,65,130,193,255,316,374,429,481,531,577,620,
        660,696,730,760,788,814,837,857,876,892,907,920,
        932,943,952,960,968,974,980,985,990,994,997,1000,
        1003,1005,1008,1010,1011,1013,1014,1015,1016,1017,1018,1018
    };
    internal LegacyMathAkTemporalNetwork(LegacyMathAkSourceGeometry source, LegacyMathAkSelectedModels models)
    { this.source = source; this.models = models; }

    public LegacyMathAkSelectedModels ModelOwner => models;

    /// <summary>Convenience factory. For many subsets prepare a shared selected
    /// model owner once, then call its BindTemporal method for each source.</summary>
    public static LegacyMathAkTemporalNetwork PrepareActual(LegacyMathAkSourceGeometry source,
        ReadOnlySpan<byte> actualResource, CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(source); token.ThrowIfCancellationRequested();
        return LegacyMathAkSelectedModels.PrepareActual(actualResource, token).BindTemporal(source, token);
    }

    /// <summary>Runs unmasked scalar modular dots and actual exponent10 hidden
    /// lookup. No normalized score, calibration, winner or candidate is inferred.</summary>
    public NumericalResult EvaluateRaw(LegacyMathAkSourceGeometry.TemporalFeatures features,
        CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(features); token.ThrowIfCancellationRequested();
        if (!source.OwnsTemporalFeatures(features))
            throw new ArgumentException("Exact privately issued source feature owner required.", nameof(features));
        var result = models.EvaluateTemporal(source, features, token);
        return new(this, result.Dots, result.Hidden, result.Raw);
    }
    internal static (int[] Dots, short[] Hidden, int[] Raw) RunModel(ReadOnlySpan<short> input,
        ReadOnlySpan<short> first, ReadOnlySpan<sbyte> second, ReadOnlySpan<byte> scales,
        int firstStride, int secondStride, CancellationToken token)
    {
        var dots = new int[200]; var hidden = new short[201]; hidden[0] = 1024;
        for (int row = 0; row < dots.Length; row++)
        {
            if ((row & 15) == 0) token.ThrowIfCancellationRequested();
            int dot = Dot16(input, first, row * firstStride); dots[row] = dot;
            hidden[row + 1] = Hidden(dot);
        }
        var raw = new int[257];
        for (int row = 0; row < raw.Length; row++)
        {
            if ((row & 15) == 0) token.ThrowIfCancellationRequested();
            raw[row] = Dot8(hidden, second, row * secondStride, scales[row + 1]);
        }
        token.ThrowIfCancellationRequested();
        return (dots, hidden, raw);
    }
    private static int Dot16(ReadOnlySpan<short> input, ReadOnlySpan<short> weights, int at)
    {
        int dot = 0;
        unchecked { for (int i = 0; i < input.Length; i++) dot += input[i] * weights[at + i]; }
        return dot;
    }
    private static int Dot8(ReadOnlySpan<short> input, ReadOnlySpan<sbyte> weights, int at, byte scale)
    {
        int dot = 0;
        unchecked { for (int i = 0; i < input.Length; i++) dot += input[i] * weights[at + i]; return dot * scale; }
    }
    private static short Hidden(int value)
    {
        //249DE0 has a native signed-negation out-of-table INT_MIN case; this
        //bounded portable path refuses that case instead of repairing it.
        if (value == int.MinValue) throw new NotSupportedException("INT_MIN hidden lookup domain.");
        int index = Math.Abs(value) >> 16;
        int magnitude = index < 48 ? HiddenTable[index] : 1018;
        return (short)(value <= 0 ? -magnitude : magnitude);
    }
    /// <summary>Same fixed1F unmasked EXP probability/cost channel only.
    /// All257 network rows participate; virtual256 is not a final AL candidate.</summary>
    public NetworkCostResult EvaluateNetworkCosts(LegacyMathAkSourceGeometry.TemporalFeatures features,
        CancellationToken token = default)
    {
        NumericalResult numerical = EvaluateRaw(features, token);
        var converted = ConvertRaw(numerical.CopyRawOutputs(), token);
        token.ThrowIfCancellationRequested();
        return new(numerical, converted.Probability, converted.Costs);
    }
    internal static (int[] Probability, int[] Costs) ConvertNetworkRaw(ReadOnlySpan<int> raw, CancellationToken token)
        => ConvertRaw(raw, token);
    private static (int[] Probability, int[] Costs) ConvertRaw(ReadOnlySpan<int> raw, CancellationToken token)
    {
        //011BF0: AL sigmoid-specialFFFF chooses212.V50. E69F0 true Q15
        //probability buffer is exposed by C8=EBD10, not quantized scratch.
        //012950 floors probability at1 before2D05EA70 (log8192).
        int[] probability = LegacyClassScores.Convert(raw, token).Probability32768.ToArray();
        var costs = new int[probability.Length];
        for (int i = 0; i < costs.Length; i++)
        { if ((i & 31) == 0) token.ThrowIfCancellationRequested(); costs[i] = LegacyAcousticFixedLog.ProbabilityCost(probability[i]); }
        token.ThrowIfCancellationRequested(); return (probability, costs);
    }
    public sealed class NetworkCostResult
    {
        private readonly NumericalResult numerical;
        private readonly int[] probability, costs;
        internal NetworkCostResult(NumericalResult numerical, int[] probability, int[] costs)
        { this.numerical = numerical; this.probability = probability; this.costs = costs; }
        public LegacyMathAkSourceGeometry Source => numerical.Source;
        public int ModelStart => 1_007_739;
        public bool HasUnifiedAlphabetCosts => false;
        public bool HasCandidateAuthority => false;
        public int[] CopyHiddenDots() => numerical.CopyHiddenDots();
        public short[] CopyHiddenValues() => numerical.CopyHiddenValues();
        public int[] CopyRawOutputs() => numerical.CopyRawOutputs();
        public int[] CopyProbability32768() => (int[])probability.Clone();
        public int[] CopyNetworkCosts8192() => (int[])costs.Clone();
    }
    public sealed class NumericalResult
    {
        private readonly LegacyMathAkTemporalNetwork owner;
        private readonly int[] hiddenDots, raw;
        private readonly short[] hidden;
        internal NumericalResult(LegacyMathAkTemporalNetwork owner, int[] dots, short[] hidden, int[] raw)
        { this.owner = owner; hiddenDots = dots; this.hidden = hidden; this.raw = raw; }
        public LegacyMathAkSourceGeometry Source => owner.source;
        public int ModelStart => 1_007_739;
        public int Quantizer => 10;
        public bool HasCandidateAuthority => false;
        public bool HasUnifiedAlphabetCosts => false;
        public int[] CopyHiddenDots() => (int[])hiddenDots.Clone();
        public short[] CopyHiddenValues() => (short[])hidden.Clone();
        public int[] CopyRawOutputs() => (int[])raw.Clone();
    }
}
