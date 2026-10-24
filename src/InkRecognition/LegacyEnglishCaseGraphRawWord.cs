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
    private GraphRawWordData BuildGraphRawWord(GraphLanguageScope stage,LegacyPreparedMainCompactPublication.Result published,
        LegacyPreparedMainTerminalWinner winner,CancellationToken token)
    {
        RequireGraphPublication(stage,token);
        if(graphConfidenceScorer is null||graphRawWord is not null||!winner.HasWinner||published.Count is<1 or>4096)
            throw new NotSupportedException("One actual selected bounded compact WORD owner required.");
        var graph=stage.Lease.Pass.Initializer.PlanForAudit.BindingForAudit.Graph;
        var geometry=graphLoopGeometry??throw new InvalidOperationException("Same live source geometry required.");
        long reserve=checked(131072L+published.Count*4096L+graph.GroupCount*32768L);
        stage.Ledger.Allocate(LegacyEnglishCaseNode0Ledger.Site.FinalImages,reserve);
        stage.Ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.FinalImages,checked(reserve*2));
        var operations=geometry.CreateGraphProtocolOperations(stage,token);
        byte[] endpoint=new byte[64];BinaryPrimitives.WriteInt32LittleEndian(endpoint.AsSpan(8),-1);
        BinaryPrimitives.WriteInt32LittleEndian(endpoint.AsSpan(24),graph.NodeCount);
        _=operations.ReadDelimiter(endpoint,0,token);
        byte[] header=stage.Lease.Root.Source.Alphabet.CopySerializedHeader();
        ushort unit=BinaryPrimitives.ReadUInt16LittleEndian(header.AsSpan(6));
        int selected=checked(published.Starts[stage.CurrentNode]+BinaryPrimitives.ReadUInt16LittleEndian(stage.Ring.CurrentRows.Slice(winner.Ordinal*88+34,2)));
        var trace=LegacyPreparedMainGlyphBacktrackRows.Run(graph,stage.Lease.Root.Source.Alphabet,unit,published.Storage,
            published.Starts,published.Count,selected,ushort.MaxValue,new byte[72],token);
        if(trace.Residuals.Length!=0)throw new NotSupportedException("Reached association requires actual residual source producer.");
        // Concrete private ring implementation sets2520[node]=0 in its sole
        //completion writer. The bounded<50 graph never executes normalization;
        //no other method writes vector8. This is a sealed source invariant,
        //not a zero-value inference from a detached diagnostic snapshot.
        if(graph.NodeCount>49)throw new NotSupportedException("Native cumulative normalization requires its connected stage.");
        var cumulative=new int[graph.NodeCount];
        var plan=LegacyPreparedMainGlyphFlushPlan.Run(graph,stage.Lease.Root.Source.Alphabet,published.Storage,trace.Rows,
            published.Starts,trace.Visited,cumulative,published.Count,selected,ushort.MaxValue,winner,ReadTerminal,token);
        if(plan.Flushes.Length is<1 or>4096)throw new InvalidDataException("Bounded actual flush chronology required.");
        // Narrow first connected domain: one ordinary word followed by its
        //separate empty leading FFFA flush. No merged chain or interior fake.
        if(plan.Flushes.Length!=2||plan.Flushes[0].Protocol||!plan.Flushes[1].Protocol||
            plan.Flushes.Any(f=>f.Additions.Length!=0))throw new NotSupportedException("One ordinary word plus separate leading protocol flush required.");
        foreach(var flush in plan.Flushes)
        for(int at=0;at<flush.Rows.Length;at+=72)
        {
            var row=flush.Rows.AsSpan(at,72);token.ThrowIfCancellationRequested();
            if(!flush.Protocol&&row[60]==0)continue;
            if(!flush.Protocol||flush.Rows.Length!=72||row[60]!=3||BinaryPrimitives.ReadUInt16LittleEndian(row[58..])!=0xfffa||
                BinaryPrimitives.ReadInt32LittleEndian(row[24..])!=-1)
                throw new NotSupportedException("Only actual leading FFFA with previousNode8==-1 is connected.");
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
        var words=new List<GraphPlainWordData>();
        for(var node=publisher.Head;node is not null;node=node.Next)
        {token.ThrowIfCancellationRequested();if(words.Count==4096)throw new InvalidDataException("Bounded published words required.");
            words.Add(new(node.Word.CopyStorage(),node.Word.FlatReferences.ToArray(),node.Span.First,node.Span.Last,node.WordIndex));}
        RequireGraphPublication(stage,token);
        return new(outputs,words.ToArray(),pool.CopyOwnerNodes(token).ToArray(),trace.Rows,trace.Visited,plan.Events.Length,
            graphWordSourceRequested?"MAIN_PER_FLUSH_THEN_SOURCE188_AUGMENT_RANK275_DATA_NO_TOP273_ACCEPTANCE":"MAIN21FFD0_PER_FLUSH_21FE30_PACKET_AND_275_PREFIX_NO_RANK273_UTF8_ACCEPTANCE");
        int First(int n,ushort a)=>unchecked((int)graph.GetNode(n)[a].StartContextId);
        int Last(int n,ushort a)=>unchecked((int)graph.GetNode(n)[a].EndContextId);
        ReadOnlyMemory<byte> ReadTerminal(ulong tag,ushort ending,CancellationToken ct)
        {
            RequireGraphPublication(stage,ct);bool found=false;
            for(int j=0;j<published.Count;j++)
            {
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
        stage.Lease.Pass.Initializer.PlanForAudit.RequireGraphPlainSymbolService(stage,token);
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
    internal void RequireGraphPlainSymbolService(LegacyEnglishCaseNode0Completion.GraphLanguageScope stage,CancellationToken token)
    {
        stage.Owner.RequireGraphPublication(stage,token);RequirePublished(token);
        if(!ReferenceEquals(BoundRoot,stage.Lease.Root)||!ReferenceEquals(knowledge,stage.Lease.Root.Knowledge))
            throw new InvalidOperationException("Same source case/publication service required.");
        var alphabet=knowledge.Source.Alphabet;
        if(LegacyWideSymbolPairs.CreateUnrestricted(alphabet,token).Length!=0)throw new NotSupportedException("Wide source scalar conversion remains separate.");
        for(ushort i=0;i<alphabet.LabelCount;i++)
        {token.ThrowIfCancellationRequested();if(alphabet.TryGetText(i,out string text))foreach(var rune in text.EnumerateRunes())
            if(publication.ReadScript(rune.Value)==5)throw new NotSupportedException("Actual source Arabic shaping remains separate.");}
    }
}
