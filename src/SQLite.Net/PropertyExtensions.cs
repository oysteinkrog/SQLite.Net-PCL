using System;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;

namespace SQLite.Net
{
    internal static class PropertyExtensions
    {
        public interface IAccessor { }

        public interface IGetter : IAccessor
        {
            object Get(object instance);

            object this[object instance] { get; }
        }

        public class Getter<TObject, TMember> : IGetter
        {
            private readonly Func<TObject, TMember> getter;

            public Getter([NotNull] Func<TObject, TMember> getter)
            {
                if (getter == null) throw new ArgumentNullException(nameof(getter));
                this.getter = getter;
            }

            public TMember Get(TObject obj) => this.getter(obj);

            public TMember this[TObject obj] => this.Get(obj);

            #region Implementation of IGetter

            public object Get(object instance) => this.getter((TObject)instance);

            public object this[object instance] => this.Get(instance);

            #endregion

            public static implicit operator Func<TObject, TMember>(Getter<TObject, TMember> self) => self?.getter;
        }

        public interface ISetter : IAccessor
        {
            void Set(object instance, object value);

            object this[object obj] { set; }
        }

        public class Setter<TObject, TMember> : ISetter
        {
            private readonly Action<TObject, TMember> setter;

            public Setter([NotNull] Action<TObject, TMember> setter)
            {
                if (setter == null) throw new ArgumentNullException(nameof(setter));
                this.setter = setter;
            }

            public void Set(TObject obj, TMember value) => this.setter(obj, value);

            public TMember this[TObject obj]
            {
                set { this.Set(obj, value); }
            }

            #region Implementation of ISetter

            public void Set(object instance, object value) => this.setter((TObject)instance, (TMember)value);

            public object this[object obj]
            {
                set { this.Set(obj, value); }
            }

            #endregion

            public static implicit operator Action<TObject, TMember>(Setter<TObject, TMember> self) => self?.setter;
        }

        public static Getter<TObject, TProperty> CompileGetter<TObject, TProperty>([NotNull] this PropertyInfo property)
        {
            if (property == null) throw new ArgumentNullException(nameof(property));
            if (typeof(TObject) != property.DeclaringType) throw new InvalidOperationException();
            if (typeof(TProperty) != property.PropertyType) throw new InvalidOperationException();

            var argObj = Expression.Parameter(typeof(TObject));
            var accessor = Expression.Property(argObj, property);
            return new Getter<TObject, TProperty>(Expression.Lambda<Func<TObject, TProperty>>(accessor, argObj).Compile());
        }

        public static Getter<object, object> CompileGetter([NotNull] this PropertyInfo property)
        {
            if (property == null) throw new ArgumentNullException(nameof(property));

            var obj = Expression.Parameter(typeof(object));
            var accessor = Expression.MakeMemberAccess(Expression.Convert(obj, property.DeclaringType), property);
            return new Getter<object, object>(
                Expression.Lambda<Func<object, object>>(Expression.Convert(accessor, typeof(object)), obj
                    ).Compile());
        }

        public static Setter<TObject, TProperty> CompileSetter<TObject, TProperty>([NotNull] this PropertyInfo property)
        {
            if (property == null) throw new ArgumentNullException(nameof(property));
            if (typeof(TObject) != property.DeclaringType) throw new InvalidOperationException();
            if (typeof(TProperty) != property.PropertyType) throw new InvalidOperationException();

            var argObj = Expression.Parameter(typeof(TObject));
            var argInput = Expression.Parameter(typeof(TProperty));
            var accessor = Expression.Property(argObj, property);
            var assign = Expression.Assign(accessor, argInput);
            return new Setter<TObject, TProperty>(Expression.Lambda<Action<TObject, TProperty>>(assign, argObj, argInput).Compile());
        }

        public static Setter<object, object> CompileSetter([NotNull] this PropertyInfo property)
        {
            if (property == null) throw new ArgumentNullException(nameof(property));

            var obj = Expression.Parameter(typeof(object));
            var value = Expression.Parameter(typeof(object));
            var accessor = Expression.Property(Expression.Convert(obj, property.DeclaringType), property);
            var assign = Expression.Assign(accessor, Expression.Convert(value, property.PropertyType));
            return new Setter<object, object>(Expression.Lambda<Action<object, object>>(assign, obj, value).Compile());
        }
    }
}