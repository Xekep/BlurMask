using System.Buffers.Binary;
using System.Runtime.CompilerServices;

[assembly:InternalsVisibleTo("LegacyMainSpanGlyphRefresh.Probe")]

namespace InkRecognition;

/// <summary>Actual negative-span source export. The source key retains context
/// endpoints and modebit56; no acoustic group or glyph alternative is invented.</summary>
public sealed class LegacyOwnedMainSpanGlyphSourceResult
{
    private readonly LegacyFirstResultSourceRange[] references;
    private readonly byte[] storage;
    public LegacyOwnedMainSpanGlyphSource Owner {get;}
    public ulong SourceKey {get;}
    public int LastContext {get;}
    public int FirstContext {get;}
    public byte Mode {get;}
    public int ReferenceCount=>references.Length;
    public IReadOnlyList<LegacyFirstResultSourceRange> References {get;}
    internal LegacyOwnedMainSpanGlyphSourceResult(LegacyOwnedMainSpanGlyphSource owner,ulong key,
        int last,int first,byte mode,LegacyFirstResultSourceRange[] references,byte[] storage)
    {Owner=owner;SourceKey=key;LastContext=last;FirstContext=first;Mode=mode;
        this.references=references;this.storage=storage;References=Array.AsReadOnly(references);}
    /// <summary>Native217670 span allocation:24*N bytes only. The caller passes
    /// no residual output pointer and does not append a two-byte residual tail.</summary>
    public byte[] CopySourceStorage24()=>storage.ToArray();
}

/// <summary>Ordinary reverse1A9AE0 branch bound to an actual freshly constructed
/// one-descriptor raw main result. Graph/source constructor and dispatch prove
/// no pending protocol contexts here; multi-descriptor/reused sources are not
/// accepted by this factory. It does not reverse forward range membership.</summary>
public sealed class LegacyOwnedMainSpanGlyphSource
{
    private readonly LegacyOrdinaryGraphSourceRanges source;
    private readonly LegacySourcePointIdentity[][] points,ends;
    private readonly LegacyInkRange[][] ranges;
    private readonly LegacySourcePointKey[] originalKeys;
    public LegacyFreshMainSingleLinePlainRecognitionResult RawResult {get;}
    public LegacyOrdinaryCursiveCandidateGraph Graph=>source.Graph;
    private LegacyOwnedMainSpanGlyphSource(LegacyFreshMainSingleLinePlainRecognitionResult raw,
        LegacyOrdinaryGraphSourceRanges source,LegacySourcePointKey[] keys,CancellationToken token)
    {
        RawResult=raw;this.source=source;originalKeys=keys;
        var chronology=source.PreparedChronology;var original=source.OriginalSource;
        int[] counts=chronology.StrokePointCounts.ToArray();
        points=new LegacySourcePointIdentity[counts.Length][];ends=new LegacySourcePointIdentity[counts.Length][];
        for(int s=0;s<counts.Length;s++)
        {
            points[s]=new LegacySourcePointIdentity[counts[s]];ends[s]=new LegacySourcePointIdentity[counts[s]];
            for(int p=0;p<counts[s];p++)
            {token.ThrowIfCancellationRequested();points[s][p]=chronology.Resolve(s,p);ends[s][p]=original.ResolveEndingMetadata(points[s][p],token);}
        }
        ranges=Graph.Groups.Select(g=>g.Ranges.ToArray()).ToArray();
    }

    public static LegacyOwnedMainSpanGlyphSource Prepare(LegacyFreshMainSingleLinePlainRecognitionResult actualResult,
        CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(actualResult);token.ThrowIfCancellationRequested();
        var sampled=actualResult.Sampled;var ownership=actualResult.Session.Ownership;
        var controller=ownership.Controller;var span=controller.Owner;
        byte[] descriptor=sampled.Owner.CopyDescriptor().ToArray();
        if(!ReferenceEquals(span.MainLineSource,sampled)||!ReferenceEquals(span.SourceGraph,sampled.SourceRanges.Graph)||
            !ReferenceEquals(ownership.Materialization.ConfidenceOwner.Graph,sampled.SourceRanges.Graph)||
            ownership.Materialization.ConfidenceOwner.Model.SourceKind!=0x135||
            sampled.Owner.SourceDescriptorOrdinal!=0||descriptor.Length<44||
            (BinaryPrimitives.ReadUInt16LittleEndian(descriptor)&15)!=0||
            BinaryPrimitives.ReadInt32LittleEndian(descriptor.AsSpan(36))!=0||
            BinaryPrimitives.ReadInt32LittleEndian(descriptor.AsSpan(40))!=chronologyCount(sampled)-1)
            throw new ArgumentException("Same actual fresh ordinary one-descriptor main source required.");
        if(ownership.CaptureGlyphGeometry||ownership.Geometry.CollectionC1||controller.Configuration.Trace2368!=0||
            controller.Configuration.Mode22f6!=0||controller.Refresh.SourceFormat22f9!=0)
            throw new NotSupportedException("Fresh nontracing no-capture plain main source required.");
        var source=sampled.SourceRanges;var original=source.OriginalSource;
        var keys=new LegacySourcePointKey[original.Strokes.Sum(s=>s.Points.Count)];var assigned=new bool[keys.Length];
        for(int s=0;s<original.Strokes.Count;s++)for(int p=0;p<original.Strokes[s].Points.Count;p++)
        {
            token.ThrowIfCancellationRequested();var id=original.Chronology.Resolve(s,p);
            if((uint)id.KeyRecordIndex>=(uint)keys.Length||assigned[id.KeyRecordIndex])
                throw new ArgumentException("Complete actual originalF8 key-record owner required.");
            keys[id.KeyRecordIndex]=id.Key;assigned[id.KeyRecordIndex]=true;
        }
        return new(actualResult,source,keys,token);
        static int chronologyCount(LegacyFreshMainSingleLineSampled sampled)=>sampled.Chronology.StrokePointCounts.Length;
    }

