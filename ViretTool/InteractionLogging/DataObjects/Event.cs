using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViretTool.InteractionLogging.DataObjects
{
    public class Event
    {
        public long Timestamp { get; set; }
        public List<Action> Actions { get; set; }

        public Event()
        {
            Timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            Actions = new List<Action>();
        }
    }
}
