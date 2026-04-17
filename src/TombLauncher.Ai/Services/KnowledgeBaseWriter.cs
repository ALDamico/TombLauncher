using System.Data;
using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using TombLauncher.Ai.Configuration;
using TombLauncher.Ai.Models;
using TombLauncher.Contracts.Enums;

namespace TombLauncher.Ai.Services;

public class KnowledgeBaseWriter
{
    private readonly KnowledgeBaseEmbedderConfiguration _embedderConfiguration;

    public KnowledgeBaseWriter(IOptions<KnowledgeBaseEmbedderConfiguration> embedderConfiguration)
    {
        _embedderConfiguration = embedderConfiguration.Value;
    }

    // If JOIN performance on metadata_id becomes an issue, add: CREATE INDEX IF NOT EXISTS idx_chunks_metadata_id ON knowledge_chunks(metadata_id)
    private const string CreateEmbeddingsTable = @"
CREATE TABLE IF NOT EXISTS knowledge_chunks
(
    id INTEGER PRIMARY KEY,
    document_title TEXT,
    section_title TEXT,
    chunk_text TEXT,
    embedding BLOB,
    metadata_id INT NULL,
    FOREIGN KEY (metadata_id) REFERENCES knowledge_metadata(id)
);";

    private const string CreateMetadataTable = @"
CREATE TABLE IF NOT EXISTS knowledge_metadata
(
    id INTEGER PRIMARY KEY,
    app_version_range TEXT NULL,
    engine_version TEXT NULL,
    applies_to TEXT NULL,
    platforms TEXT NULL,
    source TEXT NULL
);";

    private const string InsertEmbeddingQuery = @"
    INSERT INTO knowledge_chunks(document_title, section_title, chunk_text, embedding, metadata_id)
    VALUES (@DocumentTitle, @SectionTitle, @ChunkText, vector_as_f32(@Embedding), @MetadataId)
";

    private const string InsertMetadataQuery = @"
INSERT INTO knowledge_metadata(app_version_range, engine_version, applies_to, platforms, source)
VALUES(@AppVersionRange, @EngineVersion, @AppliesTo, @Platforms, @Source)
RETURNING *
";

    private const string VectorInit =
        "SELECT vector_init('knowledge_chunks', 'embedding', 'type=FLOAT32,dimension=768');";

    private const string VectorQuantize = "SELECT vector_quantize('knowledge_chunks', 'embedding');";

    private Task WriteChunk(IDbTransaction transaction, Chunk chunk)
    {
        return transaction.Connection!.ExecuteAsync(InsertEmbeddingQuery,
            new
            {
                DocumentTitle = chunk.DocumentTitle, SectionTitle = chunk.SectionTitle, ChunkText = chunk.ChunkText,
                Embedding = JsonConvert.SerializeObject(chunk.Embedding), chunk.MetadataId
            });
    }

    private static (List<GameEngine> AppliesTo, List<Platform> Platforms, string? Source, string? AppVersionRange, Dictionary<string, string> EngineVersions)
        MergeMetadata(DocumentMetadata document, SectionMetadata? section)
    {
        var appliesTo = section?.AppliesTo.Count > 0 ? section.AppliesTo : document.AppliesTo;
        var platforms = section?.Platforms.Count > 0 ? section.Platforms : document.Platforms;
        return (appliesTo, platforms, document.Source, section?.AppVersionRange, section?.EngineVersions ?? []);
    }

    private Task<IEnumerable<KnowledgeMetadata>> WriteMetadata(IDbTransaction transaction, AnnotatedChunk annotatedChunk)
    {
        var (appliesTo, platforms, source, appVersionRange, engineVersions) =
            MergeMetadata(annotatedChunk.DocumentMetadata, annotatedChunk.SectionMetadata);
        return transaction.Connection!.QueryAsync<KnowledgeMetadata>(InsertMetadataQuery,
            new
            {
                AppVersionRange = JsonConvert.SerializeObject(appVersionRange),
                EngineVersion = JsonConvert.SerializeObject(engineVersions),
                AppliesTo = JsonConvert.SerializeObject(appliesTo),
                Platforms = JsonConvert.SerializeObject(platforms),
                Source = source
            });
    }

    public async Task WriteChunks(IEnumerable<AnnotatedChunk> annotatedChunks, CancellationToken cancellationToken)
    {
        using var connection = GetConnection($"Data Source={_embedderConfiguration.KnowledgeBasePath}");
        connection.Open();
        await EnsureTables(connection);
        var transaction = CreateTransaction(connection, cancellationToken);
        foreach (var annotatedChunk in annotatedChunks)
        {
            var insertionResult = await WriteMetadata(transaction, annotatedChunk);

            var insertedMetadata = insertionResult.FirstOrDefault();

            foreach (var chunk in annotatedChunk.Chunks)
            {
                chunk.MetadataId = insertedMetadata?.Id;
                await WriteChunk(transaction, chunk);
            }
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
        await connection.ExecuteAsync("DELETE FROM knowledge_metadata");
        await connection.ExecuteAsync("DELETE FROM knowledge_chunks");
        await connection.ExecuteAsync("DROP TABLE knowledge_metadata");
        await connection.ExecuteAsync("DROP TABLE knowledge_chunks");
        await connection.ExecuteAsync(CreateMetadataTable);
        await connection.ExecuteAsync(CreateEmbeddingsTable);
    }

    private IDbConnection GetConnection(string connectionString)
    {
        var connection = new SqliteConnection(connectionString);
        connection.EnableExtensions();
        connection.LoadExtension("./vector");
        return connection;
    }
}