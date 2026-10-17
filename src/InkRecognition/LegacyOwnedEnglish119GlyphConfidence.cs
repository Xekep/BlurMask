using System.Buffers.Binary;
using System.Runtime.CompilerServices;

[assembly:InternalsVisibleTo("LegacyEnglish119Confidence.Probe")]
namespace InkRecognition;

public readonly record struct LegacyEnglish119ConfidenceValue(ushort Symbol,ushort OutputLabel,bool SourceProbability,float Value);

/// <summary>Owned FIRST119 candidate.vector10[0] from the existing actual
/// two-family cache. This is not candidate24.field0, word confidence or text.</summary>
public sealed class LegacyOwnedEnglish119GlyphConfidence
{
    public LegacyOwnedEnglish119AcousticCache Owner {get;}
    public LegacyFirstAcoustic119WireModel Model=>Owner.Model;
    private readonly LegacyAcousticConfidenceLabelMap map;
    private readonly ushort closed;
    private readonly object gate=new();
    private readonly Dictionary<int,LegacyEnglish119ConfidenceGroup> completed=[];
    public int ScoredGroupCount {get{lock(gate)return completed.Count;}}
    private LegacyOwnedEnglish119GlyphConfidence(LegacyOwnedEnglish119AcousticCache owner,LegacyAcousticConfidenceLabelMap map,ushort closed)
    {Owner=owner;this.map=map;this.closed=closed;}

    public static LegacyOwnedEnglish119GlyphConfidence Prepare(LegacyOwnedEnglish119AcousticCache owner,CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(owner);token.ThrowIfCancellationRequested();
        var model=owner.Model;var alphabet=model.Source.Alphabet;
        byte[] A(string name)=>model.Fields.Single(f=>f.Owner=="21D"&&f.Destination==name).CopyBytes();
        if((owner.KnowledgeFlags&0x800)!=0||BinaryPrimitives.ReadInt32LittleEndian(A("108"))!=0||
            model.Slots[0].Mode!=2||model.Slots[1].Mode!=2||model.Slots.Skip(2).Any(s=>s.Networks.Count!=0))
            throw new NotSupportedException("Fresh two-family default scalar channel required.");
        byte[] header=alphabet.CopySerializedHeader();
        if(BinaryPrimitives.ReadUInt16LittleEndian(header)==65535||BinaryPrimitives.ReadUInt16LittleEndian(header.AsSpan(4))!=65535||
            !alphabet.TryGetLabel("\U000f0001",out ushort closed))
            throw new NotSupportedException("Actual EXP header and source-backed closed primitive required.");
        int count=BinaryPrimitives.ReadUInt16LittleEndian(A("C8"));var groups=new LegacySymbolMaskKnowledgeGroup[count];
        byte[] ends=count==0?[]:A("D0"),ids=count==0?[]:A("D8");
        for(int i=0;i<count;i++)
        {
            token.ThrowIfCancellationRequested();int first=BinaryPrimitives.ReadUInt16LittleEndian(ends.AsSpan(i*2)),last=BinaryPrimitives.ReadUInt16LittleEndian(ends.AsSpan((i+1)*2));
            ushort[] labels=new ushort[last-first];for(int j=0;j<labels.Length;j++)labels[j]=BinaryPrimitives.ReadUInt16LittleEndian(ids.AsSpan((first+j)*2));
            groups[i]=new(labels,BinaryPrimitives.ReadSingleLittleEndian(A($"E0[{i}]")));
        }
        var map=new LegacyAcousticConfidenceLabelMap(alphabet,header,model.Slots[0].Networks[0].OutputWidth,groups,token);
        if(map.TryResolveOutputLabel(closed,out _))throw new NotSupportedException("Closed primitive must retain its source probability service.");
        return new(owner,map,closed);
    }

