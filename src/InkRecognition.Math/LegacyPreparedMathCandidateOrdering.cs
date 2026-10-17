using System.Buffers.Binary;

namespace InkRecognition.Math;

/// <summary>Document036810 ordering of complete168-byte candidate records.
/// This prepared numerical kernel does not construct a native beam, resolve
/// pointer fields, evaluate networks or replace the application's decoder.</summary>
public static class LegacyPreparedMathCandidateOrdering
{
    public const int RecordSize=168;

    /// <summary>Copies input; preserves every record byte, including NaN
    /// payloads. Native ascending strict comparisons use stable insertion for
    /// at most20 records and midpoint Hoare partitioning for larger ranges.</summary>
    public static byte[] Sort(ReadOnlySpan<byte> records,CancellationToken token=default)
    {
        token.ThrowIfCancellationRequested();
        if(records.Length%RecordSize!=0||records.Length/RecordSize>65536)
            throw new ArgumentException("Complete168-byte records within the portable65536-candidate bound required.");
        byte[] data=records.ToArray();int count=data.Length/RecordSize;
        if(count<2)return data;
        var pending=new Stack<(int First,int Last)>();pending.Push((0,count-1));
        byte[] temporary=new byte[RecordSize];int work=0;
        void Check()
        {
            token.ThrowIfCancellationRequested();
            if(++work>64_000_000)throw new NotSupportedException("Portable candidate-ordering work budget exceeded.");
        }
        float Cost(int ordinal)=>BinaryPrimitives.ReadSingleLittleEndian(data.AsSpan(ordinal*RecordSize));
        while(pending.TryPop(out var range))
        {
            Check();int first=range.First,last=range.Last;
            if(last-first<20) //Native byte distance <0xD20, with an inclusive last pointer.
            {
                for(int i=first+1;i<=last;i++)
                {
                    Check();data.AsSpan(i*RecordSize,RecordSize).CopyTo(temporary);
                    float cost=BinaryPrimitives.ReadSingleLittleEndian(temporary);
                    int j=i-1;
                    while(j>=first&&cost<Cost(j))
                    {
                        Check();data.AsSpan(j*RecordSize,RecordSize).CopyTo(data.AsSpan((j+1)*RecordSize));j--;
                    }
                    temporary.CopyTo(data.AsSpan((j+1)*RecordSize));
                }
            }
            else
            {
                float pivot=Cost(first+(last-first)/2);int left=first,right=last;
                do
                {
                    Check();
                    while(Cost(left)<pivot){Check();left++;}
                    while(pivot<Cost(right)){Check();right--;}
                    if(right<left)break;
                    data.AsSpan(left*RecordSize,RecordSize).CopyTo(temporary);
                    data.AsSpan(right*RecordSize,RecordSize).CopyTo(data.AsSpan(left*RecordSize));
                    temporary.CopyTo(data.AsSpan(right*RecordSize));left++;right--;
                }while(left<=right);
                //Explicit stack preserves native left-first execution while
                //avoiding a process stack overflow on adversarial pivot input.
                if(left<last)pending.Push((left,last));
                if(first<right)pending.Push((first,right));
            }
        }
        token.ThrowIfCancellationRequested();return data;
    }
}
