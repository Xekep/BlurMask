using InkRecognition;
using System.Buffers.Binary;
int cases=0;
foreach(bool duplicate in new[]{false,true})for(int shift=0;shift<32;shift++)
{
 const ulong owner=9,source=17;byte[] compact=new byte[256*40];int first=shift,end=256-shift,expected=0;
 for(int i=0;i<256;i++)
 {
  var row=compact.AsSpan(i*40,40);byte traits=(byte)((i+shift)&255);ulong key=i%3==0?source:source+1;
  row[32]=traits;BinaryPrimitives.WriteUInt64LittleEndian(row[8..],key);BinaryPrimitives.WriteUInt16LittleEndian(row[28..],(ushort)(i+1));
  if(i>=first&&i<end&&(traits&0x27)==0x20&&key==source)expected++;
 }
 var pool=LegacyNativeGlyphRecordPool.CreateWithoutGeometryCapture(false);pool.AppendDescriptor(owner,10000,duplicate,new byte[40]);
 // Prefill the actual matching IDs. false dedups on refresh; true appends despite matching IDs.
 for(int i=first;i<end;i++){var row=compact.AsSpan(i*40,40);if((row[32]&0x27)==0x20&&BinaryPrimitives.ReadUInt64LittleEndian(row[8..])==source)pool.AppendDescriptor(owner,i-first,false,new byte[40]);}
 int initialRows=pool.CopyOwnerNode(owner).Records.Length/40;int resolves=0,scores=0,costs=0;
 var ops=new LegacyOrdinaryGlyphRefreshOperations((s,t)=>{resolves++;return new(s,s,0);},(s,t)=>{scores++;return 0x3f000000;},(s,e,t)=>{costs++;return new(123);});
 int actual=LegacyNativeOrdinaryGlyphRefreshKernel.Apply(pool,owner,compact,first,end,source,ops,new byte[40],default);
 if(actual!=expected||resolves!=expected||scores!=expected||costs!=expected||pool.CopyOwnerNode(owner).Records.Length!=40*(initialRows+(duplicate?expected:0)))throw new Exception("Actual kernel predicate/append bound differs.");
 int increase=pool.CopyOwnerNode(owner).Records.Length/40-initialRows;if(increase<0||increase>expected||!duplicate&&increase!=0)throw new Exception("Append increase exceeds matching bound or misses dedup.");
 cases++;
}
int failurePrefixes=0;
foreach(bool duplicate in new[]{false,true})
{
 const ulong owner=9,source=17;const int matching=4;byte[] compact=new byte[matching*40];
 for(int i=0;i<matching;i++){var row=compact.AsSpan(i*40,40);row[32]=0x20;BinaryPrimitives.WriteUInt64LittleEndian(row[8..],source);BinaryPrimitives.WriteUInt16LittleEndian(row[28..],(ushort)(10+i));}
 var pool=LegacyNativeGlyphRecordPool.CreateWithoutGeometryCapture(false);pool.AppendDescriptor(owner,10000,duplicate,new byte[40]);pool.AppendDescriptor(owner,0,false,new byte[40]);
 int initial=pool.CopyOwnerNode(owner).Records.Length/40,resolves=0,scores=0,costs=0;
 var ops=new LegacyOrdinaryGlyphRefreshOperations((s,t)=>{resolves++;return new(s,s,0);},(s,t)=>{if(++scores==2)throw new InvalidOperationException("fake-score-failure");return 0x3f000000;},(s,e,t)=>{costs++;return new(123);});
 bool failed=false;
 try{LegacyNativeOrdinaryGlyphRefreshKernel.Apply(pool,owner,compact,0,matching,source,ops,new byte[40],default);}
 catch(InvalidOperationException e)when(e.Message=="fake-score-failure"){failed=true;}
 int increase=pool.CopyOwnerNode(owner).Records.Length/40-initial;
 if(!failed||resolves!=2||scores!=2||costs!=1||increase!=(duplicate?2:1)||increase>matching)throw new Exception("Actual callback failure prefix exceeded matching bound or lost writes.");
 failurePrefixes++;
}
var budget=new LegacyGlobalWordTextWorkBudget();budget.AddSource(65530,0,0,1);budget.AddSource(1,2,1,1);if(budget.Rows!=65533||budget.Ranges!=1)throw new Exception("Exact matching budget differs.");
try{var old=new LegacyGlobalWordTextWorkBudget();old.AddSource(65530,0,0,1);old.AddSource(1,256,1,1);throw new Exception("Global compact bound unexpectedly fit.");}catch(NotSupportedException){}
Console.WriteLine($"PASS {cases} real-kernel matching-ID/dedup cases +{failurePrefixes} callback failure prefixes + matching/global budget boundary; no model/native.");
