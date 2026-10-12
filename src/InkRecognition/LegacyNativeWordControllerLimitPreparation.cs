using System.Buffers.Binary;

namespace InkRecognition;

/// <summary>Prepared2b3 knowledge short fields used by2144f0. Later setters
/// can change them; the constructor values are not assumed for arbitrary input.</summary>
public readonly record struct LegacyNativeWordKnowledgeLimits(short Header14, short Primary16, short Secondary18)
{
    public static LegacyNativeWordKnowledgeLimits CreateFresh00c860() => new(-1, -1, -1);
    public static LegacyNativeWordKnowledgeLimits CreateChild156630(bool hasFirstSource)
        => new(hasFirstSource ? (short)1 : (short)20, 1, 1);
    public static LegacyNativeWordKnowledgeLimits ReadPreparedSnapshot(ReadOnlySpan<byte> knowledge)
    {
        if (knowledge.Length < 0x1a) throw new ArgumentException("Prepared knowledge must cover shorts14/16/18.");
        return new(BinaryPrimitives.ReadInt16LittleEndian(knowledge[0x14..]),
            BinaryPrimitives.ReadInt16LittleEndian(knowledge[0x16..]),
            BinaryPrimitives.ReadInt16LittleEndian(knowledge[0x18..]));
    }
}

public readonly record struct LegacyNativeWordPreparedLimits(LegacyNativeWordControllerConfiguration Configuration,
    bool RequiresAlternativePoolReset);

/// <summary>2144f0's scalar first-iteration preparation. The actual frontendC8
/// value and knowledge snapshot are supplied; native pool freeing is a separate
/// required action when RequiresAlternativePoolReset is true.</summary>
public static class LegacyNativeWordControllerLimitPreparation
{
    public static LegacyNativeWordPreparedLimits Prepare(LegacyNativeWordControllerConfiguration configuration,
        uint frontendArcLimitC8, LegacyNativeWordKnowledgeLimits knowledge, bool initialIteration)
    {
        var prepared = configuration with { PrimaryArcLimit230a = unchecked((ushort)frontendArcLimitC8) };
        if (!initialIteration) return new(prepared, false);
        ushort header = knowledge.Header14 == -1 ? (ushort)2 : unchecked((ushort)knowledge.Header14);
        ushort primary = knowledge.Primary16 == -1 ? (ushort)2 : unchecked((ushort)knowledge.Primary16);
        ushort secondary = knowledge.Secondary18 == -1
            ? configuration.Mode22f6 == 0 && (configuration.PackedSource22e8 & 7) > 1 ? (ushort)1 : (ushort)2
            : unchecked((ushort)knowledge.Secondary18);
        bool reset = primary != configuration.PrimaryAlternativeCount2402 || secondary != configuration.SecondaryAlternativeCount2404;
        prepared = prepared with { HeaderCount2400 = header, PrimaryAlternativeCount2402 = primary, SecondaryAlternativeCount2404 = secondary };
        return new(prepared, reset);
    }
}
