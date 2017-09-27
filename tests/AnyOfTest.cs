using System.Linq.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SQLite.Net.Attributes;

#if __WIN32__
using SQLitePlatformTest = SQLite.Net.Platform.Win32.SQLitePlatformWin32;
#elif WINDOWS_PHONE
using SQLitePlatformTest = SQLite.Net.Platform.WindowsPhone8.SQLitePlatformWP8;
#elif __WINRT__
using SQLitePlatformTest = SQLite.Net.Platform.WinRT.SQLitePlatformWinRT;
#elif __IOS__
using SQLitePlatformTest = SQLite.Net.Platform.XamarinIOS.SQLitePlatformIOS;
#elif __ANDROID__
using SQLitePlatformTest = SQLite.Net.Platform.XamarinAndroid.SQLitePlatformAndroid;
#else
using SQLitePlatformTest = SQLite.Net.Platform.Generic.SQLitePlatformGeneric;
#endif


namespace SQLite.Net.Tests
{
    [TestFixture]
    internal class AnyOfTest
    {
        public class TestObj
        {
            [AutoIncrement, PrimaryKey]
            public int Id { get; set; }

            public string ColumnA { get; set; }
            public string ColumnB { get; set; }

        }

        public class TestDb : SQLiteConnection
        {
            public TestDb(String path)
                : base(new SQLitePlatformTest(), path)
            {
                CreateTable<TestObj>();

                Insert(new TestObj { ColumnA = "Foo", ColumnB = "Bar" });
                Insert(new TestObj { ColumnA = "Bar", ColumnB = "Baz" });
                Insert(new TestObj { ColumnA = "Baz", ColumnB = "Qux" });
            }
        }

        [Test]
        public void ParamsList()
        {
            var db = new TestDb(TestPath.GetTempFileName());

            TableQuery<TestObj> results = db.Table<TestObj>().AnyOf(o => o.ColumnA == "Foo", o => o.ColumnB == "Qux");
            List<TestObj> resultsList = results.OrderBy(o => o.Id).ToList();

            Assert.AreEqual(2, resultsList.Count);
            Assert.AreEqual("Bar", resultsList[0].ColumnB);
            Assert.AreEqual("Baz", resultsList[1].ColumnA);
        }

        [Test]
        public void Array()
        {
            var db = new TestDb(TestPath.GetTempFileName());

            List<Expression<Func<TestObj, bool>>> preds = new List<Expression<Func<TestObj, bool>>>();
            preds.Add(o => o.ColumnA == "Foo");
            preds.Add(o => o.ColumnB == "Qux");

            TableQuery<TestObj> results = db.Table<TestObj>().AnyOf(preds.ToArray());
            List<TestObj> resultsList = results.OrderBy(o => o.Id).ToList();

            Assert.AreEqual(2, resultsList.Count);
            Assert.AreEqual("Bar", resultsList[0].ColumnB);
            Assert.AreEqual("Baz", resultsList[1].ColumnA);
        }
    }
}