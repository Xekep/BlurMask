using System.Buffers.Binary;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
[assembly:InternalsVisibleTo("LegacyFullEnglishFirstPlainText.Probe")]
namespace InkRecognition;

/// <summary>Detached rich WORD component, not UTF8/accepted text or source720.
/// Opaque references belong to this child's private pool.</summary>
public sealed class LegacyFullEnglishFirstPlainWordSnapshot
{
    private readonly byte[] data;private readonly ulong[] references;
    public bool IsDelimiter{get;} public int TotalCostDelta{get;} public int AcousticCostDelta{get;} public int SourceNodeSpan{get;}
    public LegacyNativeWordSourceSpan Span{get;} public int WordIndex{get;}
    internal LegacyFullEnglishFirstPlainWordSnapshot(LegacyFirstPublishedTextWord actual,CancellationToken token)
    {
        token.ThrowIfCancellationRequested();IsDelimiter=actual.IsDelimiter;TotalCostDelta=actual.TotalCostDelta;AcousticCostDelta=actual.AcousticCostDelta;SourceNodeSpan=actual.SourceNodeSpan;
        Span=actual.Published.Span;WordIndex=actual.Published.WordIndex;data=actual.Published.Word.CopyStorage();references=actual.Published.Word.FlatReferences.ToArray();
    }
    public byte[] CopyStorage()=>data.ToArray();public ulong[] CopyReadingOrderReferences()=>references.ToArray();
}

/// <summary>Semantic full-source rich-pool/WORD child. The mutable builders
/// remain private. No serialized273 unknown fields or final accepted text.</summary>
public sealed class LegacyFullEnglishFirstPlainChild
{
    public LegacyFullEnglishFirstPlainText Owner{get;} public LegacyFullEnglishFirstTerminalChild Terminal{get;}
    internal LegacyFullEnglishFirstPlainResultFactory Factory{get;} internal LegacyFirstPlainTextStorage Storage{get;}
    internal IReadOnlyList<LegacyFirstPublishedTextWord> NativeWords{get;}
    public LegacyOrdinaryRawInputDescriptor RawDescriptor=>Factory.Preparation.RawDescriptor;
    public LegacyOrdinaryRawInputDescriptor AssignedDescriptor=>Factory.AssignedDescriptor;
    public IReadOnlyList<LegacyFullEnglishFirstPlainWordSnapshot> Words{get;}
    public IReadOnlyList<ulong> AllocatedGlyphReferences{get;} public IReadOnlyList<ulong> DetachedDelimiterReferences{get;}
    public IReadOnlyDictionary<ulong,float> OwnerFirstScores1C{get;} public float MinimumOwnerFirstScore{get;}
    public int RichAllocationAttempts{get;} public int WordPublicationAttempts=>Words.Count;
    internal LegacyFullEnglishFirstPlainChild(LegacyFullEnglishFirstPlainText owner,LegacyFullEnglishFirstPlainResultFactory factory,LegacyFirstPlainTextStorage storage,LegacyNativeFirstPlainTextKernelResult? result,CancellationToken token)
    {
        Owner=owner;Terminal=factory.Terminal;Factory=factory;Storage=storage;
        var native=result?.Words??[];NativeWords=Array.AsReadOnly(native.ToArray());
        foreach(var word in native){token.ThrowIfCancellationRequested();if(!ReferenceEquals(word.Published.Word.Pool,storage.GlyphPool))throw new InvalidDataException("Foreign full WORD pool.");}
        Words=Array.AsReadOnly(native.Select(w=>new LegacyFullEnglishFirstPlainWordSnapshot(w,token)).ToArray());
        AllocatedGlyphReferences=Array.AsReadOnly(result?.Glyphs.ToArray()??[]);DetachedDelimiterReferences=Array.AsReadOnly(result?.Detached.ToArray()??[]);
        OwnerFirstScores1C=new ReadOnlyDictionary<ulong,float>(result is null?new Dictionary<ulong,float>():new Dictionary<ulong,float>(result.OwnerFirstScores));MinimumOwnerFirstScore=result?.MinimumFirstScore??1;
        int attempts=0;
        if(result is not null)
        {
            attempts=1;for(int i=0;i<Terminal.Cells.Count;i++)
            {token.ThrowIfCancellationRequested();var cell=Terminal.Cells[i];if(cell.Symbol is 211 or 217)continue;attempts++;if(i==0||cell.WordBoundaryBit)attempts++;}
        }
        RichAllocationAttempts=attempts;
    }
    public byte[] CopyGlyphRecord(ulong reference,CancellationToken token=default)=>Storage.GlyphPool.CopyRecord(reference,token);
    public IReadOnlyList<LegacyNativeGlyphRecordNode> CopyGlyphOwners(CancellationToken token=default)=>Storage.GlyphPool.CopyOwnerNodes(token);
}

