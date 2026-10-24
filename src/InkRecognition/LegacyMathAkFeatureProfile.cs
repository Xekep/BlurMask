using System.Buffers.Binary;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;

[assembly: InternalsVisibleTo("LegacyMathAkFeatureProfile.Probe")]

namespace InkRecognition;

/// <summary>Once-authenticated immutable selected Math feature program and four
/// calibration pairs. Geometry subsets share it without resource rehashing or
/// calibration copies. It supplies features only, never model/candidate authority.</summary>
public sealed partial class LegacyMathAkFeatureProfile
{
    private readonly short[][] minimum, maximum;
    private readonly ProgramDescriptor program;
    internal LegacyMathAkOriginalSmoothingPolicy OriginalSmoothing { get; }
    internal LegacyMathAkSource13A2Policy Source13A2Policy { get; }
    //2 logical scalar bytes per authenticated profile, not calibration payload.
    //Native tail13A4+32, distinct from Program.GridScale and FIRST13A4.
    internal ushort FreshCommonHeapCapacity { get; }
    public LegacyMathAkShapePrototypes ShapePrototypes { get; }
    private LegacyMathAkFeatureProfile(short[][] minimum, short[][] maximum, ProgramDescriptor program, LegacyMathAkShapePrototypes shapes, LegacyMathAkOriginalSmoothingPolicy smoothing, LegacyMathAkSource13A2Policy sampler, ushort freshCommonHeapCapacity, First276Program first276, FirstAlphabetPrefix firstAlphabet)
    { this.minimum = minimum; this.maximum = maximum; this.program = program; ShapePrototypes = shapes; OriginalSmoothing = smoothing; Source13A2Policy = sampler; FreshCommonHeapCapacity = freshCommonHeapCapacity; this.first276 = first276; this.firstAlphabet = firstAlphabet; }
    public int CalibrationPayloadBytes => 4772;
    public string SourceFingerprint => LegacyMathMixed107RawResource.ResourceFingerprint;
    public ProgramDescriptor Program => program;
    public bool HasCandidateAuthority => false;
    public readonly record struct ProgramDescriptor(byte ModeX, byte ModeY, ushort NormalizationTarget,
        ushort LegacyMask, uint LegacyFlags, ushort TemporalPointCount, ushort SpatialBins,
        ushort GridSide, ushort Quantizer, ushort DirectionalMask, uint GlobalMask, uint BaselineMask,
        uint TemporalMask, uint SpatialMask, uint UpperMask, uint TargetCount, uint GridScale,
        short TemporalWindows, short SpatialColumns, short SpatialRows, short UpperColumns,
        short UpperRows, byte UnitPolicy, byte Backend);

