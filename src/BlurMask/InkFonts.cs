using Avalonia.Media;

namespace BlurMask;

internal static class InkFonts
{
    internal static readonly string[] Names = ["Inter", "Manrope", "Source Serif 4", "STIX Two Math"];
    internal static readonly Typeface MathVariable = new(new FontFamily("avares://BlurMask/Assets/Fonts#STIX Two Text"), FontStyle.Italic);
    private static readonly Dictionary<string, Typeface> Faces = new(StringComparer.Ordinal);
    internal static Typeface Face(string name)
    {
        if (string.IsNullOrEmpty(name)) name = "Inter";
        if (!Names.Contains(name)) return Typeface.Default;
        if (!Faces.TryGetValue(name, out var face))
            Faces[name] = face = new Typeface(new FontFamily($"avares://BlurMask/Assets/Fonts#{name}, avares://BlurMask/Assets/Fonts#STIX Two Math"));
        return face;
    }
}
