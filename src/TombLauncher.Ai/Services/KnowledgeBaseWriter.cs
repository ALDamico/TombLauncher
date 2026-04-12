using System.Data;
using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using TombLauncher.Ai.Configuration;
using TombLauncher.Ai.Models;

namespace TombLauncher.Ai.Services;

public class KnowledgeBaseWriter
{
    private readonly KnowledgeBaseEmbedderConfiguration _embedderConfiguration;
    
    public KnowledgeBaseWriter(IOptions<KnowledgeBaseEmbedderConfiguration> embedderConfiguration)
    {
        _embedderConfiguration = embedderConfiguration.Value;
    }
    private const string CreateEmbeddingsTable = @"
CREATE TABLE IF NOT EXISTS knowledge_chunks 
(
    id INTEGER PRIMARY KEY,
    document_title TEXT,
    section_title TEXT,
    chunk_text TEXT,
    embedding BLOB
);";

    private const string InsertEmbeddingQuery = @"
    INSERT INTO knowledge_chunks(document_title, section_title, chunk_text, embedding)
    VALUES (@DocumentTitle, @SectionTitle, @ChunkText, vector_as_f32(@Embedding))
";

    private const string VectorInit = "SELECT vector_init('knowledge_chunks', 'embedding', 'type=FLOAT32,dimension=768');";

    private const string VectorQuantize = "SELECT vector_quantize('knowledge_chunks', 'embedding');";

    private Task WriteChunk(IDbTransaction transaction, Chunk chunk)
    {
        return transaction.Connection!.ExecuteAsync(InsertEmbeddingQuery, new {DocumentTitle = chunk.DocumentTitle, SectionTitle =chunk.SectionTitle, ChunkText = chunk.ChunkText, Embedding = JsonConvert.SerializeObject(chunk.Embedding)});
    }

    public async Task WriteChunks(IEnumerable<Chunk> chunks, CancellationToken cancellationToken)
    {
        using var connection = GetConnection($"Data Source={_embedderConfiguration.KnowledgeBasePath}");
        connection.Open();
        await EnsureTables(connection);
        var transaction = CreateTransaction(connection, cancellationToken);
        foreach (var chunk in chunks)
        {
            await WriteChunk(transaction, chunk);
        }
        
        transaction.Commit();
        await connection.ExecuteAsync(VectorInit);
        await connection.ExecuteAsync(VectorQuantize);
    }

    private IDbTransaction CreateTransaction(IDbConnection connection, CancellationToken cancellationToken)
    {
        var transaction = connection.BeginTransaction();
        cancellationToken.Register(() =>
        {
            if (transaction.Connection != null)
                transaction.Rollback();
        });
        return transaction;
    }

    private async Task EnsureTables(IDbConnection connection)
    {
        await connection.ExecuteAsync(new CommandDefinition(CreateEmbeddingsTable));
        await connection.ExecuteAsync("DELETE FROM knowledge_chunks");
    }
    
    private IDbConnection GetConnection(string connectionString)
    {
        var connection = new SqliteConnection(connectionString);
        connection.EnableExtensions();
        connection.LoadExtension("./vector");
        return connection;
    }
}