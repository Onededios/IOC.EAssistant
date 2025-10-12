using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IOC.E_Assistant.Infraestructure.Implementation.Databases.EAssistant.Entities;
public class Session : Entity
{
    public DateTime expired_at { get; set; }
    public ICollection<Question> questions { get; set; } = new List<Question>();
    public ICollection<Answer> answers { get; set; } = new List<Answer>();
}
