using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DdkTestTool.Models
{
    public class TreeItem
    {
        /// <summary>
        /// 显示名称
        /// </summary>
        public string Name { get; set; }
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
        /// <summary>
        /// 子集集合
        /// </summary>
        public List<TreeItem> Children { get; set; }
        /// <summary>
        /// 图标
        /// </summary>
        public string? ImageSource { get; set; }
        public TreeItem(string name, string tag, int layer)
        {
            this.Name = name;
            this.Tag = tag;
            Layer = layer;
            Children = new List<TreeItem>();
        }   
    }
}
