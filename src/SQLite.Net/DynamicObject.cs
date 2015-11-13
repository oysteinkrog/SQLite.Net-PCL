using System;
using System.Collections.Generic;
using System.Dynamic;

// From http://www.sullinger.us/blog/2014/1/6/create-objects-dynamically-in-c
namespace SQLite.Net
{
    public class DynamicObject
    {
        public dynamic Instance = new ExpandoObject();

        public void AddProperty(string name, object value)
        {
            ((IDictionary<string, object>)this.Instance).Add(name, value);
        }

        public dynamic GetProperty(string name)
        {
            if (((IDictionary<string, object>)this.Instance).ContainsKey(name))
                return ((IDictionary<string, object>)this.Instance)[name];
            else
                return null;
        }

        public void AddMethod(Action methodBody, string methodName)
        {
            this.AddProperty(methodName, methodBody);
        }
    }
}
