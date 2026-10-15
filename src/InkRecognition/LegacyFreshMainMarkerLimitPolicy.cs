using System.Buffers.Binary;
using System.Security.Cryptography;

namespace InkRecognition;

/// <summary>Actual selected main0 source247/v10D to sampler297 category-limit
/// binding. Source148 is absent in this serialized version, so driver80 and
/// runtime297.1C8 are null and191430 uses no closed-stroke category cap. The
/// ordinary marker-count/width limiter still runs. A nonnull optional object,
/// a different source version or a reused/external owner is not inferred.</summary>
public sealed class LegacyFreshMainMarkerLimitPolicy
{
    public LegacyFreshMainSingleLineSampler Source { get; }
    public LegacyCursiveMarkerLimitOptions Options { get; }
    public bool HasCategoryLimitObject => false;

    private LegacyFreshMainMarkerLimitPolicy(LegacyFreshMainSingleLineSampler source,
        LegacyCursiveMarkerLimitOptions options)
    { Source=source; Options=options; }

    public static LegacyFreshMainMarkerLimitPolicy Prepare(ReadOnlySpan<byte> resource,
        LegacyFreshMainSingleLineSampler source,CancellationToken cancellationToken=default)
    {
        ArgumentNullException.ThrowIfNull(source);cancellationToken.ThrowIfCancellationRequested();
        if(resource.Length!=572763||source.CategoryOwner.SelectedSourceOffset!=0||source.CategoryOwner.SamplerNodeOffset!=188157||
            !Convert.ToHexString(SHA256.HashData(resource)).Equals(source.CategoryOwner.SourceFingerprint,StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Actual owned selected main0 resource required.");
        if(BinaryPrimitives.ReadUInt32LittleEndian(resource[8..])!=0x247||
            BinaryPrimitives.ReadUInt16LittleEndian(resource[14..])!=0x10d||
            BinaryPrimitives.ReadUInt32LittleEndian(resource[188165..])!=0x297||
            BinaryPrimitives.ReadUInt16LittleEndian(resource[188171..])!=0x106)
            throw new NotSupportedException("Only the proven247/v10D absent148 to297/v106 binding is supported.");
        int tail=checked(source.AcousticOffset+8+BinaryPrimitives.ReadInt32LittleEndian(resource[(source.AcousticOffset+4)..]));
        if(BinaryPrimitives.ReadInt32LittleEndian(resource[tail..])!=0||8+BinaryPrimitives.ReadInt32LittleEndian(resource[4..])-tail!=190)
            throw new NotSupportedException("No external283 source extension may replace the optional limit object.");
        int sampler=source.CategoryOwner.SamplerNodeOffset;
        int gate=resource[sampler+59];float width=BinaryPrimitives.ReadSingleLittleEndian(resource[(sampler+60)..]);
        if(gate!=6||width!=4)throw new InvalidDataException("Actual Russian final marker-limit scalar profile required.");
        return new(source,new(gate,width,null));
    }

    /// <summary>Execute the actual whole-buffer cut pipeline, including191430's
    /// width/count pruning. The absent optional object is propagated as null,
    /// rather than imposing a caller-selected or guessed closed category cap.</summary>
    public LegacyCursiveCutResult GenerateCuts(CancellationToken cancellationToken=default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var current=Source.Preparation.Input.Input.Input.Owner.Controller.Source;
        return LegacyCursiveCutProducer.GenerateRussianVersion106(Source.Preparation.Strokes,
            Source.SourceBaseline,Source.SourceHeight,current.SourceStrokeProbabilities.ToArray(),
            false,Options.ClosedStrokeCategoryLimit,cancellationToken);
    }

    public LegacyCursiveMarkerLimitTrace GenerateCutsWithLimitTrace(CancellationToken cancellationToken=default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var current=Source.Preparation.Input.Input.Input.Owner.Controller.Source;
        return LegacyCursiveCutProducer.GenerateRussianVersion106WithLimitTrace(Source.Preparation.Strokes,
            Source.SourceBaseline,Source.SourceHeight,current.SourceStrokeProbabilities.ToArray(),
            false,Options.ClosedStrokeCategoryLimit,cancellationToken);
    }

}
