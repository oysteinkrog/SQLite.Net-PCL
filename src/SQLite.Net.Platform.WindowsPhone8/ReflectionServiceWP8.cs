﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using SQLite.Net.Interop;

namespace SQLite.Net.Platform.WindowsPhone8
{
    public class ReflectionServiceWP8 : IReflectionService
    { 
        public IEnumerable<PropertyInfo> GetPublicInstanceProperties(Type mappedType)
        {
            if (mappedType == null) throw new ArgumentNullException(nameof(mappedType));

            return from p in mappedType.GetRuntimeProperties()
                   where
                       ((p.GetMethod != null && p.GetMethod.IsPublic) || (p.SetMethod != null && p.SetMethod.IsPublic) ||
                        (p.GetMethod != null && p.GetMethod.IsStatic) || (p.SetMethod != null && p.SetMethod.IsStatic))
                   select p;
        }

        public IEnumerable<PropertyInfo> GetDecoratedPrivateInstanceProperties(Type mappedType, Type attributeType)
        {
            return from p in mappedType.GetRuntimeProperties()
                   where (
                       (p.GetMethod != null && p.GetMethod.IsPrivate) || (p.SetMethod != null && p.SetMethod.IsPrivate) ||
                       p.GetCustomAttribute(attributeType) != null
                   )
                   select p;
        }

        public object GetMemberValue(object obj, Expression expr, MemberInfo member)
        {
            if (member is PropertyInfo) 
            { 
                var m = (PropertyInfo) member; 
                return m.GetValue(obj, null); 
            } 
            if (member is FieldInfo) 
            { 
                var m = (FieldInfo) member; 
                return m.GetValue(obj); 
            } 
            throw new NotSupportedException("MemberExpr: " + member.DeclaringType); 
        }
    }
}