using System.Buffers.Binary;
namespace InkRecognition;

// Logical array/semantic-object budget. No RSS or transitive CPU certificate.
internal readonly record struct LegacyEnglishGraphCompactBudget(int RecordCapacity,long AllocationBytes,long CopyBytes)
{
    internal static LegacyEnglishGraphCompactBudget FromCounts(int nodes,int arcs,int records)
    {
        if(nodes is <2 or >49||arcs is <1 or >4096||records is <1 or >1048576)
            throw new NotSupportedException("Bounded completed graph/compact extents required.");
        // Initial and publication40B pools, retained data+mask, detached returned
        // data+mask: six full capacity payloads. Twelve node payload equivalents
        // cover input/reachability/publication clones, node-vector/sidecar/record
        // frames and bounded output observation copies. Group8192 includes the
        // separately metered cache-only score4096 and transient symbol/weight data.
        long payload=checked(records*40L*6+nodes*(12L*(156*88+125*80)+4096)+arcs*8192L+65536);
        long copy=checked(payload+nodes*2L*(156*88+125*80));
        return new(records,payload,copy);
    }
}

internal sealed partial class LegacyEnglishCaseNode0Completion
{
    private static LegacyEnglishGraphCompactBudget PrepareGraphCompactBudget(GraphLanguageScope stage,
        LegacyOrdinaryRecognitionGraph graph,LegacySymbolContextFactory contexts,CancellationToken token)
    {
        stage.RequireCompactPhase();token.ThrowIfCancellationRequested();
        int arcs=graph.RequireSameSource().CandidateCount;
        stage.Ledger.Allocate(LegacyEnglishCaseNode0Ledger.Site.FinalImages,64);
        var lengths=new Dictionary<ulong,int>();
        int records=checked(arcs*2);
        for(int node=0;node<graph.NodeCount;node++)
        {
            stage.RequireCompactPhase();int end=stage.Ring.SourceEndAt(node);
            var rows=stage.Ring.OwnedRowsAt(node);var masks=stage.Ring.OwnedRowMaskAt(node);
            var alternatives=stage.Ring.OwnedPrimaryAt(node);var alternativeMasks=stage.Ring.OwnedPrimaryMaskAt(node);
            if(end is <125 or >156)throw new InvalidDataException("Actual complete retained SourceEnd required.");
            for(int ordinal=0;ordinal<end;ordinal++)
            {
                token.ThrowIfCancellationRequested();var row=rows.Slice(ordinal*88,88);var mask=masks.Slice(ordinal*88,88);
                RequireKnown(mask,16,4);
                if(BinaryPrimitives.ReadInt32LittleEndian(row[16..])>=0x4000000)continue;
                RequireKnown(mask,8,8);
                // 207420 is either current-context ancestry or the one-symbol
                // knowledge replacement. Ordinary VC8 returns one flag0 coordinate:
                //206960 can remove/merge, cannot increase this frame length.
                records=checked(records+Math.Max(1,Length(BinaryPrimitives.ReadUInt64LittleEndian(row[8..]))));
                if(ordinal>=125)continue;
                for(int k=0;k<2;k++)
                {
                    int at=ordinal*80+k*40;RequireKnown(alternativeMasks,at,4);
                    if(BinaryPrimitives.ReadInt32LittleEndian(alternatives[at..])>=0x4000000)break;
                    RequireKnown(alternativeMasks,at+16,8);
                    records=checked(records+Math.Max(1,Length(BinaryPrimitives.ReadUInt64LittleEndian(alternatives[(at+16)..]))));
                }
                if(records>1048576)throw new NotSupportedException("Complete compact bound exceeds the explicit pool.");
            }
        }
        stage.RequireCompactPhase();return LegacyEnglishGraphCompactBudget.FromCounts(graph.NodeCount,arcs,records);
        int Length(ulong identity)
        {
            Span<ulong> trail=stackalloc ulong[50];int count=0,tail=0;ulong item=identity;
            while(item!=0)
            {
                token.ThrowIfCancellationRequested();
                if(lengths.TryGetValue(item,out tail))break;
                if(count==trail.Length)throw new NotSupportedException("Actual context ancestry exceeds the native fifty-symbol frame.");
                trail[count++]=item;
                stage.Ledger.Allocate(LegacyEnglishCaseNode0Ledger.Site.FinalImages,64);
                stage.Ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.FinalImages,64);
                item=contexts.ReadNode(item).ParentLink;
            }
            for(int i=count-1;i>=0;i--)
            {
                if(++tail>50)throw new NotSupportedException("Actual context ancestry exceeds the native fifty-symbol frame.");
                stage.Ledger.Allocate(LegacyEnglishCaseNode0Ledger.Site.FinalImages,192);
                stage.Ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.FinalImages,192);
                lengths.Add(trail[i],tail);
            }
            return tail;
        }
        static void RequireKnown(ReadOnlySpan<byte> mask,int start,int count)
        {for(int i=start;i<start+count;i++)if(mask[i]!=255)throw new NotSupportedException("Cannot derive a compact bound from an opaque cost/context field.");}
    }
}
