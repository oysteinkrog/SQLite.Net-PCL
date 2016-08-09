using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using SQLite.Net.Async;
using SQLite.Net.Attributes;

namespace SQLite.Net.Tests
{


    /// <summary>
    ///     Defines tests that exercise async behaviour.
    /// </summary>
    [TestFixture]
    public class OrderByTest
    {
        public class TestObj
        {
            [AutoIncrement, PrimaryKey]
            public int Id { get; set;  }

            public override string ToString()
            {
                return string.Format("[TestObj: Id={0}]", Id);
            }
            
            public override bool Equals(Object obj)
            {
                return Id == (obj as TestObj)?.Id;
            }

            protected bool Equals(TestObj other)
            {
                return Id == other.Id;
            }

            public override int GetHashCode()
            {
                return Id;
            }
        }

        public class TestDb : SQLiteConnection
        {
            public TestDb(String path)
                : base(new SQLitePlatformTest(), path)
            {
                CreateTable<TestObj>();
            }
        }

        [Test]
        public void OrderByWorks()
        {
            using (var db = new TestDb(TestPath.CreateTemporaryDatabase()))
            {
                TestObj testObj = new TestObj();
                TestObj[] testObjects = new TestObj[100];
                
                for (int i = 0; i < testObjects.Length; i++)
                    testObjects[i] = testObj;

                db.InsertAll(testObjects);

                try
                {
                    CollectionAssert.AreEqual(
                        db.Table<TestObj>().OrderBy(k => k.Id),
                        db.Table<TestObj>().OrderBy(k => k.Id));
                    CollectionAssert.AreNotEqual(
                        db.Table<TestObj>().OrderBy(k => k.Id),
                        db.Table<TestObj>().OrderByDescending(k => k.Id));
                    CollectionAssert.AreNotEqual(
                        db.Table<TestObj>().OrderByRand(),
                        db.Table<TestObj>().OrderByRand());
                }
                catch (NotImplementedException)
                {
                    //Allow Not implemented exceptions as the selection may be too complex.
                }
            }
           
        }
    }
}