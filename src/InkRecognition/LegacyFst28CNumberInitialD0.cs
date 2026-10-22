using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;

[assembly: InternalsVisibleTo("LegacyFst28CNumberInitialD0.Probe")]
namespace InkRecognition;

/// <summary>Detached diagnostic data. Mutating any returned array grants no controller authority.</summary>
public sealed record LegacyFst28CNumberD0GroupSnapshot(int Group, LegacyMainGlyphSourceFrame Frame,
    LegacyInkRange[] Ranges, LegacyInkPoint[][] Normalized, LegacyInkBaseline Baseline,
    short[] Context, short StartBoundary, short EndBoundary, int[][] FamilyProbabilities,
    int[] RawCosts, int[] FinalCosts, float[] Weights, LegacyDynamicAcousticWeightResult Dynamic,
    LegacySourcePrimitiveScope Primitive, LegacyAcousticCacheCandidate[] Candidates);

/// <summary>One-use fresh node0/span0 number-source D0 prefix. No issued query,
/// history, repeated refresh, default selection or OCR authority.</summary>
public sealed class LegacyFst28CNumberInitialD0
{
    public const long MaximumLogicalBytes = 32983513, MaximumWork = 69123317896;
    private const string AcousticHash = "7A5F08809E81FB4A2D4C78B3EFD251D0986503B0D30D21DE3695DAB1F8A89978";
    private sealed class Claim { internal int Taken; }
    private static readonly ConditionalWeakTable<LegacyFst28CNumberSymbolMasks, Claim> Claims = new();
    private readonly Row[] rows;
    private readonly byte[] permissions, eligibility;
    private readonly ushort[] primary;
    private readonly object cohort = new();
    private readonly Model model;
    public LegacyFst28CNumberSymbolMasks Masks { get; }
    public LegacyFst28CFreshPrimaryControllerBootstrap Bootstrap => Masks.Bootstrap;
    public LegacyFst28CInitialState InitialState => Masks.InitialState;
    public LegacyFst28CTargetPreparation Policy => Masks.Policy;
    public LegacyFst13AOutgoingSource Source => Masks.Source;
    public LegacyFreshMainCompactOrdinaryGraph Graph => Masks.Graph;
    public int CurrentNode => 0;
    public int CurrentSpan => 0;
    public bool ReusedPreviousCache => false;
    public bool HasQueryAuthority => false;
    public int AcousticEvaluations => rows.Length;
    public long LogicalByteReservation { get; }
    public long WorkReservation { get; }
    private LegacyFst28CNumberInitialD0(LegacyFst28CNumberSymbolMasks masks, Model model,
        Row[] rows, byte[] permissions, byte[] eligibility, ushort[] primary, long bytes, long work)
    { Masks=masks;this.model=model;this.rows=rows;this.permissions=permissions;this.eligibility=eligibility;
      this.primary=primary;LogicalByteReservation=bytes;WorkReservation=work; }

    public static LegacyFst28CNumberInitialD0 CreateActualDirectNumberFresh(
        LegacyFst28CNumberSymbolMasks masks, ReadOnlySpan<byte> acousticResource, CancellationToken token=default)
        =>CreateBounded(masks,acousticResource,MaximumLogicalBytes,MaximumWork,token);

