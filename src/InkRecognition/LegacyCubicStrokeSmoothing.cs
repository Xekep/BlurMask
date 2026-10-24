namespace InkRecognition;

public readonly record struct LegacyCubicCoefficients(double Value, double Linear, double Quadratic, double Cubic);

/// <summary>Prepared ordinary spline171700 and no-pivot five-diagonal factorization1712c0.</summary>
public static class LegacyCubicStrokeSmoothing
{
    public static LegacyCubicCoefficients[] Fit(ReadOnlySpan<float> values, ReadOnlySpan<double> lengths,
        double penalty, CancellationToken cancellationToken = default)
        => FitCore(values, lengths, penalty, 4096, cancellationToken);

    //Neutral numerical DATA only. The private same-source Math plan owns input
    //and coefficients; this helper grants no source/candidate authority.
    internal static LegacyCubicCoefficients[] FitOwnedMathSource(ReadOnlySpan<float> values,
        ReadOnlySpan<double> lengths, double penalty, CancellationToken cancellationToken)
        => FitCore(values, lengths, penalty, 8192, cancellationToken);

    private static LegacyCubicCoefficients[] FitCore(ReadOnlySpan<float> values, ReadOnlySpan<double> lengths,
        double penalty, int maximumPoints, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (values.Length < 4 || values.Length > maximumPoints || lengths.Length != values.Length - 1 || !double.IsFinite(penalty) || penalty < 0)
            throw new ArgumentException("Invalid prepared smoothing spline input.");
        for (int i = 0; i < values.Length; i++)
            if (!float.IsFinite(values[i]) || (i < lengths.Length && (!double.IsFinite(lengths[i]) || lengths[i] <= 0)))
                throw new ArgumentException("Expected finite spline coordinates and positive segment lengths.");
        int count = values.Length - 2; var matrix = new double[count, 5]; var rhs = new double[count]; double six = penalty * 6;
        for (int i = 0; i < count; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            double before = 1 / lengths[i], after = 1 / lengths[i + 1];
            rhs[i] = ((double)(values[i + 2] - values[i + 1]) * after - (double)(values[i + 1] - values[i]) * before) * 3;
            matrix[i, 0] = i < 2 ? 0 : six * (1 / lengths[i - 1]) * before;
            matrix[i, 1] = i == 0 ? 0 : lengths[i] - (before + before + 1 / lengths[i - 1] + after) * six * before;
            matrix[i, 2] = ((after + before) * (after + before) + before * before + after * after) * six + lengths[i + 1] + lengths[i] + lengths[i + 1] + lengths[i];
            matrix[i, 3] = i >= count - 1 ? 0 : lengths[i + 1] - (after + after + before + 1 / lengths[i + 2]) * six * after;
            matrix[i, 4] = i >= count - 2 ? 0 : (six * after) / lengths[i + 2];
        }
        var solved = Solve(matrix, rhs, cancellationToken); var curvature = new double[values.Length]; solved.CopyTo(curvature, 1);
        var fitted = new double[values.Length]; double twice = penalty + penalty;
        fitted[0] = ((curvature[0] - curvature[1]) * twice) / lengths[0] + values[0];
        double inverseBefore = 1 / lengths[0];
        for (int i = 1; i < values.Length - 1; i++)
        {
            double inverseAfter = 1 / lengths[i];
            fitted[i] = values[i] - ((inverseBefore * curvature[i - 1] - (inverseAfter + inverseBefore) * curvature[i]) + curvature[i + 1] * inverseAfter) * twice;
            inverseBefore = inverseAfter;
        }
        fitted[^1] = values[^1] - ((curvature[^2] - curvature[^1]) * twice) / lengths[^1];
        var result = new LegacyCubicCoefficients[values.Length];
        for (int i = 0; i < lengths.Length; i++)
        {
            double inverse = 1 / lengths[i];
            result[i] = new(fitted[i], (fitted[i + 1] - fitted[i]) * inverse - ((curvature[i] + curvature[i] + curvature[i + 1]) * lengths[i]) / 3,
                curvature[i], ((curvature[i + 1] - curvature[i]) * inverse) / 3);
        }
        result[^1] = new(fitted[^1], 0, curvature[^1], 0);
        foreach (var c in result)
            if (!double.IsFinite(c.Value) || !double.IsFinite(c.Linear) || !double.IsFinite(c.Quadratic) || !double.IsFinite(c.Cubic))
                throw new InvalidDataException("Smoothing spline produced nonfinite coefficients.");
        return result;
    }

    private static double[] Solve(double[,] a, double[] rhs, CancellationToken cancellationToken)
    {
        int n = rhs.Length; var diagonal = new double[n]; var upper1 = new double[n]; var upper2 = new double[n];
        var lower1 = new double[n]; var intermediate = new double[n];
        diagonal[0] = a[0, 2]; Check(diagonal[0]); upper1[0] = a[0, 3] / diagonal[0]; upper2[0] = a[0, 4] / diagonal[0];
        lower1[1] = a[1, 1]; diagonal[1] = a[1, 2] - lower1[1] * upper1[0]; Check(diagonal[1]);
        upper1[1] = (a[1, 3] - lower1[1] * upper2[0]) / diagonal[1]; upper2[1] = a[1, 4] / diagonal[1];
        for (int i = 2; i < n; i++)
        {
            cancellationToken.ThrowIfCancellationRequested(); lower1[i] = a[i, 1] - upper1[i - 2] * a[i, 0];
            diagonal[i] = (a[i, 2] - upper2[i - 2] * a[i, 0]) - lower1[i] * upper1[i - 1]; Check(diagonal[i]);
            upper1[i] = (a[i, 3] - lower1[i] * upper2[i - 1]) / diagonal[i]; upper2[i] = a[i, 4] / diagonal[i];
        }
        intermediate[0] = rhs[0] / diagonal[0];
        for (int i = 1; i < n; i++) intermediate[i] = ((rhs[i] - a[i, 0] * (i > 1 ? intermediate[i - 2] : 0)) - intermediate[i - 1] * lower1[i]) / diagonal[i];
        var result = new double[n]; result[^1] = intermediate[^1]; result[^2] = intermediate[^2] - result[^1] * upper1[^2];
        for (int i = n - 3; i >= 0; i--) result[i] = (intermediate[i] - result[i + 1] * upper1[i]) - result[i + 2] * upper2[i];
        return result;
    }
    private static void Check(double value) { if (!double.IsFinite(value) || value == 0) throw new InvalidDataException("Singular/nonfinite no-pivot spline system."); }
}
