using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text.Json;

namespace InkRecognition;

/// <summary>Checked portable snapshot of the inspected engine's ordinary256-float grouping table.</summary>
public sealed class LegacyCursiveSeparationTable
{
    private const string Hash = "b23ed52a3a10808eab597a8f371a7bbb66bde2fab5fe2666431b1b192abc1baf";
    private readonly float[] values;
    public ReadOnlyMemory<float> Values => values;
    private LegacyCursiveSeparationTable(float[] values) { this.values = values; }

    /// <summary>Data is separate from implementation code and has engine provenance; no vendor DLL is loaded.</summary>
    public static LegacyCursiveSeparationTable Load(ReadOnlySpan<byte> profile, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (profile.Length is < 2 or > 32 * 1024) throw new InvalidDataException("Separation profile outside bounds.");
        try
        {
            using var document = JsonDocument.Parse(profile.ToArray(), new JsonDocumentOptions { MaxDepth = 4 });
            var root = document.RootElement;
            if (root.GetProperty("schemaVersion").GetInt32() != 1 ||
                root.GetProperty("profile").GetString() != "legacy-ru-lite-separation-v1" ||
                root.GetProperty("sha256").GetString() != Hash)
                throw new InvalidDataException("Unsupported separation table profile.");
            var array = root.GetProperty("values");
            if (array.GetArrayLength() != 256) throw new InvalidDataException("Expected256 separation table values.");
            var values = new float[256]; var bytes = new byte[1024];
            for (int i = 0; i < values.Length; i++)
            {
                cancellationToken.ThrowIfCancellationRequested(); values[i] = array[i].GetSingle();
                if (!float.IsFinite(values[i]) || values[i] > 0) throw new InvalidDataException("Invalid separation log probability.");
                BinaryPrimitives.WriteSingleLittleEndian(bytes.AsSpan(i * 4), values[i]);
            }
            if (!SHA256.HashData(bytes).AsSpan().SequenceEqual(Convert.FromHexString(Hash)))
                throw new InvalidDataException("Separation float-byte fingerprint mismatch.");
            return new(values);
        }
        catch (Exception ex) when (ex is JsonException or KeyNotFoundException or InvalidOperationException or FormatException or OverflowException)
        { throw new InvalidDataException("Invalid separation table profile.", ex); }
    }
}
