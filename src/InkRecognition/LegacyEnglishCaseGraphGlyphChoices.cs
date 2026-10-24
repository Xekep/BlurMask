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
        if(graphWordPublicationRequested)
        {
            CompleteGraphWordPublication(stage,compact,publisher,pool,ordinary,token);
            return;
        }
        CompleteGraphGlyphChoiceRows(stage,compact,publisher,pool,ordinary,head.Word.FlatReferences,token);
    }
    private void CompleteGraphGlyphChoiceRows(GraphLanguageScope stage,LegacyPreparedMainCompactPublication.Result compact,
        LegacyNativePlainWordPublisher publisher,LegacyNativeGlyphRecordPool pool,LegacyMainOrdinaryGlyphProductionOperations ordinary,
        IReadOnlyList<ulong> refs,CancellationToken token)
    {
        RequireGraphPublication(stage,token);graphWordSourceSession!.RequireCompletedForTop(publisher,pool,token);
        var head=publisher.Head??throw new InvalidDataException("Actual selected WORD required.");publisher.RequireSoleActiveSourceHead(head);
        if(graphWordPublicationContext is not null)graphWordPublicationContext.RequireReferences(refs,token);
        if(refs.Count is<1 or>128)throw new NotSupportedException("Bounded ordinary glyph-reference domain required.");
        // Reserve a small issuer envelope before the private source receipt.
        stage.Ledger.Allocate(LegacyEnglishCaseNode0Ledger.Site.FinalImages,4096);
        var graph=stage.Lease.Pass.Initializer.PlanForAudit.BindingForAudit.Graph;
        var ranges=stage.Lease.Root.Knowledge.Owner.RetainGraphWordRanges(stage,token);
        if(graphCharactersRequested)graphCharacterSourceRanges=ranges;
        //Only the authentic same-pool readiness receipt can skip217670 costs.
        //No arrays are allocated by these two preflight scans. Base covers the
        //per-choice progress/list metadata and bounded readonly wrappers.
        stage.Ledger.Allocate(LegacyEnglishCaseNode0Ledger.Site.FinalImages,checked(4096L+refs.Count*256L));
        stage.Ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.FinalImages,checked(8192L+refs.Count*512L));
        stage.Ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.FinalImages,checked(4096L+refs.Count*refs.Count*16L));
        if(graphGlyphDispatch is null)
        {
            stage.Ledger.Allocate(LegacyEnglishCaseNode0Ledger.Site.FinalImages,32768);
            stage.Ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.FinalImages,65536);
            graphGlyphDispatch=new GraphGlyphRefreshDispatch(pool,128,token);
        }
        var dispatch=graphGlyphDispatch;
        long payload=0,copies=0,work=0;int cold=0,matchedTotal=0;
        for(int i=0;i<refs.Count;i++)
        {
            token.ThrowIfCancellationRequested();ulong key=LegacyNativeGlyphRecordOwner.NormalizeOwnerKey(refs[i]);
            var address=LegacyNativeResidualSource.Decode(key);
            if((key&(1UL<<56))!=0||address.Node<0||address.Node>=graph.NodeCount||unchecked((short)address.Arc)<0)
                throw new NotSupportedException("Higher/protocol owner needs genuine source issuer.");
            var arcs=graph.GetNode(address.Node);
            if(address.Arc>=arcs.Count)throw new NotSupportedException("Actual same-source arc required.");
            bool hit=dispatch.TryRead(pool,key,token,out _);bool earlier=false;
            for(int j=0;j<i;j++){token.ThrowIfCancellationRequested();if(LegacyNativeGlyphRecordOwner.NormalizeOwnerKey(refs[j])==key){earlier=true;break;}}
            int originalRows=pool.ReadGraphRefreshOwnerCount(key,token);
            if(hit||earlier)continue; //first cold occurrence alone performs count/fill/refresh.
            var arc=arcs[address.Arc];var bounds=ranges.Preflight(checked((int)arc.StartContextId),checked((int)arc.EndContextId),token);
            int first=compact.Starts[address.Node],end=compact.Starts[address.Node+1],matched=0;
            if(first<0||end<first||end>compact.Storage.Length/40)throw new InvalidDataException("Complete private compact interval required.");
            //Predicate scan is charged BEFORE its first row access; never inferred from its result.
            stage.Ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.FinalImages,checked((long)(end-first)*128));
            for(int at=first;at<end;at++){token.ThrowIfCancellationRequested();if(GraphRefreshMatches(compact.Storage.AsSpan(checked(at*40),40),key))matched++;}
            var charge=GraphRefreshCharge(bounds.Payload,bounds.Copies,matched,originalRows);
            payload=checked(payload+charge.Payload);copies=checked(copies+charge.Copies);work=checked(work+charge.Visits);
            cold++;matchedTotal=checked(matchedTotal+matched);
        }
        graphGlyphAllocationProgress=new(refs.Count,cold,matchedTotal,stage.Ledger.Payload,payload,stage.Ledger.Copies,copies,stage.Ledger.Visits,work,false);
        stage.Ledger.Allocate(LegacyEnglishCaseNode0Ledger.Site.FinalImages,payload);
        stage.Ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.FinalImages,copies);
        stage.Ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.FinalImages,work);
        graphGlyphAllocationProgress=graphGlyphAllocationProgress with{Reserved=true};
        if(graphGlyphGeometryRequested)graphGlyphGeometryContext=GraphGlyphGeometryContext.Issue(this,stage,publisher,pool,token);
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
            graphWordPublicationContext?.BeforeReference(reference,token);
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
            int rowCount=pool.ReadGraphRefreshOwnerCount(key,token);
            stage.Ledger.Allocate(LegacyEnglishCaseNode0Ledger.Site.FinalImages,checked(2048L+rowCount*256L));
            stage.Ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.FinalImages,checked(4096L+rowCount*512L+4L*rowCount*rowCount));
            stage.Ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.FinalImages,checked(4096L+128L*rowCount*rowCount));
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
