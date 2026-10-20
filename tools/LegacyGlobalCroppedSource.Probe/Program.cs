using InkRecognition;
using System.Text.Json;
using System.Buffers.Binary;
if(args.Length!=3)throw new ArgumentException("MODELS RECORDED-MANIFEST CAPTURE");
string m=args[0];byte[] R(string n)=>File.ReadAllBytes(Path.Combine(m,n));
byte[] ak=R("ru-RU/ru_RU-ak-cur.lite.res");
var models=LegacyFreshMainRecognitionContext.Load(ak,R("ru-RU/ru_RU-lk-text.lite.res"),R("ru-RU/legacy-engine-profile.json"),R("line-geometry/line-2e3-30.res"),R("line-geometry/line-2e3-50.res"));
var table=LegacyCursiveSeparationTable.Load(R("ru-RU/legacy-separation-profile.json"));
using var cs=File.OpenRead(Path.Combine(m,"text-unicode/legacy-case-data.json"));using var ps=File.OpenRead(Path.Combine(m,"text-unicode/legacy-text-publication-data.json"));
using var ns=File.OpenRead(Path.Combine(m,"text-unicode/legacy-normalization-data.json"));using var cis=File.OpenRead(Path.Combine(m,"text-unicode/legacy-circled-payload-data.json"));
var cd=LegacyNativeCaseData.Load(cs);var pd=LegacyNativeTextPublicationData.Load(ps);var norm=LegacyNativeUnicodeNormalizationData.Load(ns);var circles=LegacyNativeCircledPayloadData.Load(cis);
InkStroke M()=>new(new InkPoint[]{new(0,90),new(0,10),new(25,60),new(50,10),new(50,90)});
var samples=new List<(string Name,InkStroke[] Ink)>{("M",[M()]),("MIR",[M(),new(new InkPoint[]{new(70,10),new(70,90),new(120,10),new(120,90)}),new(new InkPoint[]{new(140,90),new(140,10),new(180,10),new(190,20),new(190,40),new(180,50),new(140,50)})]),("I",[new(new InkPoint[]{new(0,10),new(0,90)})]),("II",[new(new InkPoint[]{new(0,10),new(0,90)}),new(new InkPoint[]{new(40,10),new(40,90)})])};
var recorded=RecordedInputs.Load(args[1]);for(int i=0;i<recorded.Samples.Length;i++)samples.Add((recorded.Names[i],recorded.Samples[i]));


