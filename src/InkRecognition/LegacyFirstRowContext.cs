using System.Buffers.Binary;

namespace InkRecognition;

/// <summary>Stored type227/226 configuration from the first owned ordinary profile.</summary>
public sealed record LegacyFirstRowContextProfile(float LeftGapOffset,float LeftVariance,float RightVariance,
    bool UseMaximumX,byte MaximumWindowStrokes,ushort NativeBlankClass,float NonblankCutoff,
    bool IncludeBlankOutput,byte Selector,bool UsePointMeanY,bool UseExtendedGeometry)
{
    public static LegacyFirstRowContextProfile ReadSource(ReadOnlySpan<byte> resource,LegacyFirstAcousticModel model)
    {
        ArgumentNullException.ThrowIfNull(model);
        int network=checked(model.OrdinaryNodeOffset+21);
        if(network<0||network>resource.Length-8)throw new InvalidDataException("Context network header outside source.");
        uint bytes=BinaryPrimitives.ReadUInt32LittleEndian(resource[(network+4)..]);
        if(bytes>1024*1024||bytes>resource.Length-network-8)throw new InvalidDataException("Invalid context network size.");
        int p=checked(network+8+(int)bytes);
        if(p>resource.Length-24)throw new InvalidDataException("Truncated context settings.");
        var s=resource.Slice(p,24);
        float left=BinaryPrimitives.ReadSingleLittleEndian(s),lv=BinaryPrimitives.ReadSingleLittleEndian(s[4..]),rv=BinaryPrimitives.ReadSingleLittleEndian(s[8..]);
        float cutoff=BinaryPrimitives.ReadSingleLittleEndian(s[16..]);
        ushort blank=BinaryPrimitives.ReadUInt16LittleEndian(s[14..]);
        if(!float.IsFinite(left)||!float.IsFinite(lv)||lv<=0||!float.IsFinite(rv)||rv<=0||!float.IsFinite(cutoff)||cutoff is <0 or >1||
            s[12]>1||s[13] is <1 or >3||blank!=28||s[20]!=1||s[21]!=model.ContextSelector||s[22]>1||s[23]!=1||model.ContextNetwork.OutputWidth!=28)
            throw new NotSupportedException("Unsupported first context weights/window/blank/geometry branch.");
        return new(left,lv,rv,s[12]!=0,s[13],blank,cutoff,true,s[21],s[22]!=0,true);
    }
}

/// <summary>Fresh single ordinary row context on original prepared float source coordinates.
/// No source mutation cache, opaque groups, custom scopes, boundary kinds or word search.</summary>
public sealed class LegacyFirstRowContext
{
    private readonly LegacyOrdinaryRowAcousticInput row;
    private readonly LegacyFirstPreparedAcousticClassifier classifier;
    private readonly LegacyPriorContextGroup[] groups;
    public LegacyFirstRowContextProfile Profile {get;}
    public IReadOnlyList<LegacyPriorContextGroup> Groups {get;}

    private LegacyFirstRowContext(LegacyOrdinaryRowAcousticInput row,LegacyFirstPreparedAcousticClassifier classifier,
        LegacyFirstRowContextProfile profile,LegacyPriorContextGroup[] groups)
    {this.row=row;this.classifier=classifier;Profile=profile;this.groups=groups;Groups=Array.AsReadOnly(groups);}

    public static LegacyFirstRowContext Prepare(ReadOnlySpan<byte> resource,int sourceOffset,
        LegacyOrdinaryRowAcousticInput row,LegacyInkActivations activations,CancellationToken cancellationToken=default)
    {
        ArgumentNullException.ThrowIfNull(row);cancellationToken.ThrowIfCancellationRequested();
        var classifier=LegacyFirstPreparedAcousticClassifier.ReadSource(resource,sourceOffset,activations,cancellationToken);
        if(row.Profile!=classifier.Model.RowProfile)throw new ArgumentException("Context row does not belong to the loaded first source profile.");
        var profile=LegacyFirstRowContextProfile.ReadSource(resource,classifier.Model);
        var groups=ScoreOrdinaryLine(row.Prepared.Strokes,row.Frame.Line.Height,row.Frame.Line.Baseline,
            classifier.Model.ContextNetwork,activations,profile,cancellationToken);
        return new(row,classifier,profile,groups);
    }

