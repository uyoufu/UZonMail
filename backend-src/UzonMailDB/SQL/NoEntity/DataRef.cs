namespace UZonMail.DB.SQL.NoEntity
{
    /// <summary>
    /// 数据引用
    /// 当要保存数组数据，但是又不想保存全部实例时，可通过此类来建立一个引用
    /// </summary>
    public class DataRef : IDataRef
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public DataRef() { }

        /// <summary>
        /// 新建一个数据引用
        /// </summary>
        /// <param name="id"></param>
        /// <param name="name"></param>
        /// <param name="description"></param>
        public DataRef(long id, string name, string description = "")
        {
            Id = id;
            Name = name;
            Description = description;
        }

        public DataRef(IDataRef data) : this(data.Id, data.Name, data.Description)
        {
        }

        public DataRef(IDataRefName data) : this(data.Id, data.Name) { }

        /// <summary>
        /// 从实体中创建一个引用
        /// 这个方法效率没有直接 new DataRef 高
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <param name="idField">必须是 int 类型</param>
        /// <param name="nameField"></param>
        /// <param name="descriptionField"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static DataRef From<T>(T data, string idField = "Id", string nameField="Name",string descriptionField="Description")
        {
            var type = typeof(T);
            var id = type.GetProperty(idField)?.GetValue(data);
            var name = type.GetProperty(nameField)?.GetValue(data);
            var description = type.GetProperty(descriptionField)?.GetValue(data);
            if (id == null || name == null)
            {
                throw new ArgumentException("Id or Name is null");
            }

            // 判断 id 是否是 int 类型
            if (id.GetType() != typeof(int))
            {
                throw new ArgumentException("Id is not int");
            }

            return new DataRef((int)id, name.ToString(), description?.ToString() ?? "");
        }
    }
}
