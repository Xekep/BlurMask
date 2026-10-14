using System.Buffers.Binary;

namespace InkRecognition;

/// <summary>Bounded terminal-node branch of dictionary2b7.virtual70=094040.
/// Actual2489e0 assigns terminal preorder IDs; nonterminals are not accepted
/// here because their runtime table entry can name a later terminal.</summary>
public sealed class LegacyPreparedDictionaryWordMetadata
{
    private readonly LegacyPackedLexicon dictionary;
    private readonly LegacyLexiconWordIndex words;
    public int WordCount => words.WordCount;

    /// <summary>Identity of the selected owned dictionary and its index, not
    /// numerical compatibility with another same-sized dictionary.</summary>
    public bool IsForModel(LegacyRussianModelBundle selectedModel)
    {
        ArgumentNullException.ThrowIfNull(selectedModel);
        return ReferenceEquals(dictionary, selectedModel.Lexicon) &&
            ReferenceEquals(words, selectedModel.WordIndex);
    }

    public LegacyPreparedDictionaryWordMetadata(LegacyPackedLexicon dictionary, LegacyLexiconWordIndex words)
    {
        ArgumentNullException.ThrowIfNull(dictionary);
        ArgumentNullException.ThrowIfNull(words);
        if (!words.IsForGraph(dictionary)) throw new ArgumentException("Word-index ownership must match the selected dictionary.");
        this.dictionary = dictionary;
        this.words = words;
    }

    public ReadOnlyMemory<byte> ReadTerminal(ulong childSourceTag, byte boundaryArgument,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        uint node = (uint)childSourceTag;
        if (node >= dictionary.NodeCount || !words.TryGetWordId((int)node, out int wordId))
            throw new NotSupportedException("Actual source70 token must own a terminal of the selected dictionary.");
        byte[] bytes = new byte[16]; //094040 explicitly clears both output qwords.
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(4), wordId);
        bytes[12] = boundaryArgument;
        bytes[13] = (byte)(childSourceTag >> 42 & 1);
        return bytes;
    }
}
