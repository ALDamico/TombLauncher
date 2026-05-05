using Dapper;
using Microsoft.Extensions.AI;
using Newtonsoft.Json;
using NuGet.Versioning;
using TombLauncher.Ai.Abstractions;
using TombLauncher.Ai.Configuration;
using TombLauncher.Core.PlatformSpecific;
using TombLauncher.Ai.Models;
using TombLauncher.Contracts.Enums;
using TombLauncher.Contracts.Progress;
using TombLauncher.Core.Dtos;

namespace TombLauncher.Ai.Services;

public class VectorSearchService : VectorDbService
{
    private readonly IEmbeddingGenerator<string, Embedding<float>> _embedder;
    private readonly EmbeddingGenerationOptions _embeddingGenerationOptions;

    private const string ChunksQuery = @"SELECT document_title AS ""DocumentTitle"", section_title AS ""SectionTitle"", chunk_text AS ""ChunkText"", metadata_id AS ""MetadataId""
FROM knowledge_chunks c
JOIN vector_quantize_scan('knowledge_chunks', 'embedding', vector_as_f32(@Vector), @TopK) AS v ON c.id = v.rowid
";

    private const string MetadataQuery = @"SELECT id AS ""Id"", app_version_range AS ""AppVersionRange"", engine_version AS ""EngineVersion"", applies_to AS ""AppliesTo"", platforms AS ""Platforms"", source AS ""Source""
FROM knowledge_metadata
WHERE id IN @Ids;";
    private readonly string _knowledgeBasePath;

    public VectorSearchService(IEmbeddingGenerator<string, Embedding<float>> embedder, IAiConfig configuration,
        IPlatformSpecificFeatures platformSpecificFeatures) : base(configuration)
    {
        _embedder = embedder;
        _knowledgeBasePath = Path.Combine(platformSpecificFeatures.GetAppDataDirectory(),
            configuration.KnowledgeBasePath ?? "");
        _embeddingGenerationOptions = new EmbeddingGenerationOptions()
        {
            Dimensions = 768,
            ModelId = configuration.EmbeddingModelId
        };
    }

    public async Task<List<KnowledgeBaseItemDto>> SearchAsync(IProgress<DownloadProgressInfo> progress, string query, int topK = 5, CancellationToken cancellationToken = default)
    {
        var embedding = await _embedder.GenerateAsync(query, cancellationToken: cancellationToken);

        using var connection = GetConnection($"Data Source={_knowledgeBasePath}");
        connection.Open();
        await ExecuteVectorInit(connection);
        await ExecuteVectorQuantizePreload(connection);
        
        var vector = embedding.Vector.ToArray();

        var result = await connection.QueryAsync<Chunk>(ChunksQuery,
            new { Vector = JsonConvert.SerializeObject(vector), TopK = topK });

        var resultList = result.ToList();

        var metadataIds = resultList.Select(c => c.MetadataId).Where(id => id != null).Select(id => id.GetValueOrDefault()).ToHashSet();

        IEnumerable<KnowledgeMetadata> metadataResult = [];
        
        if (metadataIds.Any())
            metadataResult = await connection.QueryAsync<KnowledgeMetadata>(MetadataQuery, new { Ids = metadataIds });
        var metadataLookup = metadataResult.ToLookup(m => m.Id);

        return resultList
            .Select(c => ConvertChunk(c, metadataLookup))
            .ToList();
    }

    private KnowledgeBaseItemDto ConvertChunk(Chunk c, ILookup<int, KnowledgeMetadata> metadataLookup)
    {
        var metadata = metadataLookup[c.MetadataId.GetValueOrDefault()].FirstOrDefault();

        var dto = new KnowledgeBaseItemDto()
        {
            Source = metadata?.Source,
            DocumentTitle = c.DocumentTitle!,
            SectionTitle = c.SectionTitle!,
            Text = c.ChunkText
        };

        if (metadata != null)
        {
            VersionRange.TryParse(metadata.AppVersionRange!, out var appVersionRange);
            dto.AppVersionRange = appVersionRange;
            var engineVersions =
                JsonConvert.DeserializeObject<Dictionary<GameEngine, string>>(metadata.EngineVersion!);

            dto.EngineVersions = new Dictionary<GameEngine, VersionRange>();
            if (engineVersions != null)
            {
                foreach (var engineKvp in engineVersions)
                {
                    var engine = engineKvp.Key;
                    var rangeStr = engineKvp.Value;
                    if (VersionRange.TryParse(rangeStr, out var engineVersionRange))
                    {
                        dto.EngineVersions[engine] = engineVersionRange;
                    }
                }
            }

            dto.Platforms = JsonConvert.DeserializeObject<List<Platform>>(metadata.Platforms ?? "[]") ?? [];
            dto.AppliesTo = JsonConvert.DeserializeObject<List<GameEngine>>(metadata.AppliesTo ?? "[]") ?? [];
        }
        return dto;
    }
}