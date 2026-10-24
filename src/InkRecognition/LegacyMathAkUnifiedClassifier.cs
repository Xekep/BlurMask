[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("LegacyMathAkUnifiedClassifier.Probe")]

namespace InkRecognition;

/// <summary>Fixed selected-five, unrestricted-AL, absent-guide numerical
/// classifier. This is not the13A4 grammar mask, matcher or candidate-tree owner.
/// A source must have been created by the exact shared model feature profile.</summary>
public sealed class LegacyMathAkUnifiedClassifier
{
    public const int MaximumOriginalPoints = 255;
    private const int Alphabet = 259, Width = 262, Infinite = 0x04000000;
    private readonly object resultIssuer = new();
    private readonly LegacyMathAkSelectedModels models;
    private readonly LegacyMathAkSourceGeometry source;
    private readonly LegacyMathAkSourceGeometry.TemporalFeatures temporal;
    private readonly LegacyMathAkSourceGeometry.SpatialFeatures spatial;
    private readonly LegacyMathAkSourceGeometry.DirectionalFeatureBundle directional;

    private LegacyMathAkUnifiedClassifier(LegacyMathAkSelectedModels models,
        LegacyMathAkSourceGeometry source, LegacyMathAkSourceGeometry.TemporalFeatures temporal,
        LegacyMathAkSourceGeometry.SpatialFeatures spatial,
        LegacyMathAkSourceGeometry.DirectionalFeatureBundle directional)
    { this.models = models; this.source = source; this.temporal = temporal;
      this.spatial = spatial; this.directional = directional; }

    /// <summary>Prepares source-issued features only; does not run a model.
    /// The299 shape producer is deferred until a finite best label has alternatives.
    /// No raw resource, expected outputs, mask, guide or settings are accepted.</summary>
    public static LegacyMathAkUnifiedClassifier Prepare(LegacyMathAkSelectedModels models,
        LegacyMathAkSourceGeometry source, CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(models); ArgumentNullException.ThrowIfNull(source);
        token.ThrowIfCancellationRequested();
        if (!ReferenceEquals(models.FeatureProfile, source.FeatureProfile))
            throw new ArgumentException("Exact shared model feature-profile source required.", nameof(source));
        if (source.StrokeCount != 1 || source.PointCount is < 2 or > MaximumOriginalPoints)
            throw new NotSupportedException("Classifier requires one whole stroke with2..255 points.");
        // Existing private producers retain all their degenerate/overflow/255
        // prepared-sample guards. The299 branch follows native post-NN selection;
        // no equivalence to native allocation-failure prefixes is claimed.
        var temporal = source.PrepareUnguidedSingleStrokeTemporal(token);
        var spatial = source.PrepareUnguidedSingleStrokeSpatial(token);
        var directional = source.PrepareUnguidedSingleStrokeDirectionalBundle(token);
        token.ThrowIfCancellationRequested();
        return new(models, source, temporal, spatial, directional);
    }

    public LegacyMathAkSourceGeometry Source => source;
    public LegacyMathAkSelectedModels ModelOwner => models;
    public bool HasGrammarCandidateAuthority => false;

