using DdkTestTool.Models;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DdkTestTool.Events
{ 
    public class UpdateTreeViewModel
    {
        public enum UpdateType
        {
            Add,
            Remove,
            Update,
        }
        public TreeItem TreeItem { get; set; }
        public UpdateType Type { get; set; }
        public UpdateTreeViewModel(TreeItem treeItem, UpdateType type = UpdateType.Add)
        {
            TreeItem = treeItem;
            Type = type;
        }

    }
    public class UpdateTreeViewEvent : PubSubEvent<UpdateTreeViewModel>
    { 
    }
}
