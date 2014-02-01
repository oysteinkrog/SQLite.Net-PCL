//
// Copyright (c) 2012 Krueger Systems, Inc.
// Copyright (c) 2013 Øystein Krog (oystein.krog@gmail.com)
// Copyright (c) 2014 Benjamin Mayrargue (softlion@softlion.com)
//  - Included patch from https://github.com/praeclarum/sqlite-net/issues/158 (implements NOT operator)
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
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using SQLite.Net.Interop;

namespace SQLite.Net
{
    public class TableQuery<T> : BaseTableQuery, IEnumerable<T>
    {
        private readonly ISQLitePlatform _sqlitePlatform;
        private bool _deferred;

        private BaseTableQuery _joinInner;
        private Expression _joinInnerKeySelector;
        private BaseTableQuery _joinOuter;
        private Expression _joinOuterKeySelector;
        private Expression _joinSelector;
        private int? _limit;
        private int? _offset;
        private List<Ordering> _orderBys;

        private Expression _selector;
        private Expression _where;

        private TableQuery(ISQLitePlatform platformImplementation, SQLiteConnection conn, TableMapping table)
        {
            _sqlitePlatform = platformImplementation;
            Connection = conn;
            Table = table;
        }

        public TableQuery(ISQLitePlatform platformImplementation, SQLiteConnection conn)
        {
            _sqlitePlatform = platformImplementation;
            Connection = conn;
            Table = Connection.GetMapping(typeof (T));
        }

        public SQLiteConnection Connection { get; private set; }

        public TableMapping Table { get; private set; }

        private IEnumerable<TResult> GetEnumerable<TResult>()
        {
            var selectedColumns = ParseSelectedColumnsExpression();

            if (!_deferred)
            {
                return GenerateCommand(selectedColumns).ExecuteQuery<TResult>();
            }

            return GenerateCommand(selectedColumns).ExecuteDeferredQuery<TResult>();
        }

        /// <summary>
        /// Parse _selector expression
        /// </summary>
        /// <returns>
        /// a simple type, or the object properties, in a comma separated string
        /// </returns>
        private string ParseSelectedColumnsExpression()
        {
            if (_selector == null)
                return "*";

            //TODO: find all members of T used in the expression
            //currently support a single member: Select(t => t.MemberName)
            var propertyNames = new [] { ((_selector as LambdaExpression).Body as MemberExpression).Member.Name };

            //Then map propertyNames to column names
            var mapping = Connection.GetMapping(typeof (T));
            var selectedColumns = String.Join(",",propertyNames.Select(p => "\""+mapping.FindColumnWithPropertyName(p).Name+"\""));

            return selectedColumns;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return GetEnumerable<T>().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerable<T>().GetEnumerator();
        }

        public TableQuery<T> Clone()
        {
            var q = new TableQuery<T>(_sqlitePlatform, Connection, Table);
            q._where = _where;
            q._deferred = _deferred;
            if (_orderBys != null)
            {
                q._orderBys = new List<Ordering>(_orderBys);
            }
            q._limit = _limit;
            q._offset = _offset;
            q._joinInner = _joinInner;
            q._joinInnerKeySelector = _joinInnerKeySelector;
            q._joinOuter = _joinOuter;
            q._joinOuterKeySelector = _joinOuterKeySelector;
            q._joinSelector = _joinSelector;
            q._selector = _selector;
            return q;
        }

        public TableQuery<T> Where(Expression<Func<T, bool>> predExpr)
        {
            if (predExpr.NodeType == ExpressionType.Lambda)
            {
                var lambda = (LambdaExpression) predExpr;
                Expression pred = lambda.Body;
                TableQuery<T> q = Clone();
                q.AddWhere(pred);
                return q;
            }
            else
            {
                throw new NotSupportedException("Must be a predicate");
            }
        }

        public TableQuery<T> Take(int n)
        {
            TableQuery<T> q = Clone();
            q._limit = n;
            return q;
        }

