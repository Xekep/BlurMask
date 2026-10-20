using System.Text.Json;
using System.Text.Json.Serialization;
using InkRecognition;

[JsonSerializable(typeof(LegacyOrdinaryGeometrySnapshot))]
[JsonSourceGenerationOptions(PropertyNamingPolicy=JsonKnownNamingPolicy.CamelCase)]
internal partial class GeometryJson:JsonSerializerContext{}

internal static class GeometryCapture
{
    internal static byte[] Capture(string name,LegacyFreshMainGlobalWordTextPublication parent,LegacyPhysicalRawInkIngress ingress)
    {
        using var stream=new MemoryStream();using var w=new Utf8JsonWriter(stream);w.WriteStartObject();
        w.WriteString("name",name);w.WriteBoolean("genuineMeasuredTrack",name.EndsWith(".track.json",StringComparison.Ordinal));
        if(!ReferenceEquals(parent.Scope.Document,ingress.Document))throw new Exception("Timed source identity lost before NN.");
        w.WriteStartArray("physicalSources");var inputs=ingress.TimingReceipt.CopySources();
        foreach(var s in inputs)
        {
            w.WriteStartObject();w.WriteNumber("source",s.OriginalSourceIndex);w.WriteNumber("points",s.PointCount);
            w.WriteNumber("kind",(int)s.Kind);w.WriteNumber("rateBits",BitConverter.SingleToInt32Bits(s.ExplicitFormatRateHz));
            w.WriteNumber("period",s.NativePeriodMicroseconds);w.WriteNumber("baseTag",s.BaseTagMicroseconds);
            if(s.NativeTimeChannel is{} channel)w.WriteNumber("nativeChannel",channel);else w.WriteNull("nativeChannel");
            if(s.NativeChannelMultiplier is{} multiplier)w.WriteNumber("nativeMultiplier",multiplier);else w.WriteNull("nativeMultiplier");
            w.WriteStartArray("channelMs");foreach(int value in ingress.TimingReceipt.CopyMeasuredMilliseconds(s.OriginalSourceIndex)??[])w.WriteNumberValue(value);w.WriteEndArray();
            w.WriteStartArray("originalSampleTimes");for(int p=0;p<s.PointCount;p++)w.WriteNumberValue(ingress.TimingReceipt.ReadOriginalSample(s.OriginalSourceIndex,p));w.WriteEndArray();
            w.WriteEndObject();
        }
        w.WriteEndArray();var cropped=LegacyGlobalOrdinaryCroppedSource.Prepare(parent);
        w.WriteStartArray("views");foreach(var v in cropped.Views)
        {
            w.WriteStartObject();w.WriteBoolean("word",v.IsWord);w.WriteNumber("node",v.SourceNodeIdentity);w.WriteNumber("range",v.RangeOrdinal);w.WriteNumber("object",v.RawObjectIndex);
            w.WriteNumber("kind",(int)v.Kind);w.WriteNumber("first",v.FirstOriginalPoint);w.WriteNumber("last",v.LastOriginalPoint);
            w.WriteNumber("fraction0",BitConverter.SingleToInt32Bits(v.FirstFraction));w.WriteNumber("fraction1",BitConverter.SingleToInt32Bits(v.LastFraction));
            w.WriteBoolean("include0",v.IncludesFirst);w.WriteBoolean("include1",v.IncludesLast);w.WriteNumber("startTime",ingress.TimingReceipt.ReadViewStart(v));
            w.WriteStartArray("points");foreach(var p in v.CopyPoints()){w.WriteStartArray();w.WriteNumberValue(BitConverter.SingleToInt32Bits(p.X));w.WriteNumberValue(BitConverter.SingleToInt32Bits(p.Y));w.WriteEndArray();}w.WriteEndArray();w.WriteEndObject();
        }w.WriteEndArray();
        LegacyFreshMainGlobalOrdinaryGeometry? geometry=null;
        try
        {
            geometry=LegacyFreshMainGlobalOrdinaryGeometry.Prepare(parent,cropped,ingress.TimingReceipt);
            var snapshot=geometry.Run();if(!snapshot.Ready)throw new Exception("Ready result was not complete.");
            w.WritePropertyName("result");w.WriteRawValue(JsonSerializer.SerializeToUtf8Bytes(snapshot,GeometryJson.Default.LegacyOrdinaryGeometrySnapshot));
        }
        catch(Exception e)when(e is ArgumentException or InvalidOperationException or NotSupportedException)
        {
            w.WriteString("refusal",e.GetType().Name+":"+e.Message);
            if(geometry is not null){w.WritePropertyName("prefix");w.WriteRawValue(JsonSerializer.SerializeToUtf8Bytes(geometry.CopySnapshot(),GeometryJson.Default.LegacyOrdinaryGeometrySnapshot));}
        }
        w.WriteEndObject();w.Flush();return stream.ToArray();
    }
}
