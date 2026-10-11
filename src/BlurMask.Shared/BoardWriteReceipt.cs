using System.Text.Json.Serialization;

namespace BlurMask.Shared;

// Opt-in HTTP acknowledgement; ordinary API callers continue receiving snapshots.
public sealed record BoardWriteReceipt
{
    public const string MediaType = "application/vnd.blurmask.board-receipt+json";
    public const string BaseRevisionHeader = "X-BlurMask-Base-Revision";
    public Guid BoardId { get; init; }
    public Guid OperationId { get; init; }
    public long BaseRevision { get; init; }
    public long Revision { get; init; }
    public string Title { get; init; } = "";
    public string Background { get; init; } = "";
    public int Width { get; init; }
    public int Height { get; init; }
    public BoardPatch? Patch { get; init; }

    public bool TryApply(BoardSnapshot before, Guid operationId, out BoardSnapshot? after)
    {
        after = null;
        if (before.ReadOnly || before.Marks is null || before.Marks.Any(m => m is null) || BoardId == Guid.Empty ||
            BoardId != before.Id || operationId == Guid.Empty || OperationId != operationId ||
            BaseRevision < 0 || BaseRevision != before.Revision) return false;
        BoardSnapshot? candidate;
        if (Patch is null)
        {
            if (Revision != BaseRevision || !BoardValidation.Valid(before)) return false;
            candidate = before;
        }
        else if (BaseRevision == long.MaxValue || Patch.OperationId != operationId ||
            !LiveProtocol.TryApply(before, new() { Type = "changed", BaseRevision = BaseRevision, Revision = Revision, Patch = Patch }, out candidate)) return false;
        if (candidate is null || candidate.Revision != Revision || candidate.Title != Title ||
            candidate.Background != Background || candidate.Width != Width || candidate.Height != Height) return false;
        after = candidate;
        return true;
    }
}

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(BoardWriteReceipt))]
public partial class BoardWriteReceiptJsonContext : System.Text.Json.Serialization.JsonSerializerContext;