/// <summary>One fresh completed full B5 -> rich glyph/pool/WORD pass. Default
/// storage is library-zeroed, not inferred native allocator residue. No source720,
/// final guide, serialized273, accepted text or default-recognizer change.</summary>
public sealed class LegacyFullEnglishFirstPlainText
{
    public LegacyFullEnglishFirstTerminal Owner{get;}
    private readonly object gate=new();private readonly LegacyFullEnglishFirstTerminalChild[] children;
    private readonly LegacyFullEnglishFirstPlainBudget budget;private readonly LegacyCursiveCategoryProfile categories;
    private readonly List<LegacyFullEnglishFirstPlainChild> prefix=[];private bool entered,running,complete,faulted;
    public bool IsComplete{get{lock(gate)return complete;}}public bool IsFaulted{get{lock(gate)return faulted;}}
    public long AdditionalLogicalPeakBytes=>budget.Peak;public long LogicalCopyWorkBytes=>budget.Work;public long LogicalVisitWork=>budget.Visits;
    public int ReservedRichAttempts=>budget.RichAttempts;public int ReservedWordAttempts=>budget.WordAttempts;
    private LegacyFullEnglishFirstPlainText(LegacyFullEnglishFirstTerminal owner,long peak,long work,long visits,CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(owner);token.ThrowIfCancellationRequested();Owner=owner;RequireComplete();children=owner.CopyCompletedChildren().ToArray();
        foreach(var c in children){token.ThrowIfCancellationRequested();if(!ReferenceEquals(c.Owner,owner)||!ReferenceEquals(c.Context.Owner,owner.Owner)||!ReferenceEquals(c.Context.Primary.Owner,owner.Owner.Owner)||!ReferenceEquals(c.Context.Primary.Initializer.Owner,owner.Owner.Owner.Owner))throw new ArgumentException("Same canonical full B5/B4/B3/B2 required.");}
        budget=new(children,peak,work,visits,token);
        var first=children[0].Context.Primary.Initializer;var model=first.ActualCache.Model;var source=model.Source;
        if(model.SelectedProcessor.Kind!=0x249||model.SelectedProcessor.Version!=0x100||model.SelectedProcessor.Offset!=8827||model.SelectedProcessor.End!=373961||source.Offset!=8843||source.End!=373957||source.SourceKind!=0x135||source.Version!=0x110||owner.Invocation.Configuration99||owner.Invocation.ConfigurationA8)
            throw new NotSupportedException("Actual fresh full249 literal FIRST scope required.");
        var policy=new LegacyFullEnglishFirstPolicy(first.Owner.Owner,first.ActualCache,token);categories=policy.Categories;
        if(source.Fields.Single(f=>f.Owner=="247"&&f.Destination=="B9").CopyBytes()[0]!=0)throw new NotSupportedException("Word geometry finalization remains unported.");
        var graph=first.ActualCache.Owner;var bytes=source.CopyChildEnvelopeBytes(graph.GraphChild,token);
        if(bytes.Length!=355||LegacyCursiveLatticeSpanProfile.ReadV10aResourceNode(bytes,0).NativeSecondByte!=1||bytes[350]!=0)throw new NotSupportedException("Actual full ordinary graph09=1/159=0 required.");
        foreach(var c in children){token.ThrowIfCancellationRequested();if(!ReferenceEquals(c.Context.Primary.Initializer.ActualCache.Model,model)||!ReferenceEquals(c.Context.Primary.Initializer.ActualCache.Owner.Preparation.Source,source))throw new ArgumentException("Same full parent/source required.");}
    }
    private void RequireComplete()
    {
        if(!Owner.IsComplete||Owner.IsFaulted||!Owner.Owner.IsComplete||Owner.Owner.IsFaulted||!Owner.Owner.Owner.IsComplete||Owner.Owner.Owner.IsFaulted||!Owner.Owner.Owner.Owner.IsComplete||Owner.Owner.Owner.Owner.IsFaulted||!Owner.Owner.Owner.Owner.Owner.IsComplete||Owner.Owner.Owner.Owner.Owner.IsFaulted||!Owner.Owner.Owner.Owner.Owner.Parent.IsComplete||Owner.Owner.Owner.Owner.Owner.Parent.IsFaulted)throw new ArgumentException("Complete nonfaulted full B5..A authority required.");
    }
    public static LegacyFullEnglishFirstPlainText Prepare(LegacyFullEnglishFirstTerminal completedB5,CancellationToken token=default)=>new(completedB5,LegacyFullEnglishFirstPlainBudget.PeakLimit,LegacyFullEnglishFirstPlainBudget.WorkLimit,LegacyFullEnglishFirstPlainBudget.VisitLimit,token);
    internal static LegacyFullEnglishFirstPlainText PrepareBounded(LegacyFullEnglishFirstTerminal owner,long peak,long work,long visits,CancellationToken token=default)=>new(owner,peak,work,visits,token);
    public void Run(CancellationToken token=default)=>RunCore(null,null,new byte[120],new byte[40],token);
    internal void RunPrepared(Action<string>? checkpoint,Action<LegacyFullEnglishFirstPlainCostObservation>? observation,ReadOnlyMemory<byte> word,ReadOnlyMemory<byte> glyph,CancellationToken token=default)=>RunCore(checkpoint,observation,word,glyph,token);
    private void RunCore(Action<string>? checkpoint,Action<LegacyFullEnglishFirstPlainCostObservation>? observation,ReadOnlyMemory<byte> word,ReadOnlyMemory<byte> glyph,CancellationToken token)
    {
        token.ThrowIfCancellationRequested();RequireComplete();if(word.Length!=120||glyph.Length!=40||BinaryPrimitives.ReadUInt64LittleEndian(word.Span[32..])!=0||BinaryPrimitives.ReadUInt64LittleEndian(word.Span[88..])!=0||BinaryPrimitives.ReadUInt64LittleEndian(word.Span[104..])!=0||BinaryPrimitives.ReadInt32LittleEndian(word.Span[96..])!=0||BinaryPrimitives.ReadInt32LittleEndian(word.Span[112..])!=0)throw new ArgumentException("Explicit empty allocator storage constraints required.");
        word=word.ToArray();glyph=glyph.ToArray();lock(gate){token.ThrowIfCancellationRequested();if(entered||running)throw new InvalidOperationException("Full B6 owner is one-use.");entered=running=true;}
        try
        {
            checkpoint?.Invoke("entry");token.ThrowIfCancellationRequested();int records=0,attempts=0,words=0;
            foreach(var child in children)
            {
                checkpoint?.Invoke("child:entry");token.ThrowIfCancellationRequested();var factory=new LegacyFullEnglishFirstPlainResultFactory(child,categories,observation,token);var storage=factory.CreateStorage(token);LegacyNativeFirstPlainTextKernelResult? result=null;
                if(child.HasFiniteTerminal)
                {
                    var interior=new LegacyFullEnglishFirstInteriorDelimiter(factory,token);
                    var operations=new LegacyFirstPlainTextOperations(factory.ShouldAllocate,factory.CreatePlan,factory.ApplySymbol,g=>factory.Cache(g,token),interior.Read);
                    result=LegacyNativeFirstPlainTextKernel.Run(storage,factory.Graph,child.Context.Primary.Primary,child.Choice!.Value,child.Cells,1,0x135,operations,word,glyph,token);
                }
                checkpoint?.Invoke("child:materialized");token.ThrowIfCancellationRequested();var value=new LegacyFullEnglishFirstPlainChild(this,factory,storage,result,token);
                attempts=checked(attempts+value.RichAllocationAttempts);words=checked(words+value.WordPublicationAttempts);
                foreach(var node in storage.GlyphPool.CopyOwnerNodes(token)){token.ThrowIfCancellationRequested();records=checked(records+node.Records.Length/40);}
                if(records>attempts||attempts>budget.RichAttempts||words>budget.WordAttempts)throw new InvalidDataException("Rich allocation/publication exceeded reserved whole-parent attempts.");
                lock(gate){token.ThrowIfCancellationRequested();prefix.Add(value);}checkpoint?.Invoke("child:published");token.ThrowIfCancellationRequested();
            }
            lock(gate){token.ThrowIfCancellationRequested();complete=true;}
        }
        catch{lock(gate)faulted=true;throw;}finally{lock(gate)running=false;}
    }
    /// <summary>Detached completed prefixes. Faulted/incomplete owners provide
    /// historical data only, never future source720 authority.</summary>
    public IReadOnlyList<LegacyFullEnglishFirstPlainChild> CopyCompletedChildren(){lock(gate)return Array.AsReadOnly(prefix.ToArray());}
}
