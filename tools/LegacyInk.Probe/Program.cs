using System.Buffers.Binary;
using System.Text.Json;
using InkRecognition;

if (args.Length == 2 && args[0] == "--score-controls")
{
    using var controls = JsonDocument.Parse(File.ReadAllBytes(args[1]));
    int count = 0;
    foreach (var test in controls.RootElement.EnumerateArray())
    {
        var scores = LegacyClassScores.Convert(Values(test.GetProperty("raw")));
        Require(scores.Probability32768.Span.SequenceEqual(Values(test.GetProperty("probability"))), "Quantized EXP probability mismatch.");
        var expected = test.GetProperty("logarithms").EnumerateArray().Select(x => x.GetSingle()).ToArray();
        for (int i = 0; i < expected.Length; i++)
            Require(Math.Abs(scores.LogScores.Span[i] - expected[i]) < .00001f, "EXP log-score mismatch.");
        Require(scores.StabilizationOffset == test.GetProperty("offset").GetInt32(), "Score stabilization mismatch.");
        count++;
    }
    Console.WriteLine($"PASS {count} EXP conversion vectors. Candidate masks, ensemble and language decoding are not tested.");
    return;
}

if (args.Length == 4 && args[0] == "--glyph-cases")
{
    var resource = File.ReadAllBytes(args[1]);
    using var cases = JsonDocument.Parse(File.ReadAllBytes(args[2]));
    using var tables = JsonDocument.Parse(File.ReadAllBytes(args[3]));
    var activations = new LegacyInkActivations(Values(tables.RootElement.GetProperty("tanh").GetProperty("values")),
        Values(tables.RootElement.GetProperty("sigmoid").GetProperty("values")));
    var root = cases.RootElement;
    var network = LegacyInkNetwork.Load(resource.AsSpan(root.GetProperty("offset").GetInt32(),
        root.GetProperty("end").GetInt32() - root.GetProperty("offset").GetInt32()));
    Require(network.InputWidth is 247 or 229 && network.OutputWidth == 180, "Unexpected isolated classifier profile.");
    int count = 0;
    foreach (var test in root.GetProperty("cases").EnumerateArray())
    {
        var points = test.GetProperty("points").EnumerateArray().Select(x =>
            new LegacyInkPoint(checked((short)x[0].GetInt32()), checked((short)x[1].GetInt32()))).ToArray();
        var features = network.InputWidth == 229 ? LegacyRasterFeatures.ExtractIsolatedInput(new LegacyInkStroke[] { new(points) })
            : LegacyStrokeFeatures.ExtractIsolatedInput(points);
        var scores = network.Evaluate(features, activations);
        int best = 0;
        for (int i = 1; i < scores.Length; i++) if (scores[i] > scores[best]) best = i;
        Require(best == test.GetProperty("expectedOutput").GetInt32(), "Isolated glyph top output mismatch.");
        Console.WriteLine($"{test.GetProperty("name").GetString()}: canonical output {best}.");
        count++;
    }
    Console.WriteLine($"PASS {count} synthetic isolated glyph cases. This is not a word/cursive accuracy evaluation.");
    return;
}

