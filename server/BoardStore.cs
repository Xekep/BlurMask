using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using BlurMask.Shared;
using Npgsql;
using NpgsqlTypes;

namespace BlurMask.Server;

public sealed class BoardStore(NpgsqlDataSource source)
{
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await using var command = source.CreateCommand("""
            CREATE TABLE IF NOT EXISTS pen_boards (
                id uuid PRIMARY KEY,
                edit_hash bytea NOT NULL,
                view_hash bytea NOT NULL,
                revision bigint NOT NULL DEFAULT 0 CHECK (revision >= 0),
                document jsonb NOT NULL,
                created_at timestamptz NOT NULL DEFAULT now(),
                updated_at timestamptz NOT NULL DEFAULT now()
            );
            CREATE TABLE IF NOT EXISTS pen_operations (
                board_id uuid NOT NULL REFERENCES pen_boards(id) ON DELETE CASCADE,
                operation_id uuid NOT NULL,
                PRIMARY KEY (board_id, operation_id)
            );
            """);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<BoardAccess> CreateAsync(CreateBoardRequest request, string publicUrl, CancellationToken ct)
    {
        var board = new BoardSnapshot { Id = Guid.CreateVersion7(), Title = request.Title.Trim(), Background = request.Background };
        var edit = NewKey();
        var view = NewKey();
        await using var command = source.CreateCommand("""
            INSERT INTO pen_boards (id, edit_hash, view_hash, document) VALUES ($1, $2, $3, $4)
            """);
        command.Parameters.Add(new() { Value = board.Id });
        command.Parameters.Add(new() { Value = Hash(edit) });
        command.Parameters.Add(new() { Value = Hash(view) });
        command.Parameters.Add(new() { Value = JsonSerializer.Serialize(board, BoardJsonContext.Default.BoardSnapshot), NpgsqlDbType = NpgsqlDbType.Jsonb });
        await command.ExecuteNonQueryAsync(ct);
        return new(board, edit, view, Link(publicUrl, board.Id, edit), Link(publicUrl, board.Id, view));
    }

    public async Task<BoardSnapshot?> ReadAsync(Guid id, string key, CancellationToken ct)
    {
        if (!ValidKey(key)) return null;
        await using var command = source.CreateCommand("""
            SELECT document::text, revision, edit_hash FROM pen_boards
            WHERE id = $1 AND (edit_hash = $2 OR view_hash = $2)
            """);
        command.Parameters.Add(new() { Value = id });
        command.Parameters.Add(new() { Value = Hash(key) });
        await using var reader = await command.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct)) return null;
        return JsonSerializer.Deserialize(reader.GetString(0), BoardJsonContext.Default.BoardSnapshot)! with
        { Revision = reader.GetInt64(1), ReadOnly = !Matches(key, reader.GetFieldValue<byte[]>(2)) };
    }

    public async Task<(BoardSnapshot? Board, bool ReadOnly)> UpdateAsync(Guid id, string key, BoardPatch patch, CancellationToken ct)
    {
        if (!ValidKey(key)) return (null, false);
        await using var connection = await source.OpenConnectionAsync(ct);
        await using var transaction = await connection.BeginTransactionAsync(ct);
        await using var select = new NpgsqlCommand("""
            SELECT CASE WHEN edit_hash = $2 THEN document::text ELSE NULL END, revision, edit_hash
            FROM pen_boards WHERE id = $1 AND (edit_hash = $2 OR view_hash = $2) FOR UPDATE
            """, connection, transaction);
        select.Parameters.Add(new() { Value = id });
        select.Parameters.Add(new() { Value = Hash(key) });
        BoardSnapshot board;
        await using (var reader = await select.ExecuteReaderAsync(ct))
        {
            if (!await reader.ReadAsync(ct)) return (null, false);
            if (!Matches(key, reader.GetFieldValue<byte[]>(2))) return (null, true);
            board = JsonSerializer.Deserialize(reader.GetString(0), BoardJsonContext.Default.BoardSnapshot)! with { Revision = reader.GetInt64(1) };
        }
        await using var previousOperation = new NpgsqlCommand("""
            SELECT EXISTS(SELECT 1 FROM pen_operations WHERE board_id = $1 AND operation_id = $2)
            """, connection, transaction);
        previousOperation.Parameters.Add(new() { Value = id });
        previousOperation.Parameters.Add(new() { Value = patch.OperationId });
        if ((bool)(await previousOperation.ExecuteScalarAsync(ct))!) return (board, false);
        var removed = patch.Remove.ToHashSet();
        var marks = board.Marks.Where(m => !removed.Contains(m.Id)).ToList();
        var changed = marks.Count != board.Marks.Count;
        var updated = patch.Update.ToDictionary(m => m.Id);
        for (var i = 0; i < marks.Count; i++)
            if (updated.TryGetValue(marks[i].Id, out var replacement) && !BoardValidation.Same(marks[i], replacement))
            { marks[i] = replacement; changed = true; }
        var existing = marks.Select(m => m.Id).ToHashSet();
        foreach (var mark in patch.Add) if (existing.Add(mark.Id)) { marks.Add(mark); changed = true; }
        if (!changed && (patch.Title is null || patch.Title.Trim() == board.Title) &&
            (patch.Background is null || patch.Background == board.Background)) return (board, false);
        board = board with
        {
            Revision = board.Revision + 1, Marks = marks,
            Title = patch.Title?.Trim() ?? board.Title, Background = patch.Background ?? board.Background
        };
        if (!BoardValidation.Valid(board)) throw new BoardLimitException();
        var document = JsonSerializer.Serialize(board, BoardJsonContext.Default.BoardSnapshot);
        if (Encoding.UTF8.GetByteCount(document) > BoardValidation.MaxDocumentBytes) throw new BoardLimitException();
        await using var operation = new NpgsqlCommand("INSERT INTO pen_operations (board_id, operation_id) VALUES ($1, $2)", connection, transaction);
        operation.Parameters.Add(new() { Value = id });
        operation.Parameters.Add(new() { Value = patch.OperationId });
        await operation.ExecuteNonQueryAsync(ct);
        await using var update = new NpgsqlCommand("""
            UPDATE pen_boards SET document = $2, revision = $3, updated_at = now() WHERE id = $1
            """, connection, transaction);
        update.Parameters.Add(new() { Value = id });
        update.Parameters.Add(new() { Value = document, NpgsqlDbType = NpgsqlDbType.Jsonb });
        update.Parameters.Add(new() { Value = board.Revision });
        await update.ExecuteNonQueryAsync(ct);
        await transaction.CommitAsync(ct);
        return (board, false);
    }

    public async Task<bool> PingAsync(CancellationToken ct)
    {
        await using var command = source.CreateCommand("SELECT 1");
        return (int)(await command.ExecuteScalarAsync(ct))! == 1;
    }
    private static string NewKey() => Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    private static byte[] Hash(string key) => SHA256.HashData(Encoding.UTF8.GetBytes(key));
    private static bool Matches(string key, byte[] hash) => key.Length == 43 && CryptographicOperations.FixedTimeEquals(Hash(key), hash);
    public static bool ValidKey(string key) => key.Length == 43 && key.All(c => char.IsAsciiLetterOrDigit(c) || c is '-' or '_');
    private static string Link(string server, Guid id, string key) => $"{server.TrimEnd('/')}/#{BoardInvite.Code(id, key)}";
}
public sealed class BoardLimitException : Exception;
