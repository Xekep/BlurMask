using System.Buffers.Binary;
using System.Text;
using System.Runtime.CompilerServices;
[assembly:InternalsVisibleTo("LegacyEnglishGraphRawWord.ActualEntry")]
[assembly:InternalsVisibleTo("LegacyEnglishMainFlush.Probe")]
namespace InkRecognition;

internal sealed partial class LegacyEnglishCaseNode0Completion
{
    internal sealed record GraphFlushWordData(int Current,int Filtered,bool Protocol,int Rows,byte[] Packet64,
        int[] SeedOrigins,byte[] Word276,int SymbolSpan,bool Succeeded,int? PublishedIndex);
    internal sealed record GraphPlainWordData(byte[] Word275,ulong[] FlatReferences,int First,int Last,int Index);
    internal sealed record GraphRawWordData(GraphFlushWordData[] Flushes,GraphPlainWordData[] Published275,
        LegacyNativeGlyphRecordNode[] Owners,byte[] Rows72,int[] Visited,int MetadataEvents,string Scope);
    private GraphRawWordData? graphRawWord;
    private enum GraphRawWordOutputKind { Snapshot, PrivatePendingTextExecution }
    // A private value stamp, not a fake empty RAW snapshot or source capability.
    private readonly record struct GraphRawWordExecutionStamp(GraphRawWordOutputKind Output,int CompletedNode);
    private GraphRawWordOutputKind graphRawWordOutput;
    private GraphRawWordExecutionStamp? graphRawWordExecutionStamp;
    // Explicit same-owner MAIN word entry. Packet/positive-span275 publication
    // is connected;13D360 ranking/140B90/TOP273/UTF8 admission are still absent.
    internal GraphRawWordData RunGraphFirstRawWord(CancellationToken token=default,Action<GraphMainConfidenceObservation>? observer=null)
    {
        primary.RequireGraphLoop(this,token);graphConfidenceObserver=observer;
        try{_=RunGraphFirstLoopCore(true,token,true);}
        finally{graphConfidenceObserver=null;}
        if(Volatile.Read(ref phase)!=2||graphRawWord is not{} value)throw new InvalidOperationException("No completed same-owner WORD result.");
        return value with{Rows72=value.Rows72.ToArray(),Visited=value.Visited.ToArray(),
            Flushes=value.Flushes.Select(f=>f with{Packet64=f.Packet64.ToArray(),SeedOrigins=f.SeedOrigins.ToArray(),Word276=f.Word276.ToArray()}).ToArray(),
            Published275=value.Published275.Select(w=>w with{Word275=w.Word275.ToArray(),FlatReferences=w.FlatReferences.ToArray()}).ToArray(),
            Owners=value.Owners.Select(n=>n with{Records=n.Records.ToArray()}).ToArray()};
    }
    private GraphRawWordData? BuildGraphRawWord(GraphLanguageScope stage,LegacyPreparedMainCompactPublication.Result published,
        LegacyPreparedMainTerminalWinner winner,CancellationToken token)
    {
        RequireGraphPublication(stage,token);
        if(graphConfidenceScorer is null||graphRawWord is not null||graphRawWordExecutionStamp is not null||!winner.HasWinner||published.Count is<1 or>4096)
            throw new NotSupportedException("One actual selected bounded compact WORD owner required.");
        var graph=stage.Lease.Pass.Initializer.PlanForAudit.BindingForAudit.Graph;
        var geometry=graphLoopGeometry??throw new InvalidOperationException("Same live source geometry required.");
        bool preciseMemory=graphPeriodicActive&&graphOriginalSelectedRequested&&graphTextPublicationRequested;
        long reserve=preciseMemory?LegacyEnglishPeriodicPublicationBudget.RawInitialPayload(published.Count):
            checked(131072L+published.Count*4096L+(graphPeriodicActive?0L:graph.GroupCount*32768L));
        stage.Ledger.Allocate(LegacyEnglishCaseNode0Ledger.Site.FinalImages,reserve);
        stage.Ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.FinalImages,checked(reserve*2));
        if(graphPeriodicActive)stage.Ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.FinalImages,16384); // Fixed raw setup only; connected selected work is charged below.
        var operations=geometry.CreateGraphProtocolOperations(stage,token);
        byte[] endpoint=new byte[64];BinaryPrimitives.WriteInt32LittleEndian(endpoint.AsSpan(8),-1);
        BinaryPrimitives.WriteInt32LittleEndian(endpoint.AsSpan(24),graph.NodeCount);
        _=operations.ReadDelimiter(endpoint,0,token);
        byte[] header=stage.Lease.Root.Source.Alphabet.CopySerializedHeader();
        ushort unit=BinaryPrimitives.ReadUInt16LittleEndian(header.AsSpan(6));
        int selected=checked(published.Starts[stage.CurrentNode]+BinaryPrimitives.ReadUInt16LittleEndian(stage.Ring.CurrentRows.Slice(winner.Ordinal*88+34,2)));
        var trace=graphPeriodicActive
            ?RunGraphPeriodicBacktrackRows(stage,published,graph,stage.Lease.Root.Source.Alphabet,unit,published.Count,selected,ushort.MaxValue,new byte[72],token)
            :LegacyPreparedMainGlyphBacktrackRows.Run(graph,stage.Lease.Root.Source.Alphabet,unit,published.Storage,
                published.Starts,published.Count,selected,ushort.MaxValue,new byte[72],token);
        if(trace.Residuals.Length!=0)throw new NotSupportedException("Reached association requires actual residual source producer.");
        int[] cumulative;
        if(graphPeriodicActive)cumulative=stage.Ring.CopyPeriodicCumulative(stage,token);
        else
        {
            // Existing default<50 zero theorem remains on its original branch.
            if(graph.NodeCount>49)throw new NotSupportedException("Native cumulative normalization requires its connected stage.");
            cumulative=new int[graph.NodeCount];
        }
        if(graphPeriodicActive)stage.Ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.FinalImages,128); // BEFORE detached trace length metadata.
        if(graphPeriodicActive)stage.Ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.FinalImages,
            LegacyEnglishPeriodicRawWork.FlushVisits(trace.Visited.Length,trace.Rows.Length/72));
        var plan=LegacyPreparedMainGlyphFlushPlan.Run(graph,stage.Lease.Root.Source.Alphabet,published.Storage,trace.Rows,
            published.Starts,trace.Visited,cumulative,published.Count,selected,ushort.MaxValue,winner,ReadTerminal,token);
        if(graphPeriodicActive)stage.Ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.FinalImages,128); // BEFORE plan length metadata.
        if(graphPeriodicActive)stage.Ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.FinalImages,checked(1024L+plan.Flushes.Length*64L+trace.Rows.Length*2L));
        if(plan.Flushes.Length is<1 or>4096)throw new InvalidDataException("Bounded actual flush chronology required.");
        // Narrow first connected domain: one ordinary word followed by its
        //separate empty leading FFFA flush. No merged chain or interior fake.
        if(plan.Flushes.Length!=2||plan.Flushes[0].Protocol||!plan.Flushes[1].Protocol||
            plan.Flushes.Any(f=>f.Additions.Length!=0))throw new NotSupportedException("One ordinary word plus separate leading protocol flush required.");
        int ordinaryRows=0,protocolRows=0;
        foreach(var flush in plan.Flushes)
        for(int at=0;at<flush.Rows.Length;at+=72)
        {
            var row=flush.Rows.AsSpan(at,72);token.ThrowIfCancellationRequested();
            if(!flush.Protocol&&row[60]==0){if(graphPeriodicActive)ordinaryRows++;continue;}
            if(graphPeriodicActive)protocolRows++;
            if(!flush.Protocol||flush.Rows.Length!=72||row[60]!=3||BinaryPrimitives.ReadUInt16LittleEndian(row[58..])!=0xfffa||
                BinaryPrimitives.ReadInt32LittleEndian(row[24..])!=-1)
                throw new NotSupportedException("Only actual leading FFFA with previousNode8==-1 is connected.");
        }
        if(graphPeriodicActive)
        {
            var chain=LegacyEnglishPeriodicRawWork.Chain(ordinaryRows,protocolRows);
            // Separate complete chain/callback envelope, no credit from the coarse retained memory reservation.
            stage.Ledger.Allocate(LegacyEnglishCaseNode0Ledger.Site.FinalImages,chain.Payload);
            stage.Ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.FinalImages,chain.Copy);
            stage.Ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.FinalImages,chain.Visit);
        }
        var ordinary=geometry.CreateGraphOrdinaryOperations(stage,graphConfidenceScorer,token);
        var pool=LegacyNativeGlyphRecordPool.CreateWithoutGeometryCapture(false,token);
        var word=LegacyNativeWordBuilder.CreateEmpty(pool,new byte[120],token);var initialWord=word.CopyStorage(token);
        //Fresh00C860 C4=0 and unformatted selected sourceB9=0. The same
        //source135/297 owner supplies ordinary full-source graph endpoints.
        if(stage.Lease.Root.Source.Fields.Single(f=>f.Owner=="247"&&f.Destination=="B9").CopyBytes() is not [0])
            throw new NotSupportedException("Actual sourceB9=0 unformatted plain publisher required.");
        var publisher=new LegacyNativePlainWordPublisher(pool,false,false,false);
        var outputs=new GraphFlushWordData[plan.Flushes.Length];
        for(int i=0;i<plan.Flushes.Length;i++)
        {
            RequireGraphPublication(stage,token);var flush=plan.Flushes[i];byte[] bytes=flush.Template.ToArray();
            for(int j=0;j<64;j++)if(flush.Origins[j]!=-1)
            {if((uint)flush.Origins[j]>=120)throw new InvalidDataException("Actual seed origin escapes120B owner.");bytes[j]=initialWord[flush.Origins[j]];}
            var packet=new LegacyPreparedMainWordPublicationPacket(bytes);word.ApplyMainPublicationPacket(packet,token);
            bool Execute()=>LegacyMainOrdinaryGlyphChainKernel.Apply(word,flush.Rows,[],graph.NodeCount,published.Count,
                published.Starts,published.Storage,n=>graph.GetNode(n).Count,
                (row,scratch,conversion,compact,ct)=>LegacyMainOrdinaryGlyphProductionKernel.Produce(ordinary,row,scratch,conversion,compact,ct),
                (row,compact,scratch,ac,b0,residue,ct)=>LegacyMainProtocolGlyphProductionKernel.Produce(operations,row,compact,scratch,ac,b0,residue,ct).Finalization,
                packet.PreviousSymbol,0,0,false,new byte[40],new ushort[256],new byte[64],0,token).Succeeded;
            var outcome=LegacyMainPlainWordCallbackKernel.Execute(word,Execute,
                ()=>publisher.Publish(word,First,Last,packet.FirstBoundaryOverride,packet.LastBoundaryOverride,token),token);
            if(!outcome.Succeeded)throw new InvalidDataException("Actual per-flush chain/publication refused.");
            if(i==0&&outcome.Published is null||i==1&&(outcome.Published is not null||outcome.Span!=0))
                throw new InvalidDataException("Ordinary positive word and separate empty leading-flush accounting differ.");
            outputs[i]=new(flush.Current,flush.Filtered,flush.Protocol,flush.Rows.Length/72,bytes,flush.Origins.ToArray(),
                outcome.CompletedWord,outcome.Span,outcome.Succeeded,outcome.Published?.WordIndex);
        }
        if(graphTopRequested)PrepareGraphTop(stage,publisher,pool,winner,token);
        if(graphWordSourceRequested)CompleteGraphWordSource(stage,published,pool,publisher,ordinary,operations,token);
        if(graphTopRequested)graphTopOwner!.Complete(token);
        if(graphGlyphChoicesRequested)CompleteGraphGlyphChoices(stage,published,publisher,pool,ordinary,token);
        if(graphGlyphNormalization is not null)CompleteGraphGlyphUtf8(stage,pool,token);
        if(graphPeriodicActive)
        {
            RequireGraphPublication(stage,token);if(graphWordSourceRequested)graphWordSourceSession!.RequireCompletedForTop(publisher,pool,token);
            stage.Ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.FinalImages,128); // BEFORE private pool scalar counts.
            var counts=pool.ReadRawExportCounts();var export=LegacyEnglishPeriodicRawWork.PoolExport(counts.Owners,counts.Records);
            // Additional exact-export envelope; never credited from source-stage copies or refunded.
            // Keep the exact numeric pool refusal counts for BOTH outputs.
            // PoolExport above validates owners<=4096/records<=65536 and layout;
            // only its detached snapshot reservation is absent in execution.
            if(graphRawWordOutput==GraphRawWordOutputKind.Snapshot)
            {
                stage.Ledger.Allocate(LegacyEnglishCaseNode0Ledger.Site.FinalImages,export.Payload);
                stage.Ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.FinalImages,export.Copy);
                stage.Ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.FinalImages,export.Visit);
            }
        }
        List<GraphPlainWordData>? words=graphRawWordOutput==GraphRawWordOutputKind.Snapshot?new():null;
        int observedWords=0;
        for(var node=publisher.Head;node is not null;node=node.Next)
        {token.ThrowIfCancellationRequested();
            if(graphPeriodicActive)
            {
                stage.Ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.FinalImages,256); // BEFORE WORD/reference-count observation.
                int references=node.Word.FlatReferences.Count;
                if(references is<0 or>4096)throw new NotSupportedException("Bounded actual RAW WORD export required.");
                if(graphRawWordOutput==GraphRawWordOutputKind.Snapshot)
                {
                    stage.Ledger.Allocate(LegacyEnglishCaseNode0Ledger.Site.FinalImages,checked(512L+16L*references));
                    stage.Ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.FinalImages,checked(1024L+32L*references));
                    stage.Ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.FinalImages,checked(2048L+64L*references));
                }
            }
            if(observedWords==4096)throw new InvalidDataException("Bounded published words required.");
            observedWords++;
            if(words is not null)words.Add(new(node.Word.CopyStorage(),node.Word.FlatReferences.ToArray(),node.Span.First,node.Span.Last,node.WordIndex));}
        RequireGraphPublication(stage,token);
        if(graphRawWordOutput==GraphRawWordOutputKind.PrivatePendingTextExecution)
        {
            // All live source/TOP/CHAR/WORD/TEXT/index work above still ran.
            // This stamp is private and only the distinct execution entry reads it.
            //The existing full finalizer above and final RequireGraphPublication
            //remain; no source/publisher mutation or callback occurs between.
            token.ThrowIfCancellationRequested();stage.Ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.OwnerMetadata,32);
            graphRawWordExecutionStamp=new GraphRawWordExecutionStamp(graphRawWordOutput,stage.CurrentNode);
            return null;
        }
        return new(outputs,words!.ToArray(),pool.CopyOwnerNodes(token).ToArray(),trace.Rows,trace.Visited,plan.Events.Length,
            graphWordSourceRequested?"MAIN_PER_FLUSH_THEN_SOURCE188_AUGMENT_RANK275_DATA_NO_TOP273_ACCEPTANCE":"MAIN21FFD0_PER_FLUSH_21FE30_PACKET_AND_275_PREFIX_NO_RANK273_UTF8_ACCEPTANCE");
        int First(int n,ushort a)=>unchecked((int)graph.GetNode(n)[a].StartContextId);
        int Last(int n,ushort a)=>unchecked((int)graph.GetNode(n)[a].EndContextId);
        ReadOnlyMemory<byte> ReadTerminal(ulong tag,ushort ending,CancellationToken ct)
        {
            RequireGraphPublication(stage,ct);bool found=false;
            for(int j=0;j<published.Count;j++)
            {
                if(graphPeriodicActive)stage.Ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.TerminalMetadata,40);
                var row=published.Storage.AsSpan(j*40,40);
                if(BinaryPrimitives.ReadUInt64LittleEndian(row)==tag&&BinaryPrimitives.ReadUInt16LittleEndian(row[28..])==ending&&
                    BinaryPrimitives.ReadInt32LittleEndian(row[36..])!=-1&&row[4]==1){found=true;break;}
            }
            if(!found||(uint)tag>int.MaxValue||((tag>>42)&1)!=0)throw new NotSupportedException("Same-source unsuppressed actual terminal metadata required.");
            int lexical=index.ReadTerminalWordId((int)(uint)tag);int category=fresh.ReadGraphCurrentCaseCategory(stage,tag,ending);
            var result=new byte[16];BinaryPrimitives.WriteInt32LittleEndian(result,1);BinaryPrimitives.WriteInt32LittleEndian(result.AsSpan(4),lexical);
            BinaryPrimitives.WriteInt32LittleEndian(result.AsSpan(8),category);RequireGraphPublication(stage,ct);return result;
        }
    }
}
internal sealed partial class LegacyEnglishCaseNode1Geometry
{
    internal LegacyMainOrdinaryGlyphProductionOperations CreateGraphOrdinaryOperations(
        LegacyEnglishCaseNode0Completion.GraphLanguageScope stage,LegacyEnglishFreshCaseMainScorer scorer,CancellationToken token)
    {
        RequireGraphProtocolOwner(stage,token);stage.Owner.RequireGraphMainConfidence(scorer,token);
        if(stage.PeriodicEnabled)stage.Lease.Pass.Initializer.PlanForAudit.RequireGraphPreparedPlainSymbolService(stage,token);
        else stage.Lease.Pass.Initializer.PlanForAudit.RequireGraphPlainSymbolService(stage,token);
        if(!Alphabet.TryGetLabel("'",out ushort apostrophe)||!Alphabet.TryGetLabel("\"",out ushort quote)||
            !Alphabet.TryGetLabel("\ufffd",out ushort replacement)||!Alphabet.TryGetLabel("\U000f0001",out ushort closed))
            throw new InvalidDataException("Actual source scalar labels required.");
        byte[] h=Alphabet.CopySerializedHeader();ushort H(int n)=>BinaryPrimitives.ReadUInt16LittleEndian(h.AsSpan(n*2));
        var caches=new Dictionary<int,LegacyNativeGlyphScoreCache>();
        return new(Alphabet.LabelCount,H(3),apostrophe,quote,ReadGroup,Resolve,ReadCache,
            (n,a,s,end)=>{RequireGraphProtocolOwner(stage,token);return LegacyMainGlyphScoringKernel.CreateRequest(categories,closed,n,a,s,end);});
        int ReadGroup(int node,ushort arc,CancellationToken ct)
        {RequireGraphProtocolOwner(stage,ct);source.RequireOrdinaryGroup(node,arc);return source.Graph.GetNode(node)[arc].GroupId;}
        LegacyRussianMainReplacementResolution Resolve(ushort symbol,ulong conversion,int node,ushort arc,CancellationToken ct)
        {
            _=ReadGroup(node,arc,ct);
            if(unchecked((byte)conversion)!=0)
            {if(symbol==Alphabet.LabelCount+1)return new(apostrophe,apostrophe,0);if(symbol==Alphabet.LabelCount+2)return new(quote,quote,0);}
            //216860 reaches frontendV1A8 only for replacement24F4. Its
            //live return premise is not closed here: reject that real branch
            //instead of silently substitutingFFFF/original. Ordinary labels
            //remain on their proven identity/no-wide/no-Arabic path.
            GraphWordSourceNumerics.RequireSupportedReplacement(symbol,replacement);return new(symbol,symbol,0);
        }
        LegacyNativeGlyphScoreCache ReadCache(int group,CancellationToken ct)
        {
            RequireGraphProtocolOwner(stage,ct);stage.Owner.RequireGraphMainConfidence(scorer,ct);
            if(caches.TryGetValue(group,out var cached))return cached;
            if(stage.PeriodicEnabled){stage.Ledger.Allocate(LegacyEnglishCaseNode0Ledger.Site.FinalImages,32768);stage.Ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.FinalImages,65536);stage.Ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.FinalImages,32768);}
            var value=scorer.CopyGraphMainConfidence(stage.Owner,group,ct);var candidates=value.Scores.CopyCandidates();
            if(candidates.Length!=value.Values.Length)throw new InvalidDataException("Actual aligned vector10 required.");
            var rows=new LegacyMainGlyphConfidenceCandidate[candidates.Length];
            for(int i=0;i<rows.Length;i++)rows[i]=new(candidates[i].Symbol,candidates[i].Cost,value.Values[i]);
            ushort Label(string text)=>Alphabet.TryGetLabel(text,out ushort x)?x:ushort.MaxValue;
            cached=LegacyMainGlyphScoringKernel.CreateCache(rows,source.Separation[group],categories.Prototypes.Length,H(0),
                value.Scores.FallbackCost,value.Scores.PowerScale,[closed,Label("\U000f0004"),Label("\U000f0008"),Label("\U000f0002"),Label("\U000f0003"),H(4),H(5),H(6)],ct);
            caches.Add(group,cached);return cached;
        }
    }
}
internal sealed partial class LegacyEnglishCaseInitializationPlan
{
    internal void RequireGraphPreparedPlainSymbolService(LegacyEnglishCaseNode0Completion.GraphLanguageScope stage,CancellationToken token)
    {
        stage.Owner.RequireGraphPublication(stage,token);RequirePublished(token);
        if(!stage.PeriodicEnabled||!ReferenceEquals(BoundRoot,stage.Lease.Root)||!ReferenceEquals(knowledge,stage.Lease.Root.Knowledge))
            throw new InvalidOperationException("Same periodic source case/publication service required.");
        var alphabet=knowledge.Source.Alphabet;
            // BEFORE actual immutable preparation/pool/epoch identity reads. Script5 remains proven by that preparation.
            stage.Ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.FinalImages,512);
            if(binding is not{} issued||!ReferenceEquals(issued.Normal.Expansion.Preparation.Alphabet,alphabet)||executionPool is null)
                throw new InvalidOperationException("Same prepared non-Arabic source alphabet required.");
            if(!executionPool.RequireOuterWide(this,stage,issued.Normal.Expansion.Preparation,alphabet,publication,token))
                throw new NotSupportedException("Wide source scalar conversion remains separate.");
    }
    internal void RequireGraphPlainSymbolService(LegacyEnglishCaseNode0Completion.GraphLanguageScope stage,CancellationToken token)
    {
        stage.Owner.RequireGraphPublication(stage,token);RequirePublished(token);
        if(!ReferenceEquals(BoundRoot,stage.Lease.Root)||!ReferenceEquals(knowledge,stage.Lease.Root.Knowledge))
            throw new InvalidOperationException("Same source case/publication service required.");
        var alphabet=knowledge.Source.Alphabet;
        if(stage.PeriodicEnabled)stage.Ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.FinalImages,checked(4096L+alphabet.LabelCount*128L));
        if(LegacyWideSymbolPairs.CreateUnrestricted(alphabet,token).Length!=0)throw new NotSupportedException("Wide source scalar conversion remains separate.");
        if(stage.PeriodicEnabled)
        {
            // The SAME published plan already completed PrepareSourcePrefix's exhaustive script5 refusal.
            // Charge BEFORE reading its private immutable binding/expansion/preparation identity.
            stage.Ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.FinalImages,512);
            if(binding is not{} issued||!ReferenceEquals(issued.Normal.Expansion.Preparation.Alphabet,alphabet))
                throw new InvalidOperationException("Same prepared non-Arabic source alphabet required.");
            return;
        }
        for(ushort i=0;i<alphabet.LabelCount;i++)
        {token.ThrowIfCancellationRequested();
            if(stage.PeriodicEnabled)stage.Ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.FinalImages,256);
            if(alphabet.TryGetText(i,out string text))
            {
                if(stage.PeriodicEnabled)stage.Ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.FinalImages,checked(128L+text.Length*512L));
                foreach(var rune in text.EnumerateRunes())
                    if(publication.ReadScript(rune.Value)==5)throw new NotSupportedException("Actual source Arabic shaping remains separate.");
            }
        }
    }
}
