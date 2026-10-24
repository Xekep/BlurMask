using System.Buffers.Binary;
using System.Text.Json;
using InkRecognition;
using Owner=InkRecognition.LegacyEnglishCaseNode0Completion;
using Dispatch=InkRecognition.LegacyEnglishCaseNode0Completion.GraphGlyphRefreshDispatch;
if(args.Length!=1||File.Exists(args[0]))throw new ArgumentException("Fresh output required.");
var cases=new List<object>();ulong key=0x1000001;int controls=0;
for(int traits=0;traits<256;traits++)
{
 var pool=LegacyNativeGlyphRecordPool.CreateWithoutGeometryCapture(false);pool.AppendDescriptor(key,99,false,new byte[40]);
 var compact=new byte[160];int predicted=0;
 for(int i=0;i<4;i++)
 {
  var row=compact.AsSpan(i*40,40);row[32]=(byte)(traits^(i==1?0x40:0));
  BinaryPrimitives.WriteUInt64LittleEndian(row[8..],i==3?key+1:key);BinaryPrimitives.WriteUInt16LittleEndian(row[28..],(ushort)(10+i));
  if(Owner.GraphRefreshMatches(row,key))predicted++;
 }
 int callbacks=0;var ops=new LegacyOrdinaryGlyphRefreshOperations((s,t)=>{callbacks++;return new(s,s,0);},(s,t)=>0x3f000000,(s,e,t)=>new(123));
 int actual=LegacyNativeOrdinaryGlyphRefreshKernel.Apply(pool,key,compact,0,4,key,ops,new byte[40],default);
 if(actual!=predicted||callbacks!=predicted)throw new InvalidDataException("Preflight/kernel predicate disagreement.");
 var dispatch=new Dispatch(pool,2,default);if(dispatch.TryRead(pool,key,default,out _))throw new InvalidDataException();
 var receipt=new Dispatch.Receipt([new(new(0,0,0),new(0,0,2),3,5)],new byte[26],0x3f000000,actual);dispatch.Remember(pool,key,receipt,default);
 if(!dispatch.TryRead(pool,LegacyNativeGlyphRecordOwner.NormalizeOwnerKey(key|(7UL<<40)),default,out var cached)||!ReferenceEquals(cached.Fill,receipt.Fill))throw new InvalidDataException();
 cases.Add(new{Traits=traits,Predicted=predicted,Actual=actual,Callbacks=callbacks,ColdCharge=Owner.GraphRefreshCharge(8192,16384,predicted,1),WarmRefreshCharge=0,FullOwner=pool.CopyOwnerNode(key).Records});controls++;
}
var entries=new List<object>();for(int b=0;b<32;b++)
{bool result=Owner.GraphCharacterEntryAllowed((b&1)!=0,(b&2)!=0,(b&4)!=0,(b&8)!=0,(b&16)!=0);entries.Add(new{Bits=b,Allowed=result});}
var charges=new List<object>();foreach(int m in new[]{0,1,2,10,128,4096})foreach(int rows in new[]{0,1,10,2040})
 charges.Add(new{Matched=m,Rows=rows,Charge=Owner.GraphRefreshCharge(8192,16384,m,rows)});
var ledger=LegacyEnglishCaseNode0Ledger.Create(LegacyEnglishCaseNode0Ledger.MaximumAggregate);
ledger.Allocate(LegacyEnglishCaseNode0Ledger.Site.FinalImages,LegacyEnglishCaseNode0Ledger.ContinuationCeiling-ledger.Payload-200000);
long before=ledger.Payload;bool oldRefused=false;try{ledger.Allocate(LegacyEnglishCaseNode0Ledger.Site.FinalImages,1048576+8192);}catch(InvalidDataException){oldRefused=true;}
if(!oldRefused||ledger.Payload!=before)throw new InvalidDataException("Refusal mutated used prefix.");
var successor=Owner.GraphRefreshCharge(8192,16384,2,1);ledger.Allocate(LegacyEnglishCaseNode0Ledger.Site.FinalImages,successor.Payload);
var refusals=new List<string>();
Refuse<ArgumentException>("row39",()=>Owner.GraphRefreshMatches(new byte[39],key));
Refuse<ArgumentOutOfRangeException>("negative-source",()=>Owner.GraphRefreshCharge(-1,0,0,0));
Refuse<ArgumentOutOfRangeException>("overmatched",()=>Owner.GraphRefreshCharge(0,0,4097,1));
Refuse<InvalidOperationException>("foreign-cache",()=>new Dispatch(poolForGuard(),1,default).TryRead(poolForGuard(),key,default,out _));
File.WriteAllBytes(args[0],JsonSerializer.SerializeToUtf8Bytes(new{Scope="PREPARED_REFRESH_COUNTS_CACHE_REUSE_AND_SYNTHETIC_LEDGER_PREFIX_NOT_ACTUAL_74_REPLAY",Cases=cases,EntryGuards=entries,Charges=charges,Controls=controls,ProgressSize=System.Runtime.CompilerServices.Unsafe.SizeOf<Owner.GraphGlyphAllocationProgress>(),ProgressReferenceFree=!System.Runtime.CompilerServices.RuntimeHelpers.IsReferenceOrContainsReferences<Owner.GraphGlyphAllocationProgress>(),
 SyntheticPrefix=new{Before=before,OldRequested=1048576+8192,OldRefused=oldRefused,NewRequested=successor.Payload,After=ledger.Payload},Refusals=refusals,NN=false,Actual74UsedPrefixAvailable=false,ActualOwnerExecuted=false}));
static LegacyNativeGlyphRecordPool poolForGuard()=>LegacyNativeGlyphRecordPool.CreateWithoutGeometryCapture(false);
void Refuse<T>(string name,Action action)where T:Exception{try{action();}catch(Exception e){if(e.GetType()!=typeof(T))throw;refusals.Add(name);return;}throw new InvalidDataException("Not refused "+name);}
