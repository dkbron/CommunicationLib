using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SamplesWPF.Models
{   
    public sealed class ItemViewModel
    {
        public ItemViewModel(string name)
        { 
            Name = name;
        }

        public string Name { get;}

        public ObservableCollection<ItemViewModel> Children { get; set; } = new ObservableCollection<ItemViewModel>();
    }

    public sealed class TreeL1
    {
        public TreeL1(string name)
        {
            Name = name; 
        }

        public string Name { get; set; }
        public ObservableCollection<TreeL2> treeL2s { get; set; } = new ObservableCollection<TreeL2>();
    }

    public sealed class TreeL2
    {
        public TreeL2(string name)
        {
            Name = name;
        }

        public TreeL1? Parent;

        public string Name { get; set; }
        public ObservableCollection<TreeL3> treeL3s { get; set; } = new ObservableCollection<TreeL3>();
    }
    public sealed class TreeL3
    {
        public TreeL3(string name)
        {
            Name = name;
        }

        public TreeL2? Parent;

        public string Name { get; set; }
    }
}
