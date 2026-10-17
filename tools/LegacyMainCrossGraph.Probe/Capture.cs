using System.Text.Json;
using InkRecognition;

internal static class Capture
{
internal static void State(Utf8JsonWriter x,LegacyMainGraphAppendState state)
{
    x.WriteStartObject();x.WriteNumber("cap",state.MaximumAtoms);x.WriteStartArray("groups");foreach(var g in state.Groups)
    {
        x.WriteStartObject();var f=g.Fields;WriteInts(x,"fields",[f.DescriptorOrdinal,f.SecondaryDescriptorOrdinal,f.AuxiliaryContextIndex,f.ReferenceCounter,f.HistoryOwned08,f.Flags50,f.Flags51,f.Flags52]);
        var b=g.Bounds;WriteInts(x,"boundsBits",new[]{b.MinimumX,b.MinimumY,b.MaximumX,b.MaximumY}.Select(BitConverter.SingleToInt32Bits));
        x.WriteStartArray("ranges");foreach(var r in g.CopyRanges())WriteInts(x,null,[r.StrokeIndex,r.StartPoint,r.EndPointExclusive]);x.WriteEndArray();
        x.WriteStartArray("history");foreach(var h in g.CopyHistory()){x.WriteStartArray();foreach(var r in h)WriteInts(x,null,[r.StrokeIndex,r.StartPoint,r.EndPointExclusive]);x.WriteEndArray();}x.WriteEndArray();x.WriteEndObject();
    }x.WriteEndArray();x.WriteStartArray("contexts");foreach(var c in state.Contexts){x.WriteStartObject();x.WriteBase64String("prefix",c.CopyPrefix());WriteInts(x,"controls",c.CopyControls());x.WriteEndObject();}x.WriteEndArray();
    x.WriteStartArray("nodes");for(int n=0;n<state.NodeCount;n++){x.WriteStartArray();foreach(var a in state.GetNode(n))Arc(x,a);x.WriteEndArray();}x.WriteEndArray();x.WritePropertyName("root");Arc(x,state.Root);x.WriteEndObject();
}
static void Arc(Utf8JsonWriter x,LegacyMainGraphAppendArc a){x.WriteStartObject();x.WriteStartArray("header");foreach(long value in new long[]{a.GroupId,a.Span,a.Placement,a.Flags,a.StartContextId,a.EndContextId,a.AuxiliaryContextIndex,a.Extra1a,a.Scalar1cBits})x.WriteNumberValue(value);x.WriteEndArray();WriteInts(x,"previous",a.CopyPredecessors());x.WriteStartArray("next");foreach(var v in a.CopySuccessors())WriteInts(x,null,v);x.WriteEndArray();x.WriteEndObject();}
static void WriteInts(Utf8JsonWriter x,string? name,IEnumerable<int> values){if(name is not null)x.WritePropertyName(name);x.WriteStartArray();foreach(int v in values)x.WriteNumberValue(v);x.WriteEndArray();}

}
