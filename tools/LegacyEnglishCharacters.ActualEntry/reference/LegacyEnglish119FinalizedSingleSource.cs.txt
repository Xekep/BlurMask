using System.Buffers.Binary;
using System.Runtime.CompilerServices;
[assembly:InternalsVisibleTo("LegacyEnglish119FinalSingle.Probe")]
namespace InkRecognition;

public sealed partial class LegacyEnglish119ParentPostprocessing
{
    private bool finalSingleReserved;
    internal LegacyEnglish119FinalizedSingleSource ReserveFinalSingle(CancellationToken token)
    {
        token.ThrowIfCancellationRequested();lock(gate)
        {
            token.ThrowIfCancellationRequested();
            if(!complete||faulted||finalSingleReserved)throw new InvalidOperationException("One completed unreserved EN post265 owner required.");
            //Validate bounded owned counts BEFORE copying any complete point buffer.
            if(initialDescriptors.Length!=1||(BinaryPrimitives.ReadUInt16LittleEndian(initialDescriptors[0])&15)!=0)
                throw new NotSupportedException("Single ordinary descriptor required; active native multiline merge is not skipped.");
            var counts=finalChronology!.StrokePointCounts.Span;int total=0;
            if(counts.Length is<1 or>256||counts.Length!=input.Strokes.Length||published.Length is<1 or>4096)
                throw new ArgumentException("Bounded whole actual parent source required.");
            foreach(int n in counts){token.ThrowIfCancellationRequested();if(n<1||n>4096-total)throw new ArgumentException("Whole post265 point budget exceeded.");total+=n;}
            if(snapshots.Count==0||snapshots[^1].Stage!="post265")throw new InvalidDataException("Actual completed retained post265 image required.");
            var image=snapshots[^1];if(image.Rows.Count!=published.Length)throw new InvalidDataException("Whole same-source720 interval required.");
            int rangeCount=0,visits=0;
            foreach(var row in image.Rows)
            {
                token.ThrowIfCancellationRequested();if(row.Ranges.Count>65536-rangeCount)throw new ArgumentException("Whole source range budget exceeded.");rangeCount+=row.Ranges.Count;
                foreach(var r in row.Ranges)
                {
                    token.ThrowIfCancellationRequested();var a=r.First;var z=r.Last;
                    if(a.First!=z.First||a.Second<0||z.Second<a.Second||z.Second>=input.Strokes.Length||a.Third<0||z.Third<0||a.Third>=input.Strokes[a.Second].Points.Count||z.Third>=input.Strokes[z.Second].Points.Count||a.Second==z.Second&&z.Third<a.Third)
                        throw new ArgumentException("Actual pre265 source720 domain required.");
                    for(int i=a.Second;i<=z.Second;i++){token.ThrowIfCancellationRequested();int n=(i==z.Second?z.Third+1:input.Strokes[i].Points.Count)-(i==a.Second?a.Third:0);if(n>1048576-visits)throw new ArgumentException("Whole range visit budget exceeded.");visits+=n;}
                }
            }
            //This type can ONLY originate from actual fresh Parent.Prepare and
            //this exact completed continuation. No supplied maps/freshness flags.
            _=LegacyEnglish119FinalInputCopyBudget.Preflight(total,counts.Length,published.Length);
            var value=new LegacyEnglish119FinalizedSingleSource(this,original,input.Probabilities,finalChronology,image,total,token);
            token.ThrowIfCancellationRequested();finalSingleReserved=true;return value;
        }
    }
}

