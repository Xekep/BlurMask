using System.Buffers.Binary;

namespace InkRecognition;

/// <summary>21FBF0 over actual WORD276 reference chains and their shared274 pool.
/// Does not choose words, construct glyphs, or infer missing scalar scratch.</summary>
public sealed class LegacyNativeWordQuoteNormalization
{
    public LegacyInkAlphabet Alphabet {get;}
    private readonly ushort headerE, syntheticApostrophe, syntheticQuote, apostrophe, quote;
    private LegacyNativeWordQuoteNormalization(LegacyInkAlphabet alphabet,ushort a,ushort q)
    {
        Alphabet=alphabet;apostrophe=a;quote=q;
        headerE=BinaryPrimitives.ReadUInt16LittleEndian(alphabet.CopySerializedHeader().AsSpan(6));
        syntheticApostrophe=checked((ushort)(alphabet.LabelCount+1));
        syntheticQuote=checked((ushort)(alphabet.LabelCount+2));
    }
    public static LegacyNativeWordQuoteNormalization Prepare(LegacyInkAlphabet actualAlphabet,
        CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(actualAlphabet);token.ThrowIfCancellationRequested();
        if(actualAlphabet.LabelCount>ushort.MaxValue-2||!actualAlphabet.TryGetLabel("'",out ushort a)||
            !actualAlphabet.TryGetLabel("\"",out ushort q))
            throw new NotSupportedException("Actual bounded synthetic and ordinary quote labels required.");
        return new(actualAlphabet,a,q);
    }

    /// <summary>Scratch values are the two separate caller stack slots carried
    /// across all words. Glyph writes preserve every field except ushort08.</summary>
    public void Normalize(LegacyNativeGlyphRecordPool actualPool,IReadOnlyList<LegacyNativeWordBuilder> actualWords,
        Span<byte> leadingCallerStorage4,Span<byte> trailingCallerStorage4,
        Func<string,ushort>? actualControlLabelLookup=null,CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(actualPool);ArgumentNullException.ThrowIfNull(actualWords);
        token.ThrowIfCancellationRequested();
        if(leadingCallerStorage4.Length!=4||trailingCallerStorage4.Length!=4||
            leadingCallerStorage4.Overlaps(trailingCallerStorage4)||actualWords.Count>4096)
            throw new ArgumentException("Two explicit four-byte scratch slots and bounded actual word list required.");
        foreach(var word in actualWords)
        {
            token.ThrowIfCancellationRequested();ArgumentNullException.ThrowIfNull(word);
            if(!ReferenceEquals(word.Pool,actualPool))throw new ArgumentException("Words must share the actual TOP273 glyph pool.");
            NormalizeChain(actualPool,word.CopyChain(token),leadingCallerStorage4,trailingCallerStorage4,actualControlLabelLookup,token);
        }
        token.ThrowIfCancellationRequested();
    }

    /// <summary>Same21FBF0 operation on the actual published275 WORD list.
    /// Its native head/last-reference access does not require a new276 builder.
    /// The caller supplies the native279.Next order and the two retained slots.</summary>
    public void NormalizePublished(LegacyNativeGlyphRecordPool actualPool,
        IReadOnlyList<LegacyNativePlainWordRecord> actualWords,Span<byte> leadingCallerStorage4,
        Span<byte> trailingCallerStorage4,Func<string,ushort>? actualControlLabelLookup=null,
        CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(actualPool);ArgumentNullException.ThrowIfNull(actualWords);
        token.ThrowIfCancellationRequested();
        if(leadingCallerStorage4.Length!=4||trailingCallerStorage4.Length!=4||
            leadingCallerStorage4.Overlaps(trailingCallerStorage4)||actualWords.Count>4096)
            throw new ArgumentException("Two explicit four-byte scratch slots and bounded actual word list required.");
        foreach(var word in actualWords)
        {
            token.ThrowIfCancellationRequested();ArgumentNullException.ThrowIfNull(word);
            if(!ReferenceEquals(word.Pool,actualPool))throw new ArgumentException("Published words must share the actual TOP273 glyph pool.");
            var chain=LegacyNativeWordBuilder.CopyChain(actualPool,word.HeadReference,token);
            if(chain.Length!=word.FlatReferences.Count||!chain.Reverse().SequenceEqual(word.FlatReferences))
                throw new ArgumentException("Actual published head and reading-order references required.");
            NormalizeChain(actualPool,chain,leadingCallerStorage4,trailingCallerStorage4,actualControlLabelLookup,token);
        }
        token.ThrowIfCancellationRequested();
    }

    private void NormalizeChain(LegacyNativeGlyphRecordPool actualPool,ulong[] chain,
        Span<byte> leadingCallerStorage4,Span<byte> trailingCallerStorage4,
        Func<string,ushort>? actualControlLabelLookup,CancellationToken token)
    {
        if(chain.Length==0)throw new NotSupportedException("Native empty-word glyph dereference has no owned record.");
        foreach(ulong reference in chain)
        {
            byte[] row=actualPool.CopyRecord(reference,token);
            Read(row,leadingCallerStorage4,actualControlLabelLookup,token);
            ushort label=BinaryPrimitives.ReadUInt16LittleEndian(leadingCallerStorage4);
            if(leadingCallerStorage4[2]==0&&label==headerE)continue;
            if(leadingCallerStorage4[2]==0)
            {
                if(label==syntheticApostrophe)Write(reference,row,apostrophe);
                else if(label==syntheticQuote)Write(reference,row,quote);
            }
            break;
        }
        byte[] last=actualPool.CopyRecord(chain[^1],token);
        Read(last,trailingCallerStorage4,actualControlLabelLookup,token);
        if(trailingCallerStorage4[2]==0&&BinaryPrimitives.ReadUInt16LittleEndian(trailingCallerStorage4)==syntheticApostrophe)
            Write(chain[^1],last,apostrophe);
        void Write(ulong reference,byte[] row,ushort label)
        {
            BinaryPrimitives.WriteUInt16LittleEndian(row.AsSpan(8),label);
            actualPool.ReplaceRecord(reference,row,token);
        }
    }
    private static void Read(byte[] row,Span<byte> scratch,Func<string,ushort>? lookup,CancellationToken token)=>
        LegacyNativeGlyphUtf8Tokens.ReadScalar(new(BinaryPrimitives.ReadUInt16LittleEndian(row.AsSpan(4)),
            BinaryPrimitives.ReadUInt16LittleEndian(row.AsSpan(8))),scratch,lookup,token);
}
