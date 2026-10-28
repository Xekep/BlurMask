using InkRecognition;
int controls=0;
if(args.Length!=1)throw new ArgumentException("frozen fake-streams.json required");
using var data=System.Text.Json.JsonDocument.Parse(File.ReadAllBytes(args[0]));
foreach(var test in data.RootElement.EnumerateArray())
{
 var uses=test.GetProperty("uses").EnumerateArray().Select(u=>new Use(u[0].GetUInt64(),u[1].GetInt32(),u[2].GetBoolean())).ToArray();
 Compare(uses,test.GetProperty("fault").GetInt32(),test.GetProperty("cancel").GetInt32(),test.GetProperty("quota").GetBoolean(),test);controls++;
}
if(controls!=196)throw new Exception("Incomplete frozen fake streams.");
var collision=LegacyWholeWordPreflightScalarCache.Create(4096,128L*1024*1024);
for(int i=0;i<4096;i++)collision.Store((ulong)i*8192,i%7,i+1,default);
for(int i=0;i<4096;i++)if(!collision.TryGet((ulong)i*8192,out int m,out int r,default)||m!=i%7||r!=i+1)throw new Exception("Collision lost scalar.");
if(collision.Count!=4096||collision.Capacity!=8192||collision.ReservedBytes!=131200)throw new Exception("Cache census differs.");
try{LegacyWholeWordPreflightScalarCache.Create(4096,131199);throw new Exception("Workspace precharge ignored.");}catch(NotSupportedException){}
try{collision.Store(7,0,0,default);throw new Exception("Zero source count accepted.");}catch(ArgumentOutOfRangeException){}
try{collision.Store(0,999,1,default);throw new Exception("Changed immutable scalar accepted.");}catch(InvalidOperationException){}
using(var c=new CancellationTokenSource()){c.Cancel();try{LegacyWholeWordPreflightScalarCache.Create(1,128L*1024*1024,c.Token);throw new Exception("Cancelled allocation accepted.");}catch(OperationCanceledException){}try{collision.TryGet(0,out _,out _,c.Token);throw new Exception("Cancelled hit accepted.");}catch(OperationCanceledException){}try{collision.Store(0,0,1,c.Token);throw new Exception("Cancelled store accepted.");}catch(OperationCanceledException){}}
//Edge controls beyond the frozen stream corpus.
foreach(int invalid in new[]{-1,0,65537})
    Expect<ArgumentOutOfRangeException>(()=>LegacyWholeWordPreflightScalarCache.Create(invalid,long.MaxValue));
