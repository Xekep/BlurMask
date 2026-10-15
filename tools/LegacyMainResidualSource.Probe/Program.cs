using InkRecognition;
using System.Text.Json;
if(args.Length!=7)throw new ArgumentException("AK LK engine separation case publication capture.json");
byte[] ak=File.ReadAllBytes(args[0]),lk=File.ReadAllBytes(args[1]),engine=File.ReadAllBytes(args[2]),separation=File.ReadAllBytes(args[3]);
var fixture=MainFixture.Create(ak,lk,engine,separation);
var span=new LegacyOwnedMainWordSpanPreparation(ak,lk,engine,fixture.Sampled,fixture.Table);
var policy=LegacyOwnedFreshMainWordControllerPolicy.Prepare(ak,span);
var confidence=LegacyOwnedRussianMainGlyphConfidence.Prepare(ak,span.DictionaryOwner);
var geometry=LegacyOwnedRussianMainSourceGeometry.Prepare(ak,confidence,fixture.Sampled);
var overlay=LegacyOwnedRussianMainResidualSource.Prepare(geometry,policy);
using var caseFile=File.OpenRead(args[4]);using var publicationFile=File.OpenRead(args[5]);
var caseData=LegacyNativeCaseData.Load(caseFile);var publication=LegacyNativeTextPublicationData.Load(publicationFile);
var caseDictionary=LegacyFreshCaseRussianDictionaryPreparation.Prepare(ak,lk,span.CommonModel,fixture.Sampled,caseData,publication);
var caseSpan=new LegacyOwnedMainWordSpanPreparation(ak,caseDictionary,fixture.Sampled,fixture.Table);
var caseController=LegacyOwnedFreshMainWordControllerPolicy.Prepare(ak,caseSpan);
var casePolicy=LegacyFreshCaseMainSourcePolicy.Prepare(ak,lk,caseController);
var caseConfidence=LegacyOwnedRussianMainGlyphConfidence.Prepare(ak,caseDictionary);
var caseGeometry=LegacyOwnedRussianMainSourceGeometry.Prepare(ak,caseConfidence,fixture.Sampled);
var caseOverlay=LegacyOwnedRussianMainResidualSource.Prepare(caseGeometry,casePolicy);
var callbacks=LegacyOwnedRussianMainSourceSymbolCallbacks.Prepare(ak,confidence,fixture.Sampled);
ulong identity=1;
LegacyMainDirectStorageAllocation Allocate(LegacyMainDirectStorageRequest request)
{var bytes=new byte[request.RequiredBytes];request.PreviousStorage.Span.CopyTo(bytes);return new(bytes,identity++);}
var pass=new LegacyOwnedMainDirectWordPass(ak,policy,fixture.Table,Allocate).RunOwned();
var terminal=LegacyOwnedFreshMainTerminalInvocation.Prepare(policy).SelectOwnedPass(pass);
var compact=LegacyOwnedMainCompactWordMaterialization.Materialize(ak,terminal,confidence,Enumerable.Repeat((byte)0xa7,4096*40).ToArray());
var backtrack=LegacyOwnedMainGlyphBacktrackRows.Prepare(ak,compact,callbacks,new byte[72]);
var actual=overlay.Apply(backtrack);int guards=0,merges=0,higher=0;
if(!actual.Succeeded||actual.RemainingCount!=0||!ReferenceEquals(actual.Owner,overlay)||!ReferenceEquals(actual.BacktrackRows,backtrack))throw new Exception("Owning217DB0 bridge failed.");
using(var stream=File.Create(args[6]))using(var w=new Utf8JsonWriter(stream))
{
    w.WriteStartObject();w.WriteStartArray("preparedRangeControls");
    for(int n=0;n<512;n++)
    {
        var input=Enumerable.Range(0,2+n%31).Select(i=>new LegacyInkRange((n+i*17)%4,(n*11+i*7)%13,(n*11+i*7)%13+1+(n+i)%7)).ToArray();
        if(n==0)input=[new(0,0,1),new(0,0,2)];
        if(n==1)input=[new(0,0,3),new(0,2,6),new(0,5,8)];
        if(n==2)input=[new(0,0,5),new(0,2,9)];
        var output=LegacyOwnedRussianMainResidualSource.SortAndCoalesce(input.ToArray());
        if(n==0&&!(output.Length==2&&output[0].EndPointExclusive==2))throw new Exception("Native equal-key swap replaced by stable sort.");
        if(n==1&&!(output.Length==1&&output[0].EndPointExclusive==8))throw new Exception("Shared endpoint cascade lost.");
        if(n==2&&output.Length!=2)throw new Exception("Arbitrary overlap was incorrectly united.");
        w.WriteStartObject();WriteRanges("input",input);WriteRanges("output",output);w.WriteEndObject();
    }
    w.WriteEndArray();w.WriteStartArray("graph");
    for(int node=0;node<overlay.Graph.NodeCount;node++)
    {w.WriteStartArray();foreach(var a in overlay.Graph.GetNode(node)){w.WriteStartArray();w.WriteNumberValue(a.GroupId);w.WriteNumberValue(a.StartContextId);w.WriteNumberValue(a.EndContextId);w.WriteEndArray();}w.WriteEndArray();}
    w.WriteEndArray();w.WriteStartArray("groups");foreach(var g in overlay.Graph.Groups){w.WriteStartArray();foreach(var r in g.Ranges.Span)Range(r);w.WriteEndArray();}w.WriteEndArray();
    var refs=Enumerable.Range(0,overlay.Graph.NodeCount).SelectMany(n=>Enumerable.Range(0,overlay.Graph.GetNode(n).Count).Select(a=>Pack(n,a))).ToArray();
    w.WriteStartArray("merges");ulong firstHigher=ulong.MaxValue;
    for(int n=0;n<384;n++)
    {
        ulong[] input=Enumerable.Range(0,2+n%5).Select(i=>refs[(n*17+i*11)%refs.Length]).ToArray();
        var result=overlay.MergeSources(input);var ranges=overlay.CopySourceRanges(result.Reference);var decoded=LegacyNativeResidualSource.Decode(result.Reference);
        var caseResult=caseOverlay.MergeSources(input);
        if(caseResult.Reference!=result.Reference||!caseOverlay.CopySourceRanges(caseResult.Reference).AsSpan().SequenceEqual(ranges))throw new Exception("CASE source overlay changed the same native geometry algorithm.");
        bool isHigher=decoded.Arc>=overlay.Graph.GetNode(decoded.Node).Count;
        if(isHigher){higher++;if(firstHigher==ulong.MaxValue)firstHigher=result.Reference;}
        if(!result.Succeeded||overlay.MergeSources(input).Reference!=result.Reference)throw new Exception("Repeat merge/cache changed reference.");
        ulong original=overlay.ResolveBaseReference(result.Reference);
        var baseSource=LegacyNativeResidualSource.Decode(original);
        if(overlay.ReadBounds(result.Reference)!=geometry.ReadBounds(baseSource.Node,baseSource.Arc))throw new Exception("Merged ranges replaced original neural-group bbox.");
        if(overlay.MergeSources(new[]{result.Reference|0xab00000000000000UL}).Reference!=(result.Reference|0xab00000000000000UL))throw new Exception("Count1 did not preserve all reference bits.");
        var copy=overlay.CopySourceRanges(result.Reference);copy[0]=new(255,0,1);if(overlay.CopySourceRanges(result.Reference)[0]==copy[0])throw new Exception("Range snapshot escaped.");
        w.WriteStartObject();w.WriteStartArray("input");foreach(ulong key in input)w.WriteNumberValue(key);w.WriteEndArray();w.WriteNumber("reference",result.Reference);w.WriteNumber("original",original);w.WriteBoolean("higher",isHigher);
        if(isHigher)w.WriteNumber("capacity",overlay.ReadRangeCapacity(result.Reference));else w.WriteNull("capacity");WriteRanges("ranges",ranges);w.WriteEndObject();merges++;
    }
    w.WriteEndArray();w.WriteBase64String("beforeRows",backtrack.CopyRows());w.WriteBase64String("afterRows",actual.CopyRows72());
    w.WriteStartArray("actualResiduals");foreach(ulong r in backtrack.CopyResidualSourceReferences())w.WriteNumberValue(r);w.WriteEndArray();
    w.WriteNumber("actualRemaining",actual.RemainingCount);w.WriteNumber("actualRows",backtrack.Count);
    if(firstHigher==ulong.MaxValue)throw new Exception("Real graph controls did not exercise any new higher range owner.");
    Reject(()=>overlay.MergeSources(new[]{firstHigher,refs[0]}));Reject(()=>overlay.MergeSources([]));Reject(()=>overlay.MergeSources(new[]{ulong.MaxValue}));
    Reject(()=>overlay.MergeSources(new[]{refs[0]},new CancellationToken(true)));Reject(()=>overlay.CopySourceRanges(ulong.MaxValue));Reject(()=>overlay.ReadRangeCapacity(refs[0]));
    Reject(()=>overlay.Apply(backtrack,new CancellationToken(true)));Reject(()=>overlay.Apply(null!));
    var otherSpan=new LegacyOwnedMainWordSpanPreparation(ak,lk,engine,fixture.Sampled,fixture.Table);
    var otherPolicy=LegacyOwnedFreshMainWordControllerPolicy.Prepare(ak,otherSpan);
    Reject(()=>LegacyOwnedRussianMainResidualSource.Prepare(geometry,otherPolicy));
    Reject(()=>LegacyOwnedRussianMainResidualSource.Prepare(geometry,policy,new CancellationToken(true)));
    Reject(()=>LegacyOwnedRussianMainResidualSource.Prepare(caseGeometry,caseController));
    Reject(()=>LegacyOwnedRussianMainResidualSource.Prepare(geometry,casePolicy));
    Reject(()=>caseOverlay.Apply(backtrack));
    var changed=actual.CopyRows72();if(changed.Length!=0){changed[0]^=1;if(actual.CopyRows72()[0]==changed[0])throw new Exception("Association row snapshot escaped.");}
    w.WriteNumber("mergeCount",merges);w.WriteNumber("caseMerges",merges);w.WriteNumber("higherResults",higher);w.WriteNumber("guards",guards);w.WriteEndObject();
    void WriteRanges(string name,IEnumerable<LegacyInkRange> ranges){w.WriteStartArray(name);foreach(var r in ranges)Range(r);w.WriteEndArray();}
    void Range(LegacyInkRange r){w.WriteStartArray();w.WriteNumberValue(r.StrokeIndex);w.WriteNumberValue(r.StartPoint);w.WriteNumberValue(r.EndPointExclusive);w.WriteEndArray();}
}
Console.WriteLine($"PASS512 explicit range-controls, {merges} merges over actualgraph/{higher} higher results, actual compact-winner217DB0 {backtrack.Count} rows/{backtrack.CopyResidualSourceReferences().Length} residuals;{guards} guards. No synthetic residuals inserted into actual trace.");
static ulong Pack(int node,int arc)=>(uint)node&0xffffffUL|((ulong)(ushort)arc<<24);
void Reject(Action action){try{action();}catch(Exception e)when(e is ArgumentException or NotSupportedException or OperationCanceledException){guards++;return;}throw new Exception("Invalid owning residual input accepted");}
