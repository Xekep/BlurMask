using System.Buffers.Binary;
using System.Runtime.CompilerServices;
[assembly:InternalsVisibleTo("LegacyMainOwnedInput.Probe")]

namespace InkRecognition;

/// <summary>One fresh raw first-stage owning invocation. All masks, graph cap,
/// pruning, primitive scope, protocol identity, acoustic costs and selected
/// publication are produced internally; caller supplies resource/runtime data
/// and ink only. Main240, complete multiword273 and language winner are separate.</summary>
public sealed class LegacyFreshFirstOwnedRecognition
{
    private readonly byte[] resource;
    private readonly LegacyCursiveSeparationTable table;
    private readonly LegacyFirstOrdinaryCacheScores[] scores;
    private readonly LegacyFreshFirstBlankPrimary primaryProducer;
    private readonly LegacyFirstPlainWordMaterializer materializer;
    public LegacyFreshFirstResultOwnershipFactory Factory {get;}
    public LegacySmoothedRowCutResult Cuts {get;}
    public LegacyFreshFirstCandidateGraphPreparation Prepared {get;}
    public LegacyFreshFirstAcousticMaskPolicy MaskPolicy {get;}
    public IReadOnlyList<LegacyFirstOrdinaryCacheScores> GroupScores {get;}

    private LegacyFreshFirstOwnedRecognition(byte[] resource,LegacyCursiveSeparationTable table,
        LegacyFreshFirstResultOwnershipFactory factory,LegacySmoothedRowCutResult cuts,
        LegacyFreshFirstCandidateGraphPreparation prepared,LegacyFreshFirstAcousticMaskPolicy masks,
        LegacyFirstOrdinaryCacheScores[] scores,LegacyFreshFirstBlankPrimary primary,
        LegacyFirstPlainWordMaterializer materializer)
    {this.resource=resource;this.table=table;Factory=factory;Cuts=cuts;Prepared=prepared;MaskPolicy=masks;
        this.scores=scores;GroupScores=Array.AsReadOnly(scores);primaryProducer=primary;this.materializer=materializer;}

    public static LegacyFreshFirstOwnedRecognition Prepare(ReadOnlySpan<byte> sourceResource,int sourceOffset,
        IReadOnlyList<InkStroke> rawPhysicalStrokes,LegacyInkActivations activations,
        LegacyCursiveSeparationTable separationTable,CancellationToken cancellationToken=default)
    {
        ArgumentNullException.ThrowIfNull(rawPhysicalStrokes);ArgumentNullException.ThrowIfNull(activations);
        ArgumentNullException.ThrowIfNull(separationTable);cancellationToken.ThrowIfCancellationRequested();
        if(sourceResource.Length>64*1024*1024)throw new ArgumentOutOfRangeException(nameof(sourceResource));
        if(rawPhysicalStrokes.Count is <1 or >256)throw new ArgumentException("Bounded fresh raw physical strokes required.",nameof(rawPhysicalStrokes));
        var raw=new InkStroke[rawPhysicalStrokes.Count];int rawTotal=0;
        for(int i=0;i<raw.Length;i++)
        {
            cancellationToken.ThrowIfCancellationRequested();var points=rawPhysicalStrokes[i]?.Points??throw new ArgumentException("Null raw physical stroke.",nameof(rawPhysicalStrokes));
            if(points.Count is <1 or >4096||rawTotal>4096-points.Count)throw new ArgumentException("Raw point budget exceeded.",nameof(rawPhysicalStrokes));
            rawTotal+=points.Count;raw[i]=new(points.ToArray());
        }
        byte[] resource=sourceResource.ToArray();
        var factory=LegacyFreshFirstResultOwnershipFactory.Prepare(resource,sourceOffset,raw,cancellationToken);
        return PrepareCore(resource,factory,activations,separationTable,cancellationToken);
    }

    /// <summary>Reuse one actual main249 submission's private fresh factory;
    /// do not manufacture an equal-looking replacement frame/result owner.</summary>
    internal static LegacyFreshFirstOwnedRecognition PrepareForOwnedFactory(ReadOnlySpan<byte> sourceResource,
        LegacyFreshFirstResultOwnershipFactory factory,LegacyInkActivations activations,
        LegacyCursiveSeparationTable separationTable,CancellationToken cancellationToken=default)
    {
        ArgumentNullException.ThrowIfNull(factory);ArgumentNullException.ThrowIfNull(activations);ArgumentNullException.ThrowIfNull(separationTable);
        cancellationToken.ThrowIfCancellationRequested();
        if(sourceResource.Length>64*1024*1024)throw new ArgumentOutOfRangeException(nameof(sourceResource));
        return PrepareCore(sourceResource.ToArray(),factory,activations,separationTable,cancellationToken);
    }

