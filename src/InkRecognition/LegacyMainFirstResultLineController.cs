using System.Buffers.Binary;

namespace InkRecognition;

/// <summary>A720-row produced against one main source's retained parent points.
/// The public record producer still needs actual selected glyph alternatives/ranges.</summary>
public sealed class LegacyMainOwnedFirstResultLineRow
{
    internal LegacyMainFirstResultLineController Owner {get;}
    public LegacyFirstResultSourceRecord Record {get;}
    internal LegacyMainOwnedFirstResultLineRow(LegacyMainFirstResultLineController owner,LegacyFirstResultSourceRecord record)
    {Owner=owner;Record=record;}
}

public sealed class LegacyMainFirstResultLineOutput
{
    private readonly byte[] descriptor;
    private readonly byte[][] guides;
    public LegacyFirstResultLineDeskewResult? Deskew {get;}
    public LegacyCharacterLineCompletedFit? NewLineFit {get;}
    public bool AddedLine=>NewLineFit is not null;
    internal LegacyMainFirstResultLineOutput(byte[] descriptor,byte[][] guides,LegacyFirstResultLineDeskewResult? deskew,LegacyCharacterLineCompletedFit? fit)
    {this.descriptor=descriptor;this.guides=guides;Deskew=deskew;NewLineFit=fit;}
    public ReadOnlyMemory<byte> CopyDescriptor()=>descriptor.ToArray();
    public byte[][] CopyGuides()=>guides.Select(g=>g.ToArray()).ToArray();
}

/// <summary>Bounded fresh no-prior/no-explicit-guideline source24B170400 branch.
/// Owns the resource-selected parent AL, raw ingress points and runtime resolution
/// (mandatory164520 input). Existing source guides and initial unknown-guide tail
/// remain genuine upstream caller inputs; this is not whole main247 orchestration.</summary>
public sealed class LegacyMainFirstResultLineController
{
    public LegacySourceSamplerCategoryOwner FirstCategoryOwner {get;}
    public LegacyOrdinarySourceInk Source {get;}
    public LegacySourceAlphabetByteLookup Alphabet {get;}
    public LegacyFirstResultLineMarkers Markers {get;}
    public LegacyFirstResultLineProfile Profile {get;}
    public float PhysicalResolution {get;}
    public int SourceOffset {get;}
    public int Processor24BOffset {get;}
    public int FirstSourceOffset {get;}
    /// <summary>Loaded249's158BC0 tail is preserved by155DA0's final1C8 qword copy,
    /// after its initial reset to1. This is not the unbound raw constructor policy.</summary>
    public float SourceScoreExponent=>FirstCategoryOwner.Serialized249SourceScoreExponent!.Value;
    private LegacyMainFirstResultLineController(LegacyOrdinarySourceInk source,LegacySourceAlphabetByteLookup alphabet,
        LegacyFirstResultLineProfile profile,LegacySourceSamplerCategoryOwner categories,float resolution,int sourceOffset,int processorOffset,int firstSourceOffset)
    {Source=source;Alphabet=alphabet;Markers=alphabet.ResolveLineMarkers();Profile=profile;FirstCategoryOwner=categories;
        PhysicalResolution=resolution;SourceOffset=sourceOffset;Processor24BOffset=processorOffset;FirstSourceOffset=firstSourceOffset;}

    /// <summary>Selected resource247/v10d owns its processor array: require first249
    /// containing a direct first247/v10d source, and exactly one resource24B/v103.
    /// No previous-source object, explicit page lines or property-event overrides
    /// are synthesized. Physical resolution must be the actual source218 setter value.</summary>
    public static LegacyMainFirstResultLineController Prepare(ReadOnlySpan<byte> resource,int sourceOffset,
        IReadOnlyList<InkStroke> rawPhysicalStrokes,float actualPhysicalResolution,CancellationToken cancellationToken=default)
        =>PrepareCore(resource,sourceOffset,rawPhysicalStrokes,actualPhysicalResolution,null,cancellationToken);

