namespace BlurMask.Server;

public sealed class LiveLimits
{
    public int MaxConnections { get; set; } = 4096;
    public int MaxPerAddress { get; set; } = 1024;
    public int MaxPerBoard { get; set; } = 128;
    public int MaxPreviewPoints { get; set; } = 2_000_000;
    public void Validate()
    {
        if (MaxConnections is < 1 or > 16384 || MaxPerAddress < 1 || MaxPerAddress > MaxConnections ||
            MaxPerBoard is < 2 or > 1024 || MaxPerBoard > MaxConnections || MaxPreviewPoints is < 40000 or > 8_000_000)
            throw new InvalidOperationException("Invalid Live connection limits.");
    }
}