    private static LegacyFreshFirstOwnedRecognition PrepareCore(byte[] resource,LegacyFreshFirstResultOwnershipFactory factory,
        LegacyInkActivations activations,LegacyCursiveSeparationTable separationTable,CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();int sourceOffset=factory.InvocationPolicy.Binding.SourceOffset;
        if(sourceOffset<0||sourceOffset>resource.Length-16)throw new ArgumentException("Owned selected first source is outside the resource.");
        uint length=BinaryPrimitives.ReadUInt32LittleEndian(resource.AsSpan(sourceOffset+4));
        if(length<8||length>(uint)(resource.Length-sourceOffset-8)||!StringComparer.Ordinal.Equals(factory.SourceFingerprint,
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(resource.AsSpan(sourceOffset,checked((int)length+8))))))
            throw new ArgumentException("The resource must contain this factory's exact selected first source snapshot.");
        var model=LegacyFirstAcousticModel.ReadSource(resource,sourceOffset,cancellationToken);
        if(model.SourceFingerprint!=factory.SourceFingerprint||model.RowProfile!=factory.Frame.Row.Profile)
            throw new ArgumentException("The exact resource and actual owned first factory are required.");
        factory.ValidateInvocation(factory.AcousticModel,factory.Frame,factory.InvocationPolicy,cancellationToken);
        var row=factory.Frame.Row;var cuts=row.GenerateRussianCuts(cancellationToken:cancellationToken);
        var profile=LegacyFirstTransitionGeometryProfile.ReadSource(resource,sourceOffset,cancellationToken);
        int graphOffset=checked(profile.SamplerOffset+8+(int)BinaryPrimitives.ReadUInt32LittleEndian(resource[(profile.SamplerOffset+4)..]));
        var seedProfile=LegacyOrdinaryCursiveSeedProfile.ReadV10aResourceNode(resource,graphOffset);
        //Fresh raw input has ordinary source flags0 and original physical
        //boundary IDs-1. This scope does not import opaque/symbol-token input.
        int sourceCount=row.OriginalSource.Strokes.Count;
        var seeds=LegacyOrdinaryCursiveSeedProducer.GenerateLeftToRight(cuts.Cuts,row.Frame.Line.Height,seedProfile,
            new byte[sourceCount],Enumerable.Repeat(-1,sourceCount).ToArray(),cancellationToken:cancellationToken);
        var prepared=LegacyFreshFirstCandidateGraphPreparer.Prepare(resource,factory,cuts,seeds,separationTable,cancellationToken);
        var masks=LegacyFreshFirstAcousticMaskPolicy.Create(resource,factory,cancellationToken);
        //The actual108 ending owner preserves a trailing duplicate run, unlike
        //last retained ordinal+1. No caller-supplied count/context is introduced.
        var original=row.OriginalSource;var rawCounts=new int[original.Strokes.Count];
        for(int stroke=0;stroke<rawCounts.Length;stroke++)
        {
            cancellationToken.ThrowIfCancellationRequested();int last=original.Strokes[stroke].Points.Count-1;
            rawCounts[stroke]=checked(original.ResolveEndingMetadata(original.Chronology.Resolve(stroke,last),cancellationToken).Key.Third+1);
        }
        var scope=new LegacyFirstRowPrimitiveScope(row,cuts,rawCounts,cancellationToken);
        var scorer=LegacyFirstOrdinaryCacheScorer.Prepare(resource,sourceOffset,row,activations,cancellationToken);
        byte[] primaryMask=masks.CopyWorkingPrimary(cancellationToken);var pairs=masks.CopyPairs(cancellationToken);
        var scores=new LegacyFirstOrdinaryCacheScores[prepared.SourceRanges.Graph.Groups.Count];
        for(int g=0;g<scores.Length;g++)
        {
            cancellationToken.ThrowIfCancellationRequested();var ranges=prepared.SourceRanges.Graph.Groups[g].Ranges.ToArray();
            int[] order=cuts.Chronology.OrderRangeIndicesForNormalization(ranges,cancellationToken);
            var boundary=LegacyCursiveGlyphBoundaryPolicy.Resolve(order.Select(i=>ranges[i]).ToArray(),cuts.Cuts.PreparedInk.StrokePointCounts.Span,0,false,false);
            scores[g]=scorer.Score(cuts,ranges,boundary.Start,boundary.End,15,Math.Min(256,masks.MaximumNormalizedStrokes),primaryMask,
                glyph=>masks.CreateCacheContext(scope.Evaluate(ranges,glyph.MergedGeometry.Strokes.Count,cancellationToken)),pairs,cancellationToken);
        }
        var ownership=LegacyFreshFirstProtocolOwnership.Create(factory,cancellationToken);
        var producer=ownership.PreparePrimary(resource,cuts,prepared,scores,separationTable,cancellationToken);
        var materializer=new LegacyFirstPlainWordMaterializer(resource,sourceOffset,factory,separationTable,cancellationToken);
        return new(resource,separationTable,factory,cuts,prepared,masks,scores,producer,materializer);
    }

    public LegacyFreshFirstOwnedRecognitionResult Run(CancellationToken cancellationToken=default)
    {
        cancellationToken.ThrowIfCancellationRequested();var primary=primaryProducer.Run(cancellationToken);
        var contexts=LegacyFirstBlankContextPasses.Run(resource,Factory,Cuts,Prepared,primaryProducer,table,cancellationToken);
        var materialized=materializer.MaterializeNativePlainResult(resource,Factory.InvocationPolicy.Binding.SourceOffset,
            Cuts,Prepared.SourceRanges.Graph,primary,contexts,scores,cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();return new(this,primary,contexts,materialized);
    }
}

public sealed record LegacyFreshFirstOwnedRecognitionResult(LegacyFreshFirstOwnedRecognition Owner,
    LegacyFirstOrdinaryAcousticRecurrence Primary,LegacyFirstOrdinaryContextPasses Contexts,
    LegacyFirstMaterializedPlainResult Materialized);
