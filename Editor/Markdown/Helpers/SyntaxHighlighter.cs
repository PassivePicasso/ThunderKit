using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using static UnityEngine.GraphicsBuffer;
#else
using UnityEngine.Experimental.UIElements;
#endif

public static class SyntaxHighlighter
{
    const string space = " ";

    public static VisualElement Highlight(string code)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(code);
        var root = syntaxTree.GetRoot();
        var lineGroups = new Dictionary<int, VisualElement>();
        HighlightNode(root, lineGroups);

        var document = new VisualElement();
        foreach (var lineGroup in lineGroups.OrderBy(kvp => kvp.Key))
        {
            var lineElement = lineGroup.Value;
            if (lineElement == null) continue;
            var children = lineElement.Children().ToList();
            var lastElement = children.First();
            foreach (var element in children)
            {
                if (element == lastElement) continue;
                if (RequiresSpace(lastElement, element)) lineElement.Insert(lineElement.IndexOf(element), Space());
                lastElement = element;
            }
            document.Add(lineGroup.Value);
        }
        return document;
    }
    private static void HighlightNode(SyntaxNode node, Dictionary<int, VisualElement> lineGroups, int scopeDepth = 0)
    {
        if (node is null) return;
        foreach (var child in node.ChildNodesAndTokens())
        {
            if (child.IsNode)
            {
                HighlightNode(child.AsNode(), lineGroups, scopeDepth);
            }
            else
            {
                var token = child.AsToken();
                var lineNumber = token.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
                if (token.IsKind(SyntaxKind.CloseBraceToken))
                    scopeDepth--;
                if (!lineGroups.ContainsKey(lineNumber))
                {
                    lineGroups[lineNumber] = new VisualElement();
                    if (scopeDepth > 0)
                        lineGroups[lineNumber].Add(new Label(new string(' ', 2 * scopeDepth)));
                }

                var lineElement = lineGroups[lineNumber];
                lineElement.AddToClassList("line");

                if (PrependSpace(token))
                    lineElement.Add(Space());

                var tokenLabel = AddToken(token);
                if (node.IsKind(SyntaxKind.ClassDeclaration) && token.IsKind(SyntaxKind.IdentifierToken))
                    tokenLabel.AddToClassList("class");

                if (node.IsKind(SyntaxKind.SimpleBaseType))
                {
                    if (token.IsKind(SyntaxKind.IdentifierToken))
                        tokenLabel.AddToClassList("simple-base-type");
                }

                lineElement.Add(tokenLabel);

                if (AppendSpace(token))
                    lineElement.Add(Space());

                if (token.IsKind(SyntaxKind.OpenBraceToken))
                    scopeDepth++;
            }
        }
    }
    private static bool PrependSpace(SyntaxToken token)
    {
        return token.Kind() == SyntaxKind.EqualsGreaterThanToken || token.Kind() == SyntaxKind.ColonToken;
    }
    private static bool AppendSpace(SyntaxToken token)
    {
        return token.Kind() == SyntaxKind.EqualsGreaterThanToken || token.Kind() == SyntaxKind.ColonToken;
    }
    private static bool RequiresSpace(VisualElement previous, VisualElement current)
    {
        return (
        previous.ClassListContains("identifier")
        || previous.ClassListContains("keyword")
        || previous.ClassListContains("numeric-literal")
        || previous.ClassListContains("string-literal")
        || previous.ClassListContains("character-literal")
        || previous.ClassListContains("predefined-type")
        ) && (
        current.ClassListContains("identifier")
        || current.ClassListContains("keyword")
        || current.ClassListContains("numeric-literal")
        || current.ClassListContains("string-literal")
        || current.ClassListContains("character-literal")
        || current.ClassListContains("predefined-type")
        );
    }
    private static Label AddToken(SyntaxToken token)
    {
        var classification = GetClassification(token);
        var label = new Label(token.ToString());
        label.AddToClassList(classification);
        return label;
    }
    private static Label Space()
    {
        var label = new Label(space);
        label.AddToClassList("space");
        return label;
    }
    private static string GetClassification(SyntaxToken token)
    {
        if (token.IsKeyword()) return "keyword";
        if (SyntaxFacts.IsTypeDeclaration(token.Kind())) return "type-declaration";
        if (SyntaxFacts.IsPunctuation(token.Kind())) return "punctuation";
        if (SyntaxFacts.IsPredefinedType(token.Kind())) return "predefined-type";
        if (token.IsKind(SyntaxKind.NumericLiteralToken)) return "numeric-literal";
        if (token.IsKind(SyntaxKind.StringLiteralToken)) return "string-literal";
        if (token.IsKind(SyntaxKind.CharacterLiteralToken)) return "character-literal";
        if (token.IsKind(SyntaxKind.IdentifierToken)) return "identifier";
        return "text";
    }
}