using System.Globalization;
using Avalonia;
using Avalonia.Media;

namespace NovaPen;

// Small native geometry typesetter for the structural LaTeX emitted by Math2.
// It never evaluates code or loads a browser. Unsupported notation stays ink.
internal static class MathTypesetting
{
    private sealed record Box(Geometry Geometry, double Width, double Height, double Baseline);
    private abstract record Node { internal abstract Box Layout(double size); }
    private sealed record Run(string Text, bool Upright = false) : Node
    {
        internal override Box Layout(double size)
        {
            var variable = !Upright && Text.Any(char.IsLetter) && Text is not ("sin" or "cos" or "tan" or "cot" or "sec" or "csc" or "log" or "ln" or "exp" or "lim" or "max" or "min" or "sinh" or "cosh" or "tanh" or "arcsin" or "arccos" or "arctan" or "det" or "gcd");
            var text = new FormattedText(Text, CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
                variable ? InkFonts.MathVariable : InkFonts.Face("STIX Two Math"), size, Brushes.Black);
            return new(text.BuildGeometry(default) ?? new GeometryGroup(), text.WidthIncludingTrailingWhitespace, text.Height, text.Baseline);
        }
    }
    private sealed record Row(IReadOnlyList<Node> Children) : Node
    {
        internal override Box Layout(double size)
        {
            var boxes = Children.Select(n => n.Layout(size)).ToArray();
            var baseline = boxes.Length == 0 ? size * .8 : boxes.Max(b => b.Baseline);
            var below = boxes.Length == 0 ? size * .2 : boxes.Max(b => b.Height - b.Baseline);
            var geometry = new GeometryGroup(); double x = 0;
            foreach (var box in boxes) { Add(geometry, box, x, baseline - box.Baseline); x += box.Width; }
            return new(geometry, x, baseline + below, baseline);
        }
    }
    private sealed record Fraction(Node Top, Node Bottom) : Node
    {
        internal override Box Layout(double size)
        {
            var a = Top.Layout(size * .8); var b = Bottom.Layout(size * .8);
            var width = Math.Max(a.Width, b.Width) + size * .24; var gap = size * .12;
            var bar = Math.Max(1, size * .045); var geometry = new GeometryGroup();
            Add(geometry, a, (width - a.Width) / 2, 0);
            geometry.Children.Add(new RectangleGeometry(new Rect(0, a.Height + gap, width, bar)));
            Add(geometry, b, (width - b.Width) / 2, a.Height + gap * 2 + bar);
            return new(geometry, width, a.Height + b.Height + gap * 2 + bar, a.Height + gap + size * .28);
        }
    }
    private sealed record Radical(Node Content) : Node
    {
        internal override Box Layout(double size)
        {
            var content = Content.Layout(size); var sign = new Run("√").Layout(size);
            var height = Math.Max(content.Height, sign.Height); var geometry = new GeometryGroup(); var lead = sign.Width * .88;
            Add(geometry, sign, 0, 0, 1, height / sign.Height);
            Add(geometry, content, lead, size * .08);
            geometry.Children.Add(new RectangleGeometry(new Rect(lead, size * .05, content.Width + size * .06, Math.Max(1, size * .04))));
            return new(geometry, lead + content.Width + size * .06, height + size * .08, content.Baseline + size * .08);
        }
    }
    private sealed record Script(Node Base, Node? Sub, Node? Sup, bool UseLimits = false) : Node
    {
        internal override Box Layout(double size)
        {
            var body = Base.Layout(size); var sub = Sub?.Layout(size * .65); var sup = Sup?.Layout(size * .65);
            if (UseLimits)
            {
                double width = Math.Max(body.Width, Math.Max(sub?.Width ?? 0, sup?.Width ?? 0));
                double top = sup is null ? 0 : sup.Height + size * .12;
                double bottom = sub is null ? 0 : sub.Height + size * .12;
                var limits = new GeometryGroup();
                if (sup is not null) Add(limits, sup, (width - sup.Width) / 2, 0);
                Add(limits, body, (width - body.Width) / 2, top);
                if (sub is not null) Add(limits, sub, (width - sub.Width) / 2, top + body.Height + size * .12);
                return new(limits, width, top + body.Height + bottom, top + body.Baseline);
            }
            var up = sup?.Height * .6 ?? 0; var down = sub?.Height * .6 ?? 0;
            var geometry = new GeometryGroup(); Add(geometry, body, 0, up);
            if (sup is not null) Add(geometry, sup, body.Width, 0);
            if (sub is not null) Add(geometry, sub, body.Width, up + body.Height - sub.Height * .4);
            return new(geometry, body.Width + Math.Max(sub?.Width ?? 0, sup?.Width ?? 0), up + body.Height + down, up + body.Baseline);
        }
    }
    private sealed record Table(IReadOnlyList<IReadOnlyList<Node>> Rows) : Node
    {
        internal override Box Layout(double size)
        {
            var boxes = Rows.Select(r => r.Select(n => n.Layout(size)).ToArray()).ToArray();
            var widths = Enumerable.Range(0, boxes.Max(r => r.Length)).Select(c => boxes.Max(r => c < r.Length ? r[c].Width : 0)).ToArray();
            var geometry = new GeometryGroup(); double y = 0; var gap = size * .3;
            foreach (var row in boxes)
            {
                var baseline = row.Max(b => b.Baseline); var height = baseline + row.Max(b => b.Height - b.Baseline); double x = 0;
                for (var i = 0; i < row.Length; i++) { Add(geometry, row[i], x + (widths[i] - row[i].Width) / 2, y + baseline - row[i].Baseline); x += widths[i] + gap; }
                y += height + gap;
            }
            return new(geometry, widths.Sum() + gap * (widths.Length - 1), y - gap, (y - gap) / 2 + size * .28);
        }
    }
    private sealed record Fence(string Left, Node Content, string Right) : Node
    {
        internal override Box Layout(double size)
        {
            var content = Content.Layout(size); var left = new Run(Left == "." ? "" : Left).Layout(size); var right = new Run(Right == "." ? "" : Right).Layout(size);
            var geometry = new GeometryGroup(); var height = Math.Max(size, content.Height);
            if (Left != "." && left.Height > 0) Add(geometry, left, 0, 0, 1, height / left.Height);
            Add(geometry, content, left.Width + size * .08, (height - content.Height) / 2);
            if (Right != "." && right.Height > 0) Add(geometry, right, left.Width + content.Width + size * .16, 0, 1, height / right.Height);
            return new(geometry, left.Width + content.Width + right.Width + size * .16, height, (height - content.Height) / 2 + content.Baseline);
        }
    }
    private static void Add(GeometryGroup group, Box box, double x, double y, double sx = 1, double sy = 1) =>
        group.Children.Add(new GeometryGroup { Children = [box.Geometry], Transform = new MatrixTransform(Matrix.CreateScale(sx, sy) * Matrix.CreateTranslation(x, y)) });

