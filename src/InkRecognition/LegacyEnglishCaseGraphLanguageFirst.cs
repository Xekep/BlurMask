using System.Buffers.Binary;
namespace InkRecognition;
internal sealed partial class LegacyEnglishCaseNode0Completion
{
    // Real204290 FIRST/BOS connected to SAME current-node source/controller,
    // private terminal metadata and accepted descriptor. Later history refuses
    // at its actual reached terminal; it is never converted to FIRST.
    private void RunGraphLanguageFirstOnly(GraphLanguageScope scope,int primaryCount,LegacyCursiveCategoryProfile categories,CancellationToken token)
    {
        scope.Require(token);if(graphControllerValues is null||graphControllerMask is null||primaryCount is <0 or >125||scope.Descriptor.Count!=primaryCount)
            throw new InvalidOperationException("Exact completed phase0 current node and controller required.");
        var plan=scope.Lease.Pass.Initializer.PlanForAudit;var contexts=plan.BindingForAudit.Contexts;
        plan.BindingForAudit.Dictionary.RequireGraphEmptyAuxiliary(index,scope);
        // Native212D80 resets logical active count then124capacity, retaining250backing.
        scope.Descriptor.ClearActiveCount();if(!scope.Descriptor.Resize(124,0,null,cancellationToken:token)||scope.Descriptor.AllocatedCapacity!=250||scope.Descriptor.Count!=0)
            throw new InvalidDataException("Generic phase1 logical124/retained250 reset failed.");
        ledger.Allocate(LegacyEnglishCaseNode0Ledger.Site.AuxiliaryMetadata,105541);ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.AuxiliaryMetadata,128291);
        var first=ledger.Bytes(LegacyEnglishCaseNode0Ledger.Site.LanguageFirst,440);var firstMask=ledger.Bytes(LegacyEnglishCaseNode0Ledger.Site.LanguageFirstMask,440);
        var second=ledger.Bytes(LegacyEnglishCaseNode0Ledger.Site.LanguageSecond,440);var secondMask=ledger.Bytes(LegacyEnglishCaseNode0Ledger.Site.LanguageSecondMask,440);
        ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.LanguageFirst,4);ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.LanguageSecond,4);
        BinaryPrimitives.WriteInt32LittleEndian(first,0x4000000);BinaryPrimitives.WriteInt32LittleEndian(second,0x4000000);
        ledger.Fill(LegacyEnglishCaseNode0Ledger.Site.LanguageFirstMask,firstMask.AsSpan(0,4),255);ledger.Fill(LegacyEnglishCaseNode0Ledger.Site.LanguageSecondMask,secondMask.AsSpan(0,4),255);
        ledger.Allocate(LegacyEnglishCaseNode0Ledger.Site.AuxiliaryHelperCopy,4096);ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.AuxiliaryHelperCopy,4096);
        var helper=new LegacyNativeAuxiliaryWordQueryPages((tag,q,ct)=>ReadGraphAuxiliary(scope,tag,q,ct),contexts,128,graphControllerValues);
        int activeOrdinal=-1;graphLanguageFirstOnly=true;
        Func<LegacyNativeAuxiliaryWordQueryPage,CancellationToken,bool> consume=Consume;
        try
        {
            for(int ordinal=0;ordinal<125;ordinal++)
            {
                scope.Require(token);var row=scope.Ring.CurrentRows.Slice(ordinal*88,88);
                if(BinaryPrimitives.ReadInt32LittleEndian(row[16..])>=0x4000000)continue;
                if(ordinal>=primaryCount||row[4]!=1)throw new NotSupportedException("Actual finite ordinary source1 phase0 row required.");
                bool terminal=(row[5]&2)!=0&&(row[7]&1)==0;bool bypass=(row[6]&1)!=0;
                if(terminal)
                {
                    ledger.Allocate(LegacyEnglishCaseNode0Ledger.Site.AuxiliaryInjected,32);ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.AuxiliaryInjected,88);
                    ledger.Allocate(LegacyEnglishCaseNode0Ledger.Site.AuxiliaryPage,32);ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.AuxiliaryPage,32);
                    ledger.Allocate(LegacyEnglishCaseNode0Ledger.Site.AuxiliaryBuffer,4096);ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.AuxiliaryBuffer,4096);
                    ledger.Allocate(LegacyEnglishCaseNode0Ledger.Site.AuxiliaryConsumedList,32);ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.AuxiliaryConsumedList,8);
                }
                ledger.Allocate(LegacyEnglishCaseNode0Ledger.Site.AuxiliaryRow,88);ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.AuxiliaryRow,88);
                activeOrdinal=ordinal;
                var result=helper.Run(row,scope.CurrentNode,new(0,0,false,false,120),0,false,false,consume,2,token);activeOrdinal=-1;
                if(!result.Completed||result.ConsumedPages.Count!=(terminal?1:0)||result.FinalAcceptedSkip!=0||result.SourceReadCount!=(bypass?0:terminal?2:1))
                    throw new InvalidDataException("Actual empty-source/injected-current generic auxiliary chronology required.");
            }
            _=scope.Descriptor.StampGraphPhysicalOrdinals(scope,token);scope.SealCanonicalFirstOnly();scope.Require(token);
        }
        finally{activeOrdinal=-1;}
        bool Consume(LegacyNativeAuxiliaryWordQueryPage page,CancellationToken ct)
        {
            scope.Require(ct);
            if(page.SourceReturnedCount!=0||!page.InjectedCurrentTerminal||page.Records32.Length!=32||page.BufferSnapshot.Length!=4096||activeOrdinal<0)
                throw new InvalidDataException("One actual current-terminal injection required.");
            var anchor=LegacyEnglishCaseLanguageAnchor.Resolve(scope.Ring,scope.CurrentNode,activeOrdinal,ct);
            if(!anchor.IsFirst){graphLanguageFirstOnly=false;throw new NotSupportedException("Reached actual later203690 history requires its connected service; FIRST is not substituted.");}
            var terminal=ReadGraphFirstTerminal(scope,activeOrdinal,page.Records32.Span,ct);
            var anchorMask=scope.Ring.OwnedRowMaskAt(anchor.AnchorNode);if(anchorMask[anchor.AnchorOrdinal*88+26]!=255)throw new NotSupportedException("Beginning category byte must be defined.");
            var anchorRow=scope.Ring.OwnedRowsAt(anchor.AnchorNode).Slice(anchor.AnchorOrdinal*88,88);
            var composites=categories.CompositeCategories.Span;int composite=anchorRow[26]&63;
            if(categories.BaseEntryCount!=217||composites.Length!=42||(uint)composite>=(uint)composites.Length)throw new InvalidDataException("Same-source actual42composite profile required.");
            int firstId=composites[composite].FirstId;if((uint)firstId>6)throw new InvalidDataException("Actual seven beginning semantic IDs required.");
            int penalty=firstId is 2 or 4?8192:0;
            _=InsertGraphCurrentFirst(scope,activeOrdinal,terminal,first,firstMask,penalty,ct);
            // Helper copy is private actual current controller; only its written
            // terminal prefix becomes known. Tail+zero-source residue is retained.
            ledger.CopyBytes(LegacyEnglishCaseNode0Ledger.Site.Controller,page.BufferSnapshot.Span,graphControllerValues);
            ledger.Fill(LegacyEnglishCaseNode0Ledger.Site.ControllerMask,graphControllerMask.AsSpan(0,24),255);
            return true;
        }
    }
}