    internal static LegacyFst28CNumberInitialD0 CreateBounded(LegacyFst28CNumberSymbolMasks masks,
        ReadOnlySpan<byte> acousticResource,long byteBudget,long workBudget,CancellationToken token=default,
        Action<string>? checkpoint=null)
    {
        token.ThrowIfCancellationRequested();ArgumentNullException.ThrowIfNull(masks);
        if(byteBudget is <0 or >MaximumLogicalBytes)throw new ArgumentOutOfRangeException(nameof(byteBudget));
        if(workBudget is <0 or >MaximumWork)throw new ArgumentOutOfRangeException(nameof(workBudget));
        if(acousticResource.Length!=572763||masks.Source.ActualAcousticFingerprint!=AcousticHash||
            !masks.Source.HasActualFreshMain135Authority||masks.Source.ActualAcousticSourceKind!=0x135||
            masks.Bootstrap.HasIssuedQuery||masks.Bootstrap.ReadSourceMetadata(token)!=new LegacyFst28CNumberSourceMetadata(1,0,1,0,1,uint.MaxValue,1)||
            masks.InitialState.DecodedSourceName!="ru_RU-lk-text.number")
            throw new ArgumentException("Exact direct-number masks/bootstrap and same MAIN135 required.");
        var plan=Preflight(masks.Graph,token);
        if(byteBudget<plan.Bytes||workBudget<plan.Work)throw new InvalidDataException("Whole initial-D0 logical/work envelope exceeds budget.");
        void Step(string name){token.ThrowIfCancellationRequested();checkpoint?.Invoke(name);token.ThrowIfCancellationRequested();}
        Step("before-snapshot");var snapshot=new byte[572763];
        try
        {
            for(int at=0;at<snapshot.Length;at+=65536){token.ThrowIfCancellationRequested();
                acousticResource.Slice(at,Math.Min(65536,snapshot.Length-at)).CopyTo(snapshot.AsSpan(at));}
            Step("after-snapshot");
            if(Convert.ToHexString(SHA256.HashData(snapshot))!=AcousticHash)throw new ArgumentException("Authenticated same AK snapshot required.");
            Step("after-hash");
            if(Interlocked.CompareExchange(ref Claims.GetValue(masks,static _=>new Claim()).Taken,1,0)!=0)
                throw new InvalidOperationException("This exact mask issuer already claimed its fresh initial D0 pass.");
            Step("after-claim");
            var model=Model.Load(snapshot,token);Step("after-model");
            var sourceRanges=LegacyFreshMainGlobalSourceRanges.Prepare(masks.Graph,token);
            //Each of these unchanged APIs authenticates the complete resource again.
            var context=LegacyFreshMainGlobalContext.Prepare(snapshot,sourceRanges,model.Activations,token);
            Step("after-context");
            var geometry=LegacyFreshMainGlobalGlyphGeometry.Prepare(snapshot,context,token);
            var preparedInk=geometry.CopyPreparedInk(token); //ONE whole-source copy, never per group.
            var enabled=masks.CopyPrimaryMask(token);var rows=new Row[plan.Groups.Length];
            var processed=new LegacyPreparedAcousticPermissionGroup[rows.Length];
            var eligibility=new byte[195];byte[] families=[1,1,1,1,0,0,0];
            for(int ordinal=0;ordinal<rows.Length;ordinal++)
            {
                Step("before-group-"+ordinal);int group=plan.Groups[ordinal];
                var frame=geometry.ReadFrame(group,token);var ranges=geometry.CopyChronologicalRanges(group,token);
                if(ranges.Length>10)throw new NotSupportedException("Actual main normalized pen-up cap exceeded.");
                var channels=context.GetContext(group,token);var signs=geometry.ReadBoundarySigns(group,token);
                var glyph=model.Classifier.Classify(preparedInk,ranges,geometry.ReadMinimumX(group,token),
                    frame.Baseline,frame.Height,geometry.TargetHeight,masks.Graph.State.MaximumAtoms,channels,
                    signs.Start,signs.End,true,families,1,enabled.AsSpan(0,180),token);
                var primitive=geometry.ReadPrimitiveScope(group,token);
                var scope=new LegacyOrdinaryAcousticCacheContext(true,false,false,
                    primitive.BoundaryAlignedSingleNormalizedStroke,primitive.EndpointFieldsAreZero,
                    primitive.SourceStrokeProbability,false,ReadOnlyMemory<ushort>.Empty);
                var final=model.Cache.Build(glyph.IntegerCosts.Span,glyph.Geometry.Strokes,enabled,scope,model.Pairs,token);
                rows[ordinal]=new(group,frame,ranges,glyph,final,primitive,channels,signs.Start,signs.End);
                processed[ordinal]=new(rows[ordinal].FinalCosts,model.Scale,true,0,true,0,true);
                for(int i=0;i<eligibility.Length;i++)if(rows[ordinal].FinalCosts[i]<0x4000000)eligibility[i]=1;
                Step("after-group-"+ordinal);
            }
            Step("before-permissions");
            var permission=LegacyPreparedAcousticDictionaryPermissions.BuildBytes(192,model.Reserved,processed,token);
            if(!permission.Succeeded||permission.Permissions.Length!=192)throw new InvalidDataException("Initial permission production did not complete.");
            var flags=permission.Permissions.ToArray();Step("before-refresh");
            //28C.V68: restore from preserved58, filter ordinary primary only; secondary remains owned by Policy.
            var primary=masks.CopyPreservedMap(token);
            for(int i=0;i<primary.Length;i++){token.ThrowIfCancellationRequested();if(primary[i]<192&&(flags[primary[i]]&1)==0)primary[i]=ushort.MaxValue;}
            Step("before-publish");
            return new(masks,model,rows,flags,eligibility,primary,plan.Bytes,plan.Work);
        }
        finally{CryptographicOperations.ZeroMemory(snapshot);}
    }