    /// <summary>Export a canonical negative-span key against this actual source.
    /// A caller-prepared key remains a prepared request; existing pool ownership
    /// is enforced separately by the owning refresh operation.</summary>
    public LegacyOwnedMainSpanGlyphSourceResult Read(ulong canonicalSpanKey,CancellationToken token=default)
    {
        token.ThrowIfCancellationRequested();
        if(canonicalSpanKey!=LegacyNativeGlyphRecordOwner.NormalizeOwnerKey(canonicalSpanKey))
            throw new ArgumentException("Canonical span owner key required, without a glyph-alternative ordinal.");
        int last=(unchecked((int)canonicalSpanKey)<<8)>>8;
        int delta=unchecked((short)(canonicalSpanKey>>24));
        if(delta>=0)throw new ArgumentException("Negative signed context-span delta required.");
        int first=unchecked(last+delta);byte mode=(byte)(canonicalSpanKey>>56&1);
        if(last<0||first< -1)throw new NotSupportedException("Bounded actual ordinary reverse contexts required.");
        //Native count pass and fill pass have no residual-output pointer. The
        //immutable portable source can check the two passes agree exactly.
        var countPass=Resolve(last,first,mode,token);
        byte[] storage=new byte[checked(countPass.Length*24)];
        var fillPass=Resolve(last,first,mode,token);
        if(!countPass.AsSpan().SequenceEqual(fillPass))throw new InvalidDataException("Immutable span source changed between native count and fill.");
        for(int i=0;i<fillPass.Length;i++)
        {
            token.ThrowIfCancellationRequested();WriteKey(storage.AsSpan(i*24),fillPass[i].First);
            WriteKey(storage.AsSpan(i*24+12),fillPass[i].Last);
        }
        token.ThrowIfCancellationRequested();return new(this,canonicalSpanKey,last,first,mode,fillPass,storage);
    }

    private LegacyFirstResultSourceRange[] Resolve(int last,int first,byte mode,CancellationToken token)
    {
        var starting=Find(last,true,token);
        if(starting is null&&mode==1)
        {
            var next=Find(first,true,token)??throw new NotSupportedException("Native mode1 fallback lacks an owned singleton starting arc.");
            var key=FirstPoint(next).Key;return [new(key,key)];
        }
        //No protocol-owner index/global trailing context exists in this fresh
        //single ordinary descriptor. Native1A96D0/1A9800 are not skipped for
        //arbitrary prepared graph metadata; that domain is outside this owner.
        LegacySourcePointIdentity end;
        if(starting is not null)end=FirstPoint(starting);
        else
        {
            var preceding=Find(last,false,token)??throw new NotSupportedException("Native reverse fallback lacks an owned singleton ending arc.");
            var range=preceding[^1];
            //168ED0 is168E10 at the last physical point. Resolve through the
            //actual composed map, never substitute the unfiltered raw ordinal.
            end=ends[range.StrokeIndex][^1];
        }
        if(first==-1)return [new(end.Key,end.Key)];
        var previous=Find(first,false,token)??throw new NotSupportedException("Native reverse fallback lacks the earlier singleton ending arc.");
        var final=previous[^1];
        var previousStart=points[final.StrokeIndex][final.EndPointExclusive-1];
        var previousEnd=ends[final.StrokeIndex][final.EndPointExclusive-1];
        int nextRecord=previousStart.KeyRecordIndex+1;
        bool adjacent=nextRecord<originalKeys.Length&&originalKeys[nextRecord]==end.Key;
        if(previousStart.Key==end.Key||adjacent)return [new(previousEnd.Key,end.Key)];
        return [new(previousEnd.Key,previousEnd.Key),new(end.Key,end.Key)];
    }

    private LegacyInkRange[]? Find(int context,bool byStart,CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        if(context<0)return null;
        int node=context>>7;
        if((uint)node>=(uint)Graph.NodeCount)return null;
        foreach(var arc in Graph.GetNode(node))
        {
            token.ThrowIfCancellationRequested();
            //1A9AE0/1A9650 use native byte4 (span), not group kind or flags6.
            if(arc.Span==0&&(byStart?arc.StartContextId:arc.EndContextId)==(uint)context)
                return ranges[arc.GroupId];
        }
        return null;
    }
    private LegacySourcePointIdentity FirstPoint(LegacyInkRange[] group)
    {var r=group[0];return points[r.StrokeIndex][r.StartPoint];}
    private static void WriteKey(Span<byte> output,LegacySourcePointKey key)
    {BinaryPrimitives.WriteInt32LittleEndian(output,key.First);BinaryPrimitives.WriteInt32LittleEndian(output[4..],key.Second);BinaryPrimitives.WriteInt32LittleEndian(output[8..],key.Third);}
}
