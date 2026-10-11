namespace InkRecognition;

// Pure managed scalar execution of the recovered quantized BLSTM layout.
// Every call owns its scratch arrays; loaded weights are never modified.
internal sealed class NativeNetwork(int inputWidth, LstmLayer[] layers, AffineWeights output)
{
    internal int InputWidth { get; } = inputWidth;
    internal int OutputWidth => output.Rows;

    internal float[] Run(float[] features, int frames, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(features);
        if (frames <= 0 || (long)frames * InputWidth != features.Length ||
            features.Length > ModelFormat.MaxSequenceFloats)
            throw new ArgumentException("Expected a bounded, nonempty frame × input-width feature matrix.", nameof(features));
        foreach (float value in features)
            if (!float.IsFinite(value)) throw new ArgumentException("Features must be finite.", nameof(features));
        cancellationToken.ThrowIfCancellationRequested();
        float[] current = features;
        int width = InputWidth;
        foreach (var layer in layers)
        {
            int nextWidth = layer.OutputWidth;
            var next = AllocateFrames(frames, nextWidth);
            Direction(current, frames, width, layer.Forward, false, next, nextWidth, 0, cancellationToken);
            if (layer.Backward is not null)
                Direction(current, frames, width, layer.Backward, true, next, nextWidth, layer.Width, cancellationToken);
            current = next; width = nextWidth;
        }
        var probabilities = AllocateFrames(frames, output.Rows);
        var scratch = new AffineScratch(output.Columns);
        for (int t = 0; t < frames; t++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var values = probabilities.AsSpan(t * output.Rows, output.Rows);
            Affine(output, current.AsSpan(t * width, width), 1f, values, scratch, cancellationToken);
            float maximum = values[0];
            for (int i = 1; i < values.Length; i++) maximum = MathF.Max(maximum, values[i]);
            for (int i = 0; i < values.Length; i++) values[i] = MathF.Exp(values[i] - maximum);
            // NumPy's contiguous float32 reduction uses eight accumulators and
            // recursively splits blocks above 128 elements.
            float sum = PairwiseSum(values);
            if (!(sum > 0) || !float.IsFinite(sum)) throw new InvalidDataException("Invalid network softmax.");
            for (int i = 0; i < values.Length; i++) values[i] /= sum;
        }
        return probabilities;
    }

    private static float[] AllocateFrames(int frames, int width)
    {
        long count = (long)frames * width;
        if (count > ModelFormat.MaxSequenceFloats) throw new ArgumentException("Sequence exceeds the activation-memory limit.", nameof(frames));
        return new float[(int)count];
    }

    private static void Direction(float[] features, int frames, int featureWidth, LstmDirection direction,
        bool reverse, float[] output, int outputWidth, int outputOffset, CancellationToken cancellationToken)
    {
        int hidden = direction.Input[0].Rows;
        var state = new float[hidden]; var cell = new float[hidden];
        var gates = new float[checked(hidden * 4)]; var recurrence = new float[hidden];
        int columns = Math.Max(direction.Input[0].Columns, direction.Recurrent[0].Columns);
        var scratch = new AffineScratch(columns);
        for (int step = 0; step < frames; step++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            int t = reverse ? frames - 1 - step : step;
            for (int gate = 0; gate < 4; gate++)
            {
                var values = gates.AsSpan(gate * hidden, hidden);
                Affine(direction.Input[gate], features.AsSpan(t * featureWidth, featureWidth), 1f, values, scratch, cancellationToken);
                if (step == 0) continue;
                Affine(direction.Recurrent[gate], state, 0f, recurrence, scratch, cancellationToken);
                for (int i = 0; i < hidden; i++) values[i] += recurrence[i];
            }
            for (int i = 0; i < hidden; i++)
            {
                float incoming = gates[hidden + i], forget = gates[hidden * 2 + i];
                if (step != 0)
                {
                    float inputPeephole = cell[i] * direction.InputPeephole[i];
                    float forgetPeephole = cell[i] * direction.ForgetPeephole[i];
                    incoming += inputPeephole; forget += forgetPeephole;
                }
                float candidate = MathF.Tanh(gates[i]);
                float inputGate = Sigmoid(incoming);
                float forgetGate = Sigmoid(forget);
                float nextCell = candidate * inputGate;
                if (step != 0)
                {
                    float retained = cell[i] * forgetGate;
                    nextCell += retained;
                }
                cell[i] = nextCell;
                float outputPeephole = nextCell * direction.OutputPeephole[i];
                float outGate = Sigmoid(gates[hidden * 3 + i] + outputPeephole);
                state[i] = MathF.Tanh(nextCell) * outGate;
                if (!float.IsFinite(state[i])) throw new InvalidDataException("Nonfinite recurrent network state.");
                output[t * outputWidth + outputOffset + i] = state[i];
            }
        }
    }

