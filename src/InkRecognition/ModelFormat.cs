namespace InkRecognition;

// Portable pack v1: LE primitives, strict UTF-8, SHA256 over the complete body.
// Weights retain their padded native tensor layout; no vendor container is needed.
internal static class ModelFormat
{
    internal static ReadOnlySpan<byte> Magic => "IKRPACK1"u8;
    internal const uint Version = 1;
    internal const int MaxBodyBytes = 64 * 1024 * 1024;
    internal const int MaxInputWidth = 256;
    internal const int MaxOutputWidth = 4096;
    internal const int MaxLayers = 16;
    internal const int MaxHiddenWidth = 2048;
    internal const int MaxTensorColumns = 4128; // Align(2 * MaxHiddenWidth + bias, 32).
    internal const int MaxTensorCount = 1024;
    internal const int MaxSequenceFloats = 16 * 1024 * 1024;
    internal static int Align(int value, int multiple = 8) => checked((value + multiple - 1) / multiple * multiple);
}

internal sealed class RecognitionModel(
    string language, string?[] labels, int blankIndex, float[] means, float[] scales,
    float spacing, int minPoints, int linkMode, NativeNetwork network)
{
    internal string Language { get; } = language;
    internal string?[] Labels { get; } = labels;
    internal int BlankIndex { get; } = blankIndex;
    internal float[] Means { get; } = means;
    internal float[] Scales { get; } = scales;
    internal float Spacing { get; } = spacing;
    internal int MinPoints { get; } = minPoints;
    internal int LinkMode { get; } = linkMode;
    internal NativeNetwork Network { get; } = network;
}

internal sealed class AffineWeights(int rows, int inputWidth, int columns, int storage,
    float[]? floats, short[]? shorts, sbyte[]? bytes, float[] scales, float[] rowSums, float[] auxiliary)
{
    internal int Rows { get; } = rows;
    internal int InputWidth { get; } = inputWidth;
    internal int Columns { get; } = columns;
    internal int Storage { get; } = storage;
    internal float[]? Floats { get; } = floats;
    internal short[]? Shorts { get; } = shorts;
    internal sbyte[]? Bytes { get; } = bytes;
    internal float[] Scales { get; } = scales;
    internal float[] RowSums { get; } = rowSums;
    internal float[] Auxiliary { get; } = auxiliary;
}

internal sealed class LstmDirection(AffineWeights[] input, AffineWeights[] recurrent,
    float[] inputPeephole, float[] forgetPeephole, float[] outputPeephole)
{
    // Gate order is g (candidate), i, f, o throughout the implementation.
    internal AffineWeights[] Input { get; } = input;
    internal AffineWeights[] Recurrent { get; } = recurrent;
    internal float[] InputPeephole { get; } = inputPeephole;
    internal float[] ForgetPeephole { get; } = forgetPeephole;
    internal float[] OutputPeephole { get; } = outputPeephole;
}

internal sealed class LstmLayer(int width, LstmDirection forward, LstmDirection? backward)
{
    internal int Width { get; } = width;
    internal LstmDirection Forward { get; } = forward;
    internal LstmDirection? Backward { get; } = backward;
    internal int OutputWidth => checked(Width * (Backward is null ? 1 : 2));
}
