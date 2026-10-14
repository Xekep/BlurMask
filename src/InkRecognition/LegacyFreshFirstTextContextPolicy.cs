using System.Buffers.Binary;

namespace InkRecognition;

/// <summary>Invocation settings after158970 copies the actual source-frame150
/// flag to child configurationA8. That flag is explicit, without a zero default.</summary>
public readonly record struct LegacyFreshFirstInvocationSettings(bool NativeModeE0,bool Configuration99,bool ConfigurationA8);

/// <summary>Fresh NEW child249-selected source/configuration binding156200/
///156630/00cb50. It is not a reused/external2b3 configuration or the parent
///text driver/main240 linguistic controller. No caller options are overwritten.</summary>
public sealed class LegacyFreshFirstTextContextPolicy
{
    public int SourceOffset {get;}
    public int SourceKind {get;}
    public LegacyInkAlphabet Alphabet {get;}
    public bool NativeModeE0 {get;}
    public bool AliasFold=>false;
    public int ShortlistColumnsA0=>1;
    public int ContextColumnsA2=>1;
    public int MergedColumnsA4=>1;
    public bool OwnerFlagC4=>false;
    public bool Configuration99=>false;
    public bool ConfigurationA8AtBinding=>false;
    public ushort TrailingSymbolE6 {get;}
    public LegacyFreshFirstInvocationSettings ForInvocation(bool actualSourceFrameFlag150)
        =>new(NativeModeE0,Configuration99,actualSourceFrameFlag150);
    private LegacyFreshFirstTextContextPolicy(int sourceOffset,int sourceKind,LegacyInkAlphabet alphabet)
    {
        SourceOffset=sourceOffset;SourceKind=sourceKind;Alphabet=alphabet;
        //1edde0 uses ownerC4, sourcevirtual58==137, or exactU+F0009.
        //The NEW selected child's ownerC4 is zero through the complete binder,
        //not just constructor state. It does not test shadda for this flag.
        NativeModeE0=sourceKind==0x137||alphabet.TryGetLabel("\U000f0009",out _);
        TrailingSymbolE6=alphabet.TryGetLabel("\u0651",out ushort label)?label:ushort.MaxValue;
    }
    public static LegacyFreshFirstTextContextPolicy ReadForFreshSelectedChild(ReadOnlySpan<byte> resource,
        int sourceOffset,CancellationToken cancellationToken=default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var model=LegacyFirstAcousticModel.ReadSource(resource,sourceOffset,cancellationToken);
        int p=checked(sourceOffset+20+4+56);
        int marker=BinaryPrimitives.ReadInt32LittleEndian(resource[p..]);p+=4;if(marker==-1)p+=4;p+=4;
        if(resource[p++]!=1||BinaryPrimitives.ReadUInt32LittleEndian(resource[p..])!=0x2b)
            throw new NotSupportedException("Owned inline first acoustic alphabet required.");
        p+=4;ushort version=BinaryPrimitives.ReadUInt16LittleEndian(resource[p..]);p+=2;
        if(version!=0x103)throw new NotSupportedException("Only checked first alphabet103 supported.");
        var alphabet=LegacyInkAlphabet.Load(resource[p..],version);
        if(alphabet.LabelCount<model.TrajectoryNetwork.OutputWidth)throw new InvalidDataException("First model exceeds its inline alphabet ownership.");
        cancellationToken.ThrowIfCancellationRequested();
        return new(sourceOffset,model.RowProfile.Source.SourceKind,alphabet);
    }
}
