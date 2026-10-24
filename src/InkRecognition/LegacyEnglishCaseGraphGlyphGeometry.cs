using System.Runtime.CompilerServices;
[assembly:InternalsVisibleTo("LegacyEnglishGlyphGeometry.Probe")]
namespace InkRecognition;

internal sealed partial class LegacyEnglishCaseNode0Completion
{
    internal sealed record GraphGlyphGeometryData(ulong Reference,byte[] OriginalDescriptor40,bool OriginalJoiner,ulong[] ProjectedPairs,ulong[] WordClippedPairs);
    internal sealed record GraphGlyphGeometrySnapshot(int[] OriginalStrokePointCounts,ulong[] OriginalRoot,
        ulong[] WordClip,GraphGlyphGeometryData[] Glyphs);
    internal sealed record GraphGlyphGeometryResult(GraphGlyphUtf8Result Glyphs,GraphGlyphGeometrySnapshot Geometry);
    private bool graphGlyphGeometryRequested;
    private GraphGlyphGeometryContext? graphGlyphGeometryContext;
    private GraphGlyphGeometrySnapshot? graphGlyphGeometryData;
    //Geometry-only owning successor. No CHAR key/selection/document/TEXT is minted.
    internal GraphGlyphGeometryResult RunGraphFirstGlyphGeometry(LegacyNativeUnicodeNormalizationData normalization,
        CancellationToken token=default,Action<GraphMainConfidenceObservation>? observer=null,bool captureSourceInputs=false)
    {
        ArgumentNullException.ThrowIfNull(normalization);
        primary.RequireGraphLoop(this,token);
        if(graphGlyphGeometryRequested)throw new InvalidOperationException("One source geometry execution required.");
        graphGlyphGeometryRequested=true;
        var glyphs=RunGraphFirstGlyphUtf8(normalization,token,observer,captureSourceInputs);
        var x=graphGlyphGeometryData??throw new InvalidOperationException("No completed owning geometry.");
        return new(glyphs,x with{OriginalStrokePointCounts=x.OriginalStrokePointCounts.ToArray(),OriginalRoot=x.OriginalRoot.ToArray(),
            WordClip=x.WordClip.ToArray(),Glyphs=x.Glyphs.Select(g=>g with{OriginalDescriptor40=g.OriginalDescriptor40.ToArray(),ProjectedPairs=g.ProjectedPairs.ToArray(),WordClippedPairs=g.WordClippedPairs.ToArray()}).ToArray()});
    }
    private sealed partial class GraphGlyphGeometryContext
    {
        private readonly LegacyEnglishCaseNode0Completion owner;
        private readonly GraphLanguageScope stage;
        private readonly LegacyNativePlainWordPublisher publisher;
        private readonly LegacyNativeGlyphRecordPool pool;
        private readonly LegacyNativePublishedWordNode head;
        private readonly GraphWordSourceData source;
        private readonly LegacyOwnedRawInkDocument document;
        private readonly LegacyNativeTextSourceInputMap map;
        private readonly LegacyNativeInkPositionRange[] original;
        private readonly LegacyOwnedRawInkRangeOperations operations;
        private readonly List<GraphGlyphGeometryData> glyphs=new(128);
        private LegacyNativeInkPositionRange[] wordClip=[];
        private bool finished;
        private byte[]? pendingOriginal;
        private ulong pendingReference;
        private bool pendingJoiner;
        private GraphGlyphGeometryContext(LegacyEnglishCaseNode0Completion owner,GraphLanguageScope stage,
            LegacyNativePlainWordPublisher publisher,LegacyNativeGlyphRecordPool pool,LegacyNativePublishedWordNode head,
            GraphWordSourceData source,LegacyOwnedRawInkDocument document)
        {
            this.owner=owner;this.stage=stage;this.publisher=publisher;this.pool=pool;this.head=head;this.source=source;this.document=document;
            map=new(document.CopySelectedInputs(),[],0);
            original=[new(LegacyNativeInkPosition.Pack(0,0,3,document.Strokes[0].Points.Count),
                LegacyNativeInkPosition.Pack((uint)document.ObjectCount-1,document.Strokes[^1].Points.Count-1,5,document.Strokes[^1].Points.Count))];
            operations=LegacyOwnedRawInkRangeOperations.Prepare(document);
        }
        internal static GraphGlyphGeometryContext Issue(LegacyEnglishCaseNode0Completion owner,GraphLanguageScope stage,
            LegacyNativePlainWordPublisher publisher,LegacyNativeGlyphRecordPool pool,CancellationToken token)
        {
            owner.RequireGraphPublication(stage,token);owner.graphWordSourceSession!.RequireCompletedForTop(publisher,pool,token);
            if(owner.graphGlyphGeometryContext is not null||!owner.graphGlyphGeometryRequested)
                throw new InvalidOperationException("One private geometry issuer required.");
            var head=publisher.Head??throw new InvalidDataException("Actual selected positive WORD required.");publisher.RequireSoleActiveSourceHead(head);
            //This is the retained PRIVATE completed producer result, never an
            //import of Run/CopyCompleted/diagnostic arrays or a caller source key.
            var source=owner.graphWordSourceData??throw new InvalidOperationException("Same source producer required.");
            if(source.References.Length is<1 or>4096||head.Span.First!=source.First||head.Span.Last!=source.Last)
                throw new NotSupportedException("Same actual completed forward WORD source interval required.");
            var parent=stage.Lease.Root.Knowledge.Owner.Owner.Owner.Parent;
            var document=parent.CreateGraphRawDocument(stage,token);
            Reserve(stage,source.References.Length,document.ObjectCount,0,token);
            var result=new GraphGlyphGeometryContext(owner,stage,publisher,pool,head,source,document);
            owner.graphGlyphGeometryContext=result;
            result.wordClip=ProjectCore(result.document,result.map,result.original,source.References,token);
            result.Require(token);return result;
        }
        private void Require(CancellationToken token)
        {
            owner.RequireGraphPublication(stage,token);owner.graphWordSourceSession!.RequireCompletedForTop(publisher,pool,token);
            if(!ReferenceEquals(owner.graphGlyphGeometryContext,this)||!ReferenceEquals(owner.graphWordSourceData,source)||finished)
                throw new InvalidOperationException("Same unfinished live geometry/WORD source cohort required.");
            publisher.RequireSoleActiveSourceHead(head);
        }
        internal void BeginReference(ulong reference,CancellationToken token)
        {
            Require(token);if(pendingOriginal is not null)throw new InvalidOperationException("Previous source geometry not completed.");
            var original=pool.CopyRecord(reference,token);
            if(original.Length!=40||(original[4]&7)!=0)throw new NotSupportedException("This geometry slice admits original scalar kind0 only; native kind1 skip/control paths remain separate.");
            ushort symbol=System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(original.AsSpan(8));
            var alphabet=stage.Lease.Root.Source.Alphabet;
            pendingJoiner=alphabet.TryGetLabel("\u200d",out ushort joiner)&&symbol==joiner;
            pendingReference=reference;pendingOriginal=original;Require(token);
        }
        internal void Project(ulong reference,ReadOnlySpan<LegacyOrdinaryGraphSourceReference> fill,CancellationToken token)
        {
            Require(token);
            if(pendingOriginal is null||pendingReference!=reference)throw new InvalidOperationException("Exact original descriptor must precede its own refresh.");
            if(glyphs.Count>=128)throw new NotSupportedException("Bounded all-reference geometry required.");
            Reserve(stage,fill.Length,document.ObjectCount,wordClip.Length,token);
            var projected=ProjectCore(document,map,original,fill,token);
            var clipped=operations.Intersect(projected,wordClip,token);
            Require(token);
            if(owner.graphCharactersRequested)RetainCharacterRanges(reference,pendingOriginal,pendingJoiner,projected,clipped,token);
            glyphs.Add(new(reference,pendingOriginal,pendingJoiner,Pairs(projected),Pairs(clipped)));pendingOriginal=null;
        }
        internal GraphGlyphGeometrySnapshot Finish(int references,CancellationToken token)
        {
            Require(token);if(pendingOriginal is not null||glyphs.Count!=references)throw new InvalidDataException("Every actual reference must project before selection.");
            var value=new GraphGlyphGeometrySnapshot(document.Strokes.Select(s=>s.Points.Count).ToArray(),Pairs(original),Pairs(wordClip),glyphs.ToArray());
            Require(token);finished=true;return value;
        }
    }
    private static void Reserve(GraphLanguageScope stage,int count,int objects,int filters,CancellationToken token)
    {
        stage.Owner.RequireGraphPublication(stage,token);
        if(count is<1 or>4096||objects is<1 or>256||filters is<0 or>4096)
            throw new NotSupportedException("Bounded same raw source geometry required.");
        long projected=checked((long)count*objects),clipped=checked(projected*filters);
        if(projected>4096||clipped>4096)throw new NotSupportedException("Geometry projection/intersection admission cap reached before allocation.");
        stage.Ledger.Allocate(LegacyEnglishCaseNode0Ledger.Site.FinalImages,checked(65536L+count*256L+projected*256L+clipped*192L));
        stage.Ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.FinalImages,checked(131072L+count*512L+projected*512L+clipped*256L));
        //Finite named work reservation, not a new whole-transitive CPU proof.
        stage.Ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.FinalImages,checked(4096L+count*1024L+(projected+clipped)*512L));
    }
    //Model-neutral numeric projection only; this helper cannot issue a live
    //context or accept caller DATA as WORD/graph authority.
    internal static LegacyNativeInkPositionRange[] ProjectCore(LegacyOwnedRawInkDocument document,
        LegacyNativeTextSourceInputMap map,ReadOnlySpan<LegacyNativeInkPositionRange> original,
        ReadOnlySpan<LegacyOrdinaryGraphSourceReference> actual,CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        if(actual.Length is<1 or>4096||original.Length!=1||map.InputCount!=document.ObjectCount)
            throw new NotSupportedException("Bounded ordinary projection inputs required.");
        var refs=new LegacyNativeTextSourceReference[actual.Length];
        for(int i=0;i<refs.Length;i++)
        {
            token.ThrowIfCancellationRequested();var r=actual[i];
            if(r.First.First!=0||r.Last.First!=0)throw new NotSupportedException("Actual ordinary raw ingress group0 only.");
            refs[i]=new(new(r.First.First,r.First.Second,r.First.Third),new(r.Last.First,r.Last.Second,r.Last.Third),r.FirstResidual,r.LastResidual);
        }
        return LegacyNativeTextSourcePositionPublication.Project(map,refs,true,original,token);
    }
    private static ulong[] Pairs(ReadOnlySpan<LegacyNativeInkPositionRange> ranges)
    {
        var values=new ulong[checked(ranges.Length*2)];for(int i=0;i<ranges.Length;i++){values[i*2]=ranges[i].First.Value;values[i*2+1]=ranges[i].Last.Value;}return values;
    }
}

