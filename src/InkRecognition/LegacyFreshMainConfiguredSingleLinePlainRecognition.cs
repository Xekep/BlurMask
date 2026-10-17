using System.Text.Json;

namespace InkRecognition;

/// <summary>Genuine raw input and first-source preprocessing followed by the
/// known-property18 configured2 main result pass. This is a distinct owner,
/// not a count1 RawResult or an accepted-text/application recognizer.</summary>
public sealed class LegacyFreshMainConfiguredSingleLinePlainRecognition
{
    private readonly byte[] acoustic, linguistic;
    private readonly LegacyRussianModelBundle model;
    private readonly LegacyInkActivations activations;
    private readonly LegacyCursiveSeparationTable table;
    private readonly LegacyNativeCaseData caseData;
    private readonly LegacyNativeTextPublicationData publication;
    private int started;
    public LegacyFreshMainSingleLinePlainRecognition InputOwner { get; }
    public LegacyOwnedRawInkDocument SourceDocument => InputOwner.SourceDocument;

    private LegacyFreshMainConfiguredSingleLinePlainRecognition(byte[] acoustic, byte[] linguistic,
        LegacyRussianModelBundle model, LegacyInkActivations activations, LegacyCursiveSeparationTable table,
        LegacyNativeCaseData caseData, LegacyNativeTextPublicationData publication,
        LegacyFreshMainSingleLinePlainRecognition inputOwner)
    {
        this.acoustic = acoustic; this.linguistic = linguistic; this.model = model; this.activations = activations;
        this.table = table; this.caseData = caseData; this.publication = publication; InputOwner = inputOwner;
    }

    public static LegacyFreshMainConfiguredSingleLinePlainRecognition Prepare(
        ReadOnlySpan<byte> acousticResource, ReadOnlySpan<byte> linguisticResource, ReadOnlySpan<byte> engineProfile,
        ReadOnlySpan<byte> spatial30, ReadOnlySpan<byte> physical50, IReadOnlyList<InkStroke> raw, float resolution,
        LegacyCursiveSeparationTable table, LegacyNativeCaseData caseData,
        LegacyNativeTextPublicationData publication, CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(table); ArgumentNullException.ThrowIfNull(caseData);
        ArgumentNullException.ThrowIfNull(publication); token.ThrowIfCancellationRequested();
        if (acousticResource.Length != 572763 || linguisticResource.Length != 6200992 ||
            spatial30.Length != 85812 || physical50.Length != 161332)
            throw new ArgumentException("Complete actual Russian/line resources required before owned allocation.");
        byte[] acoustic = acousticResource.ToArray(), linguistic = linguisticResource.ToArray();
        byte[] engine = engineProfile.ToArray();
        var model = LegacyRussianModelBundle.Load(acoustic, linguistic, engine, token);
        using var parsed = JsonDocument.Parse(engine);
        var a = parsed.RootElement.GetProperty("activations");
        var activations = new LegacyInkActivations(a.GetProperty("hidden").EnumerateArray().Select(v => v.GetInt32()).ToArray(),
            a.GetProperty("output").EnumerateArray().Select(v => v.GetInt32()).ToArray());
        var input = LegacyFreshMainSingleLinePlainRecognition.PrepareOwnedModel(acoustic, linguistic,
            model, activations, spatial30.ToArray(), physical50.ToArray(), raw, resolution, table, caseData, publication, token);
        token.ThrowIfCancellationRequested();
        return new(acoustic, linguistic, model, activations, table, caseData, publication, input);
    }

