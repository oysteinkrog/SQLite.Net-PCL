using System.Collections;
using System.Collections.Generic;

namespace SQLite.Net
{
	public class ReadOnlyQueryCache : Dictionary<string, string>
	{
	    public static void CheckIfCacheNeedsClearing(SQLiteConnection connection, string commandString)
	    {
	        if (commandString.StartsWith("insert") || commandString.StartsWith("update") || commandString.StartsWith("delete") ||
	            commandString.StartsWith("create") || commandString.StartsWith("alter") || commandString.StartsWith("drop"))
	        {
	            connection.ReadOnlyCache.Clear();
	        }
	    }
	}
}