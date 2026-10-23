using System.Runtime.CompilerServices;
namespace InkRecognition;

//Logical allocation/copy/work admission; neither process RSS nor SDK limits.
internal sealed class LegacyFullEnglishFirstGlyphEmissionBudget
{
    internal const long PeakLimit=64L<<20,WorkLimit=128L<<20,VisitLimit=128L<<20;
    internal long Peak{get;} internal long Work{get;} internal long Visits{get;}
    internal int OriginalRows{get;} internal int AttemptedAppends{get;} internal int Owners{get;}
    internal LegacyFullEnglishFirstGlyphEmissionBudget(LegacyFullEnglishFirstPlainText owner,
        LegacyFullEnglishFirstPlainChild[] children,long peak,long work,long visits,CancellationToken token)
    {
        if(IntPtr.Size>8||RuntimeHelpers.IsReferenceOrContainsReferences<LegacyFullEnglishFirstGlyphEmissionPlan>()||
           Unsafe.SizeOf<LegacyFullEnglishFirstGlyphEmissionPlan>()>64||
           RuntimeHelpers.IsReferenceOrContainsReferences<LegacyFullEnglishFirstGlyphEmissionQuery>()||
           Unsafe.SizeOf<LegacyFullEnglishFirstGlyphEmissionQuery>()>40||Unsafe.SizeOf<LegacyFirstResultSourceRange>()>24||
           RuntimeHelpers.IsReferenceOrContainsReferences<LegacyFullEnglishFirstGlyphEmissionRange>()||Unsafe.SizeOf<LegacyFullEnglishFirstGlyphEmissionRange>()>48)
            throw new NotSupportedException("Bounded reference-free B7 plan required.");
        if(children.Length is <1 or >256||owner.ReservedRichAttempts is <0 or >8704||owner.ReservedWordAttempts is <0 or >8192)
            throw new NotSupportedException("Bounded complete B6 parent required.");
        long c=children.Length,h=owner.ReservedRichAttempts,w=owner.ReservedWordAttempts,n=0,g=0,k=0,t=0,u=0,actualWords=0;
        foreach(var child in children)
        {
            token.ThrowIfCancellationRequested();var graph=child.Factory.Graph;
            actualWords=checked(actualWords+child.NativeWords.Count);if(actualWords>w)throw new InvalidDataException("Actual B6 WORD count exceeds its reservation.");
            n=checked(n+graph.NodeCount);g=checked(g+child.Factory.Initializer.ActualCache.Owner.CopyLiveGroupIds().Length);
            if(n>4096||g>4096)throw new NotSupportedException("B7 whole-parent graph cap4096.");
            var touched=new HashSet<ulong>();
            foreach(var word in child.NativeWords)
            {
                token.ThrowIfCancellationRequested();var refs=word.Published.Word.FlatReferences;
                k=checked(k+refs.Count);if(k>8192)throw new NotSupportedException("B7 all-reference cap8192.");
                foreach(ulong reference in refs)
                {
                    token.ThrowIfCancellationRequested();var key=LegacyFirstResultGlyphKey.Decode(reference);if(key.Arc<0)continue;
                    if(++t>4096||(reference&(1UL<<56))!=0||key.Node<0||key.Alternative<0)
                        throw new NotSupportedException("B7 ordinary positive key cap/bit56 required.");
                    var row=graph.GetNode(key.Node);if(key.Arc>=row.Count)throw new InvalidDataException("Owned B7 arc required.");
                    var arc=row[key.Arc];long delta=(long)(arc.EndContextId>>7)-(arc.StartContextId>>7);
                    if(delta is <0 or >7)throw new NotSupportedException("B7 source path at most8 required.");
                    touched.Add(LegacyNativeGlyphRecordOwner.NormalizeOwnerKey(reference));
                }
            }
            u=checked(u+touched.Count);if(u>4096)throw new NotSupportedException("B7 normal owner cap4096.");
        }
        long e=t,r=checked(8*u);
        Peak=checked((8L<<20)+512*h+512*e+512*u+96*r+512*n+1024*g+128*k+4096*c+128*w);
        Work=checked((16L<<20)+2048*h+4096*e+6144*u+512*r+2048*n+4096*g+1024*k+8192*c+512*w);
        //Two separately executed count/fill passes; each existing DFS is capped1,048,576.
        Visits=checked((4L<<20)+2097152*u+4096*(h+e)+256*r+128*n+256*g+128*k+64*w+4096*c);
        if(peak<0||peak>PeakLimit||work<0||work>WorkLimit||visits<0||visits>VisitLimit||Peak>peak||Work>work||Visits>visits)
            throw new ArgumentException("B7 whole-parent logical preflight refused before claim.");
        OriginalRows=checked((int)h);AttemptedAppends=checked((int)e);Owners=checked((int)u);
    }
}
