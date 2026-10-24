using System.Buffers.Binary;
using System.Text.Json;
using InkRecognition;
if(args.Length!=1||File.Exists(args[0]))throw new ArgumentException("Fresh capture path required.");
var pool=LegacyNativeGlyphRecordPool.CreateWithoutGeometryCapture(false);var initial=pool.AppendDescriptor(0,77,false,new byte[40]);pool.SetOrdinarySymbol(initial.Reference,3);
byte[] compact=new byte[160];
void Row(int n,ulong key,ushort symbol,byte traits,byte score){var r=compact.AsSpan(n*40,40);BinaryPrimitives.WriteUInt64LittleEndian(r[8..],key);BinaryPrimitives.WriteUInt16LittleEndian(r[28..],symbol);r[32]=traits;r[33]=score;}
Row(0,0,1,0x60,255);Row(1,0,2,0x20,128);Row(2,1,4,0x20,100);Row(3,0,1,0x20,200);
var calls=new List<string>();var operations=new LegacyOrdinaryGlyphRefreshOperations(
 (s,t)=>{calls.Add("resolve:"+s);return new(s,s,0);},
 (s,t)=>{calls.Add("score:"+s);return BitConverter.SingleToUInt32Bits(s*.125f);},
 (s,end,t)=>{calls.Add("cost:"+s+":"+(end?1:0));return new(1000+s+(end?10:0));});
int visited=LegacyNativeOrdinaryGlyphRefreshKernel.Apply(pool,0,compact,0,4,0,operations,new byte[40],default);
var owner=pool.CopyOwnerNode(0);var selected=LegacyPreparedGlyphAlternativeSelection.Select(owner,0,new byte[32]);
if(visited!=3||!selected.Succeeded||selected.Count!=3||selected.SelectedMarker!=0)throw new Exception("Refresh/selection structure mismatch.");
using var cancel=new CancellationTokenSource();cancel.Cancel();bool exactCancel=false;
try{LegacyNativeOrdinaryGlyphRefreshKernel.Apply(pool,0,compact,0,4,0,operations,new byte[40],cancel.Token);}catch(OperationCanceledException e){exactCancel=e.GetType()==typeof(OperationCanceledException)&&e.CancellationToken==cancel.Token;}
if(!exactCancel)throw new Exception("Exact supplied pre-entry cancellation required.");
var before=pool.CopyOwnerNode(0).Records.ToArray();int originalCalls=calls.Count;
int repeated=LegacyNativeOrdinaryGlyphRefreshKernel.Apply(pool,0,compact,0,4,0,operations,new byte[40],default);
bool repeatedExact=repeated==3&&before.AsSpan().SequenceEqual(pool.CopyOwnerNode(0).Records.Span)&&calls.Count==originalCalls*2;
if(!repeatedExact)throw new Exception("Repeated source refresh must execute callbacks without duplicating descriptor storage.");
bool badRange=false;
try{LegacyNativeOrdinaryGlyphRefreshKernel.Apply(pool,0,compact,-1,4,0,operations,new byte[40],default);}catch(InvalidDataException){badRange=true;}
if(!badRange||!before.AsSpan().SequenceEqual(pool.CopyOwnerNode(0).Records.Span))throw new Exception("Range refusal mutated pool.");
var large=LegacyNativeGlyphRecordPool.CreateWithoutGeometryCapture(false);
for(int i=0;i<2040;i++){var added=large.AppendDescriptor(0,i,true,new byte[40]);var r=large.CopyRecord(added.Reference);BinaryPrimitives.WriteUInt16LittleEndian(r.AsSpan(8),(ushort)i);BinaryPrimitives.WriteSingleLittleEndian(r.AsSpan(12),i);large.ReplaceRecord(added.Reference,r);}
int maximumOwnerCount=large.ReadGraphRefreshOwnerCount(0,default);var largeNode=large.CopyOwnerNode(0);
var largeSelection=LegacyPreparedGlyphAlternativeSelection.Select(largeNode,0,new byte[12248]);
if(maximumOwnerCount!=2040||!largeSelection.Succeeded||largeSelection.Count!=2040||largeSelection.SelectedMarker!=2039)throw new Exception("Maximum native owner selection mismatch.");
var reverse=largeSelection.CopySelectionRows4();for(int i=0;i<2040;i++)if(BinaryPrimitives.ReadInt32LittleEndian(reverse.AsSpan(i*4))!=2039-i)throw new Exception("Worst insertion order mismatch.");
bool missingOwner=false;try{large.ReadGraphRefreshOwnerCount(1,default);}catch(InvalidDataException){missingOwner=true;}if(!missingOwner)throw new Exception("Foreign/missing owner count accepted.");
using var file=new FileStream(args[0],FileMode.CreateNew);JsonSerializer.Serialize(file,new{Scope="PREPARED_GENERIC_REFRESH_SELECTION_NOT_ACTUAL_EN_OWNER",Visited=visited,Calls=calls,Owner=Convert.ToHexString(owner.Records.Span),Selected=Convert.ToHexString(selected.CopySelectionRows4()),Marker=selected.SelectedMarker,ExactCancel=exactCancel,BadRange=badRange,RepeatedExact=repeatedExact,MaximumOwnerCount=maximumOwnerCount,MaximumSelection=Convert.ToHexString(reverse),MaximumMarker=largeSelection.SelectedMarker,MissingOwnerRefused=missingOwner});