/// <summary>Owned fresh single ordinary post265>169C30>161D20(no pair)>
///15D360>15D920 finalized input. Multiline and preexisting text are excluded;
///this is not main acoustic or linguistic recognition.</summary>
public sealed partial class LegacyEnglish119FinalizedSingleSource
{
    private readonly object gate=new();
    private readonly LegacyOrdinarySourceInk original;
    private readonly LegacySourcePointChronology chronology;
    private readonly InkStroke[] points;
    private readonly float[] probabilities;
    private readonly byte[] descriptor,guide;
    private readonly LegacyFirstResultLineRow[] rows;
    private readonly List<string> trace=[];
    private bool entered,faulted,complete;
    private string? lastCompletedStage;
    public LegacyEnglish119ParentPostprocessing Owner{get;}
    public LegacyEnglish119MainWireContext MainWire=>Owner.MainWire;
    public bool IsComplete{get{lock(gate)return complete;}}
    public bool IsFaulted{get{lock(gate)return faulted;}}
    public bool HasEntered{get{lock(gate)return entered;}}
    public string? LastCompletedStage{get{lock(gate)return lastCompletedStage;}}
    public int ExistingTextRecordCount=>0;
    public int ExistingUtf8SegmentCount=>0;
    public int ExistingControlCount=>0;
    public string[] CopyTrace(){lock(gate)return trace.ToArray();}
    public byte[] CopyDescriptor()=>descriptor.ToArray();
    public byte[] CopyGuide()=>guide.ToArray();
    public InkStroke[] CopyStrokes()=>LegacyEnglish119ParentSubmission.Copy(points);
    public float[] CopyPhysicalProbabilities()=>probabilities.ToArray();
    public IReadOnlyList<LegacyFirstResultLineRow> CopyRows()=>Array.AsReadOnly(rows.ToArray());
    public LegacySourcePointIdentity ResolvePoint(int stroke,int point)=>chronology.Resolve(stroke,point);
    public LegacySourcePointIdentity ResolveEndingMetadata(int stroke,int point,CancellationToken token=default)=>original.ResolveEndingMetadata(chronology.Resolve(stroke,point),token);
    public static LegacyEnglish119FinalizedSingleSource Prepare(LegacyEnglish119ParentPostprocessing actualCompletedPost,CancellationToken token=default)
    {ArgumentNullException.ThrowIfNull(actualCompletedPost);return actualCompletedPost.ReserveFinalSingle(token);}
    internal LegacyEnglish119FinalizedSingleSource(LegacyEnglish119ParentPostprocessing owner,LegacyOrdinarySourceInk original,
        float[] actualProbabilities,LegacySourcePointChronology chronology,LegacyEnglish119PostprocessingSnapshot image,int boundedPointCount,CancellationToken token)
    {
        Owner=owner;this.original=original;this.chronology=chronology;token.ThrowIfCancellationRequested();
        var source=owner.MainWire.Source;
        if(!ReferenceEquals(source,owner.Parent.Model.ParentSource)||source.Offset!=0||source.SourceKind!=0x135||!source.Fingerprint.Equals("ca46297ada4a80de0d4bbd40650a8ace5c3809af9edbd493fb8f4eb692a18657",StringComparison.OrdinalIgnoreCase))throw new ArgumentException("Actual same EN fresh main owner required.");
        byte Field(string n)=>source.Fields.Single(f=>f.Owner=="parametersC0"&&f.Destination==n).CopyBytes()[0];
        if(Field("21")!=1||Field("48")!=0||Field("41")!=0||Field("50")!=0)throw new NotSupportedException("Exact fresh main native finalizer configuration required.");
        var ds=image.CopyDescriptors();var gs=image.CopyGuides();
        if(ds.Length!=1||gs.Length!=1||ds[0].Length!=88||gs[0].Length!=40)throw new ArgumentException("Single complete assigned guide required.");
        descriptor=ds[0];guide=gs[0];int I(byte[] b,int at)=>BinaryPrimitives.ReadInt32LittleEndian(b.AsSpan(at));
        if((BinaryPrimitives.ReadUInt16LittleEndian(descriptor)&15)!=0||I(descriptor,12)!=-1||I(descriptor,16)!=0||I(descriptor,36)!=0||I(descriptor,40)!=chronology.StrokePointCounts.Length-1||I(descriptor,52)!=0||I(descriptor,56)!=image.Rows.Count||I(descriptor,76)!=-1||I(descriptor,80)!=-1||I(guide,32)!=-1||I(guide,36)!=int.MinValue)
            throw new ArgumentException("Actual fresh identity-finalizer descriptor/guide/source interval required.");
        if(actualProbabilities.Length!=chronology.StrokePointCounts.Length)throw new ArgumentException("Whole current probability owner required.");
        foreach(float p in actualProbabilities){token.ThrowIfCancellationRequested();if(!float.IsFinite(p)||p<0||p>1)throw new ArgumentException("Finite actual probability required.");}
        //The source owns one image only. Full point buffers were bounded by the
        //parent preflight; no O(stages?whole-source) retained history is created.
        int identityCount=0;for(int s=0;s<chronology.StrokePointCounts.Length;s++)for(int p=0;p<chronology.StrokePointCounts.Span[s];p++)
        {token.ThrowIfCancellationRequested();identityCount++;_=original.ResolveEndingMetadata(chronology.Resolve(s,p),token);}
        if(identityCount!=boundedPointCount)throw new InvalidDataException("Whole metadata identity count differs before copies.");
        points=image.CopyStrokes();probabilities=actualProbabilities.ToArray();rows=image.Rows.ToArray();
        int count=0;for(int s=0;s<points.Length;s++)
        {
            token.ThrowIfCancellationRequested();if(points[s].Points.Count!=chronology.StrokePointCounts.Span[s])throw new InvalidDataException("Actual completed image/chronology dimensions differ.");
            for(int p=0;p<points[s].Points.Count;p++){token.ThrowIfCancellationRequested();count++;_=original.ResolveEndingMetadata(chronology.Resolve(s,p),token);}
        }
        if(count!=boundedPointCount)throw new InvalidDataException("Whole point count changed during snapshot.");
    }
    public LegacyEnglish119FinalizedSingleSource Run(CancellationToken token=default)=>RunCore(null,token);
    internal LegacyEnglish119FinalizedSingleSource RunPrepared(Action<string> checkpoint,CancellationToken token=default)=>RunCore(checkpoint,token);
    private LegacyEnglish119FinalizedSingleSource RunCore(Action<string>? checkpoint,CancellationToken token)
    {
        token.ThrowIfCancellationRequested();lock(gate)
        {token.ThrowIfCancellationRequested();if(entered)throw new InvalidOperationException("Finalization is once-only.");entered=true;}
        void Stage(string name)
        {lock(gate){lastCompletedStage=name;trace.Add(name);}checkpoint?.Invoke(name);token.ThrowIfCancellationRequested();}
        try
        {
            lock(gate)trace.Add("enter");checkpoint?.Invoke("enter");token.ThrowIfCancellationRequested();
            //All branch predicates were validated against owned fresh state.
            //Single descriptor169C30 returns immediately;161D20 count<=2
            //returns no pair. SourceV70 reads only physical1B&1, cleared here by15D360;
            //no claim is made that other metadata flag bits stay zero.
            Stage("169C30:single");Stage("161D20:no-pair");Stage("15D360:fresh");Stage("15D920:fresh-identity");Stage("prior130:cleared");
            lock(gate){complete=true;trace.Add("complete");}return this;
        }
        catch{lock(gate){faulted=true;trace.Add("fault");}throw;}
    }
}

//Conservative additional finalized-input snapshot copy envelope, not RSS.
//The immutable original/current lineage and source720 payloads are shared.
internal static class LegacyEnglish119FinalInputCopyBudget
{
    internal static long Preflight(int points,int physical,int rows)
    {
        if(points is<1 or>4096||physical is<1 or>256||physical>points||rows is<1 or>4096)throw new ArgumentException("Bounded complete finalized snapshot required.");
        long bytes=checked(1024L+points*8L+physical*160L+rows*8L);
        if(bytes>128*1024)throw new ArgumentException("Whole finalized-input copy envelope exceeded.");return bytes;
    }
}