    public short[] GetContext(LegacySmoothedRowCutResult cuts,ReadOnlySpan<LegacyInkRange> ranges,
        CancellationToken cancellationToken=default)
    {
        ArgumentNullException.ThrowIfNull(cuts);cancellationToken.ThrowIfCancellationRequested();
        if(!ReferenceEquals(cuts.Owner,row)||ranges.Length is <1 or >15)throw new ArgumentException("Owned1..15 prepared ranges required.");
        var ink=cuts.Cuts.PreparedInk;var counts=ink.StrokePointCounts.Span;var starts=new int[counts.Length];
        for(int i=1;i<starts.Length;i++)starts[i]=starts[i-1]+counts[i-1];
        float sum=0;int points=0;var physical=new int[ranges.Length];
        for(int i=0;i<ranges.Length;i++)
        {
            var r=ranges[i];if((uint)r.StrokeIndex>=(uint)counts.Length||r.StartPoint<0||r.EndPointExclusive<=r.StartPoint||r.EndPointExclusive>counts[r.StrokeIndex])
                throw new ArgumentException("Invalid context target range.",nameof(ranges));
            //Context ca5f0 walks the supplied native graph ranges. This checked
            //ordinary endpoint requires physical-order, nonoverlapping groups;
            //chronological sorting belongs to the separate normalizer.
            if(i>0&&(r.StrokeIndex<ranges[i-1].StrokeIndex||
                (r.StrokeIndex==ranges[i-1].StrokeIndex&&r.StartPoint<ranges[i-1].EndPointExclusive)))
                throw new ArgumentException("Context requires physical-order nonoverlapping native group ranges.",nameof(ranges));
            physical[i]=r.StrokeIndex;
            for(int p=r.StartPoint;p<r.EndPointExclusive;p++)
            {cancellationToken.ThrowIfCancellationRequested();sum+=ink.Points.Span[starts[r.StrokeIndex]+p].X;points++;if(points>4096)throw new ArgumentException("Context target exceeds4096 points.");}
        }
        var mode=Profile.Selector switch{0=>LegacyNativePriorContextMode.AllContained,1=>LegacyNativePriorContextMode.Disjoint,_=>LegacyNativePriorContextMode.SameLineAll};
        var selected=LegacyPriorContextSelection.SelectNative(groups,0,physical,mode,cancellationToken:cancellationToken);
        return LegacyFloatInputQuantization.Quantize(LegacyContextWeighting.Combine(selected,sum/points,row.Frame.Line.Height,
            Profile.LeftGapOffset,Profile.LeftVariance,Profile.RightVariance,cancellationToken),cancellationToken:cancellationToken);
    }

    public LegacyFirstPreparedAcousticScores Classify(LegacySmoothedRowCutResult cuts,ReadOnlySpan<LegacyInkRange> ranges,
        short startBoundary,short endBoundary,int maximumRangesBeforeMerge,int maximumStrokesAfterMerge,
        ReadOnlySpan<byte> enabledClasses=default,CancellationToken cancellationToken=default)
        =>classifier.Classify(row,cuts,ranges,GetContext(cuts,ranges,cancellationToken),startBoundary,endBoundary,
            maximumRangesBeforeMerge,maximumStrokesAfterMerge,enabledClasses,cancellationToken);

    private static LegacyPriorContextGroup[] ScoreOrdinaryLine(IReadOnlyList<InkStroke> strokes,float height,float baseline,
        LegacyInkNetwork network,LegacyInkActivations activations,LegacyFirstRowContextProfile profile,CancellationToken cancellationToken)
    {
        var result=new List<LegacyPriorContextGroup>();float reference=profile.UseMaximumX?float.MaxValue:-float.MaxValue;
        for(int start=1;start<strokes.Count;start++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            foreach(var p in strokes[start-1].Points)reference=profile.UseMaximumX?Math.Min(reference,p.X):Math.Max(reference,p.X);
            float sum=0;int count=0;
            for(int length=1;length<=profile.MaximumWindowStrokes&&start+length<=strokes.Count;length++)
            {
                var window=new InkStroke[length];for(int i=0;i<length;i++)window[i]=strokes[start+i];
                foreach(var p in strokes[start+length-1].Points){sum+=p.X;count++;}
                var features=LegacyFloatContextGeometry.Extract(window,height,reference,baseline,profile.UseMaximumX,profile.UsePointMeanY,cancellationToken);
                var input=LegacyFloatInputQuantization.Quantize(features,10,cancellationToken);
                var probability=LegacyClassScores.Convert(network.Evaluate(input,activations,cancellationToken:cancellationToken),cancellationToken).Probability32768.Span;
                float mass=0;var values=new float[28];for(int i=0;i<28;i++){values[i]=probability[i]*(1f/32768);if(i!=profile.NativeBlankClass-1)mass+=values[i];}
                if(profile.NonblankCutoff<mass)result.Add(new(0,-1,start,start+length-1,sum/count,values));
            }
        }
        return result.ToArray();
    }
}
