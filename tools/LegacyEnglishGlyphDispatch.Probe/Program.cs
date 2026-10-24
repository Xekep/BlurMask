using InkRecognition;
using System.Text.Json;
using Dispatch=InkRecognition.LegacyEnglishCaseNode0Completion.GraphGlyphRefreshDispatch;
if(args.Length!=1)throw new ArgumentException("fresh output required");
var pool=LegacyNativeGlyphRecordPool.CreateWithoutGeometryCapture(false);
var foreign=LegacyNativeGlyphRecordPool.CreateWithoutGeometryCapture(false);
var state=new Dispatch(pool,2,default);ulong key=0x1000001;
if(state.TryRead(pool,key,default,out _))throw new Exception("Fresh state");
var fill=new[]{new LegacyOrdinaryGraphSourceReference(new(0,0,0),new(0,0,2),3,5)};
var receipt=new Dispatch.Receipt(fill,new byte[26],0x3e800000,7);
state.Remember(pool,key,receipt,default);
if(!state.TryRead(pool,key,default,out var got)||!ReferenceEquals(fill,got.Fill)||got.Bits!=0x3e800000||got.Visited!=7)throw new Exception("Exact cache receipt");
ulong marked=key|(7UL<<40);if(LegacyNativeGlyphRecordOwner.NormalizeOwnerKey(marked)!=key)throw new Exception("marker alias");
if(!state.TryRead(pool,LegacyNativeGlyphRecordOwner.NormalizeOwnerKey(marked),default,out got))throw new Exception("Alias cache");
int calls=1;foreach(var k in new[]{key,key,0x2000001UL}){if(!state.TryRead(pool,k,default,out _)){calls++;state.Remember(pool,k,receipt,default);}}
if(calls!=2)throw new Exception("Per owner dispatch");
var guards=new List<string>();
Refuse<InvalidOperationException>("foreign pool",()=>state.TryRead(foreign,key,default,out _));
Refuse<InvalidOperationException>("marker key",()=>state.TryRead(pool,marked,default,out _));
Refuse<InvalidOperationException>("duplicate",()=>state.Remember(pool,key,receipt,default));
Refuse<InvalidOperationException>("capacity",()=>state.Remember(pool,0x3000001,receipt,default));
Refuse<NotSupportedException>("zero capacity",()=>new Dispatch(pool,0,default));
Refuse<NotSupportedException>("oversize capacity",()=>new Dispatch(pool,129,default));
Refuse<InvalidDataException>("malformed receipt",()=>new Dispatch(pool,1,default).Remember(pool,key,new(fill,new byte[25],0,0),default));
using var cts=new CancellationTokenSource();cts.Cancel();
try{state.TryRead(pool,key,cts.Token,out _);throw new Exception("Cancellation ignored");}catch(OperationCanceledException e){if(e.GetType()!=typeof(OperationCanceledException)||e.CancellationToken!=cts.Token)throw;guards.Add("exact cancellation");}
File.WriteAllText(args[0],JsonSerializer.Serialize(new{Scope="PREPARED_CACHE_NUMERIC_ONLY_NOT_OWNER_AUTHORITY",FreshHit=false,AliasHit=true,CallbackCalls=calls,ReadyOwners=2,Guards=guards,LiveOwnerExecuted=false,NN=false}));
void Refuse<T>(string name,Action a)where T:Exception{try{a();}catch(Exception e){if(e.GetType()!=typeof(T))throw;guards.Add(name);return;}throw new Exception("Not refused "+name);}
