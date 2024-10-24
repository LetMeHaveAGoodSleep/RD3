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

        public List<List<double>> Xs { get; set; } = [];

        public List<List<double>> Ys { get; set; } = [];
    }
}
