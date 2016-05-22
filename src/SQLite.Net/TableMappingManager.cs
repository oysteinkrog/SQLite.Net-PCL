using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using SQLite.Net.Interop;

namespace SQLite.Net
{
    public class TableMappingManager : ITableMappingManager
    {
        private readonly IDictionary<string, TableMapping> _tableMappings;
        private readonly object _tableMappingsLocks = new object();

        public TableMappingManager(
            [NotNull] ISQLitePlatform sqlitePlatform,
            [CanBeNull] IDictionary<string, TableMapping> tableMappings = null,
            [CanBeNull] IDictionary<Type, string> extraTypeMappings = null)
        {
            this.Platform = sqlitePlatform;
            this.ExtraTypeMappings = extraTypeMappings ?? new Dictionary<Type, string>();
            this._tableMappings = tableMappings ?? new Dictionary<string, TableMapping>();
        }

        /// <summary>
        ///     Retrieves the mapping that is automatically generated for the given type.
        /// </summary>
        /// <param name="type">
        ///     The type whose mapping to the database is returned.
        /// </param>
        /// <param name="createFlags">
        ///     Optional flags allowing implicit PK and indexes based on naming conventions
        /// </param>
        /// <returns>
        ///     The mapping represents the schema of the columns of the database and contains
        ///     methods to set and get properties of objects.
        /// </returns>
        [PublicAPI]
        public TableMapping GetMapping(Type type, CreateFlags createFlags = CreateFlags.None)
        {
            lock (this._tableMappingsLocks)
            {
                TableMapping map;
                return this._tableMappings.TryGetValue(type.FullName, out map)
                    ? map
                    : this.CreateAndSetMapping(type, createFlags, this._tableMappings);
            }
        }

        [NotNull, PublicAPI]
        public ISQLitePlatform Platform { get; private set; }

        private TableMapping CreateAndSetMapping(Type type, CreateFlags createFlags,
            IDictionary<string, TableMapping> mapTable)
        {
            var props = this.Platform.ReflectionService.GetPublicInstanceProperties(type);
            var map = new TableMapping(type, props, createFlags);
            mapTable[type.FullName] = map;
            return map;
        }

        /// <summary>
        ///     Retrieves the mapping that is automatically generated for the given type.
        /// </summary>
        /// <returns>
        ///     The mapping represents the schema of the columns of the database and contains
        ///     methods to set and get properties of objects.
        /// </returns>
        [PublicAPI]
        public TableMapping GetMapping<T>()
        {
            return this.GetMapping(typeof(T));
        }

        /// <summary>
        ///     Returns the mappings from types to tables that the connection
        ///     currently understands.
        /// </summary>
        [PublicAPI]
        [NotNull]
        public IEnumerable<TableMapping> TableMappings
        {
            get
            {
                lock (this._tableMappingsLocks)
                {
                    return this._tableMappings.Values.ToList();
                }
            }
        }

        public IDictionary<Type, string> ExtraTypeMappings { get; private set; }
    }
}