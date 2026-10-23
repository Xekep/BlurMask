using System.Buffers.Binary;
using System.Security.Cryptography;

namespace InkRecognition;

/// <summary>
/// Owns the two observed integer-symbol language networks in the pinned Math AK.
/// Source ordinals are the legacy symbol-table ordinals, not Unicode or LaTeX.
/// This primitive provides next-symbol distributions, not recognition or rescore authority.
/// </summary>
public sealed class LegacyMathAkSymbolSequence
{
    public const int MaximumSymbols = 4095;
    public const int SymbolOrdinalCount = 290;
    public const int OutputCount = 241;
    public const int BoundaryId = 240;
    private const int ResourceBytes = 3685881;
    private const int PayloadBytes = 411488;
    private const string ResourceSha = "8c8d9c8ce93aa74820f30a592d77ec3fe382c930a72063d5ced48629f3c167c1";
    private readonly NativeNetwork network;
    private readonly float[] embeddings;
    private readonly int[] symbolMap;

    private LegacyMathAkSymbolSequence(bool reverse, NativeNetwork network, float[] embeddings, int[] symbolMap)
    {
        ReverseInterior = reverse;
        this.network = network;
        this.embeddings = embeddings;
        this.symbolMap = symbolMap;
    }

    public bool ReverseInterior { get; }

    /// <summary>
    /// Snapshots and authenticates the complete resource before reading any model field.
    /// This is a fixed known-resource reader, not a generic 5F parser or model selector.
    /// </summary>
    public static LegacyMathAkSymbolSequence Read(ReadOnlySpan<byte> resource, bool reverseInterior,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (resource.Length != ResourceBytes) throw new InvalidDataException("Unexpected Math AK length.");
        byte[] owned = resource.ToArray();
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!Convert.ToHexStringLower(SHA256.HashData(owned)).Equals(ResourceSha, StringComparison.Ordinal))
                throw new InvalidDataException("Unexpected Math AK identity.");
            cancellationToken.ThrowIfCancellationRequested();
            int payloadOffset = reverseInterior ? 593312 : 180512;
            int mapOffset = reverseInterior ? 1004804 : 592004;
            int cursor = payloadOffset;
            var input = new AffineWeights[4];
            var recurrent = new AffineWeights[4];
            for (int i = 0; i < 4; i++) input[i] = Matrix(100, 50, 56);
            for (int i = 0; i < 4; i++) recurrent[i] = Matrix(100, 100, 104);
            float[] pi = Vector(104, 100), pf = Vector(104, 100), po = Vector(104, 100);
            AffineWeights output = Matrix(241, 100, 104);
            float[] embedding = Vector(241 * 56, 241 * 56);
            if (cursor != payloadOffset + PayloadBytes) throw new InvalidDataException("Unexpected tensor boundary.");
            for (int row = 0; row < 241; row++)
                for (int col = 50; col < 56; col++)
                    if (embedding[row * 56 + col] != 0) throw new InvalidDataException("Nonzero embedding padding.");
            var map = new int[SymbolOrdinalCount];
            for (int i = 0; i < map.Length; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                int id = BinaryPrimitives.ReadInt32LittleEndian(owned.AsSpan(mapOffset + i * 4, 4));
                if ((uint)id >= BoundaryId) throw new InvalidDataException("Invalid mapped symbol ID.");
                map[i] = id;
            }
            cancellationToken.ThrowIfCancellationRequested();
            var layer = new LstmLayer(100, new LstmDirection(input, recurrent, pi, pf, po), null);
            var executor = new NativeNetwork(50, [layer], output,
                numericalPolicy: NativeNetworkNumericalPolicy.SymbolSequenceScalar);
            return new LegacyMathAkSymbolSequence(reverseInterior, executor, embedding, map);

            float[] Vector(int stored, int retained)
            {
                var values = new float[retained];
                for (int i = 0; i < stored; i++)
                {
                    if ((i & 255) == 0) cancellationToken.ThrowIfCancellationRequested();
                    float value = BinaryPrimitives.ReadSingleLittleEndian(owned.AsSpan(cursor, 4));
                    cursor += 4;
                    if (!float.IsFinite(value) || (i >= retained && value != 0))
                        throw new InvalidDataException("Invalid tensor value or padding.");
                    if (i < retained) values[i] = value;
                }
                return values;
            }

