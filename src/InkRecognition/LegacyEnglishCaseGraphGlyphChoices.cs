using System.Buffers.Binary;
using System.Runtime.CompilerServices;
[assembly:InternalsVisibleTo("LegacyEnglishGlyphRefresh.Probe")]
namespace InkRecognition;

internal sealed partial class LegacyEnglishCaseNode0Completion
{
    internal sealed record GraphGlyphChoiceData(ulong Reference,byte[] Source26,uint FirstScoreBits,
        int Refreshed,byte[] Owner40,byte[] Selected4,uint? Marker);
    internal sealed record GraphGlyphChoicesResult(GraphTopResult Top,GraphGlyphChoiceData[] Glyphs);
    private bool graphGlyphChoicesRequested;
    private GraphGlyphChoiceData[]? graphGlyphChoices;
    internal GraphGlyphChoicesResult RunGraphFirstGlyphChoices(CancellationToken token=default,Action<GraphMainConfidenceObservation>? observer=null,bool captureSourceInputs=false)
    {
        primary.RequireGraphLoop(this,token);
        if(graphGlyphChoicesRequested)throw new InvalidOperationException("One owning glyph publication required.");
        graphGlyphChoicesRequested=true;
        var top=RunGraphFirstTop(token,observer,captureSourceInputs);
        return new(top,(graphGlyphChoices??throw new InvalidOperationException("No owning glyph selection."))
            .Select(x=>x with{Source26=x.Source26.ToArray(),Owner40=x.Owner40.ToArray(),Selected4=x.Selected4.ToArray()}).ToArray());
    }
    private void CompleteGraphGlyphChoices(GraphLanguageScope stage,LegacyPreparedMainCompactPublication.Result compact,
        LegacyNativePlainWordPublisher publisher,LegacyNativeGlyphRecordPool pool,
        LegacyMainOrdinaryGlyphProductionOperations ordinary,CancellationToken token)
    {
        RequireGraphPublication(stage,token);
        if(!graphGlyphChoicesRequested||graphGlyphChoices is not null||graphTopOwner is null)
            throw new InvalidOperationException("Same fresh live TOP required.");
        graphWordSourceSession!.RequireCompletedForTop(publisher,pool,token);
        var head=publisher.Head??throw new InvalidDataException("Actual selected WORD required.");
        publisher.RequireSoleActiveSourceHead(head);
        var refs=head.Word.FlatReferences;
        if(refs.Count is<1 or>128)throw new NotSupportedException("Bounded ordinary glyph-reference domain required.");
        // Reserve a small issuer envelope before the private source receipt.
        stage.Ledger.Allocate(LegacyEnglishCaseNode0Ledger.Site.FinalImages,4096);
        var graph=stage.Lease.Pass.Initializer.PlanForAudit.BindingForAudit.Graph;
        var ranges=stage.Lease.Root.Knowledge.Owner.RetainGraphWordRanges(stage,token);
        if(graphCharactersRequested)graphCharacterSourceRanges=ranges;
        long payload=65536,copies=131072,work=0;
        // Preflight immutable endpoint/count bounds before DFS/pool/selection
        // allocations. Charge every reference even when source keys duplicate.
        foreach(ulong reference in refs)
        {
            ulong key=LegacyNativeGlyphRecordOwner.NormalizeOwnerKey(reference);
            var address=LegacyNativeResidualSource.Decode(key);
            if((key&(1UL<<56))!=0||address.Node<0||address.Node>=graph.NodeCount||
                unchecked((short)address.Arc)<0||address.Arc>=graph.GetNode(address.Node).Count)
                throw new NotSupportedException("Higher/protocol owner needs genuine sourceV140 overlay issuer.");
            var arc=graph.GetNode(address.Node)[address.Arc];
            var bounds=ranges.Preflight(checked((int)arc.StartContextId),checked((int)arc.EndContextId),token);
            int originalRows=pool.ReadGraphRefreshOwnerCount(key,token);
            int repetitions=0;for(int j=0;j<refs.Count;j++)if(LegacyNativeGlyphRecordOwner.NormalizeOwnerKey(refs[j])==key)repetitions++;
            int nodeRows=checked(compact.Starts[address.Node+1]-compact.Starts[address.Node]);
            int maximumRows=(int)Math.Min(2040L,checked(originalRows+(long)repetitions*nodeRows));
            // Include every per-row scratch/header/request/cache callback allocation,
            // full owner/selection/export clones, and worst insertion CopyTo shifts.
            payload=checked(payload+bounds.Payload+1048576L+nodeRows*1024L+maximumRows*256L);
            copies=checked(copies+bounds.Copies+2097152L+nodeRows*4096L+maximumRows*320L+2L*maximumRows*(maximumRows-1));
            work=checked(work+16384L+nodeRows*128L+32L*maximumRows*maximumRows);
        }
        stage.Ledger.Allocate(LegacyEnglishCaseNode0Ledger.Site.FinalImages,payload);
        stage.Ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.FinalImages,copies);
        stage.Ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.FinalImages,work);
        if(graphGlyphGeometryRequested)graphGlyphGeometryContext=GraphGlyphGeometryContext.Issue(this,stage,publisher,pool,token);
        // Fresh same-pool readiness is private; exported snapshots never issue it.
        stage.Ledger.Allocate(LegacyEnglishCaseNode0Ledger.Site.FinalImages,32768);
        stage.Ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.FinalImages,65536);
        stage.Ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.FinalImages,4096);
        var dispatch=graphGlyphDispatch??=new GraphGlyphRefreshDispatch(pool,128,token);
        var progress=new List<(ulong Key,byte[] Source,uint Bits,int Visited)>(refs.Count);
        //024F30 first pass: complete every actual reference's refresh BEFORE
        //any selection. No detached WORD image is used as source authority.
        foreach(ulong reference in refs)
        {
            RequireGraphPublication(stage,token);
            ulong key=LegacyNativeGlyphRecordOwner.NormalizeOwnerKey(reference);
            var address=LegacyNativeResidualSource.Decode(key);
            if((key&(1UL<<56))!=0||address.Node<0||address.Node>=graph.NodeCount||
                unchecked((short)address.Arc)<0||address.Arc>=graph.GetNode(address.Node).Count)
                throw new NotSupportedException("Higher/protocol owner needs genuine sourceV140 overlay issuer.");
            var arc=graph.GetNode(address.Node)[address.Arc];
            int first=checked((int)arc.StartContextId),last=checked((int)arc.EndContextId);
            _=ranges.Preflight(first,last,token);
            graphGlyphGeometryContext?.BeginReference(reference,token);
            GraphGlyphRefreshDispatch.Receipt receipt;
            bool wasReady=dispatch.TryRead(pool,key,token,out receipt);
            if(!wasReady)
            {
            var count=ranges.ReadOrdinary140(address.Node,address.Arc,token);var fill=ranges.ReadOrdinary140(address.Node,address.Arc,token);
            if(count.Length==0||!count.AsSpan().SequenceEqual(fill))throw new InvalidDataException("Source count/fill changed.");
            byte[] source=GraphWordSourceNumerics.Encode26(fill,token);
            int group=ordinary.ReadGroup(address.Node,address.Arc,token);
            var cache=ordinary.ReadCache(group,token);
            uint bits=BitConverter.SingleToUInt32Bits(cache.ReadFirstCandidateScore(token));
            var operations=new LegacyOrdinaryGlyphRefreshOperations(
                (symbol,ct)=>{RequireGraphPublication(stage,ct);if(symbol>=ordinary.AlphabetCount)throw new NotSupportedException("Refresh macro labels need their actual conversion issuer.");return ordinary.ResolveSymbol(symbol,0,address.Node,address.Arc,ct);},
                (symbol,ct)=>{RequireGraphPublication(stage,ct);return BitConverter.SingleToUInt32Bits(cache.ReadSymbolScore(symbol,0,ct));},
                (symbol,end,ct)=>{RequireGraphPublication(stage,ct);return new(cache.ReadRequestCost(ordinary.CreateRequest(address.Node,address.Arc,symbol,end),ct));});
            int visited=LegacyNativeOrdinaryGlyphRefreshKernel.Apply(pool,key,compact.Storage,
                compact.Starts[address.Node],compact.Starts[address.Node+1],key,operations,new byte[40],token);
            RequireGraphPublication(stage,token);
            //024F30 first pass: own refresh precedes projection/WORD clipping;
            //ALL references finish these steps before the selection loop below.
                receipt=new(fill,source,bits,visited);
                dispatch.Remember(pool,key,receipt,token);
            }
            // Dispatcher13C6E0 skips frontendV90 on an already-ready owner,
            // but024F30 still projects every actual reference and later selects it.
            graphGlyphGeometryContext?.Project(reference,receipt.Fill,token);
            progress.Add((key,receipt.Source,receipt.Bits,wasReady?0:receipt.Visited));
        }
        if(graphGlyphGeometryRequested)graphGlyphGeometryData=graphGlyphGeometryContext!.Finish(refs.Count,token);
        if(graphCharactersRequested)
        {
            CompleteGraphCharacterPass(stage,pool,refs,progress,token);
            return;
        }
        var output=new GraphGlyphChoiceData[refs.Count];
        for(int i=0;i<refs.Count;i++)
        {
            RequireGraphPublication(stage,token);ulong reference=refs[i];
            ulong key=LegacyNativeGlyphRecordOwner.NormalizeOwnerKey(reference);var source=progress[i];
            if(source.Key!=key)throw new InvalidDataException("Reference chronology changed after refresh.");
            var node=pool.CopyOwnerNode(key,token);
            var selected=LegacyPreparedGlyphAlternativeSelection.Select(node,
                unchecked((uint)(int)(short)(reference>>40)),new byte[checked((node.Records.Length/40*3/2+2)*4)],0,token);
            if(!selected.Succeeded)throw new InvalidDataException("Bounded native selection allocation refused.");
            output[i]=new(reference,source.Source.ToArray(),source.Bits,source.Visited,
                selected.CopyOwnerStorage40(),selected.CopySelectionRows4(),selected.SelectedMarker);
        }
        RequireGraphPublication(stage,token);graphGlyphChoices=output;
    }
}
