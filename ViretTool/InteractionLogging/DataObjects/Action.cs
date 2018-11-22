using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViretTool.InteractionLogging.DataObjects
{
    public class Action
    {
        public string Category { get; set; }
        public string Type { get; set; }
        public string Value { get; set; }
        public string Attributes { get; set; }

        public Action(string category = null, string type = null, string value = null, string attributes = null)
        {
            Category = category;
            Type = type;
            Value = value;
            Attributes = attributes;
        }
    }
}
