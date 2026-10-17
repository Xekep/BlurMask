using System.Buffers.Binary;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("LegacyMainTextChoices.Probe")]

namespace InkRecognition;

internal readonly record struct LegacyPreparedPublishedGlyphRead(ushort Label,ulong Reference,
    bool HasReference,int WordOrdinal,int ReferenceOrdinal);

/// <summary>Bounded21F210 traversal over the actual WORD275 flat vectors in
/// caller-supplied native279 list order. Scalar scratch remains explicit;
/// it is neither cleared nor substituted with the preceding returned label.</summary>
internal sealed class LegacyPreparedPublishedGlyphCursor
{
    private readonly LegacyNativeGlyphRecordPool pool;
    private readonly LegacyNativePlainWordRecord[] words;
    private int word,ordinal;
    private bool failed;
    internal LegacyPreparedPublishedGlyphCursor(LegacyNativeGlyphRecordPool actualPool,
        IReadOnlyList<LegacyNativePlainWordRecord> nativeWordOrder,CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(actualPool);ArgumentNullException.ThrowIfNull(nativeWordOrder);
        token.ThrowIfCancellationRequested();
        if(nativeWordOrder.Count>4096)throw new ArgumentException("Bounded native WORD list required.");
        pool=actualPool;words=nativeWordOrder.ToArray();int references=0;
        foreach(var item in words)
        {
            token.ThrowIfCancellationRequested();ArgumentNullException.ThrowIfNull(item);
            if(!ReferenceEquals(item.Pool,pool))throw new ArgumentException("WORDs must use the identical TOP glyph pool.");
            if(item.FlatReferences.Count>65536-references)throw new ArgumentException("Portable reference traversal budget exceeded.");
            references+=item.FlatReferences.Count;
        }
    }

    internal LegacyPreparedPublishedGlyphRead Next(Span<byte> actualCallerScalarStorage4,
        Func<string,ushort>? actualControlLabelLookup=null,CancellationToken token=default)
    {
        token.ThrowIfCancellationRequested();
        if(actualCallerScalarStorage4.Length!=4)throw new ArgumentException("Actual four-byte scalar scratch required.");
        if(failed)throw new InvalidOperationException("Failed scalar read retains the advanced cursor; retry is unsupported.");
        //Native recursion advances across exhausted and empty WORD records.
        //Bounded iteration preserves that order without a call-stack budget.
        while(word<words.Length&&ordinal==words[word].FlatReferences.Count)
        {token.ThrowIfCancellationRequested();word++;ordinal=0;}
        if(word==words.Length)return new(ushort.MaxValue,0,false,word,0);
        int consumed=ordinal;ulong reference=words[word].FlatReferences[ordinal++];
        try
        {
            //21F210 increments the vector cursor before either virtual read.
            byte[] row=pool.CopyRecord(reference,token);
            LegacyNativeGlyphUtf8Tokens.ReadScalar(new(BinaryPrimitives.ReadUInt16LittleEndian(row.AsSpan(4)),
                BinaryPrimitives.ReadUInt16LittleEndian(row.AsSpan(8))),actualCallerScalarStorage4,actualControlLabelLookup,token);
            return new(BinaryPrimitives.ReadUInt16LittleEndian(actualCallerScalarStorage4),reference,true,word,consumed);
        }
        catch{failed=true;throw;}
    }
}
