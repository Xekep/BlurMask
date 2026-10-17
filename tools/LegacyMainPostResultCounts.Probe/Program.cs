using InkRecognition;
using System.Buffers.Binary;
using System.Text.Json;
if(args.Length!=8)throw new ArgumentException("AK engine separation case publication line30 line50 capture");
byte[] ak=File.ReadAllBytes(args[0]),line30=File.ReadAllBytes(args[5]),line50=File.ReadAllBytes(args[6]);
using var engine=JsonDocument.Parse(File.ReadAllBytes(args[1]));var a=engine.RootElement.GetProperty("activations");
var activations=new LegacyInkActivations(a.GetProperty("hidden").EnumerateArray().Select(v=>v.GetInt32()).ToArray(),a.GetProperty("output").EnumerateArray().Select(v=>v.GetInt32()).ToArray());
var table=LegacyCursiveSeparationTable.Load(File.ReadAllBytes(args[2]));using var cf=File.OpenRead(args[3]);using var pf=File.OpenRead(args[4]);
var cd=LegacyNativeCaseData.Load(cf);var pd=LegacyNativeTextPublicationData.Load(pf);
var shear=LegacyOrdinaryShearProfile.ReadV100Node(ak,188012);var gap=LegacyOrdinaryGapProfile.ReadV100Node(ak,188050);
int prepared=0,guards=0,zero=0,oversized=0,under=0,bounded=0;
using var file=File.Create(args[7]);using var w=new Utf8JsonWriter(file);w.WriteStartObject();
w.WriteStartArray("profile");foreach(int x in new[]{(int)shear.MinimumGaps,shear.MaximumGaps,gap.MinimumGaps,gap.MaximumGaps,BitConverter.SingleToInt32Bits(gap.ZeroGap)})w.WriteNumberValue(x);w.WriteEndArray();
w.WriteStartArray("prepared");int[] values=[0,1,2,3,10,11,12,31,64];
for(int length=1;length<=4;length++)
{
    int combinations=(int)Math.Pow(values.Length,length);
    for(int code=0;code<combinations;code++)
    {
        int index=code,at=0;var counts=new int[length];var descriptors=new byte[length][];
        for(int i=0;i<length;i++){counts[i]=values[index%values.Length];index/=values.Length;descriptors[i]=Descriptor(at,at+counts[i]);at+=counts[i];}
        var result=LegacyFreshMainPostResultCountPolicy.EvaluateDescriptors(descriptors,at,1,shear,gap);
        if(result.GapBranch==LegacyNativeGapAllocationBranch.ZeroShortcut){zero++;if(result.ReadNativeZeroGap()!=gap.ZeroGap)throw new Exception("ZeroGap field differs.");result.RequireBoundedGapAllocation();}
        else if(result.GapBranch==LegacyNativeGapAllocationBranch.BoundedObservationBuffers){bounded++;result.RequireBoundedGapAllocation();}
        else {if(result.GapBranch==LegacyNativeGapAllocationBranch.RefusedOversizedAllocation)oversized++;else under++;Reject(result.RequireBoundedGapAllocation);}
        Write(counts,result);prepared++;
    }
}
//Native counts skip unassigned ordinary and kind3; kind6 remains an ordinary
//prepared-count branch, but is not accepted by the fresh owning factory.
foreach(int kind in new[]{0,3,6})foreach(int line in new[]{-1,0})
{
    var d=Descriptor(0,0,kind,line);var result=LegacyFreshMainPostResultCountPolicy.EvaluateDescriptors([d],0,1,shear,gap);
    w.WriteStartObject();w.WriteStartArray("counts");w.WriteNumberValue(0);w.WriteEndArray();w.WriteNumber("kind",kind);w.WriteNumber("line",line);Fields(result);w.WriteEndObject();prepared++;
}
w.WriteEndArray();w.WriteStartArray("actual");
var samples=new[]{Word(0,0),Word(0,250).Concat(Word(0,0)).ToArray(),Word(0,0).Concat(Word(230,0)).ToArray(),Word(0,0).Concat(Word(300,0)).ToArray(),Word(0,0).Concat(Word(450,0)).ToArray(),Word(0,0).Concat(Word(700,0)).ToArray(),Hi(0).Concat(Hi(220)).ToArray()};
foreach(var raw in samples)
{
    var input=LegacyFreshMainOwnedTextInputPreparation.Prepare(ak,line30,line50,raw,0,activations,table,cd,pd).Run();
    var policy=LegacyFreshMainPostResultCountPolicy.Prepare(ak,input.Lines);
    if(!ReferenceEquals(policy.Source,input.Lines)||policy.Processor24DOffset!=188012||policy.Processor29BOffset!=188050||policy.Counts.EmptyDescriptorCount!=0)
        throw new Exception("Actual count owner/profile changed.");
    policy.Counts.RequireBoundedGapAllocation();
    var counts=input.Lines.CopyDescriptors().Where(d=>(BinaryPrimitives.ReadUInt16LittleEndian(d)&15)==0)
        .Select(d=>BinaryPrimitives.ReadInt32LittleEndian(d.AsSpan(56))-BinaryPrimitives.ReadInt32LittleEndian(d.AsSpan(52))).ToArray();
    Write(counts,policy.Counts);
    Reject(()=>LegacyFreshMainPostResultCountPolicy.Prepare(ak,input.Lines,new(true)));
    var foreign=ak.ToArray();foreign[^1]^=1;Reject(()=>LegacyFreshMainPostResultCountPolicy.Prepare(foreign,input.Lines));
}
w.WriteEndArray();
Reject(()=>LegacyFreshMainPostResultCountPolicy.EvaluateDescriptors([new byte[87]],0,1,shear,gap));
Reject(()=>LegacyFreshMainPostResultCountPolicy.EvaluateDescriptors([Descriptor(1,0)],0,1,shear,gap));
Reject(()=>LegacyFreshMainPostResultCountPolicy.EvaluateDescriptors([Descriptor(0,1)],0,1,shear,gap));
Reject(()=>LegacyFreshMainPostResultCountPolicy.EvaluateDescriptors([Descriptor(0,0,2)],0,1,shear,gap));
Reject(()=>LegacyFreshMainPostResultCountPolicy.EvaluateDescriptors([Descriptor(0,0)],0,1,shear,gap,new(true)));
w.WriteNumber("preparedCount",prepared);w.WriteNumber("zero",zero);w.WriteNumber("bounded",bounded);w.WriteNumber("oversized",oversized);w.WriteNumber("underCapacity",under);w.WriteNumber("guards",guards);w.WriteEndObject();w.Flush();
Console.WriteLine($"PASS{prepared} prepared UINT count states ({zero} zero-shortcuts/{bounded} bounded/{oversized} oversized/{under} undercapacity);7 actual source owners/{guards} guards. Scalar policy only; old24D/29B geometry APIs unchanged.");
void Reject(Action call){try{call();}catch(Exception e)when(e is ArgumentException or NotSupportedException or OperationCanceledException){guards++;return;}throw new Exception("Invalid count policy accepted");}
void Write(int[] counts,LegacyNativeMainPostResultCounts result){w.WriteStartObject();w.WriteStartArray("counts");foreach(int x in counts)w.WriteNumberValue(x);w.WriteEndArray();Fields(result);w.WriteEndObject();}
void Fields(LegacyNativeMainPostResultCounts c){w.WriteNumber("shearCount",c.ShearGapCount);w.WriteNumber("total",c.GapAllocationCount);w.WriteNumber("qualified",c.QualifiedAllocationCount);w.WriteNumber("produced",c.ProducedGapCount);w.WriteNumber("producedQualified",c.ProducedQualifiedGapCount);w.WriteNumber("empty",c.EmptyDescriptorCount);w.WriteNumber("shearBlendBits",BitConverter.SingleToInt32Bits(c.ShearInitialBlend));w.WriteNumber("gapBlendBits",BitConverter.SingleToInt32Bits(c.GapInitialBlend));w.WriteNumber("branch",(int)c.GapBranch);if(c.GapBranch==LegacyNativeGapAllocationBranch.ZeroShortcut)w.WriteNumber("zeroGapBits",BitConverter.SingleToInt32Bits(c.ReadNativeZeroGap()));}
static byte[] Descriptor(int first,int end,int kind=0,int line=0){var d=new byte[88];BinaryPrimitives.WriteUInt16LittleEndian(d,(ushort)kind);BinaryPrimitives.WriteInt32LittleEndian(d.AsSpan(16),line);BinaryPrimitives.WriteInt32LittleEndian(d.AsSpan(52),first);BinaryPrimitives.WriteInt32LittleEndian(d.AsSpan(56),end);return d;}
static InkStroke[] Word(float x,float y)=>[
 new(new InkPoint[]{new(x,y+90),new(x,y+90),new(x,y+10),new(x+25,y+60),new(x+50,y+10),new(x+50,y+90)}),
 new(new InkPoint[]{new(x+70,y+10),new(x+70,y+90),new(x+120,y+10),new(x+120,y+90)}),
 new(new InkPoint[]{new(x+140,y+90),new(x+140,y+10),new(x+180,y+10),new(x+190,y+20),new(x+190,y+40),new(x+180,y+50),new(x+140,y+50)})];
static InkStroke[] Hi(float x)=>[
 new(new InkPoint[]{new(x,10),new(x,90)}),new(new InkPoint[]{new(x+40,10),new(x+40,90)}),new(new InkPoint[]{new(x,50),new(x+40,50)}),new(new InkPoint[]{new(x+70,10),new(x+70,90)})];
