using System.Buffers.Binary;

namespace InkRecognition;

/// <summary>One copied120-byte WORD and its unchanged reading references in
/// the newly imported result lifecycle. It is not an original275 pool alias.</summary>
public sealed class LegacyImportedControlWord
{
    private readonly byte[] storage;
    private readonly ulong[] references;
    public LegacyNativeWordSourceSpan SourceSpan {get;}
    public int WordIndex {get;}
    internal LegacyImportedControlWord(LegacyNativePublishedWordNode node)
    {storage=node.Word.CopyStorage();references=node.Word.FlatReferences.ToArray();SourceSpan=node.Span;WordIndex=node.WordIndex;}
    public byte[] CopyStorage120()=>storage.ToArray();
    public ulong[] CopyReferences()=>references.ToArray();
}

/// <summary>Fresh completed global main result -> bounded13BE80 control-import
/// lifecycle. The original pool/WORDs are unchanged. Dynamic control bytes,
/// original primary AL and alternate storage remain separate owners.</summary>
public sealed class LegacyFreshMainGlobalControlImport
{
    private readonly LegacyNativeGlyphRecordNode[] originalNodes;
    private readonly LegacyNativePublishedWordNode[] sourceWords;
    private readonly LegacyImportedControlWord[] words;
    private int started;
    private bool complete,stopped;
    public LegacyFreshMainGlobalFirstAlternativeSnapshot Source {get;}
    public LegacyNativeGlyphControlImport ImportedPool {get;}
    public IReadOnlyList<LegacyImportedControlWord> Words {get;}
    public bool Succeeded=>complete&&!stopped&&ImportedPool.Succeeded;
    public bool IsStopped=>stopped||ImportedPool.IsStopped;
    public bool OwnsCopiedAlternateStorage {get;private set;}
    public int AlternateByteCount=>0;
    public int AlternateOffsetCount=>0;
    /// <summary>13B5A0 reserves count+1 even for empty alternate bytes. Its
    /// unused allocation byte has no modeled value; no NUL is invented.</summary>
    public int AlternateByteCapacity {get;private set;}
    private LegacyFreshMainGlobalControlImport(LegacyFreshMainGlobalFirstAlternativeSnapshot source,
        LegacyNativeGlyphRecordNode[] nodes,LegacyNativePublishedWordNode[] sourceWords,LegacyImportedControlWord[] words)
    {
        Source=source;originalNodes=nodes;this.sourceWords=sourceWords;this.words=words;Words=Array.AsReadOnly(words);
        ImportedPool=new(nodes,source.SourcePolicy.Owner.Model.AcousticAlphabet);
    }
    public static LegacyFreshMainGlobalControlImport Prepare(LegacyFreshMainGlobalFirstAlternativeSnapshot source,
        CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(source);token.ThrowIfCancellationRequested();
        var session=source.Session;var ownership=session.Ownership;var policy=source.SourcePolicy;
        source.Terminal.PassResult.ValidateResultQueryLease(token);
        if(!session.IsComplete||session.IsStopped||!ReferenceEquals(source.Terminal,ownership.Materialization.TerminalResult)||
            !ReferenceEquals(source.WordHead18,ownership.WordPublisher.Head)||source.ActiveAlternativeCount60!=1||
            policy.HasAssociatedSource||policy.Configuration.Trace2368!=0||policy.Refresh.SourceFormat22f9!=0||
            policy.Owner.Model.SourceKind!=0x135)
            throw new ArgumentException("Actual completed same-owner fresh plain global first alternative required.");
        var sourceWords=new List<LegacyNativePublishedWordNode>();var words=new List<LegacyImportedControlWord>();
        var nodes=ownership.GlyphPool.CopyOwnerNodes(token).ToArray();var records=new LegacyNativeGlyphRecordOwner(nodes);
        for(var node=source.WordHead18;node is not null;node=node.Next)
        {
            token.ThrowIfCancellationRequested();
            if(sourceWords.Count==4096||!ReferenceEquals(node.Word.Pool,ownership.GlyphPool))
                throw new ArgumentException("Bounded published words must retain the exact original pool.");
            byte[] storage=node.Word.CopyStorage();ValidateAuxiliaryCounts(storage);
            foreach(ulong reference in node.Word.FlatReferences)_=records.Resolve(reference);
            sourceWords.Add(node);words.Add(new(node));
        }
        if(sourceWords.Count!=source.PublishedWordCount)throw new ArgumentException("Actual completed published word count required.");
        token.ThrowIfCancellationRequested();return new(source,nodes,sourceWords.ToArray(),words.ToArray());
    }
    public LegacyNativeGlyphControlImport Run(CancellationToken token=default)
    {
        token.ThrowIfCancellationRequested();
        if(Interlocked.Exchange(ref started,1)!=0)throw new InvalidOperationException("Imported result creation is single-use, including failure.");
        try
        {
            ValidateSource(token);
            //13B5A0's actual fresh empty input: no bytes or offsets to copy;
            //one spare byte exists but its uninitialized contents are omitted.
            AlternateByteCapacity=1;OwnsCopiedAlternateStorage=true;
            ImportedPool.Execute(token);ValidateSource(token);complete=true;return ImportedPool;
        }
        catch{stopped=true;throw;}
    }
    private void ValidateSource(CancellationToken token)
    {
        token.ThrowIfCancellationRequested();Source.Terminal.PassResult.ValidateResultQueryLease(token);
        var session=Source.Session;
        if(!session.IsComplete||session.IsStopped||!ReferenceEquals(Source.WordHead18,session.Ownership.WordPublisher.Head))
            throw new InvalidOperationException("Original result lifecycle changed after import snapshot preparation.");
        var current=session.Ownership.GlyphPool.CopyOwnerNodes(token);
        if(current.Count!=originalNodes.Length)throw new InvalidOperationException("Original glyph pool changed after snapshot preparation.");
        var byKey=current.ToDictionary(n=>n.Key);
        foreach(var node in originalNodes)
            if(!byKey.TryGetValue(node.Key,out var other)||other.PackedCount!=node.PackedCount||!other.Records.Span.SequenceEqual(node.Records.Span))
                throw new InvalidOperationException("Original glyph pool changed after snapshot preparation.");
        for(int i=0;i<sourceWords.Length;i++)
        {
            byte[] bytes=sourceWords[i].Word.CopyStorage();ValidateAuxiliaryCounts(bytes);
            if(!bytes.AsSpan().SequenceEqual(words[i].CopyStorage120())||
                !sourceWords[i].Word.FlatReferences.SequenceEqual(words[i].CopyReferences()))
                throw new InvalidOperationException("Original WORD changed after import snapshot preparation.");
        }
    }
    internal static void ValidateAuxiliaryCounts(ReadOnlySpan<byte> storage)
    {
        if(storage.Length!=120||BinaryPrimitives.ReadInt32LittleEndian(storage[96..])!=0||
            BinaryPrimitives.ReadInt32LittleEndian(storage[112..])!=0)
            throw new NotSupportedException("Nonempty WORD58/68 auxiliary vectors require their actual import producers.");
    }
}
