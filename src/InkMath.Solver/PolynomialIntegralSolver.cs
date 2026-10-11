using System.Globalization;
using System.Numerics;

namespace InkMath.Solver;

/// <summary>
/// Exact finite polynomial integrals without an external runtime. Unsupported
/// functions, symbolic bounds and singular rational integrands are left to CAS.
/// </summary>
public static class PolynomialIntegralSolver
{
    private const int MaximumDegree = 32;

    public static MathSolution? Solve(string latex, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(latex);
        cancellationToken.ThrowIfCancellationRequested();
        try
        {
            if (new MathExpressionParser(latex).Parse() is not Call
                { Name: "integrate", Arguments.Length: 4 } integral || integral.Arguments[1] is not Variable variable)
                return null;
            if (variable.Name is "e" or "i" or "pi" or "infinity") return null;
            var coefficients = Polynomial(integral.Arguments[0], variable.Name, cancellationToken);
            var lower = Constant(integral.Arguments[2], cancellationToken);
            var upper = Constant(integral.Arguments[3], cancellationToken);
            Rational result = Rational.Zero;
            for (int degree = 0; degree < coefficients.Length; degree++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                result += coefficients[degree] * (upper.Pow(degree + 1) - lower.Pow(degree + 1)) / new Rational(degree + 1, 1);
            }
            return new(result.LaTex, false);
        }
        catch (Exception error) when (error is FormatException or ArithmeticException) { return null; }
    }

    private static Rational Constant(Expression expression, CancellationToken token)
    {
        var value = Polynomial(expression, null, token);
        if (value.Length != 1) throw new FormatException("Expected rational constant.");
        return value[0];
    }

    private static Rational[] Polynomial(Expression expression, string? variable, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        if (expression is Number number) return [Rational.Parse(number.Value)];
        if (expression is Variable symbol && variable is not null && symbol.Name == variable) return [Rational.Zero, Rational.One];
        if (expression is not Binary binary) throw new FormatException("Not a single-variable rational polynomial.");
        var left = Polynomial(binary.Left, variable, token);
        switch (binary.Operator)
        {
            case "+": case "-":
                var right = Polynomial(binary.Right, variable, token);
                var sum = Enumerable.Repeat(Rational.Zero, Math.Max(left.Length, right.Length)).ToArray();
                for (int i = 0; i < sum.Length; i++)
                    sum[i] = (i < left.Length ? left[i] : Rational.Zero) +
                        (i < right.Length ? right[i] : Rational.Zero) * (binary.Operator == "+" ? Rational.One : new Rational(-1, 1));
                return Trim(sum);
            case "*": return Product(left, Polynomial(binary.Right, variable, token), token);
            case "/":
                var denominator = Constant(binary.Right, token);
                return Trim(left.Select(value => value / denominator).ToArray());
            case "^":
                var power = Constant(binary.Right, token);
                if (power.Denominator != BigInteger.One || power.Numerator < 0 || power.Numerator > MaximumDegree)
                    throw new FormatException("Unsupported polynomial exponent.");
                if (power.Numerator.IsZero && left.Length == 1 && left[0].Numerator.IsZero)
                    throw new FormatException("Undefined zero power.");
                Rational[] result = [Rational.One];
                for (int i = 0; i < (int)power.Numerator; i++) result = Product(result, left, token);
                return result;
            default: throw new FormatException("Unsupported polynomial operator.");
        }
    }

    private static Rational[] Product(Rational[] left, Rational[] right, CancellationToken token)
    {
        if (left.Length + right.Length - 2 > MaximumDegree) throw new FormatException("Polynomial degree limit.");
        var result = Enumerable.Repeat(Rational.Zero, left.Length + right.Length - 1).ToArray();
        for (int i = 0; i < left.Length; i++)
        {
            token.ThrowIfCancellationRequested();
            for (int j = 0; j < right.Length; j++) result[i + j] += left[i] * right[j];
        }
        return Trim(result);
    }

    private static Rational[] Trim(Rational[] coefficients)
    {
        int length = coefficients.Length;
        while (length > 1 && coefficients[length - 1].Numerator.IsZero) length--;
        return coefficients[..length];
    }

    private readonly record struct Rational
    {
        public BigInteger Numerator { get; }
        public BigInteger Denominator { get; }
        public static Rational Zero => new(0, 1);
        public static Rational One => new(1, 1);
        public Rational(BigInteger numerator, BigInteger denominator)
        {
            if (denominator.IsZero) throw new DivideByZeroException();
            if (numerator.GetBitLength() > 4096 || denominator.GetBitLength() > 4096) throw new ArithmeticException("Rational size limit.");
            if (denominator.Sign < 0) { numerator = -numerator; denominator = -denominator; }
            var divisor = BigInteger.GreatestCommonDivisor(numerator, denominator);
            Numerator = numerator / divisor; Denominator = denominator / divisor;
        }
        public static Rational Parse(string value)
        {
            int decimalPoint = value.IndexOf('.');
            return decimalPoint < 0 ? new(BigInteger.Parse(value, CultureInfo.InvariantCulture), 1) :
                new(BigInteger.Parse(value.Remove(decimalPoint, 1), CultureInfo.InvariantCulture), BigInteger.Pow(10, value.Length - decimalPoint - 1));
        }
        public Rational Pow(int exponent) => new(BigInteger.Pow(Numerator, exponent), BigInteger.Pow(Denominator, exponent));
        public string LaTex => Denominator.IsOne ? Numerator.ToString(CultureInfo.InvariantCulture) :
            (Numerator.Sign < 0 ? "-" : "") + @"\frac{" + BigInteger.Abs(Numerator).ToString(CultureInfo.InvariantCulture) + "}{" + Denominator.ToString(CultureInfo.InvariantCulture) + "}";
        public static Rational operator +(Rational a, Rational b) => new(a.Numerator * b.Denominator + b.Numerator * a.Denominator, a.Denominator * b.Denominator);
        public static Rational operator -(Rational a, Rational b) => new(a.Numerator * b.Denominator - b.Numerator * a.Denominator, a.Denominator * b.Denominator);
        public static Rational operator *(Rational a, Rational b) => new(a.Numerator * b.Numerator, a.Denominator * b.Denominator);
        public static Rational operator /(Rational a, Rational b) => new(a.Numerator * b.Denominator, a.Denominator * b.Numerator);
    }
}
