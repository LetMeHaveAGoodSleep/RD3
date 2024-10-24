using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RD3.Shared
{
    public class DeviceExperimentHistoryData
    {
        public string DeviceName {  get; set; }

        public List<ExperimentHistoryData> ExperimentHistoryDatas { get; set; }= [];
    }
}