using var file=File.Create(args[2]);using var w=new Utf8JsonWriter(file);w.WriteStartObject();w.WriteStartArray("samples");
int guards=0,views=0,points=0;
foreach(var(name,raw)in samples)
{
 using var scope=Scope(raw);var parent=Parent(scope);Reject(()=>LegacyGlobalOrdinaryCroppedSource.Prepare(parent));parent.Publish(new byte[40],new byte[8160]);
 using var stop=new CancellationTokenSource();stop.Cancel();Reject(()=>LegacyGlobalOrdinaryCroppedSource.Prepare(parent,stop.Token));
 var pending=parent.CopyPendingSelections().ToArray();byte[] lines=parent.CopySourceLines56();var result=LegacyGlobalOrdinaryCroppedSource.Prepare(parent);
 w.WriteStartObject();w.WriteString("name",name);w.WriteStartArray("raw");foreach(var stroke in scope.Document.Strokes){w.WriteStartArray();foreach(var point in stroke.Points){w.WriteStartArray();w.WriteNumberValue(BitConverter.SingleToInt32Bits(point.X));w.WriteNumberValue(BitConverter.SingleToInt32Bits(point.Y));w.WriteEndArray();}w.WriteEndArray();}w.WriteEndArray();
 w.WriteStartArray("views");foreach(var view in result.Views)
 {
  views++;points+=view.PointCount;w.WriteStartObject();w.WriteBoolean("word",view.IsWord);w.WriteNumber("node",view.SourceNodeIdentity);w.WriteNumber("range",view.RangeOrdinal);w.WriteNumber("object",view.RawObjectIndex);w.WriteNumber("kind",(int)view.Kind);w.WriteNumber("first",view.FirstOriginalPoint);w.WriteNumber("last",view.LastOriginalPoint);w.WriteNumber("fraction0",BitConverter.SingleToInt32Bits(view.FirstFraction));w.WriteNumber("fraction1",BitConverter.SingleToInt32Bits(view.LastFraction));w.WriteBoolean("include0",view.IncludesFirst);w.WriteBoolean("include1",view.IncludesLast);
  var values=view.CopyPoints();w.WriteStartArray("points");foreach(var point in values){w.WriteStartArray();w.WriteNumberValue(BitConverter.SingleToInt32Bits(point.X));w.WriteNumberValue(BitConverter.SingleToInt32Bits(point.Y));w.WriteEndArray();}w.WriteEndArray();
  var chunks=new List<InkPoint>();for(int offset=0;offset<view.PointCount;offset+=20)chunks.AddRange(view.ReadChunk(offset,Math.Min(20,view.PointCount-offset)));
  if(!values.SequenceEqual(chunks))throw new Exception("Chunk20 point identity lost.");
  Reject(()=>view.ReadChunk(-1,1));Reject(()=>view.ReadChunk(0,21));Reject(()=>view.ReadChunk(view.PointCount,1));
  values.AsSpan().Fill(new(float.NaN,float.NaN));if(view.CopyPoints().Any(p=>float.IsNaN(p.X)))throw new Exception("Actual crop aliases diagnostics.");
  w.WriteEndObject();
 }w.WriteEndArray();
 if(!parent.CopyPendingSelections().SequenceEqual(pending)||!parent.CopySourceLines56().SequenceEqual(lines))throw new Exception("Coordinate view preparation mutated its source transaction.");
 scope.Dispose();Reject(()=>result.Validate());Reject(()=>result.Views[0].CopyPoints());w.WriteEndObject();
}
w.WriteEndArray();
{
 using var scope=Scope([M()]);var parent=Parent(scope);parent.Publish(new byte[40],new byte[8160]);var result=LegacyGlobalOrdinaryCroppedSource.Prepare(parent);
 var pending=parent.CopyPendingSelections().ToArray();long next=scope.SharedState.Selections.NextSelectionId,output=-1;
 if(scope.SharedState.Selections.CreatePrepared(parent.Text.CopyRanges(),0,ref output,1)is not null||!scope.SharedState.Selections.Failed||scope.SharedState.Selections.NextSelectionId!=next+1||!parent.CopyPendingSelections().SequenceEqual(pending))throw new Exception("Prepared poisoned counter prefix changed.");
 Reject(()=>result.Validate());Reject(()=>result.Views[0].ReadChunk(0,1));Reject(()=>LegacyGlobalOrdinaryCroppedSource.Prepare(parent));
}
w.WriteBoolean("poisonedCounterWithoutCommand",true);w.WriteNumber("guards",guards);w.WriteNumber("views",views);w.WriteNumber("points",points);w.WriteEndObject();w.Flush();Console.WriteLine($"PASS13 global originals/crops: {views} views/{points} points/{guards} guards; no2DB/238B0.");
void Reject(Action call){try{call();}catch(Exception e)when(e is ArgumentException or InvalidOperationException or NotSupportedException or OperationCanceledException){guards++;return;}throw new Exception("Expected refusal.");}
LegacyLibraryGlobalTextPublicationScope Scope(InkStroke[] ink){var d=LegacyOwnedRawInkDocument.Create(ink,default);var original=LegacyLibraryOriginalTextFieldBootstrap.Prepare(LegacyFreshRawInkCurrentView.Prepare(d),["a","b"]);original.Run();var selected=LegacyLibraryOriginalSelectedFieldContext.Prepare(original,original.State.Inputs[1]);selected.Run();return LegacyLibraryGlobalTextPublicationScope.PrepareOriginalSelected(models,selected,0,table,cd,pd);}
LegacyFreshMainGlobalWordTextPublication Parent(LegacyLibraryGlobalTextPublicationScope scope){scope.Run(new byte[4096*40],new byte[120],new byte[40],new ushort[256],new byte[64],new byte[72]);var c=scope.CompleteSources(new byte[400],new byte[4],new byte[4]);return LegacyFreshMainGlobalWordTextPublication.Prepare(LegacyFreshMainGlobalTextSourceWrapper.Prepare(scope,c,ak,pd,norm,circles));}
