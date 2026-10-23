using InkRecognition;
using System.Text.Json;
using System.Buffers.Binary;
internal static class ContextReceipts
{
 internal static void Run(string[] args)
 {
if(args.Length!=3)throw new ArgumentException("models raw-child-inputs output");
var model=LegacyFullEnglishFirstKnowledge.Load(File.ReadAllBytes(Path.Combine(args[0],"en-US/en_US-ak-cur.res")));var source=model.Source;var table=LegacyCursiveSeparationTable.Load(File.ReadAllBytes(Path.Combine(args[0],"ru-RU/legacy-separation-profile.json")));
using var input=JsonDocument.Parse(File.ReadAllBytes(args[1]));using var stream=File.Create(args[2]);using var w=new Utf8JsonWriter(stream);w.WriteStartArray();int cases=0;
void Ints(IEnumerable<int> xs){w.WriteStartArray();foreach(int x in xs)w.WriteNumberValue(x);w.WriteEndArray();}
foreach(var item in input.RootElement.EnumerateArray())
{
 var raw=item.GetProperty("raw").EnumerateArray().Select(s=>new InkStroke(s.EnumerateArray().Select(p=>new InkPoint(p[0].GetSingle(),p[1].GetSingle())).ToArray())).ToArray();var budget=new LegacyFullEnglishFirstBudget(new(),default);var prep=LegacyFullEnglishFirstPreprocessing.Prepare(model,budget,raw);
 var graphChild=source.Children.Single(c=>c.Destination=="247.138");var graphBytes=source.CopyChildEnvelopeBytes(graphChild);var span=LegacyCursiveLatticeSpanProfile.ReadV10aResourceNode(graphBytes,0);var profile=LegacyOrdinaryCursiveSeedProfile.ReadV10aResourceNode(graphBytes,0);var sampler=source.ReadSampler(source.Children.Single(c=>c.Destination=="247.130"));byte[] S(string n)=>sampler.Fields.Single(f=>f.Destination==n).CopyBytes();float F(string n)=>BinaryPrimitives.ReadSingleLittleEndian(S(n));int I(string n)=>BinaryPrimitives.ReadInt32LittleEndian(S(n));
 var cuts=LegacyCursiveCutProducer.GenerateRussianVersion106(prep.CopyPreparedStrokes(),prep.Line.Baseline,prep.Line.Height,prep.OriginalSource.SourceStrokeProbabilities.ToArray(),false,null);
 var seeds=LegacyOrdinaryCursiveSeedProducer.GenerateLeftToRight(cuts,prep.Line.Height,profile,new byte[raw.Length],Enumerable.Repeat(-1,raw.Length).ToArray());var seedGraph=LegacyOrdinaryCursiveSeedGraph.CreateOrdinaryDeferred(seeds);
 var chronology=prep.PreparedChronology.WithInsertedPreparedInk(cuts.PreparedInk);var ranges=LegacyOrdinaryGraphSourceRanges.PrepareBounded(seeds,chronology,prep.OriginalSource,span.MaximumAtoms,4096,budget.Graph);
 var pruning=LegacyOrdinarySamplerGroupPruning.PrepareSingleDescriptor(ranges.Graph,cuts.PreparedInk,prep.Line.Height,new(new(F("68"),F("114"),I("118")),I("11C")),table);var graph=ranges.WithSamplerPruning(pruning).Graph;
 w.WriteStartObject();w.WriteString("Name",item.GetProperty("name").GetString());w.WriteNumber("Descriptor",item.GetProperty("descriptor").GetInt32());w.WriteNumber("Height",prep.Line.Height);w.WriteNumber("RawLine",prep.RawDescriptor.CurrentLineId);w.WriteNumber("DerivedAssignedLine",0);w.WriteBoolean("NNEvaluated",false);
 w.WriteStartArray("Cut");foreach(var s in cuts.PreparedInk.ToStrokes()){w.WriteStartArray();foreach(var pt in s.Points){w.WriteStartArray();w.WriteNumberValue(pt.X);w.WriteNumberValue(pt.Y);w.WriteEndArray();}w.WriteEndArray();}w.WriteEndArray();
 w.WriteStartArray("Seeds");foreach(var seed in seeds.Seeds){w.WriteStartObject();w.WriteNumber("Atom",seed.SourceAtomIndex);w.WriteBoolean("Deferred",seed.Deferred);w.WriteNumber("Displacement",seed.InsertionDisplacement);w.WritePropertyName("Range");Ints(new[]{seed.Atom.Range.StrokeIndex,seed.Atom.Range.StartPoint,seed.Atom.Range.EndPointExclusive});w.WriteEndObject();}w.WriteEndArray();
 w.WriteStartArray("SeedNodes");for(int n=0;n<seedGraph.NodeCount;n++){w.WriteStartArray();foreach(var arc in seedGraph.GetNode(n)){w.WriteStartObject();w.WriteNumber("Group",arc.SeedGroupIndex);w.WriteNumber("Start",arc.StartContextId);w.WriteNumber("End",arc.EndContextId);w.WriteNumber("Flags",arc.Flags);w.WritePropertyName("Pred");Ints(arc.PredecessorArcIndices.ToArray());w.WritePropertyName("Succ");Ints(arc.SuccessorArcIndices.ToArray());w.WriteEndObject();}w.WriteEndArray();}w.WriteEndArray();
 w.WriteStartArray("Nodes");var groups=new HashSet<int>();for(int n=0;n<graph.NodeCount;n++){w.WriteStartArray();foreach(var arc in graph.GetNode(n)){groups.Add(arc.GroupId);w.WriteStartObject();w.WriteNumber("Group",arc.GroupId);w.WriteNumber("Span",arc.Span);w.WriteNumber("Start",arc.StartContextId);w.WriteNumber("End",arc.EndContextId);w.WriteNumber("Flags",arc.Flags);w.WritePropertyName("Pred");Ints(arc.PredecessorArcIndices.ToArray());w.WriteEndObject();}w.WriteEndArray();}w.WriteEndArray();
 var all=graph.Groups;w.WriteStartArray("Groups");foreach(int id in groups.Order()){w.WriteStartObject();w.WriteNumber("Id",id);w.WritePropertyName("Seeds");Ints(all[id].SeedGroupIndices.ToArray());w.WriteStartArray("Ranges");foreach(var r in all[id].Ranges.Span)Ints(new[]{r.StrokeIndex,r.StartPoint,r.EndPointExclusive});w.WriteEndArray();w.WriteEndObject();}w.WriteEndArray();w.WriteEndObject();w.Flush();cases++;
}
w.WriteEndArray();Console.WriteLine($"PASS existingpure full-FIRST child preprocessing/seed/graph receipts{cases}; no NN evaluator called, no B6 production");
 }
}
