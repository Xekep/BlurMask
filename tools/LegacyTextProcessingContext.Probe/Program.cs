using InkRecognition;
using System.Text.Json;

if(args.Length!=11)throw new ArgumentException("AK LK engine separation case publication normalization circles line30 line50 capture");
byte[] ak=File.ReadAllBytes(args[0]),lk=File.ReadAllBytes(args[1]),engine=File.ReadAllBytes(args[2]);
var table=LegacyCursiveSeparationTable.Load(File.ReadAllBytes(args[3]));
using var cf=File.OpenRead(args[4]);using var pf=File.OpenRead(args[5]);
var cases=LegacyNativeCaseData.Load(cf);var publication=LegacyNativeTextPublicationData.Load(pf);
byte[] line30=File.ReadAllBytes(args[8]),line50=File.ReadAllBytes(args[9]);
using var file=File.Create(args[10]);using var w=new Utf8JsonWriter(file);int guards=0,successes=0,failures=0;
w.WriteStartObject();w.WriteStartArray("actual");
InkStroke[] m=[new(new InkPoint[]{new(0,90),new(0,90),new(0,10),new(25,60),new(50,10),new(50,90)})];
InkStroke[][] samples=[m,Word(0).Concat(Word(300)).ToArray(),Word(450).Concat(Word(0)).ToArray(),Rotate(m,.18f)];
foreach(var raw in samples)
{
 var residue=new LegacyMainConfiguredAlternativeResidue(new byte[120],new byte[72],new byte[40],new ushort[256],new byte[64],new byte[4],new byte[4],new byte[4],new byte[4]);
 var inference=LegacyFreshMainConfiguredSingleLinePlainRecognition.Prepare(ak,lk,engine,line30,line50,raw,0,table,cases,publication)
     .Run(residue,r=>new byte[r.RequiredBytes],new byte[20000*40]);
 Test(inference.SourceDocument,true);
}
w.WriteEndArray();w.WriteStartArray("library");
for(int count=1;count<=128;count++)
 Test(LegacyOwnedRawInkDocument.Create(Enumerable.Range(0,count).Select(i=>new InkStroke(Enumerable.Range(0,2+(i+count)%5).Select(j=>new InkPoint(i*12+j*.25f,j*1.5f+count)).ToArray())).ToArray(),default),false);
w.WriteEndArray();w.WriteNumber("guards",guards);w.WriteNumber("successes",successes);w.WriteNumber("failures",failures);w.WriteEndObject();w.Flush();
Console.WriteLine($"PASS4 recognition-owned +128 library input DOM/context imports/{successes} prepared/{failures} failure prefixes/{guards} guards; no SDK acceptance.");

