using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DdkTestTool.Models
{
    public class Tool
    {
        public Tool(string name, string tag)
        {
            this.Name = name;
            this.Tag = tag;
        }
        public string Name { get; set; }
        public string Tag { get; set; }
        public string? Description { get; set; }

        public string? ImageSource { get; set; }
    }
}
