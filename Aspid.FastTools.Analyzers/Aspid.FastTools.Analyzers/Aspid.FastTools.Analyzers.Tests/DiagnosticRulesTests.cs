using Xunit;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Reflection;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Aspid.FastTools.Analyzers.Descriptions;

namespace Aspid.FastTools.Analyzers.Tests;

public sealed class DiagnosticRulesTests
{
    private const string DocsUrl = "https://vpdpersonal.github.io/Aspid.FastTools/docs/";

    private static readonly Regex HeadingPattern = new(@"^#{1,6}\s+(.+?)\s*$");
    private static readonly Regex ExplicitIdPattern = new(@"\{#([^}]+)\}$");
    private static readonly Regex HtmlTagPattern = new("<[^>]+>");
    private static readonly Regex OrderPrefixPattern = new(@"^\d+-");

    private static readonly DiagnosticDescriptor[] Rules = typeof(DiagnosticRules)
        .GetFields(BindingFlags.Public | BindingFlags.Static)
        .Where(field => field.FieldType == typeof(DiagnosticDescriptor))
        .Select(field => (DiagnosticDescriptor)field.GetValue(obj: null)!)
        .ToArray();

    // The docs pages embedded by the csproj, keyed by their site route: 03-type-selector.md -> type-selector.
    private static readonly Dictionary<string, string> DocsPages = typeof(DiagnosticRulesTests).Assembly
        .GetManifestResourceNames()
        .Where(name => name.StartsWith("Docs/", StringComparison.Ordinal))
        .ToDictionary(
            name => OrderPrefixPattern.Replace(Path.GetFileNameWithoutExtension(name), replacement: string.Empty),
            ReadResource);

    [Fact]
    public void EveryRule_HasDescriptionAndDocsHelpLink()
    {
        Assert.NotEmpty(Rules);

        foreach (var rule in Rules)
        {
            Assert.False(string.IsNullOrWhiteSpace(rule.Description.ToString()), $"{rule.Id} has no description");
            Assert.True(rule.HelpLinkUri.StartsWith(DocsUrl, StringComparison.Ordinal), $"{rule.Id}: {rule.HelpLinkUri}");
        }
    }

    // A renamed heading breaks the IDE link without an error: the page still opens, but not at the section.
    [Fact]
    public void EveryHelpLink_PointsToHeadingOnItsPage()
    {
        var broken = new List<string>();

        foreach (var rule in Rules.Where(rule => rule.HelpLinkUri.StartsWith(DocsUrl, StringComparison.Ordinal)))
        {
            var parts = rule.HelpLinkUri.Substring(DocsUrl.Length).Split('#');
            var page = parts[0];

            if (!DocsPages.TryGetValue(page, out var markdown))
                broken.Add($"{rule.Id}: no page '{page}'");
            else if (parts.Length > 1 && !HeadingIds(markdown).Contains(parts[1]))
                broken.Add($"{rule.Id}: no heading '#{parts[1]}' on '{page}'");
        }

        Assert.True(broken.Count == 0, string.Join(Environment.NewLine, broken));
    }

    private static IEnumerable<string> HeadingIds(string markdown)
    {
        var inCodeBlock = false;

        foreach (var line in markdown.Split('\n'))
        {
            var text = line.TrimEnd('\r');

            if (text.StartsWith("```", StringComparison.Ordinal))
            {
                inCodeBlock = !inCodeBlock;
                continue;
            }

            if (inCodeBlock) continue;

            var heading = HeadingPattern.Match(text);
            if (!heading.Success) continue;

            var explicitId = ExplicitIdPattern.Match(heading.Groups[1].Value);
            yield return explicitId.Success ? explicitId.Groups[1].Value : Slug(heading.Groups[1].Value);
        }
    }

    // The github-slugger rule Docusaurus uses for plain headings: lower case, punctuation dropped, spaces to hyphens.
    private static string Slug(string heading)
    {
        var text = HtmlTagPattern.Replace(heading, replacement: string.Empty).ToLowerInvariant();
        var slug = new StringBuilder(text.Length);

        foreach (var symbol in text)
        {
            if (symbol == ' ') slug.Append('-');
            else if (char.IsLetterOrDigit(symbol) || symbol is '-' or '_') slug.Append(symbol);
        }

        return slug.ToString();
    }

    private static string ReadResource(string name)
    {
        using var stream = typeof(DiagnosticRulesTests).Assembly.GetManifestResourceStream(name)!;
        using var reader = new StreamReader(stream);

        return reader.ReadToEnd();
    }
}
