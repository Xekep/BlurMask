using InkRecognition;
using System.Text.Json;
namespace FullFirstSourceRecordsProbe;

//Generic numeric kernel controls ONLY; no actual B7/B8 owner is fabricated.
internal static class PreparedControls
{
    internal static void Run(string models,string controls,string output)
    {
        var knowledge=LegacyFullEnglishFirstKnowledge.Load(File.ReadAllBytes(Path.Combine(models,"en-US/en_US-ak-cur.res")));var model=knowledge.Model;
        var sampler=model.Source.Children.Single(c=>c.Destination=="247.130");var category=model.Source.ReadSampler(sampler).ReadCategories();
        using var json=JsonDocument.Parse(File.ReadAllBytes(controls));if(!json.RootElement.GetProperty("PreparedOnly").GetBoolean())throw new ArgumentException("Prepared inputs only");
        using var file=File.Create(output);using var w=new Utf8JsonWriter(file);w.WriteStartObject();w.WriteBoolean("PreparedOnly",true);w.WriteBoolean("NNEvaluated",false);w.WriteStartArray("Cases");
        foreach(var item in json.RootElement.GetProperty("Cases").EnumerateArray())
        {
            var strokes=item.GetProperty("Bits").EnumerateArray().Select(s=>new InkStroke(s.EnumerateArray().Select(p=>new InkPoint(BitConverter.Int32BitsToSingle(p[0].GetInt32()),BitConverter.Int32BitsToSingle(p[1].GetInt32()))).ToArray())).ToArray();
            var map=item.GetProperty("Mapping").EnumerateArray().Select(x=>x.GetInt32()).ToArray();
            var ranges=item.GetProperty("Ranges").EnumerateArray().Select(r=>new LegacyFirstResultSourceRange(new(r[0].GetInt32(),r[1].GetInt32(),r[2].GetInt32()),new(r[3].GetInt32(),r[4].GetInt32(),r[5].GetInt32()))).ToArray();
            byte[] records=item.GetProperty("Records").GetBytesFromBase64();int selected=item.GetProperty("Selected").GetInt32();
            using var cancel=new CancellationTokenSource();if(item.TryGetProperty("Cancel",out var c)&&c.GetBoolean())cancel.Cancel();
            w.WriteStartObject();w.WriteString("Name",item.GetProperty("Name").GetString());
            try
            {
                var row=LegacyFirstResultSourceRecord.Create(records,selected,ranges,map,item.TryGetProperty("NullParent",out var np)&&np.GetBoolean()?null!:strokes,category,model.SourceScoreExponent,cancel.Token);
                w.WriteString("Status","Produced");w.WriteBase64String("Storage",row.CopyStorage().Span);w.WriteNumber("LabelCount",row.LabelCount);w.WriteStartArray("Projected");foreach(var r in row.Ranges)SourceExports.Ints(w,SourceExports.Range(r));w.WriteEndArray();
            }
            catch(Exception e)when(e is ArgumentException or NotSupportedException or OperationCanceledException)
            {w.WriteString("Status","Refused");w.WriteString("ExceptionType",e.GetType().Name);}
            w.WriteEndObject();
        }w.WriteEndArray();w.WriteEndObject();
    }
}