    public static LegacyMathAkFeatureProfile PrepareActual(ReadOnlySpan<byte> resource,
        CancellationToken token = default) => PrepareCore(resource, null, token);
    internal static LegacyMathAkFeatureProfile PrepareWithObserver(ReadOnlySpan<byte> resource,
        Action<string> observer, CancellationToken token = default)
        => PrepareCore(resource, observer ?? throw new ArgumentNullException(nameof(observer)), token);
    private static void Boundary(string phase, Action<string>? observer, CancellationToken token)
    { token.ThrowIfCancellationRequested(); observer?.Invoke(phase); token.ThrowIfCancellationRequested(); }
    private static LegacyMathAkFeatureProfile PrepareCore(ReadOnlySpan<byte> resource,
        Action<string>? observer, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        if (resource.Length != LegacyMathMixed107RawResource.ResourceBytes)
            throw new ArgumentException("Exact actual Math AK resource required.", nameof(resource));
        byte[] captured = new byte[resource.Length];
        try
        {
            const int chunk = 16384;
            for (int at = 0; at < captured.Length; at += chunk)
            { token.ThrowIfCancellationRequested(); resource.Slice(at, Math.Min(chunk, captured.Length - at)).CopyTo(captured.AsSpan(at)); }
            Boundary("captured", observer, token);
            using (var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256))
            {
                for (int at = 0; at < captured.Length; at += chunk)
                { token.ThrowIfCancellationRequested(); hash.AppendData(captured, at, Math.Min(chunk, captured.Length - at)); }
                if (!Convert.ToHexString(hash.GetHashAndReset()).Equals(SourceFingerprintValue, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidDataException("Actual Math AK fingerprint required.");
            }
            Boundary("authenticated", observer, token);
            //Same private authenticated snapshot, actual13A0 child265/v102.
            if (BinaryPrimitives.ReadUInt32LittleEndian(captured.AsSpan(7301)) != 0x0101fe90 ||
                BinaryPrimitives.ReadUInt32LittleEndian(captured.AsSpan(7305)) != 40 ||
                BinaryPrimitives.ReadUInt32LittleEndian(captured.AsSpan(7309)) != 0x265 ||
                BinaryPrimitives.ReadUInt32LittleEndian(captured.AsSpan(7313)) != 0x01020000)
                throw new InvalidDataException("Actual original-source smoothing child differs.");
            float F(int at) => BinaryPrimitives.ReadSingleLittleEndian(captured.AsSpan(at, 4));
            var smoothing = new LegacyMathAkOriginalSmoothingPolicy(F(7317), F(7321), F(7325), F(7329),
                F(7333), F(7337), F(7341), F(7345));
            //All field reads are from the same private authenticated snapshot.
            ushort H(int at) => BinaryPrimitives.ReadUInt16LittleEndian(captured.AsSpan(at, 2));
            uint I(int at) => BinaryPrimitives.ReadUInt32LittleEndian(captured.AsSpan(at, 4));
            var program = new ProgramDescriptor(captured[1005996], captured[1005997], H(1005998),
                H(1006005), I(1006007), H(1006011), H(1006013), H(1006015), H(1006017),
                H(1006019), I(1006021), I(1006025), I(1006029), I(1006033), I(1006037),
                I(1006041), I(1006045), (short)H(1006051), (short)H(1006055), (short)H(1006057),
                (short)H(1006061), (short)H(1006063), captured[1006065], captured[1006066]);
            var expected = new ProgramDescriptor(1, 1, 8192, 0x1211, 63, 30, 16, 8, 10,
                0xa111, 0x1e1fff, 7, 0xe1fff, 0xe1fff, 0xff, 100, 10, 16, 4, 4, 5, 5, 1, 2);
            if (program != expected || captured[1006000] != 0 || H(1006001) != 0 ||
                captured[1006003] != 1 || captured[1006004] != 0 || H(1006049) != 1 ||
                H(1006053) != 1 || H(1006059) != 1 || H(1006067) != 0 || I(1006069) != 0 ||
                I(1006073) != 0 || H(1006077) != 0 || H(1006079) != 0 || captured[1006081] != 0)
                throw new InvalidDataException("Actual selected Math feature program differs.");
            int[] counts = [412, 279, 279, 223];
            int[] starts = [1006082, 1288051, 1458131, 1628211];
            var minimum = new short[4][]; var maximum = new short[4][];
            for (int family = 0; family < 4; family++)
            {
                int start = starts[family], count = counts[family];
                if (captured[start] != 1 || H(start + 1) != count)
                    throw new InvalidDataException("Actual selected calibration dimensions differ.");
                minimum[family] = new short[count]; maximum[family] = new short[count];
                for (int i = 0; i < count; i++)
                {
                    if ((i & 31) == 0) token.ThrowIfCancellationRequested();
                    minimum[family][i] = (short)H(start + 3 + i * 2);
                    maximum[family][i] = (short)H(start + 3 + count * 2 + i * 2);
                }
            }
            var shapes = LegacyMathAkShapePrototypes.ReadAuthenticated(captured, token);
            var sampler = LegacyMathAkSource13A2Policy.ReadAuthenticated(captured);
            //Full private snapshot SHA already authenticated above. Actual
            //tail13A4 frame1005964..1791853; native1D7C0 reads this ushort
            //into receiver+32, clone1B150 retains it,009500 copies it to P+58.
            ushort freshCommonHeapCapacity = H(1790683);
            //Separate FIRST owner, from THIS private authenticated capture.
            //No caller-buffer reread, selected-tail calibration or model copy.
            var first276 = First276Program.ReadAuthenticated(captured, token);
            var firstAlphabet = FirstAlphabetPrefix.ReadAuthenticated(captured, token);
            Boundary("calibrated", observer, token);
            Boundary("publish", observer, token);
            return new(minimum, maximum, program, shapes, smoothing, sampler, freshCommonHeapCapacity, first276, firstAlphabet);
        }
        finally { CryptographicOperations.ZeroMemory(captured); }
    }
    private const string SourceFingerprintValue = LegacyMathMixed107RawResource.ResourceFingerprint;
    public LegacyMathAkSourceGeometry CreateGeometry(IReadOnlyList<Vector2[]> strokes,
        CancellationToken token = default) => LegacyMathAkSourceGeometry.CreateFromProfile(this, strokes, token);
    internal short[] ComposeUnguidedDirectionalTemporal(ReadOnlySpan<int> global,
        IReadOnlyList<int[]> windows, CancellationToken token)
    {
        //BF090 calibrates only the274-column prefix with THIS supplied3F pair;
        //baseline mask7 adds three channels afterwards. No calibration clone.
        Span<int> absentGuide = stackalloc int[9]; absentGuide.Clear();
        short[] full = LegacySaturatedDirectionalFeatureAssembly.Compose(global, windows,
            absentGuide, false, minimum[1].AsSpan(0, 274), maximum[1].AsSpan(0, 274), token);
        var values = new short[279]; full.AsSpan(0, 277).CopyTo(values);
        values[277] = 1024; values[278] = 1024;
        token.ThrowIfCancellationRequested(); return values;
    }
    internal short[] ComposeUnguidedDirectionalSpatial(ReadOnlySpan<int> global,
        IReadOnlyList<int[]> windows, CancellationToken token)
    {
        //BFF40 calibrates only the274-column prefix with THIS supplied4F pair;
        //baseline mask7 adds three channels afterwards. No calibration clone.
        Span<int> absentGuide = stackalloc int[9]; absentGuide.Clear();
        short[] full = LegacySaturatedDirectionalFeatureAssembly.Compose(global, windows,
            absentGuide, true, minimum[2].AsSpan(0, 274), maximum[2].AsSpan(0, 274), token);
        var values = new short[279]; full.AsSpan(0, 277).CopyTo(values);
        values[277] = 1024; values[278] = 1024;
        token.ThrowIfCancellationRequested(); return values;
    }
    internal short[] ComposeUnguidedDirectionalUpper(ReadOnlySpan<int> global,
        int[][] cells, int physicalArc, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        if (global.Length != 17 || cells.Length != 25 || physicalArc < 0)
            throw new ArgumentException("Actual17 globals/25upper cells/nonnegative arc required.");
        var values = new short[223]; int at = 0; values[at++] = 1024;
        unchecked
        {
            for (int i = 0; i < 17; i++)
            {
                token.ThrowIfCancellationRequested(); int v = global[i];
                values[at++] = UpperClamp(i switch {2 => (v * 2048 - 2048) / 5 - 1024,
                    3 => v * 2048 / 3072 - 1024, 8 => v * 2,
                    >= 9 and <= 15 => v * 2048 / 3 - 1024, _ => v * 2048 / 1024 - 1024});
            }
            int denominator = physicalArc / 20; //C0DA0: arc / ((5+5)*2), before each division.
            foreach (var cell in cells)
            {
                token.ThrowIfCancellationRequested();
                if (cell is null || cell.Length != 8) throw new ArgumentException("Eight upper direction masses required.");
                for (int i = 0; i < 8; i++) values[at++] = TransformUpperMass(cell[i], denominator);
            }
            //F0 active starts calibration at1; only218-prefix is calibrated.
            for (int i = 1; i < 218; i++)
            {
                token.ThrowIfCancellationRequested(); int range = maximum[3][i] - minimum[3][i];
                if (range != 0) values[i] = UpperClamp((values[i] - minimum[3][i]) * 2048 / range - 1024);
            }
        }
        values[218] = values[219] = values[220] = -1024;
        values[221] = values[222] = 1024;
        token.ThrowIfCancellationRequested(); return values;
    }
    private static short TransformUpperMass(int mass, int denominator)
        => denominator == 0 ? (short)0 : UpperClamp(unchecked((mass << 11) / denominator - 1024));
    private static short UpperClamp(int value) => (short)Math.Clamp(value, short.MinValue, short.MaxValue);
    internal short MinimumAt(int family, int index) => minimum[family][index];
    internal short MaximumAt(int family, int index) => maximum[family][index];
}
