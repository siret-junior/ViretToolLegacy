using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViretTool.InteractionLogging.DataObjects
{
    public class Log
    {
        public int TeamId { get; set; }
        public int MemberId { get; set; }
        public List<Event> Events { get; set; }

        public string TeamName { get; set; }



        public Log()
        {
            Events = new List<Event>();
        }
    }
}
