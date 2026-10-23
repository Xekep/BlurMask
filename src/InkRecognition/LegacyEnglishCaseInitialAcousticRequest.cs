using System.Buffers.Binary;
namespace InkRecognition;
// Reused private 1FD240 cell. Unknown stack padding has NO value and cannot be read.
internal sealed class LegacyEnglishCaseInitialAcousticRequest
{
    private readonly byte[] bytes=new byte[80],written=new byte[80];
    private readonly LegacyEnglishCaseInitialConsumer owner;
    private static long nextCell;
    internal LegacyNativeWordTransitionCostCell Cost{get;}=new(checked((ulong)Interlocked.Increment(ref nextCell)),0);
    internal LegacyEnglishCaseInitialAcousticRequest(LegacyEnglishCaseInitialConsumer owner){this.owner=owner;}
    internal Accessor Issue(LegacyEnglishFreshCaseMainSource.InitialQueryLease lease,ushort arc,ushort symbol,int geometry,int baseCost,ulong tag)
    {
        owner.RequireLease(lease);Array.Clear(written);
        W32(0,0);W32(4,-1);W32(8,-1);W16(12,arc);W16(14,65535);W16(16,65535);
        W8(18,checked((byte)geometry));W8(19,255);W32(20,-1);W32(24,-1);W8(28,0);W32(32,0);W16(36,symbol);W16(38,65535);
        BinaryPrimitives.WriteUInt64LittleEndian(bytes.AsSpan(40),Cost.Identity);Mark(40,8);
        W32(48,0);W32(52,0x4000000);W8(56,0);W8(57,0);W8(58,0);W32(64,6);W32(68,0);W8(72,(byte)(tag>>56&1));W8(73,0);
        Cost.Value=baseCost;return new(this,lease);
    }
    private void Mark(int at,int count){for(int i=0;i<count;i++)written[at+i]=1;}
    private void W8(int at,byte value){bytes[at]=value;Mark(at,1);}
    private void W16(int at,ushort value){BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(at),value);Mark(at,2);}
    private void W32(int at,int value){BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(at),value);Mark(at,4);}
    private ReadOnlySpan<byte> Read(int at,int count)
    {if(at<0||count>80-at)throw new ArgumentOutOfRangeException(nameof(at));for(int i=0;i<count;i++)if(written[at+i]==0)throw new InvalidOperationException("Unwritten native request field.");return bytes.AsSpan(at,count);}
    internal (byte[] Bytes,byte[] Mask,int Cost) CopyHistorical()
    {var copy=new byte[80];for(int i=0;i<80;i++)if(written[i]!=0)copy[i]=bytes[i];return(copy,written.ToArray(),Cost.Value);}
    internal readonly struct Accessor : ILegacyNativeWordAcousticAccessor
    {
        private readonly LegacyEnglishCaseInitialAcousticRequest request;
        private readonly LegacyEnglishFreshCaseMainSource.InitialQueryLease lease;
        internal Accessor(LegacyEnglishCaseInitialAcousticRequest request,LegacyEnglishFreshCaseMainSource.InitialQueryLease lease){this.request=request;this.lease=lease;}
        private void Require()=>request.owner.RequireLease(lease);
        public LegacyNativeWordTransitionCostCell Cost{get{Require();return request.Cost;}}
        public int ReadInt32(int at){Require();return BinaryPrimitives.ReadInt32LittleEndian(request.Read(at,4));}
        public uint ReadUInt32(int at){Require();return BinaryPrimitives.ReadUInt32LittleEndian(request.Read(at,4));}
        public ushort ReadUInt16(int at){Require();return BinaryPrimitives.ReadUInt16LittleEndian(request.Read(at,2));}
        public byte ReadByte(int at){Require();return request.Read(at,1)[0];}
        public void WriteByte(int at,byte value){Require();if(at<48||at>51)throw new InvalidOperationException("Only native accumulator30 writes allowed.");_=request.Read(at,1);request.bytes[at]=value;}
    }
}
