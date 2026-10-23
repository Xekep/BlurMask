using InkRecognition;
using System.Text.Json;

internal static class SourceReceipt
{
    internal static void Ints(Utf8JsonWriter w,IEnumerable<int> values){w.WriteStartArray();foreach(int v in values)w.WriteNumberValue(v);w.WriteEndArray();}
    internal static int[] Identity(LegacySourcePointIdentity p)=>[p.Key.First,p.Key.Second,p.Key.Third,p.MetadataPointIndex,p.KeyRecordIndex,p.RemainderByte];
    internal static int[] Range(LegacyFirstResultSourceRange r)=>[r.First.First,r.First.Second,r.First.Third,r.Last.First,r.Last.Second,r.Last.Third];
    internal static void Strokes(Utf8JsonWriter w,IReadOnlyList<InkStroke> strokes)
    {w.WriteStartArray();foreach(var s in strokes){w.WriteStartArray();foreach(var p in s.Points){w.WriteStartArray();w.WriteNumberValue(p.X);w.WriteNumberValue(p.Y);w.WriteEndArray();}w.WriteEndArray();}w.WriteEndArray();}
    internal static void Metadata(Utf8JsonWriter w,IReadOnlyList<InkStroke> raw,LegacyOrdinarySourceInk original,LegacySourcePointChronology prepared)
    {
        w.WritePropertyName("Raw");Strokes(w,raw);w.WritePropertyName("Retained");Strokes(w,original.Strokes);
        w.WriteStartArray("RetainedIndices");foreach(var ids in original.OriginalPointIndices)Ints(w,ids.ToArray());w.WriteEndArray();
        w.WriteStartArray("OriginalIdentities");for(int s=0;s<original.Strokes.Count;s++){w.WriteStartArray();for(int p=0;p<original.Strokes[s].Points.Count;p++)Ints(w,Identity(original.Chronology.Resolve(s,p)));w.WriteEndArray();}w.WriteEndArray();
        w.WriteStartArray("OriginalEndingIdentities");for(int s=0;s<original.Strokes.Count;s++){w.WriteStartArray();for(int p=0;p<original.Strokes[s].Points.Count;p++)Ints(w,Identity(original.ResolveEndingMetadata(original.Chronology.Resolve(s,p))));w.WriteEndArray();}w.WriteEndArray();
        w.WritePropertyName("Counts");Ints(w,prepared.StrokePointCounts.ToArray());w.WriteNumber("Factor",prepared.SourceFactor);
        w.WriteStartArray("Map");foreach(uint value in prepared.PreparedPointMap.Span)w.WriteNumberValue(value);w.WriteEndArray();
        w.WriteStartArray("Resolved");for(int s=0;s<prepared.StrokePointCounts.Length;s++){w.WriteStartArray();for(int p=0;p<prepared.StrokePointCounts.Span[s];p++)Ints(w,Identity(prepared.Resolve(s,p)));w.WriteEndArray();}w.WriteEndArray();
    }
    internal static void Write(Utf8JsonWriter w,LegacyFullEnglishFirstPlainChild child)
    {
        var c=child.Factory.Candidates;var service=c.SourceRanges;var raw=c.Child.Submission.Raw;
        if(!ReferenceEquals(service.OriginalSource,c.Preparation.OriginalSource))throw new Exception("Foreign source receipt");
        var cuts=new LegacyCursiveCutResult(c.PreparedInk,c.CopyMarkers(),c.CopyAtoms());
        var profile=LegacyOrdinaryCursiveSeedProfile.ReadV10aResourceNode(c.Model.Source.CopyChildEnvelopeBytes(c.GraphChild),0);
        var seeds=LegacyOrdinaryCursiveSeedProducer.GenerateLeftToRight(cuts,c.Preparation.Line.Height,profile,new byte[raw.Count],Enumerable.Repeat(-1,raw.Count).ToArray());
        var seedGraph=LegacyOrdinaryCursiveSeedGraph.CreateOrdinaryDeferred(seeds);
        w.WriteStartObject();w.WriteNumber("Descriptor",c.Child.DescriptorOrdinal);w.WriteBoolean("NNEvaluated",false);w.WritePropertyName("ParentMap");Ints(w,c.Child.CopyCurrentParentMap());
        Metadata(w,raw,service.OriginalSource,service.PreparedChronology);
        //Independent oracle composes native incoming factor1 inserted indices
        //through actual prior265 factor200. These are inputs, not expected ranges.
        w.WritePropertyName("PreCutMap200");Ints(w,c.Preparation.CopyPointKeys200());
        w.WritePropertyName("CutOriginalIndices");Ints(w,c.CopyCutRecords().Select(p=>p.OriginalPointIndex));
        w.WriteStartArray("Seeds");foreach(var s in seeds.Seeds){w.WriteStartObject();w.WriteNumber("Atom",s.SourceAtomIndex);w.WriteBoolean("Deferred",s.Deferred);w.WriteNumber("Displacement",s.InsertionDisplacement);w.WritePropertyName("Range");Ints(w,[s.Atom.Range.StrokeIndex,s.Atom.Range.StartPoint,s.Atom.Range.EndPointExclusive]);w.WriteEndObject();}w.WriteEndArray();
        w.WriteStartArray("SeedNodes");for(int n=0;n<seedGraph.NodeCount;n++){w.WriteStartArray();foreach(var a in seedGraph.GetNode(n)){w.WriteStartObject();w.WriteNumber("Group",a.SeedGroupIndex);w.WriteNumber("Start",a.StartContextId);w.WriteNumber("End",a.EndContextId);w.WriteNumber("Flags",a.Flags);w.WritePropertyName("Pred");Ints(w,a.PredecessorArcIndices.ToArray());w.WritePropertyName("Succ");Ints(w,a.SuccessorArcIndices.ToArray());w.WriteEndObject();}w.WriteEndArray();}w.WriteEndArray();
        w.WriteStartArray("Nodes");for(int n=0;n<service.Graph.NodeCount;n++){w.WriteStartArray();foreach(var a in service.Graph.GetNode(n)){w.WriteStartObject();w.WriteNumber("Group",a.GroupId);w.WriteNumber("Start",a.StartContextId);w.WriteNumber("End",a.EndContextId);w.WriteNumber("Span",a.Span);w.WriteNumber("Flags",a.Flags);w.WritePropertyName("Pred");Ints(w,a.PredecessorArcIndices.ToArray());w.WriteEndObject();}w.WriteEndArray();}w.WriteEndArray();w.WriteEndObject();
    }
}
