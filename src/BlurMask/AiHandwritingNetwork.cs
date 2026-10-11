using System.Buffers;
using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace BlurMask;

// Native C# PARSeq-S inference, adapted from PARSeq, Copyright 2022 Darwin Bautista.
// Apache License 2.0: LICENSE-AI-ENGINE.txt. Attribution: THIRD-PARTY-NOTICES.txt.
// Fixed dimensions prevent unbounded allocations; only one background inference runs at once.
internal sealed class AiHandwritingNetwork
{
    internal static readonly Lazy<AiHandwritingNetwork> Instance = new(() => new());
    internal static readonly Lazy<AiHandwritingNetwork> English = new(() => new("BlurMask.AiEnglish.bin", 32, 128, 25, 94));
    private const int D = 384, Hidden = 1536;
    private readonly int N, _height, _width, _maxLength;
    private readonly Dictionary<string, float[]> _weights = new(StringComparer.Ordinal);
    private readonly string _charset;
    private static readonly ParallelOptions Workers = new() { MaxDegreeOfParallelism = Math.Min(3, Environment.ProcessorCount) };
    private float[] W(string name) => _weights[name];
    private AiHandwritingNetwork(string resource = "BlurMask.AiHandwriting.bin", int height = 48, int width = 512, int maxLength = 100, int vocabulary = 232)
    {
        _height = height; _width = width; _maxLength = maxLength; N = height / 4 * (width / 8);
        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resource)
            ?? throw new InvalidDataException("Handwriting model is missing.");
        using var reader = new BinaryReader(stream);
        if (!reader.ReadBytes(8).AsSpan().SequenceEqual("BMPSEQ01"u8)) throw new InvalidDataException("Invalid handwriting model.");
        var count = reader.ReadInt32(); if (count != vocabulary) throw new InvalidDataException("Invalid handwriting vocabulary.");
        var chars = new char[count]; for (var i = 0; i < count; i++) chars[i] = checked((char)reader.ReadInt32());
        _charset = new(chars); count = reader.ReadInt32();
        if (count != 175) throw new InvalidDataException("Invalid handwriting parameters.");
        for (var i = 0; i < count; i++)
        {
            var length = reader.ReadInt32(); if (length is < 1 or > 100) throw new InvalidDataException("Invalid tensor name.");
            var name = Encoding.ASCII.GetString(reader.ReadBytes(length)); var size = reader.ReadInt32();
            if (size is < 1 or > D * Hidden) throw new InvalidDataException("Invalid tensor size.");
            var packed = ArrayPool<byte>.Shared.Rent(size * 2);
            try
            {
                stream.ReadExactly(packed.AsSpan(0, size * 2)); var halves = MemoryMarshal.Cast<byte, ushort>(packed.AsSpan(0, size * 2));
                var values = new float[size];
                for (var j = 0; j < size; j++) values[j] = (float)BitConverter.UInt16BitsToHalf(halves[j]);
                _weights.Add(name, values);
            }
            finally { ArrayPool<byte>.Shared.Return(packed); }
        }
        if (stream.Position != stream.Length) throw new InvalidDataException("Trailing handwriting data.");
    }

    internal float[] Encode(float[] pixels)
    {
        if (pixels.Length != _height * _width) throw new ArgumentException("Invalid handwriting image dimensions.");
        using var x = new Buffer(N * D); using var norm = new Buffer(N * D);
        using var qkv = new Buffer(N * D * 3); using var attention = new Buffer(N * D);
        using var temp = new Buffer(N * D); using var hidden = new Buffer(N * Hidden);
        var patch = W("encoder.patch_embed.proj.weight"); var bias = W("encoder.patch_embed.proj.bias"); var position = W("encoder.pos_embed");
        Parallel.For(0, N, Workers, token =>
        {
            var top = token / (_width / 8) * 4; var left = token % (_width / 8) * 8;
            for (var output = 0; output < D; output++)
            {
                var sum = bias[output];
                for (var channel = 0; channel < 3; channel++) for (var y = 0; y < 4; y++) for (var dx = 0; dx < 8; dx++)
                    sum += pixels[(top + y) * _width + left + dx] * patch[output * 96 + channel * 32 + y * 8 + dx];
                x.Values[token * D + output] = sum + position[token * D + output];
            }
        });
        for (var layer = 0; layer < 12; layer++)
        {
            var prefix = $"encoder.blocks.{layer}.";
            Norm(x.Values, norm.Values, N, prefix + "norm1", 1e-6f);
            Dense(norm.Values, qkv.Values, N, D, D * 3, prefix + "attn.qkv");
            SelfAttention(qkv.Values, attention.Values);
            Dense(attention.Values, temp.Values, N, D, D, prefix + "attn.proj");
            Add(x.Values, temp.Values, N * D);
            Norm(x.Values, norm.Values, N, prefix + "norm2", 1e-6f);
            Dense(norm.Values, hidden.Values, N, D, Hidden, prefix + "mlp.fc1");
            Gelu(hidden.Values, N * Hidden);
            Dense(hidden.Values, temp.Values, N, Hidden, D, prefix + "mlp.fc2");
            Add(x.Values, temp.Values, N * D);
        }
        var memory = new float[N * D]; Norm(x.Values, memory, N, "encoder.norm", 1e-6f); return memory;
    }
    private void SelfAttention(float[] qkv, float[] output)
    {
        Parallel.For(0, N * 6, Workers, item =>
        {
            var token = item / 6; var head = item % 6; var scores = ArrayPool<float>.Shared.Rent(N);
            try
            {
                var query = qkv.AsSpan(token * D * 3 + head * 64, 64);
                for (var key = 0; key < N; key++) scores[key] = Dot(query, qkv.AsSpan(key * D * 3 + D + head * 64, 64)) / 8;
                Softmax(scores, N);
                var destination = output.AsSpan(token * D + head * 64, 64); destination.Clear();
                for (var key = 0; key < N; key++) MultiplyAdd(destination, qkv.AsSpan(key * D * 3 + 2 * D + head * 64, 64), scores[key]);
            }
            finally { ArrayPool<float>.Shared.Return(scores); }
        });
    }
    // A single decoder layer only updates the query stream; content keys stay fixed.
    internal float[] Decode(float[] memory, int[] tokens, int[] queryPositions, bool refinement = false, (float[] Key, float[] Value)? crossCache = null)
    {
        var length = tokens.Length; var queries = queryPositions.Length;
        if (memory.Length != N * D || length < 1 || length > _maxLength + 1 || queries < 1 || queries > _maxLength + 1) throw new ArgumentException("Invalid decoder dimensions.");
        var embeddings = W("text_embed.embedding.weight"); var positions = W("pos_queries");
        using var content = new Buffer(length * D); using var normalized = new Buffer(length * D);
        using var query = new Buffer(queries * D); using var queryNorm = new Buffer(queries * D);
        for (var i = 0; i < length; i++) for (var j = 0; j < D; j++)
            content.Values[i * D + j] = embeddings[tokens[i] * D + j] * MathF.Sqrt(D) + (i == 0 ? 0 : positions[(i - 1) * D + j]);
        for (var i = 0; i < queries; i++) Array.Copy(positions, queryPositions[i] * D, query.Values, i * D, D);
        const string prefix = "decoder.layers.0.";
        Norm(content.Values, normalized.Values, length, prefix + "norm_c", 1e-5f);
        Norm(query.Values, queryNorm.Values, queries, prefix + "norm_q", 1e-5f);
        using var temp = new Buffer(queries * D);
        MultiAttention(queryNorm.Values, normalized.Values, temp.Values, queries, length, prefix + "self_attn", (q, k) =>
        {
            if (!refinement) return true; // AR receives only the prefix.
            if (k == queryPositions[q] + 1) return false; // cloze: exclude the character itself.
            for (var i = 1; i <= k; i++) if (tokens[i] == 0) return false;
            return true;
        });
        Add(query.Values, temp.Values, queries * D);
        Norm(query.Values, queryNorm.Values, queries, prefix + "norm1", 1e-5f);
        MultiAttention(queryNorm.Values, memory, temp.Values, queries, N, prefix + "cross_attn", null, crossCache);
        Add(query.Values, temp.Values, queries * D);
        Norm(query.Values, queryNorm.Values, queries, prefix + "norm2", 1e-5f);
        using var hidden = new Buffer(queries * Hidden);
        Dense(queryNorm.Values, hidden.Values, queries, D, Hidden, prefix + "linear1"); Gelu(hidden.Values, queries * Hidden);
        Dense(hidden.Values, temp.Values, queries, Hidden, D, prefix + "linear2"); Add(query.Values, temp.Values, queries * D);
        Norm(query.Values, queryNorm.Values, queries, "decoder.norm", 1e-5f);
        var logits = new float[queries * (_charset.Length + 1)];
        Dense(queryNorm.Values, logits, queries, D, _charset.Length + 1, "head"); return logits;
    }
    private void MultiAttention(float[] input, float[] context, float[] output, int queries, int keys, string prefix, Func<int, int, bool>? allow,
        (float[] Key, float[] Value)? cache = null)
    {
        using var q = new Buffer(queries * D); using var kBuffer = cache is null ? new Buffer(keys * D) : null;
        using var vBuffer = cache is null ? new Buffer(keys * D) : null;
        var k = cache?.Key ?? kBuffer!.Values; var v = cache?.Value ?? vBuffer!.Values;
        var weights = W(prefix + ".in_proj_weight"); var bias = W(prefix + ".in_proj_bias");
        Project(input, q.Values, queries, D, D, weights, bias, 0);
        if (cache is null)
        {
            Project(context, k, keys, D, D, weights, bias, D);
            Project(context, v, keys, D, D, weights, bias, D * 2);
        }
        using var joined = new Buffer(queries * D);
        Parallel.For(0, queries * 12, Workers, item =>
        {
            var qi = item / 12; var head = item % 12; var scores = ArrayPool<float>.Shared.Rent(keys);
            try
            {
                for (var ki = 0; ki < keys; ki++) scores[ki] = allow is not null && !allow(qi, ki) ? float.NegativeInfinity :
                    Dot(q.Values.AsSpan(qi * D + head * 32, 32), k.AsSpan(ki * D + head * 32, 32)) / MathF.Sqrt(32);
                Softmax(scores, keys); var destination = joined.Values.AsSpan(qi * D + head * 32, 32); destination.Clear();
                for (var ki = 0; ki < keys; ki++) MultiplyAdd(destination, v.AsSpan(ki * D + head * 32, 32), scores[ki]);
            }
            finally { ArrayPool<float>.Shared.Return(scores); }
        });
        Dense(joined.Values, output, queries, D, D, prefix + ".out_proj");
    }
    internal (string Text, float Confidence) Recognize(float[] pixels)
    {
        var memory = Encode(pixels); var tokens = new List<int> { _charset.Length + 1 };
        using var cachedKeys = new Buffer(N * D); using var cachedValues = new Buffer(N * D);
        var weights = W("decoder.layers.0.cross_attn.in_proj_weight"); var bias = W("decoder.layers.0.cross_attn.in_proj_bias");
        Project(memory, cachedKeys.Values, N, D, D, weights, bias, D);
        Project(memory, cachedValues.Values, N, D, D, weights, bias, D * 2);
        var cache = (cachedKeys.Values, cachedValues.Values);
        for (var step = 0; step < _maxLength; step++)
        {
            var logits = Decode(memory, tokens.ToArray(), [step], crossCache: cache); var next = Best(logits, 0, _charset.Length + 1);
            tokens.Add(next); if (next == 0) break;
        }
        if (tokens[^1] != 0) return ("", 0); // never replace truncated/hallucinated long output.
        // Match the published model's single bidirectional refinement pass.
        // Queries are independent in this single-layer decoder. Avoid computing dozens of
        // unused positions after the expected EOS; retain a margin for refinement expansion.
        var queryCount = Math.Min(_maxLength + 1, tokens.Count + 3);
        var refined = Decode(memory, tokens.Take(tokens.Count - 1).ToArray(), Enumerable.Range(0, queryCount).ToArray(), true, cache);
        var text = new StringBuilder(); var confidence = 0f; var count = 0;
        for (var i = 0; i < queryCount; i++)
        {
            var next = Best(refined, i * (_charset.Length + 1), _charset.Length + 1);
            if (next == 0) return (text.ToString().Trim(), count == 0 ? 0 : confidence / count);
            var logits = refined.AsSpan(i * (_charset.Length + 1), _charset.Length + 1); var total = 0f;
            foreach (var value in logits) total += MathF.Exp(value - logits[next]);
            confidence += 1 / total; count++; text.Append(_charset[next - 1]);
        }
        return ("", 0);
    }
    private static int Best(float[] values, int offset, int count)
    { var best = 0; for (var i = 1; i < count; i++) if (values[offset + i] > values[offset + best]) best = i; return best; }
    private void Dense(float[] input, float[] output, int rows, int columns, int outputs, string prefix)
        => Project(input, output, rows, columns, outputs, W(prefix + ".weight"), W(prefix + ".bias"), 0);
    private static void Project(float[] input, float[] output, int rows, int columns, int outputs, float[] weights, float[] bias, int start)
    {
        Parallel.For(0, rows, Workers, row =>
        {
            var source = input.AsSpan(row * columns, columns);
            var i = 0;
            // Four output channels share each input load. Independent accumulators avoid
            // the dependency chain of one long dot product on baseline SSE/NEON CPUs.
            for (; i + 3 < outputs; i += 4)
            {
                var a = weights.AsSpan((start + i) * columns, columns); var b = weights.AsSpan((start + i + 1) * columns, columns);
                var c = weights.AsSpan((start + i + 2) * columns, columns); var d = weights.AsSpan((start + i + 3) * columns, columns);
                var v0 = Vector<float>.Zero; var v1 = v0; var v2 = v0; var v3 = v0; var j = 0;
                for (; j <= columns - Vector<float>.Count; j += Vector<float>.Count)
                {
                    var x = new Vector<float>(source.Slice(j));
                    v0 += x * new Vector<float>(a.Slice(j)); v1 += x * new Vector<float>(b.Slice(j));
                    v2 += x * new Vector<float>(c.Slice(j)); v3 += x * new Vector<float>(d.Slice(j));
                }
                var s0 = Vector.Sum(v0) + bias[start + i]; var s1 = Vector.Sum(v1) + bias[start + i + 1];
                var s2 = Vector.Sum(v2) + bias[start + i + 2]; var s3 = Vector.Sum(v3) + bias[start + i + 3];
                for (; j < columns; j++) { s0 += source[j] * a[j]; s1 += source[j] * b[j]; s2 += source[j] * c[j]; s3 += source[j] * d[j]; }
                var offset = row * outputs + i; output[offset] = s0; output[offset + 1] = s1; output[offset + 2] = s2; output[offset + 3] = s3;
            }
            for (; i < outputs; i++) output[row * outputs + i] = bias[start + i] + Dot(source, weights.AsSpan((start + i) * columns, columns));
        });
    }
    private void Norm(float[] input, float[] output, int rows, string prefix, float epsilon)
    {
        var weight = W(prefix + ".weight"); var bias = W(prefix + ".bias");
        Parallel.For(0, rows, Workers, row =>
        {
            var offset = row * D; var mean = 0f;
            for (var i = 0; i < D; i++) mean += input[offset + i]; mean /= D;
            var variance = 0f; for (var i = 0; i < D; i++) { var delta = input[offset + i] - mean; variance += delta * delta; }
            var scale = 1 / MathF.Sqrt(variance / D + epsilon);
            for (var i = 0; i < D; i++) output[offset + i] = (input[offset + i] - mean) * scale * weight[i] + bias[i];
        });
    }
    private static float Dot(ReadOnlySpan<float> a, ReadOnlySpan<float> b)
    {
        var accumulator = Vector<float>.Zero; var i = 0;
        for (; i <= a.Length - Vector<float>.Count; i += Vector<float>.Count)
            accumulator += new Vector<float>(a.Slice(i)) * new Vector<float>(b.Slice(i));
        var sum = Vector.Sum(accumulator); for (; i < a.Length; i++) sum += a[i] * b[i]; return sum;
    }
    private static void MultiplyAdd(Span<float> target, ReadOnlySpan<float> source, float scalar)
    {
        var factor = new Vector<float>(scalar); var i = 0;
        for (; i <= target.Length - Vector<float>.Count; i += Vector<float>.Count)
            (new Vector<float>(target.Slice(i)) + new Vector<float>(source.Slice(i)) * factor).CopyTo(target.Slice(i));
        for (; i < target.Length; i++) target[i] += source[i] * scalar;
    }
    private static void Softmax(float[] values, int count)
    {
        var max = float.NegativeInfinity; for (var i = 0; i < count; i++) max = Math.Max(max, values[i]);
        var sum = 0f; for (var i = 0; i < count; i++) { values[i] = MathF.Exp(values[i] - max); sum += values[i]; }
        for (var i = 0; i < count; i++) values[i] /= sum;
    }
    private static void Add(float[] target, float[] source, int count)
    { for (var i = 0; i < count; i++) target[i] += source[i]; }
    private static void Gelu(float[] values, int count)
    {
        // erf approximation for PyTorch GELU (maximum absolute error < 2e-7).
        for (var i = 0; i < count; i++)
        {
            var value = values[i]; var x = MathF.Abs(value) / MathF.Sqrt(2); var t = 1 / (1 + .3275911f * x);
            var erf = 1 - (((((1.061405429f * t - 1.453152027f) * t) + 1.421413741f) * t - .284496736f) * t + .254829592f) * t * MathF.Exp(-x * x);
            values[i] = value * .5f * (1 + MathF.CopySign(erf, value));
        }
    }
    private sealed class Buffer(int count) : IDisposable
    {
        internal float[] Values { get; } = ArrayPool<float>.Shared.Rent(count);
        public void Dispose() => ArrayPool<float>.Shared.Return(Values);
    }
}