    private static float Sigmoid(float value) => 1f / (1f + MathF.Exp(-Math.Clamp(value, -88f, 88f)));

    private sealed class AffineScratch(int columns)
    {
        internal float[] Values { get; } = new float[columns];
        internal int[] Quantized { get; } = new int[columns];
    }

    private static void Affine(AffineWeights matrix, ReadOnlySpan<float> values, float bias,
        Span<float> result, AffineScratch scratch, CancellationToken cancellationToken)
    {
        var x = scratch.Values.AsSpan(0, matrix.Columns);
        x.Clear(); values.CopyTo(x); x[values.Length] = bias;
        int columns = matrix.Columns;
        if (matrix.Storage == 1)
        {
            var weights = matrix.Floats!;
            for (int row = 0; row < matrix.Rows; row++)
            {
                if ((row & 127) == 0) cancellationToken.ThrowIfCancellationRequested();
                float dot = 0;
                for (int col = 0; col < columns; col++)
                {
                    float product = weights[row * columns + col] * x[col];
                    dot += product;
                }
                result[row] = dot;
            }
            return;
        }
        var q = scratch.Quantized.AsSpan(0, columns);
        if (matrix.Storage == 2)
        {
            float peak = 0;
            foreach (float value in x) peak = MathF.Max(peak, MathF.Abs(value));
            if (peak == 0) { result.Clear(); return; }
            float limit = matrix.Auxiliary[0];
            float factor = limit / peak;
            int integerLimit = (int)limit;
            for (int col = 0; col < columns; col++)
            {
                float scaled = x[col] * factor;
                float rounded = scaled + (x[col] < 0 ? -0.49999f : 0.49999f);
                q[col] = (int)Math.Clamp(MathF.Truncate(rounded), -integerLimit, integerLimit);
            }
            float inverseFactor = peak / limit;
            var weights = matrix.Shorts!;
            Span<int> lanes = stackalloc int[8];
            for (int row = 0; row < matrix.Rows; row++)
            {
                if ((row & 127) == 0) cancellationToken.ThrowIfCancellationRequested();
                lanes.Clear();
                for (int col = 0; col < columns; col++)
                {
                    int product = unchecked(weights[row * columns + col] * q[col]);
                    int lane = (col / 2) & 7;
                    lanes[lane] = unchecked(lanes[lane] + product);
                }
                float dot = lanes[0];
                for (int lane = 1; lane < 8; lane++) dot += (float)lanes[lane];
                float unscaled = dot * inverseFactor;
                result[row] = unscaled * matrix.Scales[row];
            }
            return;
        }
        int activationColumns = ModelFormat.Align(matrix.InputWidth + 1);
        float lo = x[0], hi = x[0];
        for (int col = 1; col < activationColumns; col++)
        { lo = MathF.Min(lo, x[col]); hi = MathF.Max(hi, x[col]); }
        if (hi <= lo) { result.Clear(); return; }
        float range = hi - lo;
        float inputFactor = 127f / range;
        for (int col = 0; col < activationColumns; col++)
        {
            float shifted = x[col] - lo;
            float scaled = shifted * inputFactor;
            q[col] = (int)Math.Clamp(MathF.Truncate(scaled + 0.49999f), 0, 127);
        }
        q[activationColumns..].Clear();
        float offset = inputFactor * lo;
        float inverse = range / 127f;
        var bytes = matrix.Bytes!;
        for (int row = 0; row < matrix.Rows; row++)
        {
            if ((row & 127) == 0) cancellationToken.ThrowIfCancellationRequested();
            int accumulator = 0;
            for (int col = 0; col < columns; col++)
                accumulator = unchecked(accumulator + bytes[row * columns + col] * q[col]);
            float correction = offset * matrix.RowSums[row];
            float dot = (float)accumulator + correction;
            float scale = inverse * matrix.Scales[row];
            result[row] = dot * scale;
        }
    }

    private static float PairwiseSum(ReadOnlySpan<float> values)
    {
        if (values.Length < 8)
        {
            float result = -0f;
            foreach (float value in values) result += value;
            return result;
        }
        if (values.Length > 128)
        {
            int split = values.Length / 2;
            split -= split % 8;
            return PairwiseSum(values[..split]) + PairwiseSum(values[split..]);
        }
        Span<float> sums = stackalloc float[8];
        values[..8].CopyTo(sums);
        int i = 8;
        int end = values.Length - values.Length % 8;
        for (; i < end; i += 8)
            for (int lane = 0; lane < 8; lane++) sums[lane] += values[i + lane];
        float left = (sums[0] + sums[1]) + (sums[2] + sums[3]);
        float right = (sums[4] + sums[5]) + (sums[6] + sums[7]);
        float resultBlock = left + right;
        for (; i < values.Length; i++) resultBlock += values[i];
        return resultBlock;
    }
}
