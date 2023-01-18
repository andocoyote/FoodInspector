using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InspectionEvaluator.Model
{
    public class ServiceBusQueueEvent
    {
        public string id { get; set; }
        public string subject { get; set; }
        public string data { get; set; }
        public string eventType { get; set; }
    }
}
