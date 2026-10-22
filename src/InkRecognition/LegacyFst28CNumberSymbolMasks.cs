using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;

[assembly: InternalsVisibleTo("LegacyFst28CNumberSymbolMasks.Probe")]
namespace InkRecognition;

/// <summary>Owned source-defined symbols and ordinary M masks for the explicit
/// direct-number fresh bootstrap. No D0, permission refresh, query or OCR authority.</summary>
public sealed class LegacyFst28CNumberSymbolMasks
{
    public const int LogicalByteEnvelope = 595475, WorkEnvelope = 1160978;
    private const string AcousticHash = "7A5F08809E81FB4A2D4C78B3EFD251D0986503B0D30D21DE3695DAB1F8A89978";
    private readonly uint[] sourceTokens;
    private readonly ushort[] preserved;
    private readonly byte[] seed, primary, secondary;
    private readonly object cohort = new();
    public LegacyFst28CFreshPrimaryControllerBootstrap Bootstrap { get; }
    public LegacyFst28CInitialState InitialState => Bootstrap.InitialState;
    public LegacyFst28CTargetPreparation Policy => InitialState.TargetPreparation;
    public LegacyFst13AOutgoingSource Source => Policy.PrimaryMap.Source;
    public LegacyFreshMainCompactOrdinaryGraph Graph => Bootstrap.Graph;
    public bool HasQueryAuthority => false;
    public bool HasD0PermissionAuthority => false;
    private LegacyFst28CNumberSymbolMasks(LegacyFst28CFreshPrimaryControllerBootstrap bootstrap,
        uint[] tokens, ushort[] preserved, byte[] seed, byte[] primary, byte[] secondary)
    { Bootstrap = bootstrap; sourceTokens = tokens; this.preserved = preserved;
      this.seed = seed; this.primary = primary; this.secondary = secondary; }

    public static LegacyFst28CNumberSymbolMasks CreateActualDirectNumberFresh(
        LegacyFst28CFreshPrimaryControllerBootstrap bootstrap, ReadOnlySpan<byte> acousticResource,
        CancellationToken token = default)
        => CreateBounded(bootstrap, acousticResource, LogicalByteEnvelope, WorkEnvelope, token);

