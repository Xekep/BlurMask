using System.Buffers.Binary;

namespace InkRecognition;

public readonly record struct LegacyFirstContextGeometricAncestor(int OriginalNode,int OriginalPrimaryRow,int OriginalContextAlternative,
    LegacyFirstAcousticRecordFields OriginalRecord,int GeometryNode,int GeometryPrimaryRow,int GeometryContextAlternative,
    LegacyFirstAcousticRecordFields GeometryRecord,int SkippedCount);
public readonly record struct LegacyFirstBlankContextCorrection(int Stage,bool FinalPass,int CurrentNode,int CurrentPrimaryRow,
    LegacyFirstAncestorTransitionRequest Request,LegacyFirstContextGeometricAncestor Previous,int IncomingCost,
    LegacyFirstOrdinaryTransitionResult Result);

/// <summary>Owned fresh zero-trait first B0/B8 context walks including blank
/// and header ancestors. Actual ranked costs and sampler corrections come from
/// the same producing kernel/source graph; no unsupported rows are discarded.</summary>
public static class LegacyFirstBlankContextPasses
{
    public static LegacyFirstOrdinaryContextPasses Run(ReadOnlySpan<byte> resource,LegacyFreshFirstResultOwnershipFactory factory,
        LegacySmoothedRowCutResult cuts,LegacyFreshFirstCandidateGraphPreparation prepared,LegacyFreshFirstBlankPrimary producer,LegacyCursiveSeparationTable table,
        CancellationToken cancellationToken=default)
        =>RunCore(resource,factory,cuts,prepared,producer,table,null,null,null,cancellationToken);

    public static LegacyFirstOrdinaryContextPasses RunWithObserver(ReadOnlySpan<byte> resource,LegacyFreshFirstResultOwnershipFactory factory,
        LegacySmoothedRowCutResult cuts,LegacyFreshFirstCandidateGraphPreparation prepared,LegacyFreshFirstBlankPrimary producer,LegacyCursiveSeparationTable table,
        Action<LegacyFirstBlankContextCorrection> observer,CancellationToken cancellationToken=default)
    {ArgumentNullException.ThrowIfNull(observer);return RunCore(resource,factory,cuts,prepared,producer,table,null,null,observer,cancellationToken);}

    //Friend-probe-only prepared-row controls. Actual public entry points above
    //never accept a primary/cost override and always invoke the owning producer.
    internal static LegacyFirstOrdinaryContextPasses RunPreparedZeroTraitRows(ReadOnlySpan<byte> resource,
        LegacyFreshFirstResultOwnershipFactory factory,LegacySmoothedRowCutResult cuts,
        LegacyFreshFirstCandidateGraphPreparation prepared,LegacyFreshFirstBlankPrimary producer,LegacyCursiveSeparationTable table,
        LegacyFirstOrdinaryAcousticRecurrence primary,LegacyFirstOrdinaryGlyphCost glyphCost,
        Action<LegacyFirstBlankContextCorrection> observer,CancellationToken cancellationToken=default)
    {ArgumentNullException.ThrowIfNull(primary);ArgumentNullException.ThrowIfNull(glyphCost);ArgumentNullException.ThrowIfNull(observer);
        return RunCore(resource,factory,cuts,prepared,producer,table,primary,glyphCost,observer,cancellationToken);}

