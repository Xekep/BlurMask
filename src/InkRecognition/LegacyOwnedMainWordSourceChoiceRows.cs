using System.Buffers.Binary;

namespace InkRecognition;

/// <summary>Native24-byte CHAR reference row. Pointer08 is retained as the
/// actual typed Source sidecar, not a fabricated native address.</summary>
public sealed class LegacyOwnedMainWordCharacterLink
{
    public LegacyOwnedMainTextSourcePayload Source {get;}
    public int SelectedMarker {get;}
    internal LegacyOwnedMainWordCharacterLink(LegacyOwnedMainTextSourceChoice choice)
    {Source=choice.Source;SelectedMarker=choice.SelectedMarker;}
    public byte[] CopyScalarStorage24()
    {byte[] data=new byte[24];BinaryPrimitives.WriteInt32LittleEndian(data.AsSpan(16),SelectedMarker);return data;}
}

/// <summary>One native48-byte WORD choice produced from an actual275 row.
/// All zero pointer fields have explicit owned sidecars; no WORD120 scores
/// are transplanted into the fresh zero-initialized choice.</summary>
public sealed class LegacyOwnedMainWordSourceChoiceRow
{
    public LegacyOwnedMainSpanWordView Word {get;}
    public uint Flags {get;}
    public IReadOnlyList<LegacyOwnedMainTextSourceChoice> Glyphs {get;}
    public IReadOnlyList<LegacyOwnedMainWordCharacterLink> CharacterLinks {get;}
    internal uint SourceFlags {get;}
    internal LegacyOwnedMainWordSourceChoiceRow(LegacyOwnedMainSpanWordView word,LegacyPreparedMainTextSourceWordRender rendered)
    {Word=word;Flags=rendered.SelectedFlags;SourceFlags=rendered.SourceFlags;
     Glyphs=Array.AsReadOnly(rendered.Glyphs);CharacterLinks=Array.AsReadOnly(rendered.Glyphs.Select(g=>new LegacyOwnedMainWordCharacterLink(g)).ToArray());}
    public byte[] CopyScalarStorage48()
    {
        byte[] data=new byte[48];BinaryPrimitives.WriteUInt32LittleEndian(data.AsSpan(8),Flags);
        BinaryPrimitives.WriteInt32LittleEndian(data.AsSpan(24),CharacterLinks.Count);
        BinaryPrimitives.WriteInt32LittleEndian(data.AsSpan(28),CharacterLinks.Count);return data;
    }
}

public sealed class LegacyOwnedMainWordSourcePayload
{
    private readonly LegacyNativeInkPositionRange[] ordinaryRanges;
    public LegacyFreshMainWordSourceChoices Owner {get;}
    public LegacyOwnedMainTextSourceNode SourceNode {get;}
    public IReadOnlyList<LegacyOwnedMainWordSourceChoiceRow> Choices {get;}
    public uint Flags {get;private set;}
    public long SelectionId {get;private set;}=long.MaxValue;
    public LegacyOwnedFreshTextSelection? Selection {get;private set;}
    public int Field08=>-1;
    public ulong Field00=>0;
    internal LegacyOwnedMainWordSourcePayload(LegacyFreshMainWordSourceChoices owner,LegacyOwnedMainTextSourceNode node,
        LegacyOwnedMainWordSourceChoiceRow[] choices)
    {
        Owner=owner;SourceNode=node;Choices=Array.AsReadOnly(choices);Flags=0x200000;
        var ops=LegacyOwnedRawInkRangeOperations.Prepare(owner.Document);LegacyNativeInkPositionRange[] ranges=[];
        foreach(var row in choices)
        {
            if((row.SourceFlags&0x200000)==0)Flags&=~0x200000u;Flags|=row.Flags;
            foreach(var glyph in row.Glyphs)if(glyph.ProcessedSource.IsOrdinarySource)
                ranges=ops.Union(ranges,glyph.Source.SourceNode.FirstProcessedSource!.CopyRanges());
        }
        ordinaryRanges=ranges;
    }
    public LegacyNativeInkPositionRange[] CopySourceKeyRanges()=>SourceNode.FirstWordSource!.CopyRanges();
    public LegacyNativeInkPositionRange[] CopyOrdinaryRanges()=>ordinaryRanges.ToArray();
    internal void Register(CancellationToken token)
    {
        if((Flags&0x200000)!=0){SelectionId=-1;return;}
        Selection=Owner.SelectionContext.CreateWordSelection(this,token);SelectionId=Selection.SelectionId;
        var key=CopySourceKeyRanges();bool same=key.Length==1&&key[0].First.SourceIndex==key[0].Last.SourceIndex&&
            key[0].First.PointIndex==key[0].Last.PointIndex&&(key[0].First.Fraction==key[0].Last.Fraction||
            key[0].First.Fraction==255||key[0].Last.Fraction==255);
        Flags=(Flags&~0x800000u)|(same?0x800000u:0);
    }
}

/// <summary>One actual TOP alternative's32-byte document-choice row and its
/// single24-byte WORD reference. This is not0283B0 acceptance.</summary>
public sealed class LegacyOwnedMainWordSourceAlternative
{
    public LegacyOwnedMainCandidateSource Candidate {get;}
    public LegacyOwnedMainWordSourcePayload Source {get;}
    public int SelectedWordIndex {get;}
    public bool ReusedWordSourceKey {get;}
    internal LegacyOwnedMainWordSourceAlternative(LegacyOwnedMainCandidateSource candidate,
        LegacyOwnedMainWordSourcePayload source,bool reused)
    {
        Candidate=candidate;Source=source;ReusedWordSourceKey=reused;SelectedWordIndex=candidate.WordHead18!.WordIndex;
        if((uint)SelectedWordIndex>=(uint)source.Choices.Count)throw new InvalidDataException("Actual279 ordinal escapes its275 choice array.");
    }
    public byte[] CopyScalarStorage24()
    {byte[] data=new byte[24];BinaryPrimitives.WriteInt32LittleEndian(data.AsSpan(16),SelectedWordIndex);return data;}
    public byte[] CopyScalarStorage32()
    {
        byte[] data=new byte[32];BinaryPrimitives.WriteUInt32LittleEndian(data.AsSpan(8),Source.Choices[SelectedWordIndex].Flags&1023);
        BinaryPrimitives.WriteInt32LittleEndian(data.AsSpan(24),1);BinaryPrimitives.WriteInt32LittleEndian(data.AsSpan(28),1);return data;
    }
}

public sealed class LegacyOwnedMainWordSourceChoicesResult
{
    public LegacyFreshMainWordSourceChoices Owner {get;}
    public LegacyOwnedMainWordSourcePayload Source {get;}
    public IReadOnlyList<LegacyOwnedMainWordSourceAlternative> Alternatives {get;}
    public IReadOnlyList<LegacyOwnedFreshTextSelection> PendingSelections {get;}
    internal LegacyOwnedMainWordSourceChoicesResult(LegacyFreshMainWordSourceChoices owner,LegacyOwnedMainWordSourcePayload source,
        LegacyOwnedMainWordSourceAlternative[] alternatives,LegacyOwnedFreshTextSelection[] commands)
    {Owner=owner;Source=source;Alternatives=Array.AsReadOnly(alternatives);PendingSelections=Array.AsReadOnly(commands);}
}
