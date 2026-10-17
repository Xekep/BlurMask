using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("LegacyRawInkRangeOperations.Probe")]
namespace InkRecognition;

/// <summary>Bounded native C21 range operations with exact unsigned source
/// positions and one NEW original document's real point-count/presence table.
/// These operations do not accept recognized labels or infer source IDs.</summary>
public sealed class LegacyOwnedRawInkRangeOperations
{
    public LegacyOwnedRawInkDocument Document {get;}
    private LegacyOwnedRawInkRangeOperations(LegacyOwnedRawInkDocument document){Document=document;}
    public static LegacyOwnedRawInkRangeOperations Prepare(LegacyOwnedRawInkDocument actualDocument)
    {ArgumentNullException.ThrowIfNull(actualDocument);return new(actualDocument);}
    private static ulong N(ulong value)=>((byte)(value>>8)==255)?value&~0xff00UL:value;
    private static LegacyNativeInkPositionRange R(ulong first,ulong last)=>new(new(first),new(last));
    private static void Bound(int count){if(count>65536)throw new NotSupportedException("Portable source-range budget exceeded.");}
    private static List<LegacyNativeInkPositionRange> Copy(ReadOnlySpan<LegacyNativeInkPositionRange> ranges)
    {Bound(ranges.Length);return new(ranges.ToArray());}
    //The same binary search is used by07FAF0/0804E0/080C60. No sorting of
    //caller storage is introduced; a FF fraction compares as zero, while
    //the stored endpoint bytes are retained or replaced by native branches.
    private static (int Index,bool Found) Find(List<LegacyNativeInkPositionRange> rows,
        ulong key,int first,int last,int before=0,int after=0)
    {
        while(first<last)
        {
            int middle=(first+last)>>1;var range=rows[middle];
            if(unchecked(N(range.First.Value)-(ulong)before)<=key)
            {if(key<=unchecked(N(range.Last.Value)+(ulong)after))return(middle,true);first=middle+1;}
            else last=middle;
        }
        return(first,false);
    }
    private static bool JoinObjects(ulong last,ulong first)
        =>unchecked((uint)(last>>32)+1)==(uint)(first>>32)&&
          (unchecked((uint)(short)last+0xfdu)&0xfffdu)==0&&((uint)first is 3 or 0xff03);
    private static void Remove(List<LegacyNativeInkPositionRange> rows,int start,int count)
    {if(count>0)rows.RemoveRange(start,count);}

    /// <summary>C21.VB0/080300: native append/merge, including consecutive
    /// whole-object joins and the mode4 no-op. Not a generic geometric union.</summary>
    public LegacyNativeInkPositionRange[] Union(ReadOnlySpan<LegacyNativeInkPositionRange> target,
        ReadOnlySpan<LegacyNativeInkPositionRange> additional,CancellationToken token=default)
    {
        token.ThrowIfCancellationRequested();var rows=Copy(target);Bound(additional.Length);
        if(additional.Length==0||(rows.Count!=0&&rows[0].First.Mode==4)||additional[0].First.Mode==4)return rows.ToArray();
        foreach(var range in additional){token.ThrowIfCancellationRequested();Append(rows,range);Bound(rows.Count);}
        return rows.ToArray();
    }
    private static void Append(List<LegacyNativeInkPositionRange> rows,LegacyNativeInkPositionRange added)
    {
        ulong a=added.First.Value,b=added.Last.Value;if(b<a)throw new ArgumentException("Native append interval is reversed.");
        var left=Find(rows,N(a),0,rows.Count,after:2);int i=left.Index;
        if(left.Found)
        {
            var right=Find(rows,N(b),i,rows.Count,before:2);int j=right.Index;
            if(right.Found)
            {rows[i]=R(rows[i].First.Value,rows[j].Last.Value);Remove(rows,i+1,j-i);return;}
            rows[i]=R(rows[i].First.Value,b);Remove(rows,i+1,j-i-1);
            if(i+1<rows.Count&&JoinObjects(b,rows[i+1].First.Value))
            {rows[i]=R(rows[i].First.Value,rows[i+1].Last.Value);rows.RemoveAt(i+1);}return;
        }
        var end=Find(rows,N(b),i,rows.Count,before:2);int k=end.Index;
        if(end.Found)
        {
            rows[k]=R(a,rows[k].Last.Value);Remove(rows,i,k-i);
            if(i>0&&JoinObjects(rows[i-1].Last.Value,a))
            {rows[i-1]=R(rows[i-1].First.Value,rows[i].Last.Value);rows.RemoveAt(i);}return;
        }
        if(i>0&&JoinObjects(rows[i-1].Last.Value,a))
        {
            if(k<rows.Count&&JoinObjects(b,rows[k].First.Value))
            {rows[i-1]=R(rows[i-1].First.Value,rows[k].Last.Value);Remove(rows,i,k-i+1);}
            else{rows[i-1]=R(rows[i-1].First.Value,b);Remove(rows,i,k-i);}return;
        }
        if(k<rows.Count&&JoinObjects(b,rows[k].First.Value))
        {rows[k]=R(a,rows[k].Last.Value);Remove(rows,i,k-i);return;}
        if(i==k)rows.Insert(i,added);
        else{rows[i]=added;Remove(rows,i+1,k-i-1);}
    }