if (args.Length == 4 && args[0] is "--controls" or "--raw-controls")
{
    var resource = File.ReadAllBytes(args[1]);
    using var controls = JsonDocument.Parse(File.ReadAllBytes(args[2]));
    using var tables = JsonDocument.Parse(File.ReadAllBytes(args[3]));
    var activations = new LegacyInkActivations(Values(tables.RootElement.GetProperty("tanh").GetProperty("values")),
        Values(tables.RootElement.GetProperty("sigmoid").GetProperty("values")));
    int count = 0;
    foreach (var test in controls.RootElement.EnumerateArray())
    {
        int offset = test.GetProperty("offset").GetInt32(), end = test.GetProperty("end").GetInt32();
        var network = LegacyInkNetwork.Load(resource.AsSpan(offset, end - offset));
        var features = Values(test.GetProperty("features")).Select(x => checked((short)x)).ToArray();
        if (args[0] == "--raw-controls")
        {
            short[] extracted;
            if (test.TryGetProperty("strokes", out var strokes))
            {
                var glyph = strokes.EnumerateArray().Select(s => new LegacyInkStroke(s.EnumerateArray().Select(x =>
                    new LegacyInkPoint(checked((short)x[0].GetInt32()), checked((short)x[1].GetInt32()))).ToArray())).ToArray();
                if (test.TryGetProperty("baseline", out var baseline))
                {
                    var lineInfo = new LegacyInkBaseline(checked((short)baseline[0].GetInt32()), checked((short)baseline[1].GetInt32()));
                    extracted = network.InputWidth == 238 ? LegacyRasterFeatures.ExtractIsolatedInputWithBaseline(glyph, lineInfo)
                        : LegacyStrokeFeatures.ExtractIsolatedInputWithBaseline(glyph, lineInfo);
                }
                else
                    extracted = network.InputWidth == 229 ? LegacyRasterFeatures.ExtractIsolatedInput(glyph)
                        : LegacyStrokeFeatures.ExtractIsolatedInput(glyph);
            }
            else
            {
                var points = test.GetProperty("points").EnumerateArray().Select(x =>
                    new LegacyInkPoint(checked((short)x[0].GetInt32()), checked((short)x[1].GetInt32()))).ToArray();
                extracted = LegacyStrokeFeatures.ExtractIsolatedInput(points);
            }
            Require(extracted.SequenceEqual(features), "Raw stroke feature mismatch.");
        }
        Require(network.Evaluate(features, activations).SequenceEqual(Values(test.GetProperty("raw"))), "Raw reference mismatch.");
        Require(network.Evaluate(features, activations, true).SequenceEqual(Values(test.GetProperty("sigmoid"))), "Sigmoid reference mismatch.");
        count += 2;
    }
    Console.WriteLine(args[0] == "--raw-controls"
        ? $"PASS {count} isolated raw-stroke/network comparisons. Words, cursive segmentation and language decoding are not tested."
        : $"PASS {count} legacy supplied-vector reference comparisons. Raw handwriting is not tested.");
    return;
}
if (args.Length != 0) throw new ArgumentException("Usage: LegacyInk.Probe [--controls|--raw-controls|--glyph-cases resource.res controls.json activations.json] | [--score-controls controls.json]");

// Independently constructed networks and arbitrary monotonic lookup tables.
// No vendor weights, tables, alphabet or handwriting samples are checked in.
var hiddenTable = Enumerable.Range(0, 48).Select(x => Math.Min(1018, x * 20)).ToArray();
var outputTable = Enumerable.Range(0, 128).Select(x => Math.Min(1024, 512 + x * 4)).ToArray();
var activation = new LegacyInkActivations(hiddenTable, outputTable);
var equalScores = LegacyClassScores.Convert(new int[] { 0, 0, 0 });
Require(equalScores.Probability32768.Span.ToArray().All(x => x == 10922), "Equal EXP probabilities.");
var saturatedScores = LegacyClassScores.Convert(new int[] { 40 * 1024 * 1024, -20 * 1024 * 1024 });
Require(saturatedScores.Probability32768.Span.SequenceEqual(new int[] { 32767, 0 }) &&
    saturatedScores.StabilizationOffset > 0 && saturatedScores.LogScores.Span[1] == -4f, "Stabilized EXP path.");
var emptyScores = LegacyClassScores.Convert(new int[] { int.MinValue, int.MinValue });
Require(emptyScores.Probability32768.Span.ToArray().All(x => x == 0), "All-zero EXP path.");
Require(LegacyClassScores.Calibrate(0) == .5f && LegacyClassScores.Calibrate(10000) == 1f &&
    LegacyClassScores.Calibrate(-10000) == 0f, "Score calibration limits.");
RequireThrows<ArgumentException>(() => LegacyClassScores.Calibrate(float.NaN));
RequireThrows<ArgumentException>(() => LegacyClassScores.Convert(Array.Empty<int>()));
RequireThrows<OperationCanceledException>(() => LegacyClassScores.Convert(new int[] { 0 }, new CancellationToken(true)));
var bytes = Fixture(3, 3, 3, new sbyte[] { 0, 127, -127, 127, 0, 0 }, new byte[] { 2, 3 },
    new sbyte[] { 127, 127, -127, -127, -128, 127 }, new byte[] { 4, 5 });