    /// <summary>Runs exactly the five fixed modular networks, then the actual
    /// unmasked cost/filter/sort/prune/single-stroke-scale/top20 projection.
    /// Integer costs are not calibrated confidence or a handwriting accuracy claim.</summary>
    public Result Evaluate(CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();
        // The exact objects are privately retained from Prepare. No caller result
        // or detached feature array can become an input to this operation.
        var families = new int[5][];
        families[0] = models.BindTemporal(source, token).EvaluateNetworkCosts(temporal, token).CopyNetworkCosts8192();
        token.ThrowIfCancellationRequested();
        families[1] = models.BindSpatial(source, token).EvaluateNetworkCosts(spatial, token).CopyNetworkCosts8192();
        token.ThrowIfCancellationRequested();
        families[2] = models.BindDirectionalTemporal(source, token).EvaluateNetworkCosts(directional.Temporal, token).CopyNetworkCosts8192();
        token.ThrowIfCancellationRequested();
        families[3] = models.BindDirectionalSpatial(source, token).EvaluateNetworkCosts(directional.Spatial, token).CopyNetworkCosts8192();
        token.ThrowIfCancellationRequested();
        families[4] = models.BindDirectionalUpper(source, token).EvaluateNetworkCosts(directional.Upper, token).CopyNetworkCosts8192();
        var costs = new int[Width]; Array.Fill(costs, Infinite);
        for (int label = 0; label < 257; label++)
        {
            if ((label & 31) == 0) token.ThrowIfCancellationRequested();
            //12950: each cost*weight CVTT, signed integer sum, then F32 /sumWeight.
            // All actual mode2 fresh weights are1. Do not average probabilities.
            int sum = 0; float weight = 0;
            for (int family = 0; family < 5; family++)
            {
                int value = families[family][label];
                if (value is < 0 or > Infinite) throw new InvalidDataException("Network cost outside selected fixed domain.");
                sum = checked(sum + (int)((float)value * 1f)); weight += 1f;
            }
            costs[label] = Math.Min(Infinite, (int)((float)sum / weight));
        }
        costs[256] = Infinite; //12950 virtual network class; not a stored text label.
        //136C0, actual AL headerA..14=FFFF, absent guide requestfloat+1C=0.
        // source152 is absent U+F0001, NOT o(source154); that branch is inactive.
        costs[260] = costs[107]; //actual apostrophe alias140; scratch only.
        //The same numeric kernel is prepared-testable, but issues DATA only.
        costs = ApplyPrototypeAdjustment(source, costs, token).Costs;
        int[] rank = SortAndPrune(costs, token);
        //14660 pow(physicalStrokeCount1,.15)=1, but preserve the F32 round-trip.
        for (int i = 0; i < Width; i++)
        { if ((i & 31) == 0) token.ThrowIfCancellationRequested();
          if (costs[i] < Infinite) costs[i] = Math.Min(Infinite, (int)((float)costs[i] * 1f)); }
        var emitted = new Symbol[Math.Min(rank.Length, 20)]; int count = 0;
        for (int i = 0; i < rank.Length && i < 20; i++)
        {
            token.ThrowIfCancellationRequested(); int label = rank[i];
            if (costs[label] >= Infinite) break;
            if (label < Alphabet) emitted[count++] = new(label, costs[label]);
        }
        if (count != emitted.Length) Array.Resize(ref emitted, count);
        token.ThrowIfCancellationRequested();
        return new(this, resultIssuer, costs, rank, emitted, families);
    }

    //Internal prepared diagnostics have no Result/owner/candidate issuing API.
    internal sealed record AdjustmentData(int[] Costs, int BestLabel, int Alternatives,
        int SelectedAlternative, int GeometryCalls, float[] Distances);

    internal static AdjustmentData ApplyPrototypeAdjustment(LegacyMathAkSourceGeometry source,
        ReadOnlySpan<int> input, CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(source); token.ThrowIfCancellationRequested();
        if (source.StrokeCount != 1 || source.PointCount is < 2 or > MaximumOriginalPoints || input.Length != Width)
            throw new ArgumentException("Fixed genuine source/bounded262 numerical DATA required.");
        int[] costs = input.ToArray();
        foreach (int value in costs)
            if (value is < 0 or > Infinite) throw new ArgumentException("Bounded nonnegative initial costs required.");
        int bestLabel = -1, bestCost = Infinite;
        for (int i = 0; i < Alphabet; i++)
        { if ((i & 31) == 0) token.ThrowIfCancellationRequested();
          if (costs[i] < bestCost) { bestLabel = i; bestCost = costs[i]; } }
        if (bestLabel < 0) return new(costs, -1, 0, -1, 0, Array.Empty<float>());
        var prototypes = source.FeatureProfile.ShapePrototypes;
        int alternatives = prototypes.GetAlternativeCount(bestLabel);
        if (alternatives == 0) return new(costs, bestLabel, 0, -1, 0, Array.Empty<float>());
        //BEB0 runs V20 for every positive alternative count, even one.
        // Do not add a299 refusal or cost when the best label has no prototypes.
        var shape = source.PrepareShapeFeatures(token);
        int selected = 0; float[] distances = Array.Empty<float>();
        if (alternatives > 1)
        {
            distances = shape.MeasurePrototypeDistances(bestLabel, token);
            selected = SelectClosest(distances);
        }
        float? factor = prototypes.GetFactor(bestLabel, selected);
        if (factor is not null)
            BlendTargetCosts(costs, bestLabel, prototypes.CopyTargets(bestLabel, selected), factor.Value, token);
        token.ThrowIfCancellationRequested();
        return new(costs, bestLabel, alternatives, selected, 1, distances);
    }

    internal static int SelectClosest(ReadOnlySpan<float> distances)
    {
        if (distances.Length is < 1 or > 10) throw new ArgumentException("One to ten bounded numerical distances required.");
        foreach (float value in distances)
            if (!float.IsFinite(value) || value < 0) throw new ArgumentException("Finite nonnegative distances required.");
        int selected = 0; float nearest = distances[0];
        for (int i = 1; i < distances.Length; i++)
            if (distances[i] < nearest) { nearest = distances[i]; selected = i; }
        return selected;
    }

