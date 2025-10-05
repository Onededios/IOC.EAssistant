using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace IOC.E_Assistant.Infraestructure.Contracts.NeonDB.Entities;
public class Question : Entity
{
    public Guid session_id { get; set; }
    public required string question { get; set; }
    public int token_count { get; set; }
    public required JsonObject metadata { get; set; }
}
