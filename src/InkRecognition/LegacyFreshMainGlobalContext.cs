using System.Buffers.Binary;
using System.Security.Cryptography;

namespace InkRecognition;

public sealed record LegacyMainDescriptorContextWindow(int DescriptorOrdinal,int LineId,
    int StartPhysical,int EndPhysical,float CenterX,ReadOnlyMemory<float> Features,
    ReadOnlyMemory<short> FixedInput,ReadOnlyMemory<int> Probability32768,bool Accepted);

/// <summary>Fresh ordinary1C9870 lazy227 context over the actual shared main
/// cut source. Each descriptor resets its reference and uses its own guide;
/// native context owner is the descriptor ordinal, not the guide/line ID.</summary>
public sealed class LegacyFreshMainGlobalContext
{
    private readonly LegacyPriorContextGroup[] groups;
    private readonly LegacyMainDescriptorContextWindow[] windows;
    private readonly InkStroke[] strokes;
    private readonly Profile profile;
    public LegacyFreshMainGlobalSourceRanges Source {get;}
    public LegacyFreshMainCompactOrdinaryGraph Graph=>Source.Graph;
    public int NetworkOffset=>189755;
    public int ConfigurationOffset=>195746;
    private LegacyFreshMainGlobalContext(LegacyFreshMainGlobalSourceRanges source,Profile profile,
        InkStroke[] strokes,LegacyPriorContextGroup[] groups,LegacyMainDescriptorContextWindow[] windows)
    {Source=source;this.profile=profile;this.strokes=strokes;this.groups=groups;this.windows=windows;}
    public LegacyPriorContextGroup[] CopyGroups()=>groups.Select(g=>g with{Probabilities=g.Probabilities.ToArray()}).ToArray();
    public LegacyMainDescriptorContextWindow[] CopyWindows()=>windows.Select(g=>g with{
        Features=g.Features.ToArray(),FixedInput=g.FixedInput.ToArray(),Probability32768=g.Probability32768.ToArray()}).ToArray();

    public static LegacyFreshMainGlobalContext Prepare(ReadOnlySpan<byte> resource,LegacyFreshMainGlobalSourceRanges source,
        LegacyInkActivations activations,CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(source);ArgumentNullException.ThrowIfNull(activations);token.ThrowIfCancellationRequested();
        var cutSource=source.Graph.Source.Source;
        if(resource.Length!=572763||!Convert.ToHexString(SHA256.HashData(resource)).Equals(cutSource.SourceFingerprint,StringComparison.OrdinalIgnoreCase)||cutSource.AcousticOffset!=189734)
            throw new ArgumentException("Same actual main source/resource owner required for lazy227.");
        var network=LegacyInkNetwork.Load(resource[189755..195746]);
        if(network.Kind!=0x7001||network.InputWidth!=30||network.OutputWidth!=28)throw new InvalidDataException("Actual main30-to28 EXP context network required.");
        var b=resource.Slice(195746,24);float offset=F(b,0),left=F(b,4),right=F(b,8),cutoff=F(b,16);ushort blank=BinaryPrimitives.ReadUInt16LittleEndian(b[14..]);
        if(!float.IsFinite(offset)||!float.IsFinite(left)||left<=0||!float.IsFinite(right)||right<=0||b[12]!=0||b[13]!=3||blank!=28||
            !float.IsFinite(cutoff)||cutoff<0||cutoff>1||b[20]!=1||b[21]!=2||b[22]!=0||b[23]!=1)
            throw new InvalidDataException("Checked native main227 geometry/window/selector profile required.");
        var profile=new Profile(offset,left,right,b[13],blank,cutoff);
        var current=cutSource.CopyStrokes().Select(s=>new InkStroke(s.Points.ToArray())).ToArray();
        var groups=new List<LegacyPriorContextGroup>();var windows=new List<LegacyMainDescriptorContextWindow>();
        foreach(var step in cutSource.Steps)
        {
            token.ThrowIfCancellationRequested();if(step.IsPendingSeparator)continue;
            var descriptor=step.CopyDescriptor();
            //Fresh no-prior source: native cached-window reuse and positive
            //copied-prefix50 branches are separate from this owning factory.
            if(step.Kind!=0||I(descriptor,76)>=0||I(descriptor,80)>0)
                throw new NotSupportedException("Fresh ordinary negative4C/nonpositive50 context ownership required.");
            float reference=-float.MaxValue;
            for(int start=step.FirstPhysical+1;start<=step.LastPhysical;start++)
            {
                foreach(var p in current[start-1].Points)reference=Math.Max(reference,p.X);
                float sum=0;int count=0;
                for(int n=1;n<=profile.MaximumWindowStrokes&&start+n-1<=step.LastPhysical;n++)
                {
                    token.ThrowIfCancellationRequested();var window=current.Skip(start).Take(n).ToArray();
                    foreach(var p in current[start+n-1].Points){sum+=p.X;count++;}
                    var geometry=LegacyFloatContextGeometry.Extract(window,step.Height,reference,step.Baseline,false,false,token);
                    var input=LegacyFloatInputQuantization.Quantize(geometry,10,token);
                    var probabilities=LegacyClassScores.Convert(network.Evaluate(input,activations,cancellationToken:token),token).Probability32768.ToArray();
                    float mass=0;var values=new float[28];for(int i=0;i<values.Length;i++){values[i]=probabilities[i]*(1f/32768);if(i!=blank-1)mass+=values[i];}
                    bool accepted=cutoff<mass;float center=sum/count;
                    if(windows.Count>=1024)throw new ArgumentException("Portable context window budget exceeded.");
                    windows.Add(new(step.DescriptorOrdinal,step.LineId,start,start+n-1,center,geometry,input,probabilities,accepted));
                    if(accepted)groups.Add(new(step.DescriptorOrdinal,-1,start,start+n-1,center,values));
                }
            }
        }
        token.ThrowIfCancellationRequested();return new(source,profile,current,groups.ToArray(),windows.ToArray());
    }

