namespace InkRecognition;

/// <summary>Prepared C1A boundary/profile observation accepted by Ink020d70.</summary>
public readonly record struct LegacyCharacterLineObservation(double CenterX, double TopY, double BottomY,
    double TopProfile, double BottomProfile, double Weight);

/// <summary>The native ten ordered double moments. Profile entries describe prepared glyph boundary candidates.</summary>
public readonly record struct LegacyCharacterLineMoments(double W, double X, double Y, double Q,
    double XX, double YX, double QX, double YY, double YQ, double QQ)
{
    public double[] CopyValues() => [W, X, Y, Q, XX, YX, QX, YY, YQ, QQ];
}

public readonly record struct LegacyCharacterLineSolution(double HeightScale, double Slope, double Intercept)
{
    /// <summary>Ink021110's final float outputs before its wrapper's acceptance/fallback policy.</summary>
    public (float BaselineY, float XHeight, float Slope) ToPreparedFitOutput()
    {
        float baseline = (float)(Intercept - HeightScale), height = (float)(HeightScale * -2.0), slope = (float)Slope;
        if (!float.IsFinite(baseline) || !float.IsFinite(height) || !float.IsFinite(slope))
            throw new InvalidDataException("Prepared line solution exceeds the native float output range.");
        return (baseline, height, slope);
    }
}

/// <summary>
/// Ink020d70 moment producer and020ed0 constrained three-variable solve, used by
/// C1A021110. Input candidate production, IRLS reweighting and acceptance remain
/// separate: this is not automatic raw handwriting x-height inference.
/// </summary>
public static class LegacyPreparedCharacterLineEquations
{
    public static LegacyCharacterLineMoments Accumulate(ReadOnlySpan<LegacyCharacterLineObservation> observations,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (observations.Length > 65536) throw new ArgumentException("Prepared line observations exceed the explicit budget.");
        Span<double> m = stackalloc double[10];
        m.Clear();
        foreach (var o in observations)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!double.IsFinite(o.CenterX) || !double.IsFinite(o.TopY) || !double.IsFinite(o.BottomY) ||
                !double.IsFinite(o.TopProfile) || !double.IsFinite(o.BottomProfile) || !double.IsFinite(o.Weight) || o.Weight < 0)
                throw new ArgumentException("Finite prepared boundaries/profiles and nonnegative weights required.");
            double y = (o.TopY + o.BottomY) * .5, q = (o.TopProfile + o.BottomProfile) * .5;
            double span = o.BottomProfile - o.TopProfile;
            m[0] = o.Weight + m[0];
            m[1] = o.CenterX * o.Weight + m[1];
            m[2] = y * o.Weight + m[2];
            m[3] = q * o.Weight + m[3];
            m[4] = o.CenterX * o.CenterX * o.Weight + m[4];
            m[5] = y * o.CenterX * o.Weight + m[5];
            m[6] = q * o.CenterX * o.Weight + m[6];
            m[7] = y * y * o.Weight + m[7];
            m[8] = y * q * o.Weight + m[8] + (o.BottomY - o.TopY) * span * o.Weight;
            m[9] = q * q * o.Weight + m[9] + span * span * o.Weight;
        }
        foreach (double value in m)
            if (!double.IsFinite(value)) throw new InvalidDataException("Prepared line moment arithmetic overflow.");
        return new(m[0], m[1], m[2], m[3], m[4], m[5], m[6], m[7], m[8], m[9]);
    }

    public static LegacyCharacterLineSolution Solve(LegacyCharacterLineMoments m, double minimumSlope,
        double maximumSlope, bool forceZeroSlope = false)
    {
        if (m.W <= 0 || !Finite(m) ||
            !double.IsFinite(minimumSlope) || !double.IsFinite(maximumSlope) || minimumSlope > maximumSlope)
            throw new ArgumentException("Positive prepared moment mass and ordered finite slope bounds required.");
        double e6 = m.QX * m.Q - m.QQ * m.X;
        double e7 = (m.XX + 1e-12) * m.Q - m.QX * m.X;
        double e9 = m.Q * m.YX - m.YQ * m.X;
        double e5 = m.Q * m.Q - m.W * m.QQ;
        double e8 = m.Y * m.Q - m.W * m.YQ;
        double determinant = forceZeroSlope ? 0 : (m.Q * m.X - m.QX * m.W) * e6 - e5 * e7;
        if (!double.IsFinite(e6) || !double.IsFinite(e7) || !double.IsFinite(e9) || !double.IsFinite(e5) ||
            !double.IsFinite(e8) || !double.IsFinite(determinant))
            throw new InvalidDataException("Prepared line determinant arithmetic overflow.");
        double slope = Math.Abs(determinant) <= 1e-10 ? 0 : (e8 * e6 - e5 * e9) / determinant;
        if (!double.IsFinite(slope)) throw new InvalidDataException("Prepared line slope arithmetic overflow.");
        // Native comparisons and order, including force-zero followed by bound clamps.
        if (minimumSlope <= slope) minimumSlope = slope;
        if (maximumSlope <= minimumSlope) minimumSlope = maximumSlope;
        slope = minimumSlope;
        double height = 0;
        if (slope == 0)
        {
            double tolerance = Math.Max(Math.Abs(e5) * 4.440892098500626e-16, 2.220446049250313e-16);
            if (tolerance < Math.Abs(-e5)) height = e8 / e5;
        }
        else
        {
            double tolerance = Math.Max(Math.Abs(e6) * 4.440892098500626e-16, 2.220446049250313e-16);
            if (tolerance < Math.Abs(-e6)) height = (e9 - slope * e7) / e6;
        }
        double intercept = ((m.Y - m.Q * height) - m.X * slope) / m.W;
        if (!double.IsFinite(height) || !double.IsFinite(slope) || !double.IsFinite(intercept))
            throw new InvalidDataException("Prepared constrained line solve overflow.");
        return new(height, slope, intercept);
    }

    private static bool Finite(LegacyCharacterLineMoments m) => double.IsFinite(m.W) && double.IsFinite(m.X) &&
        double.IsFinite(m.Y) && double.IsFinite(m.Q) && double.IsFinite(m.XX) && double.IsFinite(m.YX) &&
        double.IsFinite(m.QX) && double.IsFinite(m.YY) && double.IsFinite(m.YQ) && double.IsFinite(m.QQ);
}