    /// <summary>C21.VC0/080B40 with native single/multiple clipping paths.
    /// A mode4 first entry on either side empties the result.</summary>
    public LegacyNativeInkPositionRange[] Intersect(ReadOnlySpan<LegacyNativeInkPositionRange> target,
        ReadOnlySpan<LegacyNativeInkPositionRange> filters,CancellationToken token=default)
    {
        token.ThrowIfCancellationRequested();var rows=Copy(target);Bound(filters.Length);
        if(rows.Count==0||filters.Length==0||rows[0].First.Mode==4||filters[0].First.Mode==4)return [];
        var output=new List<LegacyNativeInkPositionRange>();
        foreach(var filter in filters)
        {
            token.ThrowIfCancellationRequested();ulong a=filter.First.Value,b=filter.Last.Value;
            if(b<a)throw new ArgumentException("Native intersection interval is reversed.");
            var first=Find(rows,N(a),0,rows.Count);var last=Find(rows,N(b),first.Index,rows.Count);
            int end=last.Index+(last.Found?1:0),offset=output.Count;
            for(int i=first.Index;i<end;i++)output.Add(rows[i]);
            if(end>first.Index)
            {
                if(first.Found)output[offset]=R(a,output[offset].Last.Value);
                if(last.Found)output[^1]=R(output[^1].First.Value,b);
            }
            Bound(output.Count);
        }
        return output.ToArray();
    }

    /// <summary>C21.VD0/0812D0 and080C60. Neighbor endpoints use this
    /// identical original document, including the native8000 missing count.</summary>
    public LegacyNativeInkPositionRange[] Subtract(ReadOnlySpan<LegacyNativeInkPositionRange> target,
        ReadOnlySpan<LegacyNativeInkPositionRange> removed,CancellationToken token=default)
    {
        token.ThrowIfCancellationRequested();var rows=Copy(target);Bound(removed.Length);
        foreach(var cut in removed)
        {
            token.ThrowIfCancellationRequested();if(rows.Count==0)break;
            ulong a=cut.First.Value,b=cut.Last.Value;if(b<a)throw new ArgumentException("Native subtraction interval is reversed.");
            if(cut.First.SourceIndex==cut.Last.SourceIndex&&cut.First.PointIndex==cut.Last.PointIndex&&
               (cut.First.Fraction==cut.Last.Fraction||cut.First.Fraction==255||cut.Last.Fraction==255)&&
               cut.First.Mode==4&&cut.Last.Mode==4)continue;
            ulong before=PreviousEnd(a),after=NextStart(b);
            var left=Find(rows,N(before),0,rows.Count);var right=Find(rows,N(after),left.Index,rows.Count);
            int i=left.Index,j=right.Index;
            if(left.Found)
            {
                if(right.Found&&i==j)
                {ulong oldLast=rows[i].Last.Value;rows[i]=R(rows[i].First.Value,before);rows.Insert(i+1,R(after,oldLast));}
                else
                {rows[i]=R(rows[i].First.Value,before);if(right.Found)rows[j]=R(after,rows[j].Last.Value);Remove(rows,i+1,j-i-1);}
            }
            else
            {if(right.Found)rows[j]=R(after,rows[j].Last.Value);Remove(rows,i,j-i);}
            Bound(rows.Count);
        }
        return rows.ToArray();
    }
    private int PointCount(uint id)=>id<(uint)Document.ObjectCount?Document.Strokes[(int)id].Points.Count:0x8000;
    private ulong PreviousEnd(ulong value)
    {
        uint id=(uint)(value>>32);ushort point=(ushort)(value>>16);byte fraction=(byte)(value>>8),mode=(byte)value;
        if(id!=0&&(uint)value is 3 or 0xff03)
            return((ulong)(id-1)<<32)|((ulong)unchecked((ushort)(PointCount(id-1)-1))<<16)|0xff05;
        if(mode==3)return(value&~0xffUL&~(fraction==255?0xff00UL:0UL))|1;
        ulong output=(value&~0xffUL)|5;
        if(id<(uint)Document.ObjectCount&&point==PointCount(id)-1)output=(output&~0xff00UL)|0xff00;
        return output;
    }
    private ulong NextStart(ulong value)
    {
        uint id=(uint)(value>>32);ushort point=(ushort)(value>>16);byte fraction=(byte)(value>>8),mode=(byte)value;
        if(fraction==255){id=unchecked(id+1);return((ulong)id<<32)|3UL|(PointCount(id)==1?0xff00UL:0);}
        if(mode==5)return(value&~0xffUL)|7;
        ulong output=(value&~0xffUL)|3;
        if(id<(uint)Document.ObjectCount&&point==PointCount(id)-1)output=(output&~0xff00UL)|0xff00;
        return output;
    }
    public static bool Equal(ReadOnlySpan<LegacyNativeInkPositionRange> first,ReadOnlySpan<LegacyNativeInkPositionRange> second)
        =>first.SequenceEqual(second);
    /// <summary>C21.V118/081CC0. Search comparisons alias FF fraction to0;
    /// its final last-endpoint containment comparison remains raw unsigned.</summary>
    public static bool Contains(ReadOnlySpan<LegacyNativeInkPositionRange> target,LegacyNativeInkPositionRange range)
    {
        if(range.First.Value>range.Last.Value)return false;var rows=Copy(target);
        var found=Find(rows,N(range.First.Value),0,rows.Count);
        return found.Found&&range.Last.Value<=rows[found.Index].Last.Value;
    }
}
