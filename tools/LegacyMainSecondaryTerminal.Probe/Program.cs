using InkRecognition;
using System.Buffers.Binary;
using System.Text.Json;

if (args.Length != 8) throw new ArgumentException("controls AK LK engine separation case publication capture");
using var input = JsonDocument.Parse(File.ReadAllBytes(args[0]));
int controls = 0, guards = 0;
foreach (var c in input.RootElement.EnumerateArray())
{
    var contexts = new LegacySymbolContextFactory();
    ulong[] ids = [0, contexts.ResolveInitial(0xfff9).Identity, contexts.ResolveInitial(0xfffb).Identity,
        contexts.ResolveInitial(0xfffc).Identity, 0];
    ulong parent = contexts.ResolveInitial(17).Identity;
    ids[4] = contexts.ResolveChild(parent, 0xfff9).Identity;
    contexts.LinkCreatedContext(ids[4], parent);
    var rows = c.GetProperty("rows"); var alternatives = c.GetProperty("alts");
    int count = alternatives[0].GetArrayLength();
    byte[] storage = new byte[rows.GetArrayLength()*88], matrix = new byte[rows.GetArrayLength()*count*40];
    for (int i = 0; i < rows.GetArrayLength(); i++)
    {
        var row = rows[i]; var dest = storage.AsSpan(i*88,88);
        W32(dest,16,row.GetProperty("cost").GetInt32());dest[4]=(byte)row.GetProperty("source").GetInt32();
        dest[5]=(byte)row.GetProperty("flags").GetInt32();dest[0x27]=(byte)row.GetProperty("flag27").GetInt32();
        W32(dest,0x30,row.GetProperty("field30").GetInt32());
        BinaryPrimitives.WriteUInt64LittleEndian(dest[8..],ids[row.GetProperty("context").GetInt32()]);
        for (int k = 0; k < count; k++)
        {
            var source = alternatives[i][k];var alternative=matrix.AsSpan((i*count+k)*40,40);
            W32(alternative,0,source[0].GetInt32());W32(alternative,4,source[1].GetInt32());
            BinaryPrimitives.WriteUInt64LittleEndian(alternative[8..],source[2].GetUInt64());
            BinaryPrimitives.WriteUInt16LittleEndian(alternative[0x1c..],source[3].GetUInt16());
        }
    }
    var sources=c.GetProperty("sources").EnumerateArray().Select(s=>new LegacyPreparedTerminalSource(s[0].GetUInt32(),s[1].GetInt32())).ToArray();
    var o=c.GetProperty("options");
    var options=new LegacyPreparedMainTerminalOptions(o.GetProperty("primary").GetUInt16(),o.GetProperty("sentinels").GetBoolean(),
        o.GetProperty("filter").GetBoolean(),o.GetProperty("alternate").GetBoolean(),o.GetProperty("require").GetBoolean(),o.GetProperty("flag").GetByte());
    var excluded=c.GetProperty("excluded").EnumerateArray().Select(e=>e.GetUInt64()).ToArray();
    var actual=LegacyNativeMainSecondaryTerminalSelection.SelectWithoutRescue(storage,(ushort)rows.GetArrayLength(),matrix,
        (ushort)count,sources,options,c.GetProperty("cumulative").GetInt32(),contexts,excluded);
    long[] values=[actual.Winner.Ordinal,actual.SecondaryOrdinal,actual.Winner.RelativeCost,actual.Winner.TotalCost,
        actual.Winner.RowCost14,actual.Winner.RowField22,actual.Winner.FallbackFlag,actual.Winner.ResultFlag8,(long)actual.ExclusionIdentity];
    if (!values.SequenceEqual(c.GetProperty("expected").EnumerateArray().Select(e=>e.GetInt64())))
        throw new Exception($"Independent matrix control{controls} differs: {string.Join(',',values)}.");
    controls++;
}

