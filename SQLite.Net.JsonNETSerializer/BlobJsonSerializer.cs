using System;
using System.Text;

using Newtonsoft.Json;

namespace SQLite.Net.JsonNETSerializer
{
    public class BlobJsonSerializer : IBlobSerializer
    {
        public byte[] Serialize<T>(T obj)
        {
            var str = JsonConvert.SerializeObject(obj);
            return Encoding.UTF8.GetBytes(str);
        }

        public object Deserialize(byte[] data, Type type)
        {
            var str = Encoding.UTF8.GetString(data, 0, data.Length);
            return JsonConvert.DeserializeObject(str, type);
        }

        public bool CanDeserialize(Type type)
        {
            return (type.IsClass && !type.IsGenericParameter);
        }
    }
}
