using System.Buffers.Binary;

namespace InkRecognition;

public sealed record LegacyNativeKnownWordLanguageRecordResult(bool Accepted,
    LegacyWordLanguageScore Score, ReadOnlyMemory<byte> Record40);

/// <summary>Checked order2/flags3, no-vocabulary-map100090 record producer.
/// Literal fresh2cd initializer0f0260 sets default class64 toFFFFFFFF;
/// version100 does not overwrite it. Caller-configured defaults are not inferred.
/// The40-byte temporary heap residue is owned input, not guessed zero storage.</summary>
public sealed class LegacyNativeKnownWordLanguageRecord
{
    private readonly LegacyWordLanguageScorer scorer;
    private readonly LegacyWordLanguageModel model;
    private readonly LegacyWordLanguageCostProfile profile;
    private readonly LegacyWordAuxiliaryProfile auxiliary;
    public int WordCount => scorer.WordCount;

    public LegacyNativeKnownWordLanguageRecord(LegacyWordLanguageModel model, LegacyWordLanguageCostProfile profile,
        LegacyWordAuxiliaryProfile auxiliary, float runtimeScale)
    {
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(profile);
        ArgumentNullException.ThrowIfNull(auxiliary);
        this.model = model;
        this.profile = profile;
        this.auxiliary = auxiliary;
        scorer = new(model, profile, auxiliary, runtimeScale);
    }

    /// <summary>Selected resource identity, including the source-kind-selected
    /// cost and auxiliary profiles. Equal word counts or coefficients do not
    /// establish ownership of the source70 word ordinal.</summary>
    public bool IsForModel(LegacyRussianModelBundle selectedModel)
    {
        ArgumentNullException.ThrowIfNull(selectedModel);
        int index = selectedModel.ResourceLanguageProfileIndex;
        return ReferenceEquals(model, selectedModel.LanguageModel) &&
            ReferenceEquals(profile, selectedModel.LanguageProfiles[index]) &&
            ReferenceEquals(auxiliary, selectedModel.AuxiliaryProfiles[index]);
    }

    public LegacyNativeKnownWordLanguageRecordResult Evaluate(ReadOnlySpan<byte> sourceToken16, ReadOnlySpan<uint> history,
        int category, bool knowledgeF5, int lexicalLimit, ReadOnlySpan<byte> temporaryRecordResidue40,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (sourceToken16.Length != 16 || temporaryRecordResidue40.Length != 40 || history.Length > 1 ||
            (uint)category > 12 || sourceToken16[13] > 1)
            throw new ArgumentException("Checked source70 token, order2 history, category and actual40-byte heap residue required.");
        int wordId = BinaryPrimitives.ReadInt32LittleEndian(sourceToken16[4..]);
        bool suppressCategory = sourceToken16[13] != 0;
        bool boundary = sourceToken16[12] != 0;
        //100090 passes request01(knowledgeF5) as0ffcf0's clear-history flag,
        //request25(source tag42) as its distinct suppress-category flag.
        var score = scorer.Score(wordId, history, category, knowledgeF5, suppressCategory, boundary, cancellationToken);
        if (score.LexicalSubtotal >= 0x4000000 || score.LexicalSubtotal > lexicalLimit)
            return new(false, score, ReadOnlyMemory<byte>.Empty);

        var tagged = auxiliary.ScoreCategory(category, history, knowledgeF5, suppressCategory);
        byte[] output = temporaryRecordResidue40.ToArray();
        BinaryPrimitives.WriteInt32LittleEndian(output, score.LexicalSubtotal);
        BinaryPrimitives.WriteUInt32LittleEndian(output.AsSpan(4), (uint)history.Length + 1);
        //0ffec0's two-state class history always ends with one current class.
        output[8] = 1;
        uint newClass = (uint)score.WordClass & 0xffffff;
        uint tag = (uint)category;
        if (suppressCategory)
            tag = tagged.TaggedHistoryPrefixLength != 0 && !history.IsEmpty ? history[0] >> 24 & 15 : 0;
        BinaryPrimitives.WriteUInt32LittleEndian(output.AsSpan(12), newClass | (tag & 15) << 24);
        //Flags3 omit optional vocabulary map90.0ffec0 receives model70
        //(WordCount), recognizes that sentinel and writes stateFFFFFFFF.
        BinaryPrimitives.WriteUInt32LittleEndian(output.AsSpan(24), uint.MaxValue);
        //The general100090 branch returns its source word ordinal(local124)
        //at1C, distinct from the absent-vocabulary sentinel passed to0ffec0.
        BinaryPrimitives.WriteInt32LittleEndian(output.AsSpan(28), wordId);
        cancellationToken.ThrowIfCancellationRequested();
        return new(true, score, output);
    }
}
