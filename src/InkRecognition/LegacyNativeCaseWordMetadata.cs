using System.Buffers.Binary;
namespace InkRecognition;

/// <summary>Wrapper292 virtual70=234970 over the owned dictionary terminal;
/// this changes the language category at8, not word identity or score.</summary>
public sealed class LegacyNativeCaseWordMetadata
{
    public LegacyPreparedDictionaryWordMetadata Inner {get;}
    public LegacyNativeCaseRecordExpansion Expansion {get;}
    public LegacyNativeCaseWordMetadata(LegacyPreparedDictionaryWordMetadata inner,LegacyNativeCaseRecordExpansion expansion)
    {ArgumentNullException.ThrowIfNull(inner);ArgumentNullException.ThrowIfNull(expansion);Inner=inner;Expansion=expansion;}
    public ReadOnlyMemory<byte> ReadTerminal(ulong sourceTag,ushort endingSymbol,byte boundaryArgument,
        bool ignoreCaseC4,CancellationToken cancellationToken=default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        byte[] bytes=Inner.ReadTerminal(sourceTag,boundaryArgument,cancellationToken).ToArray();
        if(Expansion.Policy.Version==0)return bytes;
        int state=(byte)(sourceTag>>56)>>2&31;
        byte kind=Expansion.Preparation.ReadClassification(endingSymbol);
        int category=ignoreCaseC4?0:(sourceTag&(1UL<<40))!=0&&kind==8?7:kind==9?8:Expansion.Policy.ReadWordCategory(state);
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(8),category);
        return bytes;
    }
}
