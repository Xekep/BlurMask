using System.Buffers.Binary;
using System.Security.Cryptography;

namespace InkRecognition;

/// <summary>Actual fresh main24D ->29B no-active-mixture branch ->265, preserving
/// original physical metadata and factor200 chronology. Long qualifying rows
/// activate native29B EM and are explicitly refused until that producer is ported.</summary>
public sealed class LegacyFreshMainOrdinaryPostprocessing
{
    public LegacyFreshMainShearPreparation Input {get;}
    public LegacyOrdinaryGapResult Gap {get;}
    public LegacyOrientationSmoothingProfile SmoothingProfile {get;}
    public LegacyOrientationStrokeMetrics OriginalSourceMetrics {get;}
    public LegacySourcePointChronology Chronology {get;}
    public IReadOnlyList<InkStroke> Strokes {get;}
    public IReadOnlyList<LegacyOrientationStrokePreparationWithKeysResult> DescriptorSmoothing {get;}
    public ReadOnlyMemory<uint> InputPointKeys200=>keys.ToArray();
    public int Processor29BOffset {get;}
    public int Processor265Offset {get;}
    private readonly uint[] keys;
    private LegacyFreshMainOrdinaryPostprocessing(LegacyFreshMainShearPreparation input,LegacyOrdinaryGapResult gap,
        LegacyOrientationSmoothingProfile profile,LegacyOrientationStrokeMetrics metrics,LegacySourcePointChronology chronology,
        InkStroke[] strokes,LegacyOrientationStrokePreparationWithKeysResult[] perDescriptor,uint[] keys,int gapOffset,int smoothingOffset)
    {Input=input;Gap=gap;SmoothingProfile=profile;OriginalSourceMetrics=metrics;Chronology=chronology;Strokes=Array.AsReadOnly(strokes);
        DescriptorSmoothing=Array.AsReadOnly(perDescriptor);this.keys=keys;Processor29BOffset=gapOffset;Processor265Offset=smoothingOffset;}

    public static LegacyFreshMainOrdinaryPostprocessing Prepare(ReadOnlySpan<byte> resource,LegacyFreshMainShearPreparation actualInput,
        CancellationToken cancellationToken=default)
    {
        ArgumentNullException.ThrowIfNull(actualInput);cancellationToken.ThrowIfCancellationRequested();
        var line=actualInput.Input;var controller=line.Owner.Controller;
        if(!Convert.ToHexString(SHA256.HashData(resource)).Equals(line.Owner.Input.Input.Input.SourceFingerprint,StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Actual pinned main0 source required.");
        int gapOffset=actualInput.Processor24DOffset+38;
        var profile=LegacyOrdinaryGapProfile.ReadV100Node(resource,gapOffset);int smoothingOffset=gapOffset+58;
        var smoothing=LegacyOrientationSmoothingProfile.ReadV102Node(resource,smoothingOffset);
        byte[][] descriptors=line.CopyDescriptors(),guides=line.CopyGuides();
        //16A130 initializes fresh secondary9C=-1. With nonzero but insufficient
        //gaps175350 preserves this field;1.5 is only the genuine zero-gap branch.
        var gap=LegacyNativeOrdinaryGapPreparation.ApplyWithoutMixture(profile,
            descriptors.Select(d=>new ReadOnlyMemory<byte>(d)).ToArray(),guides.Select(g=>new ReadOnlyMemory<byte>(g)).ToArray(),
            actualInput.Output.Rows,-1,cancellationToken);
        //15ABE0 precedes249/24B/24D. Preserve metrics of that original current
        //physical owner instead of extracting them from deskewed/sheared ink.
        var metrics=LegacySourceOrientationPreparation.ExtractMetrics(controller.Source,cancellationToken);
        var before=actualInput.Output.CopyPhysicalPoints();var strokes=new List<InkStroke>();var keys=new List<uint>();
        var outputs=new List<LegacyOrientationStrokePreparationWithKeysResult>();int nextPhysical=0,inputOffset=0;
        foreach(var d in descriptors)
        {
            int kind=BinaryPrimitives.ReadUInt16LittleEndian(d)&15;if(kind==3)continue;
            if(kind!=0)throw new NotSupportedException("Only fresh ordinary physical smoothing lineage restored.");
            int first=I(d,36),last=I(d,40),guide=I(d,16);
            if(first!=nextPhysical||last<first||last>=before.Length||guide<0||guide>=guides.Length)
                throw new ArgumentException("Complete ascending owned physical descriptor partition required.");
            float height=BinaryPrimitives.ReadSingleLittleEndian(guides[guide].AsSpan(4));
            var input=before.Skip(first).Take(last-first+1).Select(p=>new InkStroke(p)).ToArray();
            var output=LegacyOrientationStrokePreparation.PrepareWithPointKeys(input,smoothing,height,metrics,cancellationToken);
            foreach(int key in output.PointKeys200.Span)keys.Add(checked((uint)(key+inputOffset*200)));
            strokes.AddRange(output.Strokes);outputs.Add(output);inputOffset+=input.Sum(s=>s.Points.Count);nextPhysical=last+1;
            if(keys.Count>4096)throw new InvalidDataException("Bounded full265 output exceeded.");
        }
        if(nextPhysical!=before.Length||strokes.Count!=before.Length)throw new ArgumentException("Incomplete owned physical265 coverage.");
        var chronology=line.Chronology.RemapPreparedPoints(strokes.Select(s=>s.Points.Count).ToArray(),keys.ToArray(),200,cancellationToken);
        return new(actualInput,gap,smoothing,metrics,chronology,strokes.ToArray(),outputs.ToArray(),keys.ToArray(),gapOffset,smoothingOffset);
    }
    private static int I(ReadOnlySpan<byte>b,int at)=>BinaryPrimitives.ReadInt32LittleEndian(b[at..]);
}