    public ushort[][] CopyMappings()=>map.ExplicitMappings.OrderBy(p=>p.Key).Select(p=>new[]{p.Key,p.Value}).ToArray();
    public float ReadSymbolScore(int group,ushort symbol,CancellationToken token=default)=>ScoreGroup(group,token).ReadSymbolScore(symbol,token);
    public LegacyEnglish119ConfidenceGroup ScoreGroup(int group,CancellationToken token=default)
    {
        token.ThrowIfCancellationRequested();lock(gate)
        {
            token.ThrowIfCancellationRequested();if(completed.TryGetValue(group,out var found))return found;
            var actual=Owner.ScoreGroup(group,token);var candidates=actual.CopyCandidates();
            int[] trajectory=actual.Raw.Trajectory.CopyProbabilities(),raster=actual.Raw.Raster.CopyProbabilities();
            var values=new LegacyEnglish119ConfidenceValue[candidates.Length];
            for(int i=0;i<values.Length;i++)
            {
                token.ThrowIfCancellationRequested();ushort symbol=candidates[i].Symbol;
                if(i>=20){values[i]=new(symbol,65535,false,0);continue;}
                if(symbol==closed){values[i]=new(symbol,65535,true,ReadStoredSourceProbability(actual,token));continue;}
                if(!map.TryResolveOutputLabel(symbol,out ushort output)||output>=trajectory.Length||output>=raster.Length)
                    throw new NotSupportedException("Retained label requires an unresolved probability service; no neural out-of-bounds/default value.");
                //13E8E0 has a DOUBLE0.575 default. Math.Exp is a portable
                //transcendental adaptation, with native float stores preserved.
                float a=CalibrateProbability(trajectory[output]),b=CalibrateProbability(raster[output]);
                float sum=b+a;for(int absent=2;absent<7;absent++)sum+=0f;
                float mean=sum/2;values[i]=new(symbol,output,false,mean>=1?1:mean);
            }
            token.ThrowIfCancellationRequested();var result=new LegacyEnglish119ConfidenceGroup(this,actual,values);completed.Add(group,result);return result;
        }
    }

    internal float ReadStoredSourceProbability(LegacyEnglish119CachedGroup actual,CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(actual);token.ThrowIfCancellationRequested();
        if(!ReferenceEquals(actual.Owner,Owner)||!ReferenceEquals(actual.Raw.Owner,Owner.Owner))throw new ArgumentException("Same actual cache/source group required.");
        var ranges=actual.Raw.Features.CopyStoredRanges();if(ranges.Length==0)throw new ArgumentException("Missing actual group range.");
        var probabilities=Owner.Owner.CopyPhysicalProbabilities();int physical=ranges[0].StrokeIndex;
        if((uint)physical>=(uint)probabilities.Length||!float.IsFinite(probabilities[physical])||probabilities[physical]<0||probabilities[physical]>1)
            throw new ArgumentException("Actual original physical probability required.");
        return probabilities[physical]; //No whole-group test, sigmoid, mean or clamp.
    }
    internal static float CalibrateProbability(int probability)
    {
        if((uint)probability>32768)throw new ArgumentOutOfRangeException(nameof(probability));
        return(float)(1/(1+Math.Exp(-(double)(probability*(1f/32768))*.575)));
    }
}

public sealed class LegacyEnglish119ConfidenceGroup
{
    public LegacyOwnedEnglish119GlyphConfidence Owner {get;}
    public LegacyEnglish119CachedGroup ActualCache {get;}
    private readonly LegacyEnglish119ConfidenceValue[] values;
    internal LegacyEnglish119ConfidenceGroup(LegacyOwnedEnglish119GlyphConfidence owner,LegacyEnglish119CachedGroup actual,LegacyEnglish119ConfidenceValue[] values)
    {Owner=owner;ActualCache=actual;this.values=values.ToArray();}
    public LegacyEnglish119ConfidenceValue[] CopyValues()=>values.ToArray();
    public float ReadSymbolScore(ushort symbol,CancellationToken token=default)
    {token.ThrowIfCancellationRequested();foreach(var value in values)if(value.Symbol==symbol)return value.Value;return 0;}
}
