using System.Buffers.Binary;
namespace InkRecognition;
internal sealed partial class LegacyEnglishCaseNode0Completion
{
    private LegacyNativeWordSlotInitializer? graphFirstWriter;
    private GraphLanguageScope? graphFirstScope;
    private byte[]? graphFirstLanguage;
    private GraphFirstTerminal graphFirstTerminal;
    private int graphFirstOrdinal,graphFirstCost,graphFirstBusy,graphFirstMaskValidated;
    private bool graphLanguageFirstOnly;
    private bool InsertGraphCurrentFirst(GraphLanguageScope scope,int ordinal,GraphFirstTerminal prefix,byte[] language,byte[] languageMask,int beginningPenalty,CancellationToken token)
    {
        scope.RequireAccepting();token.ThrowIfCancellationRequested();
        if(language.Length!=440||languageMask.Length!=440||prefix.Numeric.Word!=prefix.Word||prefix.Numeric.Category!=prefix.Category||prefix.Numeric.MatchedOrder is <1 or >2||graphFirstBusy!=0||(uint)ordinal>=125)
            throw new InvalidOperationException("Canonical same-owner FIRST result/ordinal/language buffer required.");
        if(prefix.Numeric.Cost>=0x4000000)return false;
        var mask=scope.Ring.CurrentRowMask.AsSpan(ordinal*88,88);for(int i=0;i<88;i++)if(mask[i]!=255)throw new NotSupportedException("Canonical FIRST copied parent88 must be fully defined.");
        ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.FirstLanguage,25);var entry=language.AsSpan(40,40);
        BinaryPrimitives.WriteInt32LittleEndian(entry,prefix.Numeric.Cost);BinaryPrimitives.WriteInt32LittleEndian(entry[4..],prefix.Numeric.MatchedOrder);entry[8]=2;
        BinaryPrimitives.WriteInt32LittleEndian(entry[12..],prefix.Category<<24);BinaryPrimitives.WriteInt32LittleEndian(entry[16..],prefix.Numeric.Class);
        BinaryPrimitives.WriteInt32LittleEndian(entry[24..],-1);BinaryPrimitives.WriteInt32LittleEndian(entry[28..],prefix.Word);
        ledger.Fill(LegacyEnglishCaseNode0Ledger.Site.FirstLanguage,languageMask.AsSpan(40,9),255);ledger.Fill(LegacyEnglishCaseNode0Ledger.Site.FirstLanguage,languageMask.AsSpan(52,8),255);ledger.Fill(LegacyEnglishCaseNode0Ledger.Site.FirstLanguage,languageMask.AsSpan(64,8),255);
        for(int i=0;i<20;i++){byte expected=i is >=1 and <=3 or >=12 and <=15?(byte)0:(byte)255;if(languageMask[48+i]!=expected)throw new InvalidDataException("Canonical seven-unknown FIRST language-copy domain required.");}
        var parent=scope.Ring.CurrentRows.Slice(ordinal*88,88);int cost=checked(checked(BinaryPrimitives.ReadInt32LittleEndian(parent[16..])+beginningPenalty)+prefix.Numeric.Cost);
        if(graphFirstWriter is null){ledger.Allocate(LegacyEnglishCaseNode0Ledger.Site.FirstAcceptedWriter,48);ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.FirstAcceptedWriter,48);graphFirstWriter=WriteAcceptedGraphFirst;}
        ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.FirstAcceptedWriter,160);graphFirstScope=scope;graphFirstLanguage=language;graphFirstTerminal=prefix;graphFirstOrdinal=ordinal;graphFirstCost=cost;graphFirstMaskValidated=1;graphFirstBusy=1;
        try{return scope.Descriptor.TryAllocateGraphAfterHole(scope,cost,graphFirstWriter,token);}
        finally{graphFirstBusy=0;graphFirstMaskValidated=0;graphFirstScope=null;graphFirstLanguage=null;graphFirstTerminal=default;graphFirstOrdinal=0;graphFirstCost=0;}
    }
    private void WriteAcceptedGraphFirst(Span<byte> slot)
    {
        ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.FirstAcceptedWriter,1254);var scope=graphFirstScope;
        if(scope is null||graphFirstBusy!=1||graphFirstMaskValidated!=1||graphFirstLanguage is null||slot.Length!=128)throw new InvalidOperationException("Bound canonical generic accepted FIRST writer required.");
        scope.RequireAccepting();scope.RequireOwnedFirstWriter(graphFirstWriter!);
        var parent=scope.Ring.CurrentRows.Slice(graphFirstOrdinal*88,88);var anchor=LegacyEnglishCaseLanguageAnchor.Resolve(scope.Ring,scope.CurrentNode,graphFirstOrdinal,default);
        if(!anchor.IsFirst||BinaryPrimitives.ReadUInt64LittleEndian(parent)!=graphFirstTerminal.CurrentTag||parent[4]!=1)throw new InvalidDataException("Current parent/anchor changed after generic FIRST token issue.");
        ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.FirstAcceptedWriter,171);
        WriteAcceptedFirstFields(slot,parent,graphFirstLanguage.AsSpan(48,20),graphFirstTerminal.CurrentTag,graphFirstCost,graphFirstOrdinal,anchor.PreviousWordNode);
    }
}
