using InkRecognition;
using System.Text.Json;
using System.Text;
using System.Security.Cryptography;
if(args.Length==2&&args[0]=="--prepared-controls"){PreparedControls.Run(args[1]);return;}
if(args.Length!=4||args[0]!="--actual")throw new ArgumentException("--prepared-controls output OR --actual models blind-input output; actual mode requires review authorization");
//Compiled for source review. Actual mode has NOT been executed in compile freeze.
var knowledge=LegacyFullEnglishFirstKnowledge.Load(File.ReadAllBytes(Path.Combine(args[1],"en-US/en_US-ak-cur.res")));
var m30=File.ReadAllBytes(Path.Combine(args[1],"line-geometry/line-2e3-30.res"));var m50=File.ReadAllBytes(Path.Combine(args[1],"line-geometry/line-2e3-50.res"));
var table=LegacyCursiveSeparationTable.Load(File.ReadAllBytes(Path.Combine(args[1],"ru-RU/legacy-separation-profile.json")));
using var input=JsonDocument.Parse(File.ReadAllBytes(args[2]));var items=input.RootElement.ValueKind==JsonValueKind.Array?input.RootElement.EnumerateArray().ToArray():input.RootElement.GetProperty("samples").EnumerateArray().ToArray();
using var output=File.Create(args[3]);using var w=new Utf8JsonWriter(output);w.WriteStartObject();w.WriteBoolean("TargetsRead",false);w.WriteString("Scope","Actual B7 owner/source observations, not720/acceptedtext/guide");w.WriteStartArray("Actual");int accepted=0,refused=0;
foreach(var item in items)
{
    foreach(var p in item.EnumerateObject())if(p.Name is not("Raw" or "Name" or "Resolution" or "strokes" or "id"))throw new ArgumentException("Only blind coordinate/name/resolution fields admitted");
    var raw=item.TryGetProperty("Raw",out var rawValue)?rawValue:item.GetProperty("strokes");string name=item.TryGetProperty("Name",out var n)?n.GetString()!:item.GetProperty("id").GetString()!;float resolution=item.TryGetProperty("Resolution",out var res)?res.GetSingle():0;
    var strokes=raw.EnumerateArray().Select(s=>new InkStroke(s.EnumerateArray().Select(p=>new InkPoint(p[0].GetSingle(),p[1].GetSingle())).ToArray())).ToArray();
    w.WriteStartObject();w.WriteString("Name",name);
    //Do not catch and overwrite JSON after publication. Build owned pass first;
    //a model/source admission refusal is emitted as a distinct complete object.
    LegacyFullEnglishFirstTerminal terminal;
    try
    {
        var a=LegacyFullEnglishFirstParentInput.Prepare(knowledge,strokes,resolution,m30,m50);var b1=LegacyFullEnglishFirstController.Prepare(a);b1.Run(table);
        var b2=LegacyFullEnglishFirstControllerCosts.Prepare(b1,table);b2.Run();var b3=LegacyFullEnglishFirstPrimary.Prepare(b2);b3.Run();
        var b4=LegacyFullEnglishFirstContextPasses.Prepare(b3);b4.Run();terminal=LegacyFullEnglishFirstTerminal.Prepare(b4);terminal.Run();
    }
    catch(Exception e)when(e is ArgumentException or NotSupportedException or InvalidDataException){w.WriteString("Status","UpstreamRefused");w.WriteString("Reason",e.Message);w.WriteEndObject();refused++;continue;}
    var parent=terminal.Owner.Owner.Owner.Owner.Parent;int nn=parent.NetworkEvaluationCount;
    var zero=LegacyFullEnglishFirstPlainText.Prepare(terminal);zero.Run();
    byte[] word=Enumerable.Range(0,120).Select(i=>(byte)(i*37+11)).ToArray(),glyph=Enumerable.Range(0,40).Select(i=>(byte)(i*59+23)).ToArray();foreach(int off in new[]{32,88,104})word.AsSpan(off,8).Clear();foreach(int off in new[]{96,112})word.AsSpan(off,4).Clear();
    var seeded=LegacyFullEnglishFirstPlainText.Prepare(terminal);seeded.RunPrepared(null,null,word,glyph);
    LifecycleControls.RunBasic(zero);LifecycleControls.RunMulti(zero);
    LegacyFullEnglishFirstGlyphEmission[] passes;
    try{passes=[LegacyFullEnglishFirstGlyphEmission.Prepare(zero),LegacyFullEnglishFirstGlyphEmission.Prepare(seeded)];}
    catch(Exception e)when(e is ArgumentException or NotSupportedException or InvalidDataException){w.WriteString("Status","B7PreentryRefused");w.WriteString("Reason",e.Message);w.WriteEndObject();refused++;continue;}
    w.WriteString("Status","Accepted");w.WriteNumber("NetworkBefore",nn);w.WriteNumber("Peak",passes[0].AdditionalLogicalPeakBytes);w.WriteNumber("Work",passes[0].LogicalCopyWorkBytes);w.WriteNumber("Visits",passes[0].LogicalVisitWork);
    var plain=zero.CopyCompletedChildren();w.WriteStartArray("Sources");foreach(var child in plain)SourceReceipt.Write(w,child);w.WriteEndArray();w.WriteStartArray("Variants");
    for(int variant=0;variant<2;variant++)
    {
        var pass=passes[variant];var attempt=new List<(int Child,LegacyFullEnglishFirstGlyphEmissionAttempt Value)>();var query=new List<(int Child,LegacyFullEnglishFirstGlyphEmissionQuery Value)>();var ranges=new List<(int Child,LegacyFullEnglishFirstGlyphEmissionRange Value)>();var stages=new List<(int Child,string Stage)>();int current=-1;
        using var eventHash=IncrementalHash.CreateHash(HashAlgorithmName.SHA256);int eventCount=0;
        void Event(string text){if(++eventCount>131072)throw new Exception("Probe event cap");eventHash.AppendData(Encoding.UTF8.GetBytes($"{current}:{text}\n"));}
        pass.RunObserved(s=>{if(s=="child:entry")current++;if(stages.Count==32768)throw new Exception("Probe stage cap");stages.Add((current,s));Event("s:"+s);},
            a=>{if(attempt.Count==4096)throw new Exception("Probe attempt cap");attempt.Add((current,a));Event("a");},
            q=>{if(query.Count==4096)throw new Exception("Probe query cap");query.Add((current,q));Event("q");},
            observation=>{if(ranges.Count==65536)throw new Exception("Probe range cap");ranges.Add((current,observation));Event("r:"+observation.Pass);},new byte[40]);
        if(!pass.IsComplete||pass.IsFaulted||parent.NetworkEvaluationCount!=nn)throw new Exception("B7 failure or NN replay");
        w.WriteStartObject();w.WriteNumber("Residue",variant);w.WriteStartArray("Attempts");foreach(var (ci,a) in attempt){w.WriteStartArray();w.WriteNumberValue(ci);w.WriteNumberValue(a.Encounter);w.WriteNumberValue(a.Reference);w.WriteNumberValue(a.Key);w.WriteNumberValue(a.Descriptor);w.WriteNumberValue(a.Ordinal);w.WriteNumberValue(a.Symbol);w.WriteBooleanValue(a.Existing);w.WriteEndArray();}w.WriteEndArray();
        w.WriteString("EventSha256",Convert.ToHexString(eventHash.GetHashAndReset()).ToLowerInvariant());w.WriteNumber("EventCount",eventCount);
        w.WriteStartArray("Queries");foreach(var (ci,q) in query)SourceReceipt.Ints(w,[ci,q.Node,q.Arc,q.Symbol,q.Category,q.Prototype,q.Ordinal,q.Top?1:0,q.Primitive?1:0,q.Cost,BitConverter.SingleToInt32Bits(q.Score)]);w.WriteEndArray();
        w.WriteStartArray("RangeObservations");foreach(var (ci,r) in ranges){w.WriteStartArray();w.WriteNumberValue(ci);w.WriteNumberValue(r.Key);w.WriteNumberValue(r.Pass);w.WriteNumberValue(r.Ordinal);w.WriteNumberValue(r.Count);foreach(int x in SourceReceipt.Range(r.Range))w.WriteNumberValue(x);w.WriteEndArray();}w.WriteEndArray();
        w.WriteStartArray("Stages");foreach(var (ci,s) in stages){w.WriteStartArray();w.WriteNumberValue(ci);w.WriteStringValue(s);w.WriteEndArray();}w.WriteEndArray();
        w.WriteStartArray("Children");foreach(var child in pass.CopyCompletedChildren())
        {
            w.WriteStartObject();w.WriteNumber("Descriptor",child.Original.Factory.Candidates.Child.DescriptorOrdinal);w.WriteNumber("Attempts",child.AppendAttempts);w.WriteNumber("Fresh",child.FreshRows);w.WriteNumber("RangePasses",child.RangePasses);
            w.WriteStartArray("OriginalOwnersAfter");foreach(var o in child.Original.CopyGlyphOwners()){w.WriteStartObject();w.WriteNumber("Key",o.Key);w.WriteNumber("PackedCount",o.PackedCount);w.WriteBase64String("Records",o.Records.Span);w.WriteEndObject();}w.WriteEndArray();
            w.WriteStartArray("OriginalWordsAfter");foreach(var o in child.Original.NativeWords){w.WriteStartObject();w.WriteBase64String("Storage",o.Published.Word.CopyStorage());w.WriteStartArray("References");foreach(ulong x in o.Published.Word.FlatReferences)w.WriteNumberValue(x);w.WriteEndArray();w.WriteEndObject();}w.WriteEndArray();
            w.WriteStartArray("Owners");foreach(var o in child.CopyGlyphOwners()){w.WriteStartObject();w.WriteNumber("Key",o.Key);w.WriteNumber("PackedCount",o.PackedCount);w.WriteBase64String("Records",o.Records.Span);w.WriteEndObject();}w.WriteEndArray();
            w.WriteStartArray("Ranges");foreach(ulong key in child.CopySourceRangeKeys()){w.WriteStartObject();w.WriteNumber("Key",key);w.WriteStartArray("Rows");foreach(var row in child.CopySourceRanges(key))SourceReceipt.Ints(w,SourceReceipt.Range(row));w.WriteEndArray();w.WriteEndObject();}w.WriteEndArray();
            w.WriteStartArray("Words");foreach(var value in child.Words){w.WriteStartObject();w.WriteBase64String("Storage",value.CopyStorage());w.WriteStartArray("References");foreach(ulong x in value.CopyReadingOrderReferences())w.WriteNumberValue(x);w.WriteEndArray();w.WriteEndObject();}w.WriteEndArray();w.WriteEndObject();
        }w.WriteEndArray();w.WriteEndObject();
    }w.WriteEndArray();w.WriteNumber("NetworkAfter",parent.NetworkEvaluationCount);w.WriteEndObject();w.Flush();accepted++;
}
w.WriteEndArray();w.WriteNumber("Accepted",accepted);w.WriteNumber("Refused",refused);w.WriteStartArray("LifecycleGuards");foreach(string guard in LifecycleControls.Passed)w.WriteStringValue(guard);w.WriteEndArray();w.WriteStartArray("LifecycleProofs");foreach(var proof in LifecycleControls.Proofs){w.WriteStartArray();w.WriteStringValue(proof.Label);w.WriteStringValue(proof.Type);w.WriteStringValue(proof.Message);w.WriteEndArray();}w.WriteEndArray();w.WriteStartArray("LifecycleReached");foreach(string stage in LifecycleControls.Reached)w.WriteStringValue(stage);w.WriteEndArray();w.WriteNumber("LifecycleBasicRuns",LifecycleControls.BasicRuns);w.WriteNumber("LifecycleMultiRuns",LifecycleControls.MultiRuns);w.WriteEndObject();Console.WriteLine($"B7 actual accepted{accepted}/refused{refused}; lifecycle{LifecycleControls.Passed.Count}; no720/guide/accuracyclaim");
