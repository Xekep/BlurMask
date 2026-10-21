using InkRecognition;
using System.Buffers.Binary;
using System.Text.Json;

if(args.Length!=10)throw new ArgumentException("AK engine separation case publication line30 line50 INPUTS capture LK");
byte[] ak=File.ReadAllBytes(args[0]),lk=File.ReadAllBytes(args[9]),engine=File.ReadAllBytes(args[1]);
using var ed=JsonDocument.Parse(engine);var activation=ed.RootElement.GetProperty("activations");
var activations=new LegacyInkActivations(activation.GetProperty("hidden").EnumerateArray().Select(v=>v.GetInt32()).ToArray(),activation.GetProperty("output").EnumerateArray().Select(v=>v.GetInt32()).ToArray());
var table=LegacyCursiveSeparationTable.Load(File.ReadAllBytes(args[2]));using var cf=File.OpenRead(args[3]);using var pf=File.OpenRead(args[4]);var cd=LegacyNativeCaseData.Load(cf);var pd=LegacyNativeTextPublicationData.Load(pf);
var model=LegacyRussianModelBundle.Load(ak,lk,engine);byte[] line30=File.ReadAllBytes(args[5]),line50=File.ReadAllBytes(args[6]);
using var data=JsonDocument.Parse(File.ReadAllBytes(args[7]));
using var file=File.Create(args[8]);using var w=new Utf8JsonWriter(file);w.WriteStartArray();int index=0;
foreach(var sample in data.RootElement.GetProperty("samples").EnumerateArray())
{
    var raw=sample.GetProperty("strokes").EnumerateArray().Select(s=>new InkStroke(s.EnumerateArray().Select(p=>new InkPoint(p[0].GetSingle(),p[1].GetSingle())).ToArray())).ToArray();
    using var cancel=new CancellationTokenSource(TimeSpan.FromSeconds(30));
    var input=LegacyFreshMainOwnedTextInputPreparation.Prepare(ak,line30,line50,raw,0,activations,table,cd,pd,cancel.Token).Run(cancel.Token);
    var cuts=LegacyFreshMainDescriptorCutSource.Prepare(ak,input.Smoothed,cancel.Token);var local=LegacyFreshMainDescriptorGraphAppend.Prepare(ak,cuts,table,cancel.Token);var global=LegacyFreshMainGlobalOrdinaryState.Prepare(ak,local,table,cancel.Token);
    var graph=LegacyFreshMainCompactOrdinaryGraph.Prepare(ak,global,cancel.Token);var source=LegacyFreshMainGlobalSourceRanges.Prepare(graph,cancel.Token);var context=LegacyFreshMainGlobalContext.Prepare(ak,source,activations,cancel.Token);var geometry=LegacyFreshMainGlobalGlyphGeometry.Prepare(ak,context,cancel.Token);
    var owner=LegacyFreshMainGlobalCaseSession.Prepare(ak,lk,model,geometry,cd,pd,cancel.Token);var cache=LegacyFreshMainGlobalTransitionCache.Prepare(ak,owner,table,cancel.Token);var costs=LegacyFreshMainGlobalGroupCosts.Prepare(ak,owner,table,cancel.Token);
    w.WriteStartObject();w.WriteNumber("sourceGap",cuts.Preparation.SourceGap);w.WriteStartArray("points");foreach(var s in cuts.CopyStrokes()){w.WriteStartArray();foreach(var p in s.Points)Floats(null,[p.X,p.Y]);w.WriteEndArray();}w.WriteEndArray();
    w.WriteStartArray("groups");for(int g=0;g<graph.State.Groups.Count;g++)
    {
        var group=graph.State.Groups[g];var pair=graph.ResolveGroupSource(g,cancel.Token);
        w.WriteStartObject();w.WriteNumber("descriptor",pair.Primary.DescriptorOrdinal);w.WriteNumber("line",pair.Primary.LineId);w.WriteNumber("height",pair.Primary.Height);w.WriteNumber("primaryBaseline",pair.Primary.Baseline);if(pair.Secondary is{} secondary){w.WriteNumber("secondaryBaseline",secondary.Baseline);w.WriteNumber("secondaryHeight",secondary.Height);}
        w.WriteNumber("cost",costs.ReadGeometryCost(g,cancel.Token));w.WriteNumber("geometryHeight",geometry.ReadFrame(g,cancel.Token).Height);Floats("bounds",[group.Bounds.MinimumX,group.Bounds.MinimumY,group.Bounds.MaximumX,group.Bounds.MaximumY]);w.WriteStartArray("ranges");foreach(var r in group.CopyRanges())Ints(null,[r.StrokeIndex,r.StartPoint,r.EndPointExclusive]);w.WriteEndArray();w.WriteEndObject();
    }w.WriteEndArray();w.WriteStartArray("nodes");
    for(int n=0;n<graph.State.NodeCount;n++)
    {
        w.WriteStartArray();var row=graph.State.GetNode(n);for(ushort a=0;a<row.Count;a++)
        {
            var arc=row[a];var state=cache.ReadState(n,a,cancel.Token);w.WriteStartObject();w.WriteNumber("group",arc.GroupId);w.WriteNumber("span",arc.Span);Ints("previous",arc.CopySelector(0).Select(v=>(int)v));Ints("next",arc.CopySelector(1).Select(v=>(int)v));Floats("fold",[state.FutureMinimumX,state.PrefixMaximumX]);Ints("costs",[state.JoinCost,state.TripleJoinCost,state.BlankCost]);Ints("matrix",state.CategoryMatrix.ToArray());w.WriteEndObject();

        }w.WriteEndArray();
    }w.WriteEndArray();
    if(owner.AcousticEvaluations!=0)throw new Exception("Diagnostic must not invoke MAIN glyph NN");
    w.WriteNumber("mainGlyphNNEvaluations",owner.AcousticEvaluations);w.WriteString("id",sample.GetProperty("id").GetString());w.WriteBoolean("expectedLabelsRead",false);w.WriteEndObject();w.Flush();Console.WriteLine("Captured "+(++index));
}
w.WriteEndArray();w.Flush();Console.WriteLine($"PASS {index} actual trajectory sources/geometry+transition caches; no MAIN glyph NN or targets.");
void Ints(string? name,IEnumerable<int> values){if(name is not null)w.WritePropertyName(name);w.WriteStartArray();foreach(int v in values)w.WriteNumberValue(v);w.WriteEndArray();}
void Floats(string? name,IEnumerable<float> values){if(name is not null)w.WritePropertyName(name);w.WriteStartArray();foreach(float v in values)w.WriteNumberValue(v);w.WriteEndArray();}
