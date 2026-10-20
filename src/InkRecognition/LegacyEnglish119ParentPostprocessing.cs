using System.Buffers.Binary;
using System.Runtime.CompilerServices;
[assembly:InternalsVisibleTo("LegacyEnglish119Postprocessing.Probe")]
namespace InkRecognition;

public sealed partial class LegacyEnglish119ParentSubmission
{
    private bool postprocessingReserved;
    //Explicit prepared validation controls; never reserve or publish a source.
    internal int ValidatePreparedPostprocessingShape(byte[][] descriptors,IReadOnlyList<LegacyFirstResultSourceRecord> rows,
        IReadOnlyList<LegacyEnglish119ParentRowOrigin> origins,CancellationToken token=default)
        =>LegacyEnglish119ParentPostprocessing.Preflight(this,current,descriptors,rows,origins,token);
    internal LegacyEnglish119ParentPostprocessing ReservePostprocessing(LegacyNativeCaseData caseData,
        LegacyNativeTextPublicationData publication,bool diagnostic,CancellationToken token)
    {
        token.ThrowIfCancellationRequested();lock(gate)
        {
            token.ThrowIfCancellationRequested();
            if(!completed||faulted||postprocessingReserved)throw new InvalidOperationException("One completed, unreserved actual EN parent publication required.");
            //No child locks or callbacks are entered. These completed arrays and
            //owners cannot subsequently change through the old publication API.
            var next=new LegacyEnglish119ParentPostprocessing(this,packet.Source,current,publishedDescriptors,
                prefix,origins,caseData,publication,diagnostic,token);
            token.ThrowIfCancellationRequested();postprocessingReserved=true;return next;
        }
    }
}

/// <summary>Immutable diagnostic image of one completed stage. Source rows remain
///in the pre265 retained-point domain; current points/keys are a separate domain.</summary>
public sealed class LegacyEnglish119PostprocessingSnapshot
{
    private readonly byte[][] descriptors,guides;
    private readonly InkStroke[] points;
    private readonly LegacyFirstResultLineRow[] rows;
    private readonly uint[] keys;
    public string Stage{get;}
    public IReadOnlyList<LegacyFirstResultLineRow> Rows=>Array.AsReadOnly(rows);
    internal LegacyEnglish119PostprocessingSnapshot(string stage,byte[][] descriptors,byte[][] guides,
        IReadOnlyList<InkStroke> points,IReadOnlyList<LegacyFirstResultLineRow> rows,uint[] keys)
    {Stage=stage;this.descriptors=descriptors.Select(x=>x.ToArray()).ToArray();this.guides=guides.Select(x=>x.ToArray()).ToArray();
     this.points=LegacyEnglish119ParentSubmission.Copy(points);this.rows=rows.ToArray();this.keys=keys.ToArray();}
    public byte[][] CopyDescriptors()=>descriptors.Select(x=>x.ToArray()).ToArray();
    public byte[][] CopyGuides()=>guides.Select(x=>x.ToArray()).ToArray();
    public InkStroke[] CopyStrokes()=>LegacyEnglish119ParentSubmission.Copy(points);
    public uint[] CopyPointKeys200()=>keys.ToArray();
}