    private static LegacyFirstOrdinaryContextPasses RunCore(ReadOnlySpan<byte> resource,LegacyFreshFirstResultOwnershipFactory factory,
        LegacySmoothedRowCutResult cuts,LegacyFreshFirstCandidateGraphPreparation prepared,LegacyFreshFirstBlankPrimary producer,LegacyCursiveSeparationTable table,
        LegacyFirstOrdinaryAcousticRecurrence? preparedPrimary,LegacyFirstOrdinaryGlyphCost? preparedCost,
        Action<LegacyFirstBlankContextCorrection>? observer,CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(producer);ArgumentNullException.ThrowIfNull(prepared);token.ThrowIfCancellationRequested();
        if(!ReferenceEquals(producer.SourceRanges,prepared.SourceRanges))throw new ArgumentException("Primary producer and context source-range owner differ.");
        var geometry=new LegacyFirstAncestorTransitionGeometry(resource,factory,cuts,prepared,table,token);
        var literal=LegacyFreshFirstLiteralContextPolicy.ReadSource(resource,factory.InvocationPolicy.Binding.SourceOffset,factory,token);
        var binding=literal.Binding;var primary=preparedPrimary??producer.Run(token);var graph=producer.SourceRanges.Graph;var connections=new LegacyOrdinaryCandidateConnections(graph,token);
        if(primary.NodeCount!=graph.NodeCount||primary.BeamWidth!=factory.PrimaryBeamWidthF8)throw new ArgumentException("Prepared primary dimensions differ from owned source graph/F8.");
        int width=primary.BeamWidth,cap0=binding.ContextColumnsA2,cap8=binding.MergedColumnsA4;
        if((long)primary.NodeCount*width*(cap0+cap8)>1_048_576)throw new ArgumentException("Portable context cell budget exceeded.");
        ushort blank=checked((ushort)binding.Alphabet.LabelCount),shadda=binding.TrailingSymbolE6;
        int p=checked(binding.SourceOffset+20+4+56);int marker=BinaryPrimitives.ReadInt32LittleEndian(resource[p..]);p+=4;if(marker==-1)p+=4;p+=11;
        ushort newline=BinaryPrimitives.ReadUInt16LittleEndian(resource[(p+8)..]),space=BinaryPrimitives.ReadUInt16LittleEndian(resource[(p+10)..]),nbsp=BinaryPrimitives.ReadUInt16LittleEndian(resource[(p+12)..]);
        var rows=Enumerable.Range(0,primary.NodeCount).Select(primary.GetRowCopy).ToArray();
        for(int n=0;n<rows.Length;n++)for(int a=0;a<rows[n].Count;a++)
        {
            token.ThrowIfCancellationRequested();var r=rows[n].Get(a);var arcs=graph.GetNode(n);int parent=n-r.Span-1;
            if(r.Traits!=0||r.Symbol>binding.Alphabet.LabelCount+2)throw new NotSupportedException("Owned first zero-trait literal/blank context required.");
            if(r.ArcOrdinal>=arcs.Count||arcs[r.ArcOrdinal].Span!=r.Span||parent<-1||parent==-1&&r.PreviousAlternative!=255||
                parent>=0&&(r.PreviousAlternative>=rows[parent].Count||!arcs[r.ArcOrdinal].PredecessorArcIndices.Span.Contains((int)rows[parent].Get(r.PreviousAlternative).ArcOrdinal)))
                throw new InvalidDataException("Primary source arc/span/predecessor differs from owned context graph.");
        }
        var b0=new LegacyFirstAcousticRecordTable[rows.Length][];var b8=new LegacyFirstAcousticRecordTable[rows.Length][];var contexts=new LegacySymbolContextFactory();
        for(int n=0;n<rows.Length;n++)
        {
            token.ThrowIfCancellationRequested();b0[n]=Enumerable.Range(0,width).Select(_=>new LegacyFirstAcousticRecordTable(cap0)).ToArray();
            b8[n]=Enumerable.Range(0,width).Select(_=>new LegacyFirstAcousticRecordTable(cap8)).ToArray();
            Produce(b0,cap0,false);Produce(b8,cap8,true);
            void Produce(LegacyFirstAcousticRecordTable[][] tables,int cap,bool final)
            {
                for(int ri=0;ri<rows[n].Count;ri++)
                {
                    token.ThrowIfCancellationRequested();var r=rows[n].Get(ri);int parent=n-r.Span-1;
                    if(parent==-1)
                    {
                        var data=rows[n].CopyStorage().AsSpan(ri*40,40).ToArray();ulong id=contexts.ResolveInitial(r.Symbol,token).Identity;
                        if(id==0)throw new InvalidOperationException("Native symbolic allocation failed.");BinaryPrimitives.WriteUInt64LittleEndian(data.AsSpan(24),id);
                        var stored=tables[n][ri].CopyStorage();data.CopyTo(stored,0);tables[n][ri]=new(stored,1);continue;
                    }
                    token.ThrowIfCancellationRequested();int cost=(preparedCost??producer.ReadGroupCost)(n,r.ArcOrdinal,r.Symbol,r.CompositeCategory,token);
                    token.ThrowIfCancellationRequested();if(cost>=0x4000000)continue;
                    for(int pri=0;pri<rows[parent].Count;pri++)
                    {
                        token.ThrowIfCancellationRequested();if(r.Symbol==blank&&pri!=r.PreviousAlternative)continue;
                        for(int ai=0;ai<tables[parent][pri].Count;ai++)
                        {
                            token.ThrowIfCancellationRequested();var original=tables[parent][pri].Get(ai);
                            if(producer.NativeConsecutiveBlankExclusion&&r.Symbol==blank&&original.Symbol==blank)continue;
                            int an=parent,ar=pri,aa=ai,skipped=0;var ancestor=original;
                            while(ancestor.Symbol==blank||ancestor.Symbol==shadda||!binding.NativeModeE0&&(ancestor.Symbol==newline||ancestor.Symbol==space||ancestor.Symbol==nbsp))
                            {
                                token.ThrowIfCancellationRequested();int previous=an-ancestor.Span-1;if(previous==-1)break;
                                aa=ancestor.ContinuationType;ar=ancestor.PreviousAlternative;an=previous;ancestor=tables[an][ar].Get(aa);skipped++;
                            }
                            //1522F0/152320 use original context parent+arc.
                            if(!connections.GetSuccessorOrdinals(parent,original.ArcOrdinal,r.Span,token).Span.Contains((int)r.ArcOrdinal))continue;
                            var owner=new LegacyFirstContextGeometricAncestor(parent,pri,ai,original,an,ar,aa,ancestor,skipped);
                            if(final?(!binding.NativeModeE0||!r.PathBit):!r.PathBit)Insert(0);
                            if(r.Symbol!=blank&&r.Symbol!=shadda&&(final?(!binding.NativeModeE0||r.PathBit):r.PathBit))Insert(2);
                            void Insert(byte kind)
                            {
                                byte correctionKind=kind==0?(r.Symbol==blank?(byte)5:r.Symbol==literal.ClosedSymbolE4?(byte)6:(byte)0):kind;
                                var endpoints=new LegacyFirstGeometricAncestor(parent,pri,original,an,ar,ancestor,skipped);
                                var request=new LegacyFirstAncestorTransitionRequest(n,r.ArcOrdinal,r.Symbol,r.CompositeCategory,r.Span,correctionKind,endpoints);
                                int subtotal=checked(original.TotalCost+cost);
                                var v140=geometry.Correct140(request,subtotal,token);Observe(140,request,owner,subtotal,v140,final,ri);if(!v140.Accepted)return;
                                var v118=geometry.Correct118(request,v140.Cost,token);Observe(118,request,owner,v140.Cost,v118,final,ri);if(!v118.Accepted||v118.Cost>=0x4000000)return;
                                ulong identity;
                                if(kind==0&&(r.Symbol==blank||final&&r.Symbol==literal.ClosedSymbolE4))identity=original.Identity;
                                else if(!final&&kind==2)identity=contexts.ResolveInitial(r.Symbol,token).Identity;
                                else
                                {ulong parentIdentity=original.Identity;if(final&&kind==2&&!binding.NativeModeE0)parentIdentity=Resolve(parentIdentity,0xfffb);identity=Resolve(parentIdentity,r.Symbol);}
                                if(identity==0)throw new InvalidOperationException("Native symbolic allocation failed.");
                                tables[n][ri].Insert(new(v118.Cost,checked(original.AcousticCost+cost),(byte)pri,(byte)ai,r.ArcOrdinal,r.Span,r.Symbol,kind==2,identity,r.CompositeCategory,0),token);
                            }
                        }
                    }
                }
            }
        }
        return LegacyFirstOrdinaryContextPasses.FromOwnedTables(primary,b0,b8,width,cap0,cap8,contexts,token);
        ulong Resolve(ulong parent,ushort symbol)
        {var c=parent==0?contexts.ResolveInitial(symbol,token):contexts.ResolveChild(parent,symbol,token);if(c.Created&&parent!=0)contexts.LinkCreatedContext(c.Identity,parent);if(c.Identity==0)throw new InvalidOperationException("Native symbolic allocation failed.");return c.Identity;}
        void Observe(int stage,LegacyFirstAncestorTransitionRequest request,LegacyFirstContextGeometricAncestor previous,int incoming,
            LegacyFirstOrdinaryTransitionResult result,bool final,int row)
        {token.ThrowIfCancellationRequested();observer?.Invoke(new(stage,final,request.CurrentNode,row,request,previous,incoming,result));token.ThrowIfCancellationRequested();}
    }
}
