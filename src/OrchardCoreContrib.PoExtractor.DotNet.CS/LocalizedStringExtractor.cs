using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Linq;

namespace OrchardCoreContrib.PoExtractor.DotNet.CS;

/// <summary>
/// Extracts <see cref="LocalizableStringOccurence"/> with the singular text from the C# AST node
/// </summary>
/// <remarks>
/// The localizable string is identified by the constructor call - new LocalizedString("TEXT TO TRANSLATE", "TEXT TO TRANSLATE")
/// or new LocalizedHtmlString("TEXT TO TRANSLATE", "TEXT TO TRANSLATE")
/// </remarks>
/// <remarks>
/// Creates a new instance of a <see cref="LocalizedStringExtractor"/>.
/// </remarks>
/// <param name="metadataProvider">The <see cref="IMetadataProvider{TNode}"/>.</param>
public class LocalizedStringExtractor(IMetadataProvider<SyntaxNode> metadataProvider) : LocalizableStringExtractor<SyntaxNode>(metadataProvider)
{
    private static readonly string[] _localizedStringTypeNames =
    [
        "LocalizedString",
        "LocalizedHtmlString"
    ];

    /// <inheritdoc/>
    public override bool TryExtract(SyntaxNode node, out LocalizableStringOccurence result)
    {
        ArgumentNullException.ThrowIfNull(node);

        result = null;

        if (node is ObjectCreationExpressionSyntax creation &&
            IsLocalizedStringType(creation.Type) &&
            creation.ArgumentList != null &&
            creation.ArgumentList.Arguments.Count >= 2 &&
            SingularStringExtractor.TryGetString(creation.ArgumentList.Arguments[0].Expression, out var value))
        {
            result = CreateLocalizedString(value, null, node);

            return true;
        }

        return false;
    }

    private static bool IsLocalizedStringType(TypeSyntax type) => type switch
    {
        IdentifierNameSyntax identifierName => _localizedStringTypeNames.Contains(identifierName.Identifier.Text),
        QualifiedNameSyntax qualifiedName => _localizedStringTypeNames.Contains(qualifiedName.Right.Identifier.Text),
        AliasQualifiedNameSyntax aliasQualifiedName => _localizedStringTypeNames.Contains(aliasQualifiedName.Name.Identifier.Text),
        _ => false
    };
}
