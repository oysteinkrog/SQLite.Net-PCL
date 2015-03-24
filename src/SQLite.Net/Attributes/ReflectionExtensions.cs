using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SQLite.Net.Attributes
{
    internal static class ReflectionExtensions
    {
        internal static IEnumerable<T> GetCustomAttributes<T>(this MemberInfo member) where T : Attribute
        {
            return member.GetCustomAttributes(typeof (T), true).Cast<T>();
        }
    }
}