namespace InkRecognition;

/// <summary>The scalar stores after a successful20D8F0 in20C8C0.</summary>
public readonly record struct LegacyNativeWordScalarRefreshResult(uint PackedSource22e8,
    byte ConsecutiveBlank22f7, byte KnowledgeOption22f8, byte SourceFormat22f9,
    int MaximumWordLength68, byte KnowledgeMarkerA0);

/// <summary>Literal20C8C0 scalar stage. Source refresh success and the selected
/// source's V60/V68 values remain explicit; this does not execute decorators.</summary>
public static class LegacyNativeWordScalarRefresh
{
    public static LegacyNativeWordScalarRefreshResult Apply(byte mode22f6, uint packedSource22e8,
        bool hasAssociatedController2300, byte selectedSourceV60, byte knowledgeA1,
        byte selectedSourceV68, int requestedWordLength3c, byte previousKnowledgeA0,
        bool hasDirectionalEmbedding24fa)
    {
        uint flags = hasAssociatedController2300 ? packedSource22e8 : packedSource22e8 & ~8u;
        int length = mode22f6 == 0 && (flags & 7) > 1
            ? requestedWordLength3c == -1 ? 120 : requestedWordLength3c : 0;
        return new(flags, selectedSourceV60, knowledgeA1, selectedSourceV68, length,
            hasDirectionalEmbedding24fa ? (byte)1 : previousKnowledgeA0);
    }
}
