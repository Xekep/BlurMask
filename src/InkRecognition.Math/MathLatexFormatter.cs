using System.Text;

namespace InkRecognition.Math;

// A token parser, independent of the recognized sample. Unsupported structural
// constructions return null while the original recognized token stream is kept.
public static class MathLatexFormatter
{
    public static string? Format(IReadOnlyList<string> tokens)
    {
        try
        {
            var parser = new Parser(tokens);
            var result = parser.Read(null);
            return parser.Position == tokens.Count ? result : null;
        }
        catch (FormatException) { return null; }
    }

    private sealed class Parser(IReadOnlyList<string> tokens)
    {
        public int Position { get; private set; }
        private int _depth;
        public string Read(string? stop)
        {
            if (++_depth > 32) throw new FormatException("Math nesting exceeds limit.");
            var text = new StringBuilder();
            while (Position < tokens.Count)
            {
                string t = tokens[Position++];
                if (t == stop) { _depth--; return text.ToString(); }
                switch (t)
                {
                    case "√": text.Append("\\sqrt{").Append(Read("</sqrt>")).Append('}'); break;
                    case "<frac>":
                        text.Append("\\frac{").Append(Read("<fracbar>")).Append("}{").Append(Read("</frac>")).Append('}'); break;
                    case "<slantedfrac>":
                        text.Append("{").Append(Read("<slantedfracbar>")).Append("}/{").Append(Read("</slantedfrac>")).Append('}'); break;
                    case "<sup>": text.Append("^{").Append(Read("</sup>")).Append('}'); break;
                    case "<sub>": text.Append("_{").Append(Read("</sub>")).Append('}'); break;
                    case "<subsup>":
                        text.Append("_{").Append(Read("<subsup2>")).Append("}^{").Append(Read("</subsup>")).Append('}'); break;
                    case "<table>": text.Append("\\begin{matrix}").Append(Read("</table>")).Append("\\end{matrix}"); break;
                    case "<&>": text.Append('&'); break;
                    case "<\\>": text.Append("\\\\"); break;
                    case "<fence>":
                        if (Position >= tokens.Count) throw new FormatException("Missing fence.");
                        string left = tokens[Position++];
                        if (left is not ("{" or "(" or "[" or "|" or "‖" or "⟨")) throw new FormatException("Unsupported fence.");
                        string inner = Read("</fence>");
                        char closing = left switch { "(" => ')', "[" => ']', "{" => '}', "|" => '|', "‖" => '‖', _ => '⟩' };
                        bool explicitRight = Position >= 2 && tokens[Position - 2] == closing.ToString();
                        if (explicitRight) inner = inner[..^Escape(closing.ToString()).Length];
                        text.Append("\\left").Append(Delimiter(left)).Append(inner).Append("\\right").Append(explicitRight ? Delimiter(closing.ToString()) : "."); break;
                    default:
                        if (t.StartsWith('<') && t.EndsWith('>') && t.Length > 2) throw new FormatException("Unsupported structural token.");
                        text.Append(Escape(t)); break;
                }
            }
            _depth--;
            if (stop != null) throw new FormatException("Unclosed structural token.");
            return text.ToString();
        }
    }

    private static string Delimiter(string token) => token switch { "{" => "\\{", "}" => "\\}", "‖" => "\\Vert ", "⟨" => "\\langle ", "⟩" => "\\rangle ", _ => token };
    private static string Escape(string token) => token switch
    {
        "α" => "\\alpha ", "β" => "\\beta ", "γ" => "\\gamma ", "δ" => "\\delta ", "ε" => "\\epsilon ", "θ" => "\\theta ", "λ" => "\\lambda ", "μ" => "\\mu ", "π" => "\\pi ", "σ" => "\\sigma ", "φ" => "\\phi ", "ω" => "\\omega ",
        "Γ" => "\\Gamma ", "Δ" => "\\Delta ", "Σ" => "\\Sigma ", "Ω" => "\\Omega ",
        "∞" => "\\infty ", "∫" => "\\int ", "∑" => "\\sum ", "∏" => "\\prod ", "∂" => "\\partial ", "∇" => "\\nabla ", "±" => "\\pm ", "×" => "\\times ", "÷" => "\\div ", "·" or "⋅" => "\\cdot ",
        "≤" => "\\leq ", "≥" => "\\geq ", "≠" => "\\neq ", "≈" => "\\approx ", "∈" => "\\in ", "∉" => "\\notin ", "→" => "\\to ", "⇒" => "\\Rightarrow ",
        "{" => "\\{", "}" => "\\}", "%" => "\\%", "$" => "\\$", "#" => "\\#", "&" => "\\&", "_" => "\\_", "\\" => "\\backslash ", "^" => "\\hat{}", "~" => "\\sim ", _ => token
    };
}
