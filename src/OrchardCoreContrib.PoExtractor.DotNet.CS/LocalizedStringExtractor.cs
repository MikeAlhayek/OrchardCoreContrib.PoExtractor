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
/// or new LocalizedHtmlString("TEXT TO TRANSLATE", "TEXT TO TRANSLATE"). The single argument form
/// new LocalizedString("TEXT TO TRANSLATE") or new LocalizedHtmlString("TEXT TO TRANSLATE") is also supported.
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
            argumentList.Arguments[0].Expression.TryGetString(out var value))
        {
            result = CreateLocalizedString(value, null, node);

            return true;
        }

        return false;
    }

    private static bool IsFactoryMethod(ExpressionSyntax expression)
        => expression is MemberAccessExpressionSyntax memberAccess &&
            memberAccess.Name.Identifier.Text == LocalizedStringTypes.FactoryMethodName &&
            IsTypeName(memberAccess.Expression, LocalizedStringTypes.FactoryTypeNames);

    private static bool IsTypeName(ExpressionSyntax expression, string[] typeNames)
    {
        var name = expression switch
        {
            IdentifierNameSyntax identifierName => identifierName.Identifier.Text,
            QualifiedNameSyntax qualifiedName => qualifiedName.Right.Identifier.Text,
            AliasQualifiedNameSyntax aliasQualifiedName => aliasQualifiedName.Name.Identifier.Text,
            MemberAccessExpressionSyntax memberAccess => memberAccess.Name.Identifier.Text,
            _ => null
        };

        return name != null && typeNames.Contains(name);
    }
}
