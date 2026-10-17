using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using InkRecognition;

if(args.Length!=2)throw new ArgumentException("models-directory capture.json");
int guards=0,tailTruncations=0;var captured=new List<ModelCapture>();var canceled=new CancellationToken(true);
void Guard(Action action)
{try{action();}catch(Exception e)when(e is ArgumentException or InvalidDataException or NotSupportedException or OperationCanceledException){guards++;return;}throw new Exception("Expected owned-wire refusal.");}
LegacySource247WireChild Selected(LegacySource247WireLayout s)=>s.Children.First(c=>c.Kind==0x249);
LegacyFirstAcoustic119WireModel Read(byte[]d){var parent=LegacySource247WireLayout.Read(d);return LegacyFirstAcoustic119WireModel.ReadSelectedFirst(d,parent,Selected(parent));}
FieldCapture Field(LegacyResourceWireField f)=>new(f.Owner,f.Destination,f.Offset,f.ElementSize,f.Count,Convert.ToBase64String(f.CopyBytes()));
foreach(string name in new[]{"en-US/en_US-ak-cur.lite.res","en-US/en_US-ak-cur.res"})
{
    byte[] data=File.ReadAllBytes(Path.Combine(args[0],name));var parent=LegacySource247WireLayout.Read(data);var selected=Selected(parent);
    var model=LegacyFirstAcoustic119WireModel.ReadSelectedFirst(data,parent,selected);var foreign=LegacySource247WireLayout.Read(data);
    if(!ReferenceEquals(model.ParentSource,parent)||!ReferenceEquals(model.SelectedProcessor,selected)||model.SourceScoreExponent!=.75f||
        model.Slots.SelectMany(s=>s.Networks).Any(n=>!ReferenceEquals(n.Source,model.Source)))throw new Exception("Lost selected-source ownership.");
    Guard(()=>LegacyFirstAcoustic119WireModel.ReadSelectedFirst(data,parent,Selected(foreign)));
    Guard(()=>LegacyFirstAcoustic119WireModel.ReadSelectedFirst(data,parent,parent.Children.First(c=>c.Kind!=0x249)));
    Guard(()=>LegacyFirstAcoustic119WireModel.ReadSelectedFirst(data,parent,selected,canceled));
    Guard(()=>LegacyFirstAcoustic119WireModel.ReadSelectedFirst(data[..^1],parent,selected));
    byte[] changed=data.ToArray();changed[selected.Offset+18]^=1;
    Guard(()=>LegacyFirstAcoustic119WireModel.ReadSelectedFirst(changed,parent,selected));
    foreach(var field in model.Fields)
    {
        byte[] copy=field.CopyBytes();if(copy.Length!=0){byte before=copy[0];copy[0]^=255;if(field.CopyBytes()[0]!=before)throw new Exception("Mutable field snapshot.");}
    }
    foreach(var network in model.Slots.SelectMany(s=>s.Networks))
    {
        byte[] copy=network.CopyBytes();copy[8]=0;if(network.CopyBytes()[8]!=0x12)throw new Exception("Mutable network snapshot.");
        for(int i=0;i<2;i++){var a=network.CopyMatrixWeights(i);sbyte before=a[0];a[0]^=127;if(network.CopyMatrixWeights(i)[0]!=before)throw new Exception("Mutable weights.");var scales=network.CopyMatrixScales(i);scales[0]=0;if(network.CopyMatrixScales(i)[0]!=1)throw new Exception("Mutable scales.");}
        foreach(int at in new[]{network.Offset+2,network.Offset+8,network.Offset+11,network.Matrices[0].ScalesOffset,network.Matrices[1].ScalesOffset+1})
        {changed=data.ToArray();changed[at]=0;Guard(()=>Read(changed));}
        Guard(()=>network.CopyMatrixWeights(2));Guard(()=>network.CopyMatrixScales(-1));
    }
    foreach(var field in model.Fields.Where(f=>f.ElementSize==1&&f.Count==1&&f.Destination is "18.initialPresent" or "10.dynamicPresent" or "00.gridPresent" or "optionalA0" or "optionalPair" or "conversionPresent"))
    {changed=data.ToArray();changed[field.Offset]=2;Guard(()=>Read(changed));}
    foreach(var field in model.Fields.Where(f=>f.Owner=="21D"&&f.Destination is "108" or "128" or "148/150.present"))
    {changed=data.ToArray();changed[field.Offset]=1;Guard(()=>Read(changed));}
    foreach(var field in model.Fields.Where(f=>f.Owner=="21D"&&f.Destination is "0C" or "100" or "120" or "124"))
    {changed=data.ToArray();BinaryPrimitives.WriteInt32LittleEndian(changed.AsSpan(field.Offset),0x7fc00000);Guard(()=>Read(changed));}
    int groupBound=model.Fields.Single(f=>f.Owner=="21D"&&f.Destination=="D0").Offset;
    changed=data.ToArray();BinaryPrimitives.WriteUInt16LittleEndian(changed.AsSpan(groupBound+4),1);Guard(()=>Read(changed));
    int groupLabels=model.Fields.Single(f=>f.Owner=="21D"&&f.Destination=="D8").Offset;
    changed=data.ToArray();BinaryPrimitives.WriteUInt16LittleEndian(changed.AsSpan(groupLabels),(ushort)model.Source.Alphabet.LabelCount);Guard(()=>Read(changed));
    byte[] aligned=new byte[data.Length+16];data.CopyTo(aligned,16);var alignedParent=LegacySource247WireLayout.Read(aligned,16);
    var shifted=LegacyFirstAcoustic119WireModel.ReadSelectedFirst(aligned,alignedParent,Selected(alignedParent));
    var originalNets=model.Slots.SelectMany(s=>s.Networks).ToArray();var shiftedNets=shifted.Slots.SelectMany(s=>s.Networks).ToArray();
    for(int i=0;i<originalNets.Length;i++)if(shiftedNets[i].Offset!=originalNets[i].Offset+16||
        !shiftedNets[i].CopyMatrixWeights(0).SequenceEqual(originalNets[i].CopyMatrixWeights(0)))throw new Exception("Original absolute alignment lost.");
    byte[] misaligned=new byte[data.Length+1];data.CopyTo(misaligned,1);var oddParent=LegacySource247WireLayout.Read(misaligned,1);
    Guard(()=>LegacyFirstAcoustic119WireModel.ReadSelectedFirst(misaligned,oddParent,Selected(oddParent)));
    //Keep all enclosing wire sizes coherent while removing the final acoustic
    //tail suffix. The delegated source remains parseable; this reader must fail.
    foreach(int at in model.Fields.Where(f=>f.Offset>=model.PrefixEnd&&f.Offset<model.AcousticChild.End).Select(f=>f.Offset).Distinct())
    {
        int removed=model.AcousticChild.End-at;changed=new byte[data.Length-removed];data.AsSpan(0,at).CopyTo(changed);
        data.AsSpan(model.AcousticChild.End).CopyTo(changed.AsSpan(at));
        foreach(int envelope in new[]{parent.Offset,selected.Offset,model.Source.Offset,model.AcousticChild.Offset})
            BinaryPrimitives.WriteUInt32LittleEndian(changed.AsSpan(envelope+4),BinaryPrimitives.ReadUInt32LittleEndian(data.AsSpan(envelope+4))-(uint)removed);
        Guard(()=>Read(changed));tailTruncations++;
    }
    captured.Add(new(name,parent.Fingerprint,selected.Offset,model.Source.Offset,model.Source.End,
        model.SourceScoreExponent,model.RawNormalizationTarget,model.CostMultiplier,model.PrefixEnd,model.Fingerprint,model.Source.Alphabet.LabelCount,
        new(model.AcousticChild.Offset,model.AcousticChild.End),model.Fields.Select(Field).ToArray(),
        model.Slots.Select(s=>new SlotCapture(s.Index,s.First,s.Second,s.Mode,s.Weight,s.Networks.Select(n=>new NetworkCapture(n.Offset,n.End,n.Kind,n.InputWidth,n.HiddenWidth,n.OutputWidth,n.Fingerprint,n.Matrices.ToArray(),
            Convert.ToBase64String(n.CopyBytes()),Enumerable.Range(0,2).Select(i=>Convert.ToHexString(SHA256.HashData(System.Runtime.InteropServices.MemoryMarshal.AsBytes(n.CopyMatrixWeights(i).AsSpan())))).ToArray(),
            Enumerable.Range(0,2).Select(i=>Convert.ToHexString(SHA256.HashData(n.CopyMatrixScales(i)))).ToArray())).ToArray())).ToArray()));
}
byte[] ru=File.ReadAllBytes(Path.Combine(args[0],"ru-RU/ru_RU-ak-cur.lite.res"));var ruParent=LegacySource247WireLayout.Read(ru);
Guard(()=>LegacyFirstAcoustic119WireModel.ReadSelectedFirst(ru,ruParent,Selected(ruParent)));
//The existing106/RU reader and network must retain their strict domain.
var old=LegacyFirstAcousticModel.ReadSource(ru,6120);if(old.TrajectoryNetwork.InputWidth!=247||old.RasterNetwork.InputWidth!=229)throw new Exception("OldRU shape changed.");
foreach(string name in new[]{"en-US/en_US-ak-cur.lite.res","en-US/en_US-ak-cur.res","en-US/en_US-ak-superimposed.lite.res","en-US/en_US-ak-superimposed.res"})
{byte[] d=File.ReadAllBytes(Path.Combine(args[0],name));var p=LegacySource247WireLayout.Read(d);Guard(()=>LegacyFirstAcousticModel.ReadSource(d,Selected(p).Offset+16));
 if(name.Contains("superimposed",StringComparison.Ordinal))Guard(()=>Read(d));else Guard(()=>LegacyInkNetwork.Load(Read(d).Slots[0].Networks[0].CopyBytes()));}
Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(args[1]))!);File.WriteAllText(args[1],JsonSerializer.Serialize(new Capture("Wire-only actual249 FIRST119/107; no evaluator/24C/controller",captured,guards,tailTruncations),WireJson.Default.Capture)+"\n");
Console.WriteLine($"PASS2 actual first119 owners/{guards} guards/{tailTruncations} coherent-tail truncations; oldRU guards preserved");

sealed record FieldCapture(string Owner,string Destination,int Offset,int ElementSize,int Count,string Raw);
sealed record EnvelopeCapture(int Offset,int End);
sealed record ModelCapture(string Path,string ParentFingerprint,int Processor,int Source,int SourceEnd,float SourceScoreExponent,
    short RawNormalizationTarget,float CostMultiplier,int PrefixEnd,string Fingerprint,int AlphabetCount,EnvelopeCapture Acoustic,FieldCapture[] Fields,SlotCapture[] Slots);
sealed record SlotCapture(int Index,bool First,bool Second,byte Mode,float? Weight,NetworkCapture[] Networks);
sealed record NetworkCapture(int Offset,int End,ushort Kind,int InputWidth,int HiddenWidth,int OutputWidth,string Fingerprint,
    LegacyNetwork107Matrix[] Matrices,string Raw,string[] WeightHashes,string[] ScaleHashes);
sealed record Capture(string Scope,List<ModelCapture> Models,int Guards,int TailTruncations);
[JsonSourceGenerationOptions(WriteIndented=true)]
[JsonSerializable(typeof(Capture))]
partial class WireJson:JsonSerializerContext;
