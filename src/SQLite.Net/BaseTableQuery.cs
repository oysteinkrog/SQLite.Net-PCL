//
// Copyright (c) 2012 Krueger Systems, Inc.
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

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;

namespace SQLite.Net
{
    public abstract class BaseTableQuery
    {
        protected class Ordering
        {
            public string ColumnName { get; set; }
            public bool Ascending { get; set; }
        }

        protected class CompileResult
        {
            public string CommandText { get; set; }

            public object Value { get; set; }
        }

        private static readonly Dictionary<ExpressionType, string> SqlNames = new Dictionary<ExpressionType, string>
        {
            {ExpressionType.GreaterThan, ">"},
            {ExpressionType.GreaterThanOrEqual, ">="},
            {ExpressionType.LessThan, "<"},
            {ExpressionType.LessThanOrEqual, "<="},
            {ExpressionType.And, "&"},
            {ExpressionType.AndAlso, "and"},
            {ExpressionType.Or, "|"},
            {ExpressionType.OrElse, "or"},
            {ExpressionType.Equal, "="},
            {ExpressionType.NotEqual, "!="},
        };

        protected  static object ConvertTo(object obj, Type t)
        {
            var nut = Nullable.GetUnderlyingType(t);

            if (nut != null)
            {
                return obj == null ? null : Convert.ChangeType(obj, nut, CultureInfo.CurrentCulture);
            }

            return Convert.ChangeType(obj, t, CultureInfo.CurrentCulture);
        }


        protected static string GetSqlName(Expression expr)
        {
            var n = expr.NodeType;

            string sqlName;
            if (SqlNames.TryGetValue(expr.NodeType, out sqlName))
                return sqlName;

            throw new NotSupportedException("Cannot get SQL operator name for node type: " + n);
        }
    }
}
