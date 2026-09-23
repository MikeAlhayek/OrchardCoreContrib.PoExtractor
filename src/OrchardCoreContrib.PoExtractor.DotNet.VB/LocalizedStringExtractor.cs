using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using System;
using System.Linq;

namespace OrchardCoreContrib.PoExtractor.DotNet.VB;

/// <summary>
/// Extracts <see cref="LocalizableStringOccurence"/> with the singular text from the VB AST node
/// </summary>
/// <remarks>
/// The localizable string is identified by the constructor call - New LocalizedString("TEXT TO TRANSLATE", "TEXT TO TRANSLATE")
/// or New LocalizedHtmlString("TEXT TO TRANSLATE", "TEXT TO TRANSLATE")
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
            creation.ArgumentList.Arguments[0].GetExpression() is LiteralExpressionSyntax literal &&
            literal.IsKind(SyntaxKind.StringLiteralExpression))
        {
            result = CreateLocalizedString(literal.Token.ValueText, null, node);

            return true;
        }

        return false;
    }

    private static bool IsLocalizedStringType(TypeSyntax type) => type switch
    {
        IdentifierNameSyntax identifierName => IsLocalizedStringTypeName(identifierName.Identifier.ValueText),
        QualifiedNameSyntax qualifiedName => IsLocalizedStringTypeName(qualifiedName.Right.Identifier.ValueText),
        _ => false
    };

    // Visual Basic identifiers are case-insensitive.
    private static bool IsLocalizedStringTypeName(string name)
        => _localizedStringTypeNames.Contains(name, StringComparer.OrdinalIgnoreCase);
}