    public short[] GetContext(int groupIndex,CancellationToken token=default)
    {
        token.ThrowIfCancellationRequested();if((uint)groupIndex>=(uint)Graph.State.Groups.Count)throw new ArgumentOutOfRangeException(nameof(groupIndex));
        var group=Graph.State.Groups[groupIndex];var owner=Graph.ResolveGroupSource(groupIndex,token).Primary;
        var ranges=group.CopyRanges();float sum=0;int count=0;
        foreach(var r in ranges)
        {
            if((uint)r.StrokeIndex>=(uint)strokes.Length||r.StartPoint<0||r.EndPointExclusive<=r.StartPoint||r.EndPointExclusive>strokes[r.StrokeIndex].Points.Count)
                throw new InvalidDataException("Actual shared group range ownership differs.");
            for(int p=r.StartPoint;p<r.EndPointExclusive;p++)
            {token.ThrowIfCancellationRequested();if(count>=1_048_576)throw new ArgumentException("Portable context point work budget exceeded.");sum+=strokes[r.StrokeIndex].Points[p].X;count++;}
        }
        if(count==0)throw new InvalidDataException("Actual context target has no points.");
        var selected=LegacyPriorContextSelection.SelectNative(groups,owner.DescriptorOrdinal,ranges.Select(r=>r.StrokeIndex).ToArray(),LegacyNativePriorContextMode.SameLineAll,cancellationToken:token);
        return LegacyFloatInputQuantization.Quantize(LegacyContextWeighting.Combine(selected,sum/count,owner.Height,
            profile.LeftGapOffset,profile.LeftVariance,profile.RightVariance,token),cancellationToken:token);
    }
    private static float F(ReadOnlySpan<byte>b,int at)=>BinaryPrimitives.ReadSingleLittleEndian(b[at..]);
    private static int I(ReadOnlySpan<byte>b,int at)=>BinaryPrimitives.ReadInt32LittleEndian(b[at..]);
    private sealed record Profile(float LeftGapOffset,float LeftVariance,float RightVariance,byte MaximumWindowStrokes,ushort Blank,float Cutoff);
}
