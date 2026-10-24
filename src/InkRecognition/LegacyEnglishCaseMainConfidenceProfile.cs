using System.Buffers.Binary;
namespace InkRecognition;

// Pure authenticated MAIN21D profile. This object alone issues no group/WORD authority.
internal sealed class LegacyEnglishCaseMainConfidenceProfile
{
    internal LegacySource247WireLayout Source{get;}
    internal ushort Closed{get;}
    private readonly LegacyAcousticConfidenceLabelMap labels;
    private LegacyEnglishCaseMainConfidenceProfile(LegacySource247WireLayout source,LegacyAcousticConfidenceLabelMap labels,ushort closed)
    {Source=source;this.labels=labels;Closed=closed;}
    internal const long PreparationEnvelope=1024*1024;
    internal static LegacyEnglishCaseMainConfidenceProfile Read(LegacySource247WireLayout source,CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(source);token.ThrowIfCancellationRequested();
        if(source.Offset!=0||source.End!=545871||source.SourceKind!=0x135||source.Version!=0x110||source.Alphabet.LabelCount!=217||
            !source.Fingerprint.Equals("ca46297ada4a80de0d4bbd40650a8ace5c3809af9edbd493fb8f4eb692a18657",StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException("Actual immutable English MAIN source required.");
        var child=source.Children.Single(c=>c.Destination=="247.140");
        if(child.Offset!=184155||child.End!=545680||child.Kind!=0x21d||child.Version!=0x119||child.End-child.Offset+16384>PreparationEnvelope)
            throw new InvalidDataException("Exact bounded MAIN119 parameter child required before snapshot.");
        var raw=source.CopyChildEnvelopeBytes(child,token);
        ReadOnlySpan<byte> At(int offset,int count)=>raw.AsSpan(offset-child.Offset,count);
        if(BinaryPrimitives.ReadInt32LittleEndian(At(545663,4))!=0||BinaryPrimitives.ReadInt32LittleEndian(At(545667,4))!=0)
            throw new NotSupportedException("Actual default confidence calibration/correction only.");
        byte[] header=source.Alphabet.CopySerializedHeader();
        ushort[] expected=[181,178,65535,180,184,182,183,217];
        for(int i=0;i<expected.Length;i++)if(BinaryPrimitives.ReadUInt16LittleEndian(header.AsSpan(i*2))!=expected[i])
            throw new InvalidDataException("Actual MAIN confidence header required.");
        if(BinaryPrimitives.ReadUInt16LittleEndian(At(545542,2))!=7)throw new InvalidDataException("Actual seven MAIN groups required.");
        var groups=new LegacySymbolMaskKnowledgeGroup[7];int before=0;
        for(int i=0;i<7;i++)
        {
            token.ThrowIfCancellationRequested();int first=BinaryPrimitives.ReadUInt16LittleEndian(At(545544+i*2,2));
            int last=BinaryPrimitives.ReadUInt16LittleEndian(At(545546+i*2,2));
            if(first!=before||last<first||last>25)throw new InvalidDataException("Bounded original group order required.");
            var symbols=new ushort[last-first];for(int j=0;j<symbols.Length;j++)symbols[j]=BinaryPrimitives.ReadUInt16LittleEndian(At(545560+(first+j)*2,2));
            float weight=BinaryPrimitives.ReadSingleLittleEndian(At(545610+i*4,4));groups[i]=new(symbols,weight);before=last;
        }
        if(before!=25||!source.Alphabet.TryGetLabel("\U000f0001",out ushort closed))throw new InvalidDataException("Whole MAIN groups and closed primitive required.");
        var map=new LegacyAcousticConfidenceLabelMap(source.Alphabet,header,182,groups,token);
        if(map.TryResolveOutputLabel(closed,out _))throw new NotSupportedException("This actual closed primitive must consume source90 probability.");
        token.ThrowIfCancellationRequested();return new(source,map,closed);
    }
    internal bool TryResolve(ushort symbol,out ushort output)=>labels.TryResolveOutputLabel(symbol,out output);
}
