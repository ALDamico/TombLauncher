using Markdig;
using Markdig.Syntax;
using Microsoft.Extensions.AI;
using TombLauncher.Ai.Models;

namespace TombLauncher.Ai.Utils;

public static class DocumentChunker
{
    public static async Task<List<AnnotatedChunk>> GetAnnotatedChunks(string fileContent, IEmbeddingGenerator<string, Embedding<float>> embedder, DocumentMetadata metadata, CancellationToken cancellationToken)
    {
        var documentTitle = string.Empty;
        var output = new List<AnnotatedChunk>();
        var document = Markdown.Parse(fileContent);

        HeadingBlock? currentHeading = null;
        var sectionLines = new List<string>();

        var groupedMetadata = metadata.Sections.ToLookup(section => section.Header);
        var identicalHeadingCount = 0;

        foreach (var block in document)
        {
            if (block is HeadingBlock heading)
            {
                if (heading.Level == 1)
                {
                    documentTitle = GetHeadingText(heading);
                    continue;
                }

                if (heading.Level == 2)
                {
                    if (currentHeading != null)
                    {
                        var annotatedChunk = await ProcessSection(embedder, cancellationToken, currentHeading, sectionLines, groupedMetadata,
                            identicalHeadingCount, documentTitle, metadata);
                        output.Add(annotatedChunk);

                        identicalHeadingCount = GetHeadingText(currentHeading) == GetHeadingText(heading)
                            ? identicalHeadingCount + 1
                            : 0;

                        sectionLines.Clear();
                    }
                    currentHeading = heading;
                }
                else if (currentHeading != null)
                {
                    sectionLines.Add(ExtractRawText(block, fileContent));
                }
            }
            else if (currentHeading != null)
            {
                sectionLines.Add(ExtractRawText(block, fileContent));
            }
        }

        if (currentHeading != null)
        {
            var annotatedChunk = await ProcessSection(embedder, cancellationToken, currentHeading, sectionLines, groupedMetadata, identicalHeadingCount, documentTitle, metadata);
            output.Add(annotatedChunk);
        }

        return output;
    }

    private static async Task<AnnotatedChunk> ProcessSection(IEmbeddingGenerator<string, Embedding<float>> embedder, CancellationToken cancellationToken,
        HeadingBlock currentHeading, List<string> sectionLines, ILookup<string, SectionMetadata> groupedMetadata, int identicalHeadingCount,
        string documentTitle, DocumentMetadata metadata)
    {
        var (header, sectionText) = (GetHeadingText(currentHeading), string.Join("\n\n", sectionLines));
        var split = ChunkUtils.SplitChunks(sectionText);
        var annotatedChunk = new AnnotatedChunk()
        {
            SectionHeader = header,
            DocumentMetadata = metadata,
            SectionMetadata = groupedMetadata[header].ElementAtOrDefault(identicalHeadingCount)
        };
        foreach (var item in split)
        {
            var embeddings = await embedder.GenerateAsync([$"{header}\n\n{item}"], cancellationToken: cancellationToken);
            var chunk = new Chunk()
            {
                DocumentTitle = documentTitle,
                ChunkText = item,
                SectionTitle = header,
                Embedding = embeddings[0].Vector.ToArray()
            };
            annotatedChunk.Chunks.Add(chunk);
        }

        return annotatedChunk;
    }

    private static string GetHeadingText(HeadingBlock heading)
    {
        var inline = heading.Inline?.FirstChild;
        var parts = new List<string>();
        while (inline != null)
        {
            if (inline is Markdig.Syntax.Inlines.LiteralInline lit)
                parts.Add(lit.Content.ToString());
            inline = inline.NextSibling;
        }
        return string.Join("", parts);
    }

    private static string ExtractRawText(Block block, string source)
    {
        return source.Substring(block.Span.Start, block.Span.Length);
    }
}
