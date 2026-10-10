namespace BlurMask.Shared;

// Preview packets are ephemeral. Only the regular changes API commits a mark.
public sealed record LiveFrame
{
    public string Type { get; init; } = "preview";
    public Guid Peer { get; init; }
    public InkMarkDto? Mark { get; init; }
    public int Offset { get; init; }
    public long Revision { get; init; }
}

public static class LiveProtocol
{
    public const int MaxPacketBytes = 64 * 1024;
    public const int ChunkPoints = 512;
    public static bool TryMerge(LiveFrame frame, InkMarkDto? previous, out InkMarkDto? merged)
    {
        merged = null;
        if (frame.Type != "preview" || frame.Mark is not { } mark ||
            !BoardValidation.Valid(mark) || mark.Tool is "Image" || mark.Points.Count > ChunkPoints || frame.Offset < 0) return false;
        if (frame.Offset == 0) { merged = mark; return true; }
        if (previous is null || previous.Id != mark.Id || previous.Tool is not ("Pen" or "Highlighter") ||
            previous.Tool != mark.Tool || previous.Color != mark.Color || previous.Width != mark.Width ||
            previous.Text != mark.Text || previous.FontSize != mark.FontSize ||
            previous.Points.Count != frame.Offset || frame.Offset + mark.Points.Count > 20000) return false;
        merged = previous with { Points = [.. previous.Points, .. mark.Points] };
        return true;
    }
    public static IEnumerable<LiveFrame> Chunks(InkMarkDto mark, int offset = 0)
    {
        for (var i = offset; i < mark.Points.Count; i += ChunkPoints)
            yield return new() { Mark = mark with { Points = mark.Points.Skip(i).Take(ChunkPoints).ToList() }, Offset = i };
    }
}
