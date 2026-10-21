using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text.Json;

namespace InkRecognition;

/// <summary>
/// Portable loading of the checked Russian lite resources and their separate
/// engine data profile. No vendor DLL is loaded. Recognition still requires an
/// explicit source baseline and does not generate joined-character cuts.
/// </summary>
public sealed class LegacyRussianModelBundle
{
    private const string AcousticHash = "7a5f08809e81fb4a2d4c78b3efd251d0986503b0d30d21de3695dab1f8a89978";
    private const string LexicalHash = "d26ed08baf419361484958b3a04a5f740a228f4664f7e2643f43459223ef3e4e";
    private readonly LegacyDirectionalAcousticFamily[] _families;
    private readonly LegacyInkNetwork _initial;
    private readonly LegacyInkActivations _activations;
    private readonly LegacyDirectionalCalibration _temporal, _spatial;
    public LegacyInkAlphabet AcousticAlphabet { get; }
    public LegacyInkAlphabet LexicalAlphabet { get; }
    public LegacyPackedLexicon Lexicon { get; }
    public LegacyLexiconWordIndex WordIndex { get; }
    public LegacyWordLanguageModel LanguageModel { get; }
    public IReadOnlyList<LegacyWordLanguageCostProfile> LanguageProfiles { get; }
    public IReadOnlyList<LegacyWordAuxiliaryProfile> AuxiliaryProfiles { get; }
    public IReadOnlyList<LegacyAutomaticLineProfile> LineProfiles { get; }
    public IReadOnlyList<LegacyCursiveCategoryProfile> CursiveCategoryProfiles { get; }
    public LegacyDirectionalAcousticWeightConfiguration AcousticWeights { get; }
    public LegacyCursiveLatticeSpanProfile CursiveSpanProfile { get; }
    /// <summary>Stored source kind forwarded by the checked ordinary type2b7 path.</summary>
    public int SourceKind { get; }
    public int ResourceLanguageProfileIndex => 0x137 - SourceKind;

    private LegacyRussianModelBundle(LegacyDirectionalAcousticFamily[] families, LegacyInkNetwork initial,
        LegacyInkActivations activations, LegacyDirectionalCalibration temporal, LegacyDirectionalCalibration spatial,
        LegacyInkAlphabet acousticAlphabet, LegacyInkAlphabet lexicalAlphabet, LegacyPackedLexicon lexicon,
        LegacyLexiconWordIndex wordIndex, LegacyWordLanguageModel languageModel, LegacyWordLanguageCostProfile[] profiles,
        LegacyWordAuxiliaryProfile[] auxiliaryProfiles, int sourceKind,
        LegacyAutomaticLineProfile[] lineProfiles, LegacyCursiveCategoryProfile[] cursiveProfiles,
        LegacyDirectionalAcousticWeightConfiguration acousticWeights, LegacyCursiveLatticeSpanProfile cursiveSpanProfile)
    {
        _families = families; _initial = initial; _activations = activations; _temporal = temporal; _spatial = spatial;
        AcousticAlphabet = acousticAlphabet; LexicalAlphabet = lexicalAlphabet; Lexicon = lexicon;
        WordIndex = wordIndex; LanguageModel = languageModel; LanguageProfiles = Array.AsReadOnly(profiles);
        AuxiliaryProfiles = Array.AsReadOnly(auxiliaryProfiles);
        SourceKind = sourceKind;
        LineProfiles = Array.AsReadOnly(lineProfiles);
        CursiveCategoryProfiles = Array.AsReadOnly(cursiveProfiles);
        AcousticWeights = acousticWeights;
        CursiveSpanProfile = cursiveSpanProfile;
    }

