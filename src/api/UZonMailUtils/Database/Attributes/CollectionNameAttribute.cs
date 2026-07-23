using System;
using System.Collections.Generic;
using System.Text;

namespace UzonMail.Utils.Database.Attributes
{
    /// <summary>
    /// 用于标记集合名称
    /// </summary>
    public class CollectionNameAttribute:Attribute
    {
        public string Name { get; set; }
        public CollectionNameAttribute(string name)
        {
            Name = name;
        }
    }
}