    /// <summary>Apply the known property after genuine first preprocessing,
    /// before construction of this main controller. No claim is made that a
    /// top-level engine property was forwarded into every first-child factory.</summary>
    public LegacyFreshMainConfiguredSingleLinePlainRecognitionResult Run(
        LegacyMainConfiguredAlternativeResidue callerResidue, LegacyMainAlternativeStorageAllocator alternativeAllocator,
        ReadOnlySpan<byte> compactAllocatorResidue40, CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(callerResidue); ArgumentNullException.ThrowIfNull(alternativeAllocator);
        token.ThrowIfCancellationRequested();
        if (compactAllocatorResidue40.Length == 0 || compactAllocatorResidue40.Length % 40 != 0 ||
            compactAllocatorResidue40.Length > 64 * 1024 * 1024)
            throw new ArgumentException("Bounded explicit complete compact allocation required before raw processing.");
        byte[] compactResidue = compactAllocatorResidue40.ToArray();
        if (Interlocked.CompareExchange(ref started, 1, 0) != 0)
            throw new InvalidOperationException("This actual configured raw input is single-use, including failed prefixes.");
        var preprocessed = InputOwner.Input.Run(token);
        var sampled = LegacyFreshMainSingleLineSampler.Prepare(acoustic, preprocessed.Smoothed, token).Sample(table, token);
        if (sampled.SourceRanges.Graph.NodeCount > 120)
            throw new NotSupportedException("The configured compact result currently requires its proved fresh one-shot nodes0..119.");
        var context = LegacyFreshMainSingleLineContext.Prepare(acoustic, sampled, activations, token);
        var dictionary = LegacyFreshCaseRussianDictionaryPreparation.Prepare(acoustic, linguistic,
            model, sampled, caseData, publication, token);
        var owner = new LegacyOwnedMainWordSpanPreparation(acoustic, dictionary, sampled, context, table, token);
        var controller = LegacyOwnedFreshMainWordControllerPolicy.PrepareWithSecondaryCountProperty(acoustic, owner, 2, token);
        var policy = LegacyFreshCaseMainSourcePolicy.Prepare(acoustic, linguistic, controller, token);
        ulong identity = 1;
        LegacyMainDirectStorageAllocation Allocate(LegacyMainDirectStorageRequest request)
        {
            token.ThrowIfCancellationRequested();
            byte[] storage = new byte[request.RequiredBytes]; request.PreviousStorage.Span.CopyTo(storage);
            return new(storage, identity++);
        }
        var confidence = LegacyOwnedRussianMainGlyphConfidence.Prepare(acoustic, dictionary, context, token);
        var pass = new LegacyOwnedMainCaseWordPass(acoustic, policy, owner.MainLineAcoustic, table, Allocate, token).RunOwned(token);
        var compact = LegacyOwnedMainSecondaryCompactWordMaterialization.Materialize(acoustic, pass, confidence, compactResidue, token);
        var callbacks = LegacyOwnedRussianMainSourceSymbolCallbacks.Prepare(acoustic, confidence, sampled, token);
        var geometry = LegacyOwnedRussianMainSourceGeometry.Prepare(acoustic, confidence, sampled, token);
        var interior = LegacyOwnedRussianMainInteriorDelimiter.Prepare(acoustic, callbacks, geometry, token);
        var alternatives = LegacyOwnedMainConfiguredAlternativeAssembly.Prepare(acoustic, compact, geometry,
            callbacks, interior, table, alternativeAllocator, callerResidue, token);
        alternatives.Run(token);
        token.ThrowIfCancellationRequested();
        return new(this, preprocessed, sampled, context, pass, compact, geometry, alternatives);
    }
}

/// <summary>Same raw document, first/main source, configured pass and genuine
/// active273 owners. Raw WORD UTF8 pieces are available per alternative; this
/// does not convert the result into the old count1 candidate type.</summary>
public sealed class LegacyFreshMainConfiguredSingleLinePlainRecognitionResult
{
    public LegacyFreshMainConfiguredSingleLinePlainRecognition Owner { get; }
    public LegacyFreshMainSingleLinePlainRecognition InputOwner => Owner.InputOwner;
    public LegacyOwnedRawInkDocument SourceDocument => Owner.SourceDocument;
    public LegacyFreshMainOwnedTextPreprocessedInput Preprocessed { get; }
    public LegacyFreshMainSingleLineSampled Sampled { get; }
    public LegacyFreshMainSingleLineContext Context { get; }
    public LegacyOwnedMainCaseWordPassResult Pass { get; }
    public LegacyOwnedMainSecondaryCompactWordMaterialization Compact { get; }
    public LegacyOwnedRussianMainSourceGeometry Geometry { get; }
    public LegacyOwnedMainConfiguredAlternativeAssembly Alternatives { get; }
    internal LegacyFreshMainConfiguredSingleLinePlainRecognitionResult(
        LegacyFreshMainConfiguredSingleLinePlainRecognition owner, LegacyFreshMainOwnedTextPreprocessedInput preprocessed,
        LegacyFreshMainSingleLineSampled sampled, LegacyFreshMainSingleLineContext context,
        LegacyOwnedMainCaseWordPassResult pass, LegacyOwnedMainSecondaryCompactWordMaterialization compact,
        LegacyOwnedRussianMainSourceGeometry geometry, LegacyOwnedMainConfiguredAlternativeAssembly alternatives)
    {
        Owner = owner; Preprocessed = preprocessed; Sampled = sampled; Context = context;
        Pass = pass; Compact = compact; Geometry = geometry; Alternatives = alternatives;
    }
}
