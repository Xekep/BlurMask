using System.Globalization;

namespace InkRecognition.Math;

/// <summary>Bounded numeric arithmetic for plain text or a small, explicit LaTeX grammar.</summary>
public static class NumericExpressionEvaluator
{
    /// <summary>
    /// Returns a compact invariant result. Variables, undefined arithmetic and
    /// unsupported syntax are rejected. An existing equality must agree at the
    /// displayed precision; this API does not solve symbolic equations.
    /// </summary>
    public static bool TryEvaluate(string? expression, out string result)
    {
        result = "";
        if (string.IsNullOrWhiteSpace(expression) || expression.Length > 2048) return false;
        try
        {
            var parser = new Parser(expression);
            var left = parser.Expression(0);
            var formatted = Format(left);
            if (parser.Take(Kind.Equals) && parser.Current != Kind.End)
            {
                var right = parser.Expression(0);
                if (Format(right) != formatted) return false;
            }
            if (parser.Current != Kind.End) return false;
            result = formatted;
            return true;
        }
        catch (FormatException) { return false; }
    }

    private static string Format(double value)
    {
        if (value == 0) return "0";
        var text = value.ToString("G15", CultureInfo.InvariantCulture);
        var e = text.IndexOf('E');
        if (e < 0) return text;
        var exponent = int.Parse(text.AsSpan(e + 1), CultureInfo.InvariantCulture);
        if (exponent is < -12 or > 15) return text;
        var sign = text[0] == '-' ? "-" : "";
        var mantissa = text[sign.Length..e];
        var dot = mantissa.IndexOf('.');
        var position = (dot < 0 ? mantissa.Length : dot) + exponent;
        var digits = mantissa.Replace(".", "", StringComparison.Ordinal);
        return sign + (position <= 0 ? "0." + new string('0', -position) + digits :
            position >= digits.Length ? digits + new string('0', position - digits.Length) : digits.Insert(position, "."));
    }

    private enum Kind { End, Number, Plus, Minus, Multiply, Divide, Power, Open, Close, Brace, EndBrace, Bracket, EndBracket, Fraction, Root, Equals }