        public TableQuery<T> Skip(int n)
        {
            TableQuery<T> q = Clone();
            q._offset = n;
            return q;
        }

        public T ElementAt(int index)
        {
            return Skip(index).Take(1).First();
        }

        public TableQuery<T> Deferred()
        {
            TableQuery<T> q = Clone();
            q._deferred = true;
            return q;
        }

        public TableQuery<T> OrderBy<TValue>(Expression<Func<T, TValue>> orderExpr)
        {
            return AddOrderBy(orderExpr, true);
        }

        public TableQuery<T> OrderByDescending<TValue>(Expression<Func<T, TValue>> orderExpr)
        {
            return AddOrderBy(orderExpr, false);
        }

        private TableQuery<T> AddOrderBy<TValue>(Expression<Func<T, TValue>> orderExpr, bool asc)
        {
            if (orderExpr.NodeType == ExpressionType.Lambda)
            {
                var lambda = (LambdaExpression) orderExpr;

                MemberExpression mem = null;

                var unary = lambda.Body as UnaryExpression;
                if (unary != null && unary.NodeType == ExpressionType.Convert)
                {
                    mem = unary.Operand as MemberExpression;
                }
                else
                {
                    mem = lambda.Body as MemberExpression;
                }

                if (mem != null && (mem.Expression.NodeType == ExpressionType.Parameter))
                {
                    TableQuery<T> q = Clone();
                    if (q._orderBys == null)
                    {
                        q._orderBys = new List<Ordering>();
                    }
                    q._orderBys.Add(new Ordering
                    {
                        ColumnName = Table.FindColumnWithPropertyName(mem.Member.Name).Name,
                        Ascending = asc
                    });
                    return q;
                }

                throw new NotSupportedException("Order By does not support: " + orderExpr);
            }

            throw new NotSupportedException("Must be a predicate");
        }

        private void AddWhere(Expression pred)
        {
            _where = _where == null ? pred : Expression.AndAlso(_where, pred);
        }

        public TableQuery<TResult> Join<TInner, TKey, TResult>(
            TableQuery<TInner> inner,
            Expression<Func<T, TKey>> outerKeySelector,
            Expression<Func<TInner, TKey>> innerKeySelector,
            Expression<Func<T, TInner, TResult>> resultSelector)
        {
            var q = new TableQuery<TResult>(_sqlitePlatform, Connection, Connection.GetMapping(typeof (TResult)))
            {
                _joinOuter = this,
                _joinOuterKeySelector = outerKeySelector,
                _joinInner = inner,
                _joinInnerKeySelector = innerKeySelector,
                _joinSelector = resultSelector,
            };
            return q;
        }

        /// <summary>
        /// Select is always last, we are able (and should) return an enumeration here instead of a clone.
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="selector"></param>
        /// <returns></returns>
        public IEnumerable<TResult> Select<TResult>(Expression<Func<T, TResult>> selector)
        {
            //var q = Clone();
            //q._selector = selector;
            //return q;

            _selector = selector;
            return GetEnumerable<TResult>();
        }

        private SQLiteCommand GenerateCommand(string selectionList)
        {
            if (_joinInner != null && _joinOuter != null)
            {
                throw new NotSupportedException("Joins are not supported.");
            }

            var cmdText = new StringBuilder().AppendFormat("select {0} from \"{1}\"", selectionList, Table.TableName);
            var args = new List<object>();
            if (_where != null)
            {
                CompileResult w = CompileExpr(_where, args);
                cmdText.Append(" where ").Append(w.CommandText);
            }
            if ((_orderBys != null) && (_orderBys.Count > 0))
            {
                string t = string.Join(", ", _orderBys.Select(o => "\"" + o.ColumnName + "\"" + (o.Ascending ? "" : " desc")).ToArray());
                cmdText.Append(" order by ").Append(t);
            }
            if (_limit.HasValue)
            {
                cmdText.Append(" limit ").Append(_limit.Value);
            }
            if (_offset.HasValue)
            {
                if (!_limit.HasValue)
                {
                    cmdText.Append(" limit -1 ");
                }
                cmdText.Append(" offset ").Append(_offset.Value);
            }
            return Connection.CreateCommand(cmdText.ToString(), args.ToArray());
        }

