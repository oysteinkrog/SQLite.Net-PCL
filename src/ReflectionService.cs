using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using SQLite.Net.Interop;

namespace SQLite.Net.Platform {
    public abstract class ReflectionService : IReflectionService {
        protected internal ReflectionService() { }

        public IEnumerable<PropertyInfo> GetPublicInstanceProperties(Type mappedType) {
            return mappedType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.SetProperty);
        }

        public IEnumerable<PropertyInfo> GetDecoratedPrivateInstanceProperties(Type mappedType, Type attributeType) {
            return mappedType.GetProperties(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetProperty)
                .Where(x => x.GetSetMethod(false) != null || x.GetCustomAttribute(attributeType) != null);
        }

        public object GetMemberValue(object obj, Expression expr, MemberInfo member) {
            if (member.MemberType == MemberTypes.Property) {
                var m = (PropertyInfo)member;
                return m.GetValue(obj, null);
            }
            if (member.MemberType == MemberTypes.Field) {
                var m = (FieldInfo)member;
                return m.GetValue(obj);
            }
            throw new NotSupportedException("MemberExpr: " + member.MemberType);
        }
    }
}