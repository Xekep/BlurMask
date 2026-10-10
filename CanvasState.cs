using System.Text.Json.Serialization;
using BlurMask.Shared;

namespace BlurMask;

internal sealed record CanvasState
{
    public BoardSnapshot Board { get; init; } = new();
    public BoardSnapshot Baseline { get; init; } = new();
    public BoardConnection? Connection { get; init; }
    public BoardPatch? Pending { get; init; }
    public BoardSnapshot? PendingBefore { get; init; }
    public string? ViewKey { get; init; }
}
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(CanvasState))]
internal partial class CanvasJsonContext : JsonSerializerContext;

internal static class CanvasMerge
{
    public static BoardPatch Difference(BoardSnapshot before, BoardSnapshot after)
    {
        var previous = before.Marks.Select(m => m.Id).ToHashSet();
        var current = after.Marks.Select(m => m.Id).ToHashSet();
        return new()
        {
            Add = after.Marks.Where(m => !previous.Contains(m.Id)).ToList(),
            Remove = before.Marks.Where(m => !current.Contains(m.Id)).Select(m => m.Id).ToList(),
            Title = before.Title == after.Title ? null : after.Title,
            Background = before.Background == after.Background ? null : after.Background
        };
    }
    public static bool HasChanges(BoardPatch patch) => patch.Add.Count > 0 || patch.Remove.Count > 0 || patch.Title is not null || patch.Background is not null;
    public static BoardSnapshot Rebase(BoardSnapshot remote, BoardSnapshot before, BoardSnapshot current)
    {
        var patch = Difference(before, current);
        var removed = patch.Remove.ToHashSet();
        var marks = remote.Marks.Where(m => !removed.Contains(m.Id)).ToList();
        var ids = marks.Select(m => m.Id).ToHashSet();
        foreach (var mark in patch.Add) if (ids.Add(mark.Id)) marks.Add(mark);
        return remote with { Marks = marks, Title = patch.Title ?? remote.Title, Background = patch.Background ?? remote.Background };
    }
}
