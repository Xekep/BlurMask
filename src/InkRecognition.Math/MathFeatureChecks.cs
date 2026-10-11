namespace InkRecognition.Math;

internal readonly record struct MathFeatureCheckResult(string Name, int Frames, float MaximumError);

/// <summary>Independent Python fixture comparisons. Invoked by development probes, never during recognition.</summary>
internal static class MathFeatureChecks
{
    // IMFCHK1: ASCII7; int32 case count; UTF8 string; strokes/int32 points/float32xy;
    // float32 Scale; int32 frames; int64 ends; frames*26 little-endian float32 values.
    internal static IReadOnlyList<MathFeatureCheckResult> Compare(Stream fixture, float tolerance = 2e-5f)
    {
        ArgumentNullException.ThrowIfNull(fixture);
        if (!float.IsFinite(tolerance) || tolerance < 0) throw new ArgumentOutOfRangeException(nameof(tolerance));
        using var reader = new BinaryReader(fixture, System.Text.Encoding.UTF8, leaveOpen: true);
        if (!reader.ReadBytes(7).AsSpan().SequenceEqual("IMFCHK1"u8)) throw new InvalidDataException("Invalid feature fixture.");
        var cases = reader.ReadInt32();
        if (cases is < 1 or > 100) throw new InvalidDataException("Invalid fixture case count.");
        var results = new List<MathFeatureCheckResult>(cases);
        for (var c = 0; c < cases; c++)
        {
            var name = reader.ReadString(); var strokeCount = reader.ReadInt32();
            if (strokeCount is < 1 or > MathPreprocessor.MaxStrokes) throw new InvalidDataException("Invalid stroke count.");
            var strokes = new InkStroke[strokeCount];
            for (var s = 0; s < strokeCount; s++)
            {
                var count = reader.ReadInt32();
                if (count is < 1 or > MathPreprocessor.MaxPoints) throw new InvalidDataException("Invalid point count.");
                var points = new InkPoint[count];
                for (var p = 0; p < count; p++) points[p] = new(reader.ReadSingle(), reader.ReadSingle());
                strokes[s] = new(points);
            }
            var scale = reader.ReadSingle(); var frames = reader.ReadInt32();
            var actual = MathPreprocessor.Process(strokes);
            if (!float.IsFinite(scale) || actual.Frames != frames || MathF.Abs(actual.Scale - scale) > tolerance)
                throw new InvalidDataException($"{name}: geometry differs; frames {actual.Frames}/{frames}, scale {actual.Scale}/{scale}.");
            for (var s = 0; s < strokeCount; s++)
                if (reader.ReadInt64() != actual.StrokeEnds[s]) throw new InvalidDataException($"{name}: stroke boundary differs.");
            var maximum = 0f;
            for (var i = 0; i < checked(frames * MathPreprocessor.Channels); i++)
            {
                var expected = reader.ReadSingle(); var error = MathF.Abs(actual.Values[i] - expected);
                if (!float.IsFinite(expected) || !float.IsFinite(actual.Values[i]) || error > tolerance)
                    throw new InvalidDataException($"{name}: frame {i / 26}, channel {i % 26}: {actual.Values[i]}/{expected}, error {error}.");
                maximum = MathF.Max(maximum, error);
            }
            results.Add(new(name, frames, maximum));
        }
        if (reader.BaseStream.ReadByte() != -1) throw new InvalidDataException("Trailing feature fixture data.");
        return results;
    }
}
