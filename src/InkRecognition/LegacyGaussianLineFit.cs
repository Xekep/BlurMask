namespace InkRecognition;

public readonly record struct LegacyGaussianLineEstimate(double Guideline0, double Guideline1,
    double Guideline2, double Guideline3, double Slope, int Iterations, int ExtremumCount);

/// <summary>
/// Text 18000b3d0 four-component hard-assignment Gaussian/shared-slope MAP fit,
/// with the original 18000b300 default profile. Explicit prepared line only;
/// excludes class 24a slope clamping, sparse/dense blending and callback adjustments.
/// </summary>
public static class LegacyGaussianLineFit
{
    public const int MaximumExtrema = 16_384;

    public static LegacyGaussianLineEstimate Fit(IReadOnlyList<InkPoint[]> preparedStrokes,
        bool includeEndpoints = true, CancellationToken cancellationToken = default)
    {
        var extrema = LegacyLineExtrema.Extract(preparedStrokes, includeEndpoints, cancellationToken);
        return FitExtrema(extrema.Lower, extrema.Upper, cancellationToken);
    }

    public static LegacyGaussianLineEstimate FitExtrema(IReadOnlyList<InkPoint> lower,
        IReadOnlyList<InkPoint> upper, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(lower); ArgumentNullException.ThrowIfNull(upper);
        cancellationToken.ThrowIfCancellationRequested();
        if (lower.Count > MaximumExtrema || upper.Count > MaximumExtrema || lower.Count + upper.Count is < 1 or > MaximumExtrema)
            throw new ArgumentException("Expected 1..16384 prepared extrema.");
        var points = new InkPoint[lower.Count + upper.Count]; int index = 0;
        foreach (var collection in new[] { lower, upper }) foreach (var point in collection)
        {
            if (!float.IsFinite(point.X) || !float.IsFinite(point.Y)) throw new ArgumentException("Nonfinite extremum.");
            points[index++] = point;
        }
        int n = points.Length; double originX = 0, meanY = 0;
        foreach (var p in points) { originX += p.X; meanY += p.Y; }
        originX /= n; meanY /= n;
        double variance = 0.000001;
        foreach (var p in points) { double y = p.Y - meanY; variance += y * y; }
        variance /= n; double standardDeviation = Math.Sqrt(variance);
        double[] priorMeans = [standardDeviation * (double)2.6f + meanY, standardDeviation * .5 + meanY,
            standardDeviation * (double)(-.65f) + meanY, standardDeviation * -3 + meanY];
        double[] means = (double[])priorMeans.Clone();
        double[] priorVariances = [variance * 2, variance * 4, variance * 4, variance * 2];
        double likelihoodVariance = variance * .25;
        double normalizer = 1 / Math.Sqrt(likelihoodVariance * 6.283185005187988);
        double slopePriorVariance = n * (double).1f, slope = 0;
        int iterations = 0;
        // Profile class weights are all one, all four fixed likelihood variances are equal.
        for (int iteration = 0; iteration < 20; iteration++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            double threshold = (normalizer + 0 + normalizer + normalizer + normalizer) * .25 * .75;
            if (iteration == 0) threshold *= .25;
            int[] counts = new int[4]; double[] sumY = new double[4], sumX = new double[4], sumXY = new double[4], sumXX = new double[4];
            for (int j = 0; j < points.Length; j++)
            {
                cancellationToken.ThrowIfCancellationRequested(); var p = points[j]; double x = p.X - originX;
                double adjustedY = p.Y - x * slope; int first = j < lower.Count ? 0 : 2;
                double distance0 = adjustedY - means[first], distance1 = adjustedY - means[first + 1];
                double score0 = Math.Exp((double)((float)(distance0 * distance0 / likelihoodVariance) * -.5f)) * normalizer;
                double score1 = Math.Exp((double)((float)(distance1 * distance1 / likelihoodVariance) * -.5f)) * normalizer;
                int component = score0 < score1 ? first + 1 : first;
                if (Math.Max(score0, score1) <= threshold) continue;
                counts[component]++; sumY[component] += p.Y; sumX[component] += x;
                sumXY[component] += p.Y * x; sumXX[component] += x * x;
            }
            double numerator = 0, denominator = 0;
            for (int i = 0; i < 4; i++)
            {
                double divisor = likelihoodVariance * n + counts[i] * priorVariances[i];
                numerator += (sumXY[i] / likelihoodVariance - sumX[i] * sumY[i] * priorVariances[i] / (likelihoodVariance * divisor))
                    - n * priorMeans[i] * sumX[i] / divisor;
                denominator += sumXX[i] / likelihoodVariance - sumX[i] * sumX[i] * priorVariances[i] / (likelihoodVariance * divisor);
            }
            // Prior slope is zero in the original default profile.
            slope = numerator / (denominator + n / slopePriorVariance);
            double differenceSquared = 0;
            for (int i = 0; i < 4; i++)
            {
                double next = ((sumY[i] - slope * sumX[i]) / likelihoodVariance + n * priorMeans[i] / priorVariances[i]) /
                    (counts[i] / likelihoodVariance + n / priorVariances[i]);
                double difference = next - means[i]; differenceSquared += difference * difference; means[i] = next;
            }
            priorVariances[0] += priorVariances[0]; priorVariances[3] += priorVariances[3]; iterations++;
            if (!(Math.Sqrt(differenceSquared * .25) > 0)) break;
        }
        for (int i = 0; i < 4; i++) means[i] -= originX * slope;
        if (!double.IsFinite(slope) || means.Any(value => !double.IsFinite(value)))
            throw new InvalidDataException("Nonfinite Gaussian line fit.");
        return new(means[0], means[1], means[2], means[3], slope, iterations, n);
    }
}
