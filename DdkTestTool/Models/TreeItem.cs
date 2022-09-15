using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DdkTestTool.Models
{
    public class TreeItem :BindableBase
    {
        /// <summary>
        /// 显示名称
        /// </summary> 
        private string name;

        public string Name
        {
            get { return name; }
            set { name = value;  RaisePropertyChanged(); }
        }

        /// <summary>
        /// 内容标签
        /// </summary> 
        public string Tag { get; set; }

        /// <summary>
        /// 层级
        /// </summary>
        public int Layer { get; set; }
        /// <summary>
        /// 父级元素
        /// </summary>
        public TreeItem? Parent { get; set; }
        public string? ParentTag { get; set; }
        /// <summary>
        /// 子集集合
        /// </summary>
        public ObservableCollection<TreeItem> Children { get; set; }
        /// <summary>
        /// 图标
        /// </summary>
        private string imageSource;
        public string? ImageSource
        {
            get { return imageSource; }
            set { imageSource = value; RaisePropertyChanged(); }
        }
        public TreeItem(string name, string tag, int layer)
        {
            this.Name = name;
            this.Tag = tag;
            Layer = layer;
            Children = new ObservableCollection<TreeItem>();
        }
         
    }
}
