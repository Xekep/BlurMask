namespace InkRecognition;

/// <summary>Source-bound selected5F upper223 numerical view. It shares immutable
/// selected-five weights and existing modular/lookup/EXP kernels. Its257 channels
/// are not the complete259-symbol AL or a grammar/candidate classifier.</summary>
public sealed class LegacyMathAkDirectionalUpperNetwork
{
    private readonly LegacyMathAkSourceGeometry source;
    private readonly LegacyMathAkSelectedModels models;
    internal LegacyMathAkDirectionalUpperNetwork(LegacyMathAkSourceGeometry source, LegacyMathAkSelectedModels models)
    { this.source = source; this.models = models; }
    public LegacyMathAkSelectedModels ModelOwner => models;
    public NumericalResult EvaluateRaw(LegacyMathAkSourceGeometry.DirectionalUpperFeatures features,
        CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(features); token.ThrowIfCancellationRequested();
        var result = models.EvaluateDirectionalUpper(source, features, token);
        return new(this, result.Dots, result.Hidden, result.Raw);
    }
    public NetworkCostResult EvaluateNetworkCosts(LegacyMathAkSourceGeometry.DirectionalUpperFeatures features,
        CancellationToken token = default)
    {
        var numerical = EvaluateRaw(features, token);
        var converted = LegacyMathAkTemporalNetwork.ConvertNetworkRaw(numerical.CopyRawOutputs(), token);
        token.ThrowIfCancellationRequested();
        return new(numerical, converted.Probability, converted.Costs);
    }
    public sealed class NumericalResult
    {
        private readonly LegacyMathAkDirectionalUpperNetwork owner;
        private readonly int[] dots, raw;
        private readonly short[] hidden;
        internal NumericalResult(LegacyMathAkDirectionalUpperNetwork owner, int[] dots, short[] hidden, int[] raw)
        { this.owner = owner; this.dots = dots; this.hidden = hidden; this.raw = raw; }
        public LegacyMathAkSourceGeometry Source => owner.source;
        public int ModelStart => 1_629_111;
        public int Quantizer => 10;
        public bool HasCandidateAuthority => false;
        public bool HasUnifiedAlphabetCosts => false;
        public int[] CopyHiddenDots() => (int[])dots.Clone();
        public short[] CopyHiddenValues() => (short[])hidden.Clone();
        public int[] CopyRawOutputs() => (int[])raw.Clone();
    }
    public sealed class NetworkCostResult
    {
        private readonly NumericalResult numerical;
        private readonly int[] probability, costs;
        internal NetworkCostResult(NumericalResult numerical, int[] probability, int[] costs)
        { this.numerical = numerical; this.probability = probability; this.costs = costs; }
        public LegacyMathAkSourceGeometry Source => numerical.Source;
        public int ModelStart => 1_629_111;
        public bool HasCandidateAuthority => false;
        public bool HasUnifiedAlphabetCosts => false;
        public int[] CopyHiddenDots() => numerical.CopyHiddenDots();
        public short[] CopyHiddenValues() => numerical.CopyHiddenValues();
        public int[] CopyRawOutputs() => numerical.CopyRawOutputs();
        public int[] CopyProbability32768() => (int[])probability.Clone();
        public int[] CopyNetworkCosts8192() => (int[])costs.Clone();
    }
}