/// <summary>NEW bounded actual EN parent249->24B->assigned24A->24D->29B->EN265.
/// Stops before169C30/15D360/15D920/main inference. No supplied geometry/results.</summary>
public sealed partial class LegacyEnglish119ParentPostprocessing
{
    private readonly object gate=new();
    private readonly LegacyOrdinarySourceInk original;
    private readonly LegacyFreshPhysicalBuffer input;
    private readonly byte[][] initialDescriptors;
    private readonly LegacyFirstResultSourceRecord[] published;
    private readonly LegacyFirstResultLineMarkers markers;
    private readonly LegacyFirstResultLineProfile lineProfile;
    private readonly LegacyOrdinaryShearProfile shearProfile;
    private readonly LegacyOrdinaryGapProfile gapProfile;
    private readonly LegacyOrdinaryGapMixtureProfile mixtureProfile;
    private readonly LegacyOrientationSmoothingProfile smoothingProfile;
    private readonly LegacyNativeCaseAlphabetRanges cased;
    private readonly List<string> trace=[];
    private readonly List<LegacyEnglish119PostprocessingSnapshot> snapshots=[];
    private bool entered,faulted,complete;
    private readonly bool diagnostic;
    internal LegacyEnglish119SnapshotCopyBudget CopyBudget{get;}
    public LegacyEnglish119ParentSubmission Parent{get;}
    public LegacyEnglish119MainWireContext MainWire{get;}
    public LegacyOrientationStrokeMetrics OriginalMetrics{get;}
    public LegacyOrdinaryShearResult? Shear{get;private set;}
    public LegacyNativeFreshGapResult? Gap{get;private set;}
    private LegacySourcePointChronology? finalChronology;
    public bool IsFaulted{get{lock(gate)return faulted;}}
    public bool IsComplete{get{lock(gate)return complete;}}
    public bool HasEntered{get{lock(gate)return entered;}}
    private readonly List<LegacyCharacterLineCompletedFit> lineFits=[];
    public IReadOnlyList<LegacyCharacterLineCompletedFit> CopyLineFits(){lock(gate)return Array.AsReadOnly(lineFits.ToArray());}
    public string[] CopyTrace(){lock(gate)return trace.ToArray();}
    /// <summary>Production returns only the latest completed image (zero or one).
    /// Internal diagnostic preparation retains all completed stage images.</summary>
    public IReadOnlyList<LegacyEnglish119PostprocessingSnapshot> CopySnapshots(){lock(gate)return Array.AsReadOnly(snapshots.ToArray());}
    public LegacySourcePointIdentity ResolveFinalPoint(int stroke,int point)
    {lock(gate){if(!complete)throw new InvalidOperationException("Completed265 chronology required.");return finalChronology!.Resolve(stroke,point);}}
    public LegacySourcePointIdentity ResolveFinalEndingMetadata(int stroke,int point,CancellationToken token=default)
    {lock(gate){token.ThrowIfCancellationRequested();return original.ResolveEndingMetadata(ResolveFinalPoint(stroke,point),token);}}
    public static LegacyEnglish119ParentPostprocessing Prepare(LegacyEnglish119ParentSubmission actualCompletedParent,
        LegacyNativeCaseData caseData,LegacyNativeTextPublicationData publication,CancellationToken token=default)
    {ArgumentNullException.ThrowIfNull(actualCompletedParent);ArgumentNullException.ThrowIfNull(caseData);ArgumentNullException.ThrowIfNull(publication);
     return actualCompletedParent.ReservePostprocessing(caseData,publication,false,token);}

    //Internal diagnostic policy is selected before reservation and all large copies.
    internal static LegacyEnglish119ParentPostprocessing PrepareDiagnostic(LegacyEnglish119ParentSubmission parent,
        LegacyNativeCaseData caseData,LegacyNativeTextPublicationData publication,CancellationToken token=default)
    {ArgumentNullException.ThrowIfNull(parent);ArgumentNullException.ThrowIfNull(caseData);ArgumentNullException.ThrowIfNull(publication);
     return parent.ReservePostprocessing(caseData,publication,true,token);}

