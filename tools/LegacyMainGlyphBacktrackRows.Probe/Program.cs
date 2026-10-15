using System.Text.Json;
using InkRecognition;

if(args.Length!=5)throw new ArgumentException("AK LK engine separation capture.json");
byte[] ak=File.ReadAllBytes(args[0]),lk=File.ReadAllBytes(args[1]),engine=File.ReadAllBytes(args[2]),separation=File.ReadAllBytes(args[3]);
var fixture=MainFixture.Create(ak,lk,engine,separation);
var owner=new LegacyOwnedMainWordSpanPreparation(ak,lk,engine,fixture.Sampled,fixture.Table);
var policy=LegacyOwnedFreshMainWordControllerPolicy.Prepare(ak,owner);
var confidence=LegacyOwnedRussianMainGlyphConfidence.Prepare(ak,owner.DictionaryOwner);
var callbacks=LegacyOwnedRussianMainSourceSymbolCallbacks.Prepare(confidence);
ulong identity=1;
LegacyMainDirectStorageAllocation Allocate(LegacyMainDirectStorageRequest request)
{var bytes=new byte[request.RequiredBytes];request.PreviousStorage.Span.CopyTo(bytes);return new(bytes,identity++);}
var pass=new LegacyOwnedMainDirectWordPass(ak,policy,fixture.Table,Allocate).RunOwned();
var terminal=LegacyOwnedFreshMainTerminalInvocation.Prepare(policy).SelectOwnedPass(pass);
var compact=LegacyOwnedMainCompactWordMaterialization.Materialize(ak,terminal,confidence,Enumerable.Repeat((byte)0xa7,4096*40).ToArray());
int guards=0,rows=0;
using(var output=File.Create(args[4]))using(var w=new Utf8JsonWriter(output))
{
    w.WriteStartObject();w.WriteNumber("recordCount",compact.RecordCount);w.WriteNumber("selected",compact.SelectedRecordIndex);
    w.WriteNumber("alternative",compact.SelectedAlternative);w.WriteBase64String("compact",compact.CopyCompactStorage());
    w.WriteStartArray("starts");foreach(int start in compact.CopyNodeStarts2478())w.WriteNumberValue(start);w.WriteEndArray();
    w.WriteStartArray("graph");for(int node=0;node<owner.SourceGraph.NodeCount;node++)
    {w.WriteStartArray();foreach(var arc in owner.SourceGraph.GetNode(node)){w.WriteStartArray();w.WriteNumberValue(arc.GroupId);w.WriteNumberValue(arc.Span);w.WriteNumberValue(arc.StartContextId);w.WriteNumberValue(arc.EndContextId);w.WriteEndArray();}w.WriteEndArray();}w.WriteEndArray();
    w.WriteStartArray("controls");
    for(int n=0;n<128;n++)
    {
        byte[] residue=Enumerable.Range(0,72).Select(i=>(byte)(n*73+i*31+11)).ToArray();
        var result=LegacyOwnedMainGlyphBacktrackRows.Prepare(ak,compact,callbacks,residue);byte[] emitted=result.CopyRows();
        Require(result.Count>0&&emitted.Length==result.Count*72,"No real compact path produced rows.");rows+=result.Count;
        w.WriteStartObject();w.WriteBase64String("residue",residue);w.WriteBase64String("rows",emitted);
        w.WriteStartArray("visited");foreach(int index in result.CopyVisitedCompactIndices())w.WriteNumberValue(index);w.WriteEndArray();
        w.WriteStartArray("residuals");foreach(ulong key in result.CopyResidualSourceReferences())w.WriteNumberValue(key);w.WriteEndArray();w.WriteEndObject();
        Array.Fill(emitted,(byte)0x55);Require(!result.CopyRows().AsSpan().SequenceEqual(emitted),"Public rows escaped ownership.");
        var visited=result.CopyVisitedCompactIndices();visited[0]=-999;Require(result.CopyVisitedCompactIndices()[0]!=-999,"Path snapshot escaped ownership.");
    }
    w.WriteEndArray();
    Reject(()=>LegacyOwnedMainGlyphBacktrackRows.Prepare(ak,null!,callbacks,new byte[72]));
    Reject(()=>LegacyOwnedMainGlyphBacktrackRows.Prepare(ak,compact,null!,new byte[72]));
    Reject(()=>LegacyOwnedMainGlyphBacktrackRows.Prepare(ak,compact,callbacks,new byte[71]));
    Reject(()=>LegacyOwnedMainGlyphBacktrackRows.Prepare(ak,compact,callbacks,new byte[72],new(true)));
    var corrupted=ak.ToArray();corrupted[566416]^=1;Reject(()=>LegacyOwnedMainGlyphBacktrackRows.Prepare(corrupted,compact,callbacks,new byte[72]));
    var otherOwner=new LegacyOwnedMainWordSpanPreparation(ak,lk,engine,fixture.Sampled,fixture.Table);
    var otherCallbacks=LegacyOwnedRussianMainSourceSymbolCallbacks.Prepare(LegacyOwnedRussianMainGlyphConfidence.Prepare(ak,otherOwner.DictionaryOwner));
    Reject(()=>LegacyOwnedMainGlyphBacktrackRows.Prepare(ak,compact,otherCallbacks,new byte[72]));
    w.WriteNumber("guards",guards);w.WriteNumber("rowCount",rows);w.WriteEndObject();w.Flush();
}
Console.WriteLine($"PASS128 complete actual compact-winner walks, {rows} full72 rows, padding/state/source-key controls and {guards} owner/cancel/resource guards. No segment callback, protocol scoring or full218560 claim.");
void Reject(Action action){try{action();}catch(Exception e)when(e is ArgumentException or InvalidDataException or NotSupportedException or OperationCanceledException or InvalidOperationException){guards++;return;}throw new Exception("Invalid owned backtrack input accepted.");}
static void Require(bool value,string text){if(!value)throw new Exception(text);}
