using InkRecognition;
using System.Buffers.Binary;
using System.Text.Json;

if(args.Length is not (1 or 2))throw new ArgumentException("controls.json [actual-ru-ak.res]");
using var doc=JsonDocument.Parse(File.ReadAllBytes(args[0]));int cases=0,rejections=0;
void Require(bool value,string message){if(!value)throw new InvalidOperationException(message);}
void Reject(Action action){try{action();}catch(Exception e)when(e is ArgumentException or NotSupportedException or OperationCanceledException){return;}throw new Exception("Invalid input admitted.");}
foreach(var c in doc.RootElement.EnumerateArray())
{
 var pool=LegacyNativeGlyphRecordPool.CreateWithoutGeometryCapture(false);
 ulong reference=pool.AppendDescriptor(0,17,false,new byte[40]).Reference;
 pool.ReplaceRecord(reference,Convert.FromBase64String(c.GetProperty("before").GetString()!));
 bool accepted=LegacyNativeGlyphSymbolUpdate.Apply(pool,reference,c.GetProperty("symbol").GetUInt16(),c.GetProperty("count").GetInt32(),c.GetProperty("flag").GetByte(),c.GetProperty("variant").GetBoolean());
 Require(accepted==c.GetProperty("accepted").GetBoolean()&&pool.CopyRecord(reference).SequenceEqual(Convert.FromBase64String(c.GetProperty("after").GetString()!)),"13AF30 packing/precedence/unchanged fields differ from oracle.");
 if(!accepted)rejections++;cases++;
}
int endpoints=0;
for(int n=1;n<=8;n++)
{
 var source=Source(n);var graph=LegacyOrdinaryCursiveCandidateGraph.Generate(source,LegacyOrdinaryCursiveSeedGraph.CreateShiftZero(source),5);
 var delimiters=new LegacyFirstPlainWordDelimiters(graph);
 for(int node=0;node<n;node++)for(ushort arc=0;arc<graph.GetNode(node).Count;arc++)foreach(int capacity in new[]{0,1,20,4096})
 {
  Require(delimiters.ReadEndpoint(-1,ushort.MaxValue,node,arc,capacity)==new LegacyFirstPlainDelimiterResult(0,capacity,0,0),"Leading endpoint emitted fictitious separator.");
  Require(delimiters.ReadEndpoint(node,arc,n,ushort.MaxValue,capacity)==new LegacyFirstPlainDelimiterResult(0,capacity,0,0),"Trailing endpoint emitted fictitious separator.");endpoints+=2;
 }
 Reject(()=>delimiters.ReadEndpoint(-1,0,n,0,20));Reject(()=>delimiters.ReadEndpoint(0,0,0,0,20));
 Reject(()=>delimiters.ReadEndpoint(-1,0,0,ushort.MaxValue,20));Reject(()=>delimiters.ReadEndpoint(-1,0,0,0,4097));
}
var composedPool=LegacyNativeGlyphRecordPool.CreateWithoutGeometryCapture(false);
var word=LegacyNativeWordBuilder.CreateEmpty(composedPool,new byte[120]);ulong previous=ulong.MaxValue;
foreach(var spec in new[]{(Key:0UL,Count:0,Symbol:(ushort)65535),(Key:1UL,Count:1,Symbol:(ushort)17),(Key:2UL,Count:0,Symbol:(ushort)65535)})
{
 ulong reference=word.AppendGlyph(spec.Key,LegacyFirstPlainWordDelimiters.FreshDescriptorIdBase,false,new byte[40]).Reference;
 if(previous!=ulong.MaxValue)word.LinkGlyph(previous,reference);
 Require(LegacyNativeGlyphSymbolUpdate.Apply(composedPool,reference,spec.Symbol,spec.Count,0,false),"Native delimiter update rejected.");previous=reference;
}
Require(word.GetReferenceCount()==3&&word.GetSymbolSpan()==1,"Zero-span refs removed or counted as text.");
var published=new LegacyNativePlainWordPublisher(composedPool,false,false,false).Publish(word,(n,a)=>0,(n,a)=>2);
Require(published.Word.GetReferenceCount()==3&&published.Word.GetSymbolSpan()==1&&published.Word.FlatReferences.SequenceEqual(new ulong[]{2,1,0}),"Publisher lost delimiter references/readorder.");
using(var cancellation=new CancellationTokenSource())
{
 cancellation.Cancel();byte[] before=composedPool.CopyRecord(0);Reject(()=>LegacyNativeGlyphSymbolUpdate.Apply(composedPool,0,32,0,0,false,cancellation.Token));
 Require(composedPool.CopyRecord(0).SequenceEqual(before),"Canceled update mutated pool.");
}
Reject(()=>LegacyNativeGlyphSymbolUpdate.Apply(composedPool,ulong.MaxValue,0,0,0,false));
Console.WriteLine($"PASS {cases} independent13AF30 cases/{rejections} unchanged rejections; {endpoints} owned ordinary endpoint checks; count0->kind1/label1->WORD276/publisher275 keeps3refs/1symbol, cancellation and ownership.");
if(args.Length==2)
{
 byte[] resource=File.ReadAllBytes(args[1]);int actualCases=0,actualEndpoints=0;
 foreach(float scale in new[]{.5f,1f,2f})foreach(float shift in new[]{-70f,0f,130f})
 {
  InkStroke[] raw=[Draw((0,90),(0,10),(25,60),(50,10),(50,90)),Draw((65,90),(65,10)),Draw((65,90),(115,10)),Draw((115,90),(115,10)),Draw((130,90),(130,10),(175,10),(185,20),(185,45),(175,55),(130,55))];
  var transformed=raw.Select(s=>new InkStroke(s.Points.Select(p=>new InkPoint(p.X*scale+shift,p.Y*scale-shift)).ToArray())).ToArray();
  var factory=LegacyFreshFirstResultOwnershipFactory.Prepare(resource,6120,transformed);var row=factory.Frame.Row;var cuts=row.GenerateRussianCuts();
  var seeds=LegacyOrdinaryCursiveSeedProducer.GenerateLeftToRight(cuts.Cuts,row.Frame.Line.Height,LegacyOrdinaryCursiveSeedProfile.ReadV10aResourceNode(resource,12727),new byte[transformed.Length],Enumerable.Repeat(-1,transformed.Length).ToArray());
  var graph=LegacyOrdinaryCursiveCandidateGraph.Generate(seeds,LegacyOrdinaryCursiveSeedGraph.CreateShiftZero(seeds),5);var callback=new LegacyFirstPlainWordDelimiters(graph);
  var actualResult=factory.CreatePlainResult();var actualWord=LegacyNativeWordBuilder.CreateEmpty(actualResult.GlyphPool,new byte[120]);
  var leading=callback.ReadEndpoint(-1,0,0,0,20);var trailing=callback.ReadEndpoint(graph.NodeCount-1,0,graph.NodeCount,0,20);
  foreach(var tuple in new[]{(Key:0UL,Count:leading.EmittedSymbolCount,Flag:leading.ComponentFlag,Variant:leading.ExternalVariant),(Key:(ulong)graph.NodeCount,Count:trailing.EmittedSymbolCount,Flag:trailing.ComponentFlag,Variant:trailing.ExternalVariant)})
  {
   ulong prior=actualWord.HeadReference;var added=actualWord.AppendGlyph(tuple.Key,LegacyFirstPlainWordDelimiters.FreshDescriptorIdBase,false,new byte[40]);
   if(prior!=ulong.MaxValue)actualWord.LinkGlyph(prior,added.Reference);
   Require(LegacyNativeGlyphSymbolUpdate.Apply(actualResult.GlyphPool,added.Reference,65535,tuple.Count,tuple.Flag,tuple.Variant!=0),"Actual resource endpoint failed symbol update.");
   byte[] updated=actualResult.GlyphPool.CopyRecord(added.Reference);Require(BinaryPrimitives.ReadUInt16LittleEndian(updated.AsSpan(4))==1&&BinaryPrimitives.ReadUInt16LittleEndian(updated.AsSpan(8))==1,"Actual endpoint acquired fabricated space/text.");actualEndpoints++;
  }
  Require(actualWord.GetReferenceCount()==2&&actualWord.GetSymbolSpan()==0,"Actual resource endpoint refs fabricated text.");actualCases++;
 }
 Console.WriteLine($"PASS {actualCases} own raw MIR scale/translation ->resource first6120 frame/cuts/ordinary graph/freshresult factory ->{actualEndpoints} actual endpoint updates. Endpoint-only: no glyph NN inference, Russian accuracy or main240 decoder claim.");
}
static LegacyOrdinaryCursiveSeedResult Source(int count)
{
 var raw=Enumerable.Range(0,count).Select(i=>new InkStroke(new[]{new InkPoint(i*100,10),new InkPoint(i*100+20,90)})).ToArray();
 var prepared=LegacyCursivePreparedInk.Create(raw);var markers=Enumerable.Range(0,count).Select(i=>new LegacyCursiveCutMarker(0,i,0)).ToArray();
 var cuts=new LegacyCursiveCutResult(prepared,markers,LegacyCursiveInkAtoms.Build(prepared.StrokePointCounts.Span,markers));
 return LegacyOrdinaryCursiveSeedProducer.GenerateLeftToRight(cuts,80,new(true,0,.3f,.3f),new byte[count],Enumerable.Repeat(-1,count).ToArray());
}
static InkStroke Draw(params(float X,float Y)[] points)=>new(points.Select(p=>new InkPoint(p.X,p.Y)).ToArray());
