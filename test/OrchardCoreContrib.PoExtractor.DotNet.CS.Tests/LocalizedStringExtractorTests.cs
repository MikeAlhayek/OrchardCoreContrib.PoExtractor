using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using OrchardCoreContrib.PoExtractor.DotNet.CS.MetadataProviders;

namespace OrchardCoreContrib.PoExtractor.DotNet.CS.Tests;

public class LocalizedStringExtractorTests
{
    [Theory]
    [InlineData("""new LocalizedString("Thing", "Thing");""", "Thing")]
    [InlineData("""new LocalizedString("Thing", "Other thing");""", "Thing")]
    [InlineData("""new LocalizedString("Thing", "Thing", true);""", "Thing")]
    [InlineData("""new Microsoft.Extensions.Localization.LocalizedString("Thing", "Thing");""", "Thing")]
    [InlineData("""new global::Microsoft.Extensions.Localization.LocalizedString("Thing", "Thing");""", "Thing")]
    [InlineData("""new LocalizedString("my " + "text", "my text");""", "my text")]
    [InlineData("""new LocalizedHtmlString("Thing", "Thing");""", "Thing")]
    [InlineData("""new LocalizedHtmlString("Thing {0}", "Thing {0}", false, 1);""", "Thing {0}")]
    [InlineData("""new Microsoft.AspNetCore.Mvc.Localization.LocalizedHtmlString("Thing", "Thing");""", "Thing")]
    [InlineData(
        """
        new LocalizedString(@"This is a multi-line
        string.", "value");
        """,
        """
        This is a multi-line
        string.
        """)]
    public void ExtractString(string source, string expected)
    {
        // Arrange
        var metadataProvider = new CSharpMetadataProvider("DummyBasePath");
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
    [InlineData("""new LocalizedString(nameof(Thing), "Thing");""")]
    [InlineData("""new LocalizedString(name, "Thing");""")]
    [InlineData("""new LocalizedString("Thing");""")]
    [InlineData("""new LocalizedHtmlString(nameof(Thing), "Thing");""")]
    [InlineData("""new Thing("Thing", "Thing");""")]
    public void ExtractString_NotLocalizedStringWithLiteralName_ReturnsFalse(string source)
    {
        // Arrange
        var metadataProvider = new CSharpMetadataProvider("DummyBasePath");
        var extractor = new LocalizedStringExtractor(metadataProvider);

        var node = GetObjectCreationNode(source);

        // Act
        var extracted = extractor.TryExtract(node, out var result);

        // Assert
        Assert.False(extracted);
        Assert.Null(result);
    }

    private static ObjectCreationExpressionSyntax GetObjectCreationNode(string source)
        => CSharpSyntaxTree.ParseText(source, path: "DummyPath")
            .GetRoot()
            .DescendantNodes()
            .OfType<ObjectCreationExpressionSyntax>()
            .First();
}
