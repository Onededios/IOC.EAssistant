using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IOC.EAssistant.Gateway.Library.Entities.Databases.EAssistant;
public class Session : Entity
{
    public DateTime expired_at { get; set; }
    public List<Conversation> conversations { get; set; } = new List<Conversation>();
}