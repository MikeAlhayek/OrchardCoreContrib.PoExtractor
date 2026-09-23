using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using OrchardCoreContrib.PoExtractor.DotNet.VB.MetadataProviders;

namespace OrchardCoreContrib.PoExtractor.DotNet.VB.Tests;

public class LocalizedStringExtractorTests
{
    [Theory]
    [InlineData("""New LocalizedString("Thing", "Thing")""", "Thing")]
    [InlineData("""New LocalizedString("Thing", "Other thing")""", "Thing")]
    [InlineData("""New localizedstring("Thing", "Thing")""", "Thing")]
    [InlineData("""New Microsoft.Extensions.Localization.LocalizedString("Thing", "Thing")""", "Thing")]
    public void ExtractString(string source, string expected)
    {
        // Arrange
        var metadataProvider = new VisualBasicMetadataProvider("DummyBasePath");
        var extractor = new LocalizedStringExtractor(metadataProvider);

        var node = GetObjectCreationNode(source);

        // Act
        var extracted = extractor.TryExtract(node, out var result);

        // Assert
        Assert.True(extracted);
        Assert.Equal(expected, result.Text);
        Assert.Null(result.TextPlural);
    }

    [Theory]
    [InlineData("""New LocalizedString(NameOf(Thing), "Thing")""")]
    [InlineData("""New LocalizedString(name, "Thing")""")]
    [InlineData("""New LocalizedString("Thing")""")]
    [InlineData("""New Thing("Thing", "Thing")""")]
    public void ExtractString_NotLocalizedStringWithLiteralName_ReturnsFalse(string source)
    {
        // Arrange
        var metadataProvider = new VisualBasicMetadataProvider("DummyBasePath");
        var extractor = new LocalizedStringExtractor(metadataProvider);

        var node = GetObjectCreationNode(source);

        // Act
        var extracted = extractor.TryExtract(node, out var result);

        // Assert
        Assert.False(extracted);
        Assert.Null(result);
    }

    private static ObjectCreationExpressionSyntax GetObjectCreationNode(string source)
        => VisualBasicSyntaxTree.ParseText($"Dim value = {source}", path: "DummyPath")
            .GetRoot()
            .DescendantNodes()
            .OfType<ObjectCreationExpressionSyntax>()
            .First();
}
