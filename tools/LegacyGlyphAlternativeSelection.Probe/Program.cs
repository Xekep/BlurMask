using System.Text.Json;
using InkRecognition;

if(args.Length!=2)throw new ArgumentException("controls.json output.json");
using var input=JsonDocument.Parse(File.ReadAllBytes(args[0]));int guards=0,totalRows=0,failed=0;
using(var stream=File.Create(args[1]))using(var output=new Utf8JsonWriter(stream))
{
    output.WriteStartObject();output.WriteStartArray("results");
    foreach(var control in input.RootElement.GetProperty("controls").EnumerateArray())
    {
        byte[] rows=control.GetProperty("owner").GetBytesFromBase64(),allocation=control.GetProperty("allocation").GetBytesFromBase64();
        var node=new LegacyNativeGlyphRecordNode(control.GetProperty("key").GetUInt64(),control.GetProperty("packed").GetUInt16(),rows);
        uint? marker=control.GetProperty("marker").ValueKind==JsonValueKind.Null?null:control.GetProperty("marker").GetUInt32();
        var result=LegacyPreparedGlyphAlternativeSelection.Select(node,marker,allocation,control.GetProperty("initialCapacity").GetInt32());
        if(result.OwnerKey!=node.Key||result.PackedOwnerCount!=node.PackedCount||!result.CopyOwnerStorage40().AsSpan().SequenceEqual(rows))
            throw new InvalidDataException("Owner rows or canonical key changed.");
        byte[] before=result.CopySelectionCapacityStorage4();var copied=result.CopySelectionCapacityStorage4();
        copied.AsSpan().Fill(0x42);rows.AsSpan().Fill(0x43);allocation.AsSpan().Fill(0x44);
        if(!result.CopySelectionCapacityStorage4().AsSpan().SequenceEqual(before)||
            !result.CopyOwnerStorage40().AsSpan().SequenceEqual(control.GetProperty("owner").GetBytesFromBase64()))
            throw new InvalidDataException("Prepared result leaked mutable caller arrays.");
        output.WriteStartObject();output.WriteBoolean("succeeded",result.Succeeded);output.WriteNumber("count",result.Count);
        output.WriteNumber("capacity",result.Capacity);output.WriteNumber("nativeOwnerCount",result.NativeOwnerCount);
        if(result.SelectedMarker is uint value)output.WriteNumber("marker",value);else output.WriteNull("marker");
        output.WriteBase64String("storage",before);output.WriteBase64String("rows",result.CopySelectionRows4());output.WriteBase64String("owner",result.CopyOwnerStorage40());
        output.WriteEndObject();totalRows+=result.Count;if(!result.Succeeded)failed++;
    }
    output.WriteEndArray();
    Expect(()=>LegacyPreparedGlyphAlternativeSelection.Select(new(1UL<<60,0,default),0,[]));
    Expect(()=>LegacyPreparedGlyphAlternativeSelection.Select(new(0,4,new byte[39]),0,[]));
    Expect(()=>LegacyPreparedGlyphAlternativeSelection.Select(new(0,8,new byte[40]),0,[]));
    Expect(()=>LegacyPreparedGlyphAlternativeSelection.Select(new(0,0,default),0,new byte[3]));
    Expect(()=>LegacyPreparedGlyphAlternativeSelection.Select(new(0,0,default),0,[],1));
    Expect(()=>LegacyPreparedGlyphAlternativeSelection.Select(new(0,0,default),0,[],-1));
    using var cancelled=new CancellationTokenSource();cancelled.Cancel();
    try{LegacyPreparedGlyphAlternativeSelection.Select(new(0,0,default),0,[],cancellationToken:cancelled.Token);throw new InvalidDataException("Cancellation ignored.");}
    catch(OperationCanceledException){guards++;}
    output.WriteNumber("guards",guards);output.WriteEndObject();
}
Console.WriteLine($"PASS prepared13A830 {totalRows} emitted selection rows/{failed} explicit allocation refusals/{guards} guards; no lazy owner-refresh or final-wrapper claim.");
void Expect(Action action){try{action();throw new InvalidDataException("Guard ignored.");}catch(ArgumentException){guards++;}}
