using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IOC.E_Assistant.Infraestructure.Contracts.NeonDB.Entities;
public abstract class Entity
{
    public Guid id { get; set; }
    public DateTime created_at { get; set; }
}
