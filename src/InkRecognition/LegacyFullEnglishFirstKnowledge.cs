using System.Buffers.Binary;
using System.Security.Cryptography;
namespace InkRecognition;

/// <summary>Actual full English root247/selected249 and its four FIRST119 nets.
/// No lite owner, accepted word, finalized parent guide or CTC frame is implied.</summary>
public sealed class LegacyFullEnglishFirstKnowledge
{
    public const string ResourceSha256="355f15d581dee2baeb092937cef69ae8db321ce3f60e28497c13e628a3b72927";
    public LegacyFirstAcoustic119WireModel Model{get;}
    public LegacySource247WireLayout Parent=>Model.ParentSource;
    public LegacySource247WireLayout Source=>Model.Source;
    private readonly LegacyPreparedNetwork107Evaluator[] evaluators;
    private LegacyFullEnglishFirstKnowledge(LegacyFirstAcoustic119WireModel model,CancellationToken token)
    {
        Model=model;var activations=LegacyOwnedFirst119Activations.Prepare(model,token);
        evaluators=Enumerable.Range(0,4).Select(i=>activations.CreatePreparedEvaluator(i,token)).ToArray();
    }
    public static LegacyFullEnglishFirstKnowledge Load(ReadOnlySpan<byte> fullResource,CancellationToken token=default)
    {
        token.ThrowIfCancellationRequested();_=LegacyFullEnglishFirstBudget.SourceEnvelope(4096,8192,256);if(fullResource.Length!=3783049)throw new ArgumentException("Exact bounded full English resource required.");
        //Snapshot the bounded caller-owned span before hashing/parsing. No
        //subsequent constructor reads mutable external resource storage.
        var owned=new byte[fullResource.Length];
        for(int at=0;at<owned.Length;at+=65536){token.ThrowIfCancellationRequested();fullResource.Slice(at,Math.Min(65536,owned.Length-at)).CopyTo(owned.AsSpan(at));}
        fullResource=owned;
        using var hash=IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        for(int at=0;at<fullResource.Length;at+=65536){token.ThrowIfCancellationRequested();hash.AppendData(fullResource.Slice(at,Math.Min(65536,fullResource.Length-at)));}
        if(!Convert.ToHexString(hash.GetHashAndReset()).Equals(ResourceSha256,StringComparison.OrdinalIgnoreCase))throw new InvalidDataException("Actual full resource fingerprint required.");
        var parent=LegacySource247WireLayout.Read(fullResource,0,token);
        var selected=parent.Children.Single(c=>c.Kind==0x249);
        var model=LegacyFirstAcoustic119WireModel.ReadSelectedFirst(fullResource,parent,selected,token);
        if(selected.Offset!=8827||selected.End!=373961||model.Source.Offset!=8843||model.Source.End!=373957||model.Source.Alphabet.LabelCount!=217||model.SourceScoreExponent!=.75f)throw new InvalidDataException("Actual full selected249/source ownership required.");
        int[] widths=[219,201,276,276];
        for(int i=0;i<7;i++){var s=model.Slots[i];if(i<4?s.First||!s.Second||s.Mode!=2||s.Networks.Count!=1||s.Networks[0].InputWidth!=widths[i]||s.Networks[0].OutputWidth!=182:s.First||s.Second||s.Mode!=0||s.Weight!=0||s.Networks.Count!=0)throw new InvalidDataException("Actual full four-network profile required.");}
        byte[] B(string name)=>model.Fields.Single(f=>f.Owner=="profile0"&&f.Destination==name).CopyBytes();
        if(BinaryPrimitives.ReadUInt16LittleEndian(B("14"))!=0x1011||BinaryPrimitives.ReadUInt16LittleEndian(B("28"))!=0x1011||B("88")[0]!=1||B("89")[0]!=2)throw new InvalidDataException("Actual baseline-absent2B1 profile required.");
        token.ThrowIfCancellationRequested();return new(model,token);
    }
    internal LegacyPreparedNetwork107Result Evaluate(int family,short[] actualInput,CancellationToken token)=>evaluators[family].EvaluatePreparedSourceUnmasked(actualInput,token);
}

