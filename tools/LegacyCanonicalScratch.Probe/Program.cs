using System.Security.Cryptography;
using System.Text;
using InkRecognition;

if (args.Length != 2) throw new ArgumentException("Pass portable Unicode model and official pinned NormalizationTest9.");
using var stream = File.OpenRead(args[0]); var model = LegacyNativeUnicodeNormalizationData.Load(stream);
byte[] corpus = File.ReadAllBytes(args[1]);
if (Convert.ToHexString(SHA256.HashData(corpus)).ToLowerInvariant() != "2d48d848656b3cf889df59980ab13551988950c4ca8c5190a17c691a17f8b9bb")
    throw new InvalidDataException("Pinned official Unicode9 corpus required.");
int cases = 0, nativeDifferences = 0;
foreach (string raw in Encoding.UTF8.GetString(corpus).Split('\n'))
{
    string line = raw.Split('#')[0].Trim(); if (line.Length == 0 || line[0] == '@') continue;
    var fields = line.Split(';').Take(5).Select(s => s.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(v => Convert.ToInt32(v, 16)).ToArray()).ToArray();
    for (int i = 0; i < fields.Length; i++)
    {
        int[] input = fields[i], expected = fields[i < 3 ? 1 : 3];
        int[] actual = LegacyNativeCanonicalScratch.PrepareDecodedCodePoints(input, model);
        if (!actual.SequenceEqual(expected))
        {
            //These two pinned SERVICE refusals are checked native deviations, not fixture-specific OCR fixes.
            if (input.Length != 1 || input[0] is not (0x37e or 0x387) || !actual.SequenceEqual(input))
                throw new Exception("Unexpected difference from official Unicode9 canonical corpus: "+string.Join(',', input.Select(v=>v.ToString("X"))));
            nativeDifferences++;
        }
        cases++;
    }
}
if (cases != 93610 || nativeDifferences != 2) throw new Exception("Unexpected official corpus cardinality/native exception count.");
var own = new (int[] Input, int[] Expected)[] {
    ([0x61, 0x301], [0xe1]), ([0x435, 0x308], [0x451]), ([0x438, 0x306], [0x439]),
    ([0x41, 0x315, 0x300], [0xc0, 0x315]), ([0x61, 0x590, 0x301], [0x590, 0xe1]),
    ([0x590, 0x301, 0x61], [0x590, 0x301, 0x61]), ([0xd800, 0xfffe, 0xffff], [0x1a, 0x1a, 0x1a]),
    ([0x1100, 0x1161, 0x11a8], [0xac01]), ([0x1100, 0x301, 0x1161], [0x1100, 0x301, 0x1161]) };
foreach (var c in own)
    if (!LegacyNativeCanonicalScratch.PrepareDecodedCodePoints(c.Input, model).SequenceEqual(c.Expected)) throw new Exception("Independent native gap/substitution/Hangul control failed.");
byte[] inputBytes = Encoding.UTF8.GetBytes("\ufeffe\u0301 Ё\r\n\ufffe\uffff");
if (!LegacyNativeCanonicalScratch.PrepareUtf8Scratch(inputBytes, model).AsSpan().SequenceEqual("é Ё\r\n\u001a\u001a"u8))
    throw new Exception("Actual UTF8 iterator BOM/substitution scratch policy mismatch.");
var ordinary = LegacyNativeOrdinaryGlyphUtf8.CompleteWithoutCircledStart(inputBytes);
if (!ordinary.AsSpan().SequenceEqual(inputBytes)) throw new Exception("Scratch was incorrectly written back to the ordinary emitter.");
int[] values = [0x435, 0x308]; var output = LegacyNativeCanonicalScratch.PrepareDecodedCodePoints(values, model); values.AsSpan().Clear(); output.AsSpan().Clear();
if (!LegacyNativeCanonicalScratch.PrepareDecodedCodePoints([0x435, 0x308], model).SequenceEqual(new int[] { 0x451 })) throw new Exception("Canonical scratch aliases source/model storage.");
Reject(() => LegacyNativeCanonicalScratch.PrepareDecodedCodePoints([-1], model));
Reject(() => LegacyNativeCanonicalScratch.PrepareDecodedCodePoints([0x110000], model));
Reject(() => LegacyNativeCanonicalScratch.PrepareDecodedCodePoints(new int[65537], model));
Reject(() => LegacyNativeCanonicalScratch.PrepareDecodedCodePoints(Enumerable.Repeat(0x301, 2000).ToArray(), model));
Reject(() => LegacyNativeCanonicalScratch.PrepareUtf8Scratch([0xff], model));
Reject(() => LegacyNativeCanonicalScratch.PrepareUtf8Scratch(new byte[1_048_577], model));
using var canceled = new CancellationTokenSource(); canceled.Cancel();
Canceled(() => LegacyNativeCanonicalScratch.PrepareDecodedCodePoints([], model, canceled.Token));
Canceled(() => LegacyNativeCanonicalScratch.PrepareUtf8Scratch([], model, canceled.Token));
Console.WriteLine($"PASS {cases} official Unicode9 corpus projections, exactly{nativeDifferences} proven native Greek deviations,9 independent gap/substitution/Hangul cases +UTF8 BOM policy, owned outputs/budgets/cancellation. Temporary scratch only; no general ordinary writeback.");

static void Reject(Action action) { try { action(); } catch (ArgumentException) { return; } catch (InvalidDataException) { return; } throw new Exception("Invalid/over-budget scratch accepted."); }
static void Canceled(Action action) { try { action(); } catch (OperationCanceledException) { return; } throw new Exception("Cancellation ignored."); }
