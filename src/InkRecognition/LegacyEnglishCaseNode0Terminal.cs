using System.Buffers.Binary;
namespace InkRecognition;

internal sealed partial class LegacyEnglishCaseNode0Completion
{
    // Private value, issued only from the real helper callback after exact
    // current ordinal/parent/query checks. It has no constructor/ingress API
    // outside this private owning type and no memory/array/view component.
    private readonly record struct CurrentTerminalToken(ulong Tag,int Source,int Word,int Category,
        byte Boundary,byte Suppression,ushort Reserved);
    private readonly record struct CurrentTerminalPrefix(CurrentTerminalToken Token,
        LegacyEnglishCaseInitializationNumericTables.Node0FirstNumeric Numeric,int BeginningPenalty);

    private CurrentTerminalPrefix ReadOwnedCurrentTerminal(Stage stage,int primaryCount,int ordinal,
        ReadOnlySpan<byte> query,CancellationToken token)
    {
        stage.Require();token.ThrowIfCancellationRequested();
        ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.TerminalMetadata,128);
        if(ring is null||(uint)ordinal>=(uint)primaryCount||(uint)primaryCount>125||query.Length!=32)
            throw new InvalidOperationException("Exact callback current primary ordinal required.");
        var parent=ring.CurrentRows.Slice(ordinal*88,88);
        ulong tag=BinaryPrimitives.ReadUInt64LittleEndian(parent);
        ushort ending=BinaryPrimitives.ReadUInt16LittleEndian(parent[36..]);
        // Same current-terminal injection, no independent source snapshot or
        // caller token. Query residue24..31 is checked, never overwritten.
        if(parent[4]!=1||(parent[5]&2)==0||(parent[7]&1)!=0||
            BinaryPrimitives.ReadInt32LittleEndian(parent[16..])>=0x4000000||
            BinaryPrimitives.ReadUInt64LittleEndian(query)!=tag||
            BinaryPrimitives.ReadUInt64LittleEndian(query[8..])!=0||
            BinaryPrimitives.ReadInt32LittleEndian(query[16..])!=0||
            BinaryPrimitives.ReadUInt16LittleEndian(query[20..])!=ending||query[22]!=0||query[23]!=0||
            BinaryPrimitives.ReadUInt64LittleEndian(query[24..])!=0||parent[32]!=0||
            BinaryPrimitives.ReadInt32LittleEndian(parent[56..])!=0||
            BinaryPrimitives.ReadUInt16LittleEndian(parent[60..])!=ordinal||
            BinaryPrimitives.ReadUInt16LittleEndian(parent[24..])!=0||
            (parent[6]&1)!=0||(parent[39]&1)!=0)
            throw new NotSupportedException("Actual nonspecial source1 terminal/self-anchor/node0/span0 and zero extra-pointer branch required.");
        fresh.RequireStage(stage);
        if(fresh.KnowledgeF5For(stage))throw new NotSupportedException("Direct fresh FIRST knowledge branch required.");
        uint node=(uint)tag;
        if(node>int.MaxValue)throw new NotSupportedException("Bounded actual terminal dictionary node required.");
        int word=index.ReadTerminalWordId((int)node);
        int category=fresh.ReadCurrentCaseCategory(stage,tag,ending);
        byte suppression=(byte)(tag>>42&1);
        if(suppression!=0)throw new NotSupportedException("Current admitted unsuppressed FIRST branch required.");
        //094040 clears16, writes owned terminal preorder word at4,
        //boundary byte12=0 and suppression13;234970 writes category8.
        //236D20 delegates selected tag-source1 and stamps first DWORD1.
        //No intermediate metadata byte-array/diagnostic copy is allocated.
        var issued=new CurrentTerminalToken(tag,1,word,category,0,suppression,0);
        var numeric=numericHandles.ReadNode0First(stage,issued.Word,issued.Category,token);
        int penalty=stage.ReadBeginningPenalty(parent[26],token);
        if(numeric.Word!=issued.Word||numeric.Category!=issued.Category||issued.Source!=parent[4])
            throw new InvalidDataException("Current terminal token/numeric lineage mismatch.");
        token.ThrowIfCancellationRequested();stage.Require();return new(issued,numeric,penalty);
    }
}
