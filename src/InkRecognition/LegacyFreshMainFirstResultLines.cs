using System.Buffers.Binary;

namespace InkRecognition;

public sealed record LegacyMainFirstSubmission(int DescriptorOrdinal,LegacyFreshFirstResultOwnershipFactory Factory,
    ReadOnlyMemory<int> CurrentParentPhysicalIndices);
public sealed record LegacyMainSelectedFirstWord(int DescriptorOrdinal,LegacyFirstMaterializedPlainWord Word,
    LegacyOrdinaryGraphSourceRanges SourceRanges);

/// <summary>Owned current main source15F050 -> each ordinary descriptor's first
/// child submission -> actual selected275/source720 ->24B guides. First winners
/// remain required actual result owners; this does not fake their selection or
/// substitute first-child frames for main guides.</summary>
public sealed class LegacyFreshMainFirstResultLines
{
    public LegacyFreshMainPhysicalReordering Input {get;}
    public LegacyMainFirstResultLineController Controller {get;}
    public IReadOnlyList<LegacyMainFirstSubmission> FirstSubmissions {get;}
    private LegacyFreshMainFirstResultLines(LegacyFreshMainPhysicalReordering input,
        LegacyMainFirstResultLineController controller,LegacyMainFirstSubmission[] submissions)
    {Input=input;Controller=controller;FirstSubmissions=Array.AsReadOnly(submissions);}
    public static LegacyFreshMainFirstResultLines Prepare(ReadOnlySpan<byte> resource,
        LegacyFreshMainPhysicalReordering actualInput,CancellationToken cancellationToken=default)
    {
        ArgumentNullException.ThrowIfNull(actualInput);cancellationToken.ThrowIfCancellationRequested();
        var controller=LegacyMainFirstResultLineController.PrepareOwnedCurrentSource(resource,actualInput,cancellationToken);
        var descriptors=actualInput.CopyDescriptors();var submissions=new List<LegacyMainFirstSubmission>();
        for(int d=0;d<descriptors.Length;d++)
        {
            if((BinaryPrimitives.ReadUInt16LittleEndian(descriptors[d])&15)==3)continue;
            int first=I(descriptors[d],36),last=I(descriptors[d],40);
            int[] map=LegacyFirstResultSourceRecord.SelectChildPhysicalIndices(controller.Source.Strokes,first,last,0,
                actualInput.Input.Input.Scale.Scales.Primary,cancellationToken);
            var factory=LegacyFreshFirstResultOwnershipFactory.Prepare(resource,controller.FirstSourceOffset,
                map.Select(i=>controller.Source.Strokes[i]).ToArray(),cancellationToken);
            submissions.Add(new(d,factory,map));
        }
        return new(actualInput,controller,submissions.ToArray());
    }

