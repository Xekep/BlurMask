using System.Buffers.Binary;
using System.Runtime.CompilerServices;
[assembly: InternalsVisibleTo("LegacyMainWordRanking.Probe")]
namespace InkRecognition;

/// <summary>Prepared numeric13D360 stage. The caller must supply actual120-byte
/// WORD payloads, selected279 ordinals and the same-pool276.V60 comparison.
/// This does not export sources, mutate a275 owner or complete TOP.V68.</summary>
internal static class LegacyPreparedMainWordRanking
{
    internal sealed record Result(byte[][] Words,int[] OldToNew,int[] Released,int QuotaAfter,
        byte[][] RankingByOriginalIndex,int[] OutputSourceOrdinals,int[] ReleasedSourceOrdinals);
    private sealed class Row
    {
        internal int Original,Group=unchecked((int)0x80000001);
        internal float Cost;
        internal bool First,Selected,Special,Flags,Keep,Removed;
        internal byte[] CopyBytes()
        {
            var b=new byte[20];BinaryPrimitives.WriteInt32LittleEndian(b,Original);
            BinaryPrimitives.WriteInt32LittleEndian(b.AsSpan(4),Group);
            BinaryPrimitives.WriteSingleLittleEndian(b.AsSpan(8),Cost);
            b[12]=(byte)(First?1:0);b[13]=(byte)(Selected?1:0);b[14]=(byte)(Special?1:0);
            b[15]=(byte)(Flags?1:0);b[16]=(byte)(Keep?1:0);b[17]=(byte)(Removed?1:0);return b;
        }
    }
    internal static int InitialQuota(ushort property18,ushort property16,bool specialMode)
        =>Math.Max(property18==65535?2:property18,property16==65535?2:property16)+(specialMode?1:0);

