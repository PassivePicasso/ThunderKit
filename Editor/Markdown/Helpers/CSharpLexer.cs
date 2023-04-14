using System.Collections.Generic;
using System.Linq;
#if UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using static UnityEngine.GraphicsBuffer;
#else
using UnityEngine.Experimental.UIElements;
#endif


public static class CSharpLexer
{
    private static Dictionary<TokenType, string> tokenTypeToClassMap = new Dictionary<TokenType, string>()
    {
        { TokenType.Keyword, "keyword" },
        { TokenType.Identifier, "identifier" },
        { TokenType.Number, "number" },
        { TokenType.Operator, "operator" },
        { TokenType.Punctuation, "punctuation" },
        { TokenType.WhiteSpace, "white-space" },
        { TokenType.String, "string" },
        { TokenType.Type, "type" },
        { TokenType.Method, "method" },
        { TokenType.Property, "property" },
    };

    private enum TokenType
    {
        Keyword,
        Identifier,
        Number,
        Operator,
        Punctuation,
        WhiteSpace,
        String,
        Namespace,
        Type,
        Method,
        Property,
    }

    private struct Token
    {
        public string value;
        public TokenType type;
        public VisualElement associatedElement;

        public Token(string value, TokenType type, VisualElement associatedElement = null)
        {
            this.value = value;
            this.type = type;
            this.associatedElement = associatedElement;
        }
    }

    public static VisualElement GetHierarchy(string input)
    {
        var container = new VisualElement();
        string[] lines = input.Split('\n');
        var tokens = new List<Token>();
        for (int i = 0; i < lines.Length; i++)
        {
            container.Add(TokenizeLine(lines[i], i + 1, tokens));
        }

        return container;
    }

    private static VisualElement TokenizeLine(string line, int lineNumber, List<Token> tokens)
    {
        VisualElement lineElements = new VisualElement();
        lineElements.AddToClassList("line-elements");

        List<Token> lineTokens = Tokenize(line, tokens);

        VisualElement lineNumberElement = new VisualElement();
        lineNumberElement.Add(new Label(lineNumber.ToString()));
        lineNumberElement.AddToClassList("line-number");
        lineElements.Add(lineNumberElement);

        VisualElement lineContainerElement = new VisualElement();
        lineContainerElement.AddToClassList("line-container");


        for (int i = 0; i < lineTokens.Count; i++)
        {
            Token token = lineTokens[i];
            var tokenElement = new Label(token.value);
            tokenElement.AddToClassList(tokenTypeToClassMap[token.type]);
            token.associatedElement = tokenElement;
            lineContainerElement.Add(tokenElement);
            lineTokens[i] = token;
        }
        tokens.AddRange(lineTokens);

        lineElements.Add(lineContainerElement);

        return lineElements;
    }