    /// <summary>Actual current parent source after typed main15B260/15F050.
    /// Uses its retained buffers, reordered probabilities and original metadata
    /// chronology without another source filter. This method does not copy a
    /// first-child frame into the main source or synthesize source descriptors.</summary>
    public static LegacyMainFirstResultLineController PrepareOwnedCurrentSource(ReadOnlySpan<byte> resource,
        LegacyFreshMainPhysicalReordering actualCurrentSource,CancellationToken cancellationToken=default)
    {
        ArgumentNullException.ThrowIfNull(actualCurrentSource);cancellationToken.ThrowIfCancellationRequested();
        var source=LegacyOrdinarySourceInk.CreateOwnedMainPhysicalReorderedView(actualCurrentSource,cancellationToken);
        if(!Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(resource)).Equals(
            actualCurrentSource.Input.Input.SourceFingerprint,StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Current parent must retain its exact selected main resource.");
        return PrepareCore(resource,actualCurrentSource.Input.Input.SourceOffset,source.Strokes,
            actualCurrentSource.Input.Input.PhysicalResolution,source,cancellationToken);
    }

    private static LegacyMainFirstResultLineController PrepareCore(ReadOnlySpan<byte> resource,int sourceOffset,
        IReadOnlyList<InkStroke> rawPhysicalStrokes,float actualPhysicalResolution,LegacyOrdinarySourceInk? ownedCurrentSource,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if(!float.IsFinite(actualPhysicalResolution)||actualPhysicalResolution<0||actualPhysicalResolution>1_000_000)
            throw new ArgumentException("Mandatory finite actual runtime physical resolution required.");
        byte[] snapshot=resource.ToArray();var config=LegacyOrdinarySourceInk.ReadV10dResourceProfile(snapshot,sourceOffset);
        var alphabet=LegacySourceAlphabetByteLookup.ReadInlineV10dSource(snapshot,sourceOffset);
        int parentEnd=NodeEnd(snapshot,sourceOffset,0x247,snapshot.Length);
        int at=config.EnclosureFlagOffset+12;
        if(at>parentEnd-4)throw new InvalidDataException("Missing source postprocessor count.");
        int count=I32(snapshot,at);at+=4;if(count is <1 or >256)throw new InvalidDataException("Bounded source postprocessor array required.");
        int first=-1,processor=-1,firstProcessor=-1;
        for(int i=0;i<count;i++)
        {
            cancellationToken.ThrowIfCancellationRequested();int end=NodeEnd(snapshot,at,null,parentEnd);int kind=I32(snapshot,at+8);
            if(i==0)
            {
                if(kind!=0x249)throw new NotSupportedException("Fresh source first processor must be249.");
                firstProcessor=at;first=at+16;_=NodeEnd(snapshot,first,0x247,end);
            }
            if(kind==0x24b){if(processor!=-1)throw new NotSupportedException("Multiple24B processors not supported by this bounded stage.");processor=at;}
            at=end;
        }
        if(processor<0)throw new NotSupportedException("Resource source does not own24B.");
        var profile=LegacyFirstResultLineProfile.ReadV103Node(snapshot,processor);
        if(profile.Option18!=0)throw new NotSupportedException("24B nonzero Option18 early-return branch is outside the fitting stage.");
        var categories=LegacySourceSamplerCategoryOwner.ReadFirstChildOf249(snapshot,firstProcessor,cancellationToken);
        var source=ownedCurrentSource??LegacyOrdinarySourceInk.Prepare(rawPhysicalStrokes,config,cancellationToken);
        if(source.Profile!=config)throw new ArgumentException("Current parent profile differs from selected source.");
        return new(source,alphabet,profile,categories,actualPhysicalResolution,sourceOffset,processor,first);
    }

    public LegacyMainOwnedFirstResultLineRow CreateFirstResultRow(ReadOnlySpan<byte> actualAlternatives40,
        int selectedAlternative,IReadOnlyList<LegacyFirstResultSourceRange> actualChildRanges,IReadOnlyList<int> actualPhysicalMap,
        CancellationToken cancellationToken=default)=>new(this,
            LegacyFirstResultSourceRecord.Create(actualAlternatives40,selectedAlternative,actualChildRanges,actualPhysicalMap,
                Source.Strokes,FirstCategoryOwner.Categories,SourceScoreExponent,cancellationToken));

    /// <summary>One ordinary descriptor from170400. Already assigned line: only16F170,
    /// with no C1A refit (fresh runtime30=0). Unknown nonempty line:16F6F0→169750
    /// disabled-metadata indices→16F170 and append. Unknown empty line is unchanged.
    /// The initial guide template carries opaque caller bytes; native18 is initialized
    /// to zero, native1C low byte zero and20/24 set to−1/INTMIN.</summary>
    public LegacyMainFirstResultLineOutput ProcessDescriptor(ReadOnlySpan<byte> descriptor88,
        IReadOnlyList<LegacyMainOwnedFirstResultLineRow> ownedRows,IReadOnlyList<ReadOnlyMemory<byte>> actualExistingGuides,
        ReadOnlySpan<byte> actualUnknownGuideTemplate,CancellationToken cancellationToken=default)
    {
        ArgumentNullException.ThrowIfNull(ownedRows);ArgumentNullException.ThrowIfNull(actualExistingGuides);
        cancellationToken.ThrowIfCancellationRequested();
        if(descriptor88.Length!=88||ownedRows.Count>4096||actualExistingGuides.Count>4095||actualUnknownGuideTemplate.Length!=40)
            throw new ArgumentException("Actual bounded descriptor, source row owners and40-byte guides required.");
        int kind=BinaryPrimitives.ReadUInt16LittleEndian(descriptor88)&15;if(kind is not (0 or 6))throw new NotSupportedException("Only ordinary descriptor kinds0/6 are supported.");
        int line=I32(descriptor88,16),first=I32(descriptor88,52),end=I32(descriptor88,56);
        int firstPhysical=I32(descriptor88,36),lastPhysical=I32(descriptor88,40);
        if(first<0||end<first||end>ownedRows.Count||end-first>256||firstPhysical<0||lastPhysical<firstPhysical||lastPhysical>=Source.Strokes.Count||
            line>=actualExistingGuides.Count)
            throw new ArgumentException("Actual source descriptor indices exceed owned source buffers.");
        var rows=new LegacyFirstResultSourceRecord[ownedRows.Count];
        for(int i=0;i<rows.Length;i++)
        {cancellationToken.ThrowIfCancellationRequested();if(ownedRows[i] is null||!ReferenceEquals(ownedRows[i].Owner,this))throw new ArgumentException("First-result row belongs to another retained source owner.");rows[i]=ownedRows[i].Record;}
        byte[] descriptor=descriptor88.ToArray();byte[][] guides=new byte[actualExistingGuides.Count][];
        for(int i=0;i<guides.Length;i++){if(actualExistingGuides[i].Length!=40)throw new ArgumentException("Complete existing source guides required.");guides[i]=actualExistingGuides[i].ToArray();}
        if(line<0&&first==end)return new(descriptor,guides,null,null);
        var fitProfile=Profile.PrepareFitProfile(PhysicalResolution,0,[]);LegacyCharacterLineCompletedFit? fit=null;byte[] guide;
        if(line<0)
        {
            var items=LegacyFirstResultLinePreparation.Prepare(rows,first,end,Markers,cancellationToken);
            var bounds=LegacyCharacterLineFitCompletion.ScanPhysicalInterval(Source.Strokes,firstPhysical,lastPhysical,cancellationToken);
            fit=LegacyCharacterLineFitCompletion.Estimate(items,fitProfile,bounds,false,cancellationToken);
            guide=actualUnknownGuideTemplate.ToArray();WF(guide,0,fit.Value.Baseline);WF(guide,4,fit.Value.Height);WF(guide,8,fit.Value.Slope);
            WF(guide,12,fit.Value.Baseline);WF(guide,16,fit.Value.Height);WF(guide,24,0);guide[28]=0;
            BinaryPrimitives.WriteInt32LittleEndian(guide.AsSpan(32),-1);BinaryPrimitives.WriteInt32LittleEndian(guide.AsSpan(36),int.MinValue);
            line=guides.Length;Array.Resize(ref guides,line+1);BinaryPrimitives.WriteInt32LittleEndian(descriptor.AsSpan(16),line);
        }
        else guide=guides[line];
        var deskew=LegacyFirstResultLineDeskew.Apply(Source.Strokes,firstPhysical,lastPhysical,
            rows.Select(LegacyFirstResultLineRow.FromFirstResult).ToArray(),first,end,guide,fitProfile.MinimumHeight,false,cancellationToken);
        guides[line]=deskew.CopyGuide().ToArray();return new(descriptor,guides,deskew,fit);
    }
    private static int NodeEnd(ReadOnlySpan<byte>b,int at,int? kind,int enclosingEnd)
    {
        if(at<0||enclosingEnd>b.Length||at>enclosingEnd-16||I32(b,at)!=0x101fe90||BinaryPrimitives.ReadUInt16LittleEndian(b[(at+12)..])!=0)
            throw new InvalidDataException("Complete unencoded resource envelope required.");
        uint length=BinaryPrimitives.ReadUInt32LittleEndian(b[(at+4)..]);if(length<8||length>enclosingEnd-at-8||kind is{} type&&I32(b,at+8)!=type)
            throw new InvalidDataException("Wrong/truncated resource envelope.");return checked(at+8+(int)length);
    }
    private static int I32(ReadOnlySpan<byte>b,int at)=>BinaryPrimitives.ReadInt32LittleEndian(b[at..]);
    private static void WF(Span<byte>b,int at,float v)=>BinaryPrimitives.WriteSingleLittleEndian(b[at..],v);
}