var tiny=LegacyWholeWordPreflightScalarCache.Create(1,160);
if(tiny.ReservedBytes!=160||tiny.Capacity!=2||tiny.Count!=0)throw new Exception("Minimum census differs.");
Expect<NotSupportedException>(()=>LegacyWholeWordPreflightScalarCache.Create(1,159));
if(tiny.TryGet(0,out int emptyM,out int emptyR,default)||emptyM!=0||emptyR!=0)throw new Exception("Empty key0 hit.");
tiny.Store(0,0,1,default);tiny.Store(0,0,1,default);
if(tiny.Count!=1||!tiny.TryGet(0,out int zeroM,out int zeroR,default)||zeroM!=0||zeroR!=1)throw new Exception("Key0/zero-match lost.");
Expect<InvalidOperationException>(()=>tiny.Store(1,0,1,default));
if(tiny.Count!=1||tiny.TryGet(1,out _,out _,default))throw new Exception("Failed insertion mutated cache.");
Expect<ArgumentOutOfRangeException>(()=>tiny.Store(0,-1,1,default));
foreach(int n in new[]{1,2,3,4095,4096,4097,65536})
{
 var c=LegacyWholeWordPreflightScalarCache.Create(n,long.MaxValue);
 if(c.Capacity>8192||c.ReservedBytes>131200)throw new Exception("Workspace bound differs.");
}
//Ensure packed arc bits affect buckets: the old XOR-fold put all256 arcs in one.
var hash=typeof(LegacyWholeWordPreflightScalarCache).GetMethod("Hash",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Static)!;
int Bucket(ulong key)=>(int)hash.Invoke(null,new object[]{key})!&8191;
if(Enumerable.Range(0,256).Select(a=>Bucket(7UL|((ulong)a<<24))).Distinct().Count()<200)throw new Exception("Packed owner arcs cluster.");
//Exercise an actual64-key collision chain and its missing-key termination.
var chain=LegacyWholeWordPreflightScalarCache.Create(4096,long.MaxValue);var colliders=new List<ulong>();
for(ulong key=0;colliders.Count<65;key++)if(Bucket(key)==0)colliders.Add(key);
for(int i=0;i<64;i++)chain.Store(colliders[i],i,i+1,default);
for(int i=0;i<64;i++)if(!chain.TryGet(colliders[i],out int m,out int r,default)||m!=i||r!=i+1)throw new Exception("Collision chain lost scalar.");
if(chain.TryGet(colliders[64],out _,out _,default))throw new Exception("Collision false positive.");
Console.WriteLine($"PASS {controls} real-scalar-helper old/new per-use traces +4096 key-pattern/workspace/immutability/cancellation controls; no source authority/model/NN.");
static void Expect<T>(Action action)where T:Exception
{try{action();}catch(T){return;}throw new Exception($"Expected {typeof(T).Name}");}
static void Compare(Use[] uses,int faultKey,int cancelAt,bool quota,System.Text.Json.JsonElement expected)
{
 var a=Run(false);var b=Run(true);if(!a.Events.SequenceEqual(b.Events)||a.Error!=b.Error||a.Rows!=b.Rows||a.Ranges!=b.Ranges)throw new Exception("Old/new use or failure prefix differs.");
 if(b.Reads>a.Reads)throw new Exception("Cache increased pure reads.");
 foreach(var pair in new[]{(Value:a,Name:"old"),(Value:b,Name:"new")})
 {
  var e=expected.GetProperty(pair.Name);var error=e.GetProperty("error");string? expectedError=error.ValueKind==System.Text.Json.JsonValueKind.Null?null:error.GetString();
  if(pair.Value.Error!=expectedError||pair.Value.Rows!=e.GetProperty("rows").GetInt32()||pair.Value.Ranges!=e.GetProperty("ranges").GetInt32()||pair.Value.Reads!=e.GetProperty("reads").GetInt32()||!pair.Value.Events.SequenceEqual(e.GetProperty("events").EnumerateArray().Select(x=>x.GetString()!)))throw new Exception("Independent frozen scalar oracle differs.");
 }

 Result Run(bool enabled)
 {
  var cache=enabled?LegacyWholeWordPreflightScalarCache.Create(Math.Max(1,uses.Length),128L*1024*1024):null;
  var budget=new LegacyGlobalWordTextWorkBudget();if(quota)budget.AddSource(65535,0,0,1);
  var events=new List<string>();int reads=0;string? error=null;using var cancel=new CancellationTokenSource();
  try{for(int i=0;i<uses.Length;i++)
  {
   if(i==cancelAt)cancel.Cancel();cancel.Token.ThrowIfCancellationRequested();var u=uses[i];events.Add($"marker:{i}:{u.Marker}");if(u.Marker<0)throw new ArgumentException("marker");
   if(u.Skip){events.Add($"skip:{i}");continue;}
   int matching=0,references=0;bool hit=cache is not null&&cache.TryGet(u.Key,out matching,out references,cancel.Token);
   if(!hit){reads++;if((long)u.Key==faultKey)throw new InvalidOperationException("immutable metadata failure");matching=(int)(u.Key%7);references=1+(int)(u.Key%3);}
   budget.AddSource(1,matching,references,1);events.Add($"debit:{i}:{matching}:{references}:{budget.Rows}:{budget.Ranges}");
   if(!hit)cache?.Store(u.Key,matching,references,cancel.Token);
  }}catch(Exception e)when(e is ArgumentException or InvalidOperationException or NotSupportedException or OperationCanceledException){error=e.GetType().Name+":"+(e is OperationCanceledException?"cancel":e.Message);}
  return new(events.ToArray(),error,budget.Rows,budget.Ranges,reads);
 }
}
readonly record struct Use(ulong Key,int Marker,bool Skip);
readonly record struct Result(string[] Events,string? Error,int Rows,int Ranges,int Reads);
