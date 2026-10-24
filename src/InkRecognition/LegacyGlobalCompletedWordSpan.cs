using System.Buffers.Binary;
namespace InkRecognition;

//Private-issued live span/cursor. There is no snapshot/Result/byte ingress or synthetic279.
internal sealed class LegacyGlobalCompletedWordSpan
{
    private readonly LegacyGlobalCompletedTextSource binding;
    private readonly LegacyNativePlainWordPublisher publisher;
    private readonly LegacyNativePlainWordSpanSnapshot snapshot;
    private readonly int thread=Environment.CurrentManagedThreadId;
    private LegacyFreshMainGlobalWordTextPublication? parent;
    private Cursor? current;
    private int next;
    private bool poisoned;
    internal int Count=>snapshot.Words.Count;
    internal int SelectedIndex{get;}
    internal int TotalReferences{get;}
    internal long NativeChoiceAndLinkLayoutBytes=>checked(48L*Count+24L*TotalReferences);
    internal long ReferenceArrayCopyBytes=>checked(8L*TotalReferences);
    private LegacyGlobalCompletedWordSpan(LegacyGlobalCompletedTextSource binding,CancellationToken token)
    {
        this.binding=binding;binding.Validate(token);publisher=binding.Source.Session!.Ownership.WordPublisher;
        publisher.RequireSoleActiveSourceHead(binding.Word);
        if(binding.Snapshot.ActiveAlternativeCount60!=1||binding.Word.Next is not null)
            throw new NotSupportedException("One actual TOP with one shared WORD span required for all275 publication.");
        snapshot=publisher.CaptureSpan(binding.Word,[binding.Word],token);
        if(snapshot.Words.Count is<1 or>4095||snapshot.SelectedNodes.Count!=1||!ReferenceEquals(snapshot.SelectedNodes[0],binding.Word))
            throw new NotSupportedException("Complete bounded actual275 array and selected279 required.");
        SelectedIndex=snapshot.SelectedOrdinals[0];int references=0;
        foreach(var row in snapshot.Words)
        {
            token.ThrowIfCancellationRequested();
            if(!ReferenceEquals(row.Pool,binding.Word.Word.Pool)||row.FlatReferences.Count is<1 or>4096||row.FlatReferences.Count>65536-references)
                throw new NotSupportedException("Actual ordinary275 rows and aggregate64Ki reference cap required before claim.");
            byte[] bytes=row.CopyStorage();
            if(bytes.Length!=120||BinaryPrimitives.ReadUInt64LittleEndian(bytes.AsSpan(88))!=0||BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(96))!=0||
                BinaryPrimitives.ReadUInt64LittleEndian(bytes.AsSpan(104))!=0||BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(112))!=0)
                throw new NotSupportedException("Every actual275 WORD must have the restored plain/no-auxiliary layout.");
            references+=row.FlatReferences.Count;
        }
        TotalReferences=references;Require(token);
    }
    internal static LegacyGlobalCompletedWordSpan Capture(LegacyGlobalCompletedTextSource binding,CancellationToken token)=>new(binding,token);
    internal void Bind(LegacyFreshMainGlobalWordTextPublication actual)
    {if(parent is not null||!ReferenceEquals(actual.Wrapper.ActualCompletion,binding))throw new ArgumentException("Private same-completion span parent required.");parent=actual;}
    internal void Require(CancellationToken token)
    {
        binding.Validate(token);
        if(poisoned)throw new InvalidOperationException("Entered all275 publication cursor is poisoned.");
        if(thread!=Environment.CurrentManagedThreadId||!ReferenceEquals(publisher.Head,binding.Word)||binding.Word.WordIndex!=SelectedIndex)
            throw new InvalidOperationException("Same current thread/completed source/TOP selected index required.");
        publisher.RequireSoleActiveSourceHead(binding.Word);publisher.RequireLiveSpanRows(snapshot,token);
    }
    internal IEnumerable<ulong> EnumerateReferences(CancellationToken token)
    {Require(token);foreach(var word in snapshot.Words)foreach(ulong reference in word.FlatReferences){token.ThrowIfCancellationRequested();yield return reference;}}
    internal Cursor Open(LegacyFreshMainGlobalWordTextPublication actual,CancellationToken token)
    {
        Require(token);if(!ReferenceEquals(parent,actual)||current is not null||next>=Count)throw new InvalidOperationException("Next private actual275 cursor required.");
        current=new(this,next,snapshot.Words[next]);return current;
    }
    internal void Close(LegacyFreshMainGlobalWordTextPublication actual,Cursor cursor,CancellationToken token)
    {RequireCursor(actual,cursor,token);current=null;next++;}
    internal void RequireComplete(LegacyFreshMainGlobalWordTextPublication actual,CancellationToken token)
    {Require(token);if(!ReferenceEquals(parent,actual)||current is not null||next!=Count)throw new InvalidOperationException("Every actual275 choice must complete before source registration.");}
    internal void RequireCursor(LegacyFreshMainGlobalWordTextPublication actual,Cursor cursor,CancellationToken token)
    {
        Require(token);if(!ReferenceEquals(parent,actual)||!ReferenceEquals(current,cursor)||!ReferenceEquals(cursor.Owner,this)||cursor.Ordinal!=next||
            !ReferenceEquals(cursor.Word,snapshot.Words[next]))throw new InvalidOperationException("Stale/foreign actual275 cursor.");
    }
    internal void Fault(LegacyFreshMainGlobalWordTextPublication actual)
    {if(!ReferenceEquals(parent,actual))throw new ArgumentException("Actual all275 parent required.");poisoned=true;}
    internal sealed class Cursor
    {
        internal LegacyGlobalCompletedWordSpan Owner{get;}
        internal int Ordinal{get;}
        internal LegacyNativePlainWordRecord Word{get;}
        internal Cursor(LegacyGlobalCompletedWordSpan owner,int ordinal,LegacyNativePlainWordRecord word){Owner=owner;Ordinal=ordinal;Word=word;}
    }
}
