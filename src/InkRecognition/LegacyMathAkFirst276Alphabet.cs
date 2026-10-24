using System.Runtime.CompilerServices;
[assembly: InternalsVisibleTo("LegacyMathAkFirstAlphabet.Probe")]

namespace InkRecognition;

public sealed partial class LegacyMathAkFeatureProfile
{
    //AL.V10/V28 readset only. No reverse/case/model/source/prepared authority.
    private readonly FirstAlphabetPrefix firstAlphabet;
    internal const int FirstAlphabetAdditionalPayloadBytes=5859+8+8;
    //Array zero + copy + two retained references + reservation counter stores.
    internal const int FirstAlphabetAdditionalCopyBytes=2*5859+16+48;
    internal const int FirstAlphabetAdditionalSourceUnits=2839083; //Derived by SOURCE audit.
    internal ushort FirstAlphabetCount=>firstAlphabet.Count;
    internal ushort ResolveFirstAlphabetSpecial(int ordinal)=>firstAlphabet.ResolveSpecial(ordinal);

    private struct FirstAlphabetReservation
    {
        private long bytes,copies,work;
        internal void Charge(long b,long c,long w)
        {
            if(b<0||c<0||w<0||b>FirstAlphabetAdditionalPayloadBytes-bytes||
                c>FirstAlphabetAdditionalCopyBytes-copies||w>FirstAlphabetAdditionalSourceUnits-work)
                throw new NotSupportedException("FIRST alphabet prefix reservation exhausted.");
            bytes+=b;copies+=c;work+=w;
        }
    }
    private sealed class FirstAlphabetPrefix
    {
        private readonly byte[] bytes;
        private FirstAlphabetPrefix(byte[] bytes){this.bytes=bytes;}
        private static ushort H(byte[] b,int at)=>(ushort)(b[at]|b[at+1]<<8);
        private static uint I(byte[] b,int at)=>(uint)(b[at]|b[at+1]<<8|b[at+2]<<16|b[at+3]<<24);
        private static void ValidateHashTable(byte[] b,int start,int count,int add,int limit)
        {
            for(int i=0;i<count;i++)
            {
                int value=H(b,start+2*i);
                if(value!=65535&&value+add>=limit)
                    throw new InvalidDataException("Actual FIRST alphabet hash table differs.");
            }
        }
        internal static FirstAlphabetPrefix ReadAuthenticated(byte[] capture,CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            var reservation=new FirstAlphabetReservation();
            reservation.Charge(FirstAlphabetAdditionalPayloadBytes,FirstAlphabetAdditionalCopyBytes,
                FirstAlphabetAdditionalSourceUnits);
            //Called ONLY after the existing full private snapshot SHA gate.
            //Exact static frame/table bounds precede new allocation/copy.
            if(capture.Length!=LegacyMathMixed107RawResource.ResourceBytes||
                I(capture,18)!=0x0101fe90||I(capture,22)!=7145||I(capture,26)!=0x2b||
                H(capture,32)!=0x104||H(capture,48)!=259||H(capture,50)!=16)
                throw new InvalidDataException("Actual FIRST alphabet frame differs.");
            ValidateHashTable(capture,52,16,31,128);
            ValidateHashTable(capture,84,128,63,1216);
            ValidateHashTable(capture,340,1216,1,249);
            int previousSpan=0;
            for(int i=0;i<249;i++)
            {
                int value=H(capture,2772+2*i);
                if(value<previousSpan||value>258||value-previousSpan>2)
                    throw new InvalidDataException("Actual FIRST alphabet spans differ.");
                previousSpan=value;
            }
            if(previousSpan!=258)throw new InvalidDataException("Actual FIRST alphabet terminal span differs.");
            uint previousOffset=0;
            for(int i=0;i<259;i++)
            {
                uint offset=I(capture,3270+8*i);
                if(offset<previousOffset||offset>551||i<258&&H(capture,3274+8*i)>=259)
                    throw new InvalidDataException("Actual FIRST alphabet string table differs.");
                previousOffset=offset;
            }
            if(previousOffset!=551)throw new InvalidDataException("Actual FIRST alphabet UTF8 extent differs.");
            var b=new byte[5859];
            for(int i=0;i<b.Length;i++)
            {
                if((i&31)==0)token.ThrowIfCancellationRequested();
                b[i]=capture[34+i];
            }
            token.ThrowIfCancellationRequested();return new FirstAlphabetPrefix(b);
        }
        internal ushort Count=>H(bytes,14);
        internal ushort ResolveSpecial(int ordinal)
        {
            //Actual CAF0 literals at2D8B740..784, not expected output IDs.
            (uint text,int length)=ordinal switch
            {
                0=>(0x27u,1),1=>(0x22u,1),2=>(0x2du,1),3=>(0x5fu,1),
                4=>(0xbc83e3u,3),5=>(0x80b8e4u,3),6=>(0x8280b0f3u,4),
                7=>(0x8380b0f3u,4),8=>(0x8080b0f3u,4),9=>(0x8180b0f3u,4),
                10=>(0x6fu,1),11=>(0x4fu,1),12=>(0x30u,1),13=>(0xbdbfefu,3),
                _=>throw new ArgumentOutOfRangeException(nameof(ordinal))
            };
            return Lookup(text,length);
        }
        private ushort Lookup(uint text,int length)
        {
            //Default codec5 same-identity raw UTF8 path in Engine0527E0.
            uint hash=0x1505;
            for(int i=0;i<length;i++)hash=unchecked(hash*33)^(byte)(text>>(8*i));
            int x=H(bytes,18+2*(int)(hash>>16&15));
            if(x==65535)return ushort.MaxValue;
            x=H(bytes,50+2*(x+(int)(hash>>6&31)));
            if(x==65535)return ushort.MaxValue;
            x=H(bytes,306+2*(x+(int)(hash&63)));
            if(x==65535)return ushort.MaxValue;
            int begin=H(bytes,2738+2*x),end=H(bytes,2738+2*(x+1));
            //Authenticated static tables prove end-begin<=2. No caller table.
            for(int row=begin;row<end;row++)
            {
                int start=(int)I(bytes,3236+8*row),stop=(int)I(bytes,3236+8*(row+1));
                int available=stop-start,n=length<available?length:available,comparison=0;
                for(int j=0;j<n;j++)
                {
                    comparison=(byte)(text>>(8*j))-bytes[5308+start+j];
                    if(comparison!=0)break;
                }
                if(comparison==0)comparison=length-(stop-start);
                if(comparison==0)return H(bytes,3236+8*row+4);
                if(comparison<0)return ushort.MaxValue;
            }
            return ushort.MaxValue;
        }
    }
}
