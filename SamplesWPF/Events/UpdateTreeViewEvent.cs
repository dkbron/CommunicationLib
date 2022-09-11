using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SamplesWPF.Events
{ 
    public class UpdateTreeViewModel
    {
        public enum UpdateType
        {
            Add,
            Remove,
        }
        public string NodeName { get; set; }
        public string ParentName { get; set; }
        public UpdateType Type { get; set; }
    }
    public class UpdateTreeViewEvent : PubSubEvent<UpdateTreeViewModel>
    {
    }
}