var model = LegacyInkNetwork.Load(bytes);
Require(model.InputWidth == 3 && model.HiddenWidth == 2 && model.OutputWidth == 2 && model.Kind == 31, "Metadata mismatch.");
Check(model, bytes, new short[] { 1024, 1024, -1024 });
Check(model, bytes, new short[] { -1024, -1024, 1024 });
Check(model, bytes, new short[] { 0, 0, 0 });
Require(model.Evaluate(new short[] { 0, 0, 0 }, activation)[0] == 1024 * 127 * 4, "Hidden leading bias not applied.");
var before = model.Evaluate(new short[] { 1024, 1, 2 }, activation);
Array.Fill(bytes, (byte)0);
Require(before.SequenceEqual(model.Evaluate(new short[] { 1024, 1, 2 }, activation)), "Network retains mutable source.");
RequireThrows<ArgumentException>(() => model.Evaluate(new short[2], activation));
RequireThrows<OperationCanceledException>(() => model.Evaluate(new short[3], activation, false, new CancellationToken(true)));
RequireThrows<ArgumentException>(() => new LegacyInkActivations(hiddenTable.AsSpan(1), outputTable));
var reversed = hiddenTable.ToArray(); reversed[10] = reversed[9] - 1;
RequireThrows<ArgumentException>(() => new LegacyInkActivations(reversed, outputTable));
var overflowBytes = Fixture(4096, 2, 2, Enumerable.Repeat((sbyte)127, 4096).ToArray(), new byte[] { 255 },
    new sbyte[] { -128, 127 }, new byte[] { 255 });
Check(LegacyInkNetwork.Load(overflowBytes), overflowBytes, Enumerable.Repeat(short.MaxValue, 4096).ToArray());
foreach (int offset in new[] { 0, 2, 4, 8, 11, 19, 23, 31, 39 })
{
    var mutant = Fixture(3, 3, 3, new sbyte[6], new byte[] { 1, 1 }, new sbyte[6], new byte[] { 1, 1 });
    mutant[offset] ^= 255;
    RequireThrows<InvalidDataException>(() => LegacyInkNetwork.Load(mutant));
}
RequireThrows<InvalidDataException>(() => LegacyInkNetwork.Load(overflowBytes.AsSpan(0, overflowBytes.Length - 1)));
var line = new LegacyInkPoint[] { new(0, 0), new(128, 0) };
var geometry = LegacyStrokeFeatures.Extract(line);
Require(geometry.Length == 217 && geometry[0] == 1024 && geometry[1] == -1024, "Horizontal directional channels.");
Require(geometry[2] == 0 && geometry[3] == 0 && geometry[4] == 0 && geometry[5] == 0, "Balanced zero-direction channels.");
Require(geometry.AsSpan(6, 30).ToArray().All(x => x == 1024), "Physical stroke channel.");
Require(geometry.AsSpan(66, 30).ToArray().All(x => x == -16), "Centered normalized Y channel.");
Require(geometry.AsSpan(96, 29).ToArray().All(x => x == 1024), "Unit tangent X channel.");
Require(geometry.AsSpan(125, 29).ToArray().All(x => x == 0), "Unit tangent Y channel.");
Require(geometry.AsSpan(154, 28).ToArray().All(x => x == 1024), "Straight-line cosine channel.");
Require(geometry.AsSpan(182, 28).ToArray().All(x => x == 0), "Straight-line sine channel.");
Require(geometry[210] == 1024 && geometry[211] == -1022 && geometry[214] == -768 && geometry[216] == 1024, "Single-stroke shape statistics.");
Require(geometry.SequenceEqual(LegacyStrokeFeatures.Extract(new LegacyInkPoint[] { new(-128, 25), new(0, 25) })), "Translation invariance.");
Require(geometry.SequenceEqual(LegacyStrokeFeatures.Extract(new LegacyInkPoint[] { new(0, 0), new(256, 0) })), "Exact integer scale invariance.");
var isolated = LegacyStrokeFeatures.ExtractIsolatedInput(line);
Require(isolated.Length == 247 && isolated.AsSpan(217, 28).ToArray().All(x => x == -1024) && isolated[^2] == 1024 && isolated[^1] == 1024, "Isolated context and boundaries.");
RequireThrows<ArgumentException>(() => LegacyStrokeFeatures.Extract(new LegacyInkPoint[] { new(1, 1), new(1, 1) }));
RequireThrows<ArgumentException>(() => LegacyStrokeFeatures.Extract(new LegacyInkPoint[4097]));
RequireThrows<OperationCanceledException>(() => LegacyStrokeFeatures.Extract(line, new CancellationToken(true)));
var oneStroke = new LegacyInkStroke[] { new(line) };
Require(geometry.SequenceEqual(LegacyStrokeFeatures.Extract(oneStroke)), "Single/multiple-stroke APIs disagree.");
var raster = LegacyRasterFeatures.Extract(oneStroke);
Require(raster.Length == 199 && raster.AsSpan(0, 7).SequenceEqual(geometry.AsSpan(210, 7)), "Raster statistics precede pixel channels.");
Require(raster[7 + 7] == -960 && raster[7 + 6] == -1024, "Horizontal row endpoints.");
Require(raster.AsSpan(7 + 32, 15).ToArray().All(x => x == -512) && raster[7 + 32 + 15] == -576, "Vertical projection endpoint counts.");
Require(raster[7 + 64 + 7] == 768 && raster[7 + 128 + 7] == 770, "Horizontal left/right contours.");
Require(raster.AsSpan(7 + 96, 15).ToArray().All(x => x == -16) && raster[7 + 96 + 15] == 114, "Vertical top contour and empty column.");
Require(raster.SequenceEqual(LegacyRasterFeatures.Extract(new LegacyInkStroke[] {
    new(new LegacyInkPoint[] { new(-128, 25), new(0, 25) }) })), "Raster translation invariance.");
