using System.Buffers.Binary;
using System.Security.Cryptography;
namespace InkRecognition;

/// <summary>Actual global frontend158/1B8/1C0 callbacks. Every group reads the
/// existing D0 snapshot and original193860 cache owner; no classifier is run.
/// Candidate24.field0 normalized weights remain a separate confidence API.</summary>
public sealed class LegacyFreshMainGlobalGlyphScoring
{
    private readonly LegacyCursiveCategoryProfile categories;
    private readonly ushort blank,closed;
    private readonly ushort[] bypass;
    private readonly Dictionary<int,LegacyNativeGlyphScoreCache> caches=[];
    private readonly object gate=new();
    public LegacyFreshMainGlobalSourceSymbolCallbacks Callbacks{get;}
    public LegacyFreshMainGlobalSourceGeometry Geometry=>Callbacks.Geometry;
    public LegacyFreshMainGlobalGlyphConfidence Confidence=>Geometry.Confidence;
    private LegacyFreshMainGlobalGlyphScoring(ReadOnlySpan<byte> resource,LegacyFreshMainGlobalSourceSymbolCallbacks callbacks,CancellationToken token)
    {
        Callbacks=callbacks;var owner=Geometry.Policy.Owner;var alphabet=owner.Model.AcousticAlphabet;
        if(resource.Length!=572763||!Convert.ToHexString(SHA256.HashData(resource)).Equals(
            Geometry.Graph.Source.Source.SourceFingerprint,StringComparison.OrdinalIgnoreCase)||
            !ReferenceEquals(Confidence.Owner,owner)||Confidence.OwnerScoreChannel88!=0||owner.Acoustic.CostScale!=1)
            throw new ArgumentException("Same actual global resource/source/confidence and channel88 required.");
        //ReadOnlyMemory profiles can be recovered as writable arrays. Retain
        //a private checked category snapshot instead of an exposed owner map.
        var selected=LegacySourceSamplerCategoryOwner.ReadV10dSource(resource,0,token);
        var actual=owner.Symbols.Geometry.CategoryOwner;
        if(selected.CategoryProfileOffset!=actual.CategoryProfileOffset||selected.SamplerNodeOffset!=actual.SamplerNodeOffset)
            throw new ArgumentException("Exact selected main category-profile owner required.");
        categories=selected.Categories;
        blank=BinaryPrimitives.ReadUInt16LittleEndian(resource[99..]);
        ushort Label(string text)=>alphabet.TryGetLabel(text,out ushort symbol)?symbol:ushort.MaxValue;
        closed=Label("\U000f0001");
        bypass=[closed,Label("\U000f0004"),Label("\U000f0008"),Label("\U000f0002"),Label("\U000f0003"),
            BinaryPrimitives.ReadUInt16LittleEndian(resource[107..]),BinaryPrimitives.ReadUInt16LittleEndian(resource[109..]),
            BinaryPrimitives.ReadUInt16LittleEndian(resource[111..])];
        token.ThrowIfCancellationRequested();
    }
    public static LegacyFreshMainGlobalGlyphScoring Prepare(ReadOnlySpan<byte> resource,
        LegacyFreshMainGlobalSourceSymbolCallbacks callbacks,CancellationToken token=default)
    {ArgumentNullException.ThrowIfNull(callbacks);token.ThrowIfCancellationRequested();return new(resource,callbacks,token);}
    public float ReadSymbolScore(int node,ushort arc,ushort symbol,CancellationToken token=default)
        =>GetCache(Geometry.Group(node,arc,token),token).ReadSymbolScore(symbol,Confidence.OwnerScoreChannel88,token);
    public float ReadFirstCandidateScore(int node,ushort arc,CancellationToken token=default)
        =>GetCache(Geometry.Group(node,arc,token),token).ReadFirstCandidateScore(token);
    public byte[] CreateRequest(int node,ushort arc,ushort symbol,bool useEndCategory,CancellationToken token=default)
    {_=Geometry.Group(node,arc,token);return LegacyMainGlyphScoringKernel.CreateRequest(categories,closed,node,arc,symbol,useEndCategory);}
    public int ReadRequestCost(ReadOnlySpan<byte> request16,CancellationToken token=default)
    {
        token.ThrowIfCancellationRequested();if(request16.Length!=16)throw new ArgumentException("Complete glyph request16 required.");
        int node=BinaryPrimitives.ReadInt32LittleEndian(request16);ushort arc=BinaryPrimitives.ReadUInt16LittleEndian(request16[4..]);
        return GetCache(Geometry.Group(node,arc,token),token).ReadRequestCost(request16,token);
    }
    internal LegacyNativeGlyphScoreCache GetCache(int group,CancellationToken token)
    {
        lock(gate)
        {
            token.ThrowIfCancellationRequested();
            //Validate the same actual source even for a cache hit. The owned
            //confidence performs no replacement NN and rejects an invalidated
            //session; a private glyph cache must not bypass that lifetime.
            var actual=Confidence.ReadGroup(group,token);
            if(caches.TryGetValue(group,out var cached))return cached;
            var candidates=actual.Candidates.Span;
            int[] final=actual.CopyFinalClassCosts();int count=Confidence.Owner.Model.AcousticAlphabet.LabelCount;
            if(final.Length!=count+3)throw new InvalidDataException("Actual complete final class cache required.");
            //1BC680 stores class[count] in group38.field18; the final snapshot
            //already owns it. Never rerun four NN families to recover fallback.
            int geometry=Geometry.Policy.Services.ReadGlobalGeometryCost(group,token);
            var value=LegacyMainGlyphScoringKernel.CreateCache(candidates,geometry,categories.Prototypes.Length,
                blank,final[count],actual.GroupPowerScale,bypass,token);
            token.ThrowIfCancellationRequested();caches.Add(group,value);return value;
        }
    }
}
