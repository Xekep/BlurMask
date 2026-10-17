using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Runtime.CompilerServices;
[assembly:InternalsVisibleTo("LegacyMainGlobalWordTransitions.Probe")]
namespace InkRecognition;

/// <summary>Actual fresh ordinary main118/140 and compact selector callbacks
/// for the shared global graph. Supports kinds0/2/5/6/7, actual fresh pending
/// separators and ordinary source descriptors. Does not infer the outer word
/// scheduler, protocol/opaque records or custom boundary probabilities.</summary>
public sealed class LegacyFreshMainGlobalWordTransitions
{
    private const int Impossible=0x4000000;
    private readonly LegacyMainDescriptorCutStep[] descriptors;
    private readonly Dictionary<int,int> formats=[];
    private readonly Dictionary<int,int> field20=[];
    private readonly Dictionary<int,int> lines=[];
    private readonly bool[] wholeStarts;
    private readonly byte[][] contexts;
    private readonly int[] symbolMatrix;
    private readonly ushort replacement,apostrophe,quote;
    private readonly bool strictSymbolMatrix;
    private readonly float pathWeight,overlapWeight;
    private readonly LegacyMainGraphAppendBounds[] mutableBounds;
    private int active;
    public LegacyFreshMainGlobalTransitionCache Cache {get;}
    public LegacyFreshMainCompactOrdinaryGraph Graph=>Cache.Graph;
    private LegacyFreshMainGlobalWordTransitions(LegacyFreshMainGlobalTransitionCache cache,int[] matrix,
        ushort replacement,ushort apostrophe,ushort quote,float pathWeight,float overlapWeight)
    {
        Cache=cache;symbolMatrix=matrix;this.pathWeight=pathWeight;this.overlapWeight=overlapWeight;
        wholeStarts=Graph.State.Groups.Select(g=>g.CopyRanges().All(r=>r.StartPoint==0)).ToArray();
        mutableBounds=Graph.State.Groups.Select(g=>g.Bounds).ToArray();this.replacement=replacement;this.apostrophe=apostrophe;this.quote=quote;
        strictSymbolMatrix=Graph.Source.Source.SpanProfile.NativeSecondByte!=0;
        descriptors=Graph.Source.Source.Steps.Where(s=>!s.IsPendingSeparator).ToArray();
        foreach(var descriptor in descriptors)
        {
            var bytes=descriptor.CopyDescriptor();formats.Add(descriptor.DescriptorOrdinal,BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(24)));
            field20.Add(descriptor.DescriptorOrdinal,BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(20)));
            lines.Add(descriptor.DescriptorOrdinal,descriptor.LineId);
            if(descriptor.Kind!=0||formats[descriptor.DescriptorOrdinal]==0x2000000)
                throw new NotSupportedException("Actual ordinary main descriptors without opaque source-format branches required.");
        }
        contexts=Graph.Source.State.Contexts.Select(c=>c.CopyPrefix()).ToArray();
        //The actual raw source produces only the pending-kind3 flag0 prefix.
        //Other19EB10 prefixes are prepared/custom policies, not this factory.
        foreach(var prefix in contexts)
            if(prefix[0]!=1||prefix.AsSpan(1,5).ContainsAnyExcept((byte)0)||
                BinaryPrimitives.ReadInt32LittleEndian(prefix.AsSpan(8))!=0)
                throw new NotSupportedException("Only the owned fresh pending separator prefix is restored.");
        foreach(var group in Graph.State.Groups)
            if(group.Kind07!=0||group.Flags05!=0)throw new NotSupportedException("Fresh ordinary groups with actual zero endpoint nibbles required.");
    }
    public static LegacyFreshMainGlobalWordTransitions Prepare(ReadOnlySpan<byte> resource,
        LegacyFreshMainGlobalTransitionCache cache,CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(cache);token.ThrowIfCancellationRequested();var source=cache.Graph.Source.Source;
        if(resource.Length!=572763||!Convert.ToHexString(SHA256.HashData(resource)).Equals(source.SourceFingerprint,StringComparison.OrdinalIgnoreCase)||
            source.GraphOffset!=189379||source.CategoryOwner.SamplerNodeOffset!=188157)
            throw new ArgumentException("Actual owned global cache/source resource required.");
        var span=LegacyCursiveLatticeSpanProfile.ReadV10aResourceNode(resource,source.GraphOffset);
        if(span!=source.SpanProfile)throw new InvalidDataException("Graph resource/source metadata differ.");
        var matrix=new int[72];for(int i=0;i<matrix.Length;i++)matrix[i]=BinaryPrimitives.ReadInt32LittleEndian(resource[(source.GraphOffset+18+i*4)..]);
        var alphabet=cache.Owner.Model.AcousticAlphabet;
        if(!alphabet.TryGetLabel("\ufffd",out ushort replacement)||!alphabet.TryGetLabel("'",out ushort apostrophe)||!alphabet.TryGetLabel("\"",out ushort quote))
            throw new InvalidDataException("Actual main alphabet protocol/quote labels required.");
        token.ThrowIfCancellationRequested();var result=new LegacyFreshMainGlobalWordTransitions(cache,matrix,replacement,apostrophe,quote,
            BinaryPrimitives.ReadSingleLittleEndian(resource[189165..]),BinaryPrimitives.ReadSingleLittleEndian(resource[189169..]));
        token.ThrowIfCancellationRequested();return result;
    }
    public LegacyNativeHigherArcTransitionOperations CreateOperations()=>new(Calculate118,Validate140,ReadSuccessors);

    public bool Calculate118(LegacyNativeWordRecombinationRequest request,CancellationToken token=default)
    {Enter(token);try{return Calculate118Core(request,token);}finally{Volatile.Write(ref active,0);}}
    private bool Calculate118Core(LegacyNativeWordRecombinationRequest request,CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(request);token.ThrowIfCancellationRequested();
        var current=Current(request);byte kind=request.ReadByte(28);
        if(kind is not (0 or 2 or 5 or 6 or 7))throw new NotSupportedException("Supported ordinary main request kinds are0/2/5/6/7.");
        int before=request.Cost.Value;bool accepted;
        if(kind==2&&!wholeStarts[(int)current.GroupId])
        {
            //1525E0 tests graph188 before sampler dispatch. The exceptional
            //previous kind1 provider is outside this owned ordinary graph.
            request.Cost.Value=Impossible;Write32(request,48,Impossible);return false;
        }
        //Fresh19C970 clears2D1 and1A6980 writes only2D0.1AD890 source-kind9
        //therefore succeeds before sampler costs, after the frontend clip gate.
        if(request.ReadUInt32(64)==9)
        {token.ThrowIfCancellationRequested();return true;}
        //195C40's ordinary default kind6 returns true without graph callbacks.
        if(kind==6)accepted=true;
        else
        {
            var prior=Previous(request);byte status=kind switch
            {0=>Status100(request,current,prior),2=>Status118(request,current,prior),7=>Status110(request,current,prior),_=>Status108(request,current,prior)};
            if(kind==7)
                //195C40 calls21B.V110. Status0/2 rejects without converting
                //the otherwise finite cost to IMP; status3 accepts unchanged.
                accepted = (status & ~2) != 0 && status != 4;
            else if(kind==5)
            {
                if(status!=0)accepted=status!=4;
                else{request.Cost.Value=unchecked(request.Cost.Value+Cache.ReadState(request.ReadInt32(0),request.ReadUInt16(12),token).BlankCost);accepted=request.Cost.Value<request.ReadInt32(52);}
            }
            else if(kind==0&&status is 0 or 1||kind==2&&status==0)
            {
                if(request.ReadInt32(4)<0)throw new NotSupportedException("Native translation requires an actual nonroot previous source cache row.");
                var a=Cache.ReadState(request.ReadInt32(0),request.ReadUInt16(12),token);
                var b=Cache.ReadState(request.ReadInt32(4),request.ReadUInt16(14),token);
                int correction;
                if(kind==0)correction=request.ReadUInt32(64)is 7 or 8?a.TripleJoinCost:a.JoinCost;
                else
                {
                    int first=request.ReadInt32(20),last=request.ReadUInt32(68)==10?3:request.ReadInt32(24);
                    if(first is <0 or >5||last is <0 or >5)throw new NotSupportedException("Native ordinary matrix cache requires its six-category index domain.");
                    correction=a.CategoryMatrix.Span[first+last*6];
                }
                float movement=Math.Max(a.Bounds.MinimumX-b.Bounds.MinimumX,a.Bounds.MaximumX-b.Bounds.MaximumX)/a.PrimaryHeight;
                float term=movement<=Cache.Profile.TranslationThreshold?(movement-Cache.Profile.TranslationThreshold)*Cache.Profile.GapSlope:0;
                if(!float.IsFinite(term)||term>=2147483648f||term<int.MinValue)throw new NotSupportedException("Native translation exceeds checked scalar conversion.");
                request.Cost.Value=unchecked(request.Cost.Value-(int)term*8192+correction);accepted=request.Cost.Value<request.ReadInt32(52);
            }
            else accepted=status!=4;
        }
        Write32(request,48,accepted?unchecked(request.ReadInt32(48)+request.Cost.Value-before):Impossible);
        token.ThrowIfCancellationRequested();return accepted;
    }
    public bool Validate140(LegacyNativeWordRecombinationRequest request,CancellationToken token=default)
    {Enter(token);try{return Validate140Core(request,token);}finally{Volatile.Write(ref active,0);}}
    private bool Validate140Core(LegacyNativeWordRecombinationRequest request,CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(request);token.ThrowIfCancellationRequested();_=Current(request);
        if(request.ReadByte(28)is not(0 or 2 or 5 or 6 or 7))throw new NotSupportedException("Supported ordinary main request kinds are0/2/5/6/7.");
        //194170 sets graph191=1. Kind6 precedes main192=false.
        if(request.ReadByte(28)==6)return ValidatePath140(request,token);
        return true;
    }
    //For kind6 only, request04 is a packed context key, not a source-node
    //ordinal.1AACD0 returns the FIRST path box and expands later group boxes.
    //The side effects remain private to this native-transition owner.
    private bool ValidatePath140(LegacyNativeWordRecombinationRequest request,CancellationToken token)
    {
        var current=Current(request);int endNode=request.ReadInt32(8);ushort endArc=request.ReadUInt16(16);
        if((uint)endNode>=(uint)Graph.State.NodeCount||endArc>=Graph.State.GetNode(endNode).Count)
            throw new ArgumentOutOfRangeException(nameof(request));
        uint start=unchecked((uint)request.ReadInt32(4)),end=Graph.State.GetNode(endNode)[endArc].EndContextId;
        var path=new List<int>();int visits=0;bool found=false;int startNode=DecodeContextNode(start);
        if((uint)startNode>=(uint)Graph.State.NodeCount)throw new ArgumentOutOfRangeException(nameof(request));
        foreach(var arc in Graph.State.GetNode(startNode))
            if(arc.StartContextId==start&&FindPath(arc,end,path,ref visits,token)){found=true;break;}
        if(!found||path.Count==0)return false;
        var updates=new Dictionary<int,LegacyMainGraphAppendBounds>();var pathBox=mutableBounds[path[0]];
        foreach(int g in path.Skip(1))
        {
            var b=updates.TryGetValue(g,out var changed)?changed:mutableBounds[g];
            updates[g]=new(Math.Min(b.MinimumX,pathBox.MinimumX),Math.Min(b.MinimumY,pathBox.MinimumY),
                Math.Max(b.MaximumX,pathBox.MaximumX),Math.Max(b.MaximumY,pathBox.MaximumY));
        }
        int group=checked((int)current.GroupId);var box=updates.TryGetValue(group,out var updated)?updated:mutableBounds[group];
        int added=0;bool accepted;
        if(Graph.State.Groups[group].Flags06!=0||!Intersects(box,pathBox))
        {request.Cost.Value=Impossible;accepted=false;}
        else
        {
            float a=box.MinimumX<=pathBox.MinimumX?(pathBox.MinimumX-box.MinimumX)/15f:box.MinimumX-pathBox.MinimumX;
            float b=pathBox.MaximumX<=box.MaximumX?(box.MaximumX-pathBox.MaximumX)/15f:pathBox.MaximumX-box.MaximumX;
            float c=box.MinimumY<=pathBox.MinimumY?(pathBox.MinimumY-box.MinimumY)/15f:box.MinimumY-pathBox.MinimumY;
            float d=pathBox.MaximumY<=box.MaximumY?(box.MaximumY-pathBox.MaximumY)/15f:pathBox.MaximumY-box.MaximumY;
            float height=Cache.ReadState(request.ReadInt32(0),request.ReadUInt16(12),token).PrimaryHeight;
            added=Round(((a+b+c+d)*pathWeight*8192f)/height);
            int prior=DecodeContextNode(start)-1;
            if(prior!=-1)
            {
                int extent=Graph.State.GetNode(prior).Count==0?0:Graph.State.GetNode(prior).Max(a=>a.Span)+1;
                float overlap=0;
                for(int span=0;span<extent;span++,prior--)
                {
                    if(prior<0)throw new InvalidDataException("Native path overlap scan escaped its ordinary prefix.");
                    foreach(var arc in Graph.State.GetNode(prior))if(arc.Span==span)
                    {
                        token.ThrowIfCancellationRequested();int g=checked((int)arc.GroupId);
                        var old=updates.TryGetValue(g,out var change)?change:mutableBounds[g];
                        if(Graph.State.Groups[g].Flags06==0&&Intersects(box,old))
                        {
                            float x=Math.Min(old.MaximumX-box.MinimumX,box.MaximumX-old.MinimumX);
                            float y=box.MaximumY-box.MinimumY,z=old.MaximumY-old.MinimumY;
                            float v=Math.Min(Math.Min(x,y),z);
                            overlap=Math.Max(overlap,v);
                        }
                    }
                }
                added=unchecked(added+Round((overlap*overlapWeight*8192f)/height));
            }
            request.Cost.Value=unchecked(request.Cost.Value+added);accepted=request.Cost.Value<request.ReadInt32(52);
        }
        token.ThrowIfCancellationRequested();foreach(var update in updates)mutableBounds[update.Key]=update.Value;
        return accepted;
    }
    private bool FindPath(LegacyMainCompactOrdinaryArc arc,uint end,List<int> path,ref int visits,CancellationToken token,int depth=0)
    {
        token.ThrowIfCancellationRequested();if(depth>512)throw new NotSupportedException("Portable path depth exceeds512 ordinary groups.");
        if(++visits>1048576)throw new NotSupportedException("Portable path traversal budget exceeded.");
        if(unchecked((int)arc.EndContextId)>unchecked((int)end))return false;
        int saved=path.Count,g=checked((int)arc.GroupId);if(Graph.State.Groups[g].Flags06==0)path.Add(g);
        if(arc.EndContextId==end)return true;
        int next=DecodeContextNode(arc.StartContextId)+1;
        if((uint)next>=(uint)Graph.State.NodeCount)throw new InvalidDataException("Native path selector escaped the owned graph.");
        foreach(ushort index in arc.CopySelector(1))
        {
            var row=Graph.State.GetNode(next);if(index>=row.Count)throw new InvalidDataException("Native path selector escaped the owned row.");
            if(FindPath(row[index],end,path,ref visits,token,depth+1))return true;
        }
        path.RemoveRange(saved,path.Count-saved);return false;
    }
    internal LegacyMainGraphAppendBounds[] CopyGeometryState()=>mutableBounds.ToArray();
    internal LegacyMainGraphAppendBounds ReadOwnedGeometryState(int group,CancellationToken token)
    {
        Enter(token);
        try
        {
            if((uint)group>=(uint)mutableBounds.Length)throw new ArgumentOutOfRangeException(nameof(group));
            return mutableBounds[group];
        }
        finally{Volatile.Write(ref active,0);}
    }
    private static int DecodeContextNode(uint key)=>unchecked((int)(key<<1))>>8;
    private static bool Intersects(LegacyMainGraphAppendBounds a,LegacyMainGraphAppendBounds b)=>
        (a.MinimumX<=b.MinimumX||a.MinimumX<=b.MaximumX)&&(b.MaximumX<=a.MaximumX||b.MinimumX<=a.MaximumX)&&
        (a.MinimumY<=b.MinimumY||a.MinimumY<=b.MaximumY)&&(b.MaximumY<=a.MaximumY||b.MinimumY<=a.MaximumY);
    private static int Round(float value)
    {
        value=value<=0?value-.5f:value+.5f;
        if(!float.IsFinite(value)||value>=2147483648f||value<int.MinValue)throw new NotSupportedException("Native path cost exceeds checked integer conversion.");
        return (int)value;
    }
    private LegacyMainCompactOrdinaryArc Current(LegacyNativeWordRecombinationRequest request)
    {
        var row=Graph.State.GetNode(request.ReadInt32(0));int ordinal=request.ReadUInt16(12);
        if(ordinal>=row.Count)throw new ArgumentOutOfRangeException(nameof(request));return row[ordinal];
    }
    private LegacyMainCompactOrdinaryGroup Previous(LegacyNativeWordRecombinationRequest request)
    {
        int node=request.ReadInt32(4);uint group;
        if(node==-1)group=Graph.State.Root.GroupId;
        else{var row=Graph.State.GetNode(node);int ordinal=request.ReadUInt16(14);if(ordinal>=row.Count)throw new ArgumentOutOfRangeException(nameof(request));group=row[ordinal].GroupId;}
        if(group>=(uint)Graph.State.Groups.Count)throw new NotSupportedException("Actual graph root/source group field does not identify an owned ordinary record.");
        return Graph.State.Groups[(int)group];
    }
    private byte Status100(LegacyNativeWordRecombinationRequest request,LegacyMainCompactOrdinaryArc arc,LegacyMainCompactOrdinaryGroup previous)
    {
        if((arc.Flags&8)!=0){request.Cost.Value=Impossible;return 4;}
        if(unchecked((short)arc.AuxiliaryContextIndex)>=0)
        {
            _=contexts[arc.AuxiliaryContextIndex];request.WriteByte(60,1);
            //The owned main106 sampler.VE0/1964A0 reads its proven242=false.
            request.Cost.Value=unchecked(request.Cost.Value+8192);return LimitedStatus(request);
        }
        var current=Graph.State.Groups[(int)arc.GroupId];
        if((request.ReadUInt16(36)==replacement)!=(request.ReadUInt16(38)==replacement)){request.Cost.Value=Impossible;return 4;}
        if(current.DescriptorOrdinal==previous.DescriptorOrdinal)return 0;
        request.Cost.Value=unchecked(request.Cost.Value+8192);return LimitedStatus(request);
    }
    private byte Status108(LegacyNativeWordRecombinationRequest request,LegacyMainCompactOrdinaryArc arc,LegacyMainCompactOrdinaryGroup previous)
    {
        if((arc.Flags&8)==0&&unchecked((short)arc.AuxiliaryContextIndex)<0&&Graph.State.Groups[(int)arc.GroupId].DescriptorOrdinal==previous.DescriptorOrdinal)return 0;
        request.Cost.Value=Impossible;return 4;
    }
    private byte Status110(LegacyNativeWordRecombinationRequest request,
        LegacyMainCompactOrdinaryArc arc, LegacyMainCompactOrdinaryGroup previous)
    {
        //1A83C0 ordering is significant: kind7 bit10 precedes bit8/context.
        //The constructor proves fresh descriptor.kind0, group.kind0 and
        //endpoint nibbles0. Custom/opaque source-format branches are excluded.
        if ((arc.Flags & 0x10) != 0) { request.Cost.Value = Impossible; return 4; }
        if ((arc.Flags & 8) != 0) return 3;
        if (unchecked((short)arc.AuxiliaryContextIndex) >= 0)
        {
            _ = contexts[arc.AuxiliaryContextIndex];
            return 3; //The owned pending prefix's first context byte is1.
        }
        var current = Graph.State.Groups[(int)arc.GroupId];
        if (current.DescriptorOrdinal == previous.DescriptorOrdinal &&
            field20[current.DescriptorOrdinal] is 0x2000001 or 0x2000002)
        {
            request.Cost.Value = Impossible;
            return 4;
        }
        return 0;
    }
    private byte Status118(LegacyNativeWordRecombinationRequest request,LegacyMainCompactOrdinaryArc arc,LegacyMainCompactOrdinaryGroup previous)
    {
        if((arc.Flags&8)!=0)return 3;
        if(unchecked((short)arc.AuxiliaryContextIndex)>=0){_=contexts[arc.AuxiliaryContextIndex];return 3;}
        ushort symbol=request.ReadUInt16(36),prior=request.ReadUInt16(38);
        if((symbol==replacement)!=(prior==replacement))return 3;
        var current=Graph.State.Groups[(int)arc.GroupId];
        if(current.DescriptorOrdinal==previous.DescriptorOrdinal)
        {
            if(field20[current.DescriptorOrdinal]==0x2000001){request.Cost.Value=Impossible;return 4;}
            return 0;
        }
        if(symbol==apostrophe||prior==apostrophe||symbol==quote||prior==quote)
        {
            int first=request.ReadInt32(20),last=request.ReadInt32(24);
            if(first is <0 or >6||last is <0 or >6)throw new NotSupportedException("Owned seven first-ID category domain required.");
            int value=symbolMatrix[first+last*6];
            if(!strictSymbolMatrix)value=Math.Min(value,8192);
            else if(value>=Impossible){request.Cost.Value=Impossible;return LimitedStatus(request);}
            request.Cost.Value=unchecked(request.Cost.Value+value);return LimitedStatus(request);
        }
        return lines[current.DescriptorOrdinal]==lines[previous.DescriptorOrdinal]?(byte)0:(byte)3;
    }
    private ReadOnlyMemory<ushort> ReadSuccessors(LegacyNativeWordRecombinationRequest request,CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(request);token.ThrowIfCancellationRequested();int selector=request.ReadInt32(32);
        if(selector<0||selector>=Graph.State.MaximumAtoms)throw new ArgumentOutOfRangeException(nameof(request));
        int node=request.ReadInt32(8);LegacyMainCompactOrdinaryArc arc;
        if(node==-1)arc=Graph.State.Root;
        else{var row=Graph.State.GetNode(node);int ordinal=request.ReadUInt16(16);if(ordinal>=row.Count)throw new ArgumentOutOfRangeException(nameof(request));arc=row[ordinal];}
        return arc.CopySelector(selector+1);
    }
    private void Enter(CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        if(Interlocked.CompareExchange(ref active,1,0)!=0)throw new InvalidOperationException("Native global transition calls must be serialized.");
    }
    private static byte LimitedStatus(LegacyNativeWordRecombinationRequest request)=>(byte)(request.Cost.Value<request.ReadInt32(52)?3:4);
    private static void Write32(LegacyNativeWordRecombinationRequest request,int offset,int value)
    {Span<byte> bytes=stackalloc byte[4];BinaryPrimitives.WriteInt32LittleEndian(bytes,value);for(int i=0;i<4;i++)request.WriteByte(offset+i,bytes[i]);}
}