    /// <summary>Load exactly the observed pair, copying retained parameters from caller buffers.</summary>
    public static LegacyRussianModelBundle Load(ReadOnlySpan<byte> acousticResource,
        ReadOnlySpan<byte> lexicalResource, ReadOnlySpan<byte> engineProfile,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Verify(acousticResource, 572763, AcousticHash);
        Verify(lexicalResource, 6200992, LexicalHash);
        int sourceKind = checked((int)BinaryPrimitives.ReadUInt32LittleEndian(acousticResource[5233..]));
        if (sourceKind != 0x135) throw Invalid("Unsupported Russian acoustic source kind.");
        // Raw21D/profile88=1,89=2: 2AF dispatches actual2B1, not the generic2B0 API.
        if (!acousticResource.Slice(195871, 16).SequenceEqual(new byte[] {1,2,0,0,0,0,0,0,0,0,0,0,0,0,0,0}))
            throw Invalid("Actual Russian1111/2B1 selector differs.");
        cancellationToken.ThrowIfCancellationRequested();
        if (engineProfile.Length is < 2 or > 128 * 1024) throw Invalid("Engine profile size outside bounds.");
        try
        {
            using var document = JsonDocument.Parse(engineProfile.ToArray(), new JsonDocumentOptions { MaxDepth = 8 });
            var root = document.RootElement;
            if (root.GetProperty("schemaVersion").GetInt32() != 1 ||
                root.GetProperty("profile").GetString() != "legacy-ru-lite-directional-v1" ||
                root.GetProperty("acousticSha256").GetString() != AcousticHash ||
                root.GetProperty("lexicalSha256").GetString() != LexicalHash)
                throw Invalid("Engine profile does not match the Russian resource pair.");
            var activations = new LegacyInkActivations(
                Integers(root.GetProperty("activations").GetProperty("hidden"), 48),
                Integers(root.GetProperty("activations").GetProperty("output"), 128));
            var temporal = Geometry(root.GetProperty("temporal"));
            var spatial = Geometry(root.GetProperty("spatial"));
            // Native optional calibration owners are NULL for these exact
            // resources, selecting built-in2B1 defaults. SHA strings in caller
            // JSON do not authenticate its arrays: compare all four vectors.
            if (!temporal.Minima.Span.SequenceEqual(LegacyDirectional2B1DefaultCalibration.CopyMinimum(false)) ||
                !temporal.Maxima.Span.SequenceEqual(LegacyDirectional2B1DefaultCalibration.CopyMaximum(false)) ||
                !spatial.Minima.Span.SequenceEqual(LegacyDirectional2B1DefaultCalibration.CopyMinimum(true)) ||
                !spatial.Maxima.Span.SequenceEqual(LegacyDirectional2B1DefaultCalibration.CopyMaximum(true)))
                throw Invalid("Russian2B1 calibration must match actual native defaults.");
            var centers = root.GetProperty("centers"); var slopes = root.GetProperty("slopes");
            if (centers.GetArrayLength() != 4 || slopes.GetArrayLength() != 4)
                throw Invalid("Expected four acoustic confidence families.");
            int[] starts = [195895, 283697, 367899, 467101];
            int[] ends = [283690, 367892, 467094, 566296];
            var families = new LegacyDirectionalAcousticFamily[4];
            for (int family = 0; family < 4; family++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                families[family] = new(LegacyInkNetwork.Load(acousticResource[starts[family]..ends[family]]),
                    Floats(centers[family], positive: false), Floats(slopes[family], positive: true));
            }
            var initial = LegacyInkNetwork.Load(acousticResource[189755..195746]);
            var acousticAlphabet = LegacyInkAlphabet.Load(acousticResource[99..], 0x103);
            var lexicalAlphabet = LegacyInkAlphabet.Load(lexicalResource[607074..], 0x103);
            var lexicon = LegacyPackedLexicon.Load(lexicalResource[609980..2513123]);
            var wordIndex = new LegacyLexiconWordIndex(lexicon, cancellationToken);
            var language = LegacyWordLanguageModel.Load(lexicalResource[2513123..4565174], cancellationToken);
            var profiles = LegacyWordLanguageCostProfile.LoadProfiles(lexicalResource[2513123..4565174], cancellationToken);
            var auxiliary = LegacyWordAuxiliaryProfile.LoadProfiles(lexicalResource[2513123..4565174], cancellationToken);
            LegacyAutomaticLineProfile[] lineProfiles = [
                LegacyAutomaticLineProfile.ReadResourceNode(acousticResource, 5789),
                LegacyAutomaticLineProfile.ReadResourceNode(acousticResource, 11421),
                LegacyAutomaticLineProfile.ReadResourceNode(acousticResource, 187976)];
            LegacyCursiveCategoryProfile[] cursiveProfiles = [
                LegacyCursiveCategoryProfile.Load(acousticResource[11571..], 11571, out _, cancellationToken),
                LegacyCursiveCategoryProfile.Load(acousticResource[188222..], 188222, out _, cancellationToken)];
            var acousticWeights = LegacyDirectionalAcousticWeightConfiguration.LoadV110Prefix(
                acousticResource[189734..], out int weightBytes, cancellationToken);
            if (weightBytes != 566330 - 189734) throw Invalid("Russian acoustic weight prefix length differs.");
            if (language.WordCount != wordIndex.WordCount) throw Invalid("Dictionary and language word counts differ.");
            cancellationToken.ThrowIfCancellationRequested();
            return new(families, initial, activations, temporal, spatial, acousticAlphabet, lexicalAlphabet,
                lexicon, wordIndex, language, profiles, auxiliary, sourceKind, lineProfiles, cursiveProfiles, acousticWeights,
                LegacyCursiveLatticeSpanProfile.ReadV10aResourceNode(acousticResource, 189379));
        }
        catch (Exception ex) when (ex is JsonException or KeyNotFoundException or InvalidOperationException or
            ArgumentException or FormatException or OverflowException)
        { throw new InvalidDataException("Invalid or unsupported Russian engine profile.", ex); }
    }

