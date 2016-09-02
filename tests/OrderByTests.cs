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
                    AssertCollectionContent(
                        db.Table<TestObj>().OrderBy(k => k.Id),
                        db.Table<TestObj>().OrderBy(k => k.Id));
                    AssertCollectionContent(
                        db.Table<TestObj>().OrderBy(k => k.Id),
                        db.Table<TestObj>().OrderByDescending(k => k.Id),
                        true);
                    AssertCollectionContent(
                        db.Table<TestObj>().OrderByRand(),
                        db.Table<TestObj>().OrderByRand(),
                        true);
                }
                catch (NotImplementedException)
                {
                    //Allow Not implemented exceptions as the selection may be too complex.
                }
            }
        }

        private void AssertCollectionContent<T>(
            IEnumerable<T> col1, IEnumerable<T> col2, bool negate = false)
        {
            Assert.AreEqual(col1.Count(), col2.Count());

            var enumerator1 = col1.GetEnumerator();
            var enumerator2 = col2.GetEnumerator();

            while (enumerator1.MoveNext() && enumerator2.MoveNext())
            {
                T item1 = enumerator1.Current;
                T item2 = enumerator2.Current;

                if (negate)
                {
                    Assert.AreNotEqual(item1, item2);

                    // Only one comparison suffice to assert condition true
                    break;
                }
                
                Assert.AreEqual(item1, item2);
            }
        }
    }
}