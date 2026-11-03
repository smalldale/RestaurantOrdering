using System.Xml.Linq;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace RestaurantOrdering.API.Swagger;

/// <summary>
/// Appends XML documentation for enum members to the generated OpenAPI schema.
/// Swashbuckle only surfaces the enum type summary by default, so this filter
/// makes the documentation written on each enum value visible in Swagger UI.
/// </summary>
public class EnumXmlCommentsSchemaFilter : ISchemaFilter
{
    private readonly IReadOnlyDictionary<string, string> _memberDescriptions;

    /// <summary>
    /// Initializes a new instance of <see cref="EnumXmlCommentsSchemaFilter"/>.
    /// </summary>
    /// <param name="xmlDocumentationPath">Path to the generated XML documentation file.</param>
    public EnumXmlCommentsSchemaFilter(string xmlDocumentationPath)
    {
        _memberDescriptions = LoadMemberDescriptions(xmlDocumentationPath);
    }

    /// <inheritdoc />
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        var enumType = Nullable.GetUnderlyingType(context.Type) ?? context.Type;

        if (!enumType.IsEnum)
        {
            return;
        }

        var documentedValues = new List<string>();

        foreach (var name in Enum.GetNames(enumType))
        {
            var memberKey = $"F:{enumType.FullName}.{name}";

            if (_memberDescriptions.TryGetValue(memberKey, out var description))
            {
                documentedValues.Add($"- `{name}`: {description}");
            }
        }

        if (documentedValues.Count == 0)
        {
            return;
        }

        var enumDocumentation = "Values:\n" + string.Join("\n", documentedValues);

        schema.Description = string.IsNullOrWhiteSpace(schema.Description)
            ? enumDocumentation
            : $"{schema.Description}\n\n{enumDocumentation}";
    }

    private static Dictionary<string, string> LoadMemberDescriptions(
        string xmlDocumentationPath)
    {
        var descriptions = new Dictionary<string, string>();

        if (!File.Exists(xmlDocumentationPath))
        {
            return descriptions;
        }

        var document = XDocument.Load(xmlDocumentationPath);

        foreach (var member in document.Descendants("member"))
        {
            var name = member.Attribute("name")?.Value;

            if (name is null || !name.StartsWith("F:", StringComparison.Ordinal))
            {
                continue;
            }

            var summary = member.Element("summary")?.Value;

            if (string.IsNullOrWhiteSpace(summary))
            {
                continue;
            }

            descriptions[name] = NormalizeWhitespace(summary);
        }

        return descriptions;
    }

    private static string NormalizeWhitespace(string value)
    {
        return string.Join(
            " ",
            value.Split(
                [' ', '\r', '\n', '\t'],
                StringSplitOptions.RemoveEmptyEntries));
    }
}
