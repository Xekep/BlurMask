using System.Buffers.Binary;
namespace InkRecognition;

/// <summary>Actual completed global WORD source key; not an ordinary CHAR
/// union or the NULL-projected TEXT key. Only the typed parent creates it.</summary>
public sealed class LegacyFreshMainGlobalWordSourceReceipt
{
    public LegacyFreshMainGlobalWordTextPublication Owner{get;}
    private readonly LegacyNativeInkPositionRange[] ranges;
    internal LegacyFreshMainGlobalWordSourceReceipt(LegacyFreshMainGlobalWordTextPublication owner)
    {Owner=owner;ranges=owner.Wrapper.ActualCompletion.CopyRanges();}
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
    private LegacyFreshMainGlobalWordSourceChoice[]? allChoices;
    private int currentChoice=-1,selectedChoice;
    private uint allChoiceFlags;
    internal int LinkCount=>allChoices is null?links.Count:currentChoice<0?0:CurrentChoice.LinkCount;
    private LegacyFreshMainGlobalWordSourceChoice CurrentChoice=>allChoices![currentChoice];
    internal uint CurrentChoiceFlags=>allChoices is null?ChoiceFlags:CurrentChoice.Flags;
    internal uint AllChoiceFlags=>allChoices is null?ChoiceFlags:allChoiceFlags;
    public long SelectionId{get;internal set;}=long.MaxValue;
    public LegacyOwnedFreshTextSelection? Selection{get;internal set;}
    public uint Flags{get;internal set;}
    public uint ChoiceFlags{get;internal set;}
    public int ChoiceCount{get;internal set;}
    public int Field08=>-1;
    public ulong Field00=>0;
    public long Field40=>-1;
    public int Field48=>-1;
    public IReadOnlyList<LegacyFreshMainGlobalWordCharacterLink> CopyCharacterLinks(){OriginalSource.Owner.Scope.Validate();return allChoices is null?Array.AsReadOnly(links.ToArray()):allChoices[selectedChoice].CopyCharacterLinks();}
    public LegacyNativeInkPositionRange[] CopyOrdinaryRanges(){OriginalSource.Owner.Scope.Validate();return allChoices is null?ordinary.ToArray():allChoices[selectedChoice].CopyOrdinaryRanges();}
    internal LegacyNativeInkPositionRange[] CopyCurrentOrdinaryRanges()=>allChoices is null?ordinary.ToArray():CurrentChoice.CopyOrdinaryRanges();
    public IReadOnlyList<LegacyFreshMainGlobalWordSourceChoice> CopyWordChoices()
    {OriginalSource.Owner.Scope.Validate();return allChoices is null?throw new NotSupportedException("All275 choice publication was not requested."):Array.AsReadOnly(allChoices.ToArray());}
    internal void InitializeChoices(int count,int selected)
    {
        if(allChoices is not null||links.Count!=0||count is<1 or>4095||(uint)selected>=count)throw new InvalidOperationException("Fresh bounded all-WORD payload required.");
        allChoices=new LegacyFreshMainGlobalWordSourceChoice[count];selectedChoice=selected;ChoiceCount=count;
        for(int i=0;i<count;i++)allChoices[i]=new(this,i);
    }
    internal void AppendChoiceSlots(int resultCount)
    {
        if(allChoices is null||resultCount<allChoices.Length||resultCount>4095||
            currentChoice!=allChoices.Length-1||allChoices.Any(c=>!c.IsComplete))
            throw new InvalidOperationException("Completed same-parent WORD payload required before native reuse append.");
        if(resultCount==allChoices.Length)return;
        var next=new LegacyFreshMainGlobalWordSourceChoice[resultCount];allChoices.CopyTo(next,0);
        for(int i=allChoices.Length;i<resultCount;i++)next[i]=new(this,i);
        allChoices=next;ChoiceCount=resultCount;
    }
    internal LegacyFreshMainGlobalWordSourceChoice ChoiceAt(int index)
    {
        OriginalSource.Owner.Scope.Validate();
        if(allChoices is null||(uint)index>=allChoices.Length||!allChoices[index].IsComplete)
            throw new InvalidOperationException("Completed shared WORD choice required.");
        return allChoices[index];
    }
    internal void BeginChoice(int ordinal)
    {if(allChoices is null||ordinal!=currentChoice+1||(uint)ordinal>=allChoices.Length)throw new InvalidOperationException("Literal next275 WORD choice required.");currentChoice=ordinal;}
    internal void CompleteChoice(bool aggregateParentFlags=true){if(allChoices is null)throw new InvalidOperationException();CurrentChoice.Seal();allChoiceFlags|=CurrentChoice.Flags;if(aggregateParentFlags)Flags|=CurrentChoice.Flags;}
    internal LegacyFreshMainGlobalWordSourcePayload(LegacyFreshMainGlobalWordSourceReceipt original,LegacyOwnedMainTextSourceNode node)
    {OriginalSource=original;Node=node;}
    internal void Append(LegacyFreshMainGlobalTextSourceChoice actual,CancellationToken token)
    {
        var parent=OriginalSource.Owner;var source=actual.ProcessedSource;
        source.Owner.RequireCurrentSource(source,token);
        if(!ReferenceEquals(source.Owner,parent.Wrapper)||!actual.Payload.IsComplete||
            !ReferenceEquals(actual.Payload.Node.FirstGlobalSource,actual.Payload.Source)||
            !ReferenceEquals(actual.Payload.Owner,parent.Wrapper))throw new ArgumentException("Actual complete same-wrapper CHAR source required.");
        if(allChoices is not null)
        {
            CurrentChoice.Append(actual,token);
            if(currentChoice==selectedChoice)ChoiceFlags=CurrentChoice.Flags;
            return;
        }
        if(source.IsOrdinarySource)
            ordinary=parent.RangeOperations.Union(ordinary,actual.Payload.Source.CopyRanges(token),token);
        links.Add(new(actual));var rows=actual.Payload.CopyRecords(token);
        if((uint)actual.Marker>=(uint)rows.Count)throw new NotSupportedException("Actual CHAR marker unavailable; no fallback row.");
        ChoiceFlags|=rows[actual.Marker].Flags;
    }
    public byte[] CopyChoiceScalar48()
    {OriginalSource.Owner.Scope.Validate();if(allChoices is not null)return allChoices[selectedChoice].CopyScalar48();byte[] b=new byte[48];BinaryPrimitives.WriteUInt32LittleEndian(b.AsSpan(8),ChoiceFlags);BinaryPrimitives.WriteInt32LittleEndian(b.AsSpan(24),links.Count);BinaryPrimitives.WriteInt32LittleEndian(b.AsSpan(28),links.Count);return b;}
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
