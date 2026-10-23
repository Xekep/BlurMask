using System.Runtime.CompilerServices;
namespace InkRecognition;

//Conservative library work reservations, not native allocation limits or RSS.
internal sealed class LegacyFullEnglishFirstSourceRecordBudget
{
    internal const int T=4096,H=12800,W=8192,P=8192,C=256,F=65536,V=1_048_576,K=8192;
    internal const long PeakLimit=64L*1024*1024,CopyLimit=128L*1024*1024,VisitLimit=128L*1024*1024;
    internal long Peak{get;} internal long Copy{get;} internal long Visits{get;}
    internal LegacyFullEnglishFirstSourceRecordBudget(long peak,long copy,long visits,CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        if(IntPtr.Size>8||RuntimeHelpers.IsReferenceOrContainsReferences<LegacyFullEnglishFirstSourceRecordEntry>()||
           Unsafe.SizeOf<LegacyFullEnglishFirstSourceRecordEntry>()>64||RuntimeHelpers.IsReferenceOrContainsReferences<InkPoint>()||
           Unsafe.SizeOf<InkPoint>()>8||RuntimeHelpers.IsReferenceOrContainsReferences<LegacySourcePointIdentity>()||
           Unsafe.SizeOf<LegacySourcePointIdentity>()>24||RuntimeHelpers.IsReferenceOrContainsReferences<LegacyCursiveCategoryPrototype>()||
           Unsafe.SizeOf<LegacyCursiveCategoryPrototype>()>16||RuntimeHelpers.IsReferenceOrContainsReferences<LegacyCursiveCompositeCategory>()||
           Unsafe.SizeOf<LegacyCursiveCompositeCategory>()>8)
            throw new NotSupportedException("B8 reference-free slot/point/chronology size envelope required.");
        //Reserve ALL possible ordinary source rows, not enrichment-only append
        //attempts. A Plan==null still produces720. No successful-only refund.
        Peak=checked(8L*1024*1024+2048L*T+128L*F+128L*H+32L*P+512L*C+512L*W);
        Copy=checked(16L*1024*1024+4096L*T+256L*H+256L*F+64L*P+4096L*C+1024L*W);
        Visits=checked(4L*1024*1024+(long)T*P+8L*V+256L*H+512L*T+32L*F+128L*K+256L*C);
        if(peak<Peak||peak>PeakLimit||copy<Copy||copy>CopyLimit||visits<Visits||visits>VisitLimit)
            throw new ArgumentException("Whole B8 logical peak/copy/visit reservation required before large snapshots.");
    }
    internal static void Ranges(IReadOnlyList<LegacyFirstResultSourceRange> ranges,IReadOnlyList<int> map,
        IReadOnlyList<InkStroke> parent,ref int totalFragments,ref int totalVisits,CancellationToken token)
    {
        if(ranges.Count is<1 or>4096||map.Count is<1 or>256)throw new ArgumentException("Actual bounded child ranges/map required.");
        int fragments=0;
        foreach(var range in ranges)
        {
            token.ThrowIfCancellationRequested();var a=range.First;var b=range.Last;
            if(a.First!=b.First||a.Second<0||b.Second<a.Second||b.Second>=map.Count||a.Third<0||b.Third<0||
               a.Third>=parent[map[a.Second]].Points.Count||b.Third>=parent[map[b.Second]].Points.Count||a.Second==b.Second&&b.Third<a.Third)
                throw new ArgumentException("Actual inclusive child source keys required.");
            for(int s=a.Second;s<=b.Second;s++)
            {
                token.ThrowIfCancellationRequested();int n=(s==b.Second?b.Third+1:parent[map[s]].Points.Count)-(s==a.Second?a.Third:0);
                if(n>V-totalVisits)throw new NotSupportedException("Whole B8 repeated geometry point-visit cap.");totalVisits+=n;
                if(s==b.Second||map[s+1]!=map[s]+1)
                {if(++fragments>4096||++totalFragments>F)throw new NotSupportedException("Whole B8 projected fragment cap.");}
            }
        }
    }
}
