using System.Buffers.Binary;
using System.Runtime.CompilerServices;

[assembly:InternalsVisibleTo("LegacyPreparedGlyphRewrite.Probe")]

namespace InkRecognition;

internal readonly record struct LegacyPreparedGlyphRewriteEvent(string Operation,int Word,int Ordinal,ulong Reference,int Value,int Extra);

/// <summary>Bounded PREPARED enabled21F320 driver. Preserves native prefetch,
/// repeated-reference liveness and ordered ushort08 stores. It cannot enable
/// the fresh controller's F4zero path or create an accepted text result.</summary>
internal sealed class LegacyPreparedPublishedGlyphRewrite
{
    private readonly LegacyPreparedContextualQuoteSource source;
    private readonly LegacyNativePublishedWordNode? head;
    private readonly byte preparedF4;
    private readonly Action<LegacyPreparedGlyphRewriteEvent>? observe;
    private readonly List<LegacyPreparedGlyphRewriteEvent> events=[];
    private int started;
    internal bool IsComplete{get;private set;}
    internal bool IsStopped{get;private set;}
    internal LegacyPreparedPublishedGlyphRewrite(LegacyPreparedContextualQuoteSource source,
        LegacyNativePublishedWordNode? actualLinkedHead,byte preparedConfigurationF4,
        Action<LegacyPreparedGlyphRewriteEvent>? observer=null)
    {ArgumentNullException.ThrowIfNull(source);this.source=source;head=actualLinkedHead;preparedF4=preparedConfigurationF4;observe=observer;}
    internal LegacyPreparedGlyphRewriteEvent[] CopyEvents()=>events.ToArray();