Require(LegacyRasterFeatures.ExtractIsolatedInput(oneStroke).Length == 229, "Isolated raster input width.");
var unavailableBaseline = new LegacyInkBaseline(0, -1);
var baselineTrajectory = LegacyStrokeFeatures.ExtractWithBaseline(oneStroke, unavailableBaseline);
Require(baselineTrajectory.Length == 226 && baselineTrajectory.AsSpan(217, 9).ToArray().All(x => x == 0), "Unavailable baseline channels.");
var baselineRaster = LegacyRasterFeatures.ExtractWithBaseline(oneStroke, unavailableBaseline);
Require(baselineRaster.Length == 208 && baselineRaster.AsSpan(0, 7).SequenceEqual(raster.AsSpan(0, 7)) &&
    baselineRaster.AsSpan(16, 192).SequenceEqual(raster.AsSpan(7, 192)), "Baseline raster channel order.");
Require(LegacyStrokeFeatures.ExtractIsolatedInputWithBaseline(oneStroke, unavailableBaseline).Length == 256 &&
    LegacyRasterFeatures.ExtractIsolatedInputWithBaseline(oneStroke, unavailableBaseline).Length == 238, "Baseline classifier widths.");
var lineBaseline = LegacyStrokeFeatures.ExtractWithBaseline(oneStroke, new(0, 64));
Require(lineBaseline.AsSpan(221, 3).SequenceEqual(new short[] { -1024, 1024, -1024 }), "Baseline raster regions.");
var twoStrokes = new LegacyInkStroke[] { new(line), new(new LegacyInkPoint[] { new(128, 128), new(0, 128) }) };
var multiGeometry = LegacyStrokeFeatures.Extract(twoStrokes);
Require(multiGeometry.AsSpan(6, 30).Contains((short)-1024), "Pen-up path was not emitted.");
Require(multiGeometry[214] == -512 && multiGeometry[216] < 1024, "Stroke count and physical-ink fraction.");
var translatedMulti = new LegacyInkStroke[] { new(new LegacyInkPoint[] { new(-100, 20), new(28, 20) }),
    new(new LegacyInkPoint[] { new(28, 148), new(-100, 148) }) };
