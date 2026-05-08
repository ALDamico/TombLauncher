using System.Data;
using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using TombLauncher.Ai.Abstractions;
using TombLauncher.Ai.Configuration;
using TombLauncher.Ai.Models;
using TombLauncher.Contracts.Enums;

namespace TombLauncher.Ai.Services;

public class KnowledgeBaseWriter : VectorDbService
{
    private readonly ILogger<KnowledgeBaseWriter> _logger;

    public KnowledgeBaseWriter(IOptions<AiConfig> embedderConfiguration, ILogger<KnowledgeBaseWriter> logger) : base(embedderConfiguration.Value)
    {
        _logger = logger;
        _logger.LogInformation("Will write to database {DbPath}", EmbedderConfiguration.KnowledgeBasePath);
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
                AppVersionRange = appVersionRange,
                EngineVersion = JsonConvert.SerializeObject(engineVersions),
                AppliesTo = JsonConvert.SerializeObject(appliesTo),
                Platforms = JsonConvert.SerializeObject(platforms),
                Source = source
            });
    }

    public async Task WriteChunks(IEnumerable<AnnotatedChunk> annotatedChunks, CancellationToken cancellationToken)
    {
        using var connection = GetConnection($"Data Source={EmbedderConfiguration.KnowledgeBasePath}");
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
        await ExecuteVectorInit(connection);
        await ExecuteVectorQuantize(connection);
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
        try
        {
            await connection.ExecuteAsync("DELETE FROM knowledge_metadata");
        }
        catch (SqliteException ex)
        {
            _logger.LogInformation("knowledge_metadata did not exist. Oh well...: {Ex}", ex);
        }

        try
        {
            await connection.ExecuteAsync("DELETE FROM knowledge_chunks");
        }
        catch (SqliteException ex)
        {
            _logger.LogInformation("knowledge_chunks did not exist. Oh well...: {Ex}", ex);
        }
        await connection.ExecuteAsync("DROP TABLE IF EXISTS knowledge_metadata");
        await connection.ExecuteAsync("DROP TABLE IF EXISTS knowledge_chunks");
        await connection.ExecuteAsync(CreateMetadataTable);
        await connection.ExecuteAsync(CreateEmbeddingsTable);
    }
}