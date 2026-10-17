using System.Buffers.Binary;
using System.Security.Cryptography;

namespace InkRecognition;

/// <summary>Immutable107 two-layer int8 wire object with its original stream
/// origin. This type reads weights and scales; it does not evaluate features.</summary>
public sealed class LegacyInkNetwork107Wire
{
    private readonly byte[] bytes;
    public LegacySource247WireLayout Source {get;}
    public int Offset {get;}
    public int End=>checked(Offset+bytes.Length);
    public string Fingerprint {get;}
    public ushort Kind {get;}
    public int InputWidth {get;}
    public int HiddenWidth {get;}
    public int OutputWidth {get;}
    public IReadOnlyList<LegacyNetwork107Matrix> Matrices {get;}
    private LegacyInkNetwork107Wire(LegacySource247WireLayout source,int offset,byte[] bytes,
        ushort kind,int inputs,int hidden,int outputs,LegacyNetwork107Matrix[] matrices)
    {Source=source;Offset=offset;this.bytes=bytes;Kind=kind;InputWidth=inputs;HiddenWidth=hidden-1;
        OutputWidth=outputs-1;Matrices=Array.AsReadOnly(matrices);Fingerprint=Convert.ToHexString(SHA256.HashData(bytes));}
    public byte[] CopyBytes()=>bytes.ToArray();
    public sbyte[] CopyMatrixWeights(int index)
    {
        if((uint)index>=Matrices.Count)throw new ArgumentOutOfRangeException(nameof(index));
        var m=Matrices[index];var result=new sbyte[checked(m.Rows*m.Stride)];
        Buffer.BlockCopy(bytes,m.WeightsOffset-Offset,result,0,result.Length);return result;
    }
    public byte[] CopyMatrixScales(int index)
    {if((uint)index>=Matrices.Count)throw new ArgumentOutOfRangeException(nameof(index));var m=Matrices[index];return bytes.AsSpan(m.ScalesOffset-Offset,m.Rows+1).ToArray();}
    internal static LegacyInkNetwork107Wire Read(LegacySource247WireLayout source,ReadOnlySpan<byte> input,int originalOffset,CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        if(input.Length is <33 or >16*1024*1024||U16(input,0)!=0xfea0||U16(input,2)!=0x107||U32(input,4)!=input.Length-8)
            throw new InvalidDataException("Complete bounded FEA0/107 required.");
        if(input[8]!=0x12||U32(input,19)!=0||U32(input,23)!=0||U16(input,31)!=10)
            throw new NotSupportedException("Only the proved two-layer12/10 network wire architecture is supported.");
        int inputs=Width(input,11),hidden=Width(input,15),outputs=Width(input,27),cursor=33;
        var matrices=new LegacyNetwork107Matrix[2];
        for(int i=0;i<2;i++)
        {
            token.ThrowIfCancellationRequested();int columns=i==0?inputs:hidden,rows=(i==0?hidden:outputs)-1;
            cursor=checked(((originalOffset+cursor+15)&~15)-originalOffset);int stride=checked((columns+15)&~15);
            int weights=cursor,scales=checked(cursor+stride*rows);
            if(cursor<0||scales>input.Length-rows-1)throw new InvalidDataException("Truncated aligned107 matrix.");
            if(input[scales]!=1)throw new InvalidDataException("Invalid leading matrix scale.");
            for(int n=1;n<=rows;n++){if((n&255)==0)token.ThrowIfCancellationRequested();if(input[scales+n]==0)throw new InvalidDataException("Zero row scale.");}
            matrices[i]=new(originalOffset+weights,originalOffset+scales,columns,stride,rows);cursor=scales+rows+1;
        }
        if(cursor!=input.Length)throw new InvalidDataException("Unconsumed107 matrix data.");
        return new(source,originalOffset,input.ToArray(),U16(input,9),inputs,hidden,outputs,matrices);
    }
    private static int Width(ReadOnlySpan<byte>d,int p){uint n=U32(d,p);return n is >1 and <=4096?(int)n:throw new InvalidDataException("Portable layer-width bound exceeded.");}
    private static ushort U16(ReadOnlySpan<byte>d,int p)=>BinaryPrimitives.ReadUInt16LittleEndian(d[p..]);
    private static uint U32(ReadOnlySpan<byte>d,int p)=>BinaryPrimitives.ReadUInt32LittleEndian(d[p..]);
}

public readonly record struct LegacyNetwork107Matrix(int WeightsOffset,int ScalesOffset,int Columns,int Stride,int Rows);
