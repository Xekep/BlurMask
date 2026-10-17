namespace InkRecognition.Math;

/// <summary>Restricted cold bundle policy, with no caller property packet.
/// The raw-only source exclusion is a library contract, not native EEB inference.</summary>
internal sealed class Math2Mode0RawOnlyEf4Policy
{
    internal Math2Mode0ColdBundle Bundle { get; }
    internal float Alpha => BitConverter.Int32BitsToSingle(0x3dcccccd);
    internal ushort Replacement => 299;
    internal ushort Separator => ushort.MaxValue;
    internal byte CorrectionByte => 1;
    internal int OptionalCount => 0;
    private Math2Mode0RawOnlyEf4Policy(Math2Mode0ColdBundle bundle) => Bundle = bundle;
    internal static Math2Mode0RawOnlyEf4Policy Bind(Math2Mode0ColdBundle bundle)
    {
        ArgumentNullException.ThrowIfNull(bundle);
        if (!bundle.ForceNonobservablePositions)
            throw new NotSupportedException("The authenticated three-model cold bundle is required.");
        return new(bundle);
    }
}
