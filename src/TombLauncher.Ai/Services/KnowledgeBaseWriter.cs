using System.Data;
using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Options;
using TombLauncher.Ai.Configuration;
using TombLauncher.Ai.Models;

namespace TombLauncher.Ai.Services;

public class KnowledgeBaseWriter
{
    private KnowledgeBaseEmbedderConfiguration _embedderConfiguration;
    public KnowledgeBaseWriter(IOptions<KnowledgeBaseEmbedderConfiguration> embedderConfiguration)
    {
        _embedderConfiguration = embedderConfiguration.Value;
    }
    private const string CreateEmbeddingsTable = @"
CREATE TABLE knowledge_chunks
(
    id INTEGER PRIMARY KEY,
    document_title TEXT,
    section_title TEXT,
    chunk_text TEXT,
    embedding BLOB
) IF NOT EXISTS";

    private const string InsertEmbeddingQuery = @"
    INSERT INTO knowledge_chunks(document_title, section_title, chunk_text, embedding)
    VALUES (?, ?, ?, ?)
";

    private Task WriteChunk(IDbTransaction transaction, Chunk chunk)
    {
        return transaction.Connection!.ExecuteAsync(InsertEmbeddingQuery, chunk);
    }

    public async Task WriteChunks(IEnumerable<Chunk> chunks, CancellationToken cancellationToken)
    {
        using var connection = GetConnection($"Data Source={_embedderConfiguration.KnowledgeBasePath}");
        await EnsureTables(connection);
        var transaction = CreateTransaction(connection, cancellationToken);
        foreach (var chunk in chunks)
        {
            await WriteChunk(transaction, chunk);
        }
        
        transaction.Commit();
    }

    private IDbTransaction CreateTransaction(IDbConnection connection, CancellationToken cancellationToken)
    {
        var transaction = connection.BeginTransaction();
        cancellationToken.Register(() => transaction.Rollback());
        return transaction;
    }

    private async Task EnsureTables(IDbConnection connection)
    {
        await connection.ExecuteAsync(new CommandDefinition(CreateEmbeddingsTable));
    }
    private IDbConnection GetConnection(string connectionString)
    {
        var connection = new SqliteConnection(connectionString);
        connection.EnableExtensions();
        connection.LoadExtension("vector");
        return connection;
    }
}