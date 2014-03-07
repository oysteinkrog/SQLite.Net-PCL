using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#if __ANDROID__
using SQLite.Net.Platform.XamarinAndroid;
#elif __IOS__
using SQLite.Net.Platform.XamarinIOS;
#elif WINDOWS_PHONE
using SQLite.Net.Platform.WindowsPhone8;
using Windows.Storage;
#else
using SQLitePlatform = SQLite.Net.Platform.Win32.SQLitePlatformWin32;
#endif

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