    internal void Run(CancellationToken token=default)
    {
        if(Interlocked.Exchange(ref started,1)!=0)throw new InvalidOperationException("Prepared rewrite is single-use, including a failed prefix.");
        try
        {
            token.ThrowIfCancellationRequested();
            var labels=source.Labels;
            if(preparedF4==0||!labels.HasEnabledReplacement){IsComplete=true;return;}
            if(head is null)throw new NotSupportedException("Enabled native body dereferences the first WORD; a null head is unsafe.");
            var words=new List<LegacyNativePlainWordRecord>();var seen=new HashSet<LegacyNativePublishedWordNode>();
            for(var node=head;node is not null;node=node.Next)
            {
                token.ThrowIfCancellationRequested();
                if(words.Count==4096||!seen.Add(node)||!ReferenceEquals(node.Word.Pool,source.Pool))
                    throw new ArgumentException("Bounded same-pool actual linked WORD list required.");
                byte[] storage=node.Word.CopyStorage();
                if(BinaryPrimitives.ReadUInt32LittleEndian(storage.AsSpan(0x28))!=node.Word.FlatReferences.Count||
                    BinaryPrimitives.ReadUInt32LittleEndian(storage.AsSpan(0x2c))<node.Word.FlatReferences.Count)
                    throw new ArgumentException("The WORD vector must retain its exact stored count/capacity.");
                words.Add(node.Word);
            }
            var cursor=new LegacyPreparedPublishedGlyphCursor(source.Pool,words,RejectOpaqueScalar,token);
            LegacyPreparedPublishedGlyphRead Next()
            {
                //Every supported scalar writes its used U16. No-write kind3 is
                //refused inside the cursor BEFORE reading scratch, after advance.
                Span<byte> scratch=stackalloc byte[4];
                var read=cursor.Next(scratch,text=>source.LookupControl(text,token),token);
                Record(new("Read",read.WordOrdinal,read.ReferenceOrdinal,read.Reference,read.Label,read.HasReference?1:0),token);
                return read;
            }
            byte Kind(LegacyPreparedPublishedGlyphRead read)
            {
                if(unchecked((short)(read.Reference>>24))<0)return byte.MaxValue;
                byte kind=source.ReadKind(read.Reference,token);
                Record(new("Kind",read.WordOrdinal,read.ReferenceOrdinal,read.Reference,kind,0),token);return kind;
            }
            void Write(LegacyPreparedPublishedGlyphRead read,ushort value)
            {
                token.ThrowIfCancellationRequested();EnsureEventBudget();
                byte[] row=source.Pool.CopyRecord(read.Reference,token);ushort before=BinaryPrimitives.ReadUInt16LittleEndian(row.AsSpan(8));
                BinaryPrimitives.WriteUInt16LittleEndian(row.AsSpan(8),value);source.Pool.ReplaceRecord(read.Reference,row,token);
                Record(new("Write",read.WordOrdinal,read.ReferenceOrdinal,read.Reference,value,before),token);
            }
            bool Ordinary(ushort label)=>label==labels.Apostrophe||label==labels.DoubleQuote;
            bool Control(ushort label)=>unchecked((ushort)(label+7))<4;
            ushort Open(ushort label)=>label==labels.Apostrophe?labels.OpenSingle:labels.OpenDouble;
            ushort Close(ushort label)=>label==labels.Apostrophe?labels.CloseSingle:labels.CloseDouble;

            var first=Next();var current=Next();var next=Next();
            bool carry=labels.LowDouble!=ushort.MaxValue&&first.Label==labels.LowDouble;
            byte firstKind=Kind(first);ushort previous=first.Label;
            if(unchecked((short)(first.Reference>>24))>=0&&firstKind==0&&Ordinary(first.Label)&&!Control(current.Label))
            {previous=LegacyPreparedContextualQuoteRules.Leading(first.Label,current.Label,0,firstKind,labels);Write(first,previous);}
            //Native frontend48 is fetched once, even if next already terminates.
            Record(new("Data",-1,-1,0,source.CategoryOwner.CategoryProfileOffset,0),token);
            while(next.Label!=ushort.MaxValue)
            {
                byte kind=Kind(current);
                if(unchecked((short)(current.Reference>>24))>=0&&kind==0&&Ordinary(current.Label))
                {
                    Record(new("Count",current.WordOrdinal,current.ReferenceOrdinal,current.Reference,source.Alphabet.LabelCount,0),token);
                    int category=-1;
                    if(previous<source.Alphabet.LabelCount)
                    {
                        category=source.ReadPreviousCategory(previous,token);
                        Record(new("Category",current.WordOrdinal,current.ReferenceOrdinal,current.Reference,category,previous),token);
                    }
                    var decision=LegacyPreparedContextualQuoteRules.Interior(previous,current.Label,next.Label,0,kind,
                        checked((ushort)source.Alphabet.LabelCount),category,carry,labels);
                    if(category==1||Control(previous)&&!Control(next.Label))Write(current,Open(current.Label));
                    else if(!Control(previous))
                    {
                        ushort closing=Close(current.Label);Write(current,closing);
                        if(carry&&closing==labels.CloseDouble)Write(current,labels.OpenDouble);
                    }
                    //The rule's carry includes the end-of-iteration LowDouble
                    //test. Native performs that test AFTER fetching the next scalar.
                    bool cleared=carry&&!Control(previous)&&category!=1&&Close(current.Label)==labels.CloseDouble;
                    if(cleared)carry=false;
                    current=current with{Label=decision.Label};
                }
                var fetched=Next();carry=carry||(labels.LowDouble!=ushort.MaxValue&&current.Label==labels.LowDouble);
                previous=current.Label;current=next;next=fetched;
            }
            byte lastKind=Kind(current);
            if(unchecked((short)(current.Reference>>24))>=0&&lastKind==0&&Ordinary(current.Label)&&!Control(previous))
            {
                ushort closing=Close(current.Label);Write(current,closing);
                ushort final=LegacyPreparedContextualQuoteRules.Trailing(previous,current.Label,0,lastKind,carry,labels);
                if(carry&&closing==labels.CloseDouble)Write(current,final);
            }
            token.ThrowIfCancellationRequested();IsComplete=true;
        }
        catch{IsStopped=true;throw;}
    }
    private static void RejectOpaqueScalar(LegacyNativeGlyphSymbolDescriptor descriptor)
    {
        if((descriptor.KindFlags&7)==3&&((descriptor.SymbolOrRunLength&0x7ffc)==0||(descriptor.SymbolOrRunLength&3)==1))
            throw new NotSupportedException("Kind3 no-write scalar requires actual native pointer/stack residue ownership.");
    }
    private void EnsureEventBudget()
    {if(events.Count==1_048_576)throw new NotSupportedException("Portable rewrite event budget exceeded.");}
    private void Record(LegacyPreparedGlyphRewriteEvent value,CancellationToken token)
    {EnsureEventBudget();events.Add(value);observe?.Invoke(value);token.ThrowIfCancellationRequested();}
}
