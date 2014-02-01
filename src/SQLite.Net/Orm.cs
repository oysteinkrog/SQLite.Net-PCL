//
// Copyright (c) 2009-2012 Krueger Systems, Inc.
// Copyright (c) 2013 Øystein Krog (oystein.krog@gmail.com)
// Copyright (c) 2014 Benjamin Mayrargue (softlion@softlion.com)
//   Fix for support of multiple primary keys
//   Support new types: TimeSpan, DateTimeOffset, XElement
//   (breaking change) Fix i18n string support: stored as i18n string (nvarchar) instead of language dependant string (varchar)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using SQLite.Net.Attributes;

namespace SQLite.Net
{
    public static class Orm
    {
        public const int DefaultMaxStringLength = 140;
        public const string ImplicitPkName = "Id";
        public const string ImplicitIndexSuffix = "Id";

        public static string SqlDecl(TableMapping.Column p, bool storeDateTimeAsTicks)
        {
            //http://www.sqlite.org/lang_createtable.html
            return String.Format("\"{0}\" {1} {2} {3} {4} ",
                p.Name,
                SqlType(p, storeDateTimeAsTicks),
                p.IsAutoInc ? "primary key autoincrement" : null, //autoincrement can not be set with a multiple primary key
                !p.IsNullable ? "not null" : null,
                !String.IsNullOrEmpty(p.Collation) ? "collate " + p.Collation : null
                );
        }

        public static string SqlType(TableMapping.Column p, bool storeDateTimeAsTicks)
        {
            var clrType = p.ColumnType;
            if (clrType == typeof(Boolean) || clrType == typeof(Byte) || clrType == typeof(UInt16) || clrType == typeof(SByte) || clrType == typeof(Int16) || clrType == typeof(Int32))
            {
                return "integer";
            }
            if (clrType == typeof (UInt32) || clrType == typeof (Int64))
            {
                return "bigint";
            }
            if (clrType == typeof (Single) || clrType == typeof (Double) || clrType == typeof (Decimal))
            {
                return "float";
            }
            if (clrType == typeof(XElement))
            {
                return "nvarchar"; //SQLite ignores the length //See http://www.sqlite.org/datatype3.html
            }
            if (clrType == typeof (String))
            {
                var len = p.MaxStringLength;
                if(len.HasValue)
                    return "nvarchar(" + len + ")";
                return "nvarchar";
            }
            if (clrType == typeof(DateTimeOffset))
            {
                return "varchar";
            }
            if (clrType == typeof(TimeSpan))
            {
                return "bigint";
            }
            if (clrType == typeof (DateTime))
            {
                return storeDateTimeAsTicks ? "bigint" : "datetime";
            }
            if (clrType.IsEnum)
            {
                return "integer";
            }
            if (clrType == typeof (byte[]))
            {
                return "blob";
            }
            if (clrType == typeof (Guid))
            {
                return "varchar(36)";
            }
                
            throw new NotSupportedException("Don't know about " + clrType);
        }

        public static bool IsPK(MemberInfo p)
        {
            object[] attrs = p.GetCustomAttributes(typeof (PrimaryKeyAttribute), true);
            return attrs.Length > 0;
        }

        public static string Collation(MemberInfo p)
        {
            object[] attrs = p.GetCustomAttributes(typeof (CollationAttribute), true);
            if (attrs.Length > 0)
            {
                return ((CollationAttribute) attrs[0]).Value;
            }
            return string.Empty;
        }

        public static bool IsAutoInc(MemberInfo p)
        {
            object[] attrs = p.GetCustomAttributes(typeof (AutoIncrementAttribute), true);
            return attrs.Length > 0;
        }

        public static IEnumerable<IndexedAttribute> GetIndices(MemberInfo p)
        {
            object[] attrs = p.GetCustomAttributes(typeof (IndexedAttribute), true);
            return attrs.Cast<IndexedAttribute>();
        }

        public static int? MaxStringLength(PropertyInfo p)
        {
            object[] attrs = p.GetCustomAttributes(typeof (MaxLengthAttribute), true);
            if (attrs.Length > 0)
            {
                return ((MaxLengthAttribute) attrs[0]).Value;
            }
            return null; //DefaultMaxStringLength;
        }
    }
}
