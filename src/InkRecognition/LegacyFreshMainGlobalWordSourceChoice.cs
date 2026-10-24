using System.Buffers.Binary;
namespace InkRecognition;

/// <summary>One actual275 WORD's native-order CHAR choice links under a shared WORDsource.
/// This is scoped output, never a TOP/query issuer or caller-selected replacement.</summary>
public sealed class LegacyFreshMainGlobalWordSourceChoice
{
    private readonly LegacyFreshMainGlobalWordSourcePayload payload;
    private readonly List<LegacyFreshMainGlobalWordCharacterLink> links=[];
    private LegacyNativeInkPositionRange[] ordinary=[];
    public int WordOrdinal{get;}
    public uint Flags{get;private set;}
    public bool IsComplete{get;private set;}
    internal int LinkCount=>links.Count;
    internal LegacyFreshMainGlobalWordSourceChoice(LegacyFreshMainGlobalWordSourcePayload payload,int ordinal)
    {this.payload=payload;WordOrdinal=ordinal;}
    internal void Append(LegacyFreshMainGlobalTextSourceChoice choice,CancellationToken token)
    {
        if(IsComplete)throw new InvalidOperationException("A completed275 choice cannot be appended.");
        if(choice.Payload.CopyAssignedScalars().Field40!=-1)throw new NotSupportedException("All275 CHAR source selection override requires separate native handling.");
        var parent=payload.OriginalSource.Owner;
        if(choice.ProcessedSource.IsOrdinarySource)ordinary=parent.RangeOperations.Union(ordinary,choice.Payload.Source.CopyRanges(token),token);
        links.Add(new(choice));var rows=choice.Payload.CopyRecords(token);
        if((uint)choice.Marker>=rows.Count)throw new NotSupportedException("Actual per-WORD CHAR marker unavailable.");Flags=FoldFlags(Flags,rows[choice.Marker].Flags,rows[choice.Marker].Flags);
    }
    //0257F9 default OR precedes02580C..02583F. BTR acts on a copy while EDX
    //retains priorAfterDefaultOR, so the tail cannot clear an existing bit20.
    //Fresh Field40=-1 and valid marker select that SAME marker as default.
    internal static uint FoldFlags(uint prior,uint defaultFlags,uint markerFlags)
    {
        uint afterDefault=prior|defaultFlags;
        return (afterDefault&~0x100000u)|((afterDefault|markerFlags)&0x100000u);
    }
    internal void Seal(){if(IsComplete)throw new InvalidOperationException("275 choice cannot complete twice.");IsComplete=true;}
    public IReadOnlyList<LegacyFreshMainGlobalWordCharacterLink> CopyCharacterLinks()
    {payload.OriginalSource.Owner.Scope.Validate();return Array.AsReadOnly(links.ToArray());}
    public LegacyNativeInkPositionRange[] CopyOrdinaryRanges()
    {payload.OriginalSource.Owner.Scope.Validate();return ordinary.ToArray();}
    internal static byte[] EncodeScalar48(uint flags,int count)
    {
        if(count is<0 or>4096)throw new ArgumentOutOfRangeException(nameof(count));
        byte[] b=new byte[48];BinaryPrimitives.WriteUInt32LittleEndian(b.AsSpan(8),flags);
        BinaryPrimitives.WriteInt32LittleEndian(b.AsSpan(24),count);
        //024F30 writes capacity=count+1 exactly at each successful append.
        BinaryPrimitives.WriteInt32LittleEndian(b.AsSpan(28),count);return b;
    }
    public byte[] CopyScalar48()
    {
        payload.OriginalSource.Owner.Scope.Validate();return EncodeScalar48(Flags,links.Count);
    }
}
