using System.Buffers.Binary;
namespace InkRecognition;
// Numeric-only internal join. Inputs are owned by node1 caller; prepared tests
// may exercise this without claiming dictionary/source authority.
internal static class LegacyEnglishCaseNode1Materialization
{
    internal sealed record Images(byte[] Heap128,byte[] Rows88,byte[] Alternatives80,int Count,int InitialCount,LegacyNativeWordHeapDescriptor? LiveDescriptor=null,int[]? LivePrimaryOrdinals=null);
    internal static Images Build(LegacyNativeWordRecordHeap heap,LegacyNativeWordHeapPreparation preparation,
        LegacyCursiveCategoryProfile categories,LegacySymbolContextFactory contexts,LegacyNativeStoredWordRecord[] parents,
        LegacyInkAlphabet alphabet,CancellationToken token,bool includeWholeStart=false,bool retainPrimaryOrdinals=false)
    {
        token.ThrowIfCancellationRequested();var c=preparation.Configuration;
        if(heap.Capacity!=250||heap.Count>250||parents.Length!=125||c.PrimaryArcLimit230a!=125||c.PrimaryAlternativeCount2402!=2||c.SecondaryAlternativeCount2404!=1)
            throw new NotSupportedException("Exact node1 primary250/125 alternatives2/1 domain required.");
        var assigned=LegacyNativeWordAlternativeOrdinals.AssignInitial(heap.GetSlotStorageSnapshot().Span,heap.Count,cancellationToken:token).SlotStorage;
        ushort Label(string text)=>alphabet.TryGetLabel(text,out ushort label)?label:ushort.MaxValue;
        byte[] header=alphabet.CopySerializedHeader();ushort H(int at)=>BinaryPrimitives.ReadUInt16LittleEndian(header.AsSpan(at));
        var produced=LegacyNativePrimaryWordRecombination.Apply(assigned.Span,heap.Count,1,c,
            node=>node==0?parents:throw new InvalidDataException("Only actual node0 parent rows are owned."),
            source=>source==1?0xc0eU:source==0?0U:throw new InvalidDataException("Unknown source."),categories,contexts,static symbol=>symbol,
            new(Label("\u0003"),unchecked((uint)preparation.Sources.SubwordSourceIndex22ec)),
            new(Label("\u0003"),120,false,0,0),new(H(8),H(10),H(12),Label("\ufffd")),null,1,new byte[80],new byte[250*80],[],token);
        // All parents below125 take PrimaryParent. No fabricated general-higher
        // geometry callback or secondary-parent reset service is supplied.
        if(produced.Steps.Any(s=>s.Route!=LegacyNativePrimaryWordRecombinationRoute.PrimaryParent&&(!includeWholeStart||s.Route!=LegacyNativePrimaryWordRecombinationRoute.Root)))
            throw new NotSupportedException("Node1 requires genuine primary-parent or explicitly owned root-start route.");
        var ordered=LegacyNativeWordAlternativeOrdering.Prepare(produced.HeapStorage128.Span,heap.Count,false,c,[0,0xc0e],false,new(2,produced.PrimaryStorage40.Span),null,token);
        var descriptor=new LegacyNativeWordHeapDescriptor(ordered.Heap.GetSlotStorageSnapshot().Span,250,ordered.Heap.Count,0,preparation.Limits[0]);
        if(!LegacyNativeWordQuotaController.Apply([descriptor],c,0,0,cancellationToken:token)||descriptor.Count>125||descriptor.AllocatedCapacity!=250)
            throw new InvalidDataException("Actual node1 quota/materialization bound failed.");
        var backing=descriptor.GetBackingStorageSnapshot().ToArray();int[]? primaryOrdinals=retainPrimaryOrdinals?new int[156]:null;if(primaryOrdinals is not null)Array.Fill(primaryOrdinals,-1);var rows=new byte[125*88];var alternatives=new byte[125*80];
        for(int i=0;i<125;i++)
        {
            BinaryPrimitives.WriteInt32LittleEndian(rows.AsSpan(i*88+16),0x4000000);BinaryPrimitives.WriteInt32LittleEndian(rows.AsSpan(i*88+20),0x4000000);
            BinaryPrimitives.WriteInt32LittleEndian(alternatives.AsSpan(i*80),0x4000000);BinaryPrimitives.WriteInt32LittleEndian(alternatives.AsSpan(i*80+4),0x4000000);
        }
        for(int i=0;i<descriptor.Count;i++)
        {
            token.ThrowIfCancellationRequested();if(primaryOrdinals is not null&&i<descriptor.Count)primaryOrdinals[i]=BinaryPrimitives.ReadInt32LittleEndian(backing.AsSpan(i*128+88));var stored=LegacyNativeWordRowMaterializer.Materialize(backing.AsSpan(i*128,128),(ushort)i,125,2,1,ordered.PrimaryAlternativeStorage.Span,cancellationToken:token);
            stored.Record.Span.CopyTo(rows.AsSpan(i*88));stored.PrimaryAlternatives.Span.CopyTo(alternatives.AsSpan(i*80));
        }
        return new(backing,rows,alternatives,descriptor.Count,heap.Count,descriptor,primaryOrdinals);
    }
}
