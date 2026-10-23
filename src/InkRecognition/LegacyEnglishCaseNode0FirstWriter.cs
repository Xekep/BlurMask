using System.Buffers.Binary;
namespace InkRecognition;

internal sealed partial class LegacyEnglishCaseNode0Completion
{
    private readonly LegacyNativeWordSlotInitializer firstWriter;
    private Stage? firstWriterStage;
    private byte[]? firstWriterLanguage;
    private CurrentTerminalPrefix firstWriterPrefix;
    private int firstWriterOrdinal,firstWriterCost,firstWriterBusy,firstWriterMaskValidated;

    private bool InsertOwnedCurrentFirst(Stage stage,int ordinal,CurrentTerminalPrefix prefix,
        byte[] language,byte[] languageMask,CancellationToken token)
    {
        stage.RequireAccepting();token.ThrowIfCancellationRequested();
        ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.FirstKernel,256);
        if(ring is null||language.Length!=440||languageMask.Length!=440||prefix.Token.Suppression!=0||
            prefix.Token.Source!=1||prefix.Numeric.Word!=prefix.Token.Word||prefix.Numeric.Category!=prefix.Token.Category||
            prefix.Numeric.MatchedOrder is <1 or >2||firstWriterBusy!=0)
            throw new InvalidOperationException("Exact current private FIRST result and reusable language allocations required.");
        // FIRST writes no result on a nonfinite language subtotal. It does not
        // clear the previous entry/residue in that case.
        if(prefix.Numeric.Cost>=0x4000000)return false;
        // Executable premises for the uniform accepted-prefix mask theorem.
        // Bind actual private current-parent masks, not its concrete zero bytes.
        ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.FirstLanguage,1024);
        var parentMask=ring.CurrentRowMask.AsSpan(ordinal*88,88);
        for(int j=0;j<88;j++)if(parentMask[j]!=255)
            throw new NotSupportedException("Actual FIRST parent88 must be fully defined before its copy.");
        ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.FirstLanguage,25);
        var entry=language.AsSpan(40,40);
        BinaryPrimitives.WriteInt32LittleEndian(entry,prefix.Numeric.Cost);
        BinaryPrimitives.WriteInt32LittleEndian(entry[4..],prefix.Numeric.MatchedOrder);entry[8]=2;
        BinaryPrimitives.WriteInt32LittleEndian(entry[12..],prefix.Token.Category<<24);
        BinaryPrimitives.WriteInt32LittleEndian(entry[16..],prefix.Numeric.Class);
        BinaryPrimitives.WriteInt32LittleEndian(entry[24..],-1);
        BinaryPrimitives.WriteInt32LittleEndian(entry[28..],prefix.Token.Word);
        // The15 other entry bytes retain both actual residue and unknown mask.
        ledger.Fill(LegacyEnglishCaseNode0Ledger.Site.FirstLanguage,languageMask.AsSpan(40,9),255);
        ledger.Fill(LegacyEnglishCaseNode0Ledger.Site.FirstLanguage,languageMask.AsSpan(52,8),255);
        ledger.Fill(LegacyEnglishCaseNode0Ledger.Site.FirstLanguage,languageMask.AsSpan(64,8),255);
        for(int j=0;j<20;j++)
        {
            byte expected=j is >=1 and <=3 or >=12 and <=15?(byte)0:(byte)255;
            if(languageMask[48+j]!=expected)throw new InvalidDataException("Actual reusable language overwrite mask must have exactly seven unknowns.");
        }
        var parent=ring.CurrentRows.Slice(ordinal*88,88);
        int cost=checked(checked(BinaryPrimitives.ReadInt32LittleEndian(parent[16..])+prefix.BeginningPenalty)+prefix.Numeric.Cost);
        // One reusable bound instance delegate, no per-candidate closure/array.
        // Precharge all scope writes AND its failure cleanup before binding, so
        // finally cannot replace an original allocation/callback/cancel fault.
        ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.FirstAcceptedWriter,160);
        firstWriterStage=stage;firstWriterLanguage=language;firstWriterPrefix=prefix;
        firstWriterOrdinal=ordinal;firstWriterCost=cost;firstWriterMaskValidated=1;firstWriterBusy=1;
        try{return stage.Descriptor.TryAllocateNode0AfterHole(stage,cost,firstWriter,token);}
        finally
        {
            firstWriterBusy=0;firstWriterMaskValidated=0;firstWriterStage=null;firstWriterLanguage=null;
            firstWriterPrefix=default;firstWriterOrdinal=0;firstWriterCost=0;
        }
    }

    private void WriteAcceptedCurrentFirst(Span<byte> slot)
    {
        // Independently reviewed successful-source upper: pinned AMD64
        // .NET10.0.12 Release little-endian, explicit intrinsic/named-call
        // leaves. Includes five Span->ReadOnlySpan conversions, both CopyTo
        // bodies and CurrentRows chain; Stage.Require auth256 is separate.
        // Not CPU/exceptional-path/whole-owner budget certification.
        ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.FirstAcceptedWriter,1254);
        RequireRunning(primary);
        var stage=firstWriterStage;
        if(firstWriterBusy!=1||firstWriterMaskValidated!=1||stage is null||!ReferenceEquals(activeStage,stage)||ring is null||
            firstWriterLanguage is null||firstWriterLanguage.Length!=440||slot.Length!=128)
            throw new InvalidOperationException("Privately bound actual accepted FIRST destination required.");
        stage.RequireAccepting();
        var parent=ring.CurrentRows.Slice(firstWriterOrdinal*88,88);
        if(BinaryPrimitives.ReadUInt64LittleEndian(parent)!=firstWriterPrefix.Token.Tag||parent[4]!=1||parent[32]!=0||
            BinaryPrimitives.ReadInt32LittleEndian(parent[56..])!=0||BinaryPrimitives.ReadUInt16LittleEndian(parent[60..])!=firstWriterOrdinal)
            throw new InvalidDataException("Current live parent changed after token issue.");
        int previous=checked(~(int)(sbyte)parent[32]+BinaryPrimitives.ReadInt32LittleEndian(parent[56..]));
        if(previous!=-1)throw new InvalidDataException("Actual self-anchor must resolve previous node minus1.");
        // Native1F9080 after 1F75B0 hole moves/clear120/cost store. These
        //171 bytes are all actual additional destination stores/copies. The
        //20-byte language copy OVERWRITES prior defined destination bytes with
        //seven unknowns; sealed N/T provenance represents that mask separately.
        ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.FirstAcceptedWriter,171);
        WriteAcceptedFirstFields(slot,parent,firstWriterLanguage.AsSpan(48,20),firstWriterPrefix.Token.Tag,firstWriterCost,firstWriterOrdinal,previous);
        // Actual slot78..7F(hex) was never touched, including after heap moves.
    }
    private static void WriteAcceptedFirstFields(Span<byte> slot,ReadOnlySpan<byte> parent,ReadOnlySpan<byte> languageEntry20,ulong tag,int cost,int ordinal,int previous)
    {
        parent.CopyTo(slot);slot[33]&=192;
        BinaryPrimitives.WriteUInt64LittleEndian(slot,tag);slot[39]&=254;
        BinaryPrimitives.WriteInt32LittleEndian(slot[20..],BinaryPrimitives.ReadInt32LittleEndian(parent[20..]));
        BinaryPrimitives.WriteUInt16LittleEndian(slot[62..],ushort.MaxValue);
        BinaryPrimitives.WriteInt32LittleEndian(slot[16..],cost);
        BinaryPrimitives.WriteUInt16LittleEndian(slot[30..],checked((ushort)ordinal));slot[32]=255;
        languageEntry20.CopyTo(slot[64..]);
        BinaryPrimitives.WriteUInt64LittleEndian(slot[8..],0);
        BinaryPrimitives.WriteUInt64LittleEndian(slot[40..],8UL<<40);
        BinaryPrimitives.WriteUInt64LittleEndian(slot[92..],((ulong)0x4000000<<32)|uint.MaxValue);
        BinaryPrimitives.WriteUInt32LittleEndian(slot[88..],uint.MaxValue);
        BinaryPrimitives.WriteInt32LittleEndian(slot[104..],previous);
        BinaryPrimitives.WriteUInt64LittleEndian(slot[112..],0);
    }

}
