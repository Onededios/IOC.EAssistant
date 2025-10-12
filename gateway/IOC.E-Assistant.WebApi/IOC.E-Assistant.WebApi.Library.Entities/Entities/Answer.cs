using System.Text.Json.Nodes;

namespace IOC.E_Assistant.WebApi.Library.Models.Entities;
public class Answer : Entity
{
    public Guid question_id { get; set; }
    public string answer { get; set; } = string.Empty;
    public int token_count { get; set; }
    public JsonObject metadata { get; set; } = new JsonObject();
    public JsonObject sources { get; set; } = new JsonObject();
}