    internal static Result Rank(IReadOnlyList<byte[]> actualWords,ReadOnlySpan<int> selectedInTraversalOrder,
        int carriedQuota,bool specialMode,Func<byte[],byte[],bool> samePoolWordEquals,CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(actualWords);ArgumentNullException.ThrowIfNull(samePoolWordEquals);
        token.ThrowIfCancellationRequested();
        //A span visited by13D360 has at least one actual279 node. Refuse the
        //native out-of-domain zero-count dereference instead of fabricating it.
        if(actualWords.Count is <1 or >4095||selectedInTraversalOrder.IsEmpty||carriedQuota<0)
            throw new ArgumentException("Bounded actual span WORDs and nonempty279 selection required.");
        int n=actualWords.Count;var words=new byte[n][];var rows=new Row[n];
        var payloadSources=Enumerable.Range(0,n).ToArray();
        for(int i=0;i<n;i++)
        {
            if(actualWords[i] is null||actualWords[i].Length!=120)throw new ArgumentException("Full120-byte payload required.");
            words[i]=actualWords[i].ToArray();float cost=BinaryPrimitives.ReadSingleLittleEndian(words[i].AsSpan(16));
            if(!float.IsFinite(cost)||cost==-float.MaxValue)
                throw new NotSupportedException("Finite unsentinelized native WORD scores required.");
            ushort flags=BinaryPrimitives.ReadUInt16LittleEndian(words[i].AsSpan(50));
            rows[i]=new(){Original=i,Cost=cost,Special=specialMode&&(flags&0x40)!=0&&
                BinaryPrimitives.ReadUInt16LittleEndian(words[i].AsSpan(48))==65534};
        }
        for(int i=0;i<selectedInTraversalOrder.Length;i++)
        {
            int selected=selectedInTraversalOrder[i];if((uint)selected>=n)throw new ArgumentException("Selected WORD escaped its span.");
            rows[selected].Selected=true;if(i==0)rows[selected].First=true;
        }
        int group=999;
        for(int i=0;i<n;i++)
        {
            token.ThrowIfCancellationRequested();var row=rows[i];if(row.Removed)continue;
            row.Group=++group;row.Flags=(BinaryPrimitives.ReadUInt16LittleEndian(words[i].AsSpan(50))&0x300)!=0;
            var duplicates=new List<int>();
            for(int j=i+1;j<n;j++)
                if(!rows[j].Removed&&BinaryPrimitives.ReadInt32LittleEndian(words[i].AsSpan(60))==
                    BinaryPrimitives.ReadInt32LittleEndian(words[j].AsSpan(60))&&
                    samePoolWordEquals(words[i].ToArray(),words[j].ToArray())&&row.Special==rows[j].Special)
                {rows[j].Group=row.Group;duplicates.Add(j);}
            bool selected=row.Selected,first=row.First;int best=i;float bestCost=row.Cost;
            foreach(int j in duplicates)
            {
                selected|=rows[j].Selected;first|=rows[j].First;
                //Strict native comparison keeps the earliest payload on a tie.
                if(bestCost<rows[j].Cost){bestCost=rows[j].Cost;best=j;}
            }
            if(best!=i)
            {
                (words[i],words[best])=(words[best],words[i]);
                (payloadSources[i],payloadSources[best])=(payloadSources[best],payloadSources[i]);row.Cost=bestCost;
            }
            row.First=first;
            foreach(int j in duplicates){rows[j].Removed=true;rows[j].Cost=-float.MaxValue;}
            //The merged selected bit protects the representative; byte+D is
            //deliberately NOT merged and still counts original selected rows.
            row.Keep=selected||row.Flags;
        }
        if(specialMode){var firstSpecial=Array.Find(rows,r=>r.Special);if(firstSpecial is not null)firstSpecial.Keep=true;}
        int protectedCount=rows.Count(r=>r.Keep&&!r.Removed),alive=n-rows.Count(r=>r.Removed);
        int quota=checked(carriedQuota+rows.Count(r=>r.Flags));
        //13E1D1 counts byte+F, not byte+D: protected flag categories accumulate
        //across span owners independently of the number of selected279 nodes.
        if(quota<alive)
        {
            int prefix=0;
            for(int i=0;i<n;i++)if(rows[i].Keep)
            {if(i!=prefix)(rows[i],rows[prefix])=(rows[prefix],rows[i]);if(++prefix>=protectedCount)break;}
            SortCost(rows,protectedCount,n-1);alive=protectedCount;
            for(int i=protectedCount;i<n;i++)if(!rows[i].Removed)
            {
                if(alive>=quota){rows[i].Removed=true;rows[i].Cost=-float.MaxValue;}else alive++;
            }
        }
        int selectedFirst=Array.FindIndex(rows,r=>r.First);
        if(selectedFirst>0)(rows[0],rows[selectedFirst])=(rows[selectedFirst],rows[0]);
        int firstGroup=rows[0].Group;foreach(var row in rows)if(row.Group==firstGroup)row.Group=0;
        SortCost(rows,1,n-1);
        if(specialMode)
        {
            //Move special rows behind ordinary rows in the live tail, leaving
            //the selected first row untouched. Native13CE00 is a stable bubble.
            int last=1;while(last<n-1&&rows[last+1].Cost!=-float.MaxValue)last++;
            bool changed;
            do
            {
                changed=false;for(int i=2;i<=last;i++)if(rows[i-1].Special&&!rows[i].Special)
                {(rows[i-1],rows[i])=(rows[i],rows[i-1]);changed=true;}
            }while(changed);
        }
        for(int i=1;i<n&&rows[i].Cost!=-float.MaxValue;i++)
        {int old=rows[i].Group;foreach(var row in rows)if(row.Group==old)row.Group=i;}
        var output=new List<byte[]>();var outputSources=new List<int>();
        for(int i=0;i<n&&rows[i].Group!=-1&&rows[i].Cost!=-float.MaxValue;i++)
        {
            byte[] payload=words[rows[i].Original].ToArray();
            if(rows[i].Special)BinaryPrimitives.WriteSingleLittleEndian(payload.AsSpan(16),-float.MaxValue);
            output.Add(payload);outputSources.Add(payloadSources[rows[i].Original]);
        }
        if(output.Count!=alive)throw new InvalidDataException("Prepared ranking live count escaped its payload prefix.");
        //13CE60 restores original index order before patching279 WORD ordinals.
        SortOriginal(rows,0,n-1);
        token.ThrowIfCancellationRequested();
        return new(output.ToArray(),rows.Select(r=>r.Group).ToArray(),rows.Where(r=>r.Removed).Select(r=>r.Original).ToArray(),
            quota,rows.Select(r=>r.CopyBytes()).ToArray(),outputSources.ToArray(),
            rows.Where(r=>r.Removed).Select(r=>payloadSources[r.Original]).ToArray());
    }
    private static void SortCost(Row[] rows,int low,int high)
    {
        //Native midpoint Hoare partition is intentionally unstable for ties.
        //Replacing this by OrderBy changes the published alternative ordinals.
        while(low<high)
        {
            int i=low,j=high;float pivot=rows[low+(high-low)/2].Cost;
            do
            {
                while(pivot<rows[i].Cost)i++;while(rows[j].Cost<pivot)j--;
                if(j<i)break;(rows[i],rows[j])=(rows[j],rows[i]);i++;j--;
            }while(i<=j);
            SortCost(rows,low,j);low=i;
        }
    }
    private static void SortOriginal(Row[] rows,int low,int high)
    {
        while(low<high)
        {
            int i=low,j=high,pivot=rows[low+(high-low)/2].Original;
            do
            {
                while(rows[i].Original<pivot)i++;while(pivot<rows[j].Original)j--;
                if(j<i)break;(rows[i],rows[j])=(rows[j],rows[i]);i++;j--;
            }while(i<=j);
            SortOriginal(rows,low,j);low=i;
        }
    }
}
