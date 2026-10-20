using System.Buffers.Binary;
namespace InkRecognition;

public sealed class LegacyEnglish119ParentRowOrigin
{
    public LegacyEnglish119ParentChild Child{get;}
    public LegacyEnglish119PlainTextResult Text{get;}
    public int WordOrdinal{get;}
    public int ReferenceOrdinal{get;}
    public ulong Reference{get;}
    public bool SoleFlatOverride{get;}
    private readonly LegacyFirstResultSourceRange[] ranges;
    internal LegacyEnglish119ParentRowOrigin(LegacyEnglish119ParentChild child,LegacyEnglish119PlainTextResult text,int word,int ordinal,ulong reference,
        LegacyFirstResultSourceRange[] actual,bool flat)
    {Child=child;Text=text;WordOrdinal=word;ReferenceOrdinal=ordinal;Reference=reference;ranges=actual.ToArray();SoleFlatOverride=flat;}
    public LegacyFirstResultSourceRange[] CopyChildRanges()=>ranges.ToArray();
}

public sealed partial class LegacyEnglish119ParentSubmission
{
    private readonly List<string> trace=[];
    public string[] CopyTrace(){lock(gate)return trace.ToArray();}
    public LegacyEnglish119ParentSubmission Publish(IReadOnlyList<LegacyEnglish119PlainTextResult> actualChildren,CancellationToken token=default)
        =>PublishCore(actualChildren,null,token);
    //Explicitly prepared scheduling control; absent on the actual route.
    internal LegacyEnglish119ParentSubmission PublishPrepared(IReadOnlyList<LegacyEnglish119PlainTextResult> actualChildren,Action<string> checkpoint,CancellationToken token=default)
        =>PublishCore(actualChildren,checkpoint,token);
    private LegacyEnglish119ParentSubmission PublishCore(IReadOnlyList<LegacyEnglish119PlainTextResult> actualChildren,Action<string>? checkpoint,CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(actualChildren);lock(gate)
        {
            token.ThrowIfCancellationRequested();if(entered)throw new InvalidOperationException("Entered EN parent publication cannot restart.");
            if(actualChildren.Count!=children.Length)throw new ArgumentException("One actual completed result per retained child required.");
            var entries=new List<ParentEntry>();int refs=0,ranges=0,visits=0;
            //Complete all identity/count/range/geometry-work bounds before
            //source publication. Caller does not supply maps, labels or scales.
            for(int c=0;c<children.Length;c++)
            {
                token.ThrowIfCancellationRequested();var child=children[c];var text=actualChildren[c]??throw new ArgumentException("Missing actual child result.");
                var factory=text.Factory;var prep=child.Submission.Preparation;var candidate=factory.Candidates;var sourceRanges=candidate.SourceRanges;var policy=factory.Policy;
                if(!ReferenceEquals(factory.Preparation,prep)||!ReferenceEquals(candidate.Preparation,prep)||
                    !ReferenceEquals(prep.Model,Model)||!ReferenceEquals(policy.Owner.Model,Model)||
                    !ReferenceEquals(prep.Source,Model.Source)||!ReferenceEquals(candidate.Sampler.Source,Model.Source)||
                    !ReferenceEquals(policy.SourceRanges,sourceRanges)||!ReferenceEquals(factory.Graph,sourceRanges.Graph)||
                    !ReferenceEquals(sourceRanges.OriginalSource,prep.OriginalSource)||!ReferenceEquals(factory.Categories,policy.Categories)||
                    candidate.Sampler.Child.Version!=0x108||!ReferenceEquals(child.Owner,this))
                    throw new ArgumentException("Exact own pre-NN EN child/model/preparation/graph/richpool/categories108 required.");
                var next=text.Storage.WordPublisher.Head;int before=entries.Count,wordOrdinal=0;
                var owners=new Dictionary<ulong,LegacyNativeGlyphRecordNode>();
                foreach(var word in text.NativeWords)
                {
                    token.ThrowIfCancellationRequested();
                    if(!ReferenceEquals(word.Published,next)||!ReferenceEquals(word.Published.Word.Pool,text.Storage.GlyphPool))throw new ArgumentException("Actual complete ordered child WORD/pool required.");
                    next=next.Next;int n=word.Published.Word.GetReferenceCount(token);
                    if(n<0||n>4096-refs)throw new ArgumentException("Whole parent4096 reference budget exceeded.");refs+=n;
                    var actualRefs=word.Published.Word.FlatReferences.ToArray();if(actualRefs.Length!=n)throw new ArgumentException("Child WORD changed during preflight.");
                    for(int ordinal=0;ordinal<actualRefs.Length;ordinal++)
                    {
                        token.ThrowIfCancellationRequested();ulong reference=actualRefs[ordinal];var key=LegacyFirstResultGlyphKey.Decode(reference);if(key.Arc<0)continue;
                        ulong ownerKey=LegacyNativeGlyphRecordOwner.NormalizeOwnerKey(reference);
                        if(!owners.TryGetValue(ownerKey,out var owner)){owner=text.Storage.GlyphPool.CopyOwnerNode(ownerKey,token);owners.Add(ownerKey,owner);}
                        if(key.Alternative<0||key.Alternative>=owner.Records.Length/40)throw new ArgumentException("Actual signed child glyph alternative required.");
                        var resolved=sourceRanges.ResolveCandidate(key.Node,key.Arc,token);
                        PreflightParentRanges(resolved,child.Map,current.Strokes,ref ranges,ref visits,token);
                        entries.Add(new(child,text,wordOrdinal,ordinal,reference,owner,key.Alternative,resolved,false));
                    }
                    wordOrdinal++;
                }
                if(next is not null)throw new ArgumentException("Truncated child WORD list.");
                if(entries.Count==before)throw new NotSupportedException("Zero-row child result requires the separately excluded native previous-row fallback lifecycle.");
                if(child.IsSoleFlat&&Model.Source.Alphabet.TryGetLabel("_",out ushort label)&&label!=ushort.MaxValue)
                    entries[^1]=entries[^1] with{Flat=true};
            }
            token.ThrowIfCancellationRequested();entered=true;
            try
            {
                trace.Add("enter");checkpoint?.Invoke("enter");token.ThrowIfCancellationRequested();int cursor=0;
                foreach(var child in children)
                {
                    token.ThrowIfCancellationRequested();trace.Add("child-begin:"+child.DescriptorOrdinal);checkpoint?.Invoke(trace[^1]);
                    int begin=prefix.Count;var descriptor=publishedDescriptors[child.DescriptorOrdinal].ToArray();
                    BinaryPrimitives.WriteInt32LittleEndian(descriptor.AsSpan(52),begin);BinaryPrimitives.WriteInt32LittleEndian(descriptor.AsSpan(56),begin);
                    publishedDescriptors[child.DescriptorOrdinal]=descriptor;trace.Add("interval-begin:"+child.DescriptorOrdinal);
                    while(cursor<entries.Count&&ReferenceEquals(entries[cursor].Child,child))
                    {
                        token.ThrowIfCancellationRequested();var e=entries[cursor++];ReadOnlyMemory<byte> alternatives=e.Owner.Records;int selected=e.Selected;
                        if(e.Flat)
                        {
                            if(!Model.Source.Alphabet.TryGetLabel("_",out ushort underscore))throw new InvalidDataException("Retained actual AL changed.");
                            byte[] row=new byte[40];BinaryPrimitives.WriteUInt16LittleEndian(row.AsSpan(8),underscore);
                            BinaryPrimitives.WriteSingleLittleEndian(row.AsSpan(12),1);BinaryPrimitives.WriteSingleLittleEndian(row.AsSpan(16),1);
                            alternatives=row;selected=0;
                        }
                        var row720=LegacyFirstResultSourceRecord.Create(alternatives.Span,selected,e.Ranges,child.Map,current.Strokes,e.Text.Factory.Categories,Model.SourceScoreExponent,token);
                        prefix.Add(row720);origins.Add(new(child,e.Text,e.Word,e.Ordinal,e.Reference,e.Ranges,e.Flat));trace.Add("row:"+(prefix.Count-1));checkpoint?.Invoke(trace[^1]);
                    }
                    publishedDescriptors[child.DescriptorOrdinal]=LegacyFirstResultSourceRecord.PublishDescriptorInterval(descriptor,begin,prefix.Count-begin,token);
                    trace.Add("child-close:"+child.DescriptorOrdinal);checkpoint?.Invoke(trace[^1]);
                }
                token.ThrowIfCancellationRequested();completed=true;trace.Add("complete");return this;
            }
            catch{faulted=true;trace.Add("fault");throw;}
        }
    }
    //Count projected fragments and actual geometry visits across ALL children.
    //A discontinuous parent map splits spans; never remap a child720 bbox.
    internal static void PreflightParentRanges(IReadOnlyList<LegacyFirstResultSourceRange> resolved,IReadOnlyList<int> map,
        IReadOnlyList<InkStroke> parent,ref int ranges,ref int visits,CancellationToken token)
    {
        token.ThrowIfCancellationRequested();if(map.Count is<1 or>256||resolved.Count is<1 or>4096)throw new ArgumentException("Bounded actual child source intervals required.");
        for(int i=0;i<map.Count;i++){token.ThrowIfCancellationRequested();if((uint)map[i]>=(uint)parent.Count||i>0&&map[i]<=map[i-1])throw new ArgumentException("Strict increasing retained parent map required.");}
        int fragments=0;
        foreach(var range in resolved)
        {
            token.ThrowIfCancellationRequested();var a=range.First;var b=range.Last;
            if(a.First!=b.First||a.Second<0||b.Second<a.Second||b.Second>=map.Count||a.Third<0||b.Third<0||
                a.Third>=parent[map[a.Second]].Points.Count||b.Third>=parent[map[b.Second]].Points.Count||a.Second==b.Second&&b.Third<a.Third)
                throw new ArgumentException("Exact pre-dedup child source keys required.");
            for(int s=a.Second;s<=b.Second;s++)
            {
                token.ThrowIfCancellationRequested();int n=(s==b.Second?b.Third+1:parent[map[s]].Points.Count)-(s==a.Second?a.Third:0);
                if(n>1_048_576-visits)throw new ArgumentException("Whole-parent geometry visit budget exceeded.");visits+=n;
                if(s==b.Second||map[s+1]!=map[s]+1)
                {if(++fragments>4096||++ranges>65_536)throw new ArgumentException("Whole-parent projected range budget exceeded.");}
            }
        }
    }
    private sealed record ParentEntry(LegacyEnglish119ParentChild Child,LegacyEnglish119PlainTextResult Text,int Word,int Ordinal,ulong Reference,
        LegacyNativeGlyphRecordNode Owner,short Selected,LegacyFirstResultSourceRange[] Ranges,bool Flat);
}

