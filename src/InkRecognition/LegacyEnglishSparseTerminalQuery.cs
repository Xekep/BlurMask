using System.Buffers.Binary;
using System.Runtime.CompilerServices;
[assembly:InternalsVisibleTo("LegacyEnglishSparseQuery.Probe")]
namespace InkRecognition;

/// <summary>Lexical terminal, not a handwriting-selected candidate or controller token.
/// The ingress policy is independently ordinary/suppress0; boundary is explicit.</summary>
public sealed class LegacyEnglishLexicalTerminal
{
    internal LegacyEnglishSparseTerminalQuery Owner{get;}
    public int Node{get;}
    public int WordId{get;}
    public bool Boundary{get;}
    public bool SuppressCategory=>false;
    internal LegacyEnglishLexicalTerminal(LegacyEnglishSparseTerminalQuery owner,int node,int word,bool boundary)
    {Owner=owner;Node=node;WordId=word;Boundary=boundary;}
    public byte[] CopySource70Token()
    {Owner.Validate();byte[] b=new byte[16];BinaryPrimitives.WriteInt32LittleEndian(b.AsSpan(4),WordId);b[12]=(byte)(Boundary?1:0);return b;}
}

/// <summary>Completed immutable V58 numerical result, not a linguistic winner.
/// Only native-written bytes are exported; caller allocation residue survives.</summary>
public sealed class LegacyEnglishSparseQueryResult
{
    internal LegacyEnglishSparseTerminalQuery Owner{get;}
    internal LegacyEnglishSparseQueryRow Row{get;}
    public LegacyEnglishLexicalTerminal Terminal{get;}
    public int Cost=>Row.Cost;
    public int MatchedOrder=>Row.MatchedOrder;
    public int HistoryCount=>Row.HistoryCount;
    public ulong WrittenByteMask=>Row.WrittenByteMask;
    internal LegacyEnglishSparseQueryResult(LegacyEnglishSparseTerminalQuery owner,LegacyEnglishLexicalTerminal terminal,LegacyEnglishSparseQueryRow row)
    {Owner=owner;Terminal=terminal;Row=row;}
    public uint GetHistory(int index){Owner.Validate();return Row.GetHistory(index);}
    public void WriteRecord(Span<byte> destination){Owner.Validate();Row.Write(destination);}
    public byte[] CopyRecordOver(ReadOnlySpan<byte> allocationResidue)
    {Owner.Validate();if(allocationResidue.Length!=40)throw new ArgumentException("Exact40-byte allocation residue required.");byte[] b=allocationResidue.ToArray();Row.Write(b);return b;}
}

internal readonly record struct LegacyEnglishSparseQueryRow(int Cost,int MatchedOrder,int HistoryCount,uint History0,uint History1,int Word)
{
    internal ulong WrittenByteMask=>0xff0001ffUL|((HistoryCount>=1?15UL:0)<<12)|((HistoryCount==2?15UL:0)<<16);
    internal uint GetHistory(int index)=>index>=0&&index<HistoryCount?(index==0?History0:History1):throw new ArgumentOutOfRangeException(nameof(index));
    internal void Write(Span<byte> b)
    {
        if(b.Length!=40)throw new ArgumentException("Exact40-byte destination required.");
        BinaryPrimitives.WriteInt32LittleEndian(b,Cost);BinaryPrimitives.WriteInt32LittleEndian(b[4..],MatchedOrder);b[8]=(byte)HistoryCount;
        if(HistoryCount>=1)BinaryPrimitives.WriteUInt32LittleEndian(b[12..],History0);
        if(HistoryCount==2)BinaryPrimitives.WriteUInt32LittleEndian(b[16..],History1);
        BinaryPrimitives.WriteInt32LittleEndian(b[24..],-1);BinaryPrimitives.WriteInt32LittleEndian(b[28..],Word);
    }
}

