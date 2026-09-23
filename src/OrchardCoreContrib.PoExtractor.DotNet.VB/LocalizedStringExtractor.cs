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
/// or New LocalizedHtmlString("TEXT TO TRANSLATE", "TEXT TO TRANSLATE"). The single argument form
/// New LocalizedString("TEXT TO TRANSLATE") or New LocalizedHtmlString("TEXT TO TRANSLATE") is also supported.
/// The factory method call - LocalizedString.Create("TEXT TO TRANSLATE") or LocalizedHtmlString.Create("TEXT TO TRANSLATE")
/// is also supported, including the LocalizedStringExtensions.Create("TEXT TO TRANSLATE") and
/// LocalizedHtmlStringExtensions.Create("TEXT TO TRANSLATE") forms.
/// </remarks>
/// <remarks>
/// Creates a new instance of a <see cref="LocalizedStringExtractor"/>.
/// </remarks>
/// <param name="metadataProvider">The <see cref="IMetadataProvider{TNode}"/>.</param>
public class LocalizedStringExtractor(IMetadataProvider<SyntaxNode> metadataProvider) : LocalizableStringExtractor<SyntaxNode>(metadataProvider)
{
    /// <inheritdoc/>
    public override bool TryExtract(SyntaxNode node, out LocalizableStringOccurence result)
    {
        ArgumentNullException.ThrowIfNull(node);

        result = null;

        var argumentList = node switch
        {
            ObjectCreationExpressionSyntax creation when IsTypeName(creation.Type, LocalizedStringTypes.TypeNames) => creation.ArgumentList,
            InvocationExpressionSyntax invocation when IsFactoryMethod(invocation.Expression) => invocation.ArgumentList,
            _ => null
        };

        if (argumentList != null &&
            argumentList.Arguments.Count > 0 &&
            argumentList.Arguments[0].GetExpression() is LiteralExpressionSyntax literal &&
            literal.IsKind(SyntaxKind.StringLiteralExpression))
        {
            result = CreateLocalizedString(literal.Token.ValueText, null, node);

            return true;
        }

        return false;
    }

    private static bool IsFactoryMethod(ExpressionSyntax expression)
        => expression is MemberAccessExpressionSyntax memberAccess &&
            IsName(memberAccess.Name.Identifier.ValueText, LocalizedStringTypes.FactoryMethodName) &&
            IsTypeName(memberAccess.Expression, LocalizedStringTypes.FactoryTypeNames);

    private static bool IsTypeName(ExpressionSyntax expression, string[] typeNames)
    {
        var name = expression switch
        {
            IdentifierNameSyntax identifierName => identifierName.Identifier.ValueText,
            QualifiedNameSyntax qualifiedName => qualifiedName.Right.Identifier.ValueText,
            MemberAccessExpressionSyntax memberAccess => memberAccess.Name.Identifier.ValueText,
            _ => null
        };

        return name != null && typeNames.Any(typeName => IsName(name, typeName));
    }

    // Visual Basic identifiers are case-insensitive.
    private static bool IsName(string name, string expected)
        => string.Equals(name, expected, StringComparison.OrdinalIgnoreCase);
}
