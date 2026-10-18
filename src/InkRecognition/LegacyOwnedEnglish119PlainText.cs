using System.Buffers.Binary;
using System.Collections.ObjectModel;
using System.Security.Cryptography;
namespace InkRecognition;

/// <summary>Explicit immutable native allocation-boundary bytes. Unwritten
/// WORD/glyph fields are preserved, without guessing an allocator's residue.</summary>
public sealed class LegacyEnglish119PlainAllocationStorage
{
    private readonly byte[] word,glyph;
    public string Fingerprint {get;}
    public LegacyEnglish119PlainAllocationStorage(ReadOnlySpan<byte> allocatedEmptyWord120,ReadOnlySpan<byte> allocatedGlyph40)
    {
        if(allocatedEmptyWord120.Length!=120||allocatedGlyph40.Length!=40)
            throw new ArgumentException("Complete WORD120/glyph40 allocation storage required.");
        if(BinaryPrimitives.ReadUInt64LittleEndian(allocatedEmptyWord120[32..])!=0||
            BinaryPrimitives.ReadUInt64LittleEndian(allocatedEmptyWord120[88..])!=0||
            BinaryPrimitives.ReadUInt64LittleEndian(allocatedEmptyWord120[104..])!=0||
            BinaryPrimitives.ReadInt32LittleEndian(allocatedEmptyWord120[96..])!=0||
            BinaryPrimitives.ReadInt32LittleEndian(allocatedEmptyWord120[112..])!=0)
            throw new NotSupportedException("Native empty owned WORD vector constraints required.");
        word=allocatedEmptyWord120.ToArray();glyph=allocatedGlyph40.ToArray();
        Fingerprint=Convert.ToHexString(SHA256.HashData(word.Concat(glyph).ToArray()));
    }
    public byte[] CopyWordStorage()=>word.ToArray();
    public byte[] CopyGlyphStorage()=>glyph.ToArray();
}

/// <summary>Detached native275 WORD view. References are opaque owned pool
/// keys, not raw document/stroke identifiers. No UTF8/accepted text is inferred.</summary>
public sealed class LegacyEnglish119PlainWordSnapshot
{
    private readonly byte[] data;
    private readonly ulong[] references;
    public bool IsDelimiter {get;}
    public int TotalCostDelta {get;}
    public int AcousticCostDelta {get;}
    public int SourceNodeSpan {get;}
    public LegacyNativeWordSourceSpan Span {get;}
    public int WordIndex {get;}
    internal LegacyEnglish119PlainWordSnapshot(LegacyFirstPublishedTextWord actual,CancellationToken token)
    {
        IsDelimiter=actual.IsDelimiter;TotalCostDelta=actual.TotalCostDelta;AcousticCostDelta=actual.AcousticCostDelta;
        SourceNodeSpan=actual.SourceNodeSpan;Span=actual.Published.Span;WordIndex=actual.Published.WordIndex;
        data=actual.Published.Word.CopyStorage();int count=actual.Published.Word.GetReferenceCount(token);references=new ulong[count];
        for(int i=0;i<count;i++){token.ThrowIfCancellationRequested();references[i]=actual.Published.Word.GetReadingOrderReference(i,token);}
    }
    public byte[] CopyStorage()=>data.ToArray();
    public ulong[] CopyReadingOrderReferences()=>references.ToArray();
}

/// <summary>Actual selected119 first rich-pool/WORD components. The mutable
/// pool/publisher are private. This is not full serializedTOP273/TOP18/V50,
/// source720, UTF8 publication or a main linguistic recognition result.</summary>
public sealed class LegacyEnglish119PlainTextResult
{
    internal LegacyOwnedEnglish119PlainResultFactory Factory {get;}
    internal LegacyFirstPlainTextStorage Storage {get;}
    internal IReadOnlyList<LegacyFirstPublishedTextWord> NativeWords {get;}
    public LegacyEnglish119TerminalResult Terminal=>Factory.Terminal;
    public LegacyOrdinaryRawInputDescriptor RawDescriptor=>Factory.Preparation.RawDescriptor;
    public LegacyOrdinaryRawInputDescriptor AssignedDescriptor=>Factory.AssignedDescriptor;
    public string AllocationStorageFingerprint {get;}
    public IReadOnlyList<LegacyEnglish119PlainWordSnapshot> Words {get;}
    public IReadOnlyList<ulong> AllocatedGlyphReferences {get;}
    public IReadOnlyList<ulong> DetachedDelimiterReferences {get;}
    public IReadOnlyDictionary<ulong,float> OwnerFirstScores1C {get;}
    public float MinimumOwnerFirstScore80 {get;}
    internal LegacyEnglish119PlainTextResult(LegacyOwnedEnglish119PlainResultFactory factory,
        LegacyNativeFirstPlainTextKernelResult result,string allocationFingerprint,CancellationToken token)
    {
        Factory=factory;Storage=result.Storage;AllocationStorageFingerprint=allocationFingerprint;
        NativeWords=Array.AsReadOnly(result.Words.ToArray());
        foreach(var word in NativeWords)if(!ReferenceEquals(word.Published.Word.Pool,Storage.GlyphPool))throw new InvalidDataException("Foreign WORD pool.");
        Words=Array.AsReadOnly(result.Words.Select(w=>new LegacyEnglish119PlainWordSnapshot(w,token)).ToArray());
        AllocatedGlyphReferences=Array.AsReadOnly(result.Glyphs.ToArray());DetachedDelimiterReferences=Array.AsReadOnly(result.Detached.ToArray());
        OwnerFirstScores1C=new ReadOnlyDictionary<ulong,float>(new Dictionary<ulong,float>(result.OwnerFirstScores));MinimumOwnerFirstScore80=result.MinimumFirstScore;
    }
    public byte[] CopyGlyphRecord(ulong reference,CancellationToken token=default)=>Storage.GlyphPool.CopyRecord(reference,token);
    public IReadOnlyList<LegacyNativeGlyphRecordNode> CopyGlyphOwners(CancellationToken token=default)=>Storage.GlyphPool.CopyOwnerNodes(token);
}

/// <summary>Genuine same119 terminal ->literal first rich-pool/WORD join.
/// Allocation bytes are explicit; source policies/scores/boundaries are owned.</summary>
public static class LegacyOwnedEnglish119PlainText
{
    public static LegacyEnglish119PlainTextResult Materialize(LegacyEnglish119TerminalResult terminal,
        LegacyEnglish119PlainAllocationStorage allocation,CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(terminal);ArgumentNullException.ThrowIfNull(allocation);token.ThrowIfCancellationRequested();
        var word=allocation.CopyWordStorage();var glyph=allocation.CopyGlyphStorage();
        var factory=LegacyOwnedEnglish119PlainResultFactory.Prepare(terminal,token);
        var interior=new LegacyOwnedEnglish119InteriorDelimiter(factory,token);var storage=factory.CreateStorage(token);
        var operations=new LegacyFirstPlainTextOperations(factory.ShouldAllocate,factory.CreatePlan,factory.ApplySymbol,
            group=>factory.Cache(group,token),interior.Read);
        var result=LegacyNativeFirstPlainTextKernel.Run(storage,factory.Graph,terminal.Context.Owner.Primary.Primary,
            factory.Alternative.Choice,factory.Alternative.Cells,terminal.Context.Owner.MergedColumnsA4,
            factory.Policy.Owner.Model.Source.SourceKind,operations,word,glyph,token);
        token.ThrowIfCancellationRequested();var completed=new LegacyEnglish119PlainTextResult(factory,result,allocation.Fingerprint,token);
        token.ThrowIfCancellationRequested();return completed;
    }
}
