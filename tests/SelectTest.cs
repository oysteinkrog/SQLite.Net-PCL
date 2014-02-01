using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using SQLite.Net.Attributes;
using SQLite.Net.Platform.Win32;

namespace SQLite.Net.Tests
{
    internal class SelectTest
    {
        public class TestObj
        {
            [AutoIncrement, PrimaryKey]
            public int Id { get; set; }

            public int Order { get; set; }

            public string Content { get; set; }

            public override string ToString()
            {
                return string.Format("[TestObj: Id={0}, Order={1}]", Id, Order);
            }
        }

        public class TestDb : SQLiteConnection
        {
            public TestDb(String path)
                : base(new SQLitePlatformWin32(), path)
            {
                CreateTable<TestObj>();
            }
        }

        [Test]
        public void SelectString()
        {
            var db = new SkipTest.TestDb(TestPath.GetTempFileName());

            var t = new SkipTest.TestObj()
            {
                Content = "toto",
                Id = 0
            };
            db.Insert(t);

            var results = db.Table<SkipTest.TestObj>().Select(cp => cp.Content).ToList();
            Assert.IsNotNull(results);
            Assert.AreEqual("toto",results[0]);
        }
    }
}
