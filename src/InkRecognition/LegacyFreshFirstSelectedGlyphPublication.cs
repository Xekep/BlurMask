namespace InkRecognition;

/// <summary>Actual1F42D0 allocation guard BEFORE276 append, glyph link and
/// confidence reads. E2/E4 are retained in the B8/source walk but do not create
/// glyphs when the owned invocation99 is false. Fresh6C's later literal blank
/// conversion cannot bypass this outer guard.</summary>
public sealed class LegacyFreshFirstSelectedGlyphPublication
{
    public LegacyFreshFirstResultOwnershipFactory Factory {get;}
    public ushort BlankE2 {get;}
    public ushort ClosedE4 {get;}
    public bool Configuration99=>Factory.InvocationPolicy.Invocation.Configuration99;
    private LegacyFreshFirstSelectedGlyphPublication(LegacyFreshFirstResultOwnershipFactory factory,ushort blank,ushort closed)
    {Factory=factory;BlankE2=blank;ClosedE4=closed;}

    public static LegacyFreshFirstSelectedGlyphPublication Create(LegacyFreshFirstResultOwnershipFactory factory,
        CancellationToken cancellationToken=default)
    {
        ArgumentNullException.ThrowIfNull(factory);cancellationToken.ThrowIfCancellationRequested();
        factory.ValidateInvocation(factory.AcousticModel,factory.Frame,factory.InvocationPolicy,cancellationToken);
        var alphabet=factory.InvocationPolicy.Binding.Alphabet;
        ushort closed=alphabet.TryGetLabel("\U000f0001",out ushort label)?label:ushort.MaxValue;
        return new(factory,checked((ushort)alphabet.LabelCount),closed);
    }

    public bool ShouldAllocate(ushort selectedSymbol,CancellationToken cancellationToken=default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if(selectedSymbol>BlankE2+2)throw new ArgumentOutOfRangeException(nameof(selectedSymbol));
        return Configuration99||(selectedSymbol!=BlankE2&&selectedSymbol!=ClosedE4);
    }
}
