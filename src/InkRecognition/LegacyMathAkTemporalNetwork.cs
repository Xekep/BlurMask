namespace InkRecognition;

/// <summary>
/// Source-bound base-tail1F numerical path only. It consumes privately issued
/// temporal222 features; it is not the six-model259-label13A5 classifier.
/// No grammar candidate/cache/workspace authority follows from these outputs.
/// </summary>
public sealed class LegacyMathAkTemporalNetwork
{
    private readonly LegacyMathAkSourceGeometry source;
    private readonly short[] first;
    private readonly sbyte[] second;
    private readonly byte[] scales;
    private readonly int firstStride, secondStride;
    //Exact48-entry Text180260FD0 table, independently checked against PE bytes.
    private static readonly int[] HiddenTable =
    {
        0,65,130,193,255,316,374,429,481,531,577,620,
        660,696,730,760,788,814,837,857,876,892,907,920,
        932,943,952,960,968,974,980,985,990,994,997,1000,
        1003,1005,1008,1010,1011,1013,1014,1015,1016,1017,1018,1018
    };
    private LegacyMathAkTemporalNetwork(LegacyMathAkSourceGeometry source,
        short[] first, sbyte[] second, byte[] scales, int firstStride, int secondStride)
    { this.source = source; this.first = first; this.second = second; this.scales = scales;
      this.firstStride = firstStride; this.secondStride = secondStride; }

    /// <summary>Authenticates actual resource through the existing raw owner,
    /// then privately owns only the fixed1F model. No caller model selection.</summary>
    public static LegacyMathAkTemporalNetwork PrepareActual(LegacyMathAkSourceGeometry source,
        ReadOnlySpan<byte> actualResource, CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(source); token.ThrowIfCancellationRequested();
        using var raw = LegacyMathMixed107RawResource.LoadActualResource(actualResource, token);
        var model = raw.Models.Single(m => m.OriginalStart == 1_007_739);
        if (model.Kind != 0x1f || model.InputWidth != 222 || model.HiddenRows != 200 ||
            model.OutputRows != 257 || model.Quantizer != 10 || model.Containing13A5Start != 1_005_980)
            throw new InvalidDataException("Actual selected1F model contract differs.");
        var snapshot = model.Capture(token);
        short[] first = snapshot.CopyFirstWeights(token);
        sbyte[] second = snapshot.CopySecondWeights(token);
        byte[] scales = snapshot.CopyScales(token);
        //Actual backend may read padded lanes. Zero weights make their values
        //irrelevant without fabricating input/scratch padding authority.
        for (int row = 0; row < 200; row++)
        {
            token.ThrowIfCancellationRequested();
            for (int c = 222; c < snapshot.FirstStride; c++)
                if (first[row * snapshot.FirstStride + c] != 0)
                    throw new NotSupportedException("Nonzero first-layer padding.");
        }
        for (int row = 0; row < 257; row++)
        {
            token.ThrowIfCancellationRequested();
            for (int c = 201; c < snapshot.SecondStride; c++)
                if (second[row * snapshot.SecondStride + c] != 0)
                    throw new NotSupportedException("Nonzero second-layer padding.");
        }
        token.ThrowIfCancellationRequested();
        return new(source, first, second, scales, snapshot.FirstStride, snapshot.SecondStride);
    }

    /// <summary>Runs unmasked scalar modular dots and actual exponent10 hidden
    /// lookup. No normalized score, calibration, winner or candidate is inferred.</summary>
    public NumericalResult EvaluateRaw(LegacyMathAkSourceGeometry.TemporalFeatures features,
        CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(features); token.ThrowIfCancellationRequested();
        if (!source.OwnsTemporalFeatures(features))
            throw new ArgumentException("Exact privately issued source feature owner required.", nameof(features));
        short[] input = features.CopyValues();
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
        return new(this, dots, hidden, raw);
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