/// <summary>Conservative independent-library limits; these are not SDK/RSS limits.</summary>
public sealed record LegacyFullEnglishFirstBounds(int MaximumLiveGroups=4096,int MaximumNetworkEvaluations=16384)
{
    internal void Validate(){if(MaximumLiveGroups is<1 or>4096||MaximumNetworkEvaluations is<4 or>16384)throw new ArgumentOutOfRangeException(nameof(MaximumLiveGroups));}
}
internal sealed class LegacyFullEnglishFirstBudget
{
    internal readonly LegacyOrdinaryGraphWorkBudget Graph;
    private readonly LegacyFullEnglishFirstBounds bounds;
    private int groups,networks,children,preparedPoints,cutPoints;
    internal LegacyFullEnglishFirstBudget(LegacyFullEnglishFirstBounds bounds,CancellationToken token){bounds.Validate();this.bounds=bounds;_=CacheEnvelope(bounds.MaximumLiveGroups,256);Graph=new(token,32L*1024*1024);}
    internal static long SourceEnvelope(int rawPoints,int derivedPoints,int physical)
    {
        if(rawPoints is<1 or>4096||derivedPoints is<1 or>8192||physical is<1 or>256)throw new ArgumentOutOfRangeException(nameof(rawPoints));
        //Logical payload peak, NOT cumulative allocator work or process RSS:
        //caller-wire snapshot+root wire, six selected-wire/matrix views,
        //384 bytes per bounded retained/derived point for XY/key/map/stage
        //views,4KiB per physical/child slot and1MiB transient feature/cut work.
        long bytes=checked(2L*3783049+6L*(373957-8843)+derivedPoints*384L+physical*4096L+1048576);
        if(bytes>16L*1024*1024)throw new ArgumentException("Whole source/model logical snapshot envelope exceeded.");return bytes;
    }
    internal long CurrentCacheEnvelope=>CacheEnvelope(Volatile.Read(ref groups),children);
    internal void Children(int count){_=CacheEnvelope(0,count);children=count;}
    internal static long CacheEnvelope(int groups,int children)
    {
        if(groups is<0 or>4096||children is<0 or>256)throw new ArgumentOutOfRangeException(nameof(groups));
        //Final int220 + conservatively12-byte ranked entries +256 bytes per
        //row for objects/arrays/dictionary capacity. Each child receives4KiB
        //for masks/groups/profile/label map;64KiB fixed table slack. Neural
        //vectors/features/NN histories are transient, never retained here.
        long bytes=checked(65536L+4096L*children+(220L*4+220L*12+256)*groups);
        if(bytes>16L*1024*1024)throw new ArgumentException("Whole retained cache envelope exceeded.");return bytes;
    }
    internal void Groups(int count,CancellationToken token){token.ThrowIfCancellationRequested();if(count<0||count>bounds.MaximumLiveGroups-groups)throw new ArgumentException("Whole parent live-group budget exceeded.");_=CacheEnvelope(groups+count,children);groups+=count;}
    internal static int AddDerivedPoints(int retained,int incoming)
    {if(retained is<0 or>8192||incoming is<1 or>4096||incoming>8192-retained)throw new ArgumentException("Whole parent derived-point snapshot budget exceeded.");return retained+incoming;}
    internal void PreparedPoints(int count,CancellationToken token){token.ThrowIfCancellationRequested();preparedPoints=AddDerivedPoints(preparedPoints,count);}
    internal void CutPoints(int count,CancellationToken token){token.ThrowIfCancellationRequested();cutPoints=AddDerivedPoints(cutPoints,count);}
    internal void Network(CancellationToken token)
    {while(true){token.ThrowIfCancellationRequested();int before=Volatile.Read(ref networks);if(before>=bounds.MaximumNetworkEvaluations)throw new ArgumentException("Whole parent NN-work budget exceeded.");if(Interlocked.CompareExchange(ref networks,before+1,before)==before)return;}}
    internal int NetworkCount=>Volatile.Read(ref networks);
}