    internal static LegacyFst28CNumberSymbolMasks CreateBounded(
        LegacyFst28CFreshPrimaryControllerBootstrap bootstrap, ReadOnlySpan<byte> acousticResource,
        int byteBudget, int workBudget, CancellationToken token = default, Action<string>? checkpoint = null)
    {
        token.ThrowIfCancellationRequested(); ArgumentNullException.ThrowIfNull(bootstrap);
        if (byteBudget is < 0 or > LogicalByteEnvelope) throw new ArgumentOutOfRangeException(nameof(byteBudget));
        if (workBudget is < 0 or > WorkEnvelope) throw new ArgumentOutOfRangeException(nameof(workBudget));
        if (byteBudget < LogicalByteEnvelope || workBudget < WorkEnvelope)
            throw new InvalidDataException("Whole source/M-mask logical copy/work envelope exceeds budget.");
        var initial = bootstrap.InitialState; var map = initial.TargetPreparation.PrimaryMap; var source = map.Source;
        var constructor = bootstrap.ReadConstructorState(token); var md = bootstrap.ReadSourceMetadata(token);
        if (bootstrap.HasIssuedQuery || bootstrap.AdmittedSourceCount != 1 || bootstrap.SelectedSourceCount != 1 ||
            bootstrap.SelectedOrdinal != 0 || !constructor.Primary || !constructor.HeaderPresent || constructor.ContextIssued ||
            !source.HasActualFreshMain135Authority || source.ActualAcousticFingerprint != AcousticHash ||
            source.ActualAcousticSourceKind != 0x135 || map.SourceLabelCount != 16 || map.AcousticLabelCount != 192 ||
            md != new LegacyFst28CNumberSourceMetadata(1, 0, 1, 0, 1, uint.MaxValue, 1) ||
            initial.DecodedSourceName != "ru_RU-lk-text.number" || acousticResource.Length != 572763)
            throw new ArgumentException("Exact retained direct-number fresh bootstrap and same MAIN135 AK required.");
        void Checkpoint(string stage) { token.ThrowIfCancellationRequested(); checkpoint?.Invoke(stage); token.ThrowIfCancellationRequested(); }
        Checkpoint("before-snapshot");
        var snapshot = new byte[572763];
        try
        {
            for (int at = 0; at < snapshot.Length; at += 65536)
            {
                token.ThrowIfCancellationRequested();
                acousticResource.Slice(at, Math.Min(65536, snapshot.Length - at)).CopyTo(snapshot.AsSpan(at));
            }
            Checkpoint("after-snapshot");
            if (Convert.ToHexString(SHA256.HashData(snapshot)) != AcousticHash)
                throw new ArgumentException("Authenticated same acoustic snapshot required.", nameof(acousticResource));
            Checkpoint("after-hash");
            // Read only AFTER full snapshot authentication. Exact v110 no-group
            // field is proved by the independent full raw-wire partition.
            if (BinaryPrimitives.ReadUInt32LittleEndian(snapshot.AsSpan(5233)) != 0x135 ||
                BinaryPrimitives.ReadUInt32LittleEndian(snapshot.AsSpan(189742)) != 0x21d ||
                BinaryPrimitives.ReadUInt16LittleEndian(snapshot.AsSpan(189748)) != 0x110 ||
                BinaryPrimitives.ReadInt16LittleEndian(snapshot.AsSpan(566380)) != 0)
                throw new InvalidDataException("Actual source/ordinary110/empty knowledge-group partition differs.");
            var alphabet = LegacyInkAlphabet.Load(snapshot.AsSpan(99, 5134), 0x103);
            if (alphabet.LabelCount != 192 || alphabet.BytesConsumed != 5134)
                throw new InvalidDataException("Actual acoustic alphabet dimensions differ.");
            Checkpoint("after-alphabet");
            var tokens = new uint[16]; var preserved = new ushort[16];
            for (ushort i = 0; i < 16; i++)
            {
                token.ThrowIfCancellationRequested(); var entry = map.MapLabel(i);
                if (entry.SourceLabel != i) throw new InvalidDataException("Source enumeration identity differs.");
                tokens[i] = i; preserved[i] = entry.Code;
            }
            // Numeric shared callback algebra only: actual source remains13A,
            // not a nominal2B7 map or selected13B owner. No caller success flag.
            LegacyLinguisticSymbolSource[] sources = [new(tokens, preserved)];
            var controller = LegacyLinguisticSymbolMaskController.Build(192, sources, true, token);
            if (controller.Outcome != LegacySymbolMaskControllerOutcome.Succeeded || controller.Mask.Length != 192)
                throw new InvalidDataException("Actual singleton enumeration did not complete.");
            var seed = controller.Mask.ToArray(); Checkpoint("after-seed");
            // Fresh restriction is absent; actual pinned alphabet has no Arabic
            // contextual candidate or wide/base pair; actual ordinary groups=0.
            var producer = new LegacyOrdinarySymbolMaskProducer(alphabet, snapshot.AsSpan(99, 16));
            var masks = producer.Build(controller.Outcome, seed, Array.Empty<LegacySymbolMaskKnowledgeGroup>(),
                [], [], [], token);
            var primary = masks.Primary.ToArray(); var secondary = masks.Secondary.ToArray();
            Checkpoint("after-masks");
            if (primary.Length != 192 || secondary.Length != 192)
                throw new InvalidDataException("Actual M-mask dimensions differ.");
            Checkpoint("before-publish");
            return new(bootstrap, tokens, preserved, seed, primary, secondary);
        }
        finally { CryptographicOperations.ZeroMemory(snapshot); }
    }

    public uint[] CopySourceTokens(CancellationToken token = default) => Copy(sourceTokens, token);
    public ushort[] CopyPreservedMap(CancellationToken token = default) => Copy(preserved, token);
    public byte[] CopySeed(CancellationToken token = default) => Copy(seed, token);
    public byte[] CopyPrimaryMask(CancellationToken token = default) => Copy(primary, token);
    public byte[] CopySecondaryMask(CancellationToken token = default) => Copy(secondary, token);
    private static T[] Copy<T>(T[] input, CancellationToken token)
    {
        token.ThrowIfCancellationRequested(); var copy = new T[input.Length];
        for (int i = 0; i < copy.Length; i++) { token.ThrowIfCancellationRequested(); copy[i] = input[i]; }
        token.ThrowIfCancellationRequested(); return copy;
    }
    internal bool IsSameIssuer(LegacyFst28CNumberSymbolMasks other)
        => ReferenceEquals(this, other) && ReferenceEquals(cohort, other.cohort);
}
