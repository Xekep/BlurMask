using System.Buffers.Binary;
namespace InkRecognition;

internal sealed partial class LegacyEnglishCaseNode0Completion
{
    private sealed class AuxiliaryState(byte[] first,byte[] second,byte[] firstMask,byte[] secondMask,byte[] controller,byte[] controllerMask)
    {
        internal readonly byte[] First=first,Second=second,FirstMask=firstMask,SecondMask=secondMask,Controller=controller,ControllerMask=controllerMask;
    }
    // Private real204290/FIRST state. It confers no source/query authority and
    //remains withheld after the actual OLD Run until a charged receipt exists.
    private AuxiliaryState RunAuxiliaryPrefixDraft(Stage stage,int primaryCount,CancellationToken token)
    {
        stage.Require();if(ring is null)throw new InvalidOperationException("No actual current ring.");
        var pass=stage.Lease.Owner;var plan=pass.Initializer.PlanForAudit;
        plan.BindingForAudit.Dictionary.RequireNode0EmptyAuxiliary(index,stage);
        var contexts=plan.BindingForAudit.Contexts;
        var first=ledger.Bytes(LegacyEnglishCaseNode0Ledger.Site.LanguageFirst,440);
        var second=ledger.Bytes(LegacyEnglishCaseNode0Ledger.Site.LanguageSecond,440);
        var firstMask=ledger.Bytes(LegacyEnglishCaseNode0Ledger.Site.LanguageFirstMask,440);
        var secondMask=ledger.Bytes(LegacyEnglishCaseNode0Ledger.Site.LanguageSecondMask,440);
        ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.LanguageFirst,4);BinaryPrimitives.WriteInt32LittleEndian(first,0x4000000);
        ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.LanguageSecond,4);BinaryPrimitives.WriteInt32LittleEndian(second,0x4000000);
        ledger.Fill(LegacyEnglishCaseNode0Ledger.Site.LanguageFirstMask,firstMask.AsSpan(0,4),255);
        ledger.Fill(LegacyEnglishCaseNode0Ledger.Site.LanguageSecondMask,secondMask.AsSpan(0,4),255);
        var controller=ledger.Bytes(LegacyEnglishCaseNode0Ledger.Site.Controller,4096);
        var controllerMask=ledger.Bytes(LegacyEnglishCaseNode0Ledger.Site.ControllerMask,4096);
        // One private page-list capture. Reserve the actual five-reference upper
        // bound before its List.ToArray, never capture once per row.
        ledger.Allocate(LegacyEnglishCaseNode0Ledger.Site.PageReferences,40);
        ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.PageReferences,40);
        var pages=pass.CopyCompletedPages();if(pages.Length is <1 or >5)throw new InvalidDataException("Whole private page prefix required.");
        int skip=0;
        foreach(var page in pages)
        {
            token.ThrowIfCancellationRequested();stage.Require();ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.PageCopies,12);
            if(page.Epoch!=stage.Lease.Epoch||page.Skip!=skip||page.Count is <0 or >128||page.Count>400-skip)
                throw new InvalidDataException("Same pass/epoch/ordered Skip prefix required.");
            int bytes=checked(page.Count*32);
            ledger.Allocate(LegacyEnglishCaseNode0Ledger.Site.PageCopies,bytes);ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.PageCopies,bytes);
            var records=page.CopyRecords();if(records.Length!=bytes)throw new InvalidDataException("Exact complete page image required.");
            ledger.CopyBytes(LegacyEnglishCaseNode0Ledger.Site.Controller,records,controller);
            ledger.Fill(LegacyEnglishCaseNode0Ledger.Site.ControllerMask,controllerMask.AsSpan(0,bytes),255);skip+=page.Count;
        }
        if(skip!=pass.ConsumedRows)throw new InvalidDataException("Current controller prefix differs from actual consumed pages.");
        stage.Require();ledger.Allocate(LegacyEnglishCaseNode0Ledger.Site.AuxiliaryHelperCopy,4096);ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.AuxiliaryHelperCopy,4096);
        var helper=new LegacyNativeAuxiliaryWordQueryPages(ReadActual,contexts,128,controller);
        int activeOrdinal=-1;LegacyNativeAuxiliaryWordQueryPage? lastTerminalPage=null;
        // One callback instance for this synchronous helper scope. The helper
        // invokes it before returning; activeOrdinal is updated per current row.
        // Selected closure/delegate metadata remains in the whole-owner ledger
        // frontier; hoisting does not assert a CLR allocation-size certificate.
        Func<LegacyNativeAuxiliaryWordQueryPage,CancellationToken,bool> consumeTerminal=ConsumeTerminal;
        for(int i=0;i<125;i++)
        {
            token.ThrowIfCancellationRequested();stage.Require();ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.AuxiliaryRow,12);
            var row=ring.CurrentRows.Slice(i*88,88);
            if(BinaryPrimitives.ReadInt32LittleEndian(row[16..])>=0x4000000)continue;
            if(i>=primaryCount||row[4]!=1||row[32]!=0||BinaryPrimitives.ReadInt32LittleEndian(row[56..])!=0)
                throw new NotSupportedException("Actual finite primary source1/node0/span0 row required.");
            bool terminal=(row[5]&2)!=0&&(row[7]&1)==0;
            bool bypass=(row[6]&1)!=0;
            ledger.Allocate(LegacyEnglishCaseNode0Ledger.Site.AuxiliaryRow,88);ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.AuxiliaryRow,88);
            if(terminal)
            {
                // Existing helper makes exactly one injected terminal and one
                // consumed page on the actual empty auxiliary backend branch.
                ledger.Allocate(LegacyEnglishCaseNode0Ledger.Site.AuxiliaryInjected,32);
                ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.AuxiliaryInjected,32+8+12+2+2+32);
                ledger.Allocate(LegacyEnglishCaseNode0Ledger.Site.AuxiliaryPage,32);ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.AuxiliaryPage,32);
                ledger.Allocate(LegacyEnglishCaseNode0Ledger.Site.AuxiliaryBuffer,4096);ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.AuxiliaryBuffer,4096);
                ledger.Allocate(LegacyEnglishCaseNode0Ledger.Site.AuxiliaryConsumedList,4*8);ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.AuxiliaryConsumedList,8);
            }
            activeOrdinal=i;
            var result=helper.Run(row,0,new(0,0,false,false,120),0,false,false,consumeTerminal,2,token);
            activeOrdinal=-1;
            if(!result.Completed||result.ConsumedPages.Count!=(terminal?1:0)||result.FinalAcceptedSkip!=0||
                result.SourceReadCount!=(bypass?0:terminal?2:1))
                throw new InvalidDataException("Literal current-injection then zero-stop auxiliary chronology required.");
        }
        int initialized=stage.Descriptor.StampNode0PhysicalOrdinals(stage,token);
        if(initialized!=stage.InitializedCountN)throw new InvalidDataException("Post204290 initialized prefix differs from sealed N.");
        // The helper's last full private snapshot equals its final buffer: the
        //actual subsequent source read returned0 and wrote no buffer bytes.
        //Snapshot is data only, not an issuer; copy once to our owned controller.
        if(lastTerminalPage is not null)
        {
            ledger.CopyBytes(LegacyEnglishCaseNode0Ledger.Site.Controller,lastTerminalPage.BufferSnapshot.Span,controller);
            ledger.Fill(LegacyEnglishCaseNode0Ledger.Site.ControllerMask,controllerMask.AsSpan(0,24),255);
        }
        ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.Phase1Working,48);
        return new(first,second,firstMask,secondMask,controller,controllerMask);

        LegacyAggregateDictionaryQueryPage ReadActual(ulong tag,LegacyPreparedDictionaryQuery query,CancellationToken ct)
        {
            stage.Require();ct.ThrowIfCancellationRequested();
            plan.BindingForAudit.Dictionary.RequireNode0EmptyAuxiliary(index,stage);
            if(query.Capacity!=128||query.ContextRoot!=contexts.ContainerIdentity)throw new InvalidOperationException("Exact privately prepared auxiliary request required.");
            ledger.Allocate(LegacyEnglishCaseNode0Ledger.Site.AuxiliaryOuter,4096);
            // Conditional source closure: two real raw Fetch scans plus the
            // higher wrapper/plan and two ASCII64 comparisons. Three direct
            // Stage.Require calls and map checks are charged at their sites.
            // This is not a runtime-pack, metadata-allocation or CPU bound.
            ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.AuxiliaryOuter,32768);
            // Raw reader explicit fields/cache-label stores: <=139+4*81 per
            // Fetch, at most two Fetch calls. Wrapper object fields remain a
            // separately unresolved allocation/copy frontier.
            ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.AuxiliaryOuter,926);
            var page=plan.ReadNode0Auxiliary(stage,tag,query,ct);
            // This successful native subset has real source calls and zero
            // source output. No delegate returns an invented zero page.
            if(page.ReturnedCount!=0||!page.Records.IsEmpty)throw new NotSupportedException("Actual empty auxiliary-map branch required.");
            return new(page.ReturnedCount,page.Records,ReadOnlyMemory<byte>.Empty,page.SlotIndex,page.AcceptedCount,null);
        }
        bool ConsumeTerminal(LegacyNativeAuxiliaryWordQueryPage page,CancellationToken ct)
        {
            stage.Require();ct.ThrowIfCancellationRequested();
            if(page.SourceReturnedCount!=0||!page.InjectedCurrentTerminal||page.Records32.Length!=32||page.BufferSnapshot.Length!=4096)
                throw new InvalidDataException("One actual current-terminal injection required.");
            var prefix=ReadOwnedCurrentTerminal(stage,primaryCount,activeOrdinal,page.Records32.Span,ct);
            if(prefix.Token.Boundary!=0||prefix.Token.Reserved!=0||prefix.Numeric.MatchedOrder is <1 or >2||
                prefix.BeginningPenalty is not (0 or 8192))throw new InvalidDataException("Invalid actual current FIRST prefix.");
            _=InsertOwnedCurrentFirst(stage,activeOrdinal,prefix,first,firstMask,ct);
            ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.AuxiliaryPage,8);lastTerminalPage=page;
            return true;
        }
    }
}

