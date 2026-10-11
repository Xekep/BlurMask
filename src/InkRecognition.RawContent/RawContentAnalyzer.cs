using System.Security.Cryptography;
using System.Text;
using InkRecognition.Math;

namespace InkRecognition.RawContent;

// The category order is part of the version1 model profile, not a heuristic.
public enum InkCategory
{
    Connector, CrossOut, Filling, FreeDrawing, Grouping, Math, PlotAxis,
    Scratch, Separator, Shape, StrikeThrough, TableGrid, Text, Underline
}
public sealed record InkClassification(int StrokeIndex, InkCategory Category, float Confidence);
public sealed record InkRegion(InkCategory Category, IReadOnlyList<int> StrokeIndices, float Confidence);
public sealed record RawContentResult(IReadOnlyList<InkClassification> Strokes, IReadOnlyList<InkRegion> Regions);

/// <summary>Experimental, language-independent stroke analyzer. Text/math decoders remain separate.</summary>
public sealed class RawContentAnalyzer : IDisposable
{
    private readonly OrtRuntime _runtime;
    private readonly OrtSession _session;
    private readonly object _gate = new();
    private bool _disposed;
    public string RuntimeVersion => _runtime.Version;
    private RawContentAnalyzer(ReadOnlyMemory<byte> graph, string? runtimePath)
    {
        _runtime = new(runtimePath);
        try { _session = _runtime.CreateSession(graph, ["node_features", "edge_index", "edge_features"], ["node_classes", "edge_connexity"], threads: 1); }
        catch { _runtime.Dispose(); throw; }
    }
    /// <summary>Loads a caller-supplied .ikrraw model pack. Retains no vendor runtime dependency.</summary>
    public static RawContentAnalyzer Load(Stream source, string? nativeRuntimePath = null)
    {
        ArgumentNullException.ThrowIfNull(source);
        using var r = new BinaryReader(source, Encoding.UTF8, leaveOpen: true);
        if (!r.ReadBytes(8).AsSpan().SequenceEqual("IKRRAW01"u8) || r.ReadUInt32() != 1)
            throw new InvalidDataException("Unsupported raw-content pack.");
        uint length = r.ReadUInt32();
        if (length is < 32 or > 16 * 1024 * 1024) throw new InvalidDataException("Invalid raw-content pack size.");
        var hash = r.ReadBytes(32); var body = r.ReadBytes((int)length);
        if (hash.Length != 32 || body.Length != length || !CryptographicOperations.FixedTimeEquals(hash, SHA256.HashData(body)) || source.ReadByte() != -1)
            throw new InvalidDataException("Raw-content pack length, checksum or trailing data mismatch.");
        using var payload = new BinaryReader(new MemoryStream(body, writable: false));
        if (payload.ReadUInt32() != 11 || payload.ReadUInt32() != 16 || payload.ReadUInt32() != 111 || payload.ReadUInt32() != 14 || payload.ReadUInt32() != 1)
            throw new InvalidDataException("Unsupported raw-content feature/category profile.");
        uint bytes = payload.ReadUInt32();
        if (bytes < 8 || bytes != length - 24 || !body.AsSpan(28, 4).SequenceEqual("ORTM"u8))
            throw new InvalidDataException("Invalid raw-content ORT graph.");
        return new(body.AsMemory(24, (int)bytes), nativeRuntimePath);
    }

    public RawContentResult Analyze(IReadOnlyList<InkStroke> strokes, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (_gate)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            var graph = RawContentFeatures.Create(strokes, cancellationToken);
            if (strokes.Count == 0) return new([], []);
            using var nodes = _runtime.Tensor(graph.Nodes, strokes.Count, 11);
            using var edges = _runtime.Tensor(graph.Edges, 2, graph.EdgeCount);
            using var features = _runtime.Tensor(graph.EdgeFeatures, graph.EdgeCount, 16);
            using var run = _runtime.CreateRun(cancellationToken);
            OrtTensor[] outputs;
            try { outputs = _session.Run(run, nodes, edges, features); }
            catch (InvalidOperationException) when (cancellationToken.IsCancellationRequested) { throw new OperationCanceledException(cancellationToken); }
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                var (classes, shape) = outputs[0].Floats(); var (links, edgeShape) = outputs[1].Floats();
                if (!shape.SequenceEqual(new long[] { strokes.Count, 14 }) || !edgeShape.SequenceEqual(new long[] { graph.EdgeCount, 1 }))
                    throw new InvalidDataException("Unexpected raw-content output shape.");
                return RawContentSegmentation.Decode(classes, links, graph.Edges, strokes.Count);
            }
            finally { foreach (var output in outputs) output.Dispose(); }
        }
    }
    public void Dispose()
    {
        lock (_gate) { if (_disposed) return; _disposed = true; _session.Dispose(); _runtime.Dispose(); }
    }
}

internal static class RawContentSegmentation
{
    internal static RawContentResult Decode(float[] logits, float[] links, long[] edges, int count)
    {
        var predictions = new InkClassification[count]; var parent = Enumerable.Range(0, count).ToArray();
        for (int i = 0; i < count; i++)
        {
            var row = logits.AsSpan(i * 14, 14); var winner = 0;
            for (int j = 1; j < 14; j++) if (row[j] > row[winner]) winner = j;
            double sum = 0;
            foreach (var value in row) sum += System.Math.Exp(value - row[winner]);
            predictions[i] = new(i, (InkCategory)winner, (float)(1 / sum));
        }
        int Find(int n) { while (parent[n] != n) { parent[n] = parent[parent[n]]; n = parent[n]; } return n; }
        var directed = new HashSet<(int, int)>();
        // Positive logits mean sigmoid > .5. Require both directions and confident,
        // matching text/math classes; drawings and decorations are never converted.
        for (int k = 0; k < links.Length; k++)
        {
            var a = (int)edges[k]; var b = (int)edges[k + links.Length];
            if (a != b && links[k] > 0) directed.Add((a, b));
        }
        foreach (var (a, b) in directed)
            if (a < b && directed.Contains((b, a)) && predictions[a].Category == predictions[b].Category &&
                predictions[a].Category is InkCategory.Text or InkCategory.Math && predictions[a].Confidence >= .85f && predictions[b].Confidence >= .85f)
                parent[Find(b)] = Find(a);
        var regions = predictions.Where(p => p.Confidence >= .85f && p.Category is InkCategory.Text or InkCategory.Math)
            .GroupBy(p => Find(p.StrokeIndex)).Select(group => new InkRegion(group.First().Category,
                group.Select(p => p.StrokeIndex).Order().ToArray(), group.Min(p => p.Confidence))).ToArray();
        return new(predictions, regions);
    }
}
