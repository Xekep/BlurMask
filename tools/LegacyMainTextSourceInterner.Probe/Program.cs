using InkRecognition;
using System.Buffers.Binary;
using System.Text.Json;

if(args.Length!=12)throw new ArgumentException("AK LK engine separation case publication normalization circles line30 line50 controls capture");
byte[] ak=File.ReadAllBytes(args[0]),lk=File.ReadAllBytes(args[1]),engine=File.ReadAllBytes(args[2]);
var table=LegacyCursiveSeparationTable.Load(File.ReadAllBytes(args[3]));using var cf=File.OpenRead(args[4]);using var pf=File.OpenRead(args[5]);
var cases=LegacyNativeCaseData.Load(cf);var publication=LegacyNativeTextPublicationData.Load(pf);
using var nf=File.OpenRead(args[6]);using var cr=File.OpenRead(args[7]);
var norm=LegacyNativeUnicodeNormalizationData.Load(nf);var circles=LegacyNativeCircledPayloadData.Load(cr);
var rawOwner=LegacyFreshMainSingleLinePlainRecognition.Prepare(ak,lk,engine,File.ReadAllBytes(args[8]),File.ReadAllBytes(args[9]),
    Word(0),0,table,cases,publication);var raw=rawOwner.Run(new byte[120],new byte[40],new ushort[256],new byte[64]);
using var controls=JsonDocument.Parse(File.ReadAllBytes(args[10]));int streams=0,insertions=0,duplicates=0,comparisons=0,guards=0;
foreach(var stream in controls.RootElement.GetProperty("streams").EnumerateArray())
{
    var owner=LegacyOwnedMainTextSourceInterner.Prepare(rawOwner.SourceDocument);var expected=stream.GetProperty("expected");int ordinal=0;
    foreach(var row in stream.GetProperty("keys").EnumerateArray())
    {
        var key=Key(row);var result=owner.InternPrepared(key);var answer=expected.GetProperty("answers")[ordinal++];
        if(result.Node.Identity!=answer[0].GetInt32()||result.Inserted!=answer[1].GetBoolean()||!ReferenceEquals(result.Node.Owner,owner))
            throw new InvalidDataException("Prepared source-node intern identity/order differs.");
        if(result.Inserted)insertions++;else duplicates++;
        byte[] original=result.Node.CopyRanges16();key.AsSpan().Clear();
        if(!result.Node.CopyRanges16().AsSpan().SequenceEqual(original))throw new InvalidDataException("Source key aliases caller coordinates.");
    }
    if(owner.RootIdentity!=expected.GetProperty("root").GetInt32())throw new InvalidDataException("Native AVL root differs.");
    var tree=owner.CopyTree();var wanted=expected.GetProperty("tree");if(tree.Length!=wanted.GetArrayLength())throw new InvalidDataException("Tree cardinality differs.");
    for(int i=0;i<tree.Length;i++)
    {
        var n=tree[i];int[] actual=[n.Identity,n.Left,n.Right,n.Balance,n.PreviousInserted,n.NextInserted];
        if(!actual.AsSpan().SequenceEqual(wanted[i].EnumerateArray().Select(v=>v.GetInt32()).ToArray()))throw new InvalidDataException("Exact native AVL/list links differ.");
    }
    streams++;
}
foreach(var test in controls.RootElement.GetProperty("comparisons").EnumerateArray())
{if(LegacyOwnedMainTextSourceInterner.Compare(Key(test[0]),Key(test[1]))!=test[2].GetInt32())throw new InvalidDataException("Unsigned lexical comparator differs.");comparisons++;}
var completed=LegacyFreshMainFirstCandidateCompletion.Complete(raw,new byte[400],new byte[4],new byte[4]);
using var choices=LegacyFreshMainTextGlyphChoices.Prepare(ak,completed,table,publication,norm,circles);
var words=choices.Publish(new byte[40],new byte[2040*4]);var actualOwner=LegacyOwnedMainTextSourceInterner.Prepare(rawOwner.SourceDocument);
using var output=File.Create(args[11]);using var w=new Utf8JsonWriter(output);w.WriteStartObject();w.WriteStartArray("actual");int actualReads=0;
foreach(var glyph in words.SelectMany(word=>word.Glyphs))
{
    var positions=glyph.SourcePositions;
    for(int repeat=0;repeat<2;repeat++)
    {
        var result=actualOwner.Intern(positions);
        if(!ReferenceEquals(result.Node.FirstSourcePositions.Document,rawOwner.SourceDocument)||
            !ReferenceEquals(result.Node.FirstSourcePositions.Result,raw))throw new InvalidDataException("Actual source-document provenance lost.");
        w.WriteStartObject();w.WriteBase64String("ranges16",result.Node.CopyRanges16());w.WriteNumber("identity",result.Node.Identity);
        w.WriteBoolean("inserted",result.Inserted);w.WriteEndObject();actualReads++;
    }
    int count=actualOwner.Count;Reject(()=>actualOwner.Intern(positions,new CancellationToken(true)));
    if(actualOwner.Count!=count)throw new InvalidDataException("Canceled interning changed the source tree.");
    var foreignDocument=LegacyOwnedRawInkDocument.Create(Word(0),default);
    Reject(()=>LegacyOwnedMainTextSourceInterner.Prepare(foreignDocument).Intern(positions));
}
w.WriteEndArray();w.WriteNumber("root",actualOwner.RootIdentity);w.WriteStartArray("tree");
foreach(var n in actualOwner.CopyTree()){w.WriteStartArray();foreach(int value in new[]{n.Identity,n.Left,n.Right,n.Balance,n.PreviousInserted,n.NextInserted})w.WriteNumberValue(value);w.WriteEndArray();}
w.WriteEndArray();w.WriteEndObject();w.Flush();
Console.WriteLine($"PASS {streams} independent native AVL/list streams/{insertions} nodes/{duplicates} duplicate keys, {comparisons} unsigned comparisons/{actualReads} actual source-position reads/{guards} guards. No024D40 payload or fullwrapper claim.");
LegacyNativeInkPositionRange[] Key(JsonElement rows)=>rows.EnumerateArray().Select(r=>new LegacyNativeInkPositionRange(new(r[0].GetUInt64()),new(r[1].GetUInt64()))).ToArray();
void Reject(Action operation){try{operation();}catch(Exception e)when(e is ArgumentException or OperationCanceledException){guards++;return;}throw new InvalidDataException("Guard ignored.");}
InkStroke[] Word(float x)=>[Draw((x,80),(x,0),(x+30,45),(x+60,0),(x+60,80)),Draw((x+85,80),(x+85,0),(x+135,0),(x+135,80)),Draw((x+165,80),(x+165,0),(x+215,0),(x+215,40),(x+165,40))];
InkStroke Draw(params(float X,float Y)[] knots)
{var points=new List<InkPoint>();for(int i=1;i<knots.Length;i++)for(int t=0;t<8;t++){float f=t/8f;points.Add(new(knots[i-1].X+(knots[i].X-knots[i-1].X)*f,knots[i-1].Y+(knots[i].Y-knots[i-1].Y)*f));}points.Add(new(knots[^1].X,knots[^1].Y));return new(points.ToArray());}