public sealed partial class LegacyPreparedDictionaryQuerySource
{
    internal void RequireNode0EmptyAuxiliary(LegacyEnglishCaseNode0LexiconIndex index,LegacyEnglishCaseNode0Completion.Stage stage)
    {
        stage.Require();index.ChargeBindingChecks();
        if(reading||!ReferenceEquals(index.Numeric,stage.Lease.Root.NumericOwner)||!ReferenceEquals(lexicon,index.Lexicon)||!map.SharesAuxiliaryMap(stage.Lease.Map)||recordScratch.Length<4096)
            throw new InvalidOperationException("Same initialized dictionary/immutable auxiliary map and existing4096 scratch required.");
        // All packed terminal labels were inspected once before root claim.
        // No mapped terminal edge exists anywhere, so real reads produce zero
        // and cannot grow context/cache payload. Recheck the exact current map.
        index.RequireEmptyAuxiliary(map.ReadSelectedMap(true,0));
    }
}

internal sealed partial class LegacyEnglishCaseInitializationPlan
{
    internal LegacyNativeDelimiterQueryPage ReadNode0Auxiliary(LegacyEnglishCaseNode0Completion.Stage stage,ulong tag,
        LegacyPreparedDictionaryQuery query,CancellationToken token)
    {
        stage.Require();RequirePublished(token);
        if(!ReferenceEquals(stage.Lease.Owner.Initializer.PlanForAudit,this)||!ReferenceEquals(stage.Lease.Root,BoundRoot)||
            executionPool is null||Volatile.Read(ref executionReset)!=1)
            throw new InvalidOperationException("Exact initialized same-live auxiliary owner required.");
        stage.Lease.Root.ValidateInitialQuery(stage.Lease);ValidateParent(token);
        return auxiliary!.Read(tag,query,token);
    }
}