    /// <summary>Every supplied result must belong to that descriptor's actual
    /// child factory/graph. Parent physical maps come from real split24/28 fields,
    /// source72034/38 intervals are materialized internally, and24B chooses main
    /// guides from those actual rows. Empty-first/next24A fallback is refused until
    /// owned; no whole-word box or first-row height is inserted as its replacement.</summary>
    public LegacyFreshMainFirstResultLineSet ApplyFirstResults(IReadOnlyList<LegacyMainSelectedFirstWord> actualResults,
        CancellationToken cancellationToken=default)
    {
        ArgumentNullException.ThrowIfNull(actualResults);cancellationToken.ThrowIfCancellationRequested();
        if(actualResults.Count!=FirstSubmissions.Count||actualResults.Select(r=>r.DescriptorOrdinal).Distinct().Count()!=actualResults.Count)
            throw new ArgumentException("One actual first result per ordinary descriptor required.");
        var descriptors=Input.CopyDescriptors();var records=new List<LegacyFirstResultSourceRecord>();
        foreach(var submission in FirstSubmissions)
        {
            var result=actualResults.SingleOrDefault(r=>r.DescriptorOrdinal==submission.DescriptorOrdinal)
                ??throw new ArgumentException("First result lacks actual parent descriptor.");
            if(result.Word is null||result.SourceRanges is null||!ReferenceEquals(result.Word.Result.Factory,submission.Factory))
                throw new ArgumentException("Selected first result belongs to another parent submission.");
            var descriptor=descriptors[submission.DescriptorOrdinal];
            int[] actualMap=LegacyFirstResultSourceRecord.SelectChildPhysicalIndices(Controller.Source.Strokes,
                I(descriptor,36),I(descriptor,40),0,Input.Input.Input.Scale.Scales.Primary,cancellationToken);
            if(!submission.CurrentParentPhysicalIndices.Span.SequenceEqual(actualMap))
                throw new ArgumentException("Child submission physical map differs from its actual parent descriptor.");
            var rows=LegacyFirstResultSourceMaterializer.Materialize(result.Word,result.SourceRanges,Controller.FirstCategoryOwner,
                actualMap,Controller.Source.Strokes,cancellationToken);
            if(rows.Length==0)throw new NotSupportedException("Empty first result requires the actual main24A fallback lifecycle.");
            descriptors[submission.DescriptorOrdinal]=LegacyFirstResultSourceRecord.PublishDescriptorInterval(
                descriptors[submission.DescriptorOrdinal],records.Count,rows.Length,cancellationToken);
            records.AddRange(rows);
        }
        if(records.Count>4096)throw new ArgumentException("Bounded source720 parent storage required.");
        var owned=records.Select(r=>new LegacyMainOwnedFirstResultLineRow(Controller,r)).ToArray();
        var guides=Array.Empty<ReadOnlyMemory<byte>>();var outputs=new List<LegacyMainFirstResultLineOutput>();
        var points=Controller.Source.Strokes.Select(s=>s.Points.ToArray()).ToArray();
        var transformed=records.Select(LegacyFirstResultLineRow.FromFirstResult).ToArray();
        foreach(var submission in FirstSubmissions)
        {
            int d=submission.DescriptorOrdinal;
            //170400 leaves ignored temporary guide bytes unspecified. Portable
            //storage zeroes only that inactive template, not a geometric result.
            var output=Controller.ProcessDescriptor(descriptors[d],owned,guides,new byte[40],cancellationToken);
            if(output.Deskew is not{} deskew||output.NewLineFit is null)
                throw new InvalidDataException("Actual nonempty first rows did not publish main24B geometry.");
            descriptors[d]=output.CopyDescriptor().ToArray();guides=output.CopyGuides().Select(g=>new ReadOnlyMemory<byte>(g)).ToArray();
            var current=deskew.CopyPhysicalPoints();foreach(int physical in submission.CurrentParentPhysicalIndices.Span)points[physical]=current[physical];
            int first=I(descriptors[d],52),end=I(descriptors[d],56);
            for(int r=first;r<end;r++)transformed[r]=deskew.Rows[r];
            outputs.Add(output);
        }
        return new(this,descriptors,guides.Select(g=>g.ToArray()).ToArray(),points,records.ToArray(),transformed,outputs.ToArray());
    }
    private static int I(ReadOnlySpan<byte> b,int at)=>BinaryPrimitives.ReadInt32LittleEndian(b[at..]);
}

/// <summary>Actual source geometry immediately after24B/before main24A/24D/29B/265.
/// Source720 publication references remain in their parent retained-point domain;
/// pre-deskew guideC/10 is distinct from current guide0/4.</summary>
public sealed class LegacyFreshMainFirstResultLineSet
{
    private readonly byte[][] descriptors,guides;private readonly InkPoint[][] points;
    public LegacyFreshMainFirstResultLines Owner {get;}
    public IReadOnlyList<LegacyFirstResultSourceRecord> PublishedRows {get;}
    public IReadOnlyList<LegacyFirstResultLineRow> CurrentRows {get;}
    public IReadOnlyList<LegacyMainFirstResultLineOutput> Outputs {get;}
    public LegacySourcePointChronology Chronology=>Owner.Controller.Source.Chronology;
    internal LegacyFreshMainFirstResultLineSet(LegacyFreshMainFirstResultLines owner,byte[][] descriptors,byte[][] guides,
        InkPoint[][] points,LegacyFirstResultSourceRecord[] published,LegacyFirstResultLineRow[] current,LegacyMainFirstResultLineOutput[] outputs)
    {Owner=owner;this.descriptors=descriptors;this.guides=guides;this.points=points;PublishedRows=Array.AsReadOnly(published);CurrentRows=Array.AsReadOnly(current);Outputs=Array.AsReadOnly(outputs);}
    public byte[][] CopyDescriptors()=>descriptors.Select(d=>d.ToArray()).ToArray();
    public byte[][] CopyGuides()=>guides.Select(g=>g.ToArray()).ToArray();
    public InkPoint[][] CopyPhysicalPoints()=>points.Select(p=>p.ToArray()).ToArray();
}
