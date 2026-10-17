using System.Buffers.Binary;

namespace InkRecognition.Math;

/// <summary>035F70 ordering of prepared48-byte candidates, distinct from
///036810 completed168-byte records. No stable tie or total NaN order.</summary>
internal static class LegacyPreparedMathRawCandidateOrdering
{
    internal static byte[] Sort(ReadOnlySpan<byte> records,CancellationToken token=default)
    {
        token.ThrowIfCancellationRequested();
        if(records.Length%48!=0||records.Length/48>32)
            throw new ArgumentException("Complete prepared48-byte candidates within32 slots required.");
        var data=records.ToArray();var temporary=new byte[48];
        float Cost(int slot)=>BinaryPrimitives.ReadSingleLittleEndian(data.AsSpan(slot*48));
        void Range(int first,int last)
        {
            do
            {
                int left=first,right=last;float pivot=Cost(first+(last-first)/2);
                do
                {
                    while(pivot>Cost(left))left++;
                    while(Cost(right)>pivot)right--;
                    if(left>right)break;
                    data.AsSpan(left*48,48).CopyTo(temporary);
                    data.AsSpan(right*48,48).CopyTo(data.AsSpan(left*48));
                    temporary.CopyTo(data.AsSpan(right*48));left++;right--;
                }while(left<=right);
                if(first<right)Range(first,right);
                first=left;
            }while(first<last);
        }
        if(data.Length!=0)Range(0,data.Length/48-1);
        return data;
    }
}
