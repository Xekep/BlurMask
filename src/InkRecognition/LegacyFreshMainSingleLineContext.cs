using System.Buffers.Binary;
using System.Security.Cryptography;

namespace InkRecognition;

public sealed record LegacyMainLineContextWindow(int StartPhysical,int EndPhysical,float CenterX,
    ReadOnlyMemory<float> Features,ReadOnlyMemory<short> FixedInput,ReadOnlyMemory<int> Probability32768,bool Accepted);

/// <summary>Actual main189734/227 lazy1BE930 to1C9870 context on source points
/// AFTER sampler193380 source replacement. Uses the main1897557001 network,
/// actual assigned guide and version106 sampler's disabled custom scope
/// (196500 resource134 to18D590 runtime242 to1964A0). No first-row transplant
/// or neutral context substitution.</summary>
public sealed class LegacyFreshMainSingleLineContext
{
    private readonly LegacyPriorContextGroup[] groups;
    private readonly NativeProfile profile;
    public LegacyFreshMainSingleLineSampled Source {get;}
    public IReadOnlyList<LegacyPriorContextGroup> Groups {get;}
    public IReadOnlyList<LegacyMainLineContextWindow> Windows {get;}
    public int NetworkOffset=>189755;
    public int ConfigurationOffset=>195746;
    public int Selector=>profile.Selector;
    private LegacyFreshMainSingleLineContext(LegacyFreshMainSingleLineSampled source,NativeProfile profile,
        LegacyPriorContextGroup[] groups,LegacyMainLineContextWindow[] windows)
    {Source=source;this.profile=profile;this.groups=groups;Groups=Array.AsReadOnly(groups);Windows=Array.AsReadOnly(windows);}

    public static LegacyFreshMainSingleLineContext Prepare(ReadOnlySpan<byte> resource,LegacyFreshMainSingleLineSampled source,
        LegacyInkActivations activations,CancellationToken cancellationToken=default)
    {
        ArgumentNullException.ThrowIfNull(source);ArgumentNullException.ThrowIfNull(activations);cancellationToken.ThrowIfCancellationRequested();
        if(!Convert.ToHexString(SHA256.HashData(resource)).Equals(source.Owner.CategoryOwner.SourceFingerprint,StringComparison.OrdinalIgnoreCase)||source.Owner.AcousticOffset!=189734)
            throw new ArgumentException("Actual main source/initial context network owner required.");
        var network=LegacyInkNetwork.Load(resource[189755..195746]);
        if(network.Kind!=0x7001||network.InputWidth!=30||network.OutputWidth!=28)throw new InvalidDataException("Actual main30-to28 EXP context network required.");
        var b=resource.Slice(195746,24);float offset=F(b,0),left=F(b,4),right=F(b,8),cutoff=F(b,16);ushort blank=BinaryPrimitives.ReadUInt16LittleEndian(b[14..]);
        if(!float.IsFinite(offset)||!float.IsFinite(left)||left<=0||!float.IsFinite(right)||right<=0||b[12]!=0||b[13]!=3||blank!=28||
            !float.IsFinite(cutoff)||cutoff<0||cutoff>1||b[20]!=1||b[21]!=2||b[22]!=0||b[23]!=1)
            throw new InvalidDataException("Checked native main227 geometry/window/selector profile required.");
        var profile=new NativeProfile(offset,left,right,b[13],blank,cutoff,b[21]);
        var counts=source.Cuts.PreparedInk.StrokePointCounts.ToArray();var current=new InkStroke[counts.Length];int index=0;
        for(int s=0;s<current.Length;s++)
        {
            var points=new InkPoint[counts[s]];for(int p=0;p<points.Length;p++){var v=source.Cuts.PreparedInk.Points.Span[index++];points[p]=new(v.X,v.Y);}
            current[s]=new(points);
        }
        var groups=new List<LegacyPriorContextGroup>();var windows=new List<LegacyMainLineContextWindow>();float reference=-float.MaxValue;
        for(int start=1;start<current.Length;start++)
        {
            foreach(var p in current[start-1].Points)reference=Math.Max(reference,p.X);
            float sum=0;int count=0;
            for(int n=1;n<=profile.MaximumWindowStrokes&&start+n<=current.Length;n++)
            {
                cancellationToken.ThrowIfCancellationRequested();var window=current.Skip(start).Take(n).ToArray();
                foreach(var p in current[start+n-1].Points){sum+=p.X;count++;}
                var geometry=LegacyFloatContextGeometry.Extract(window,source.Owner.SourceHeight,reference,source.Owner.SourceBaseline,false,false,cancellationToken);
                var input=LegacyFloatInputQuantization.Quantize(geometry,10,cancellationToken);
                var probabilities=LegacyClassScores.Convert(network.Evaluate(input,activations,cancellationToken:cancellationToken),cancellationToken).Probability32768.ToArray();
                float mass=0;var values=new float[28];for(int i=0;i<values.Length;i++){values[i]=probabilities[i]*(1f/32768);if(i!=blank-1)mass+=values[i];}
                bool accepted=cutoff<mass;float center=sum/count;
                windows.Add(new(start,start+n-1,center,geometry,input,probabilities,accepted));
                if(accepted)groups.Add(new(source.Owner.SourceDescriptorOrdinal,-1,start,start+n-1,center,values));
            }
        }
        return new(source,profile,groups.ToArray(),windows.ToArray());
    }

    public short[] GetContext(int groupIndex,CancellationToken cancellationToken=default)
    {
        cancellationToken.ThrowIfCancellationRequested();var graph=Source.SourceRanges.Graph;
        if((uint)groupIndex>=(uint)graph.Groups.Count)throw new ArgumentOutOfRangeException(nameof(groupIndex));
        var ranges=graph.Groups[groupIndex].Ranges.Span;var counts=Source.Cuts.PreparedInk.StrokePointCounts.Span;
        var starts=new int[counts.Length];for(int i=1;i<starts.Length;i++)starts[i]=starts[i-1]+counts[i-1];
        float sum=0;int count=0;var physical=new int[ranges.Length];
        for(int i=0;i<ranges.Length;i++)
        {
            physical[i]=ranges[i].StrokeIndex;
            for(int p=ranges[i].StartPoint;p<ranges[i].EndPointExclusive;p++)
            {cancellationToken.ThrowIfCancellationRequested();sum+=Source.Cuts.PreparedInk.Points.Span[starts[physical[i]]+p].X;count++;}
        }
        if(count==0)throw new InvalidDataException("Native context target has no actual points.");
        var selected=LegacyPriorContextSelection.SelectNative(groups,Source.Owner.SourceDescriptorOrdinal,physical,LegacyNativePriorContextMode.SameLineAll,
            cancellationToken:cancellationToken);
        return LegacyFloatInputQuantization.Quantize(LegacyContextWeighting.Combine(selected,sum/count,Source.Owner.SourceHeight,
            profile.LeftGapOffset,profile.LeftVariance,profile.RightVariance,cancellationToken),cancellationToken:cancellationToken);
    }
    private static float F(ReadOnlySpan<byte>b,int at)=>BinaryPrimitives.ReadSingleLittleEndian(b[at..]);
    private sealed record NativeProfile(float LeftGapOffset,float LeftVariance,float RightVariance,
        byte MaximumWindowStrokes,ushort NativeBlankClass,float NonblankCutoff,byte Selector);
}