void Test(LegacyOwnedRawInkDocument document,bool faults)
{
 var original=LegacyFreshTextFieldInputContext.Prepare(LegacyFreshRawInkCurrentView.Prepare(document));var selected=original.CreateSelectedCopy();
 Reject(()=>LegacyLibraryTextProcessingContext.CreateFresh(original));Reject(()=>LegacyLibraryTextProcessingContext.CreateFresh(selected,new(true)));
 w.WriteStartObject();w.WriteStartArray("pointCounts");foreach(var stroke in document.Strokes)w.WriteNumberValue(stroke.Points.Count);w.WriteEndArray();
 w.WriteBase64String("sourceRanges",selected.Input.CopyRangeStorage());w.WriteStartArray("attempts");
 Attempt("success",-1,-1,-1);
 if(faults)
 {
  for(int i=0;i<4;i++)Attempt("dom"+i,i,-1,-1);
  for(int i=0;i<2;i++)Attempt("prepare"+i,-1,i,-1);
  for(int i=0;i<4;i++)Attempt("cancel"+i,-1,-1,i);
 }
 w.WriteEndArray();w.WriteEndObject();

 void Attempt(string stage,int domFailure,int prepFailure,int cancel)
 {
  using var context=LegacyLibraryTextProcessingContext.CreateFresh(selected);
  if(context.State18!=0||context.Result.SourceTag30!=-1||context.Result.RangeCount210!=0||context.Result.TextCountD8!=0||context.Strings.Count!=0||
      context.Result.BoundRequest is not null||context.Result.BoundInput28 is not null||context.Result.CopyIdentifier38().Any(x=>x!=0))throw new InvalidDataException("Fresh native constructor fields changed.");
  Reject(()=>context.PrepareInput(new(true)));
  LegacyLibraryTextInputRequest? request=null;bool success=false;
  try{request=context.PrepareInputPrepared(domFailure,prepFailure,cancel);success=true;successes++;}
  catch(InvalidOperationException){failures++;}catch(OperationCanceledException){failures++;}
  request??=context.PreparedRequest??throw new InvalidDataException("Entered request prefix lost.");
  LegacyLibraryTextDomNode? retainedSnapshot=null;
  bool shouldSucceed=stage=="success";if(success!=shouldSucceed||context.IsFaulted==success||request.Succeeded!=success)throw new InvalidDataException("Preparation status changed.");
  if(success)
  {
   if(context.State18!=1||!ReferenceEquals(context.Result.BoundRequest,request)||!ReferenceEquals(context.Result.BoundInput28,selected)||
       context.Result.SourceTag30!=selected.Input.RegistryId||!context.Result.CopyInputRangeStorage().SequenceEqual(selected.Input.CopyRangeStorage())||
       !ReferenceEquals(request.Arena.Document,document)||!ReferenceEquals(request.Input,selected.Input)||request.FieldKind!=0||request.FieldNamespace!=selected.ElementNamespace||context.Strings.Count!=0)
       throw new InvalidDataException("Result/request/C20/C22/BEE ownership mismatch.");
   Reject(()=>_=request.Arena.Seed);Reject(()=>_=request.Arena.NamePool.Seed);
   var copy=context.Result.CopyInputRangeStorage();copy[0]^=255;if(copy.SequenceEqual(context.Result.CopyInputRangeStorage()))throw new InvalidDataException("Mutable range snapshot.");
   using var foreign=LegacyLibraryTextProcessingContext.CreateFresh(selected);var other=foreign.PrepareInput();
   if(ReferenceEquals(context.Result,foreign.Result)||ReferenceEquals(context.Strings,foreign.Strings)||ReferenceEquals(request.Arena.NamePool,other.Arena.NamePool))throw new InvalidDataException("Reused constructor identity.");
   Reject(()=>context.Result.BindRequest(other));
   retainedSnapshot=request.Field;var value=retainedSnapshot.Children[0].Attributes[0].Value.CopyBytes();value[0]=77;
   if(request.Field.Children[0].Attributes[0].Value.CopyBytes()[0]!=0)throw new InvalidDataException("Mutable literal attribute snapshot.");
  }
  Reject(()=>context.PrepareInput());
  w.WriteStartObject();w.WriteString("stage",stage);w.WriteBoolean("succeeded",success);w.WriteBoolean("faulted",context.IsFaulted);
  w.WriteNumber("sourceTag30",context.Result.SourceTag30);w.WriteNumber("count210",context.Result.RangeCount210);w.WriteNumber("textD8",context.Result.TextCountD8);
  w.WriteNumber("strings3E",context.Strings.Count);w.WriteBase64String("uuid38",context.Result.CopyIdentifier38());w.WriteBase64String("ranges208",context.Result.CopyInputRangeStorage());
  w.WriteNumber("state18",context.PreparedState18);
  w.WriteBoolean("boundRequest",ReferenceEquals(context.Result.BoundRequest,request));w.WriteBoolean("boundInput",ReferenceEquals(context.Result.BoundInput28,selected));
  w.WriteStartArray("names");foreach(var name in request.Arena.NamePool.CopyNames())w.WriteStringValue(name);w.WriteEndArray();
  w.WriteStartArray("preparationTrace");foreach(var item in request.CopyPreparationTrace())w.WriteStringValue(item);w.WriteEndArray();
  w.WriteStartArray("domTrace");foreach(var item in request.Arena.Trace)w.WriteStringValue(item);w.WriteEndArray();
  w.WriteStartArray("nodes");foreach(var node in request.Arena.CopyStates())
  {w.WriteStartObject();w.WriteNumber("id",node.Identity);w.WriteNumber("kind",node.Kind);w.WriteString("name",node.Name);w.WriteNumber("nameId",node.NameIdentity);w.WriteNumber("tag",node.Tag);w.WriteBase64String("value",node.Value);w.WriteNumber("parent",node.Parent);w.WriteBoolean("released",node.Released);Ints("attributes",node.Attributes);Ints("children",node.Children);w.WriteEndObject();}w.WriteEndArray();
  context.Dispose();context.Dispose();Reject(()=>context.PrepareInput());Reject(()=>_=request.Field);
  if(retainedSnapshot is not null&&retainedSnapshot.Children[0].Attributes[0].Value.CopyBytes()[0]!=0)throw new InvalidDataException("Disposed live arena mutated an immutable tree snapshot.");
  if(context.Result.BoundRequest is not null||context.Result.BoundInput28 is not null||!ReferenceEquals(selected.Document,document)||selected.Input.RegistryId!=0)throw new InvalidDataException("Release damaged source or retained imported owner.");
  w.WriteBoolean("releasedBindings",true);w.WriteEndObject();
 }
}
void Ints(string name,int[] values){w.WriteStartArray(name);foreach(int v in values)w.WriteNumberValue(v);w.WriteEndArray();}
void Reject(Action action){try{action();}catch(ArgumentException){guards++;return;}catch(InvalidOperationException){guards++;return;}catch(NotSupportedException){guards++;return;}catch(OperationCanceledException){guards++;return;}throw new InvalidDataException("Expected bounded owner/lifetime refusal.");}
static InkStroke[] Word(float x)=>[new(new InkPoint[]{new(x,90),new(x,10),new(x+25,60),new(x+50,10),new(x+50,90)})];
static InkStroke[] Rotate(InkStroke[] raw,float angle){float c=MathF.Cos(angle),s=MathF.Sin(angle);return raw.Select(r=>new InkStroke(r.Points.Select(p=>new InkPoint(c*p.X-s*p.Y,s*p.X+c*p.Y)).ToArray())).ToArray();}
