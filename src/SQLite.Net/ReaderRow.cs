using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace SQLite.Net
{
    public class ReaderItem
    {
        private readonly IDictionary<string, object> data;

        [PublicAPI]
        public object this[string propertyName]
        {
            get
            {
                if (data.ContainsKey(propertyName))
                    return data[propertyName];
                else
                    return null;
            }
            set
            {
                if (data.ContainsKey(propertyName))
                    data[propertyName] = value;
                else
                    data.Add(propertyName, value);
            }
        }

        /// <summary>
        /// Get column names
        /// </summary>
        [PublicAPI]
        public List<string> Fields
        {
            get
            {
                return data.Keys.ToList();
            }
        }

        [PublicAPI]
        public ReaderItem()
        {
            data = new Dictionary<string, object>();
        }
    }
}
