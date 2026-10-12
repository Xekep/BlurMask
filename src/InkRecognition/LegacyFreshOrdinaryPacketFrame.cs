namespace InkRecognition;

/// <summary>Fresh one-descriptor raw packet through native24a callback16e850,
/// deskew and265 preparation. The existing row result and updated descriptor
/// share the same raw ingress ownership. Whole-page packet event handling and
/// final text metrics remain separate.</summary>
public sealed class LegacyFreshOrdinaryPacketFrame
{
    public LegacyFreshOrdinaryRawPacket Packet {get;}
    public LegacyOrdinaryRowAcousticInput Row {get;}
    public LegacyOrdinaryRawInputDescriptor Descriptor {get;}
    public int LineCount=>1;
    private LegacyFreshOrdinaryPacketFrame(LegacyFreshOrdinaryRawPacket packet,LegacyOrdinaryRowAcousticInput row)
    {
        if(!ReferenceEquals(packet.Source,row.OriginalSource))throw new ArgumentException("Frame must retain the packet's ingress ownership.");
        Packet=packet;Row=row;
        //16e850 visits the one fresh kind0 descriptor withline10<0, appends one
        //40-byte24a record and assignsdescriptor10 = new frame count minus1.
        Descriptor=packet.Descriptor with {CurrentLineId=0};
    }
    public static LegacyFreshOrdinaryPacketFrame PrepareFirstSource(ReadOnlySpan<byte> resource,int sourceOffset,
        IReadOnlyList<InkStroke> rawPhysicalStrokes,CancellationToken cancellationToken=default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        byte[] snapshot=resource.ToArray();
        var packet=LegacyFreshOrdinaryRawPacket.PrepareFirstSource(snapshot,sourceOffset,rawPhysicalStrokes,cancellationToken);
        var profile=LegacyOrdinaryRowAcousticProfile.ReadResourceNode(snapshot,sourceOffset);
        var row=LegacyOrdinaryRowAcousticInput.PrepareOwnedSource(packet.Source,profile,cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();return new(packet,row);
    }
    /// <summary>218.virtual40 frame membership is native descriptor line10.
    /// Fresh one-descriptor initialization admits only descriptor0/line0.</summary>
    public LegacyDeskewedLine GetFrameForDescriptor(int descriptorOrdinal,CancellationToken cancellationToken=default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if(descriptorOrdinal!=0)throw new ArgumentOutOfRangeException(nameof(descriptorOrdinal));
        return Row.Frame;
    }
}