/// <summary>Actual EN sparse2D2.V58 terminal queries under independent ordinary
///lexical-ingress/suppress0/category0/no-option/no-trace policy. Immutable shared
///tables, same-owner previous results. No NN selection, decoder or app acceptance.</summary>
public sealed class LegacyEnglishSparseTerminalQuery
{
    public const int Impossible=0x04000000;
    private readonly LegacyLexiconWordIndex words;
    private readonly byte[] wordCodes;
    private readonly byte[][] classCodes,backoffCodes;
    private readonly int[] wordCosts;
    private readonly int[][] classCosts,backoffCosts;
    public LegacyEnglishMainOnlySourceMasks Masks{get;}
    public LegacyEnglishLkFreshNumericBinding NumericOwner=>Masks.NumericOwner;
    public LegacyEnglishLkSparseLanguageData LanguageOwner=>NumericOwner.LanguageOwner;
    public LegacyEnglishLkPackedData DataOwner=>NumericOwner.DataOwner;
    public string Policy=>"independent-ordinary-lexical-terminal/suppress0/category0/no-option/no-trace";
    public long AdditionalLogicalAllocationBudget{get;}
    private LegacyEnglishSparseTerminalQuery(LegacyEnglishMainOnlySourceMasks masks,LegacyLexiconWordIndex words,byte[] wordCodes,byte[][] classCodes,byte[][] backoffCodes,int[] wordCosts,int[][] classCosts,int[][] backoffCosts,long budget)
    {Masks=masks;this.words=words;this.wordCodes=wordCodes;this.classCodes=classCodes;this.backoffCodes=backoffCodes;this.wordCosts=wordCosts;this.classCosts=classCosts;this.backoffCosts=backoffCosts;AdditionalLogicalAllocationBudget=budget;}
    public static LegacyEnglishSparseTerminalQuery Create(LegacyEnglishMainOnlySourceMasks masks,long maximumAdditionalBytes=16L*1024*1024,CancellationToken token=default)=>CreateCore(masks,maximumAdditionalBytes,null,token);
    internal static LegacyEnglishSparseTerminalQuery CreatePrepared(LegacyEnglishMainOnlySourceMasks masks,long limit,Action<string> checkpoint,CancellationToken token)=>CreateCore(masks,limit,checkpoint,token);
    private static LegacyEnglishSparseTerminalQuery CreateCore(LegacyEnglishMainOnlySourceMasks masks,long limit,Action<string>? checkpoint,CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(masks);token.ThrowIfCancellationRequested();Validate(masks);var data=masks.NumericOwner.DataOwner;var language=masks.NumericOwner.LanguageOwner;var numeric=masks.NumericOwner;
        long budget=Preflight(data,limit);
        void Check(string stage){checkpoint?.Invoke(stage);token.ThrowIfCancellationRequested();}
        Check("query:preflight");
        //One complete identity-bound terminal index. No per-query map or decoded
        //643871-value language copy. Existing sparse packed accessor stays on-wire.
        var words=new LegacyLexiconWordIndex(data.Dictionary,token);Check("query:index");
        if(!words.IsForGraph(data.Dictionary)||words.WordCount!=260074)throw new InvalidDataException("Whole actual terminal ordinal map required.");
        var wc=language.CopyFieldPayload("98.wordCodes");var cc=new byte[3][];var bc=new byte[2][];var c=new int[3][];var b=new int[2][];
        Check("query:wordCodes");var w=numeric.CopyWordCosts();
        for(int i=0;i<3;i++){Check("query:tables");cc[i]=language.CopyFieldPayload("C0."+i);c[i]=numeric.CopyClassCosts(i);if(i<2){bc[i]=language.CopyFieldPayload("C8."+i);b[i]=numeric.CopyBackoffCosts(i);}}
        Check("query:publish");Validate(masks);return new(masks,words,wc,cc,bc,w,c,b,budget);
    }
    internal static long Preflight(LegacyEnglishLkPackedData data,long limit=16L*1024*1024)
    {
        ArgumentNullException.ThrowIfNull(data);var graph=data.Dictionary;
        if(graph.NodeCount!=666547||graph.StoredTerminalCount!=260074||graph.MaximumWordLength!=43||data.LexicalAlphabet.LabelCount!=104)throw new InvalidDataException("Whole actual English dictionary required.");
        //Additional allocations, not RSS: retained int map, ALL retained code
        //and cost arrays; generic index's worst increasing16..104 fanout scratch
        //arrays+headers, two257-int traversal stacks;64KiB conservative object/
        //receipt bookkeeping. Existing source/data/masks are shared, not copied.
        long bytes=checked(666547L*4+644893+1536*4+(104L*105/2+16)*4+105*32+2*257*4+64*1024);
        if(limit<bytes||bytes>16L*1024*1024)throw new ArgumentException("Complete additional English query allocation budget exceeded.");return bytes;
    }
    private static void Validate(LegacyEnglishMainOnlySourceMasks masks)
    {
        var n=masks.NumericOwner;var l=n.LanguageOwner;var d=n.DataOwner;var k=masks.Knowledge;
        if(!ReferenceEquals(masks.Source,d.Source)||!ReferenceEquals(k.Source,d.Source)||!ReferenceEquals(l.DataOwner,d)||!k.Owner.IsComplete||k.Owner.IsFaulted||!k.Owner.Owner.IsComplete||k.Owner.Owner.IsFaulted)throw new InvalidOperationException("Same complete actual English MAIN/P/L owners required.");
        if(masks.Policy!="independent-main13B-only/nonbypassed/no-user-restriction"||masks.DescriptorFlags!=0xc0e||masks.LanguageOrder!=3||masks.SelectedSourceIndex!=0||masks.SelectedPriority!=1024||l.Order!=3||l.ClassSlots!=1024||l.WordSlots!=260075||l.ReservedWordIndex!=260074||n.Context00!=Impossible||n.LiteralPacketByte0!=0||n.Profile!=2||n.SourceScale!=1)throw new NotSupportedException("Only actual independent MAIN sparse category0/no-option/no-trace policy is admitted.");
    }
    internal void Validate()=>Validate(Masks);
    public LegacyEnglishLexicalTerminal ReadOrdinaryTerminal(int node,bool boundary=false,CancellationToken token=default)
    {token.ThrowIfCancellationRequested();Validate();if((uint)node>=(uint)DataOwner.Dictionary.NodeCount)throw new ArgumentOutOfRangeException(nameof(node));if(!words.TryGetWordId(node,out int word))throw new NotSupportedException("Lexical ingress requires an actual terminal, never nonterminal ordinal residue.");token.ThrowIfCancellationRequested();return new(this,node,word,boundary);}
    public LegacyEnglishSparseQueryResult? First(LegacyEnglishLexicalTerminal terminal,CancellationToken token=default)=>Query(terminal,null,null,token);
    public LegacyEnglishSparseQueryResult? Continue(LegacyEnglishSparseQueryResult previous,LegacyEnglishLexicalTerminal terminal,CancellationToken token=default)
    {ArgumentNullException.ThrowIfNull(previous);return Query(terminal,previous,null,token);}
    internal LegacyEnglishSparseQueryResult? QueryPrepared(LegacyEnglishLexicalTerminal terminal,LegacyEnglishSparseQueryResult? previous,Action<string> checkpoint,CancellationToken token)=>Query(terminal,previous,checkpoint,token);
    private LegacyEnglishSparseQueryResult? Query(LegacyEnglishLexicalTerminal terminal,LegacyEnglishSparseQueryResult? previous,Action<string>? checkpoint,CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(terminal);token.ThrowIfCancellationRequested();Validate();
        if(!ReferenceEquals(terminal.Owner,this))throw new ArgumentException("Terminal receipt must belong to this exact query owner.");
        if(previous is not null&&!ReferenceEquals(previous.Owner,this))throw new ArgumentException("Previous history must be completed by this exact query owner.");
        Span<int> history=stackalloc int[2];int count;
        if(previous is null){count=1;history[0]=LanguageOwner.GetPackedValue(LegacyEnglishSparsePackedField.WordClasses,LanguageOwner.ReservedWordIndex);}
        else{count=previous.HistoryCount;for(int i=0;i<count;i++)history[i]=(int)previous.Row.GetHistory(i);}
        checkpoint?.Invoke("query:evaluate");
        var row=Evaluate(history[..count],LanguageOwner.GetPackedValue(LegacyEnglishSparsePackedField.WordClasses,terminal.WordId),terminal.WordId,terminal.Boundary,token);
        checkpoint?.Invoke("query:publish");
        token.ThrowIfCancellationRequested();Validate();return row.Cost<Impossible?new(this,terminal,row):null;
    }
    internal LegacyEnglishSparseQueryRow EvaluatePrepared(ReadOnlySpan<int> history,int currentClass,int word,bool boundary,CancellationToken token=default)
    {Validate();return Evaluate(history,currentClass,word,boundary,token);}
    private LegacyEnglishSparseQueryRow Evaluate(ReadOnlySpan<int> history,int current,int word,bool boundary,CancellationToken token)
        =>EvaluateCore(NumericOwner,wordCodes,classCodes,backoffCodes,wordCosts,classCosts,backoffCosts,
            history,current,word,boundary,token);