    internal LegacyEnglish119ParentPostprocessing(LegacyEnglish119ParentSubmission parent,LegacyOrdinarySourceInk original,
        LegacyFreshPhysicalBuffer current,byte[][] descriptors,IReadOnlyList<LegacyFirstResultSourceRecord> rows,
        IReadOnlyList<LegacyEnglish119ParentRowOrigin> origins,LegacyNativeCaseData caseData,LegacyNativeTextPublicationData publication,bool diagnostic,CancellationToken token)
    {
        Parent=parent;this.diagnostic=diagnostic;this.original=original;input=current;token.ThrowIfCancellationRequested();var source=parent.Model.ParentSource;
        if(source.Offset!=0||!source.Fingerprint.Equals("ca46297ada4a80de0d4bbd40650a8ace5c3809af9edbd493fb8f4eb692a18657",StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Actual retained EN lite main source required.");
        var processors=source.Children.Where(c=>c.Destination.StartsWith("parametersC0.58[",StringComparison.Ordinal)).ToArray();
        uint[] kinds=[0x249,0x24b,0x24a,0x24d,0x29b,0x265];ushort[] versions=[0x100,0x103,0x102,0x100,0x100,0x102];
        if(processors.Length!=6)throw new InvalidDataException("Actual six-processor EN parent required.");
        for(int i=0;i<6;i++){token.ThrowIfCancellationRequested();if(processors[i].Kind!=kinds[i]||processors[i].Version!=versions[i])throw new InvalidDataException("Actual ordered EN postprocessors required.");}
        //These reads retain this source's own child objects, including EN24A
        //and265 which differ from RU. No whole resource/frame is reconstructed.
        lineProfile=LegacyFirstResultLineProfile.ReadV103Node(source.CopyChildEnvelopeBytes(processors[1],token),0);
        if(lineProfile.Option18!=0)throw new NotSupportedException("Nonzero24B early-return option excluded.");
        _=source.CopyChildEnvelopeBytes(processors[2],token);
        shearProfile=LegacyOrdinaryShearProfile.ReadV100Node(source.CopyChildEnvelopeBytes(processors[3],token),0);
        byte[] gap=source.CopyChildEnvelopeBytes(processors[4],token);
        gapProfile=LegacyOrdinaryGapProfile.ReadV100Node(gap,0);mixtureProfile=LegacyOrdinaryGapMixtureProfile.ReadV100Node(gap,0);
        smoothingProfile=LegacyOrientationSmoothingProfile.ReadV102Node(source.CopyChildEnvelopeBytes(processors[5],token),0);
        //Validate aggregate structures and all real memberships BEFORE copying
        //rows/points or reserving the once-only continuation on the parent.
        int visits=Preflight(parent,current,descriptors,rows,origins,token);
        int ordinary=0;foreach(var descriptor in descriptors){token.ThrowIfCancellationRequested();if((H(descriptor,0)&15)==0)ordinary++;}
        CopyBudget=LegacyEnglish119SnapshotCopyBudget.Preflight(descriptors.Length,ordinary,current.Strokes.Length,rows.Count,diagnostic);
        int maximumGrid=(int)Math.Ceiling((double)(shearProfile.MaximumShear-shearProfile.MinimumShear)/shearProfile.SearchStep)+2;
        if((long)(maximumGrid+1)*visits>LegacyNativeOrdinaryShearPreparation.MaximumPointVisits)throw new ArgumentException("Whole24D conservative range-work budget exceeded before copying.");
        ushort Label(string text)=>source.Alphabet.TryGetLabel(text,out ushort n)?n:ushort.MaxValue;
        markers=new(Label("\U000f0002"),Label("\U000f0003"),Label("-"),Label("_"),Label("\u30fc"),Label("\u4e00"),Label("\ufffd"));
        cased=LegacyNativeCaseAlphabetRanges.Prepare(source.Alphabet,caseData,publication,token);
        OriginalMetrics=LegacySourceOrientationPreparation.ExtractMetrics(original,token);
        MainWire=LegacyEnglish119MainWireContext.Read(this,source,token);
        initialDescriptors=descriptors.Select(x=>x.ToArray()).ToArray();published=rows.ToArray();
        token.ThrowIfCancellationRequested();
    }

    internal static int Preflight(LegacyEnglish119ParentSubmission parent,LegacyFreshPhysicalBuffer current,
        byte[][] descriptors,IReadOnlyList<LegacyFirstResultSourceRecord> rows,IReadOnlyList<LegacyEnglish119ParentRowOrigin> origins,CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        if(descriptors.Length is<1 or>511||current.Strokes.Length is<1 or>256||rows.Count is<1 or>4096||origins.Count!=rows.Count)
            throw new ArgumentException("Bounded complete actual parent required.");
        int points=0,rangeCount=0,visits=0,nextPhysical=0,nextRow=0,ordinary=0;
        for(int s=0;s<current.Strokes.Length;s++)
        {token.ThrowIfCancellationRequested();int n=current.Strokes[s].Points.Count;if(n<1||n>4096-points)throw new ArgumentException("Whole current point budget exceeded.");points+=n;}
        for(int d=0;d<descriptors.Length;d++)
        {
            token.ThrowIfCancellationRequested();var b=descriptors[d];if(b.Length!=88)throw new ArgumentException("Complete descriptor required.");int kind=H(b,0)&15;
            if(kind==3)continue;if(kind!=0||I(b,16)!=-1)throw new NotSupportedException("Fresh unassigned ordinary descriptor required.");ordinary++;
            int first=I(b,36),last=I(b,40),begin=I(b,52),end=I(b,56);
            if(first!=nextPhysical||last<first||last>=current.Strokes.Length||begin!=nextRow||end<=begin||end>rows.Count||end-begin>256)
                throw new ArgumentException("Complete disjoint physical/source partition and portable256-item24B cap required.");
            nextPhysical=last+1;nextRow=end;
            for(int r=begin;r<end;r++)
            {
                token.ThrowIfCancellationRequested();var o=origins[r];
                if(!ReferenceEquals(o.Child.Owner,parent)||o.Child.DescriptorOrdinal!=d||!ReferenceEquals(o.Text.Factory.Candidates.Preparation,o.Child.Submission.Preparation))
                    throw new ArgumentException("Real source-row origin/submission identity required.");
                var row=rows[r];if(row.Ranges.Count>65536-rangeCount)throw new ArgumentException("Whole projected range budget exceeded.");rangeCount+=row.Ranges.Count;
                foreach(var range in row.Ranges)
                {
                    token.ThrowIfCancellationRequested();var a=range.First;var z=range.Last;
                    if(a.First!=z.First||a.Second<first||z.Second<a.Second||z.Second>last||a.Third<0||z.Third<0||a.Third>=current.Strokes[a.Second].Points.Count||z.Third>=current.Strokes[z.Second].Points.Count||a.Second==z.Second&&z.Third<a.Third)
                        throw new ArgumentException("Actual descriptor-contained parent range required.");
                    for(int s=a.Second;s<=z.Second;s++)
                    {token.ThrowIfCancellationRequested();int n=(s==z.Second?z.Third+1:current.Strokes[s].Points.Count)-(s==a.Second?a.Third:0);if(n>1048576-visits)throw new ArgumentException("Whole24B range-visit budget exceeded.");visits+=n;}
                }
            }
        }
        if(nextPhysical!=current.Strokes.Length||nextRow!=rows.Count||ordinary!=parent.Children.Count)throw new ArgumentException("Complete actual ordinary partition required.");
        return visits;
    }
    public LegacyEnglish119ParentPostprocessing Run(CancellationToken token=default)=>RunCore(null,token);
    internal LegacyEnglish119ParentPostprocessing RunPrepared(Action<string> checkpoint,CancellationToken token=default)=>RunCore(checkpoint,token);
    private LegacyEnglish119ParentPostprocessing RunCore(Action<string>? checkpoint,CancellationToken token)
    {
        token.ThrowIfCancellationRequested();lock(gate)
        {
            token.ThrowIfCancellationRequested();if(entered||faulted||complete)throw new InvalidOperationException("EN continuation is once-only.");entered=true;
            void Check(string stage){trace.Add(stage);checkpoint?.Invoke(stage);token.ThrowIfCancellationRequested();}
            try
            {
                Check("enter");var descriptors=initialDescriptors.Select(b=>b.ToArray()).ToArray();var guides=new List<byte[]>();
                var points=LegacyEnglish119ParentSubmission.Copy(input.Strokes);var rows=published.Select(LegacyFirstResultLineRow.FromFirstResult).ToArray();
                var fitProfile=lineProfile.PrepareFitProfile(Parent.PhysicalResolution,0,[]);
                for(int d=0;d<descriptors.Length;d++)
                {
                    token.ThrowIfCancellationRequested();var descriptor=descriptors[d];if((H(descriptor,0)&15)==3)continue;
                    int first=I(descriptor,36),last=I(descriptor,40),begin=I(descriptor,52),end=I(descriptor,56);
                    var items=LegacyFirstResultLinePreparation.Prepare(published,begin,end,markers,token);
                    var bounds=LegacyCharacterLineFitCompletion.ScanPhysicalInterval(points,first,last,token);
                    var fit=LegacyCharacterLineFitCompletion.Estimate(items,fitProfile,bounds,false,token);
                    byte[] guide=new byte[40];F(guide,0,fit.Baseline);F(guide,4,fit.Height);F(guide,8,fit.Slope);F(guide,12,fit.Baseline);F(guide,16,fit.Height);
                    W(guide,32,-1);W(guide,36,int.MinValue);W(descriptor,16,guides.Count);
                    guides.Add(DeskewDescriptor(points,first,last,rows,begin,end,guide,fitProfile.MinimumHeight,token));lineFits.Add(fit);
                    Snapshot("24B:"+d);Check("24B:"+d);
                }
                //16E850 enters geometry only when descriptor10<0. All supported
                //24B descriptors are assigned, including original filtered ink.
                Snapshot("24A:assigned");Check("24A:assigned");
                Shear=LegacyNativeOrdinaryShearPreparation.Apply(shearProfile,descriptors.Select(b=>new ReadOnlyMemory<byte>(b)).ToArray(),guides.Select(b=>new ReadOnlyMemory<byte>(b)).ToArray(),points,Enumerable.Repeat(-1,points.Length).ToArray(),rows,cased,token);
                points=Shear.CopyPhysicalPoints().Select(p=>new InkStroke(p)).ToArray();rows=Shear.Rows.ToArray();Snapshot("24D");Check("24D");
                Gap=LegacyNativeFreshGapPreparation.Apply(gapProfile,mixtureProfile,descriptors.Select(b=>new ReadOnlyMemory<byte>(b)).ToArray(),guides.Select(b=>new ReadOnlyMemory<byte>(b)).ToArray(),rows,-1,token);Snapshot("29B");Check("29B");
                var smoothed=new List<InkStroke>();var keys=new List<uint>();int offset=0;
                for(int d=0;d<descriptors.Length;d++)
                {
                    token.ThrowIfCancellationRequested();var descriptor=descriptors[d];if((H(descriptor,0)&15)==3)continue;
                    int first=I(descriptor,36),last=I(descriptor,40);float height=BinaryPrimitives.ReadSingleLittleEndian(guides[I(descriptor,16)].AsSpan(4));
                    var prepared=LegacyOrientationStrokePreparation.PrepareWithPointKeys(points.Skip(first).Take(last-first+1).ToArray(),smoothingProfile,height,OriginalMetrics,token);
                    if(prepared.PointKeys200.Length>4096-keys.Count)throw new ArgumentException("Whole265 output point budget exceeded.");
                    smoothed.AddRange(prepared.Strokes);foreach(int key in prepared.PointKeys200.Span){token.ThrowIfCancellationRequested();keys.Add(checked((uint)(key+offset*200)));}
                    offset+=points.Skip(first).Take(last-first+1).Sum(s=>s.Points.Count);
                    //The partial265 prefix is separate from all physical points.
                    Retain(new("265:"+d,descriptors,guides.ToArray(),smoothed,rows,keys.ToArray()));Check("265:"+d);
                }
                token.ThrowIfCancellationRequested();if(smoothed.Count!=points.Length)throw new InvalidDataException("Incomplete265 physical coverage.");
                finalChronology=input.Chronology.RemapPreparedPoints(smoothed.Select(s=>s.Points.Count).ToArray(),keys.ToArray(),200,token);
                points=smoothed.ToArray();Retain(new("post265",descriptors,guides.ToArray(),points,rows,keys.ToArray()));Check("post265");complete=true;trace.Add("complete");return this;
                void Snapshot(string stage)=>Retain(new(stage,descriptors,guides.ToArray(),points,rows,[]));
            }
            catch{faulted=true;trace.Add("fault");throw;}
        }
    }
    //The new image is fully constructed before replacing the old completed one.
    //No callback or cancellation point separates Clear/Add while holding gate.
    private void Retain(LegacyEnglish119PostprocessingSnapshot next)
    {if(!diagnostic)snapshots.Clear();snapshots.Add(next);}
    //Pure numeric projection only; both actual and separately named prepared
    //controls use this same whole-descriptor transport. No model substitution.
    internal static byte[] DeskewDescriptor(InkStroke[] points,int first,int last,LegacyFirstResultLineRow[] rows,
        int begin,int end,byte[] guide,float minimum,CancellationToken token=default)
    {
        var localRows=rows.Skip(begin).Take(end-begin).Select(row=>new LegacyFirstResultLineRow(row.CopyStorage().Span,
            row.Ranges.Select(range=>new LegacyFirstResultSourceRange(new(range.First.First,range.First.Second-first,range.First.Third),new(range.Last.First,range.Last.Second-first,range.Last.Third))).ToArray())).ToArray();
        var deskew=LegacyFirstResultLineDeskew.Apply(points.Skip(first).Take(last-first+1).ToArray(),0,last-first,localRows,0,end-begin,guide,minimum,false,token);
        var moved=deskew.CopyPhysicalPoints();for(int i=0;i<moved.Length;i++)points[first+i]=new InkStroke(moved[i]);
        for(int r=begin;r<end;r++)rows[r]=new(deskew.Rows[r-begin].CopyStorage().Span,rows[r].Ranges);
        return deskew.CopyGuide().ToArray();
    }
    private static int I(ReadOnlySpan<byte>b,int at)=>BinaryPrimitives.ReadInt32LittleEndian(b[at..]);
    private static ushort H(ReadOnlySpan<byte>b,int at)=>BinaryPrimitives.ReadUInt16LittleEndian(b[at..]);
    private static void W(Span<byte>b,int at,int x)=>BinaryPrimitives.WriteInt32LittleEndian(b[at..],x);
    private static void F(Span<byte>b,int at,float x)=>BinaryPrimitives.WriteSingleLittleEndian(b[at..],x);
}

//Conservative copy-payload/work envelope, NOT a process RSS estimate. Objects,
//array slots and headers are charged padded 64-byte overheads (8-byte slots),
//point payloads8, keys4. Shared immutable row objects are not cloned. Caller-held
//public defensive copies and runtime/GC overhead are outside this owner envelope.
internal readonly record struct LegacyEnglish119SnapshotCopyBudget(long MaximumImageBytes,long PeakRetainedBytes,long MaximumCopyWorkBytes,int MaximumImages)
{
    internal const long ProductionPeakLimit=512*1024,ProductionWorkLimit=128*1024*1024,DiagnosticWorkLimit=32*1024*1024;
    internal static LegacyEnglish119SnapshotCopyBudget Preflight(int descriptors,int ordinary,int strokes,int rows,bool diagnostic)
    {
        if(descriptors is<1 or>511||ordinary is<1 or>256||ordinary>descriptors||strokes is<1 or>256||ordinary>strokes||rows is<1 or>4096)
            throw new ArgumentException("Bounded validated snapshot shape required.");
        long image=checked(512L+descriptors*(88+64+8L)+ordinary*(40+64+8L)+strokes*(128+16L)+4096*8L+4096*8L+ordinary*8L+rows*8L);
        int count=checked(2*ordinary+4);long work=checked(image*count),peak=checked(image*(diagnostic?count:2));
        if(diagnostic?work>DiagnosticWorkLimit:peak>ProductionPeakLimit||work>ProductionWorkLimit)
            throw new ArgumentException("Whole snapshot copy envelope exceeded before reservation/copying.");
        return new(image,peak,work,count);
    }
}
