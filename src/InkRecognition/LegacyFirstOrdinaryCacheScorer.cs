using System.Buffers.Binary;

namespace InkRecognition;

public sealed record LegacyFirstOrdinaryCacheScores(LegacyFirstPreparedAcousticScores Glyph,
    LegacyOrdinaryAcousticClassCacheResult Cache);

/// <summary>Owned first source alphabet/model/context/cache. Masks, pairs and
/// cache scope/controller flags are explicit; this is not native word search.</summary>
public sealed class LegacyFirstOrdinaryCacheScorer
{
    private readonly LegacyFirstRowContext context;
    private readonly LegacyOrdinaryAcousticClassCache cache;
    public LegacyFirstAcousticModel Model {get;}
    public LegacyOrdinaryAcousticCacheProfile Profile {get;}
    public LegacyInkAlphabet Alphabet {get;}
    public int AlphabetOffset {get;}
    public int CacheWidth=>cache.CacheWidth;
    private LegacyFirstOrdinaryCacheScorer(LegacyFirstAcousticModel model,LegacyOrdinaryAcousticCacheProfile profile,
        LegacyInkAlphabet alphabet,int offset,LegacyFirstRowContext context,LegacyOrdinaryAcousticClassCache cache)
    {Model=model;Profile=profile;Alphabet=alphabet;AlphabetOffset=offset;this.context=context;this.cache=cache;}

    public static LegacyFirstOrdinaryCacheScorer Prepare(ReadOnlySpan<byte> resource,int sourceOffset,
        LegacyOrdinaryRowAcousticInput row,LegacyInkActivations activations,CancellationToken cancellationToken=default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        //All dependent readers see the same private bytes, including inline
        //alphabet and cache tail; no unrelated bundle alphabet is substituted.
        byte[] snapshot=resource.ToArray();
        var profile=LegacyOrdinaryAcousticCacheProfile.ReadFirstSource(snapshot,sourceOffset,out var model,cancellationToken);
        int p=checked(sourceOffset+20+4+56);
        int marker=BinaryPrimitives.ReadInt32LittleEndian(snapshot.AsSpan(p));p+=4;if(marker==-1)p+=4;
        p+=4;
        if(snapshot[p++]!=1||BinaryPrimitives.ReadUInt32LittleEndian(snapshot.AsSpan(p))!=0x2b)
            throw new NotSupportedException("This owned first source requires its own inline acoustic alphabet.");
        p+=4;ushort version=BinaryPrimitives.ReadUInt16LittleEndian(snapshot.AsSpan(p));p+=2;
        if(version!=0x103)throw new NotSupportedException("Only checked first source alphabet103 supported.");
        var alphabet=LegacyInkAlphabet.Load(snapshot.AsSpan(p),version);
        if(alphabet.LabelCount<ModelWidth(model))throw new InvalidDataException("First network outputs exceed its source alphabet.");
        var context=LegacyFirstRowContext.Prepare(snapshot,sourceOffset,row,activations,cancellationToken);
        var cache=new LegacyOrdinaryAcousticClassCache(profile,alphabet,snapshot.AsSpan(p,16));
        return new(model,profile,alphabet,p,context,cache);
    }

    public LegacyFirstOrdinaryCacheScores Score(LegacySmoothedRowCutResult cuts,ReadOnlySpan<LegacyInkRange> ranges,
        short startBoundary,short endBoundary,int maximumRangesBeforeMerge,int maximumStrokesAfterMerge,
        ReadOnlySpan<byte> enabledSymbols,LegacyOrdinaryAcousticCacheContext cacheContext,
        ReadOnlySpan<LegacyAcousticCachePair> nativePairs,CancellationToken cancellationToken=default)
    {
        ArgumentNullException.ThrowIfNull(cacheContext);cancellationToken.ThrowIfCancellationRequested();
        if(enabledSymbols.Length!=Alphabet.LabelCount)throw new ArgumentException("Mask must belong to the owned source alphabet.",nameof(enabledSymbols));
        var glyph=context.Classify(cuts,ranges,startBoundary,endBoundary,maximumRangesBeforeMerge,maximumStrokesAfterMerge,
            enabledSymbols[..ModelWidth(Model)],cancellationToken);
        var final=cache.Build(glyph.IntegerCosts.Span,glyph.MergedGeometry.Strokes,enabledSymbols,cacheContext,nativePairs,cancellationToken);
        return new(glyph,final);
    }

    private static int ModelWidth(LegacyFirstAcousticModel model)=>model.TrajectoryNetwork.OutputWidth;
}