    private sealed record Plan(int[] Groups,long Bytes,long Work);
    private static Plan Preflight(LegacyFreshMainCompactOrdinaryGraph graph,CancellationToken token)
    {
        var state=graph.State;var groups=state.Groups;var retainedGroups=graph.Source.State.Groups;var source=graph.Source.Source;
        int n=state.NodeCount,g=groups.Count,a=0,s=source.Preparation.Strokes.Count,d=source.Steps.Count;
        if(n is <1 or >65535||g is <1 or >65535||s is <1 or >32||d is <1 or >32||state.MaximumAtoms!=8)
            throw new NotSupportedException("Initial-D0 graph/source domain exceeds bounded facade.");
        for(int i=0;i<n;i++){token.ThrowIfCancellationRequested();int count=state.GetNode(i).Count;
            if(count>262144-a)throw new NotSupportedException("Arc domain exceeds bounded facade.");a+=count;}
        var original=source.Preparation.Input.Input.Input.Owner.Controller.Source;
        if(original.Strokes.Count!=s||retainedGroups.Count!=g)throw new InvalidDataException("Same retained source/group allocation domain required.");
        int p0=0;foreach(var stroke in original.Strokes){if(stroke.Points.Count>4096-p0)throw new NotSupportedException("Original point cap4096 exceeded.");p0+=stroke.Points.Count;}
        var last=source.Steps.LastOrDefault(v=>!v.IsPendingSeparator)??throw new NotSupportedException("Ordinary source step required.");
        var counts=last.CopyOutputPointCounts();if(counts.Length!=s)throw new InvalidDataException("Same final physical count domain required.");
        int p=0;foreach(int count in counts){if(count<1||count>4096-p)throw new NotSupportedException("Final point cap4096 exceeded.");p+=count;}
        int c=0;
        foreach(var step in source.Steps)
        {
            token.ThrowIfCancellationRequested();if(step.IsPendingSeparator)continue;
            if(step.Kind!=0||step.FirstPhysical<0||step.LastPhysical<step.FirstPhysical||step.LastPhysical>=s)
                throw new NotSupportedException("Actual ordinary descriptor interval required.");
            int k=step.LastPhysical-step.FirstPhysical;c=checked(c+(k==0?0:k==1?1:3*k-3));
            if(c>96)throw new NotSupportedException("Context window cap96 exceeded.");
            for(int start=step.FirstPhysical+1;start<=step.LastPhysical;start++)for(int length=1;length<=3&&start+length-1<=step.LastPhysical;length++)
            {int total=0;for(int j=0;j<length;j++)total=checked(total+counts[start+j]);if(total>4096)throw new NotSupportedException("Context window point domain exceeded.");}
        }
        var arcs=state.GetNode(0);if(arcs.Count is <1 or >256)throw new NotSupportedException("Initial arc cap256 exceeded.");
        var selected=new List<int>(64);
        foreach(var arc in arcs)
        {
            token.ThrowIfCancellationRequested();if(arc.Span!=0)continue;
            if(arc.GroupId>=(uint)g)throw new InvalidDataException("Actual group membership required.");
            int id=(int)arc.GroupId;if(selected.Contains(id))continue;
            if(selected.Count==64)throw new NotSupportedException("Selected group cap64 exceeded.");
            if(groups[id].Kind07!=0||groups[id].AllocationId!=id||retainedGroups[id].RangeCount is <1 or >8)
                throw new NotSupportedException("Selected ordinary retained range cap8 exceeded.");
            //Compact Prepare preserves allocation IDs and copies this exact append owner's ranges.
            var ranges=groups[id].CopyRanges();
            if(ranges.Length!=retainedGroups[id].RangeCount)throw new InvalidDataException("Compact/retained range cardinality differs.");
            int points=0;
            foreach(var range in ranges)
            {if((uint)range.StrokeIndex>=(uint)counts.Length||range.StartPoint<0||range.EndPointExclusive<=range.StartPoint||range.EndPointExclusive>counts[range.StrokeIndex])throw new InvalidDataException("Same retained group point ranges required.");points=checked(points+range.EndPointExclusive-range.StartPoint);}
            if(points is <1 or >255)throw new NotSupportedException("Selected point cap255 exceeded.");selected.Add(id);
        }
        if(selected.Count==0)throw new NotSupportedException("A genuine selected ordinary initial group is required.");
        var reservation=CalculateReservations(n,g,a,s,d,p0,p,arcs.Count,selected.Count,c);
        return new(selected.ToArray(),reservation.Bytes,reservation.Work);
    }

