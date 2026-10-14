using System.Buffers.Binary;
using System.Security.Cryptography;

namespace InkRecognition;

/// <summary>Scoped first23f literal-symbol context policy. The pinned owned
/// alphabet has no Arabic-derived array88: Engine573B0 therefore leaves the
/// constructor's NULL88 and Text1EDDE0 selects the exact zero-trait resolver.
/// Skipped ancestors, blank5, shadda and reused/external configuration remain
/// unsupported. This is not an automatic main240 language policy.</summary>
public sealed class LegacyFreshFirstLiteralContextPolicy
{
    private readonly ushort[] skipped;
    public LegacyFreshFirstTextContextPolicy Binding {get;}
    public ushort ClosedSymbolE4 {get;}
    public bool HasArabicTraits=>false;
    private LegacyFreshFirstLiteralContextPolicy(LegacyFreshFirstTextContextPolicy binding,ushort closed,ushort[] skipped)
    {Binding=binding;ClosedSymbolE4=closed;this.skipped=skipped;}

    public static LegacyFreshFirstLiteralContextPolicy ReadSource(ReadOnlySpan<byte> resource,int sourceOffset,
        LegacyFreshFirstResultOwnershipFactory factory,CancellationToken cancellationToken=default)
    {
        ArgumentNullException.ThrowIfNull(factory);cancellationToken.ThrowIfCancellationRequested();
        var model=LegacyFirstAcousticModel.ReadSource(resource,sourceOffset,cancellationToken);
        if(sourceOffset!=factory.InvocationPolicy.Binding.SourceOffset||model.SourceFingerprint!=factory.SourceFingerprint)
            throw new ArgumentException("Matching owned fresh selected-child source required.");
        int p=checked(sourceOffset+20+4+56);int marker=BinaryPrimitives.ReadInt32LittleEndian(resource[p..]);
        p+=4;if(marker==-1)p+=4;p+=11;
        var alphabet=factory.InvocationPolicy.Binding.Alphabet;
        string hash=Convert.ToHexString(SHA256.HashData(resource.Slice(p,alphabet.BytesConsumed)));
        //This is a validated profile restriction, not Unicode-script guessing.
        //The independent binary audit covers every code point in these bytes.
        if(hash!="2997884BBAAA094929249D1AF7E88F285F16EE0E5345AB9013455B66A18EB6A8")
            throw new NotSupportedException("Zero-Arabic-trait proof is restricted to the audited first alphabet103.");
        var binding=factory.InvocationPolicy.Binding;
        if(binding.NativeModeE0||!alphabet.TryGetLabel("\U000f0001",out ushort closed))
            throw new NotSupportedException("Only proved fresh ordinary E0=0 literal context scope supported.");
        ushort[] excluded=[checked((ushort)alphabet.LabelCount),binding.TrailingSymbolE6,
            BinaryPrimitives.ReadUInt16LittleEndian(resource[(p+8)..]),
            BinaryPrimitives.ReadUInt16LittleEndian(resource[(p+10)..]),
            BinaryPrimitives.ReadUInt16LittleEndian(resource[(p+12)..])];
        cancellationToken.ThrowIfCancellationRequested();return new(binding,closed,excluded);
    }

    public bool SupportsSymbol(ushort symbol)=>symbol<Binding.Alphabet.LabelCount&&!skipped.Contains(symbol);
    internal void Validate(ushort symbol,byte traits)
    {
        if(traits!=0||!SupportsSymbol(symbol))
            throw new NotSupportedException("Literal first context requires exact zero traits and a non-skipped ordinary alphabet label; blank/ancestor/shadda branches are not restored.");
    }
}
