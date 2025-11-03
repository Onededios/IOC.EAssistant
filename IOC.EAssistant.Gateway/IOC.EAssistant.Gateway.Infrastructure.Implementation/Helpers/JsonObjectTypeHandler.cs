using Dapper;
using System.Data;
using System.Text.Json.Nodes;

namespace IOC.EAssistant.Gateway.Infrastructure.Implementation.Helpers;
public class JsonObjectTypeHandler : SqlMapper.TypeHandler<JsonObject?>
{
    public override void SetValue(IDbDataParameter parameter, JsonObject? value)
    {
        parameter.Value = value?.ToJsonString() ?? (object)DBNull.Value;
        parameter.DbType = DbType.String;
    }

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