byte[] ak=File.ReadAllBytes(args[1]),lk=File.ReadAllBytes(args[2]),engine=File.ReadAllBytes(args[3]);
var model=LegacyRussianModelBundle.Load(ak,lk,engine);var table=LegacyCursiveSeparationTable.Load(File.ReadAllBytes(args[4]));
using var cf=File.OpenRead(args[5]);using var pf=File.OpenRead(args[6]);
var caseData=LegacyNativeCaseData.Load(cf);var publication=LegacyNativeTextPublicationData.Load(pf);
var sampled=MainFixture.Create(ak,engine,table);
var dictionary=LegacyFreshCaseRussianDictionaryPreparation.Prepare(ak,lk,model,sampled,caseData,publication);
var owner=new LegacyOwnedMainWordSpanPreparation(ak,dictionary,sampled,table);
var controller=LegacyOwnedFreshMainWordControllerPolicy.PrepareWithSecondaryCountProperty(ak,owner,2);
var policy=LegacyFreshCaseMainSourcePolicy.Prepare(ak,lk,controller);
ulong identity=0;
var pass=new LegacyOwnedMainCaseWordPass(ak,policy,table,r=>
{byte[] data=new byte[r.RequiredBytes];r.PreviousStorage.Span.CopyTo(data);return new(data,++identity);}).RunOwned();
Reject(()=>LegacyOwnedMainSecondaryTerminalCursor.Prepare(null!));
Reject(()=>LegacyOwnedMainSecondaryTerminalCursor.Prepare(pass,new CancellationToken(true)));
var cursor=LegacyOwnedMainSecondaryTerminalCursor.Prepare(pass);
Reject(()=>cursor.SelectNext(new CancellationToken(true)));
var final=pass.GetNodeSnapshot(pass.NodeCount-1);
int penalty=LegacyOwnedRussianTerminalSources.PreparePositiveScalePenalty(model.LanguageModel,
    model.LanguageProfiles[model.ResourceLanguageProfileIndex],dictionary.Descriptor.RuntimeLanguageScale);
using var file=File.Create(args[7]);using var writer=new Utf8JsonWriter(file);
writer.WriteStartObject();writer.WriteNumber("configured",cursor.ConfiguredOutputLimit);
writer.WriteNumber("primary",controller.Configuration.PrimaryArcLimit230a);writer.WriteNumber("sourceEnd",final.SourceEnd23f0);
writer.WriteNumber("cumulative",pass.CopyCumulativePathCosts()[^1]);writer.WriteNumber("header",dictionary.Descriptor.Flags);writer.WriteNumber("penalty",penalty);
writer.WriteBoolean("sentinels",cursor.Options.ContextSentinels24b8);
writer.WriteBase64String("records",final.Records.Span);writer.WriteBase64String("secondary",final.SecondaryAlternatives.Span);
writer.WriteStartArray("contexts");foreach(var node in owner.Contexts.CopyNodes())
{writer.WriteStartArray();writer.WriteNumberValue(node.Identity);writer.WriteNumberValue(node.ParentLink);writer.WriteNumberValue(node.Symbol);writer.WriteEndArray();}
writer.WriteEndArray();writer.WriteStartArray("choices");
var seen=new HashSet<ulong>();int choices=0;
while(!cursor.IsExhausted)
{
    var choice=cursor.SelectNext();if(choice is null)break;
    if(!ReferenceEquals(choice.PassResult,pass)||!seen.Add(choice.Selection.ExclusionIdentity))throw new Exception("Foreign or repeated native exclusion identity.");
    byte[] row=choice.CopyStoredRow88(),alternative=choice.CopySecondaryAlternative40();
    byte[] escaped=choice.CopySecondaryAlternative40();escaped[0]^=255;
    if(!choice.CopySecondaryAlternative40().SequenceEqual(alternative))throw new Exception("Choice leaked mutable2410 bytes.");
    var win=choice.Selection.Winner;
    writer.WriteStartObject();writer.WriteNumber("ordinal",win.Ordinal);writer.WriteNumber("secondaryOrdinal",choice.Selection.SecondaryOrdinal);
    writer.WriteNumber("identity",choice.Selection.ExclusionIdentity);writer.WriteNumber("relative",win.RelativeCost);writer.WriteNumber("total",win.TotalCost);
    writer.WriteNumber("row14",win.RowCost14);writer.WriteNumber("rawCompact",win.RowField22);
    writer.WriteBase64String("row88",row);writer.WriteBase64String("alternative40",alternative);writer.WriteEndObject();choices++;
}
writer.WriteEndArray();Reject(()=>cursor.SelectNext());
var stale=LegacyOwnedMainSecondaryTerminalCursor.Prepare(pass);
owner.AdvanceNodeEpoch(-1);
Reject(()=>stale.SelectNext());Reject(()=>LegacyOwnedMainSecondaryTerminalCursor.Prepare(pass));
writer.WriteNumber("controls",controls);writer.WriteNumber("guards",guards);writer.WriteEndObject();writer.Flush();
if(choices<2)throw new Exception("Actual2410 did not yield two distinct global terminal identities.");
Console.WriteLine($"PASS{controls} independent matrix controls/actual4-node2410→{choices} distinct numeric terminals/{guards} guards; no WORD or accepted-alternative counter supplied.");

static void W32(Span<byte> data,int at,int value)=>BinaryPrimitives.WriteInt32LittleEndian(data[at..],value);
void Reject(Action action)
{try{action();}catch(Exception error)when(error is ArgumentException or InvalidOperationException or OperationCanceledException){guards++;return;}throw new Exception("Invalid terminal cursor state accepted.");}