        private CompileResult CompileExpr(Expression expr, List<object> queryArgs)
        {
            if (expr == null)
            {
                throw new NotSupportedException("Expression is NULL");
            }
            
            if (expr is BinaryExpression)
            {
                var bin = (BinaryExpression) expr;

                var leftr = CompileExpr(bin.Left, queryArgs);
                var rightr = CompileExpr(bin.Right, queryArgs);

                //If either side is a parameter and is null, then handle the other side specially (for "is null"/"is not null")
                string text;
                if (leftr.CommandText == "?" && leftr.Value == null)
                {
                    text = CompileNullBinaryExpression(bin, rightr);
                }
                else if (rightr.CommandText == "?" && rightr.Value == null)
                {
                    text = CompileNullBinaryExpression(bin, leftr);
                }
                else
                {
                    text = "(" + leftr.CommandText + " " + GetSqlName(bin) + " " + rightr.CommandText + ")";
                }
                return new CompileResult
                {
                    CommandText = text
                };
            }
            
            if (expr.NodeType == ExpressionType.Call)
            {
                var call = (MethodCallExpression) expr;
                var args = new CompileResult[call.Arguments.Count];
                CompileResult obj = call.Object != null ? CompileExpr(call.Object, queryArgs) : null;

                for (int i = 0; i < args.Length; i++)
                {
                    args[i] = CompileExpr(call.Arguments[i], queryArgs);
                }

                var sqlCall = new StringBuilder();

                if (call.Method.Name == "Like" && args.Length == 2)
                {
                    sqlCall.AppendFormat("({0} like {1})", args[0].CommandText, args[1].CommandText);
                }
                else if (call.Method.Name == "Contains" && args.Length == 2)
                {
                    sqlCall.AppendFormat("({0} in {1})", args[1].CommandText, args[0].CommandText);
                }
                else if (call.Method.Name == "Contains" && args.Length == 1)
                {
                    if (obj != null)
                    {
                        if (call.Object != null && call.Object.Type == typeof (string))
                        {
                            sqlCall.AppendFormat("({0} like ('%' || {1} || '%'))", obj.CommandText, args[0].CommandText);
                        }
                        else
                        {
                            sqlCall.AppendFormat("({0} in {1})", args[0].CommandText, obj.CommandText);
                        }
                    }
                }
                else if (call.Method.Name == "StartsWith" && args.Length == 1)
                {
                    sqlCall.AppendFormat("({0} like ({1} || '%'))", obj.CommandText, args[0].CommandText);
                }
                else if (call.Method.Name == "EndsWith" && args.Length == 1)
                {
                    sqlCall.AppendFormat("({0} like ('%' || {1}))", obj.CommandText, args[0].CommandText);
                }
                else if (call.Method.Name == "Equals" && args.Length == 1)
                {
                    sqlCall.AppendFormat("({0} = ({1}))", obj.CommandText, args[0].CommandText);
                }
                else if (call.Method.Name == "ToLower")
                {
                    sqlCall.AppendFormat("(lower({0}))", obj.CommandText);
                }
                else
                {
                    sqlCall.Append(call.Method.Name.ToLower()).Append("(").Append(String.Join(",", args.Select(a => a.CommandText).ToArray())).Append(")");
                }

                return new CompileResult { CommandText = sqlCall.ToString() };
            }
            
            if (expr.NodeType == ExpressionType.Constant)
            {
                var c = (ConstantExpression) expr;
                queryArgs.Add(c.Value);
                return new CompileResult
                {
                    CommandText = "?",
                    Value = c.Value
                };
            }
            
            if (expr.NodeType == ExpressionType.Convert)
            {
                var u = (UnaryExpression) expr;
                Type ty = u.Type;
                CompileResult valr = CompileExpr(u.Operand, queryArgs);
                return new CompileResult
                {
                    CommandText = valr.CommandText,
                    Value = valr.Value != null ? ConvertTo(valr.Value, ty) : null
                };
            }

            //https://github.com/praeclarum/sqlite-net/issues/158
            if (expr.NodeType == ExpressionType.Not)
            {
                var n = (UnaryExpression)expr;
                var valn = CompileExpr(n.Operand, queryArgs);
                switch (n.Operand.NodeType)
                {
                    case ExpressionType.MemberAccess:
                        valn.CommandText += " = 0";
                        break;
                    case ExpressionType.Call:
                        valn.CommandText = valn.CommandText.Replace(" like ", " not like ");
                        valn.CommandText = valn.CommandText.Replace(" in ", " not in ");
                        valn.CommandText = valn.CommandText.Replace(" = ", " <> ");
                        break;
                }
                return new CompileResult { CommandText = valn.CommandText };
            }

            if (expr.NodeType == ExpressionType.MemberAccess)
            {
                var mem = (MemberExpression) expr;

                if (mem.Expression != null && mem.Expression.NodeType == ExpressionType.Parameter)
                {
                    //
                    // This is a column of our table, output just the column name
                    // Need to translate it if that column name is mapped
                    //
                    string columnName = Table.FindColumnWithPropertyName(mem.Member.Name).Name;
                    return new CompileResult
                    {
                        CommandText = "\"" + columnName + "\""
                    };
                }

                object obj = null;
                if (mem.Expression != null)
                {
                    CompileResult r = CompileExpr(mem.Expression, queryArgs);
                    if (r.Value == null)
                    {
                        throw new NotSupportedException("Member access failed to compile expression");
                    }
                    if (r.CommandText == "?")
                    {
                        queryArgs.RemoveAt(queryArgs.Count - 1);
                    }
                    obj = r.Value;
                }

                //
                // Get the member value
                //
                object val = _sqlitePlatform.ReflectionService.GetMemberValue(obj, expr, mem.Member);

                //
                // Work special magic for enumerables
                //
                if (val is IEnumerable && !(val is string) && !(val is IEnumerable<byte>))
                {
                    var sb = new StringBuilder("(");
                    string head = "";
                    foreach (object a in (IEnumerable) val)
                    {
                        queryArgs.Add(a);
                        sb.Append(head);
                        sb.Append("?");
                        head = ",";
                    }
                    sb.Append(")");

                    return new CompileResult
                    {
                        CommandText = sb.ToString(),
                        Value = val
                    };
                }

                queryArgs.Add(val);
                return new CompileResult
                {
                    CommandText = "?",
                    Value = val
                };
            }

            throw new NotSupportedException("Cannot compile: " + expr.NodeType.ToString());
        }

        /// <summary>
        ///     Compiles a BinaryExpression where one of the parameters is null.
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="parameter">The non-null parameter</param>
        private static string CompileNullBinaryExpression(BinaryExpression expression, CompileResult parameter)
        {
            if (expression.NodeType == ExpressionType.Equal)
            {
                return "(" + parameter.CommandText + " is ?)";
            }
            
            if (expression.NodeType == ExpressionType.NotEqual)
            {
                return "(" + parameter.CommandText + " is not ?)";
            }

            throw new NotSupportedException("Cannot compile Null-BinaryExpression with type " + expression.NodeType.ToString());
        }

        public int Count()
        {
            return GenerateCommand("count(*)").ExecuteScalar<int>();
        }

        public int Count(Expression<Func<T, bool>> predExpr)
        {
            return Where(predExpr).Count();
        }

        public T First()
        {
            TableQuery<T> query = Take(1);
            return query.ToList<T>().First();
        }

        public T FirstOrDefault()
        {
            TableQuery<T> query = Take(1);
            return query.ToList<T>().FirstOrDefault();
        }

    }
}
