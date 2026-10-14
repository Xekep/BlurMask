using System.Security.Cryptography;
using System.Text;
using InkRecognition;

if (args.Length != 3) throw new ArgumentException("Pass portable model, pinned UnicodeData9 and DerivedNormalizationProps9.");
using var stream = File.OpenRead(args[0]); var model = LegacyNativeUnicodeNormalizationData.Load(stream);
byte[] ucd = File.ReadAllBytes(args[1]), derived = File.ReadAllBytes(args[2]);
Pin(ucd, "68dfc414d28257b9b5d6ddbb8b466c768c00ebdf6cbf7784364a9b6cad55ee8f");
Pin(derived, "02d8e12cea7c61a9f3cc5fbf8eaceadf8da23fe83d60c64cf647088830b810cc");
var excluded = new HashSet<int>();
foreach (string raw in Encoding.UTF8.GetString(derived).Split('\n'))
{
    string content = raw.Split('#')[0].Trim(); if (content.Length == 0) continue;
    var fields = content.Split(';').Select(s => s.Trim()).ToArray(); if (fields[1] != "Full_Composition_Exclusion") continue;
    var interval = fields[0].Split(".."); int lo = Hex(interval[0]), hi = Hex(interval[^1]);
    for (int cp = lo; cp <= hi; cp++) excluded.Add(cp);
}
int classes = 0, canonical = 0, composed = 0, first = -1, rangeClass = -1;
var mapping = new Dictionary<int, int[]>();
foreach (string line in Encoding.UTF8.GetString(ucd).Split('\n'))
{
    if (line.Trim().Length == 0) continue;
    var f = line.TrimEnd('\r').Split(';'); int cp = Hex(f[0]), expectedClass = int.Parse(f[3]);
    if (model.ReadCombiningClass(cp) != expectedClass) throw new Exception("Assigned native/public CCC mismatch."); classes++;
    if (f[1].EndsWith(", First>")) { first = cp; rangeClass = expectedClass; }
    else if (f[1].EndsWith(", Last>"))
    {
        if (first < 0 || rangeClass != expectedClass) throw new Exception("Unexpected public assigned interval.");
        for (int q = first + 1; q < cp; q++)
        { if (model.ReadCombiningClass(q) != expectedClass) throw new Exception("Public assigned interval/native CCC mismatch."); classes++; }
        first = -1;
    }
    if (f[5].Length == 0 || f[5][0] == '<') continue;
    int[] parts = f[5].Split(' ').Select(Hex).ToArray(); mapping.Add(cp, parts);
    bool found = model.TryReadDirectCanonicalDecomposition(cp, out var actual);
    if (cp is 0x37e or 0x387)
    { if (found) throw new Exception("Proven native decomposition refusal was lost."); }
    else if (!found || !actual.SequenceEqual(parts)) throw new Exception("Public canonical decomposition/native direct getter mismatch.");
    canonical++;
    if (parts.Length == 2 && !excluded.Contains(cp))
    {
        if (!model.TryCompose(parts[0], parts[1], out int result) || result != cp) throw new Exception("Public canonical composition/native getter mismatch.");
        composed++;
    }
}
for (int syllable = 0; syllable < 11172; syllable++)
{
    //Independent complete L/V/T cross product, not labels fitted to recognition.
    int leading = 0x1100 + syllable / 588, vowel = 0x1161 + syllable / 28 % 21, trailing = syllable % 28;
    int[] expected = trailing == 0 ? [leading, vowel] : [leading, vowel, 0x11a7 + trailing];
    if (!model.TryReadDirectCanonicalDecomposition(0xac00 + syllable, out var actual) || !actual.SequenceEqual(expected))
        throw new Exception("Complete Hangul direct decomposition mismatch.");
    if (!model.TryCompose(leading, vowel, out int lv) || lv != 0xac00 + syllable - trailing)
        throw new Exception("Hangul L/V composition mismatch.");
    if (trailing != 0 && (!model.TryCompose(lv, 0x11a7 + trailing, out int lvt) || lvt != 0xac00 + syllable))
        throw new Exception("Hangul LV/T composition mismatch.");
}
if (model.ReadCombiningClass(0x590) != -1 || model.ReadCombiningClass(0x378) != 0 || model.ReadCombiningClass(0x10ffff) != -1)
    throw new Exception("Native gap/-1 versus covered-unassigned0 semantics mismatch.");
if (model.TryCompose('A', 'Z', out int noPair) || noPair != -1 || model.TryReadDirectCanonicalDecomposition('A', out var empty) || empty.Length != 0)
    throw new Exception("Absent native mapping outcome changed.");
model.TryReadDirectCanonicalDecomposition(0x451, out var owned); owned.AsSpan().Clear();
if (!model.TryReadDirectCanonicalDecomposition(0x451, out var again) || !again.SequenceEqual(new int[] { 0x435, 0x308 }))
    throw new Exception("Decomposition arrays alias model data.");
Reject(() => model.ReadCombiningClass(-1)); Reject(() => model.TryCompose(0, 0x110000, out _));
Reject(() => model.TryReadDirectCanonicalDecomposition(-1, out _));
byte[] modelBytes = File.ReadAllBytes(args[0]);
string text = Encoding.UTF8.GetString(modelBytes);
Reject(() => LegacyNativeUnicodeNormalizationData.Load(new MemoryStream(Encoding.UTF8.GetBytes(text.Replace("9.0.0", "9.0.1")))));
Reject(() => LegacyNativeUnicodeNormalizationData.Load(new MemoryStream(Encoding.UTF8.GetBytes(text.Replace("[0,767,0]", "[0,767,1]")))));
Reject(() => LegacyNativeUnicodeNormalizationData.Load(new MemoryStream(new byte[1_048_577])));
Reject(() => LegacyNativeUnicodeNormalizationData.Load(new MemoryStream(modelBytes.AsSpan(0, modelBytes.Length - 2).ToArray())));
Console.WriteLine($"PASS portable exact table checksum, {classes} public assigned CCC entries/ranges, {canonical} canonical rows (including2 native exceptions), {composed} public excluded composition pairs,11172 full Hangul syllables; native gap behavior, ownership and corrupt model refusal. Stream normalizer still separate.");

static int Hex(string value) => Convert.ToInt32(value, 16);
static void Pin(byte[] bytes, string sha) { if (Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant() != sha) throw new InvalidDataException("Pinned official UCD bytes differ."); }
static void Reject(Action action) { try { action(); } catch (ArgumentException) { return; } catch (InvalidDataException) { return; } catch (System.Text.Json.JsonException) { return; }
    throw new Exception("Invalid/out-of-scope Unicode model accepted."); }
