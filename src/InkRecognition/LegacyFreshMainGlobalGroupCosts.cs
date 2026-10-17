using System.Security.Cryptography;
using System.Runtime.InteropServices;
namespace InkRecognition;

/// <summary>Actual193860/195520 geometry paired with the selected global D0
/// class cache for frontend128. Stored range order and both source guides are
/// retained; neither chronological normalization order nor a single-line guide
/// is substituted. It does not implement1941B0 transition caches.</summary>
public sealed class LegacyFreshMainGlobalGroupCosts
{
    private readonly LegacyCursivePreparedInk ink;
    private readonly LegacyCursiveSeparationProfile profile;
    private readonly float[] table;
    private readonly Dictionary<int,int> geometry=[];
    private readonly int categoryCount;
    private int active;
    public LegacyFreshMainGlobalCaseSession Owner { get; }
    public LegacyFreshMainCompactOrdinaryGraph Graph=>Owner.Graph;
    private LegacyFreshMainGlobalGroupCosts(LegacyFreshMainGlobalCaseSession owner,
        LegacyCursiveSeparationProfile profile,float[] table,CancellationToken token)
    {
        Owner=owner;this.profile=profile;this.table=table;
        ink=owner.Symbols.Geometry.CopyPreparedInk(token);
        categoryCount=owner.Symbols.Geometry.CategoryOwner.Categories.Prototypes.Length;
        if(categoryCount is <1 or >127)throw new InvalidDataException("Actual signed-byte composite geometry domain required.");
    }
    public static LegacyFreshMainGlobalGroupCosts Prepare(ReadOnlySpan<byte> resource,
        LegacyFreshMainGlobalCaseSession owner,LegacyCursiveSeparationTable table,CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(owner);ArgumentNullException.ThrowIfNull(table);token.ThrowIfCancellationRequested();
        var source=owner.Graph.Source.Source;
        if(resource.Length!=572763||!Convert.ToHexString(SHA256.HashData(resource)).Equals(source.SourceFingerprint,StringComparison.OrdinalIgnoreCase)||
            !ReferenceEquals(owner.Acoustic.Owner.Geometry.Graph,owner.Graph)||source.CategoryOwner.SelectedSourceOffset!=0)
            throw new ArgumentException("Actual selected global graph/acoustic source owner required.");
        var profile=LegacyCursiveSeparationProfile.ReadV106Node(resource,source.CategoryOwner.SamplerNodeOffset);
        var values=table.Values.ToArray();
        if(!SHA256.HashData(MemoryMarshal.AsBytes(values.AsSpan())).AsSpan().SequenceEqual(
            Convert.FromHexString("B23ED52A3A10808EAB597A8F371A7BBB66BDE2FAB5FE2666431B1B192ABC1BAF")))
            throw new ArgumentException("Actual pinned grouping table required before retaining an owned copy.");
        return new(owner,profile,values,token);
    }
    public int ReadGeometryCost(int group,CancellationToken token=default)
    {
        Enter(token);try{return GeometryCost(group,token);}finally{Exit();}
    }
    /// <summary>Final class costs require the current actual D0 span/ordinal.
    /// The result owns copies, so recovering its ReadOnlyMemory arrays cannot
    /// alter a later read from the same source cache.</summary>
    public LegacyNativeWordAcousticGroupCosts ReadCosts(LegacyOwnedGlobalMainWordSpan span,
        int node,ushort ordinal,CancellationToken token=default)
    {
        Enter(token);try
        {
            var arcs=Graph.State.GetNode(node);if(ordinal>=arcs.Count)throw new ArgumentOutOfRangeException(nameof(ordinal));
            int group=checked((int)arcs[ordinal].GroupId);int value=GeometryCost(group,token);
            //Validate the actual class-cache lease after geometry preparation.
            var classes=Owner.CopyClassCosts(span,node,ordinal,token);
            return new(classes,Enumerable.Repeat(value,categoryCount).ToArray());
        }
        finally{Exit();}
    }
    private int GeometryCost(int group,CancellationToken token)
    {
        token.ThrowIfCancellationRequested();if((uint)group>=(uint)Graph.State.Groups.Count)throw new ArgumentOutOfRangeException(nameof(group));
        if(geometry.TryGetValue(group,out int value))return value;
        var frame=Owner.Symbols.Geometry.ReadFrame(group,token);var ranges=Graph.State.Groups[group].CopyRanges();
        value=LegacyCursiveGroupSeparationCost.Evaluate(ink,ranges,frame.Height,profile.GapOffset,profile.Slope,table,profile.MaximumCost,cancellationToken:token);
        token.ThrowIfCancellationRequested();geometry.Add(group,value);return value;
    }
    private void Enter(CancellationToken token)
    {
        token.ThrowIfCancellationRequested();if(Interlocked.CompareExchange(ref active,1,0)!=0)
            throw new InvalidOperationException("Global geometry cache calls must be serialized.");
    }
    private void Exit()=>Volatile.Write(ref active,0);
}
