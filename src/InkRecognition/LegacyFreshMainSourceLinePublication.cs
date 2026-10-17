using System.Buffers.Binary;

namespace InkRecognition;

/// <summary>One actual0283B0 outer-alternative call to021AB0. Allocation count
/// belongs to source2DA; the separately reset wrapper68 controls write indices.</summary>
public sealed class LegacyOwnedMainAlternativeSourceLines
{
    public LegacyOwnedMainWordSourceAlternative Alternative {get;}
    public int PreviousLineCount {get;}
    public int LineCount {get;}
    public int WrapperOffsetBefore=>0;
    public int WrapperOffsetAfter {get;}
    private readonly byte[] storage;
    internal LegacyOwnedMainAlternativeSourceLines(LegacyOwnedMainWordSourceAlternative alternative,
        int previous,LegacyNativeSourceLinePublication lines)
    {Alternative=alternative;PreviousLineCount=previous;LineCount=lines.Lines.Length;
     WrapperOffsetAfter=lines.NextWrapperLineOffset;storage=LegacyFreshMainSourceLinePublication.Encode(lines.Lines);}
    public byte[] CopyStorage56()=>storage.ToArray();
}

/// <summary>Fresh one-source configured2 WORD choices -> actual021AB0 guide
/// publication. Retains its actual descriptor/guide owner and independent
/// append/write counters. TEXT registration and accepted document mapping are
/// separate; no raw stroke bounding height is substituted for guideC/10.</summary>
public sealed class LegacyFreshMainSourceLinePublication
{
    private readonly byte[] descriptor,guide,storage;
    private readonly LegacyNativePublishedSourceLine[] lines;
    public LegacyOwnedMainWordSourceChoicesResult WordChoices {get;}
    public LegacyFreshMainSingleLineSampled Sampled=>WordChoices.Owner.Sources.CandidateSource.Sampled;
    public IReadOnlyList<LegacyOwnedMainAlternativeSourceLines> Alternatives {get;}
    public IReadOnlyList<LegacyNativePublishedSourceLine> Lines {get;}
    /// <summary>00C860.A4 zero ->2B3.A9/12D000C ->218.B4false disables
    /// source2D0/081E90; fresh16A130 secondaryA8 remains zero.</summary>
    public float SourceRotation=>0;
    public int LineCount=>lines.Length;
    private LegacyFreshMainSourceLinePublication(LegacyOwnedMainWordSourceChoicesResult choices,
        byte[] descriptor,byte[] guide,LegacyNativePublishedSourceLine[] lines,
        LegacyOwnedMainAlternativeSourceLines[] alternatives)
    {WordChoices=choices;this.descriptor=descriptor;this.guide=guide;this.lines=lines;
     Lines=Array.AsReadOnly(lines);Alternatives=Array.AsReadOnly(alternatives);storage=Encode(lines);}

    public static LegacyFreshMainSourceLinePublication PublishFresh(LegacyOwnedMainWordSourceChoicesResult actualWordChoices,
        CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(actualWordChoices);token.ThrowIfCancellationRequested();
        var owner=actualWordChoices.Owner;
        if(owner.IsFaulted||!ReferenceEquals(actualWordChoices.Source.Owner,owner)||
            actualWordChoices.Alternatives.Count!=2||actualWordChoices.Source.Choices.Count!=owner.WordViews.Count)
            throw new ArgumentException("Successful actual configured shared-SPAN WORD source publication required.");
        foreach(var word in owner.WordViews)word.Validate(token);
        var candidate=owner.Sources.CandidateSource;var sampled=candidate.Sampled;
        var firstLines=sampled.Owner.Preparation.Input.Input.Input;
        //These sealed owners accept fresh direct main0/no prior/no property
        //events. The configured controller changes only secondary count18,
        //after input preprocessing; it does not enable container12D000C.
        if(firstLines.Owner.Controller.SourceOffset!=0||sampled.Owner.SourceDescriptorOrdinal!=0||
            !ReferenceEquals(firstLines,candidate.Preprocessed.Lines))
            throw new ArgumentException("Identical fresh direct main input/guide lineage required.");
        byte[] descriptor=sampled.Owner.CopyDescriptor().ToArray();int line=BinaryPrimitives.ReadInt32LittleEndian(descriptor.AsSpan(16));
        byte[][] guides=firstLines.CopyGuides();
        if(descriptor.Length!=88||(descriptor[0]&15)!=0||line!=0||guides.Length!=1||guides[0].Length!=40)
            throw new NotSupportedException("Actual one assigned ordinary source line0 required.");
        byte[] guide=guides[0];
        var input=new LegacyNativeSourceLineInput(descriptor[0],line);
        var actualGuide=new LegacyNativeSourceLineGuide(F(8),F(12),F(16),F(20),F(24));
        LegacyNativePublishedSourceLine[] lines=[];var snapshots=new List<LegacyOwnedMainAlternativeSourceLines>();
        foreach(var alternative in actualWordChoices.Alternatives)
        {
            token.ThrowIfCancellationRequested();alternative.Candidate.Validate(token);
            if(!ReferenceEquals(alternative.Source,actualWordChoices.Source)||
                !ReferenceEquals(alternative.Candidate.SourceIdentity,candidate.SourceIdentity)||
                !ReferenceEquals(alternative.Candidate.TopOwner,candidate.TopOwner)||
                !ReferenceEquals(alternative.Candidate.Sampled,sampled))
                throw new ArgumentException("Actual native-order alternatives of this identical source/TOP required.");
            //01C010 starts F8/count100 at zero.0283B0 resets wrapper68 for
            //each alternative;021AB0 appends N, but writes at wrapper68+i.
            int previous=lines.Length;
            var next=LegacyNativeSourceLinePublication.Append(lines,0,[input],[actualGuide],0,token);
            snapshots.Add(new(alternative,previous,next));lines=next.Lines.ToArray();
        }
        return new(actualWordChoices,descriptor,guide,lines,snapshots.ToArray());
        float F(int at)=>BinaryPrimitives.ReadSingleLittleEndian(guide.AsSpan(at));
    }
    public byte[] CopyDescriptor88()=>descriptor.ToArray();
    public byte[] CopyGuide40()=>guide.ToArray();
    public byte[] CopyStorage56()=>storage.ToArray();
    internal static byte[] Encode(ReadOnlySpan<LegacyNativePublishedSourceLine> lines)
    {
        byte[] result=new byte[checked(lines.Length*56)];
        for(int i=0;i<lines.Length;i++)
        {
            var line=lines[i];var row=result.AsSpan(i*56,56);
            BinaryPrimitives.WriteDoubleLittleEndian(row[8..],line.OriginX);
            BinaryPrimitives.WriteDoubleLittleEndian(row[16..],line.OriginY);
            BinaryPrimitives.WriteDoubleLittleEndian(row[24..],line.Angle);
            BinaryPrimitives.WriteDoubleLittleEndian(row[32..],line.XHeight);
            BinaryPrimitives.WriteInt64LittleEndian(row[40..],line.OwnerTag);
            BinaryPrimitives.WriteDoubleLittleEndian(row[48..],line.UnadjustedBaseline);
        }
        return result;
    }
}
