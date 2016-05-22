using System;
using System.Collections.Generic;
using SQLite.Net.Interop;

namespace SQLite.Net
{
    public interface ITableMappingManager
    {
        IDictionary<Type, string> ExtraTypeMappings { get; }

        IEnumerable<TableMapping> TableMappings { get; }

        TableMapping GetMapping(Type type, CreateFlags createFlags = CreateFlags.None);
    }
}