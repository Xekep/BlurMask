using System.Buffers.Binary;
namespace InkRecognition;

/// <summary>Exact immutable 15-byte state policies and 8-byte selectors chosen by232090.
/// These are finite-state behavior constants, not trained weights or inferred case defaults.</summary>
public sealed class LegacyNativeCasePathPolicy
{
    private readonly byte[] policies, selectors;
    public ushort Version { get; }
    public int StateCount => policies.Length / 15;
    private LegacyNativeCasePathPolicy(ushort version, string policy, string selector)
    {Version=version;policies=Convert.FromHexString(policy);selectors=Convert.FromHexString(selector);}
    public static LegacyNativeCasePathPolicy Create(ushort version) => version switch
    {
        0 => new(version,"00000001020305060704ff0b0c0dff000001010101ffffffffffffffffff0000020b020308090a04ffffffffff0000030c0303090909ffffffffffff0000040d04ff0aff0a04ffffffffff0000050b050608090a07ffffffffff0000060c06060909ffffffffffffff0000070d07ff0aff0a07ffffffffff000008ff080908090a0affffffffff000009ff09090909ffffffffffffff00000aff0aff0aff0a0affffffffff00000b0b0b0cffffff0dffffffffff00000c0c0c0cffffffffffffffffff00000d0d0dffffffff0dffffffffff","01000000000000000000000001000000000000000100000000000000010000000000000001000000000000000100000000000000010000000000000001000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000"),
        512 => new(version,"00000002030408090a060118191a1b0000010bffffffffffff01ffffffff000002020c0cffffff0c0bffffffff000003150e040f1011060dffffffff0000041605051010ffff0dffffffff0000051605051010ffffffffffffff0000061707ff11ff11070dffffffff0000071707ff11ff1107ffffffffff0008081808091213140affffffffff0009091909091313ffffffffffffff000a0a1a0aff14ff140affffffffff000b0b0bffffffffffff0bffffffff00000c0c0c0cffffff0cffffffffff000d0d0dffffffffffff0dffffffff00000e150e050f101107ffffffffff000f0fff0f100f101111ffffffffff001010ff10101010ffffffffffffff001111ff11ff11ff1111ffffffffff001212ff121312131414ffffffffff001313ff13131313ffffffffffffff001414ff14ff14ff1414ffffffffff000015151516ffffff170dffffffff000016161616ffffffff0dffffffff0000171717ffffffff170dffffffff001818181819ffffff1affffffffff001919191919ffffffffffffffffff001a1a1a1affffffff1affffffffff00ffffffffff08ffffffffffffffff","0100000000000000000000000100000000000000010000000000000001000000000000000100000000000000000000000000000001000000000000000000000000000000010000000000000001000000000000000100000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000"),
        _ => throw new NotSupportedException("Native232090 accepts TCM0/200 only.")
    };
    public byte ReadTransition(int state,int column)
    {ValidateState(state);if((uint)column>=15)throw new ArgumentOutOfRangeException(nameof(column));return policies[state*15+column];}
    public bool UseAlternateForms(int state)
    {ValidateState(state);return BinaryPrimitives.ReadInt32LittleEndian(selectors.AsSpan(state*8))!=0;}
    public int ReadSelectorSecondField(int state)
    {ValidateState(state);return BinaryPrimitives.ReadInt32LittleEndian(selectors.AsSpan(state*8+4));}
    public byte[] CopyPolicies()=>policies.ToArray();
    public byte[] CopySelectors()=>selectors.ToArray();
    public void ValidateState(int state)
    {if((uint)state>=StateCount)throw new ArgumentOutOfRangeException(nameof(state),"State must belong to the selected native TCM table.");}
    private static readonly byte[] categories200=Convert.FromHexString("090b0104040404040505050a010c040606060505050202020303030500000000");
    public int ReadWordCategory(int state)
    {if((uint)state>=32)throw new ArgumentOutOfRangeException(nameof(state));if(Version!=0x200)throw new NotSupportedException("TCM0 leaves inner metadata category unchanged.");return unchecked((sbyte)categories200[state]);}
    /// <summary>233F70 changes only the five case-state bits when source header05.bit2 is set.</summary>
    public static ulong ApplyRestart(ulong innerTag,ulong requestedTag)
    {
        if(((requestedTag>>40)&4)==0)return innerTag;
        const ulong bits=0x7cUL<<56;
        return (innerTag & ~bits) | (requestedTag & bits);
    }
}