    internal static (long Bytes,long Work) CalculateReservations(int n,int g,int a,int s,int d,int p0,int p,int r,int t,int c)
    {
        if(n is <1 or >65535||g is <1 or >65535||a is <1 or >262144||s is <1 or >32||d is <1 or >32||
            p0 is <1 or >4096||p is <1 or >4096||r is <1 or >256||t is <1 or >64||c is <0 or >96)
            throw new NotSupportedException("Initial-D0 reservation domain exceeded.");
        checked
        {
            long bytes=1411577L+512L*(p0+p)+128L*d+256L*s+16L*(n+g+a)+1024L*c+327680L*t+4096;
            long work=4887306L+n+g+a+32L*(p0+p)+64L*s+64L*d+4L*d*s+r*(t+64L)+4096+
                c*(5828L+32768+256L*Math.Min(p,4096)+16L*s+16L*d+2048)+
                t*(368720L+64L*255*255+64L*4096*4096+32768+8L*d+16L*c+64L*255+64L*s+4096);
            return(bytes,work);
        }
    }

    public byte[] CopyPermissions(CancellationToken token=default){token.ThrowIfCancellationRequested();return permissions.ToArray();}
    public byte[] CopyEligibility(CancellationToken token=default){token.ThrowIfCancellationRequested();return eligibility.ToArray();}
    public ushort[] CopyCurrentPrimary(CancellationToken token=default){token.ThrowIfCancellationRequested();return primary.ToArray();}
    public LegacyFst28CSecondaryCode[] CopyUnchangedSecondary(CancellationToken token=default)=>Policy.CopySecondary(token);
    public int[] CopySelectedGroups(CancellationToken token=default){token.ThrowIfCancellationRequested();return rows.Select(r=>r.Group).ToArray();}
    public LegacyFst28CNumberD0GroupSnapshot CopyGroup(int ordinal,CancellationToken token=default)
    {token.ThrowIfCancellationRequested();if((uint)ordinal>=(uint)rows.Length)throw new ArgumentOutOfRangeException(nameof(ordinal));return rows[ordinal].Copy();}
    internal bool IsSameIssuer(LegacyFst28CNumberInitialD0 other)=>ReferenceEquals(this,other)&&ReferenceEquals(cohort,other.cohort);

    private sealed class Row
    {
        internal readonly int Group;
        internal readonly int[] FinalCosts;
        private readonly LegacyFst28CNumberD0GroupSnapshot snapshot;
        internal Row(int group,LegacyMainGlyphSourceFrame frame,LegacyInkRange[] ranges,LegacyPreparedGlyphClassification glyph,
            LegacyOrdinaryAcousticClassCacheResult final,LegacySourcePrimitiveScope primitive,short[] context,short start,short end)
        {Group=group;FinalCosts=final.Costs.ToArray();snapshot=new(group,frame,ranges.ToArray(),glyph.Geometry.Strokes.Select(s=>s.Points.ToArray()).ToArray(),glyph.Geometry.Baseline,
            context.ToArray(),start,end,glyph.Scores.FamilyProbabilities.Take(4).Select(p=>p!.Value.ToArray()).ToArray(),glyph.IntegerCosts.ToArray(),FinalCosts,
            glyph.ResolvedWeights.ToArray(),glyph.DynamicWeights,primitive,final.Candidates.ToArray());}
        internal LegacyFst28CNumberD0GroupSnapshot Copy()=>snapshot with{Ranges=snapshot.Ranges.ToArray(),Normalized=snapshot.Normalized.Select(p=>p.ToArray()).ToArray(),
            Context=snapshot.Context.ToArray(),FamilyProbabilities=snapshot.FamilyProbabilities.Select(v=>v.ToArray()).ToArray(),RawCosts=snapshot.RawCosts.ToArray(),
            FinalCosts=FinalCosts.ToArray(),Weights=snapshot.Weights.ToArray(),Candidates=snapshot.Candidates.ToArray()};
    }