    private static List<Token> Tokenize(string input, List<Token> previousTokens)
    {
        var tokens = new List<Token>();
        int i = 0;
        while (i < input.Length)
        {
            char c = input[i];

            if (char.IsWhiteSpace(c))
            {
                tokens.Add(new Token($"{c}", TokenType.WhiteSpace));
                i++;
            }
            else if (char.IsLetter(c) || c == '_')
            {
                string identifier = "";
                while (i < input.Length && (char.IsLetterOrDigit(input[i]) || input[i] == '_'))
                {
                    identifier += input[i];
                    i++;
                }

                TokenType tokenType = TokenType.Identifier;
                if (IsKeyword(identifier))
                {
                    tokenType = TokenType.Keyword;
                    switch (identifier)
                    {
                        case "get":
                        case "set":
                            for (int pi = previousTokens.Count - 1; pi >= 0; pi--)
                            {
                                var prevToken = previousTokens[pi];
                                if (prevToken.type == TokenType.Identifier)
                                {
                                    var newToken = new Token(prevToken.value, TokenType.Property, prevToken.associatedElement);
                                    if (newToken.associatedElement != null)
                                    {
                                        newToken.associatedElement.RemoveFromClassList(tokenTypeToClassMap[prevToken.type]);
                                        newToken.associatedElement.AddToClassList(tokenTypeToClassMap[newToken.type]);
                                    }
                                    previousTokens[pi] = newToken;
                                    break;
                                }
                            }
                            break;
                    }
                }
                else if (char.IsUpper(identifier[0]))
                {
                    if (i < input.Length && input[i] == '(')
                    {
                        tokenType = TokenType.Method; // method name
                    }
                    else if (i < input.Length && input[i] == '<')
                    {
                        tokenType = TokenType.Method; // namespace or type name
                    }
                    else if (i < input.Length - 2 && input[i + 1] == '=' && input[i + 2] == '>')
                    {
                        tokenType = TokenType.Property; // namespace or type name
                    }
                    else if (i < input.Length && input[i] == '.')
                    {
                        tokenType = TokenType.Type; // namespace or type name
                    }
                    else
                    {
                        tokenType = TokenType.Identifier; // type name or property name
                    }
                }
                tokens.Add(new Token(identifier, tokenType));
            }
            else if (char.IsDigit(c))
            {
                string number = "";
                while (i < input.Length && char.IsDigit(input[i]))
                {
                    number += input[i];
                    i++;
                }

                tokens.Add(new Token(number, TokenType.Number));
            }
            else if (IsOperator(c))
            {
                string @operator = "";
                while (i < input.Length && IsOperator(input[i]))
                {
                    @operator += input[i];
                    i++;
                }

                tokens.Add(new Token(@operator, TokenType.Operator));
            }
            else if (IsPunctuation(c))
            {
                tokens.Add(new Token(c.ToString(), TokenType.Punctuation));
                i++;
            }
            else if (c == '\"')
            {
                // Handle string literals
                string str = "\"";
                i++;
                while (i < input.Length && input[i] != '\"')
                {
                    if (input[i] == '\\')
                    {
                        // Handle escape sequences
                        str += input[i];
                        i++;
                        if (i < input.Length)
                        {
                            str += input[i];
                            i++;
                        }
                    }
                    else
                    {
                        str += input[i];
                        i++;
                    }
                }
                if (i < input.Length)
                {
                    str += input[i];
                    i++;
                }
                tokens.Add(new Token(str, TokenType.String));
            }
            else
            {
                // Handle unrecognized characters
                i++;
            }
        }

        return tokens;
    }



    private static bool IsKeyword(string input)
    {
        string[] keywords = new string[] {
        "abstract", "as", "base", "bool", "break", "byte", "case", "catch", "char", "checked",
        "class", "const", "continue", "decimal", "default", "delegate", "do", "double", "else",
        "enum", "event", "explicit", "extern", "false", "finally", "fixed", "float", "for",
        "foreach", "goto", "if", "implicit", "in", "int", "interface", "internal", "is",
        "lock", "long", "namespace", "new", "null", "object", "operator", "out", "override",
        "params", "private", "protected", "public", "readonly", "ref", "return", "sbyte",
        "sealed", "short", "sizeof", "stackalloc", "static", "string", "struct", "switch",
        "this", "throw", "true", "try", "typeof", "uint", "ulong", "unchecked", "unsafe",
        "ushort", "using", "virtual", "void", "volatile", "while", "get", "set", "init"
    };

        return keywords.Contains(input);
    }

    private static bool IsOperator(char c)
    {
        string[] operators = new string[] {
        "+", "-", "*", "/", "%", "++", "--",
        "==", "!=", ">", "<", ">=", "<=",
        "&&", "||", "!",
        "&", "|", "^", "<<", ">>", "~",
        "=", "+=", "-=", "*=", "/=", "%=", "<<=", ">>=", "&=", "|=", "^="
    };

        return operators.Contains(c.ToString());
    }

    private static bool IsPunctuation(char c)
    {
        string[] punctuation = new string[] {
        ".", ",", ";", ":", "?", "(", ")", "[", "]", "{", "}", "#", "::"
    };

        return punctuation.Contains(c.ToString());
    }
}