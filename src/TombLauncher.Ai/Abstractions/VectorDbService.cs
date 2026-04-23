using System.Data;
using Dapper;
using Microsoft.Data.Sqlite;
using TombLauncher.Ai.Configuration;

namespace TombLauncher.Ai.Abstractions;

public abstract class VectorDbService
{
    protected IAiConfig EmbedderConfiguration { get; }
    
    private const string VectorInit =
        "SELECT vector_init('knowledge_chunks', 'embedding', 'type=FLOAT32,dimension=768,distance=COSINE');";
    private const string VectorQuantize = "SELECT vector_quantize('knowledge_chunks', 'embedding');";
    private const string VectorQuantizePreload = "SELECT vector_quantize_preload('knowledge_chunks', 'embedding');";

    protected VectorDbService(IAiConfig embedderConfiguration)
    {
        EmbedderConfiguration = embedderConfiguration;
    }
    
    protected IDbConnection GetConnection(string connectionString)
    {
        var connection = new SqliteConnection(connectionString);
        connection.EnableExtensions();
        connection.LoadExtension("./vector");
        return connection;
    }

    protected async Task ExecuteVectorInit(IDbConnection connection)
    {
        await connection.ExecuteAsync(VectorInit);
    }

    protected Task ExecuteVectorQuantize(IDbConnection connection)
    {
        return connection.ExecuteAsync(VectorQuantize);
    }

    protected Task ExecuteVectorQuantizePreload(IDbConnection connection)
    {
        return connection.ExecuteAsync(VectorQuantizePreload);
    }
}