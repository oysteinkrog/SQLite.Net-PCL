using System;

namespace SQLite.Net.Interop
{
    internal static class ColTypeExtensions
    {
        /// <summary>
        /// Get a typeof() element from ColType enumeration.
        /// </summary>
        /// <param name="colType"></param>
        /// <returns></returns>
        internal static Type ToType(this ColType colType)
        {
            // Prepare evolution where column can be nullable
            var nullable = false;

            switch(colType)
            {
                case ColType.Blob:
                    return typeof(byte[]);
                case ColType.Float:
                    return nullable ? typeof(double?) : typeof(double);
                case ColType.Integer:
                    return nullable ? typeof(int?) : typeof(int);
                default:
                    return typeof(string);
            }
        }
    }
}
