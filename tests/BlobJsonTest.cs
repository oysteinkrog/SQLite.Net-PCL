using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#if WINDOWS_PHONE
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using TestFixture = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestClassAttribute;
using Test = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestMethodAttribute;
#else
using NUnit.Framework;
using SQLite.Net.Interop;
using SQLite.Net.Attributes;
#endif

namespace SQLite.Net.Tests
{
    [TestFixture]
    public class BlobJsonTest : BlobSerializationTest
    {
        protected override IBlobSerializer Serializer
        {
            get { return new JsonSerializer(); }
        }
    }
}
