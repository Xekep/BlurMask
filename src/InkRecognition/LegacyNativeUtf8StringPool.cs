namespace InkRecognition;

/// <summary>Bounded portable equivalent of builtin3E0315e0/030e60 byte-interval interning; identities belong to this owner.</summary>
public sealed class LegacyNativeUtf8StringPool : IDisposable
{
    private readonly Dictionary<string, LegacyNativeInternedUtf8> _entries = new(StringComparer.Ordinal);
    private bool _disposed;
    private long _bytes;
    public int Count => _entries.Count;
    public LegacyNativeInternedUtf8 Intern(ReadOnlySpan<byte> bytes, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this); cancellationToken.ThrowIfCancellationRequested();
        if (bytes.Length > 1_048_576) throw new ArgumentOutOfRangeException(nameof(bytes));
        string key = Convert.ToHexString(bytes);
        if (_entries.TryGetValue(key, out var existing)) return existing;
        if (_entries.Count >= 65536 || _bytes + bytes.Length + 1 > 33_554_432)
            throw new InvalidOperationException("Bounded string pool budget exceeded.");
        var value = new LegacyNativeInternedUtf8(this, _entries.Count + 1, bytes.ToArray());
        _entries.Add(key, value); _bytes += bytes.Length + 1; return value;
    }
    internal void Validate(LegacyNativeInternedUtf8 value)
    { ObjectDisposedException.ThrowIf(_disposed, this); if (!ReferenceEquals(value.Owner, this)) throw new ArgumentException("Foreign interned-string owner.", nameof(value)); }
    public void Dispose() { _disposed = true; _entries.Clear(); _bytes = 0; }
}

/// <summary>Opaque stable owner-local identity, not a fabricated native pointer. Original interval bytes are owned.</summary>
public sealed class LegacyNativeInternedUtf8
{
    internal LegacyNativeUtf8StringPool Owner { get; }
    private readonly byte[] _bytes;
    public int Identity { get; }
    public int ByteCount => _bytes.Length;
    internal LegacyNativeInternedUtf8(LegacyNativeUtf8StringPool owner, int identity, byte[] bytes)
    { Owner = owner; Identity = identity; _bytes = bytes; }
    public byte[] ReadBytes() { Owner.Validate(this); return (byte[])_bytes.Clone(); }
    internal ReadOnlySpan<byte> ReadSpan() { Owner.Validate(this); return _bytes; }
}
