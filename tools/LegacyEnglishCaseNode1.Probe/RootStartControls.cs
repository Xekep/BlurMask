using System.Buffers.Binary;
using InkRecognition;
internal static class RootStartControls
{
 internal static object Run(LegacyNativeWordHeapPreparation preparation,LegacyCursiveCategoryProfile categories,LegacyInkAlphabet alphabet)
 {
  var controls=new List<object>();int defaultRefusals=0;
  foreach(int cost in new[]{-1,0,200})foreach(int increment in new[]{-20,0,50})
  {
   var contexts=new LegacySymbolContextFactory(4096);var parents=Parents(contexts);var heap=new LegacyNativeWordRecordHeap(250);
   var prefix=LegacyInitialWordRecordProducer.InitializeAllocatedPrefix(Fields(0x100000003UL,cost,increment) with {SourceSpan=1,PreviousArcIndex1e=65535,AnchorEndingNode38=1,AnchorArcIndex3c=65535},0);
   heap.TryAllocate(cost,slot=>prefix.CopyTo(slot));var before=heap.GetSlotStorageSnapshot().ToArray();
   var result=LegacyEnglishCaseNode1Materialization.Build(heap,preparation,categories,contexts,parents,alphabet,default,true);
   if(result.Count!=1)throw new Exception("One root-start must survive.");
   controls.Add(new{Cost=cost,Increment=increment,Before=Hex(before),Heap=Hex(result.Heap128),Rows=Hex(result.Rows88),Alts=Hex(result.Alternatives80)});
   try{_=LegacyEnglishCaseNode1Materialization.Build(heap,preparation,categories,contexts,parents,alphabet,default);throw new Exception("Default span0 accepted root.");}catch(NotSupportedException){defaultRefusals++;}
  }
  var cx=new LegacySymbolContextFactory(4096);var mixedParents=Parents(cx);var sameHeap=new LegacyNativeWordRecordHeap(250);
  var parentPrefix=LegacyInitialWordRecordProducer.InitializeAllocatedPrefix(Fields(0x100000002UL,100,30),0);
  sameHeap.TryAllocate(100,s=>parentPrefix.CopyTo(s));
  var rootPrefix=LegacyInitialWordRecordProducer.InitializeAllocatedPrefix(Fields(0x100000003UL,200,50) with {SourceSpan=1,PreviousArcIndex1e=65535,AnchorEndingNode38=1,AnchorArcIndex3c=65535},0);
  sameHeap.TryAllocate(200,s=>rootPrefix.CopyTo(s));
  var mixed=LegacyEnglishCaseNode1Materialization.Build(sameHeap,preparation,categories,cx,mixedParents,alphabet,default,true);
  if(mixed.Count!=2||mixed.Rows88[32]!=1||mixed.Rows88[88+32]!=0)throw new Exception("Same heap did not retain root and parent branches.");
  var competition=new List<object>();
  foreach(int rootCost in new[]{-100,10300,20000})
  {
   var cc=new LegacySymbolContextFactory(4096);var pp=Parents(cc);var hh=new LegacyNativeWordRecordHeap(250);
   for(int i=0;i<250;i++){int cost=10000+i*2;var bytes=LegacyInitialWordRecordProducer.InitializeAllocatedPrefix(Fields(0x100000002UL+(ulong)i,cost,30),0);hh.TryAllocate(cost,slot=>bytes.CopyTo(slot));}
   var root=LegacyInitialWordRecordProducer.InitializeAllocatedPrefix(Fields(0x100004000UL,rootCost,50) with {SourceSpan=1,PreviousArcIndex1e=65535,AnchorEndingNode38=1,AnchorArcIndex3c=65535},0);
   bool inserted=hh.TryAllocate(rootCost,slot=>root.CopyTo(slot));
   var final=LegacyEnglishCaseNode1Materialization.Build(hh,preparation,categories,cc,pp,alphabet,default,true);
   var costs=new int[final.Count];int roots=0;
   for(int i=0;i<final.Count;i++){costs[i]=BinaryPrimitives.ReadInt32LittleEndian(final.Rows88.AsSpan(i*88+16));if(final.Rows88[i*88+32]==1)roots++;}
   if(final.Count!=125)throw new Exception("Shared root/parent quota was bypassed.");
   competition.Add(new{InputRootCost=rootCost,Inserted=inserted,Count=final.Count,RootRows=roots,RetainedCosts=costs});
  }
  var query=new LegacyPreparedDictionaryQuery{Capacity=128,ContextRoot=cx.ContainerIdentity};var tag=LegacyOrdinaryDictionaryQueryPreparation.ApplyInitial(query,new(0,0,false,false,120),1,1,0,false);
  return new{Scope="PREPARED root-start/mixed alternatives only, not actual source/NN",Controls=controls,DefaultRootRefusals=defaultRefusals,Competition=competition,Mixed=new{Count=mixed.Count,Heap=Hex(mixed.Heap128),Rows=Hex(mixed.Rows88),Alts=Hex(mixed.Alternatives80),Parent=Hex(mixedParents[0].Record.ToArray()),ParentAlt=Hex(mixedParents[0].PrimaryAlternatives.ToArray())},Query=new{Tag=tag,Node=query.Node,query.Skip,query.Parameter1c,query.Parameter20,query.Flags24,query.Flags26}};
 }
 static LegacyInitialWordRecordFields Fields(ulong tag,int cost,int increment)=>new(tag,0,cost,increment,2,1,0,0,0,0,0,0,0,0,0,0,0,0,0,-1,0,0,0);
 static LegacyNativeStoredWordRecord[] Parents(LegacySymbolContextFactory cx)
 {
  ulong a=cx.ResolveInitial(1).Identity,b=cx.ResolveInitial(2).Identity;var row=new byte[88];W64(row,0,0x100000001UL);W64(row,8,a);W32(row,16,100);W32(row,20,20);W16(row,36,1);W16(row,60,0);W16(row,62,65535);
  var alt=new byte[80];for(int k=0;k<2;k++){int at=k*40;W32(alt,at,10+k*10);W32(alt,at+4,2+k);W64(alt,at+8,a);W64(alt,at+16,b);alt[at+30]=1;alt[at+31]=32;}
  return Enumerable.Range(0,125).Select(_=>new LegacyNativeStoredWordRecord(row.ToArray(),alt.ToArray(),[])).ToArray();
 }
 static string Hex(byte[] x)=>Convert.ToHexString(x);
 static void W16(byte[] b,int at,ushort v)=>BinaryPrimitives.WriteUInt16LittleEndian(b.AsSpan(at),v);
 static void W32(byte[] b,int at,int v)=>BinaryPrimitives.WriteInt32LittleEndian(b.AsSpan(at),v);
 static void W64(byte[] b,int at,ulong v)=>BinaryPrimitives.WriteUInt64LittleEndian(b.AsSpan(at),v);
}