    /// <summary>
    /// Historical separated-whole-glyph experiment with explicit alias policy
    /// and generic wrapping2B0 features. The actual FIRST/MAIN global pipeline
    /// uses CreatePreparedGlyphClassifier instead. Native full language ranking
    /// and automatic geometry are not inferred by this historical method.
    /// </summary>
    public LegacyDirectionalWordRecognizer CreateWordRecognizer(int? relatedFormMode = null) =>
        new(_families, _initial, _activations, AcousticAlphabet, LexicalAlphabet, Lexicon, _temporal, _spatial, relatedFormMode);

    public LegacyDirectionalGlyphClassifier CreateGlyphClassifier() =>
        LegacyDirectionalGlyphClassifier.CreateActual2B1(_families, _activations, _temporal, _spatial);
    public LegacyPreparedGlyphClassifier CreatePreparedGlyphClassifier() => new(CreateGlyphClassifier(), AcousticWeights);
    public LegacyContextWindowScorer CreateContextWindowScorer() => new(_initial, _activations);

    public LegacyCursiveCategoryMapper CreateCursiveCategoryMapper(int profileIndex)
    {
        if ((uint)profileIndex >= CursiveCategoryProfiles.Count) throw new ArgumentOutOfRangeException(nameof(profileIndex));
        return new(CursiveCategoryProfiles[profileIndex]);
    }

    public LegacyWordLanguageScorer CreateLanguageScorer(int profileIndex, float runtimeScale)
    {
        if ((uint)profileIndex >= LanguageProfiles.Count) throw new ArgumentOutOfRangeException(nameof(profileIndex));
        return new(LanguageModel, LanguageProfiles[profileIndex], AuxiliaryProfiles[profileIndex], runtimeScale);
    }

    /// <summary>Use the stored acoustic kind's language profile, with an explicit runtime scale.
    /// This reproduces ordinary type2b7 forwarding, not global SDK mode selection.</summary>
    public LegacyWordLanguageScorer CreateResourceLanguageScorer(float runtimeScale) =>
        CreateLanguageScorer(ResourceLanguageProfileIndex, runtimeScale);

    /// <summary>Original prepared-choice integer beam with the checked ordinary terminal scorer.
    /// Cut candidates, history/category flags and heap policy remain caller supplied.</summary>
    public LegacyIntegerWordDecoder CreateIntegerWordDecoder(float runtimeScale) =>
        new(Lexicon, WordIndex, new(CreateResourceLanguageScorer(runtimeScale)));

    public LegacyPreparedSequenceWordSearch CreatePreparedSequenceWordSearch(float runtimeScale,
        int? relatedFormMode = null) => new(
            new LegacyAcousticLexicalMapping(AcousticAlphabet, LexicalAlphabet, _families[0].Network.OutputWidth, relatedFormMode),
            CreateIntegerWordDecoder(runtimeScale));

    private static void Verify(ReadOnlySpan<byte> data, int length, string hash)
    {
        if (data.Length != length || !SHA256.HashData(data).AsSpan().SequenceEqual(Convert.FromHexString(hash)))
            throw Invalid("Unsupported or corrupted Russian resource.");
    }

    private static int[] Integers(JsonElement array, int count)
    {
        if (array.GetArrayLength() != count) throw Invalid("Engine profile vector length differs.");
        var result = new int[count];
        for (int i = 0; i < count; i++) result[i] = array[i].GetInt32();
        return result;
    }

    private static float[] Floats(JsonElement array, bool positive)
    {
        if (array.GetArrayLength() != 180) throw Invalid("Expected180 acoustic class coefficients.");
        var result = new float[180];
        for (int i = 0; i < result.Length; i++)
        {
            float value = array[i].GetSingle();
            if (!float.IsFinite(value) || (positive && value <= 0)) throw Invalid("Invalid acoustic coefficient.");
            result[i] = value;
        }
        return result;
    }

    private static LegacyDirectionalCalibration Geometry(JsonElement element)
    {
        var minima = Integers(element.GetProperty("minima"), 274);
        var maxima = Integers(element.GetProperty("maxima"), 274);
        var low = new short[274]; var high = new short[274];
        for (int i = 0; i < low.Length; i++)
        {
            low[i] = checked((short)minima[i]); high[i] = checked((short)maxima[i]);
            if (low[i] > high[i]) throw Invalid("Invalid geometry calibration range.");
        }
        return new(low, high);
    }

    private static InvalidDataException Invalid(string reason) => new(reason);
}
