namespace InkRecognition;

/// <summary>Owned fresh direct packet invocation, with no existing source,
/// clone or property-event API. Source16a130 initializes flag150 tozero; direct
///167210/24a/265 preparation does not change it. General/reused native sources
///must instead supply their actual150 value to the binding policy.</summary>
public sealed class LegacyFreshDirectFirstInvocationPolicy
{
    public LegacyFreshFirstTextContextPolicy Binding {get;}
    public LegacyFreshFirstInvocationSettings Invocation {get;}
    public bool SourceFrameFlag150=>false;
    private LegacyFreshDirectFirstInvocationPolicy(LegacyFreshFirstTextContextPolicy binding)
    {Binding=binding;Invocation=binding.ForInvocation(false);}

    public static LegacyFreshDirectFirstInvocationPolicy Create(ReadOnlySpan<byte> resource,int sourceOffset,
        LegacyFreshOrdinaryPacketFrame frame,CancellationToken cancellationToken=default)
    {
        ArgumentNullException.ThrowIfNull(frame);cancellationToken.ThrowIfCancellationRequested();
        var binding=LegacyFreshFirstTextContextPolicy.ReadForFreshSelectedChild(resource,sourceOffset,cancellationToken);
        var rowProfile=LegacyOrdinaryRowAcousticProfile.ReadResourceNode(resource,sourceOffset);
        if(frame.Row.Profile!=rowProfile||!ReferenceEquals(frame.Row.OriginalSource,frame.Packet.Source)||
            frame.Packet.DescriptorCount!=1||frame.Descriptor.CurrentLineId!=0||frame.Descriptor.ScopeId!=-1||
            frame.Descriptor.Flags!=0x10)
            throw new ArgumentException("Matching new direct raw packet/frame ownership required.",nameof(frame));
        cancellationToken.ThrowIfCancellationRequested();return new(binding);
    }
}
