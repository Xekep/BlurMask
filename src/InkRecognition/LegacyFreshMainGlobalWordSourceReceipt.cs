using System.Buffers.Binary;
namespace InkRecognition;

/// <summary>Actual completed global WORD source key; not an ordinary CHAR
/// union or the NULL-projected TEXT key. Only the typed parent creates it.</summary>
public sealed class LegacyFreshMainGlobalWordSourceReceipt
{
    public LegacyFreshMainGlobalWordTextPublication Owner{get;}
    private readonly LegacyNativeInkPositionRange[] ranges;
    internal LegacyFreshMainGlobalWordSourceReceipt(LegacyFreshMainGlobalWordTextPublication owner)
    {Owner=owner;ranges=owner.Wrapper.Completion.CopyRanges();}
    public LegacyNativeInkPositionRange[] CopyOriginalKey(){Owner.Scope.Validate();return ranges.ToArray();}
}

public sealed class LegacyFreshMainGlobalWordCharacterLink
{
    public LegacyFreshMainGlobalTextSourceChoice Choice{get;}
    public LegacyFreshMainGlobalTextSourcePayload Source=>Choice.Payload;
    public int Marker=>Choice.Marker;
    internal LegacyFreshMainGlobalWordCharacterLink(LegacyFreshMainGlobalTextSourceChoice actual){Choice=actual;}
    public byte[] CopyScalar24(){Source.Owner.Scope.Validate();byte[] b=new byte[24];BinaryPrimitives.WriteInt32LittleEndian(b.AsSpan(16),Marker);return b;}
}

/// <summary>Native parent WORD payload; pointer/vector lanes are typed
/// sidecars. Explicit assigned defaults survive failed publication.</summary>
public sealed class LegacyFreshMainGlobalWordSourcePayload
{
    public LegacyFreshMainGlobalWordSourceReceipt OriginalSource{get;}
    public LegacyOwnedMainTextSourceNode Node{get;}
    private readonly List<LegacyFreshMainGlobalWordCharacterLink> links=[];
    private LegacyNativeInkPositionRange[] ordinary=[];
    internal int LinkCount=>links.Count;
    public long SelectionId{get;internal set;}=long.MaxValue;
    public LegacyOwnedFreshTextSelection? Selection{get;internal set;}
    public uint Flags{get;internal set;}
    public uint ChoiceFlags{get;internal set;}
    public int ChoiceCount{get;internal set;}
    public int Field08=>-1;
    public ulong Field00=>0;
    public long Field40=>-1;
    public int Field48=>-1;
    public IReadOnlyList<LegacyFreshMainGlobalWordCharacterLink> CopyCharacterLinks(){OriginalSource.Owner.Scope.Validate();return Array.AsReadOnly(links.ToArray());}
    public LegacyNativeInkPositionRange[] CopyOrdinaryRanges(){OriginalSource.Owner.Scope.Validate();return ordinary.ToArray();}
    internal LegacyFreshMainGlobalWordSourcePayload(LegacyFreshMainGlobalWordSourceReceipt original,LegacyOwnedMainTextSourceNode node)
    {OriginalSource=original;Node=node;}
    internal void Append(LegacyFreshMainGlobalTextSourceChoice actual,CancellationToken token)
    {
        var parent=OriginalSource.Owner;var source=actual.ProcessedSource;
        source.Owner.RequireCurrentSource(source,token);
        if(!ReferenceEquals(source.Owner,parent.Wrapper)||!actual.Payload.IsComplete||
            !ReferenceEquals(actual.Payload.Node.FirstGlobalSource,actual.Payload.Source)||
            !ReferenceEquals(actual.Payload.Owner,parent.Wrapper))throw new ArgumentException("Actual complete same-wrapper CHAR source required.");
        if(source.IsOrdinarySource)
            ordinary=parent.RangeOperations.Union(ordinary,actual.Payload.Source.CopyRanges(token),token);
        links.Add(new(actual));var rows=actual.Payload.CopyRecords(token);
        if((uint)actual.Marker>=(uint)rows.Count)throw new NotSupportedException("Actual CHAR marker unavailable; no fallback row.");
        ChoiceFlags|=rows[actual.Marker].Flags;
    }
    public byte[] CopyChoiceScalar48()
    {OriginalSource.Owner.Scope.Validate();byte[] b=new byte[48];BinaryPrimitives.WriteUInt32LittleEndian(b.AsSpan(8),ChoiceFlags);BinaryPrimitives.WriteInt32LittleEndian(b.AsSpan(24),links.Count);BinaryPrimitives.WriteInt32LittleEndian(b.AsSpan(28),links.Count);return b;}
}

public sealed class LegacyFreshMainGlobalTextSelectionReceipt
{
    public LegacyFreshMainGlobalWordTextPublication Owner{get;}
    private readonly LegacyNativeInkPositionRange[] ranges;
    public LegacyOwnedFreshTextSelection? Selection{get;internal set;}
    public long? SelectionId=>Selection?.SelectionId;
    internal LegacyFreshMainGlobalTextSelectionReceipt(LegacyFreshMainGlobalWordTextPublication owner,LegacyNativeInkPositionRange[] actual)
    {Owner=owner;ranges=actual.ToArray();}
    public LegacyNativeInkPositionRange[] CopyRanges(){Owner.Scope.Validate();return ranges.ToArray();}
}
