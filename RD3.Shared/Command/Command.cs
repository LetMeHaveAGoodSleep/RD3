using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RD3.Shared
{
    public class Command
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public List<CommandParam> CommandParams { get; set; }
    }
    public class CommandParam
    {
        public string Name { get; set; }
        public Type Type { get; set; }
        public int Length { get; set; }
    }
}
