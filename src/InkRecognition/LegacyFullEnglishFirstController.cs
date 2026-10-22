using System.Buffers.Binary;
using System.Runtime.CompilerServices;
[assembly:InternalsVisibleTo("LegacyFullEnglishFirstConfidence.Probe")]
namespace InkRecognition;

/// <summary>Fresh full-FIRST confidence-complete owner only. No primary/context
/// winner, WORD, source720, finalized parent guide or application-default change.</summary>
public sealed class LegacyFullEnglishFirstController
{
    public LegacyFullEnglishFirstParentInput Parent {get;}
    private readonly object gate=new();
    private readonly Dictionary<LegacyFullEnglishFirstAcousticCache,Dictionary<int,float[]>> completed=[];
    private int groups;
    private bool running,entered,faulted,complete;
    private LegacyFullEnglishFirstConfidenceAccumulator? pending;
    private Action<LegacyFullEnglishFirstAcousticCache,int,LegacyFullEnglishFirstConfidenceAccumulator>? inspection;
    internal LegacyAcousticConfidenceLabelMap Map {get;}
    internal ushort Closed {get;}
    public bool IsComplete {get{lock(gate)return complete;}}
    public bool IsFaulted {get{lock(gate)return faulted;}}
    public int CompletedGroupCount {get{lock(gate)return groups;}}
    public long LogicalCacheAndConfidenceEnvelopeBytes {get{lock(gate)return Envelope(groups,Parent.Children.Count);}}
    private LegacyFullEnglishFirstController(LegacyFullEnglishFirstParentInput parent,CancellationToken token)
    {
        Parent=parent;token.ThrowIfCancellationRequested();parent.RequireConfidencePristine();
        if(parent.LogicalSourcePayloadEnvelopeBytes>15L*1024*1024)throw new ArgumentException("B1 source logical15MiB envelope exceeded.");
        _=Envelope(4096,parent.Children.Count);
        var model=parent.Model;byte[] A(string name)=>model.Fields.Single(f=>f.Owner=="21D"&&f.Destination==name).CopyBytes();
        if(BinaryPrimitives.ReadInt32LittleEndian(A("108"))!=0||BinaryPrimitives.ReadInt32LittleEndian(A("128"))!=0||
           model.Slots.Take(4).Any(s=>s.Mode!=2||s.Networks.Count!=1)||model.Slots.Skip(4).Any(s=>s.Networks.Count!=0)||
           BinaryPrimitives.ReadUInt16LittleEndian(A("EE"))!=10)throw new NotSupportedException("Actual full fresh scalar four-family profile required.");
        var alphabet=model.Source.Alphabet;byte[] header=alphabet.CopySerializedHeader();
        if(BinaryPrimitives.ReadUInt16LittleEndian(header.AsSpan(4))!=65535||!alphabet.TryGetLabel("\U000f0001",out ushort closed))throw new NotSupportedException("Actual source-backed closed service required.");Closed=closed;
        int count=BinaryPrimitives.ReadUInt16LittleEndian(A("C8"));if(count!=8)throw new NotSupportedException("Actual eight full groups required.");
        var knowledge=new LegacySymbolMaskKnowledgeGroup[count];byte[] ends=A("D0"),ids=A("D8");
        for(int i=0;i<count;i++)
        {
            token.ThrowIfCancellationRequested();int first=BinaryPrimitives.ReadUInt16LittleEndian(ends.AsSpan(2*i)),last=BinaryPrimitives.ReadUInt16LittleEndian(ends.AsSpan(2*i+2));
            var members=new ushort[last-first];for(int j=0;j<members.Length;j++)members[j]=BinaryPrimitives.ReadUInt16LittleEndian(ids.AsSpan(2*(first+j)));
            knowledge[i]=new(members,BinaryPrimitives.ReadSingleLittleEndian(A($"E0[{i}]")));
        }
        Map=new(alphabet,header,182,knowledge,token);if(Map.TryResolveOutputLabel(Closed,out _))throw new NotSupportedException("Closed must use actual source service.");
    }
    public static LegacyFullEnglishFirstController Prepare(LegacyFullEnglishFirstParentInput parent,CancellationToken token=default)
    {ArgumentNullException.ThrowIfNull(parent);return new(parent,token);}
    internal static long Envelope(int groups,int children)
    {
        if(groups is<0 or>4096||children is<1 or>256)throw new ArgumentOutOfRangeException(nameof(groups));
        long extra=checked(65536L+256L*children+256L*groups);long bytes=checked(LegacyFullEnglishFirstBudget.CacheEnvelope(groups,children)+extra);
        if(bytes>17L*1024*1024)throw new ArgumentException("B1 combined logical17MiB cache envelope exceeded.");return bytes;
    }
    public void Run(LegacyCursiveSeparationTable table,CancellationToken token=default)=>RunCore(table,null,token);
    internal void RunPrepared(LegacyCursiveSeparationTable table,Action<string> checkpoint,CancellationToken token=default)=>RunCore(table,checkpoint,token);
    internal void RunInspected(LegacyCursiveSeparationTable table,Action<LegacyFullEnglishFirstAcousticCache,int,LegacyFullEnglishFirstConfidenceAccumulator> inspect,CancellationToken token=default)=>RunCore(table,null,token,inspect);
    private void RunCore(LegacyCursiveSeparationTable table,Action<string>? checkpoint,CancellationToken token,Action<LegacyFullEnglishFirstAcousticCache,int,LegacyFullEnglishFirstConfidenceAccumulator>? inspect=null)
    {
        token.ThrowIfCancellationRequested();lock(gate){if(running||entered)throw new InvalidOperationException("B1 owner is one-use.");running=true;inspection=inspect;}
        try{Parent.RunWithConfidence(table,this,checkpoint,token);token.ThrowIfCancellationRequested();lock(gate)complete=true;}
        catch{lock(gate)if(entered)faulted=true;throw;}
        finally{lock(gate){running=false;pending=null;inspection=null;}}
    }
    //Called only under the parent's entry gate, after its table/cancel preflight.
    internal void Enter(LegacyFullEnglishFirstParentInput parent)
    {lock(gate){if(!ReferenceEquals(parent,Parent)||!running||entered)throw new InvalidOperationException("Same fresh B1 invocation required.");entered=true;}}
    internal LegacyFullEnglishFirstConfidenceAccumulator Begin(LegacyFullEnglishFirstAcousticCache cache,int group,CancellationToken token)
    {
        token.ThrowIfCancellationRequested();LegacyFullEnglishFirstConfidenceAccumulator result;Action<LegacyFullEnglishFirstAcousticCache,int,LegacyFullEnglishFirstConfidenceAccumulator>? observe;lock(gate)
        {
            if(!running||!entered||faulted||complete||pending is not null||!ReferenceEquals(cache.Owner.Child.Owner,Parent)||!ReferenceEquals(cache.Model,Parent.Model)||cache.KnowledgeFlags!=0x21400||!cache.Owner.IsLive(group))throw new ArgumentException("Same active actual full group and no pending accumulator required.");
            _=Envelope(groups+1,Parent.Children.Count); //Before transient/per-group growth.
            if(completed.TryGetValue(cache,out var rows)&&rows.ContainsKey(group))throw new InvalidOperationException("Duplicate group entry.");
            pending=new(this,cache,group);result=pending;observe=inspection;
        }
        observe?.Invoke(cache,group,result);return result;
    }
    internal void Publish(LegacyFullEnglishFirstCachedGroup actual,LegacyFullEnglishFirstConfidenceAccumulator accumulator,CancellationToken token)
    {
        token.ThrowIfCancellationRequested();lock(gate)CheckPublication(actual,accumulator);
        if(!ReferenceEquals(actual,actual.Owner.ScoreGroup(actual.GroupId)))throw new ArgumentException("Canonical completed actual group required.");
        var values=accumulator.Finish(actual,token);token.ThrowIfCancellationRequested();lock(gate)
        {
            CheckPublication(actual,accumulator);
            _=Envelope(groups+1,Parent.Children.Count);token.ThrowIfCancellationRequested();
            if(!completed.TryGetValue(actual.Owner,out var rows)){rows=[];completed.Add(actual.Owner,rows);}rows.Add(actual.GroupId,values);groups++;pending=null;
        }
    }
    private void CheckPublication(LegacyFullEnglishFirstCachedGroup actual,LegacyFullEnglishFirstConfidenceAccumulator accumulator)
    {
        if(!running||!entered||faulted||complete||!ReferenceEquals(pending,accumulator)||!accumulator.Matches(this,actual)||
           !ReferenceEquals(actual.Owner.Owner.Child.Owner,Parent)||!ReferenceEquals(actual.Owner.Model,Parent.Model)||actual.Owner.KnowledgeFlags!=0x21400||!actual.Owner.Owner.IsLive(actual.GroupId))
            throw new ArgumentException("Same active pending accumulator, parent and actual full group required.");
    }
    /// <summary>Detached historical data, readable after fault; only IsComplete
    /// establishes confidence completion, never a word acceptance authority.</summary>
    public LegacyFullEnglishFirstConfidence ReadGroup(LegacyFullEnglishFirstCachedGroup actual)
    {
        ArgumentNullException.ThrowIfNull(actual);if(!ReferenceEquals(actual,actual.Owner.ScoreGroup(actual.GroupId)))throw new ArgumentException("Canonical actual cached group required.");lock(gate)
        {
            if(!completed.TryGetValue(actual.Owner,out var rows)||!rows.TryGetValue(actual.GroupId,out var values))throw new ArgumentException("Same completed actual group required.");
            return new(this,actual,values);
        }
    }
}
