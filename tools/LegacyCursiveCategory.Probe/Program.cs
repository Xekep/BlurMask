using InkRecognition;
using System.Text.Json;

int controls=0;
if(args.Length!=0)
{
    using var json=JsonDocument.Parse(File.ReadAllBytes(args[0]));
    foreach(var test in json.RootElement.EnumerateArray())
    {
        byte[] bytes=Convert.FromBase64String(test.GetProperty("bytes").GetString()!);
        int offset=test.GetProperty("offset").GetInt32();
        var profile=LegacyCursiveCategoryProfile.Load(bytes,offset,out int consumed);
        Require(consumed==test.GetProperty("consumed").GetInt32(),"Consumed boundary differs.");
        Require(profile.FirstIds.Span.SequenceEqual(Ints(test.GetProperty("ids"))),"First category IDs differ.");
        Require(profile.GeometryIds.Span.SequenceEqual(Ints(test.GetProperty("geometry_ids"))),"Geometry IDs differ.");
        var maps=test.GetProperty("maps").EnumerateArray().Select(Ints).ToArray();
        Require(profile.StartCompositeMap.Span.SequenceEqual(maps[0]) && profile.EndCompositeMap.Span.SequenceEqual(maps[1]) && profile.ZeroStartCompositeMap.Span.SequenceEqual(maps[2]) && profile.ZeroEndCompositeMap.Span.SequenceEqual(maps[3]),"Independent composite maps differ.");
        var cats=test.GetProperty("categories").EnumerateArray().Select(p=>new LegacyCursiveCompositeCategory(p[0].GetInt32(),p[1].GetInt32())).ToArray();
        Require(profile.CompositeCategories.Span.SequenceEqual(cats),"Composite encounter order differs.");
        var prototypes=test.GetProperty("prototypes").EnumerateArray().Select(p=>new LegacyCursiveCategoryPrototype(p[0].GetSingle(),p[1].GetSingle(),p[2].GetSingle(),p[3].GetInt32()!=0)).ToArray();
        Require(profile.Prototypes.Span.SequenceEqual(prototypes),"Prototype floats/flags differ.");
        Reject(()=>LegacyCursiveCategoryProfile.Load(bytes.AsSpan(0,consumed-1),offset,out _));
        int idStart=5+(int)(-(offset+5L)&3);
        var invalidMap=bytes.ToArray();invalidMap[idStart+profile.FirstIds.Length*4]=253;
        Reject(()=>LegacyCursiveCategoryProfile.Load(invalidMap,offset,out _));
        var missingZero=bytes.ToArray();System.Buffers.Binary.BinaryPrimitives.WriteInt32LittleEndian(missingZero.AsSpan(idStart),999);
        Reject(()=>LegacyCursiveCategoryProfile.Load(missingZero,offset,out _));
        var invalidFloat=bytes.ToArray();System.Buffers.Binary.BinaryPrimitives.WriteInt32LittleEndian(invalidFloat.AsSpan(consumed-13),0x7fc00000);
        Reject(()=>LegacyCursiveCategoryProfile.Load(invalidFloat,offset,out _));
        var owned=profile.FirstIds.ToArray();Array.Fill(bytes,(byte)0);
        Require(profile.FirstIds.Span.SequenceEqual(owned),"Caller source mutations changed category profile.");
        controls++;
    }
}
if(args.Length>1)
{
    var resource=File.ReadAllBytes(args[1]);
    var first=LegacyCursiveCategoryProfile.Load(resource.AsSpan(11571),11571,out int a);
    var second=LegacyCursiveCategoryProfile.Load(resource.AsSpan(188222),188222,out int b);
    Require(a==942 && b==943,"Russian profile byte boundary differs.");
    Require(first.BaseEntryCount==192 && first.FirstIds.Span.SequenceEqual(new[]{0,2,4,3,1,6,5}) && first.GeometryIds.Length==19,"Russian profile family differs.");
    Require(first.CompositeCategories.Span.SequenceEqual(second.CompositeCategories.Span) && first.Prototypes.Span.SequenceEqual(second.Prototypes.Span),"Russian parallel category profiles disagree.");
    Console.WriteLine($"Russian sampler297:7 first categories,19 geometry prototypes,{first.CompositeCategories.Length} composite categories; both bounded profiles match.");
}
Reject(()=>LegacyCursiveCategoryProfile.Load([0,0,1,0],0,out _));
Reject(()=>LegacyCursiveCategoryProfile.Load([1,2,255,255],0,out _));
using var cts=new CancellationTokenSource();cts.Cancel();
try{LegacyCursiveCategoryProfile.Load([],0,out _,cts.Token);throw new Exception("Cancellation ignored.");}catch(OperationCanceledException){}
Console.WriteLine($"PASS category profile201: absolute stream alignment, bounded reads, immutable owned data, native encounter-order composites, prototypes, malformed/truncated input, cancellation; {controls} independent controls.");
static int[] Ints(JsonElement e)=>e.EnumerateArray().Select(p=>p.GetInt32()).ToArray();
static void Require(bool ok,string why){if(!ok)throw new InvalidOperationException(why);}
static void Reject(Action action){try{action();throw new Exception("Malformed profile accepted.");}catch(InvalidDataException){}}