    //Literal scalar DATA helper. Production supplies only authenticated targets
    //and factor.75; prepared tests additionally exercise self-alias and >=1 branch.
    //No geometry, prediction Result or source capability can be issued here.
    internal static void BlendTargetCosts(Span<int> costs, int bestLabel,
        ReadOnlySpan<ushort> targets, float factor, CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();
        if (costs.Length != Width || (uint)bestLabel >= Alphabet || targets.Length > 3 ||
            !float.IsFinite(factor) || factor < 0 || factor > 4)
            throw new ArgumentException("Bounded scalar blend DATA required.");
        foreach (int value in costs)
            if (value is < -Infinite or > Infinite) throw new ArgumentException("Costs exceed bounded scalar domain.");
        foreach (ushort target in targets)
            if (target >= Alphabet && target != ushort.MaxValue)
                throw new ArgumentException("Target exceeds bounded alphabet.");
        foreach (ushort target in targets)
        {
            token.ThrowIfCancellationRequested(); if (target == ushort.MaxValue) continue;
            //All-label source198 eligibility is privately fixedtrue. Read best
            //LIVE for every store: prepared self-target changes later >=1 stores.
            float mixed;
            if (factor < 1f)
            { float left = (float)costs[bestLabel] * factor;
              float right = (1f - factor) * (float)costs[target]; mixed = left + right; }
            else mixed = (float)costs[bestLabel] / factor;
            mixed = mixed <= 0f ? mixed - .5f : mixed + .5f;
            if (!float.IsFinite(mixed) || mixed < int.MinValue || mixed >= 2147483648f)
                throw new NotSupportedException("Prototype scalar outside CVTT bounded domain.");
            costs[target] = (int)mixed;
        }
    }

    private static int[] SortAndPrune(int[] costs, CancellationToken token)
    {
        var ids = new int[Width]; int count = 0;
        for (int i = 0; i < Width; i++) if (i != Alphabet && costs[i] < Infinite) ids[count++] = i;
        if (count > 1) QuickSort(costs, ids, 0, count - 1, token);
        if (count != 0)
        {
            int threshold = 46000;
            if (count > 10 && costs[ids[0]] < costs[ids[10]] && costs[ids[10]] <= threshold)
                threshold = costs[ids[10]];
            if (threshold < costs[ids[0]]) threshold = costs[ids[0]];
            for (int i = 0; i < Width; i++)
            { if ((i & 31) == 0) token.ThrowIfCancellationRequested();
              if ((i != Alphabet && costs[i] >= threshold) || (i == Alphabet && costs[i] >= 0)) costs[i] = Infinite; }
        }
        if (count != ids.Length) Array.Resize(ref ids, count);
        return ids;
    }
    private static void QuickSort(int[] costs, int[] ids, int lo, int hi, CancellationToken token)
    {
        while (true)
        {
            token.ThrowIfCancellationRequested(); int pivot = costs[ids[(lo + hi) / 2]], i = lo, j = hi;
            do
            {
                while (costs[ids[i]] < pivot) i++;
                while (pivot < costs[ids[j]]) j--;
                if (j < i) break;
                (ids[i], ids[j]) = (ids[j], ids[i]); i++; j--;
            } while (i <= j);
            if (lo < j) QuickSort(costs, ids, lo, j, token);
            lo = i; if (hi <= i) return;
        }
    }

    public readonly record struct Symbol(int Label, int Cost8192);
    public sealed class Result
    {
        private readonly LegacyMathAkUnifiedClassifier owner;
        private readonly int[] costs, ranked;
        private readonly Symbol[] symbols;
        private readonly int[][] families;
        internal Result(LegacyMathAkUnifiedClassifier owner, object issuer, int[] costs, int[] ranked,
            Symbol[] symbols, int[][] families)
        { ArgumentNullException.ThrowIfNull(owner);
          if (!ReferenceEquals(owner.resultIssuer, issuer)) throw new ArgumentException("Private evaluated-result issuer required.", nameof(issuer));
          this.owner = owner; this.costs = costs; this.ranked = ranked; this.symbols = symbols; this.families = families; }
        public LegacyMathAkSourceGeometry Source => owner.source;
        public LegacyMathAkSelectedModels ModelOwner => owner.models;
        public bool HasUnifiedAlphabetCosts => true;
        public bool HasGrammarCandidateAuthority => false;
        public double? Confidence => null;
        public int NetworkExecutions => 5;
        public int[] CopyCosts() => (int[])costs.Clone();
        public int[] CopyNativeRank() => (int[])ranked.Clone();
        public Symbol[] CopyTop20Symbols() => (Symbol[])symbols.Clone();
        public int[] CopyIndividualNetworkCosts(int family)
        { if ((uint)family >= 5) throw new ArgumentOutOfRangeException(nameof(family)); return (int[])families[family].Clone(); }
    }
}
