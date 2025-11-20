using Dapper;
using System.Data;
using System.Text.Json.Nodes;

namespace IOC.EAssistant.Gateway.Infrastructure.Implementation.Helpers;

/// <summary>
/// Custom Dapper type handler for mapping <see cref="JsonObject"/> types to and from database JSON columns.
/// </summary>
/// <remarks>
/// This handler enables Dapper to automatically convert between <see cref="JsonObject"/> instances 
/// and their database string representations. It handles null values and DBNull appropriately.
/// </remarks>
public class JsonObjectTypeHandler : SqlMapper.TypeHandler<JsonObject?>
{
    /// <summary>
    /// Sets the value of the database parameter from a <see cref="JsonObject"/> instance.
    /// </summary>
    /// <param name="parameter">The database parameter to set.</param>
    /// <param name="value">The <see cref="JsonObject"/> value to convert to a JSON string, or <see langword="null"/>.</param>
    /// <remarks>
    /// If the <paramref name="value"/> is <see langword="null"/>, the parameter will be set to <see cref="DBNull.Value"/>.
    /// Otherwise, the JSON object is serialized to a string and stored as <see cref="DbType.String"/>.
    /// </remarks>
    public override void SetValue(IDbDataParameter parameter, JsonObject? value)
    {
        parameter.Value = value?.ToJsonString() ?? (object)DBNull.Value;
        parameter.DbType = DbType.String;
    }

    /// <summary>
    /// Parses a database value into a <see cref="JsonObject"/> instance.
    /// </summary>
    /// <param name="value">The database value to parse, typically a JSON string.</param>
    /// <returns>
    /// A <see cref="JsonObject"/> instance parsed from the database value, 
    /// or <see langword="null"/> if the value is null, <see cref="DBNull"/>, or an empty string.
    /// </returns>
    /// <remarks>
    /// This method handles null values, <see cref="DBNull"/> instances, and empty or whitespace strings 
    /// by returning <see langword="null"/>. Valid JSON strings are parsed using <see cref="JsonNode.Parse(string, JsonNodeOptions?, JsonDocumentOptions)"/>.
    /// </remarks>
    public override JsonObject? Parse(object value)
    {
        if (value == null || value is DBNull)
            return null;

        var json = value.ToString();
        if (string.IsNullOrWhiteSpace(json))
            return null;

        return JsonNode.Parse(json) as JsonObject;
    }
}
