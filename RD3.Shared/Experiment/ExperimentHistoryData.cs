using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RD3.Shared
{
   public class ExperimentHistoryData
    {
        public ExperimentParameter ExperimentParameter { get; set; }

        public Dictionary<IEnumerable<double>, IEnumerable<double>> Data { get; set; } = [];
    }
}
