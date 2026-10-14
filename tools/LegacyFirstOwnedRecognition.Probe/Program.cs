using InkRecognition;
using System.Text.Json;
using System.Runtime.InteropServices;
using System.Buffers.Binary;
if(args.Length is not (4 or 5))throw new ArgumentException("AK.res engine-profile.json separation-profile.json captures.json [external-corpus.json]");
byte[] resource=File.ReadAllBytes(args[0]);using var engine=JsonDocument.Parse(File.ReadAllBytes(args[1]));var a=engine.RootElement.GetProperty("activations");
var activations=new LegacyInkActivations(a.GetProperty("hidden").EnumerateArray().Select(v=>v.GetInt32()).ToArray(),a.GetProperty("output").EnumerateArray().Select(v=>v.GetInt32()).ToArray());var table=LegacyCursiveSeparationTable.Load(File.ReadAllBytes(args[2]));
InkStroke[][] fixtures=[
 [Draw((0,90),(0,10),(25,60),(50,10),(50,90)),Draw((65,90),(65,10)),Draw((65,90),(115,10)),Draw((115,90),(115,10)),Draw((130,90),(130,10),(175,10),(185,20),(185,45),(175,55),(130,55))],
 [Draw((0,0),(60,0),(60,80),(0,80),(0,0)),Draw((20,30),(30,50))],
 [Draw((0,0),(60,0),(60,80),(0,80),(0,0))],
 [Draw((0,0),(0,80)),Draw((200,0),(200,80)),Draw((400,0),(400,80))]
];
using var captureFile=File.Create(args[3]);using var w=new Utf8JsonWriter(captureFile);w.WriteStartArray();int groups=0,actual=0,external=0,externalUnsupported=0;
foreach(var raw in fixtures)
{
 var owner=LegacyFreshFirstOwnedRecognition.Prepare(resource,6120,raw,activations,table);var result=owner.Run();groups+=owner.GroupScores.Count;actual++;
 var copied=owner.MaskPolicy.CopyMasks();if(copied.Primary.Length!=192||copied.Primary.Span.IndexOf((byte)0)!=-1||copied.Secondary.Span.IndexOf((byte)0)!=-1||owner.MaskPolicy.WidePairCount!=0||owner.MaskPolicy.ClassPreselectionActive)throw new Exception("Actual native first-mask profile changed.");
 if(!MemoryMarshal.TryGetArray(copied.Primary,out ArraySegment<byte> storage))throw new Exception("Copy fixture must be backed by owned storage.");storage.Array![storage.Offset]=0;
 if(owner.MaskPolicy.CopyMasks().Primary.Span[0]!=1)throw new Exception("Public mask copy altered the live owning session.");
 var rerun=owner.Run();if((rerun.Materialized.Selection with {ContextIdentity=0})!=(result.Materialized.Selection with {ContextIdentity=0})||!rerun.Primary.GetRowCopy(0).CopyStorage().AsSpan().SequenceEqual(result.Primary.GetRowCopy(0).CopyStorage()))throw new Exception("Owned model/session rerun changed primary/selection.");
 if(!ReferenceEquals(result.Materialized.Result.Factory,owner.Factory)||!ReferenceEquals(result.Materialized.Cuts,owner.Cuts)||!ReferenceEquals(result.Materialized.Graph,owner.Prepared.SourceRanges.Graph))throw new Exception("Actual whole first result escaped its owning frame/cuts/graph.");
 using(var canceled=new CancellationTokenSource())
 {canceled.Cancel();try{owner.Run(canceled.Token);throw new Exception("Canceled owned recognition accepted.");}catch(OperationCanceledException){}}
 Capture(owner,result,false,null,null);
}
if(groups!=57||actual!=4)throw new Exception("Native owned fixture coverage changed.");
//Mutating caller raw point storage after Prepare cannot alter the owned frame.
var mutablePoints=new[]{new InkPoint(0,0),new InkPoint(60,0),new InkPoint(60,80),new InkPoint(0,80),new InkPoint(0,0)};
var isolated=LegacyFreshFirstOwnedRecognition.Prepare(resource,6120,new[]{new InkStroke(mutablePoints)},activations,table);var baseline=isolated.Run();mutablePoints[0]=new(float.NaN,float.NaN);var after=isolated.Run();
if((baseline.Materialized.Selection with {ContextIdentity=0})!=(after.Materialized.Selection with {ContextIdentity=0}))throw new Exception("Caller point mutation altered owned source.");
if(args.Length==5)
{
 using var corpus=JsonDocument.Parse(File.ReadAllBytes(args[4]));
 foreach(var sample in corpus.RootElement.GetProperty("samples").EnumerateArray())
 {
  //Expected text is metrics-only and is NEVER supplied to the owning engine.
  string id=sample.GetProperty("id").GetString()!,expected=sample.GetProperty("expected").GetString()!;
  var ink=sample.GetProperty("strokes").EnumerateArray().Select(s=>new InkStroke(s.EnumerateArray().Select(p=>new InkPoint(p[0].GetSingle(),p[1].GetSingle())).ToArray())).ToArray();
  try{var owner=LegacyFreshFirstOwnedRecognition.Prepare(resource,6120,ink,activations,table);var result=owner.Run();Capture(owner,result,true,id,expected);external++;}
  catch(Exception e)when(e is NotSupportedException or ArgumentException or InvalidDataException or InvalidOperationException)
  {w.WriteStartObject();w.WriteBoolean("external",true);w.WriteString("id",id);w.WriteString("expected",expected);w.WriteString("error",e.GetType().Name+": "+e.Message);w.WriteEndObject();externalUnsupported++;Console.WriteLine($"External diagnostic unsupported {id}: {e.Message}");}
 }
}
w.WriteEndArray();w.Flush();Console.WriteLine($"PASS4 actual raw owning first/57NNgroups; modelmask copies, rerun, cancellation and raw-point ownership. External coordinates diagnostics supported={external}/unsupported={externalUnsupported}; no complete Russian/winner claim.");
void Capture(LegacyFreshFirstOwnedRecognition owner,LegacyFreshFirstOwnedRecognitionResult result,bool externalSample,string? id,string? expected)
{
 var native=result.Materialized;var grid=new LegacyFirstMergedRecordGrid(result.Contexts.ColumnCountB8,result.Contexts.BeamWidth,result.Contexts.CopyB8Storage());var trace=LegacyFirstSelectedGlyphTrace.Read(grid,native.Selection);
 string text=string.Concat(native.GlyphReferences.Select(reference=>{byte[] row=native.Result.GlyphPool.CopyRecord(reference);ushort label=BinaryPrimitives.ReadUInt16LittleEndian(row.AsSpan(8));return owner.Factory.InvocationPolicy.Binding.Alphabet.TryGetText(label,out string value)?value:"";}));
 w.WriteStartObject();w.WriteBoolean("external",externalSample);if(id is not null)w.WriteString("id",id);if(expected is not null)w.WriteString("expected",expected);
 w.WriteNumber("groups",owner.GroupScores.Count);w.WriteNumber("cost",native.Selection.TotalCost);w.WriteBoolean("hasWord",native.PublishedWord is not null);w.WriteString("allocatedLiteralText",text);w.WriteString("publishedLiteralText",native.PublishedWord is not null?text:null);w.WriteNumber("glyphs",native.GlyphReferences.Count);
 var masks=owner.MaskPolicy.CopyMasks();w.WriteString("primaryMask",Convert.ToHexString(masks.Primary.Span));w.WriteString("secondaryMask",Convert.ToHexString(masks.Secondary.Span));w.WriteNumber("widePairs",owner.MaskPolicy.WidePairCount);
 w.WriteStartArray("labels");foreach(var c in trace)w.WriteNumberValue(c.Symbol);w.WriteEndArray();
 w.WriteStartArray("costs");foreach(var score in owner.GroupScores){w.WriteStartArray();foreach(int v in score.Cache.Costs.Span)w.WriteNumberValue(v);w.WriteEndArray();}w.WriteEndArray();
 w.WriteStartArray("primary98");for(int n=0;n<result.Primary.NodeCount;n++)w.WriteStringValue(Convert.ToHexString(result.Primary.GetRowCopy(n).CopyStorage()));w.WriteEndArray();
 w.WriteStartArray("b8");foreach(var b in result.Contexts.CopyB8Storage())w.WriteStringValue(Convert.ToHexString(b.Span));w.WriteEndArray();w.WriteEndObject();
 Console.WriteLine($"{(externalSample?"External coordinates":"Own printed")} {(id??actual.ToString())}: cost{native.Selection.TotalCost}, selected[{string.Join(',',trace.Select(c=>c.Symbol))}], literal {JsonEncodedText.Encode(text)}, word={native.PublishedWord is not null}.");
}
static InkStroke Draw(params(float X,float Y)[] p)=>new(p.Select(v=>new InkPoint(v.X,v.Y)).ToArray());
