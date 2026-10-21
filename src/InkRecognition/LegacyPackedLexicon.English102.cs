using System.Buffers.Binary;
namespace InkRecognition;

public sealed partial class LegacyPackedLexicon
{
    //NEW exact English data path. The old public Load remains v100/unaligned.
    internal static LegacyPackedLexicon ReadEnglish102(ReadOnlySpan<byte> fullResource,
        CancellationToken token,Action<string>? checkpoint=null)
    {
        token.ThrowIfCancellationRequested();LegacyEnglishLkPackedCopyBudget.Preflight();
        if(fullResource.Length!=3300812)throw Invalid("Exact English lexical extent required.");
        const int begin=2572,end=1252412;var data=fullResource;
        if(U32(data,begin)!=0x101fe90||U32(data,begin+4)!=end-begin-8||U32(data,begin+8)!=0x2b4||U32(data,begin+12)!=0x01020000||
           U32(data,begin+16)!=0||U32(data,begin+20)!=666547||U32(data,begin+24)!=43)
            throw Invalid("Exact English2B4/v102 flags0/tree shape required.");
        int count=666547,cursor=2600;
        var labels=ReadEnglishPacked(data,ref cursor,count,8,token);
        if(cursor!=669160||data[cursor++]!=17)throw Invalid("Actual English child-offset width required.");
        var counts=ReadEnglishPacked(data,ref cursor,count,7,token);
        if(cursor!=1252408||U32(data,cursor)!=260074||cursor+4!=end)throw Invalid("Whole English dictionary consumption required.");
        token.ThrowIfCancellationRequested();checkpoint?.Invoke("dictionary:arrays");token.ThrowIfCancellationRequested();
        var offsets=new PackedArray(count+1,17,new byte[1416416]);var depth=new byte[count];
        uint next=1,terminals=0;int maximumDepth=0;
        for(int node=0;node<count;node++)
        {
            token.ThrowIfCancellationRequested();if((node&65535)==0){checkpoint?.Invoke("dictionary:traversal");token.ThrowIfCancellationRequested();}
            if(next<=node||next>count||next-node>=1u<<17)throw Invalid("Bounded English BFS child ownership required.");
            offsets.Set(node,next-(uint)node);uint children=counts.Get(node);if(children>count-next)throw Invalid("English child range outside tree.");
            uint previous=0;
            for(uint child=next;child<next+children;child++)
            {
                token.ThrowIfCancellationRequested();uint label=labels.Get((int)child)&127;
                if(label>=104||child!=next&&label<=previous||depth[node]>=43)throw Invalid("English lexical104 distinct siblings/depth required.");
                previous=label;depth[child]=(byte)(depth[node]+1);maximumDepth=Math.Max(maximumDepth,depth[child]);
            }
            if(node==0&&labels.Get(node)!=0)throw Invalid("English root sentinel required.");
            if((labels.Get(node)&128)!=0)terminals++;next+=children;
        }
        if(next!=count||terminals!=260074||maximumDepth!=43)throw Invalid("Complete actual English terminal/tree/depth owner required.");
        offsets.Set(count,0);token.ThrowIfCancellationRequested();checkpoint?.Invoke("dictionary:complete");token.ThrowIfCancellationRequested();
        return new(count,43,terminals,labels,offsets);
    }
    private static PackedArray ReadEnglishPacked(ReadOnlySpan<byte> data,ref int cursor,int count,int width,CancellationToken token)
    {
        token.ThrowIfCancellationRequested();if(cursor<0||cursor>1252412-4||U32(data,cursor)!=0x01000001)throw Invalid("Actual aligned packed-array version required.");
        cursor=checked((cursor+4+3)&~3);
        if(cursor>1252412-6||U32(data,cursor)!=count||data[cursor+4]!=width||data[cursor+5]!=1)throw Invalid("Actual packed count/width/metadata required.");
        cursor=checked((cursor+6+3)&~3);int bytes=checked((int)(((long)count*width+31)/32*4));
        if(bytes>1252412-cursor)throw Invalid("Whole packed storage required.");
        byte[] owned=data.Slice(cursor,bytes).ToArray();cursor+=bytes;token.ThrowIfCancellationRequested();return new(count,width,owned);
    }
}

/// <summary>Conservative complete logical load/copy envelope, not RSS.</summary>
internal static class LegacyEnglishLkPackedCopyBudget
{
    internal const long RetainedDictionaryBytes=666548L+1416416;
    internal static long Preflight(long maximumBytes=32L*1024*1024)
    {
        //Two whole-resource copy/hash work allowances, label/count/offset/depth
        //arrays, bounded alphabet metadata and object/list bookkeeping.
        long total=checked(2L*3300812+666548+583232+1416416+666547+256*1024+64*1024);
        if(maximumBytes<0||total>maximumBytes)throw new ArgumentException("Whole English packed data copy envelope exceeded.");return total;
    }
}
