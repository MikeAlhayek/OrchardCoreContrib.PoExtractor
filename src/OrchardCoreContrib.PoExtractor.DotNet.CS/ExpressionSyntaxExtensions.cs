using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace OrchardCoreContrib.PoExtractor.DotNet.CS;

internal static class ExpressionSyntaxExtensions
{
    /// <summary>
    /// Gets the value of a string literal, or of string literals joined with the + operator.
    /// </summary>
    /// <param name="expression">The <see cref="ExpressionSyntax"/>.</param>
    /// <param name="value">The string value.</param>
    public static bool TryGetString(this ExpressionSyntax expression, out string value)
    {
        if (expression is LiteralExpressionSyntax literal && literal.IsKind(SyntaxKind.StringLiteralExpression))
        {
            value = literal.Token.ValueText;

            return true;
        }

        if (expression is BinaryExpressionSyntax binary &&
            binary.IsKind(SyntaxKind.AddExpression) &&
            binary.Left.TryGetString(out var left) &&
            binary.Right.TryGetString(out var right))
        {
            value = left + right;

            return true;
        }

        value = null;

        return false;
    }
}
