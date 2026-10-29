using System.Buffers.Binary;
[assembly:System.Runtime.CompilerServices.InternalsVisibleTo("LegacyEnglishWordSource.Probe")]
[assembly:System.Runtime.CompilerServices.InternalsVisibleTo("LegacyEnglishWordSource.ActualEntry")]
namespace InkRecognition;

internal sealed partial class LegacyEnglishCaseNode0Completion
{
    internal sealed record GraphWordSourceData(int First,int Last,LegacyOrdinaryGraphSourceReference[] References,byte[] Storage26,
        int[] CandidateIndices,int Appended,int Duplicates,int Updated,int QuotaAfter,bool SourceMarked,bool CheckboxGuard,
        GraphPlainWordData[] Words,GraphPlainWordData[] BeforeWords,LegacyNativeGlyphRecordNode[] Owners,int[] SelectedOrdinals,long ReservedPayload,long ReservedCopies,LegacyOrdinaryGraphSourceRanges.GraphWordDiagnosticInputs? SourceInputs,int[]? GroupSeparations,string Scope);
    private bool graphWordSourceRequested;
    private bool graphWordSourceDiagnosticInputs;
    private GraphWordSourceSession? graphWordSourceSession;
    private GraphWordSourceData? graphWordSourceData;
    internal sealed record GraphWordSourceResult(GraphRawWordData Raw,GraphWordSourceData Source);
    internal GraphWordSourceResult RunGraphFirstWordSourceCompletion(CancellationToken token=default,Action<GraphMainConfidenceObservation>? observer=null,bool captureSourceInputs=false)
    {
        primary.RequireGraphLoop(this,token);graphWordSourceRequested=true;graphWordSourceDiagnosticInputs=captureSourceInputs;
        var raw=RunGraphFirstRawWord(token,observer);
        var data=graphWordSourceData??throw new InvalidOperationException("No completed same-owner word source stage.");
        return new(raw,data with{GroupSeparations=data.GroupSeparations?.ToArray(),References=data.References.ToArray(),Storage26=data.Storage26.ToArray(),CandidateIndices=data.CandidateIndices.ToArray(),
            Words=data.Words.Select(w=>w with{Word275=w.Word275.ToArray(),FlatReferences=w.FlatReferences.ToArray()}).ToArray(),
            BeforeWords=data.BeforeWords.Select(w=>w with{Word275=w.Word275.ToArray(),FlatReferences=w.FlatReferences.ToArray()}).ToArray(),
            Owners=data.Owners.Select(n=>n with{Records=n.Records.ToArray()}).ToArray(),SelectedOrdinals=data.SelectedOrdinals.ToArray()});
    }
    internal GraphWordSourceResult RunGraphFirstPeriodicWordSourceCompletion(CancellationToken token=default)
    {
        primary.RequireGraphLoop(this,token,true);ledger.Allocate(LegacyEnglishCaseNode0Ledger.Site.Export,32);ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.Export,32);graphWordSourceRequested=true;graphWordSourceDiagnosticInputs=false;
        RunGraphFirstPeriodicWordSourceExecution(token);
        if(Volatile.Read(ref phase)!=2||graphRawWord is not{} raw||graphWordSourceData is not{} data)
            throw new InvalidOperationException("Same completed periodic RAW/WORD-source DATA required.");
        // All joins completed before sourceV80. These final detached DATA owners are returned once; no diagnostic clone cohort.
        return new(raw,data);
    }
    // Detached pending-TEXT DATA only: no selected Unicode flattening, positioned adapter or admission.
    internal sealed record GraphPeriodicPendingTextResult(GraphWordSourceResult Source,GraphTopData Top,
        GraphWordPublicationData Word,GraphTextPublicationData Text,GraphPostTextCoverageData Coverage,
        GraphEarlySelectedInputData? Bootstrap,string Scope);
    internal GraphPeriodicPendingTextResult RunGraphFirstPeriodicPendingText(LegacyNativeUnicodeNormalizationData normalization,CancellationToken token=default,bool completeSourceIndices=false)
    {
        ArgumentNullException.ThrowIfNull(normalization);primary.RequireGraphLoop(this,token,true);
        if(graphOriginalSelectedRequested||graphPostTextCoverageRequested||graphTextPublicationRequested||
            graphWordPublicationRequested||graphCharactersRequested||graphGlyphGeometryRequested||graphGlyphNormalization is not null||
            graphGlyphChoicesRequested||graphTopRequested||graphTopOwner is not null||graphWordSourceRequested||
            graphSelectedSourcesRequested||graphSourceIndicesRequested)
            throw new InvalidOperationException("Fresh private periodic pending-TEXT route required.");
        // Optional old RequireOriginalInputContext is insufficient for this explicit pre-NN route.
        ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.Export,512); // BEFORE new original-input identity reads.
        primary.RequirePreNeuralOriginalInputContext(token);
        try
        {
            // One56-semantic-byte reference holder plus fixed coordination work; no array clones or cap changes.
            ledger.Allocate(LegacyEnglishCaseNode0Ledger.Site.Export,128);
            ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.Export,256);
            graphOriginalSelectedRequested=true;graphPostTextCoverageRequested=true;graphTextPublicationRequested=true;
            graphSelectedSourcesRequested=completeSourceIndices;graphSourceIndicesRequested=completeSourceIndices;
            graphWordPublicationRequested=true;graphCharactersRequested=true;graphGlyphGeometryRequested=true;
            graphGlyphNormalization=normalization;graphGlyphChoicesRequested=true;graphTopRequested=true;
            var source=RunGraphFirstPeriodicWordSourceCompletion(token);
            token.ThrowIfCancellationRequested();
            if(Volatile.Read(ref phase)!=2)throw new InvalidOperationException("Same completed periodic publication required.");
            var top=graphTopOwner??throw new InvalidOperationException("No completed periodic TOP owner.");
            return new(source,top.CopyCompleted(token),
                graphWordPublicationData??throw new InvalidOperationException("No complete all275 WORD publication."),
                graphTextPublicationData??throw new InvalidOperationException("No complete pending TEXT registration."),
                graphPostTextCoverageData??throw new InvalidOperationException("No complete same-owner post-TEXT coverage."),
                completeSourceIndices?null:graphEarlySelectedInputData??throw new InvalidOperationException("No authentic pre-NN original-selected receipt."),
                completeSourceIndices?"PERIODIC_FRESH_TAIL_LIVE_CHAR_PAYLOADS_NO_CHAR_DATA_NO_POSITIONED_ADMISSION":
                "PERIODIC_PRE_NN_ORIGINAL_SELECTED_ALL275_CHAR_WORD_PENDING_TEXT_COVERAGE_NO_POSITIONED_ADMISSION");
        }
        finally
        {
            try{graphCharacterContext?.Dispose();}
            finally{primary.ReleaseOriginalInputContext();}
        }
    }

    // Distinct detached execution result. Existing named RAW/periodic DATA APIs
    // above retain their complete RAW snapshots and exact export reservations.
    internal sealed record GraphPeriodicPendingTextExecutionResult(GraphWordSourceData Source,GraphTopData Top,
        GraphWordPublicationData Word,GraphPostTextCoverageData Coverage,string Scope);
    internal GraphPeriodicPendingTextExecutionResult RunGraphFirstPeriodicPendingTextExecution(
        LegacyNativeUnicodeNormalizationData normalization,CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(normalization);primary.RequireGraphLoop(this,token,true);
        if(graphOriginalSelectedRequested||graphPostTextCoverageRequested||graphTextPublicationRequested||
            graphWordPublicationRequested||graphCharactersRequested||graphGlyphGeometryRequested||graphGlyphNormalization is not null||
            graphGlyphChoicesRequested||graphTopRequested||graphTopOwner is not null||graphWordSourceRequested||
            graphSelectedSourcesRequested||graphSourceIndicesRequested)
            throw new InvalidOperationException("Fresh private periodic pending-TEXT route required.");
        // Optional old RequireOriginalInputContext is insufficient for this explicit pre-NN route.
        ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.Export,512); // BEFORE new original-input identity reads.
        primary.RequirePreNeuralOriginalInputContext(token);
        try
        {
            // One56-semantic-byte reference holder plus fixed coordination work; no array clones or cap changes.
            ledger.Allocate(LegacyEnglishCaseNode0Ledger.Site.Export,128);
            ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.Export,256);
            ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.Authentication,128); //new private-mode/output cohort flags before reads
            graphOriginalSelectedRequested=true;graphPostTextCoverageRequested=true;graphTextPublicationRequested=true;
            graphSelectedSourcesRequested=true;graphSourceIndicesRequested=true;
            graphWordPublicationRequested=true;graphCharactersRequested=true;graphGlyphGeometryRequested=true;
            graphGlyphNormalization=normalization;graphGlyphChoicesRequested=true;graphTopRequested=true;
            ledger.Allocate(LegacyEnglishCaseNode0Ledger.Site.Export,32);ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.Export,32);
            graphWordSourceRequested=true;graphWordSourceDiagnosticInputs=false;
            graphRawWordOutput=GraphRawWordOutputKind.PrivatePendingTextExecution;
            RunGraphFirstPeriodicWordSourceExecution(token);
            ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.Export,128); //new phase/stamp/source completion scalar comparison before reads
            if(Volatile.Read(ref phase)!=2||graphRawWord is not null||
                graphRawWordExecutionStamp is not{} completed||completed.Output!=GraphRawWordOutputKind.PrivatePendingTextExecution||
                completed.CompletedNode!=graphLoopMetrics.Nodes-1||graphWordSourceData is not{} source)
                throw new InvalidOperationException("Same completed private RAW execution stamp/source producer required.");
            token.ThrowIfCancellationRequested();
            if(Volatile.Read(ref phase)!=2)throw new InvalidOperationException("Same completed periodic publication required.");
            var top=graphTopOwner??throw new InvalidOperationException("No completed periodic TOP owner.");
            return new(source,top.CopyCompleted(token),
                graphWordPublicationData??throw new InvalidOperationException("No complete all275 WORD publication."),
                graphPostTextCoverageData??throw new InvalidOperationException("No complete same-owner post-TEXT coverage."),
                "PERIODIC_PRIVATE_PENDING_TEXT_SOURCE_INDICES_EXECUTION_NO_RAW_SNAPSHOT_OR_POSITIONED_ADMISSION");
        }
        finally
        {
            try{graphCharacterContext?.Dispose();}
            finally{primary.ReleaseOriginalInputContext();}
        }
    }

    internal void RequireGraphPeriodicEquality(GraphLanguageScope stage,LegacyNativeGlyphRecordPool pool,CancellationToken token)
    {RequireGraphPublication(stage,token);if(!graphPeriodicActive||graphWordSourceSession is null)throw new InvalidOperationException("Actual periodic WORD source session required.");graphWordSourceSession.RequireEqualityPool(pool,token);}
    private void CompleteGraphWordSource(GraphLanguageScope stage,LegacyPreparedMainCompactPublication.Result compact,
        LegacyNativeGlyphRecordPool pool,LegacyNativePlainWordPublisher publisher,
        LegacyMainOrdinaryGlyphProductionOperations ordinary,LegacyMainProtocolGlyphProductionOperations protocol,CancellationToken token)
    {
        RequireGraphPublication(stage,token);
        if(graphWordSourceSession is not null||!graphWordSourceRequested)throw new InvalidOperationException("One exact word source stage required.");
        stage.Ledger.Allocate(LegacyEnglishCaseNode0Ledger.Site.FinalImages,checked(32768L+compact.Count*56L));
        stage.Ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.FinalImages,checked(65536L+compact.Count*56L));
        var session=new GraphWordSourceSession(this,stage,compact,pool,publisher,ordinary,protocol);graphWordSourceSession=session;
        graphWordSourceData=session.Run(token);
    }
    // Same owner, graph/current source lease,275span and274pool stay private.
    // No detached WORD/compact/source snapshot can issue this session.
    private sealed class GraphWordSourceSession
    {
        private readonly LegacyEnglishCaseNode0Completion owner;
        private readonly GraphLanguageScope stage;
        private readonly LegacyPreparedMainCompactPublication.Result compact;
        private readonly byte[] usedCompact;
        private readonly LegacyNativeGlyphRecordPool pool;
        private readonly LegacyNativePlainWordPublisher publisher;
        private readonly LegacyMainOrdinaryGlyphProductionOperations ordinary;
        private readonly LegacyMainProtocolGlyphProductionOperations protocol;
        private readonly LegacyOrdinaryRecognitionGraph graph;
        private readonly LegacyInkAlphabet alphabet;
        private readonly ushort marker,joiner,closed,unit,soft;
        private int state,controllerRows;
        private bool sourceMarked;
        internal GraphWordSourceSession(LegacyEnglishCaseNode0Completion owner,GraphLanguageScope stage,
            LegacyPreparedMainCompactPublication.Result compact,LegacyNativeGlyphRecordPool pool,LegacyNativePlainWordPublisher publisher,
            LegacyMainOrdinaryGlyphProductionOperations ordinary,LegacyMainProtocolGlyphProductionOperations protocol)
        {
            this.owner=owner;this.stage=stage;this.compact=compact;this.pool=pool;this.publisher=publisher;this.ordinary=ordinary;this.protocol=protocol;
            // Periodic publication/session are synchronous private owners. All compact consumers are read-only.
            // Keep the legacy default detached owner; periodic uses the already charged live pool directly.
            usedCompact=owner.graphPeriodicActive?compact.Storage:compact.Storage.AsSpan(0,checked(compact.Count*40)).ToArray();
            graph=stage.Lease.Pass.Initializer.PlanForAudit.BindingForAudit.Graph;alphabet=stage.Lease.Root.Source.Alphabet;
            ushort Label(string text)=>alphabet.TryGetLabel(text,out ushort x)?x:ushort.MaxValue;
            marker=Label("\u0003");joiner=Label("\u200d");closed=Label("\U000f0001");soft=Label("\u00ad");
            unit=BinaryPrimitives.ReadUInt16LittleEndian(alphabet.CopySerializedHeader().AsSpan(6));
        }
        private void Require(CancellationToken token,bool entered)
        {
            owner.RequireGraphPublication(stage,token);
            if(!ReferenceEquals(owner.graphWordSourceSession,this)||state!=(entered?1:0)||
                !ReferenceEquals(stage.Lease.Pass.SpanSession.Scorer,owner.graphConfidenceScorer))
                throw new InvalidOperationException("Same active EN word-source session/cache required.");
        }
        internal GraphWordSourceData Run(CancellationToken token)
        {
            Require(token,false);var span=publisher.Head??throw new NotSupportedException("Actual positive WORD/span required.");
            publisher.RequireSoleActiveSourceHead(span);
            if(span.Next is not null||span.Span.First<0||span.Span.Last<span.Span.First)
                throw new NotSupportedException("This source stage admits one actual forward WORD/span.");
            var scope=stage.Lease.Root.Knowledge.Owner.RetainGraphWordRanges(stage,token);
            var sourceBounds=scope.Preflight(span.Span.First,span.Span.Last,token);
            var estimate=LegacyGlobalWordAugmentationEnvelope.Prepare(usedCompact.AsSpan(0,checked(compact.Count*40)),compact.Starts,
                graph.NodeCount,sourceBounds.SourcePoints,token,span.Word.FlatReferences.Count,span.Span.Last);
            long payload=checked(sourceBounds.Payload+131072L+estimate.Bounds.Rows*2048L+(estimate.Bounds.Candidates+2L)*256L);
            long copies=checked(sourceBounds.Copies+estimate.Bounds.CopyBytes+payload*2);
            stage.Ledger.Allocate(LegacyEnglishCaseNode0Ledger.Site.FinalImages,payload);
            stage.Ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.FinalImages,copies);
            if(owner.graphPeriodicActive)stage.Ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.FinalImages,
                checked(256L+sourceBounds.Depth*(long)sourceBounds.Depth*256L+compact.Count*64L+(estimate.Bounds.Candidates+2L)*(estimate.Bounds.Candidates+2L)*256L+sourceBounds.SourcePoints*8L));
            // Model-neutral structural64Mi/128Mi envelope remains additional
            // finite-work validation. Existing parent9Mi/64Mi/16Mi ceilings are
            // not widened. Periodic source DFS/walk/equality debits use the same parent ledger; see the successor readset/ledger for remaining source proof limits.
            if(Interlocked.CompareExchange(ref state,1,0)!=0)throw new InvalidOperationException("Word source stage cannot retry.");
            try
            {
                var selected=publisher.CaptureSpan(span,[span],token);
                if(selected.Words.Count!=1||selected.SelectedOrdinals is not [0])throw new NotSupportedException("Fresh singleton positive275 prefix required.");
                LegacyOrdinaryGraphSourceRanges.GraphWordDiagnosticInputs? diagnosticInputs=null;int[]? groupSeparations=null;
                if(owner.graphWordSourceDiagnosticInputs)
                {
                    // Explicit diagnostic-only retained DATA, charged BEFORE
                    // allocation; no extra NN, issuer, callback or live handle.
                    stage.Ledger.Allocate(LegacyEnglishCaseNode0Ledger.Site.FinalImages,131072);
                    stage.Ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.FinalImages,262144);
                    stage.Ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.FinalImages,65536);
                    diagnosticInputs=scope.CopyDiagnosticInputs(token);
                    groupSeparations=(owner.graphLoopGeometry??throw new InvalidOperationException("Active final graph geometry required.")).CopyGraphWordSourceSeparations(stage,token);
                }
                var beforeWords=selected.Words.Select((w,i)=>new GraphPlainWordData(w.CopyStorage(),w.FlatReferences.ToArray(),span.Span.First,span.Span.Last,i)).ToArray();
                var quote=LegacyNativeWordQuoteNormalization.Prepare(alphabet,token);
                quote.NormalizePublished(pool,selected.SelectedNodes.Select(n=>n.Word).ToArray(),new byte[4],new byte[4],null,token);
                //Fresh NEW00C860 F4=0:21F320 returns before rewrite. This same
                //private fresh owner accepts no property-copy/external state.
                LegacyOrdinaryGraphSourceReference[] counted,filled;byte[] source26;LegacyPreparedMainWordAugmentationCandidates.Result scan;
                int appended=0,duplicates=0,updated=0;
                try
                {
                    Require(token,true);scope.Require(token);
                    counted=scope.ReadMode2(span.Span.First,span.Span.Last,token);
                    filled=scope.ReadMode2(span.Span.First,span.Span.Last,token);
                    if(counted.Length>sourceBounds.Depth||!counted.AsSpan().SequenceEqual(filled))
                        throw new InvalidDataException("Actual source188 count/fill chronology changed.");
                    source26=GraphWordSourceNumerics.Encode26(filled,token);estimate.Copy(checked(filled.Length*104L));
                    LegacyGlobalSpanAlternativeWalkKernel.Delimiter Delimiter(byte[] request,int cursor,CancellationToken ct)
                    {Require(ct,true);var result=protocol.ReadDelimiter(request,cursor,ct);Require(ct,true);return new(result.RemainingCursor,result.Symbols.Length,result.NativeFlag0C,0);}
                    var walker=owner.graphPeriodicActive?null:new LegacyGlobalSpanAlternativeWalkKernel(usedCompact,compact.Count,graph.NodeCount,
                        marker,joiner,closed,unit,soft,new byte[64],First,Last,Delimiter,estimate);
                    var periodicWalker=owner.graphPeriodicActive?new LegacyEnglishPeriodicSpanAlternativeWalkKernel(stage,usedCompact,compact.Count,graph.NodeCount,
                        marker,joiner,closed,unit,soft,new byte[64],First,Last,Delimiter,estimate):null;
                    LegacyGlobalSpanAlternativeWalkKernel.Result Walk(int index,int minimum,bool emit,ushort flags,ReadOnlySpan<byte> residue,CancellationToken ct)
                        =>owner.graphPeriodicActive?periodicWalker!.Run(index,minimum,emit,flags,residue,ct):walker!.Run(index,minimum,emit,flags,residue,ct);
                    scan=LegacyPreparedMainWordAugmentationCandidates.Scan(usedCompact,compact.Count,compact.Starts,span.Span.First,span.Span.Last,false,
                        First,Last,(index,minimum)=>{Require(token,true);var dry=Walk(index,minimum,false,ushort.MaxValue,[],token);controllerRows=0;return new(dry.Status,dry.LastSourceKey);},token);
                    int originalCount=publisher.CaptureSpan(span,[span],token).Words.Count;
                    var working=LegacyNativeWordBuilder.CreateEmpty(pool,new byte[120],token);
                    foreach(int candidate in scan.CompactIndices)
                    {
                        Require(token,true);
                        if(BinaryPrimitives.ReadInt32LittleEndian(usedCompact.AsSpan(candidate*40+36))!=-1)
                            throw new NotSupportedException("Reached candidate auxiliary dictionary payload requires actual fiveDWORD issuer.");
                        working.ResetPlain(token);working.ApplyAugmentationFlag32((usedCompact[candidate*40+32]&16)!=0,token);
                        int minimum=LegacyPreparedMainWordAugmentationCandidates.DecodeContextNode(span.Span.First);
                        var emission=Walk(candidate,minimum,true,65534,new byte[72],token);controllerRows=emission.Rows72.Length/72;
                        if(emission.Status is 1 or -1)throw new InvalidDataException("Included actual direct-parent candidate failed emission.");
                        if(emission.ResidualReferences.Length!=0)throw new NotSupportedException("Reached residual association requires its genuine source overlay.");
                        for(int at=0;at<emission.Rows72.Length;at+=72)if(emission.Rows72[at+60]!=0)
                            throw new NotSupportedException("Augmentation binds ordinary source0 glyphs only.");
                        var before=publisher.CaptureSpan(span,[span],token);
                        foreach(var word in before.Words)foreach(ulong reference in word.FlatReferences)
                            if((BinaryPrimitives.ReadUInt16LittleEndian(pool.CopyRecord(reference,token).AsSpan(4))&7)!=0)
                                throw new NotSupportedException("Nonordinary equality requires its actual token reader.");
                        working.ApplySpanAlternativeFlags30(emission.WordFlags,token);
                        var chain=LegacyMainOrdinaryGlyphChainKernel.Apply(working,emission.Rows72,[],graph.NodeCount,compact.Count,compact.Starts,usedCompact,
                            n=>graph.GetNode(n).Count,(row,scratch,conversion,storage,ct)=>LegacyMainOrdinaryGlyphProductionKernel.Produce(ordinary,row,scratch,conversion,storage,ct),
                            static(_,_,_,_,_,_,_)=>throw new NotSupportedException("No protocol producer substituted for ordinary augmentation."),
                            ushort.MaxValue,uint.MaxValue,uint.MaxValue,false,new byte[40],new ushort[256],new byte[64],1,token);
                        controllerRows=0;if(!chain.Succeeded)throw new InvalidDataException("Actual augmentation chain failed.");
                        var costs=LegacyPreparedMainAugmentedWordCosts.Calculate(usedCompact,compact.Count,candidate,emission.LastDirectParentIndex,
                            LegacyPreparedMainWordAugmentationCandidates.DecodeContextNode(span.Span.First),LegacyPreparedMainWordAugmentationCandidates.DecodeContextNode(span.Span.Last),0x135,token);
                        byte[] old=working.CopyStorage(token);working.SetRawScalars(costs.Score10,BinaryPrimitives.ReadSingleLittleEndian(old.AsSpan(20)),costs.Cost18,costs.Cost1C,token);
                        if(working.GetSourceSpan(First,Last,token)!=span.Span)throw new InvalidDataException("Actual appended source span changed.");
                        before=publisher.CaptureSpan(span,[span],token);int match=-1;
                        for(int i=0;i<before.Words.Count;i++)if(Equal(before.Words[i].CopyStorage(),working.CopyStorage(token))){match=i;break;}
                        if(match<0){Require(token,true);_=publisher.InternPlainWord(working,span.Span,out _,token);appended++;}
                        else
                        {
                            duplicates++;byte[] existing=before.Words[match].CopyStorage();
                            bool classWord=BinaryPrimitives.ReadUInt16LittleEndian(existing.AsSpan(48))==emission.WordFlags&&unchecked((short)emission.WordFlags)>=0;
                            if(classWord){if(emission.WordFlags>1)throw new NotSupportedException("Actual source header ordinal required.");classWord=emission.WordFlags==1&&(stage.Lease.Root.DescriptorFlags&0x1c)!=0;}
                            if(GraphWordSourceNumerics.ShouldUpdateDuplicate(match,originalCount,classWord,costs.Cost18,BinaryPrimitives.ReadSingleLittleEndian(existing.AsSpan(24))))
                            {publisher.UpdateAugmentedDuplicate(before,match,originalCount,costs.Score10-.00001f,costs.Cost18,costs.Cost1C,emission.WordFlags,token);updated++;}
                            if(chain.FirstNewReference!=ulong.MaxValue)_=pool.RemoveCreatedChainSegment(chain.FirstNewReference,chain.LastProcessedReference,token);
                        }
                    }
                }
                finally{sourceMarked=true;} //13E7F0 marks even failed V88 callback; poison prevents reuse.
                Require(token,true);var current=publisher.CaptureSpan(span,[span],token);
                //Fresh00C860 property16/property18=FFFF, ordinary non288 mode. This private
                //session owns the one actual selected279 list; no caller quota.
                var ranked=LegacyPreparedMainWordRanking.Rank(current.Words.Select(w=>w.CopyStorage()).ToArray(),current.SelectedOrdinals.ToArray(),
                    LegacyPreparedMainWordRanking.InitialQuota(65535,65535,false),false,Equal,token);
                publisher.ApplySpanRanking(current,ranked.Words,ranked.OutputSourceOrdinals,ranked.OldToNew,token);
                RequireSelectedNonCheckbox(span,token);
                current=publisher.CaptureSpan(span,[span],token);var scores=LegacyPreparedMainSpanScorePublication.Normalize(current.Words.Select(w=>w.CopyStorage()).ToArray(),token);
                publisher.ApplySpanScores(current,scores.Words,token);Require(token,true);scope.Require(token);
                if(controllerRows!=0)throw new InvalidDataException("Actual row scratch retained after augmentation.");
                current=publisher.CaptureSpan(span,[span],token);var data=current.Words.Select((w,i)=>new GraphPlainWordData(w.CopyStorage(),w.FlatReferences.ToArray(),span.Span.First,span.Span.Last,i)).ToArray();
                Volatile.Write(ref state,2);
                return new(span.Span.First,span.Span.Last,filled,source26,scan.CompactIndices,appended,duplicates,updated,ranked.QuotaAfter,sourceMarked,true,data,beforeWords,pool.CopyOwnerNodes(token).ToArray(),current.SelectedOrdinals.ToArray(),payload,copies,diagnosticInputs,groupSeparations,
                    "SAME_OWNER_EN188_MODE2_26N_219580_13D360_NONCHECKBOX13D000_140B90_DATA_NOT_TOP273_TEXT_ACCEPTANCE");
            }
            catch{Volatile.Write(ref state,3);throw;}
            bool Equal(ReadOnlySpan<byte> a,ReadOnlySpan<byte> b)
            {
                estimate.Equality();if(owner.graphPeriodicActive)
                {stage.Ledger.Allocate(LegacyEnglishCaseNode0Ledger.Site.FinalImages,8);return LegacyEnglishPeriodicWordEquality.Compare(stage,pool,a,b,new byte[4],new byte[4],joiner,static label=>label,null,token);}
                return LegacyNativeMainWordEquality.Compare(pool,a,b,new byte[4],new byte[4],joiner,static label=>label,null,token);
            }
            int First(int n,ushort a){estimate.Request();Require(token,true);return unchecked((int)graph.GetNode(n)[a].StartContextId);}
            int Last(int n,ushort a){estimate.Request();Require(token,true);return unchecked((int)graph.GetNode(n)[a].EndContextId);}
        }
        internal void RequireEqualityPool(LegacyNativeGlyphRecordPool actual,CancellationToken token)
        {Require(token,true);if(!ReferenceEquals(pool,actual)||!owner.graphPeriodicActive)throw new InvalidOperationException("Same actual periodic source equality pool required.");}
        internal void RequireCompletedForTop(LegacyNativePlainWordPublisher expectedPublisher,LegacyNativeGlyphRecordPool expectedPool,CancellationToken token)
        {
            owner.RequireGraphPublication(stage,token);
            if(state!=2||!ReferenceEquals(owner.graphWordSourceSession,this)||
                !ReferenceEquals(publisher,expectedPublisher)||!ReferenceEquals(pool,expectedPool))
                throw new InvalidOperationException("Same successful source callback required by TOP.");
        }
        private void RequireSelectedNonCheckbox(LegacyNativePublishedWordNode head,CancellationToken token)
        {
            //Native13D000 enters rewrite only for actual selected single-ref,
            //kind0 checkbox WORD. Merely having checkbox labels in AL is not
            //a rewrite. Enabled control-alphabet path is explicitly refused.
            for(var n=head;n is not null;n=n.Next)
            {
                Require(token,true);var refs=n.Word.FlatReferences;if(refs.Count!=1||refs[0]==ulong.MaxValue)continue;
                byte[] glyph=pool.CopyRecord(refs[0],token);if((glyph[4]&7)!=0)continue;
                ushort label=BinaryPrimitives.ReadUInt16LittleEndian(glyph.AsSpan(8));
                foreach(string text in new[]{"\u2610","\u2611","\u2612"})if(alphabet.TryGetLabel(text,out ushort checkbox)&&checkbox==label)
                    throw new NotSupportedException("Reached native checkbox rewrite requires its actual control alphabet.");
            }
        }
    }
}
internal static class GraphWordSourceNumerics
{
    internal static void RequireSupportedReplacement(ushort symbol,ushort replacement)
    {if(symbol==replacement)throw new NotSupportedException("Reached native frontendV1A8 replacement requires its actual return issuer.");}
    //219580 compares new relative cost; the source-header class is OR, not a score/cost selector.
    internal static bool ShouldUpdateDuplicate(int matched,int initialCount,bool sameSourceClass,float newCost,float oldCost)
        =>matched>=initialCount&&(sameSourceClass||newCost<oldCost);
    internal static byte[] Encode26(LegacyOrdinaryGraphSourceReference[] rows,CancellationToken token)
    {
        token.ThrowIfCancellationRequested();byte[] b=new byte[checked(rows.Length*26)];int tail=rows.Length*24;
        for(int i=0;i<rows.Length;i++){token.ThrowIfCancellationRequested();Write(i*24,rows[i].First);Write(i*24+12,rows[i].Last);b[tail+i*2]=rows[i].FirstResidual;b[tail+i*2+1]=rows[i].LastResidual;}
        return b;
        void Write(int at,LegacySourcePointKey p){BinaryPrimitives.WriteInt32LittleEndian(b.AsSpan(at),p.First);BinaryPrimitives.WriteInt32LittleEndian(b.AsSpan(at+4),p.Second);BinaryPrimitives.WriteInt32LittleEndian(b.AsSpan(at+8),p.Third);}
    }
}

internal sealed partial class LegacyEnglishCaseNode1Geometry
{
    internal int[] CopyGraphWordSourceSeparations(LegacyEnglishCaseNode0Completion.GraphLanguageScope stage,CancellationToken token)
    {
        RequireGraphProtocolOwner(stage,token);
        if(!ReferenceEquals(source.Graph,stage.Lease.Pass.Initializer.PlanForAudit.BindingForAudit.Graph.Identity)||
            source.Separation.Length is<1 or>128)
            throw new NotSupportedException("Bounded SAME active final graph separation vector required.");
        var values=source.Separation.ToArray();
        RequireGraphProtocolOwner(stage,token);return values;
    }
}