    //Pure numerical transport only; this confers no source/query/history authority.
    internal static LegacyEnglishSparseQueryRow EvaluateCore(LegacyEnglishLkFreshNumericBinding numeric,
        byte[] wordCodes,byte[][] classCodes,byte[][] backoffCodes,int[] wordCosts,int[][] classCosts,int[][] backoffCosts,
        ReadOnlySpan<int> history,int current,int word,bool boundary,CancellationToken token)
    {
        token.ThrowIfCancellationRequested();if(history.Length>2||(uint)current>=1024||(uint)word>=260074)throw new ArgumentOutOfRangeException(nameof(history));
        var language=numeric.LanguageOwner;
        Span<int> sequence=stackalloc int[3];for(int i=0;i<history.Length;i++){if((uint)history[i]>=1024)throw new ArgumentOutOfRangeException(nameof(history));sequence[i]=history[i];}sequence[history.Length]=current;
        int cost=boundary?numeric.Boundary94C:0,matchedOrder=0;
        for(int start=0;start<=history.Length;start++)
        {
            token.ThrowIfCancellationRequested();int index=sequence[start],matched=0,needed=history.Length-start;
            for(int level=0;level<needed;level++)
            {
                var offsets=level==0?LegacyEnglishSparsePackedField.Order1Offsets:LegacyEnglishSparsePackedField.Order2Offsets;
                var labels=level==0?LegacyEnglishSparsePackedField.Order2Labels:LegacyEnglishSparsePackedField.Order3Labels;
                int lo=language.GetPackedValue(offsets,index),hi=language.GetPackedValue(offsets,index+1),target=sequence[start+level+1],end=hi;
                while(lo<hi){int mid=lo+(hi-lo)/2;if(language.GetPackedValue(labels,mid)<target)lo=mid+1;else hi=mid;}
                if(lo==end||language.GetPackedValue(labels,lo)!=target)break;index=lo;matched++;
            }
            if(matched==needed){cost=checked(cost+classCosts[needed][classCodes[needed][index]]);matchedOrder=needed+1;break;}
            if(matched==needed-1)cost=checked(cost+backoffCosts[matched][backoffCodes[matched][index]]);
        }
        if(matchedOrder==0)throw new InvalidDataException("Actual unigram table must be total.");
        cost=checked(cost+wordCosts[wordCodes[word]]);
        int count=Math.Min(history.Length+1,2);uint first=(uint)(history.Length<2?sequence[0]:sequence[1]),second=count==2?(uint)current:0;
        return new(cost,matchedOrder,count,first,second,word);
    }
}
