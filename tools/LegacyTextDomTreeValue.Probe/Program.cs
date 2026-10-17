using InkRecognition;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;

if(args.Length!=11)throw new ArgumentException("AK LK engine separation case publication normalization circles line30 line50 capture");
byte[] ak=File.ReadAllBytes(args[0]),lk=File.ReadAllBytes(args[1]),engine=File.ReadAllBytes(args[2]);
var table=LegacyCursiveSeparationTable.Load(File.ReadAllBytes(args[3]));
using var cf=File.OpenRead(args[4]);using var pf=File.OpenRead(args[5]);using var nf=File.OpenRead(args[6]);using var cr=File.OpenRead(args[7]);
var cases=LegacyNativeCaseData.Load(cf);var publication=LegacyNativeTextPublicationData.Load(pf);
var norm=LegacyNativeUnicodeNormalizationData.Load(nf);var circles=LegacyNativeCircledPayloadData.Load(cr);
var models=LegacyFreshMainRecognitionContext.Load(ak,lk,engine,File.ReadAllBytes(args[8]),File.ReadAllBytes(args[9]));
var inputs=new Inputs(models,ak,table,cases,publication,norm,circles);
var positive=Witnesses.Create(inputs,false);
Collect();
if(positive.Scope.TryGetTarget(out _))throw new InvalidDataException("Owner-free tree retained its recognition scope.");
var negative=Witnesses.Create(inputs,true);
Collect();
if(!negative.Scope.TryGetTarget(out _))throw new InvalidDataException("Control tree unexpectedly stopped retaining Arena/Seed/Scope.");
int guards=0;
void Reject(Action action){try{action();}catch(Exception e)when(e is ArgumentException or NotSupportedException or OperationCanceledException){guards++;return;}throw new InvalidDataException("Missing value-copy guard.");}
var source=negative.Original!;
Reject(()=>LegacyLibraryTextDomTreeValue.CopyFrom(null!));
Reject(()=>LegacyLibraryTextDomTreeValue.CopyFrom(source,maximumNodes:0));
Reject(()=>LegacyLibraryTextDomTreeValue.CopyFrom(source,maximumNodes:LegacyLibraryTextDomTreeValue.MaximumNodes+1));
Reject(()=>LegacyLibraryTextDomTreeValue.CopyFrom(source,maximumValueBytes:-1));
Reject(()=>LegacyLibraryTextDomTreeValue.CopyFrom(source,maximumValueBytes:LegacyLibraryTextDomTreeValue.MaximumValueBytes+1));
Reject(()=>LegacyLibraryTextDomTreeValue.CopyFrom(source,maximumDepth:0));
Reject(()=>LegacyLibraryTextDomTreeValue.CopyFrom(source,maximumDepth:LegacyLibraryTextDomTreeValue.MaximumDepth+1));
Reject(()=>LegacyLibraryTextDomTreeValue.CopyFrom(source,maximumNodes:1));
Reject(()=>LegacyLibraryTextDomTreeValue.CopyFrom(source,maximumValueBytes:0));
Reject(()=>LegacyLibraryTextDomTreeValue.CopyFrom(source,maximumDepth:1));
Reject(()=>LegacyLibraryTextDomTreeValue.CopyFrom(source,token:new(true)));
var another=LegacyLibraryTextDomTreeValue.CopyFrom(source);
Witnesses.CheckCopies(another);
using var file=File.Create(args[10]);using var w=new Utf8JsonWriter(file);
w.WriteStartObject();w.WriteBoolean("valueScopeCollected",true);w.WriteBoolean("originalRetainsScope",true);w.WriteNumber("guards",guards);
w.WriteStartArray("rows");foreach(var witness in new[]{positive,negative})
{w.WriteStartObject();w.WritePropertyName("original");w.WriteRawValue(witness.SourceJson);w.WritePropertyName("value");Witnesses.Write(w,witness.Value);w.WriteEndObject();}
w.WriteEndArray();w.WriteEndObject();w.Flush();
Witnesses.Close(negative.Scope);GC.KeepAlive(positive.Value);GC.KeepAlive(negative.Original);
Console.WriteLine($"PASS2 actual published trees copied without Arena/Seed/Scope; positive collection and original-tree retention control;{guards} guards.");
static void Collect(){GC.Collect();GC.WaitForPendingFinalizers();GC.Collect();GC.WaitForPendingFinalizers();}
record Inputs(LegacyFreshMainRecognitionContext Models,byte[] Ak,LegacyCursiveSeparationTable Table,
    LegacyNativeCaseData Cases,LegacyNativeTextPublicationData Publication,LegacyNativeUnicodeNormalizationData Normalization,
    LegacyNativeCircledPayloadData Circles);
