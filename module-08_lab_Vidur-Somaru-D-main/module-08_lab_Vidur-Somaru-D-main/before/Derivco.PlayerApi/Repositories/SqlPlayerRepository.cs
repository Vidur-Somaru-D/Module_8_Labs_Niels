namespace Derivco.PlayerApi.Repositories;

using System.Data;
using Dapper;
using Derivco.PlayerApi.Models;
using Derivco.PlayerApi.Options;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

public class SqlPlayerRepository : IPlayerRepository
{
    private readonly string _connectionString;

    public SqlPlayerRepository(IOptions<DatabaseOptions> options)
    {
        _connectionString = options.Value.ConnectionString;
    }

    public Player? GetById(int id)
    {
        using var conn = new SqlConnection(_connectionString);
        return conn.QuerySingleOrDefault<Player>(
            "SELECT PlayerId, Username, Balance, IsActive, Region FROM Players WHERE PlayerId = @PlayerId",
            new { PlayerId = id });
    }

    public Player Create(Player player)
    {
        using var conn = new SqlConnection(_connectionString);
        var newId = conn.ExecuteScalar<int>(
            """
            INSERT INTO Players (Username, Balance, IsActive, Region)
            OUTPUT INSERTED.PlayerId
            VALUES (@Username, @Balance, 1, @Region)
            """,
            new { player.Username, player.Balance, player.Region });
        player.PlayerId = newId;
        player.IsActive = true;
        return player;
    }

    public Player? UpdateBalance(int playerId, decimal newBalance)
    {
        using var conn = new SqlConnection(_connectionString);
        conn.Execute(
            "UPDATE Players SET Balance = @Balance, UpdatedAt = GETUTCDATE() WHERE PlayerId = @PlayerId",
            new { Balance = newBalance, PlayerId = playerId });
        return GetById(playerId);
    }

    // N+1 FIXED: single query fetching active players only.
    // The naive version fetched all players, then called GetTransactionsByPlayer in a loop —
    // 1 query for the player list + N queries for N players' transactions = N+1 total.
    // The fix: one SELECT for the list. Transactions are only fetched on demand (GET /players/{id}/transactions).
    public IEnumerable<Player> GetAll()
    {
        using var conn = new SqlConnection(_connectionString);
        return conn.Query<Player>(
            "SELECT PlayerId, Username, Balance, IsActive, Region FROM Players WHERE IsActive = 1");
    }

    public IEnumerable<Transaction> GetTransactionsByPlayer(int playerId)
    {
        using var conn = new SqlConnection(_connectionString);
        return conn.Query<Transaction>(
            """
            SELECT TransactionId, PlayerId, Amount, TransactionType, CreatedAt
            FROM Transactions
            WHERE PlayerId = @PlayerId
            ORDER BY CreatedAt DESC
            """,
            new { PlayerId = playerId });
    }

    public void TransferFunds(int fromPlayerId, int toPlayerId, decimal amount)
    {
        using var conn = new SqlConnection(_connectionString);
        conn.Open();
        using var tx = conn.BeginTransaction();
        try
        {
            // UPDLOCK prevents a concurrent transfer from reading the same balance before either commits.
            // Without it two concurrent transfers could both read the same balance and both succeed,
            // overdrawing the account — a classic TOCTOU (time-of-check/time-of-use) race condition.
            var fromBalance = conn.ExecuteScalar<decimal>(
                "SELECT Balance FROM Players WITH (UPDLOCK) WHERE PlayerId = @PlayerId",
                new { PlayerId = fromPlayerId }, tx);

            if (fromBalance < amount)
                throw new InvalidOperationException(
                    $"Insufficient funds: balance is {fromBalance:C}, transfer amount is {amount:C}");

            // Debit source
            conn.Execute(
                "UPDATE Players SET Balance = Balance - @Amount, UpdatedAt = GETUTCDATE() WHERE PlayerId = @PlayerId",
                new { Amount = amount, PlayerId = fromPlayerId }, tx);

            // Credit destination
            conn.Execute(
                "UPDATE Players SET Balance = Balance + @Amount, UpdatedAt = GETUTCDATE() WHERE PlayerId = @PlayerId",
                new { Amount = amount, PlayerId = toPlayerId }, tx);

            // Record debit transaction
            conn.Execute(
                """
                INSERT INTO Transactions (PlayerId, Amount, TransactionType)
                VALUES (@PlayerId, @Amount, 'Transfer')
                """,
                new { PlayerId = fromPlayerId, Amount = -amount }, tx);

            // Record credit transaction
            conn.Execute(
                """
                INSERT INTO Transactions (PlayerId, Amount, TransactionType)
                VALUES (@PlayerId, @Amount, 'Transfer')
                """,
                new { PlayerId = toPlayerId, Amount = amount }, tx);

            tx.Commit();
        }
        catch
        {
            tx.Rollback();
            throw;
        }
    }
}