public sealed partial class LegacyEnglish119ParentSubmission
{
    internal LegacyOwnedRawInkDocument CreateGraphRawDocument(LegacyEnglishCaseNode0Completion.GraphLanguageScope stage,CancellationToken token)
    {
        stage.Owner.RequireGraphPublication(stage,token);
        if(!ReferenceEquals(stage.Lease.Root.Knowledge.Owner.Owner.Owner.Parent,this))
            throw new InvalidOperationException("Exact owning English original parent required.");
        int points=0;
        lock(gate)
        {
            token.ThrowIfCancellationRequested();if(!completed||faulted||raw.Length is<1 or>256)
                throw new InvalidOperationException("Same successful immutable original raw parent required.");
            foreach(var stroke in raw)
            {
                token.ThrowIfCancellationRequested();if(stroke.Points.Count is<1 or>4096||stroke.Points.Count>4096-points)
                    throw new NotSupportedException("Bounded complete original raw points required before document clone.");
                points+=stroke.Points.Count;
            }
        }
        //Includes raw readonly arrays, source-object/select rows, input-map
        //construction+copy, root metadata, fixed context/list and final exports.
        stage.Ledger.Allocate(LegacyEnglishCaseNode0Ledger.Site.FinalImages,checked(65536L+points*64L+raw.Length*1024L));
        stage.Ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.FinalImages,checked(131072L+points*128L+raw.Length*2048L));
        stage.Ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.FinalImages,checked(8192L+points*64L+raw.Length*256L));
        var document=LegacyOwnedRawInkDocument.Create(raw,token);
        stage.Owner.RequireGraphPublication(stage,token);return document;
    }
}
