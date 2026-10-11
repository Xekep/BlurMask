using System.Globalization;
using Avalonia;
using Avalonia.Media;

namespace BlurMask;

// Small native geometry typesetter for the structural LaTeX emitted by Math2.
// It never evaluates code or loads a browser. Unsupported notation stays ink.
internal static class MathTypesetting
{
    private sealed record Box(Geometry Geometry, double Width, double Height, double Baseline);
    private abstract record Node { internal abstract Box Layout(double size); }
    private sealed record Run(string Text) : Node
    {
        internal override Box Layout(double size)
        {
            var variable = Text.Any(char.IsLetter) && Text is not ("sin" or "cos" or "tan" or "log" or "ln" or "exp" or "lim" or "max" or "min" or "sinh" or "cosh" or "tanh" or "arcsin" or "arccos" or "arctan" or "det" or "gcd");
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
    private sealed record Script(Node Base, Node? Sub, Node? Sup) : Node
    {
        internal override Box Layout(double size)
        {
            var body = Base.Layout(size); var sub = Sub?.Layout(size * .65); var sup = Sup?.Layout(size * .65);
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
    {
        try { return Parse(latex).Layout(size).Geometry; }
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
                if (c is '^' or '_')
                {
                    Position++; if (nodes.Count == 0) throw new FormatException("Missing script base.");
                    var argument = Argument(); var previous = nodes[^1];
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
                    case "frac": nodes.Add(new Fraction(Argument(), Argument())); break;
                    case "sqrt": nodes.Add(new Radical(Argument())); break;
                    case "left":
                        var left = Delimiter(); var body = Read("\\right"); Require("\\right"); nodes.Add(new Fence(left, body, Delimiter())); break;
                    case "begin":
                        Require("{matrix}"); var rows = new List<IReadOnlyList<Node>>(); var row = new List<Node>();
                        while (true)
                        {
                            row.Add(Read("&", "\\\\", "\\end{matrix}"));
                            if (At("&")) { Position++; continue; }
                            rows.Add(row); row = [];
                            if (At("\\\\")) { Position += 2; continue; }
                            Require("\\end{matrix}"); break;
                        }
                        nodes.Add(new Table(rows)); break;
                    default:
                        if (!Symbols.TryGetValue(command, out var symbol)) throw new FormatException("Unsupported formula command.");
                        nodes.Add(new Run(symbol)); break;
                }
            }
            _depth--; return new Row(nodes);
        }
        private Node Argument() { Require("{"); var node = Read("}"); Require("}"); return node; }
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
        ["{"]="{", ["}"]="}", ["Vert"]="‖", ["langle"]="⟨", ["rangle"]="⟩", ["%"]="%", ["$"]="$", ["#"]="#", ["&"]="&", ["_"]="_", ["backslash"]="\\", ["sim"]="~"
    };
}