    internal static bool Supported(string latex)
    {
        try { Parse(latex); return true; }
        catch (FormatException) { return false; }
    }
    internal static Geometry? Build(string latex, double size)
        => Typeset(latex, size)?.Geometry;
    internal static (Geometry Geometry, double Baseline)? Typeset(string latex, double size)
    {
        try { var box = Parse(latex).Layout(size); return (box.Geometry, box.Baseline); }
        catch (FormatException) { return null; }
    }
    private static Node Parse(string latex)
    {
        if (string.IsNullOrWhiteSpace(latex) || latex.Length > 4000) throw new FormatException("Formula size limit.");
        var parser = new Parser(latex); var result = parser.Read();
        if (parser.Position != latex.Length) throw new FormatException("Trailing formula tokens.");
        return result;
    }
    private sealed class Parser(string source)
    {
        internal int Position;
        private int _depth, _nodes;
        private bool At(string value) => source.AsSpan(Position).StartsWith(value, StringComparison.Ordinal);
        internal Node Read(params string[] stops)
        {
            if (++_depth > 32) throw new FormatException("Formula nesting limit.");
            var nodes = new List<Node>();
            while (Position < source.Length && !stops.Any(At))
            {
                if (++_nodes > 1024) throw new FormatException("Formula node limit.");
                var c = source[Position];
                // TeX whitespace separates commands; it must not become the
                // base of a following upper/lower limit.
                if (char.IsWhiteSpace(c)) { Position++; continue; }
                if (c is '^' or '_')
                {
                    Position++; if (nodes.Count == 0) throw new FormatException("Missing script base.");
                    var argument = ScriptArgument(); var previous = nodes[^1];
                    nodes[^1] = previous is Script s ? c == '^' ? s with { Sup = argument } : s with { Sub = argument } : new Script(previous, c == '_' ? argument : null, c == '^' ? argument : null);
                    continue;
                }
                if (c == '{') { nodes.Add(Argument()); continue; }
                if (c is '}' or '&') throw new FormatException("Unexpected formula delimiter.");
                if (c != '\\')
                {
                    var start = Position++;
                    if (char.IsLetter(c)) while (Position < source.Length && char.IsLetter(source[Position])) Position++;
                    nodes.Add(new Run(source[start..Position])); continue;
                }
                var command = Command();
                switch (command)
                {
                    case "limits": case "nolimits":
                        if (nodes.Count == 0 || nodes[^1] is not Run { Text: "∫" or "∑" or "∏" or "lim" })
                            throw new FormatException("Limit placement needs a large operator.");
                        nodes[^1] = new Script(nodes[^1], null, null, command == "limits"); break;
                    case "mathrm": case "operatorname": case "text":
                        Require("{"); var wordStart = Position;
                        while (Position < source.Length && (char.IsAsciiLetter(source[Position]) || source[Position] == ' ')) Position++;
                        if (Position - wordStart is 0 or > 32) throw new FormatException("Invalid formula label.");
                        var word = source[wordStart..Position]; Require("}"); nodes.Add(new Run(word, Upright: true)); break;
                    case "frac": case "dfrac": case "tfrac": nodes.Add(new Fraction(Argument(), Argument())); break;
                    case "sqrt": nodes.Add(new Radical(ScriptArgument())); break;
                    case "left":
                        var left = Delimiter(); var body = Read("\\right"); Require("\\right"); nodes.Add(new Fence(left, body, Delimiter())); break;
                    case "begin":
                        Require("{"); var start = Position;
                        while (Position < source.Length && char.IsAsciiLetter(source[Position])) Position++;
                        var environment = source[start..Position]; Require("}");
                        if (environment is not ("matrix" or "pmatrix" or "bmatrix" or "vmatrix" or "array" or "cases")) throw new FormatException("Unknown matrix environment.");
                        if (environment == "array")
                        {
                            Require("{"); var columns = 0;
                            while (Position < source.Length && source[Position] is 'l' or 'c' or 'r' or '|') { Position++; if (++columns > 24) throw new FormatException("Matrix column limit."); }
                            Require("}");
                        }
                        var end = "\\end{" + environment + "}";
                        var rows = new List<IReadOnlyList<Node>>(); var row = new List<Node>();
                        while (true)
                        {
                            row.Add(Read("&", "\\\\", end));
                            if (row.Count > 12 || rows.Count >= 12) throw new FormatException("Matrix dimension limit.");
                            if (At("&")) { Position++; continue; }
                            rows.Add(row); row = [];
                            if (At("\\\\")) { Position += 2; continue; }
                            Require(end); break;
                        }
                        Node table = new Table(rows);
                        nodes.Add(environment switch { "pmatrix" => new Fence("(", table, ")"), "bmatrix" => new Fence("[", table, "]"), "vmatrix" => new Fence("|", table, "|"), "cases" => new Fence("{", table, "."), _ => table }); break;
                    default:
                        if (!Symbols.TryGetValue(command, out var symbol)) throw new FormatException("Unsupported formula command.");
                        nodes.Add(new Run(symbol)); break;
                }
            }
            _depth--; return new Row(nodes);
        }
        private Node Argument() { Require("{"); var node = Read("}"); Require("}"); return node; }
        private Node ScriptArgument()
        {
            if (At("{")) return Argument();
            if (Position == source.Length) throw new FormatException("Missing formula script.");
            if (At("\\"))
            {
                var command = Command();
                if (!Symbols.TryGetValue(command, out var symbol)) throw new FormatException("Unknown script symbol.");
                return new Run(symbol);
            }
            var start = Position;
            if (char.IsAsciiDigit(source[Position])) while (Position < source.Length && char.IsAsciiDigit(source[Position])) Position++;
            else if (char.IsLetter(source[Position])) Position++;
            else throw new FormatException("Invalid formula script.");
            return new Run(source[start..Position]);
        }
        private void Require(string value) { if (!At(value)) throw new FormatException("Incomplete formula."); Position += value.Length; }
        private string Command()
        {
            Require("\\"); var start = Position;
            if (Position == source.Length) throw new FormatException("Incomplete command.");
            if (!char.IsLetter(source[Position])) return source[Position++].ToString();
            while (Position < source.Length && char.IsLetter(source[Position])) Position++;
            var result = source[start..Position]; if (At(" ")) Position++;
            return result;
        }
        private string Delimiter()
        {
            if (Position == source.Length) throw new FormatException("Missing fence.");
            if (At("\\")) { var command = Command(); if (Symbols.TryGetValue(command, out var symbol)) return symbol; throw new FormatException("Unknown fence."); }
            var value = source[Position++];
            return value is '(' or ')' or '[' or ']' or '|' or '.' ? value.ToString() : throw new FormatException("Unknown fence.");
        }
    }
    private static readonly Dictionary<string, string> Symbols = new(StringComparer.Ordinal)
    {
        ["alpha"]="α", ["beta"]="β", ["gamma"]="γ", ["delta"]="δ", ["epsilon"]="ε", ["theta"]="θ", ["lambda"]="λ", ["mu"]="μ", ["pi"]="π", ["sigma"]="σ", ["phi"]="φ", ["omega"]="ω",
        ["Gamma"]="Γ", ["Delta"]="Δ", ["Sigma"]="Σ", ["Omega"]="Ω", ["infty"]="∞", ["int"]="∫", ["sum"]="∑", ["prod"]="∏", ["partial"]="∂", ["nabla"]="∇",
        ["pm"]="±", ["times"]="×", ["div"]="÷", ["cdot"]="⋅", ["leq"]="≤", ["geq"]="≥", ["neq"]="≠", ["approx"]="≈", ["in"]="∈", ["notin"]="∉", ["to"]="→", ["Rightarrow"]="⇒",
        ["{"]="{", ["}"]="}", ["Vert"]="‖", ["langle"]="⟨", ["rangle"]="⟩", ["%"]="%", ["$"]="$", ["#"]="#", ["&"]="&", ["_"]="_", ["backslash"]="\\", ["sim"]="~",
        ["sin"]="sin", ["cos"]="cos", ["tan"]="tan", ["cot"]="cot", ["sec"]="sec", ["csc"]="csc", ["ln"]="ln", ["log"]="log", ["exp"]="exp", ["lim"]="lim",
        ["sinh"]="sinh", ["cosh"]="cosh", ["tanh"]="tanh", ["arcsin"]="arcsin", ["arccos"]="arccos", ["arctan"]="arctan", ["det"]="det", ["inv"]="inv", ["rank"]="rank", ["trace"]="trace", ["varnothing"]="∅", [","]=" ", [";"]=" ", ["!"]=""
    };
}
