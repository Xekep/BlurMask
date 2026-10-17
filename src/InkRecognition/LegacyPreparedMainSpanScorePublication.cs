using System.Buffers.Binary;
using System.Runtime.CompilerServices;
[assembly: InternalsVisibleTo("LegacyMainSpanScores.Probe")]
namespace InkRecognition;

/// <summary>Prepared275.V08/140B90 score finalizer after13D360. It retains
/// complete WORD payloads and uses a private native binary-insertion index.
/// Float channels are numerical weights, not handwriting acceptance confidence.</summary>
internal static class LegacyPreparedMainSpanScorePublication
{
    internal sealed record Result(byte[][] Words,int[] ScoreOrder);
    internal static Result Normalize(IReadOnlyList<byte[]> actualWords,CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(actualWords);token.ThrowIfCancellationRequested();
        if(actualWords.Count>4095)throw new ArgumentOutOfRangeException(nameof(actualWords));
        int count=actualWords.Count;var words=new byte[count][];var order=new int[count];
        for(int i=0;i<count;i++)
        {
            token.ThrowIfCancellationRequested();
            if(actualWords[i] is null||actualWords[i].Length!=120)throw new ArgumentException("Complete120-byte WORD payloads required.");
            words[i]=actualWords[i].ToArray();
            if(!float.IsFinite(Read(words[i],16))||!float.IsFinite(Read(words[i],28)))
                throw new NotSupportedException("Finite native WORD score/log-rank inputs required.");
            int low=0,high=i,mid=i/2;
            while(low<high)
            {
                float old=Read(words[order[mid]],16),current=Read(words[i],16);
                if(current<=old){if(old<=current)break;low=mid+1;}else high=mid;
                mid=(high+low)/2;
            }
            for(int j=i;j>mid;j--)order[j]=order[j-1];order[mid]=i;
        }
        if(count==0)return new(words,order);
        for(int i=count-1;i>=0;i--)
        {
            token.ThrowIfCancellationRequested();float difference=Read(words[order[i]],16)-Read(words[order[0]],16);
            float mass=difference< -75f?0f:difference>75f?3.73e32f:(float)Math.Exp((double)difference);
            Write(words[order[i]],16,mass);
        }
        if(count==1){Write(words[order[0]],16,1f);Write(words[order[0]],20,1f);return new(words,order);}
        float priorRaw=Read(words[order[0]],16);int kept=0;
        if(priorRaw>0)
        {
            kept=1;Write(words[order[0]],16,priorRaw/(priorRaw+Read(words[order[1]],16)));
            for(int i=1;i<count;i++)
            {
                token.ThrowIfCancellationRequested();float currentRaw=Read(words[order[i]],16);if(currentRaw==0f)break;
                Write(words[order[i]],16,currentRaw*Read(words[order[i-1]],16)/priorRaw);
                priorRaw=currentRaw;kept++;
            }
        }
        for(int i=kept;i<count;i++)Write(words[order[i]],16,0f);
        float minimum=float.MaxValue;
        foreach(int index in order){float value=Read(words[index],28);if(value<=minimum)minimum=value;}
        foreach(int index in order)
        {
            token.ThrowIfCancellationRequested();float difference=Read(words[index],28)-minimum;
            Write(words[index],20,MathF.Exp(-difference));
        }
        token.ThrowIfCancellationRequested();return new(words,order);
    }
    private static float Read(ReadOnlySpan<byte> word,int at)=>BinaryPrimitives.ReadSingleLittleEndian(word[at..]);
    private static void Write(Span<byte> word,int at,float value)=>BinaryPrimitives.WriteSingleLittleEndian(word[at..],value);
}