record Witness(LegacyLibraryTextDomTreeValue Value,LegacyLibraryTextDomNode? Original,
    WeakReference<LegacyLibraryTextPublicationScope> Scope,string SourceJson);
static class Witnesses
{
 [MethodImpl(MethodImplOptions.NoInlining)]
 internal static Witness Create(Inputs i,bool keepOriginal)
 {
  InkStroke[] raw=[new(new InkPoint[]{new(0,90),new(0,90),new(0,10),new(25,60),new(50,10),new(50,90)})];
  var scope=LegacyLibraryTextPublicationScope.PrepareConfigured(i.Models,raw,0,i.Table,i.Cases,i.Publication);
  try
  {
   var residue=new LegacyMainConfiguredAlternativeResidue(new byte[120],new byte[72],new byte[40],new ushort[256],new byte[64],new byte[4],new byte[4],new byte[4],new byte[4]);
   var result=scope.Run(residue,r=>new byte[r.RequiredBytes],new byte[20000*40]);
   var completion=LegacyFreshMainConfiguredNoCandidateSourceCompletion.Prepare(i.Ak,result,i.Normalization,i.Circles,new byte[4],new byte[4]);completion.Run();
   var words=scope.PrepareWords(completion,i.Normalization,i.Circles).Publish(new byte[40],new byte[2040*4]);
   using var owner=LegacyLibraryTextPublicationDocument.Prepare(scope,words);owner.BuildAndTransfer();
   var source=owner.ApplyToLibraryTarget().TargetRoot!;
   var value=LegacyLibraryTextDomTreeValue.CopyFrom(source);CheckCopies(value);
   using var stream=new MemoryStream();using(var writer=new Utf8JsonWriter(stream))Write(writer,source);
   return new(value,keepOriginal?source:null,new(scope),Encoding.UTF8.GetString(stream.ToArray()));
  }
  catch{scope.Dispose();throw;}
 }
 [MethodImpl(MethodImplOptions.NoInlining)]
 internal static void Close(WeakReference<LegacyLibraryTextPublicationScope> weak){if(weak.TryGetTarget(out var scope))scope.Dispose();}
 internal static void CheckCopies(LegacyLibraryTextDomTreeValue node)
 {
  byte[] before=node.CopyValueBytes(),mutated=node.CopyValueBytes();Array.Fill(mutated,(byte)99);
  if(!before.SequenceEqual(node.CopyValueBytes()))throw new InvalidDataException("Mutable DOM value bytes.");
  foreach(var child in node.Attributes.Concat(node.Children))CheckCopies(child);
 }
 internal static void Write(Utf8JsonWriter w,LegacyLibraryTextDomTreeValue n)
 {w.WriteStartObject();w.WriteNumber("id",n.Identity);w.WriteNumber("kind",n.Kind);w.WriteString("name",n.Name);w.WriteNumber("nameId",n.NameIdentity);w.WriteNumber("tag",n.NativeTag);w.WriteBase64String("value",n.CopyValueBytes());w.WriteStartArray("attributes");foreach(var a in n.Attributes)Write(w,a);w.WriteEndArray();w.WriteStartArray("children");foreach(var c in n.Children)Write(w,c);w.WriteEndArray();w.WriteEndObject();}
 internal static void Write(Utf8JsonWriter w,LegacyLibraryTextDomNode n)
 {w.WriteStartObject();w.WriteNumber("id",n.Identity);w.WriteNumber("kind",n.Kind);w.WriteString("name",n.Name);w.WriteNumber("nameId",n.NameIdentity);w.WriteNumber("tag",n.Value.NativeTag);w.WriteBase64String("value",n.Value.CopyBytes());w.WriteStartArray("attributes");foreach(var a in n.Attributes)Write(w,a);w.WriteEndArray();w.WriteStartArray("children");foreach(var c in n.Children)Write(w,c);w.WriteEndArray();w.WriteEndObject();}
}
