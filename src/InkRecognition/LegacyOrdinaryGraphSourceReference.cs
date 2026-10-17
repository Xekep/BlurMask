using System.Buffers.Binary;

namespace InkRecognition;

/// <summary>Actual ordinary1A9AE0 source24-byte key pair and its separately
/// stored two endpoint remainder bytes. These are not document object IDs.</summary>
public readonly record struct LegacyOrdinaryGraphSourceReference(LegacySourcePointKey First,
    LegacySourcePointKey Last,byte FirstResidual,byte LastResidual)
{
    public byte[] CopyReferenceStorage24()
    {
        byte[] result=new byte[24];Write(First,0);Write(Last,12);return result;
        void Write(LegacySourcePointKey key,int offset)
        {
            BinaryPrimitives.WriteInt32LittleEndian(result.AsSpan(offset),key.First);
            BinaryPrimitives.WriteInt32LittleEndian(result.AsSpan(offset+4),key.Second);
            BinaryPrimitives.WriteInt32LittleEndian(result.AsSpan(offset+8),key.Third);
        }
    }
    public byte[] CopyResidualStorage2()=>[FirstResidual,LastResidual];
}
