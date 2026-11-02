using System.Collections.Concurrent;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using Application.Common.Interfaces.Services;

namespace Infrastructure.Email;

public sealed record EmailContent(string Subject, string HtmlBody);

/// <summary>
/// Renders the embedded HTML templates in <c>Email/Templates</c>. Content files
/// are fragments; they are wrapped in <c>Layout.html</c> at render time.
/// </summary>
public sealed class EmailTemplateRenderer
{
    private static readonly Assembly Assembly = typeof(EmailTemplateRenderer).Assembly;
    private static readonly ConcurrentDictionary<string, string> Cache = new(StringComparer.OrdinalIgnoreCase);

    private static readonly Regex SubjectRegex = new(
        @"^\s*<!--\s*subject:\s*(?<subject>.*?)\s*-->",
        RegexOptions.IgnoreCase | RegexOptions.Singleline);

    private static readonly Regex RawTokenRegex = new(@"\{\{\{(?<key>[A-Za-z0-9_]+)\}\}\}");
    private static readonly Regex TokenRegex = new(@"\{\{(?<key>[A-Za-z0-9_]+)\}\}");

    private readonly EmailConfiguration _config;

    public EmailTemplateRenderer(EmailConfiguration config) => _config = config;

    public EmailContent Render(
        EmailTemplate template,
        IReadOnlyDictionary<string, string?> model)
    {
        var source = LoadTemplate(template.ToString());

        var match = SubjectRegex.Match(source);
        var subject = match.Success ? match.Groups["subject"].Value.Trim() : template.ToString();
        var fragment = match.Success ? source[match.Length..] : source;

        var values = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
        {
            ["AppName"] = _config.FromName,
            ["AppUrl"] = _config.AppBaseUrl,
            ["SupportEmail"] = _config.SupportEmail,
            ["Year"] = DateTime.UtcNow.Year.ToString(),
        };

        foreach (var (key, value) in model)
            values[key] = value;

        var renderedSubject = ReplaceTokens(subject, values);
        var content = ReplaceTokens(fragment, values);

        var layout = LoadTemplate("Layout");
        values["Content"] = content;
        var html = ReplaceTokens(layout, values, encode: false);

        return new EmailContent(renderedSubject, html);
    }

    private static string LoadTemplate(string name)
    {
        return Cache.GetOrAdd(name, key =>
        {
            var resource = Assembly.GetManifestResourceNames()
                .FirstOrDefault(n => n.EndsWith($".{key}.html", StringComparison.OrdinalIgnoreCase))
                ?? throw new InvalidOperationException(
                    $"Email template '{key}.html' was not found. Available templates: " +
                    string.Join(", ", Assembly.GetManifestResourceNames()));

            using var stream = Assembly.GetManifestResourceStream(resource)
                ?? throw new InvalidOperationException($"Email template '{resource}' could not be opened.");
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        });
    }

    private static string ReplaceTokens(
        string input,
        IReadOnlyDictionary<string, string?> values,
        bool encode = true)
    {
        // Raw tokens ({{{Body}}}) are inserted as HTML. Encoded tokens ({{Name}})
        // are always HTML-encoded so user-provided values cannot inject markup.
        input = RawTokenRegex.Replace(input, match =>
            values.TryGetValue(match.Groups["key"].Value, out var raw) ? raw ?? string.Empty : string.Empty);

        input = TokenRegex.Replace(input, match =>
        {
            if (!values.TryGetValue(match.Groups["key"].Value, out var value))
                return string.Empty;

            value ??= string.Empty;
            return encode ? WebUtility.HtmlEncode(value) : value;
        });

        return input;
    }
}
