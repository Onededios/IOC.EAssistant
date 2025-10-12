using System.Text.Json.Nodes;

namespace IOC.E_Assistant.WebApi.Library.Models.Entities;
public class Question : Entity
{
    public required string question { get; set; }
    public int token_count { get; set; }
    public required JsonObject metadata { get; set; }
    public Answer answer { get; set; }
}
