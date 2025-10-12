using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace IOC.E_Assistant.Infraestructure.Implementation.Databases.EAssistant.Entities;
public class Answer : Entity
{
    public Guid question_id { get; set; }
    public string answer { get; set; } = string.Empty;
    public int token_count { get; set; }
    public JsonObject metadata { get; set; } = new JsonObject();
}