Require(multiGeometry.SequenceEqual(LegacyStrokeFeatures.Extract(translatedMulti)), "Multi-stroke translation invariance.");
RequireThrows<ArgumentException>(() => LegacyStrokeFeatures.Extract(Array.Empty<LegacyInkStroke>()));
RequireThrows<ArgumentException>(() => LegacyStrokeFeatures.Extract(Enumerable.Repeat(oneStroke[0], 16).ToArray()));
RequireThrows<OperationCanceledException>(() => LegacyStrokeFeatures.Extract(twoStrokes, new CancellationToken(true)));
Console.WriteLine("PASS legacy parser, scalar overflow, signed activations, bias, immutable weights, malformed input and cancellation checks.");
Console.WriteLine("PASS raw single/multiple-stroke geometry, pen-up, directional/curvature channels, translation, scale, context and cancellation checks.");
Console.WriteLine("PASS raster projections/contours, baseline ordering, stabilized EXP conversion and confidence calibration checks.");

void Check(LegacyInkNetwork network, byte[] data, short[] input)
{
    // Separate slow long-accumulator reference; wrap only after the row reduction.
    int hidden = network.HiddenWidth + 1, outputs = network.OutputWidth + 1;
    var state = new short[hidden]; state[0] = 1024;
    int firstScales = 33 + input.Length * (hidden - 1);
    for (int row = 0; row < hidden - 1; row++)
    {
        long dot = 0;
        for (int col = 0; col < input.Length; col++) dot += (long)input[col] * (sbyte)data[33 + row * input.Length + col];
        int quantized = unchecked((int)dot * data[firstScales + row + 1]);
        long index = Math.Abs((long)quantized) / 65536;
        int value = index >= hiddenTable.Length ? 1018 : hiddenTable[(int)index];
        state[row + 1] = (short)(quantized <= 0 ? -value : value);
    }
    int second = firstScales + hidden, secondScales = second + hidden * (outputs - 1);
    var expected = new int[outputs - 1]; var sigmoid = new int[outputs - 1];
    for (int row = 0; row < outputs - 1; row++)
    {
        long dot = 0;
        for (int col = 0; col < state.Length; col++) dot += (long)state[col] * (sbyte)data[second + row * hidden + col];
        expected[row] = unchecked((int)dot * data[secondScales + row + 1]);
        long index = Math.Abs((long)expected[row]) / 32768;
        int value = index >= outputTable.Length ? 1024 : outputTable[(int)index];
        sigmoid[row] = expected[row] <= 0 ? 1024 - value : value;
    }
    Require(network.Evaluate(input, activation).SequenceEqual(expected), "Integer reference mismatch.");
    Require(network.Evaluate(input, activation, true).SequenceEqual(sigmoid), "Activation reference mismatch.");
}

static byte[] Fixture(int inputs, int hidden, int outputs, sbyte[] first, byte[] firstScales, sbyte[] second, byte[] secondScales)
{
    var data = new byte[33 + first.Length + hidden + second.Length + outputs];
    BinaryPrimitives.WriteUInt16LittleEndian(data, 0xfea0); BinaryPrimitives.WriteUInt16LittleEndian(data.AsSpan(2), 0x106);
    BinaryPrimitives.WriteInt32LittleEndian(data.AsSpan(4), data.Length - 8); data[8] = 0x12;
    BinaryPrimitives.WriteUInt16LittleEndian(data.AsSpan(9), 31);
    BinaryPrimitives.WriteInt32LittleEndian(data.AsSpan(11), inputs); BinaryPrimitives.WriteInt32LittleEndian(data.AsSpan(15), hidden);
    BinaryPrimitives.WriteInt32LittleEndian(data.AsSpan(27), outputs); BinaryPrimitives.WriteUInt16LittleEndian(data.AsSpan(31), 10);
    int p = 33;
    foreach (sbyte value in first) data[p++] = unchecked((byte)value);
    data[p++] = 1; foreach (byte value in firstScales) data[p++] = value;
    foreach (sbyte value in second) data[p++] = unchecked((byte)value);
    data[p++] = 1; foreach (byte value in secondScales) data[p++] = value;
    return data;
}
static int[] Values(JsonElement value) => value.EnumerateArray().Select(x => x.GetInt32()).ToArray();
static void Require(bool condition, string reason) { if (!condition) throw new InvalidOperationException(reason); }
static void RequireThrows<T>(Action action) where T : Exception
{
    try { action(); } catch (T) { return; }
    throw new InvalidOperationException($"Expected {typeof(T).Name}.");
}
