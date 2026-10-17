using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using InkRecognition;

if(args.Length!=2)throw new ArgumentException("models-directory capture.json");
int guards=0;void Guard(Action action)
{
    try{action();}catch(Exception e)when(e is ArgumentException or InvalidDataException or NotSupportedException or OperationCanceledException)
    {guards++;return;}throw new Exception("Expected bounded reader/owner refusal.");
}
var canceled=new CancellationToken(true);var captures=new List<WireSourceCapture>();
string[] names=["en-US/en_US-ak-cur.lite.res","en-US/en_US-ak-cur.res","en-US/en_US-ak-superimposed.lite.res",
    "en-US/en_US-ak-superimposed.res","ru-RU/ru_RU-ak-cur.lite.res"];
WireFieldCapture Field(LegacyResourceWireField f)=>new(f.Owner,f.Destination,f.Offset,f.ElementSize,f.Count,Convert.ToBase64String(f.CopyBytes()));
int positive=0,refused=0,parity=0;
foreach(string name in names)
{
    byte[] data=File.ReadAllBytes(Path.Combine(args[0],name));
    var source=LegacySource247WireLayout.Read(data);var foreign=LegacySource247WireLayout.Read(data);
    var sampler=source.Children.Single(c=>c.Destination=="247.130");
    var other=source.Children.First(c=>c.Kind!=0x297);
    Guard(()=>source.ReadSampler(foreign.Children.Single(c=>c.Destination=="247.130")));
    Guard(()=>source.CopyChildEnvelopeBytes(foreign.Children[0]));
    Guard(()=>source.ReadSampler(other));Guard(()=>source.ReadSampler(null!));
    Guard(()=>LegacySource247WireLayout.Read(data,token:canceled));
    Guard(()=>source.ReadSampler(sampler,canceled));Guard(()=>source.CopyChildEnvelopeBytes(sampler,canceled));
    Guard(()=>LegacySource247WireLayout.Read(data,-1));
    byte[] changed=data.ToArray();BinaryPrimitives.WriteUInt16LittleEndian(changed.AsSpan(14),0x10f);
    Guard(()=>LegacySource247WireLayout.Read(changed));
    foreach(int length in source.Fields.Select(f=>f.Offset).Concat(source.Children.SelectMany(c=>new[]{c.Offset,c.End-1})).Distinct())
    {
        if(length<16||length>=data.Length)continue;
        byte[] cut=data.AsSpan(0,length).ToArray();BinaryPrimitives.WriteUInt32LittleEndian(cut.AsSpan(4),(uint)(length-8));
        Guard(()=>LegacySource247WireLayout.Read(cut));
    }
    foreach(var flag in source.Fields.Where(f=>(f.Owner=="247"&&(f.Destination.EndsWith(".present")||f.Destination is "B8" or "B9" or "trioPresent"))||
        (f.Owner=="parametersC0"&&f.Destination is "20" or "21" or "22" or "legacyThirdPresence" or "40" or "41" or "48" or "50")))
    {changed=data.ToArray();changed[flag.Offset]=2;Guard(()=>LegacySource247WireLayout.Read(changed));}
    foreach(var child in source.Children)
    {
        changed=data.ToArray();changed[child.Offset+12]=1;Guard(()=>LegacySource247WireLayout.Read(changed));
        changed=data.ToArray();BinaryPrimitives.WriteUInt32LittleEndian(changed.AsSpan(child.Offset+4),uint.MaxValue);
        Guard(()=>LegacySource247WireLayout.Read(changed));
        byte[] copy=source.CopyChildEnvelopeBytes(child);copy[0]^=255;
        if(source.CopyChildEnvelopeBytes(child)[0]!=0x90)throw new Exception("Public child bytes escaped owner.");
    }
    foreach(var count in source.Fields.Where(f=>f.Count==1&&f.ElementSize==4&&f.Destination.EndsWith(".count")))
    {changed=data.ToArray();BinaryPrimitives.WriteInt32LittleEndian(changed.AsSpan(count.Offset),257);Guard(()=>LegacySource247WireLayout.Read(changed));}
    foreach(var field in source.Fields)
    {byte[] copy=field.CopyBytes();if(copy.Length!=0){copy[0]^=255;if(field.CopyBytes()[0]==copy[0])throw new Exception("Field escaped immutable owner.");}}
    WireSamplerCapture? samplerCapture=null;string? error=null;
    try
    {
        var profile=source.ReadSampler(sampler);positive++;
        if(!ReferenceEquals(profile.Source,source)||!ReferenceEquals(profile.Child,sampler))throw new Exception("Sampler source owner lost.");
        var categories=profile.ReadCategories();
        Guard(()=>profile.ReadCategories(canceled));
        //A caller may recover public category arrays, so the owner reparses its private snapshot.
        if(System.Runtime.InteropServices.MemoryMarshal.TryGetArray(categories.FirstMap,out ArraySegment<sbyte> map)&&map.Count!=0)
        {
            sbyte original=map.Array![map.Offset];map.Array[map.Offset]=unchecked((sbyte)(original+1));
            if(profile.ReadCategories().FirstMap.Span[0]!=original)throw new Exception("Mutable public categories escaped wire owner.");
        }
        foreach(var field in profile.Fields.Where(f=>f.Destination is "134.present" or "1D0.present" or "18" or "19" or "1A" or "40.categoryPresent"))
        {
            changed=data.ToArray();changed[field.Offset]=2;
            var bad=LegacySource247WireLayout.Read(changed);Guard(()=>bad.ReadSampler(bad.Children.Single(c=>c.Destination=="247.130")));
        }
        foreach(int remove in new[]{1,2,5,16,64,143,189,213,214})
        {
            //Keep the parent and following delegated children structurally valid;
            //only the sampled child's scalar tail is truncated.
            byte[] cut=new byte[data.Length-remove];data.AsSpan(0,sampler.End-remove).CopyTo(cut);
            data.AsSpan(sampler.End).CopyTo(cut.AsSpan(sampler.End-remove));
            BinaryPrimitives.WriteUInt32LittleEndian(cut.AsSpan(4),(uint)(cut.Length-8));
            BinaryPrimitives.WriteUInt32LittleEndian(cut.AsSpan(sampler.Offset+4),(uint)(sampler.End-sampler.Offset-8-remove));
            var bad=LegacySource247WireLayout.Read(cut);Guard(()=>bad.ReadSampler(bad.Children.Single(c=>c.Destination=="247.130")));
        }
        samplerCapture=new(profile.CategoryOffset,categories.BaseEntryCount,
            profile.Fields.Select(Field).ToArray(),profile.ReadCategories().FirstIds.ToArray(),
            profile.ReadCategories().CompositeCategories.Length);
    }
    catch(NotSupportedException e)
    {
        refused++;error=e.Message;
        if(name.EndsWith("cur.lite.res"))throw;
    }
    if(source.Version==0x10d)
    {
        var old=LegacyOrdinarySourceInk.ReadV10dResourceProfile(data);
        if(old.SourceKind!=source.SourceKind||old.ParametersOffset!=source.ParametersOffset||old.EnclosureFlagOffset!=source.EnclosureFlagOffset)
            throw new Exception("Old RU prefix parity changed.");
        var oldCategories=LegacySourceSamplerCategoryOwner.ReadV10dSource(data,0);
        if(oldCategories.SamplerNodeOffset!=sampler.Offset||oldCategories.CategoryProfileOffset!=source.ReadSampler(sampler).CategoryOffset)
            throw new Exception("Old category owner parity changed.");
        var current=source.ReadSampler(sampler).ReadCategories();var previous=oldCategories.Categories;
        if(!current.FirstIds.Span.SequenceEqual(previous.FirstIds.Span)||!current.GeometryIds.Span.SequenceEqual(previous.GeometryIds.Span)||
            !current.FirstMap.Span.SequenceEqual(previous.FirstMap.Span)||!current.StartGeometryMap.Span.SequenceEqual(previous.StartGeometryMap.Span)||
            !current.EndGeometryMap.Span.SequenceEqual(previous.EndGeometryMap.Span)||!current.Prototypes.Span.SequenceEqual(previous.Prototypes.Span)||
            !current.CompositeCategories.Span.SequenceEqual(previous.CompositeCategories.Span)||
            !current.StartCompositeMap.Span.SequenceEqual(previous.StartCompositeMap.Span)||!current.EndCompositeMap.Span.SequenceEqual(previous.EndCompositeMap.Span)||
            !current.ZeroStartCompositeMap.Span.SequenceEqual(previous.ZeroStartCompositeMap.Span)||!current.ZeroEndCompositeMap.Span.SequenceEqual(previous.ZeroEndCompositeMap.Span))
            throw new Exception("Old RU category maps/prototypes differ.");
        parity++;
    }
    else
    {
        Guard(()=>LegacyOrdinarySourceInk.ReadV10dResourceProfile(data));
        Guard(()=>LegacySourceSamplerCategoryOwner.ReadV10dSource(data,0));
    }
    //A selected source at a nonzero parent offset preserves absolute stream alignment and IDs.
    const int prefix=12;byte[] shifted=new byte[prefix+data.Length];data.CopyTo(shifted,prefix);
    var rebased=LegacySource247WireLayout.Read(shifted,prefix);
    if(rebased.Children.Where((child,i)=>child.Offset!=source.Children[i].Offset+prefix||child.End!=source.Children[i].End+prefix).Any())
        throw new Exception("Delegated child absolute offsets changed.");
    if(samplerCapture is not null)
    {
        var shiftedSampler=rebased.ReadSampler(rebased.Children.Single(c=>c.Destination=="247.130"));
        if(shiftedSampler.CategoryOffset!=source.ReadSampler(sampler).CategoryOffset+prefix||
            !shiftedSampler.ReadCategories().FirstMap.Span.SequenceEqual(source.ReadSampler(sampler).ReadCategories().FirstMap.Span))
            throw new Exception("Original stream category alignment/owner changed.");
    }
    var expectedSourceSha=source.Fingerprint;
    data.AsSpan().Fill(0);
    if(source.Fingerprint!=expectedSourceSha||source.CopyChildEnvelopeBytes(sampler)[0]!=0x90)throw new Exception("Caller resource mutated private owner.");
    captures.Add(new(name,source.Fingerprint,source.Offset,source.End,source.Version,source.Base134Version,
        source.SourceKind,source.CoordinateMode,source.ParametersOffset,source.EnclosureFlagOffset,source.Alphabet.LabelCount,
        source.Fields.Select(Field).ToArray(),source.Children.Select(c=>new WireChildCapture(c.Destination,c.Offset,c.End,c.Kind,c.Version)).ToArray(),
        samplerCapture,error));
}
if(positive!=2||refused!=3||parity!=1)throw new Exception("Actual resource scope changed.");
File.WriteAllText(args[1],JsonSerializer.Serialize(new WireCapture(captures,positive,refused,parity,guards),WireJson.Default.WireCapture));
Console.WriteLine($"PASS5 actual source247 layouts/{positive} complete known samplers/{refused} explicit positive-payload refusals/{parity} oldRU prefix+category parity/{guards} guards; no executable English ingress");

sealed record WireFieldCapture(string owner,string destination,int offset,int size,int count,string raw);
sealed record WireChildCapture(string destination,int Offset,int End,uint Kind,ushort Version);
sealed record WireSamplerCapture(int categoryOffset,int count,WireFieldCapture[] fields,int[] categoryFirstIds,int categoryCompositeCount);
sealed record WireSourceCapture(string path,string sha256,int Offset,int End,ushort Version,ushort Base134Version,int SourceKind,
    byte CoordinateMode,int ParametersOffset,int EnclosureFlagOffset,int alphabetCount,WireFieldCapture[] fields,WireChildCapture[] children,
    WireSamplerCapture? sampler,string? samplerError);
sealed record WireCapture(List<WireSourceCapture> sources,int positiveSamplers,int refusedSamplerPayloads,int oldRuParity,int guards);
[JsonSourceGenerationOptions(WriteIndented=true)]
[JsonSerializable(typeof(WireCapture))]
partial class WireJson:JsonSerializerContext;
