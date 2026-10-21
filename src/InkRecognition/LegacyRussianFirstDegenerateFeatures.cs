using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("LegacyRussianFirstDegenerate.Probe")]

namespace InkRecognition;

/// <summary>Native00A9F0 assignments for the exact owned Russian source6120
/// profile1011/3F only. This does not widen generic glyph geometry.</summary>
internal static class LegacyRussianFirstDegenerateFeatures
{
    internal const string SupportedSourceFingerprint = "AA2AF98C8C550603E797F304DB204AEDD1102482CC8A6DDCC0ED8B62CF8113D2";

    internal static bool TryCreate(LegacyFirstAcousticModel model,
        IReadOnlyList<LegacyInkStroke> merged, CancellationToken token,
        out short[] trajectory, out short[] raster)
    {
        ArgumentNullException.ThrowIfNull(model); ArgumentNullException.ThrowIfNull(merged);
        token.ThrowIfCancellationRequested(); trajectory=[]; raster=[];
        //The private model reader owns this fingerprint over the complete
        //selected source, including the exact feature settings and both nets.
        if (!StringComparer.Ordinal.Equals(model.SourceFingerprint, SupportedSourceFingerprint) ||
            model.OrdinaryNodeOffset!=13082 || model.NormalizationTarget!=100 ||
            model.TrajectoryNetwork.Kind!=0x1f || model.TrajectoryNetwork.InputWidth!=247 ||
            model.RasterNetwork.Kind!=0x2f || model.RasterNetwork.InputWidth!=229)
            return false;
        if (merged.Count is <1 or >15) throw new ArgumentException("Bounded actual merged geometry required.",nameof(merged));
        int total=0; LegacyInkPoint? first=null;
        foreach (var stroke in merged)
        {
            token.ThrowIfCancellationRequested(); ArgumentNullException.ThrowIfNull(stroke);
            if (stroke.Points.Length>4096-total) throw new ArgumentException("Merged geometry exceeds4096 points.",nameof(merged));
            total+=stroke.Points.Length;
            foreach (var point in stroke.Points)
            {
                token.ThrowIfCancellationRequested();
                if (first is {} origin && point!=origin) return false;
                first=point;
            }
        }
        if (total==0 || first is null) throw new ArgumentException("Nonempty merged geometry required.",nameof(merged));
        //These are actual degenerate feature assignments, not zero placeholders.
        trajectory=new short[217]; trajectory.AsSpan(6,30).Fill(1024);
        ReadOnlySpan<short> statistics=[1024,-768,0,0,-768,1024,-1024];
        statistics.CopyTo(trajectory.AsSpan(210));
        raster=new short[199]; statistics.CopyTo(raster);
        raster.AsSpan(7,64).Fill(-1024); raster.AsSpan(71,128).Fill(1024);
        token.ThrowIfCancellationRequested(); return true;
    }
}
