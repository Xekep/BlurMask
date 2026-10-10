using System.Text.Json;
using System.Text.Json.Serialization;

namespace BlurMask.Shared;

public sealed record InkPointDto(double X, double Y);
public sealed record InkMarkDto
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string Tool { get; init; } = "Pen";
    public string Color { get; init; } = "#111111";
    public double Width { get; init; } = 4;
    public List<InkPointDto> Points { get; init; } = [];
    public string Text { get; init; } = "";
    public double FontSize { get; init; } = 28;
    public string? Image { get; init; }
}
public sealed record BoardSnapshot
{
    public Guid Id { get; init; }
    public long Revision { get; init; }
    public bool ReadOnly { get; init; }
    public string Title { get; init; } = "Новое полотно";
    public string Background { get; init; } = "white";
    public int Width { get; init; } = 2400;
    public int Height { get; init; } = 1600;
    public List<InkMarkDto> Marks { get; init; } = [];
}
public sealed record BoardPatch
{
    // A unique operation id makes retries safe, including a lost HTTP response.
    public Guid OperationId { get; init; } = Guid.NewGuid();
    public List<InkMarkDto> Add { get; init; } = [];
    // A setter preserves the default for older requests which omit this field
    // when deserialized by the .NET 10 source-generated serializer.
    public List<InkMarkDto> Update { get; set; } = [];
    public List<Guid> Remove { get; init; } = [];
    public string? Title { get; init; }
    public string? Background { get; init; }
}
public sealed record CreateBoardRequest(string Title = "Новое полотно", string Background = "white");
public sealed record BoardAccess(BoardSnapshot Board, string EditKey, string ViewKey, string EditUrl, string ViewUrl);
public sealed record BoardConnection(string Server, Guid Id, string Key);

public static class BoardInvite
{
    // Pack the UUID and the complete 256-bit access key into one URL-safe code.
    public static string Code(Guid id, string key)
    {
        if (id == Guid.Empty || key.Length != 43 || !key.All(c => char.IsAsciiLetterOrDigit(c) || c is '-' or '_'))
            throw new ArgumentException("Некорректный код доступа.");
        var secret = Convert.FromBase64String(key.Replace('-', '+').Replace('_', '/') + "=");
        if (Encode(secret) != key) throw new ArgumentException("Некорректный код доступа.");
        return Encode([.. Convert.FromHexString(id.ToString("N")), .. secret]);
    }

    public static (Guid Id, string Key) Parse(string code)
    {
        if (code.Length != 64 || !code.All(c => char.IsAsciiLetterOrDigit(c) || c is '-' or '_'))
            throw new ArgumentException("Вставьте корректный код приглашения или ссылку.");
        var bytes = Convert.FromBase64String(code.Replace('-', '+').Replace('_', '/'));
        var id = Guid.ParseExact(Convert.ToHexString(bytes.AsSpan(0, 16)), "N");
        if (id == Guid.Empty) throw new ArgumentException("Некорректный код доступа.");
        return (id, Encode(bytes[16..]));
    }

    private static string Encode(byte[] bytes) => Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
}

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(BoardSnapshot))]
[JsonSerializable(typeof(BoardPatch))]
[JsonSerializable(typeof(CreateBoardRequest))]
[JsonSerializable(typeof(BoardAccess))]
[JsonSerializable(typeof(BoardConnection))]
public partial class BoardJsonContext : JsonSerializerContext;

public static class BoardValidation
{
    public const int MaxMarks = 5000;
    public const int MaxPoints = 200000;
    public const int MaxDocumentBytes = 4 * 1024 * 1024;
    public static bool Valid(InkMarkDto? mark) => mark is not null && mark.Id != Guid.Empty &&
        mark.Tool is "Pen" or "Highlighter" or "Line" or "Arrow" or "Rectangle" or "Ellipse" or "Text" or "Image" &&
        mark.Color is { Length: 7 or 9 } && mark.Color[0] == '#' && mark.Color.Skip(1).All(Uri.IsHexDigit) &&
        double.IsFinite(mark.Width) && mark.Width is >= 0.5 and <= 200 &&
        double.IsFinite(mark.FontSize) && mark.FontSize is >= 8 and <= 300 &&
        mark.Text is { Length: <= 4000 } && mark.Points is { Count: > 0 and <= 20000 } &&
        mark.Points.All(p => p is not null && double.IsFinite(p.X) && double.IsFinite(p.Y) && Math.Abs(p.X) <= 100000 && Math.Abs(p.Y) <= 100000) &&
        (mark.Tool == "Image" ? mark.Points.Count == 2 && ValidImage(mark.Image) : mark.Image is null);
    public static bool ValidImage(string? image)
    {
        if (image is null || image.Length > 2 * 1024 * 1024 || !image.StartsWith("iVBORw0KGgo", StringComparison.Ordinal)) return false;
        try
        {
            var bytes = Convert.FromBase64String(image);
            if (bytes.Length < 33 || !bytes.AsSpan(0, 8).SequenceEqual(new byte[] {137, 80, 78, 71, 13, 10, 26, 10}) ||
                !bytes.AsSpan(12, 4).SequenceEqual("IHDR"u8)) return false;
            var width = System.Buffers.Binary.BinaryPrimitives.ReadUInt32BigEndian(bytes.AsSpan(16, 4));
            var height = System.Buffers.Binary.BinaryPrimitives.ReadUInt32BigEndian(bytes.AsSpan(20, 4));
            return width is > 0 and <= 4096 && height is > 0 and <= 4096 && (long)width * height <= 16 * 1024 * 1024;
        }
        catch (FormatException) { return false; }
    }
    public static bool Same(InkMarkDto a, InkMarkDto b) => a.Tool == b.Tool && a.Color == b.Color && a.Width == b.Width &&
        a.FontSize == b.FontSize && a.Text == b.Text && a.Image == b.Image && a.Points.SequenceEqual(b.Points);
    public static bool Valid(BoardPatch? patch) => patch is not null && patch.OperationId != Guid.Empty &&
        patch.Add is { Count: <= MaxMarks } && patch.Update is { Count: <= MaxMarks } && patch.Remove is { Count: <= MaxMarks } &&
        patch.Add.All(Valid) && patch.Update.All(Valid) && patch.Add.Concat(patch.Update).Sum(m => m.Points.Count) <= MaxPoints &&
        patch.Add.Concat(patch.Update).Select(m => m.Id).Distinct().Count() == patch.Add.Count + patch.Update.Count &&
        !patch.Update.Any(m => patch.Remove.Contains(m.Id)) &&
        patch.Add.Select(m => m.Id).Distinct().Count() == patch.Add.Count &&
        (patch.Title is null || (!string.IsNullOrWhiteSpace(patch.Title) && patch.Title.Length <= 120)) &&
        (patch.Background is null or "white" or "black");
    public static bool Valid(BoardSnapshot? board) => board is not null &&
        board.Width is >= 100 and <= 10000 && board.Height is >= 100 and <= 10000 &&
        board.Title is { Length: > 0 and <= 120 } && board.Background is "white" or "black" &&
        board.Marks is { Count: <= MaxMarks } && board.Marks.All(Valid) &&
        board.Marks.Sum(m => m.Points.Count) <= MaxPoints &&
        board.Marks.Select(m => m.Id).Distinct().Count() == board.Marks.Count;
}
