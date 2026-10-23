using System.Buffers.Binary;
namespace InkRecognition;
internal sealed partial class LegacyEnglishCaseNode0Completion
{
    private readonly record struct GraphFirstTerminal(ulong CurrentTag,int Word,int Category,ushort Ending,LegacyEnglishCaseInitializationNumericTables.Node0FirstNumeric Numeric);
    // Called only from future SAME helper's current-terminal callback, with
    // the private active scope. No caller/public raw32 is promoted to a token.
    private GraphFirstTerminal ReadGraphFirstTerminal(GraphLanguageScope scope,int ordinal,ReadOnlySpan<byte> injected,CancellationToken token)
    {
        scope.Require(token);if((uint)ordinal>=125||injected.Length!=32)throw new InvalidOperationException("Actual current primary terminal callback required.");
        var row=scope.Ring.CurrentRows.Slice(ordinal*88,88);var anchor=LegacyEnglishCaseLanguageAnchor.Resolve(scope.Ring,scope.CurrentNode,ordinal,token);
        ulong currentTag=BinaryPrimitives.ReadUInt64LittleEndian(row);ushort ending=BinaryPrimitives.ReadUInt16LittleEndian(row[36..]);
        if(row[4]!=1||(row[5]&2)==0||(row[7]&1)!=0||(row[6]&1)!=0||(row[39]&1)!=0||BinaryPrimitives.ReadUInt16LittleEndian(row[24..])!=scope.CurrentNode||
            BinaryPrimitives.ReadUInt64LittleEndian(injected)!=currentTag||BinaryPrimitives.ReadUInt64LittleEndian(injected[8..])!=0||
            BinaryPrimitives.ReadInt32LittleEndian(injected[16..])!=0||BinaryPrimitives.ReadUInt16LittleEndian(injected[20..])!=ending||injected[22]!=0||injected[23]!=0||
            BinaryPrimitives.ReadUInt64LittleEndian(injected[24..])!=0||((currentTag>>42)&1)!=0)
            throw new NotSupportedException("Actual ordinary unsuppressed current terminal injection required.");
        if(!anchor.IsFirst)throw new NotSupportedException("Actual later203690 history branch remains to be implemented; FIRST is not substituted.");
        uint terminalNode=(uint)currentTag;if(terminalNode>int.MaxValue)throw new NotSupportedException("Bounded actual source terminal node required.");
        int word=index.ReadTerminalWordId((int)terminalNode);int category=fresh.ReadGraphCurrentCaseCategory(scope,currentTag,ending);
        var numeric=numericHandles.ReadGraphFirst(scope,word,category,token);scope.Require(token);
        return new(currentTag,word,category,ending,numeric);
    }
}
