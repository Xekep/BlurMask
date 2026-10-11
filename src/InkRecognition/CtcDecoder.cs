using System.Text;

namespace InkRecognition;

internal static class CtcDecoder
{
    // Standard CTC collapse. This is an acoustic baseline without a language dictionary.
    internal static TextRecognition Decode(float[] probabilities, int frames, string?[] labels, int blank)
    {
        if (frames < 1 || labels.Length < 2 || probabilities.Length != frames * labels.Length || (uint)blank >= labels.Length)
            throw new InvalidDataException("Invalid recognition output dimensions.");
        var text = new StringBuilder();
        var previous = -1; var count = 0; var score = 0d;
        for (var frame = 0; frame < frames; frame++)
        {
            var offset = frame * labels.Length; var best = 0;
            for (var token = 0; token < labels.Length; token++)
            {
                var value = probabilities[offset + token];
                if (!float.IsFinite(value) || value < 0 || value > 1.00001f)
                    throw new InvalidDataException("Invalid recognition probability.");
                if (value > probabilities[offset + best]) best = token;
            }
            if (best != blank && best != previous)
            {
                // A non-text model token must never become printable replacement text.
                if (string.IsNullOrEmpty(labels[best])) return new("", 0, frames);
                if (text.Length + labels[best]!.Length > 16384) throw new InvalidDataException("Recognized text exceeds the output limit.");
                text.Append(labels[best]); count++;
                score += Math.Log(Math.Max(probabilities[offset + best], 1e-30f));
            }
            previous = best;
        }
        return new(text.ToString(), count == 0 ? 0 : (float)Math.Exp(score / count), frames);
    }
}