    private sealed class Model
    {
        internal readonly LegacyInkActivations Activations;
        internal readonly LegacyPreparedGlyphClassifier Classifier;
        internal readonly LegacyOrdinaryAcousticClassCache Cache;
        internal readonly LegacyAcousticCachePair[] Pairs;
        internal readonly ushort[] Reserved;
        internal readonly float Scale;
        private Model(LegacyInkActivations activations,LegacyPreparedGlyphClassifier classifier,LegacyOrdinaryAcousticClassCache cache,
            LegacyAcousticCachePair[] pairs,ushort[] reserved,float scale)
        {Activations=activations;Classifier=classifier;Cache=cache;Pairs=pairs;Reserved=reserved;Scale=scale;}
        internal static Model Load(ReadOnlySpan<byte> snapshot,CancellationToken token)
        {
            if(BinaryPrimitives.ReadUInt32LittleEndian(snapshot[5233..])!=0x135||
                !snapshot.Slice(195871,16).SequenceEqual(new byte[]{1,2,0,0,0,0,0,0,0,0,0,0,0,0,0,0})||
                BinaryPrimitives.ReadInt16LittleEndian(snapshot[566380..])!=0)
                throw new InvalidDataException("Actual MAIN135/2B1/empty groups required.");
            int[] hidden=new int[48],output=new int[128];
            for(int i=0;i<hidden.Length;i++)hidden[i]=(int)Math.Floor(1024*Math.Tanh(3d*i/47));
            for(int i=0;i<127;i++)output[i]=(int)Math.Floor(1024/(1+Math.Exp(-8d*i/127)));output[127]=1024;
            var activations=new LegacyInkActivations(hidden,output);var families=new LegacyDirectionalAcousticFamily[4];
            int[] starts=[195895,283697,367899,467101],ends=[283690,367892,467094,566296];
            for(int f=0;f<4;f++)
            {
                token.ThrowIfCancellationRequested();var centers=new float[180];var slopes=new float[180];
                for(int i=0;i<180;i++){int at=566416+(f*192+i)*8;centers[i]=BinaryPrimitives.ReadSingleLittleEndian(snapshot[at..]);slopes[i]=BinaryPrimitives.ReadSingleLittleEndian(snapshot[(at+4)..]);}
                families[f]=new(LegacyInkNetwork.Load(snapshot[starts[f]..ends[f]]),centers,slopes);
            }
            var temporal=new LegacyDirectionalCalibration(LegacyDirectional2B1DefaultCalibration.CopyMinimum(false),LegacyDirectional2B1DefaultCalibration.CopyMaximum(false));
            var spatial=new LegacyDirectionalCalibration(LegacyDirectional2B1DefaultCalibration.CopyMinimum(true),LegacyDirectional2B1DefaultCalibration.CopyMaximum(true));
            var weights=LegacyDirectionalAcousticWeightConfiguration.LoadV110Prefix(snapshot[189734..],out int consumed,token);
            if(consumed!=566330-189734)throw new InvalidDataException("Actual acoustic prefix partition differs.");
            var classifier=new LegacyPreparedGlyphClassifier(LegacyDirectionalGlyphClassifier.CreateActual2B1(families,activations,temporal,spatial),weights);
            var alphabet=LegacyInkAlphabet.Load(snapshot.Slice(99,5134),0x103);
            var cacheProfile=LegacyOrdinaryAcousticCacheProfile.ReadV110Node(snapshot[189734..],token);
            var scale=LegacyPreparedAcousticDictionaryPermissions.ReadDisabledPowerScale(snapshot[189734..],token);
            return new(activations,classifier,new(cacheProfile,alphabet,snapshot.Slice(99,16)),LegacyWideSymbolPairs.CreateUnrestricted(alphabet),
                LegacyPreparedAcousticDictionaryPermissions.ReadReservedSlots(alphabet,snapshot.Slice(99,16)),scale);
        }
    }
}
