using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using InkRecognition;

if (args.Length != 5) throw new ArgumentException("Pass publication model, combining model, original controls, official Unicode9 GraphemeBreakTest and pinned RU resource.");
using var dataStream = File.OpenRead(args[0]); var data = LegacyNativeTextPublicationData.Load(dataStream);
using var cccStream = File.OpenRead(args[1]); var ccc = LegacyNativeUnicodeNormalizationData.Load(cccStream);
using var controls = JsonDocument.Parse(File.ReadAllBytes(args[2]));
using var pool = new LegacyNativeUtf8StringPool(); int own = 0;
foreach (var row in controls.RootElement.GetProperty("cases").EnumerateArray())
{
    byte[] bytes = Convert.FromHexString(row.GetProperty("bytes").GetString()!);
    int count = LegacyNativeTextGlyphPublication.ReadFirstClusterScalarCount(bytes, data, ccc);
    if (count != row.GetProperty("count").GetInt32()) throw new Exception($"First-cluster mismatch for {Convert.ToHexString(bytes)}: {count}.");
    var result = LegacyNativeTextGlyphPublication.Publish(pool, row.GetProperty("glyph").GetUInt16(), bytes, data, ccc, row.GetProperty("seed").GetUInt32());
    if (result.Flags != row.GetProperty("flags").GetUInt32() || result.ByteCount != bytes.Length || !result.Text.ReadBytes().AsSpan().SequenceEqual(bytes))
        throw new Exception("Publication flags/bytes differ.");
    if (!ReferenceEquals(result.Text, pool.Intern(bytes))) throw new Exception("Interned interval identity changed.");
    byte[] copy = result.Text.ReadBytes(); copy.AsSpan().Clear();
    if (!result.Text.ReadBytes().AsSpan().SequenceEqual(bytes)) throw new Exception("Interned bytes alias caller.");
    own++;
}
for (int i = 0; i < 1024; i++)
{
    byte[] bytes = Encoding.UTF8.GetBytes($"collision-independent-{i}\0suffix");
    var a = pool.Intern(bytes); var b = pool.Intern(bytes.AsSpan(0, bytes.Length - 1));
    if (ReferenceEquals(a, b) || a.Identity == b.Identity) throw new Exception("Length-based pool key collapsed.");
    bytes.AsSpan().Fill(42); if (a.ReadBytes()[0] != 'c') throw new Exception("Pool failed to own source.");
}
using (var other = new LegacyNativeUtf8StringPool())
    if (ReferenceEquals(pool.Intern([]), other.Intern([]))) throw new Exception("Foreign pool aliases.");
var disposable = new LegacyNativeUtf8StringPool(); var expired = disposable.Intern([0]); disposable.Dispose();
try { expired.ReadBytes(); throw new Exception("Expired string accepted."); } catch (ObjectDisposedException) { }
using var cancel = new CancellationTokenSource(); cancel.Cancel();
try { LegacyNativeTextGlyphPublication.Publish(pool, 0, [], data, ccc, cancellationToken: cancel.Token); throw new Exception("Cancellation ignored."); } catch (OperationCanceledException) { }
Reject(() => LegacyNativeTextGlyphPublication.Publish(pool, 0, [0xc0, 0x80], data, ccc));
Reject(() => LegacyNativeTextGlyphPublication.Publish(pool, 0, new byte[1_048_577], data, ccc));
Reject(() => LegacyNativeTextGlyphPublication.Publish(pool, 0, new byte[65537], data, ccc));
var damaged = File.ReadAllText(args[0]).Replace("[65,90,2]", "[65,90,3]", StringComparison.Ordinal);
if (damaged == File.ReadAllText(args[0])) throw new Exception("Corrupt model fixture missed.");
Reject(() => LegacyNativeTextPublicationData.Load(new MemoryStream(Encoding.UTF8.GetBytes(damaged))));

// Public corpus is independent and untailored. Differences are reported, not goldenized into replacements.
if (Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(args[3]))).ToLowerInvariant() != "c76e7ed44a602f85161758ec54d2bcbcb7baf38f97720605a3e81ce02af47061")
    throw new Exception("Pinned official Unicode9 grapheme corpus required.");
int corpus = 0, matches = 0, invalidScalars = 0; var mismatches = new List<string>();
foreach (string line in File.ReadLines(args[3]))
{
    string code = line.Split('#')[0].Trim(); if (code.Length == 0) continue;
    var tokens = code.Split(' ', StringSplitOptions.RemoveEmptyEntries); var values = new List<int>(); int expected = -1;
    foreach (string t in tokens) { if (t == "÷") { if (values.Count > 0 && expected < 0) expected = values.Count; } else if (t != "×") values.Add(Convert.ToInt32(t, 16)); }
    if (values.Any(v => v is >= 0xd800 and <= 0xdfff)) { invalidScalars++; continue; }
    byte[] bytes = Encoding.UTF8.GetBytes(string.Concat(values.Select(char.ConvertFromUtf32)));
    int actual = LegacyNativeTextGlyphPublication.ReadFirstClusterScalarCount(bytes, data, ccc);
    if (actual == expected) matches++; else mismatches.Add($"{string.Join(' ',values.Select(v=>v.ToString("X")))}: public={expected}, native={actual}");
    corpus++;
}
File.WriteAllLines(Path.ChangeExtension(args[2], ".public-differences.txt"), mismatches);
if (corpus != 744 || matches != 683 || mismatches.Count != 61 || invalidScalars != 78)
    throw new Exception("Independent public first-cluster audit changed; inspect native/default tailoring evidence.");
byte[] res = File.ReadAllBytes(args[4]);
if (Convert.ToHexString(SHA256.HashData(res)).ToLowerInvariant() != "7a5f08809e81fb4a2d4c78b3efd251d0986503b0d30d21de3695dab1f8a89978") throw new Exception("Pinned RU data required.");
var alphabet = LegacyInkAlphabet.Load(res.AsSpan(99), 0x103); int actualLabels = 0, refused = 0;
for (int label = 0; label < alphabet.LabelCount; label++)
{
    if (!alphabet.TryGetText((ushort)label, out var text)) continue;
    if (text.Contains("\U000f0000", StringComparison.Ordinal)) { refused++; continue; }
    byte[] emitted = LegacyNativeOrdinaryGlyphUtf8.Emit(new(0,(ushort)label), alphabet);
    var record = LegacyNativeTextGlyphPublication.Publish(pool, 0x4321, emitted, data, ccc);
    if (record.ByteCount != Encoding.UTF8.GetByteCount(text) || !record.Text.ReadBytes().AsSpan().SequenceEqual(emitted) || (record.Flags & 1023) != (0x4321 & 1023))
        throw new Exception("Actual RU emitted label->published source record differs.");
    actualLabels++;
}
Console.WriteLine($"PASS {own} original cluster/publication controls, 1024 interval/owner/lifetime controls, {actualLabels} actual RU alphabet emissions->source records/{refused} circled marker scoped out. Public Unicode9 first-cluster audit {matches}/{corpus}; {mismatches.Count} native tailoring differences/{invalidScalars} non-scalar corpus rows out of UTF8 scope. No decoded-winner/source endpoint claim.");
static void Reject(Action action) { try { action(); } catch (ArgumentException) { return; } catch (InvalidDataException) { return; } catch (NotSupportedException) { return; } throw new Exception("Invalid publication/model accepted."); }