            AffineWeights Matrix(int rows, int width, int columns)
            {
                float[] values = Vector(checked(rows * columns), checked(rows * columns));
                for (int row = 0; row < rows; row++)
                    for (int col = width + 1; col < columns; col++)
                        if (values[row * columns + col] != 0) throw new InvalidDataException("Nonzero matrix padding.");
                return new AffineWeights(rows, width, columns, 1, values, null, null, [], [], []);
            }
        }
        finally { CryptographicOperations.ZeroMemory(owned); }
    }

    /// <summary>
    /// Copies ordinals, maps the interior (reversed only for the kind2 record), and owns
    /// [240, mapped symbols, 240]. The last ID is a target, not an extra input frame.
    /// </summary>
    public PreparedSymbols Prepare(ReadOnlySpan<int> symbolOrdinals, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (symbolOrdinals.Length > MaximumSymbols) throw new ArgumentException("Symbol sequence exceeds the library limit.", nameof(symbolOrdinals));
        int[] ordinals = symbolOrdinals.ToArray();
        foreach (int ordinal in ordinals)
            if ((uint)ordinal >= SymbolOrdinalCount) throw new ArgumentException("Invalid source symbol ordinal.", nameof(symbolOrdinals));
        var ids = new int[ordinals.Length + 2];
        ids[0] = ids[^1] = BoundaryId;
        for (int i = 0; i < ordinals.Length; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ids[i + 1] = symbolMap[ordinals[ReverseInterior ? ordinals.Length - 1 - i : i]];
        }
        cancellationToken.ThrowIfCancellationRequested();
        return new PreparedSymbols(this, ids);
    }

    /// <summary>
    /// Portable scalar FP32 LSTM using sequential softmax reduction and unclamped sigmoid.
    /// CRT exp/tanh approximation, SIMD reduction and native FTZ/DAZ equivalence are not claimed.
    /// This does not apply Math00F3C0's logarithmic rescore or select recognition candidates.
    /// </summary>
    public Prediction Evaluate(PreparedSymbols symbols, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(symbols);
        if (!ReferenceEquals(symbols.Owner, this)) throw new ArgumentException("Foreign prepared sequence.", nameof(symbols));
        cancellationToken.ThrowIfCancellationRequested();
        int frames = symbols.FrameCount;
        var features = new float[checked(frames * 50)];
        for (int frame = 0; frame < frames; frame++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            embeddings.AsSpan(symbols.Ids[frame] * 56, 50).CopyTo(features.AsSpan(frame * 50, 50));
        }
        float[] probabilities = network.Run(features, frames, cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        return new Prediction(symbols, probabilities);
    }

    public sealed class PreparedSymbols
    {
        private readonly int[] ids;
        internal LegacyMathAkSymbolSequence Owner { get; }
        internal int[] Ids => ids;
        internal PreparedSymbols(LegacyMathAkSymbolSequence owner, int[] ids) { Owner = owner; this.ids = ids; }
        public int FrameCount => ids.Length - 1;
        public int SymbolCount => ids.Length - 2;
        public int[] CopyIds() => (int[])ids.Clone();
        public int InputId(int frame) => (uint)frame < (uint)FrameCount ? ids[frame] : throw new ArgumentOutOfRangeException(nameof(frame));
        public int TargetId(int frame) => (uint)frame < (uint)FrameCount ? ids[frame + 1] : throw new ArgumentOutOfRangeException(nameof(frame));
    }

    public sealed class Prediction
    {
        private readonly float[] probabilities;
        private readonly PreparedSymbols symbols;
        internal Prediction(PreparedSymbols symbols, float[] probabilities) { this.symbols = symbols; this.probabilities = probabilities; }
        public int FrameCount => symbols.FrameCount;
        public int TargetId(int frame) => symbols.TargetId(frame);
        public float[] CopyProbabilities() => (float[])probabilities.Clone();
        public float Probability(int frame, int symbolId)
        {
            if ((uint)frame >= (uint)FrameCount) throw new ArgumentOutOfRangeException(nameof(frame));
            if ((uint)symbolId >= OutputCount) throw new ArgumentOutOfRangeException(nameof(symbolId));
            return probabilities[frame * OutputCount + symbolId];
        }
    }
}