    private sealed class Parser(string source)
    {
        private int _position, _operations;
        private Kind _kind;
        private double _number;
        private bool _started;
        public Kind Current { get { Start(); return _kind; } }
        private void Start() { if (!_started) { _started = true; Next(); } }
        public bool Take(Kind kind)
        {
            Start();
            if (_kind != kind) return false;
            Next(); return true;
        }
        private void Require(Kind kind) { if (!Take(kind)) throw new FormatException("Missing delimiter."); }
        private static void Depth(int depth) { if (depth > 32) throw new FormatException("Numeric nesting exceeds limit."); }
        private double Checked(double value)
        {
            if (++_operations > 256 || !double.IsFinite(value)) throw new FormatException("Undefined or excessive arithmetic.");
            return value;
        }
        public double Expression(int depth)
        {
            Depth(depth);
            var value = Term(depth);
            while (Current is Kind.Plus or Kind.Minus)
            {
                var op = Current; Next(); var right = Term(depth);
                value = Checked(op == Kind.Plus ? value + right : value - right);
            }
            return value;
        }
        private double Term(int depth)
        {
            var value = Unary(depth);
            while (Current is Kind.Multiply or Kind.Divide)
            {
                var op = Current; Next(); var right = Unary(depth);
                if (op == Kind.Divide && right == 0) throw new FormatException("Division by zero.");
                value = Checked(op == Kind.Multiply ? value * right : value / right);
            }
            return value;
        }
        private double Unary(int depth)
        {
            Depth(depth);
            if (Take(Kind.Plus)) return Checked(Unary(depth + 1));
            if (Take(Kind.Minus)) return Checked(-Unary(depth + 1));
            var value = Primary(depth);
            if (Take(Kind.Power))
            {
                var exponent = Unary(depth + 1); // Right associative; -2^2 == -(2^2).
                if (System.Math.Abs(exponent) > 10000 || value == 0 && exponent == 0) throw new FormatException("Undefined or excessive power.");
                value = Checked(System.Math.Pow(value, exponent));
            }
            return value;
        }
        private double Group(Kind open, Kind close, int depth)
        {
            Require(open); var value = Expression(depth + 1); Require(close); return value;
        }
        private double Primary(int depth)
        {
            Depth(depth);
            if (Current == Kind.Number) { var value = _number; Next(); return value; }
            if (Current == Kind.Open) return Group(Kind.Open, Kind.Close, depth);
            if (Current == Kind.Brace) return Group(Kind.Brace, Kind.EndBrace, depth);
            if (Current == Kind.Bracket) return Group(Kind.Bracket, Kind.EndBracket, depth);
            if (Take(Kind.Fraction))
            {
                var numerator = Group(Kind.Brace, Kind.EndBrace, depth);
                var denominator = Group(Kind.Brace, Kind.EndBrace, depth);
                if (denominator == 0) throw new FormatException("Division by zero.");
                return Checked(numerator / denominator);
            }
            if (Take(Kind.Root))
            {
                var index = Current == Kind.Bracket ? Group(Kind.Bracket, Kind.EndBracket, depth) : 2;
                if (index is < 2 or > 64 || index != System.Math.Truncate(index)) throw new FormatException("Unsupported root index.");
                var radicand = Primary(depth + 1);
                if (radicand < 0 && (int)index % 2 == 0) throw new FormatException("Nonreal root.");
                return Checked(radicand < 0 ? -System.Math.Pow(-radicand, 1 / index) : System.Math.Pow(radicand, 1 / index));
            }
            throw new FormatException("Numeric operand expected.");
        }
        private void Next()
        {
            while (_position < source.Length && char.IsWhiteSpace(source[_position])) _position++;
            if (_position == source.Length) { _kind = Kind.End; return; }
            var start = _position; char c = source[_position++];
            _kind = c switch
            {
                '+' => Kind.Plus, '-' or '\u2212' => Kind.Minus,
                '*' or '\u00d7' or '\u00b7' or '\u22c5' => Kind.Multiply,
                '/' or '\u00f7' => Kind.Divide, '^' => Kind.Power,
                '(' => Kind.Open, ')' => Kind.Close, '{' => Kind.Brace, '}' => Kind.EndBrace,
                '[' => Kind.Bracket, ']' => Kind.EndBracket, '=' => Kind.Equals, '\u221a' => Kind.Root,
                _ => Kind.End
            };
            if (c == '*' && _position < source.Length && source[_position] == '*') { _position++; _kind = Kind.Power; }
            if (_kind != Kind.End) return;
            if (c == '\\')
            {
                int commandStart = _position;
                while (_position < source.Length && source[_position] is >= 'a' and <= 'z' or >= 'A' and <= 'Z') _position++;
                var command = source[commandStart.._position];
                _kind = command switch
                {
                    "frac" or "dfrac" or "tfrac" => Kind.Fraction, "sqrt" => Kind.Root,
                    "times" or "cdot" => Kind.Multiply, "div" => Kind.Divide,
                    _ => Kind.End
                };
                if (_kind != Kind.End) return;
                if (command is "left" or "right")
                {
                    while (_position < source.Length && char.IsWhiteSpace(source[_position])) _position++;
                    if (_position >= source.Length || (command == "left" ? source[_position] is not ('(' or '[') : source[_position] is not (')' or ']')))
                        throw new FormatException("Unsupported LaTeX fence.");
                    Next(); return;
                }
                throw new FormatException("Unsupported LaTeX command.");
            }
            if (c is not (>= '0' and <= '9' or '.' or ',')) throw new FormatException("Unsupported numeric syntax.");
            while (_position < source.Length && source[_position] is >= '0' and <= '9') _position++;
            if (c is not ('.' or ',') && _position < source.Length && source[_position] is '.' or ',')
            {
                _position++;
                while (_position < source.Length && source[_position] is >= '0' and <= '9') _position++;
            }
            if (_position < source.Length && source[_position] is 'e' or 'E')
            {
                _position++;
                if (_position < source.Length && source[_position] is '+' or '-') _position++;
                while (_position < source.Length && source[_position] is >= '0' and <= '9') _position++;
            }
            var literal = source[start.._position].Replace(',', '.');
            if (literal.Length > 64 || !double.TryParse(literal, NumberStyles.Float, CultureInfo.InvariantCulture, out _number) || !double.IsFinite(_number))
                throw new FormatException("Invalid numeric literal.");
            _kind = Kind.Number;
        }
    }
}
