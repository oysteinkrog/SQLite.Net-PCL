using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SQLite.Net.Attributes;

namespace SQLite.Net.Tests
{
    [TestFixture]
    public class DeleteTest
    {
        private class TestTable
        {
            [PrimaryKey, AutoIncrement]
            public int Id { get; set; }

            public int Datum { get; set; }
            public string Test { get; set; }
        }

        private class TestTableCompositeKey
        {
            [PrimaryKey]
            public int Id { get; set; }

            [PrimaryKey]
            public int TestIndex { get; set; }

            public int Datum { get; set; }
            public string Test { get; set; }
        }

        private const int Count = 100;

        private SQLiteConnection CreateDb()
        {
            var db = new TestDb();

            db.CreateTable<TestTable>();
            db.CreateTable<TestTableCompositeKey>();

            var items =
                from i in Enumerable.Range(0, Count)
                select new TestTable
                {
                    Datum = 1000 + i,
                    Test = "Hello World"
                }
                ;
            db.InsertAll(items);

            var itemsCompositeKey =
                from i in Enumerable.Range(0, Count)
                select new TestTableCompositeKey
                {
                    Datum = 1000 + i,
                    Test = "Hello World",
                    Id = i,
                    TestIndex = i + 1
                }
                ;
            db.InsertAll(itemsCompositeKey);
            Assert.AreEqual(Count, db.Table<TestTableCompositeKey>().Count());

            return db;
        }

        [Test]
        public void DeleteAll()
        {
            var db = CreateDb();

            var r = db.DeleteAll<TestTable>();

            Assert.AreEqual(Count, r);
            Assert.AreEqual(0, db.Table<TestTable>().Count());
        }

        [Test]
        public void DeleteAllWithPredicate()
        {
            var db = CreateDb();

            var r = db.Table<TestTable>().Delete(p => p.Test == "Hello World");

            Assert.AreEqual(Count, r);
            Assert.AreEqual(0, db.Table<TestTable>().Count());
        }

        [Test]
        public void DeleteAllWithPredicateHalf()
        {
            var db = CreateDb();
            db.Insert(new TestTable
            {
                Datum = 1,
                Test = "Hello World 2"
            });

            var r = db.Table<TestTable>().Delete(p => p.Test == "Hello World");

            Assert.AreEqual(Count, r);
            Assert.AreEqual(1, db.Table<TestTable>().Count());
        }

        [Test]
        public void DeleteWithWhereAndPredicate()
        {
            var db = CreateDb();
            var testString = "TestData";
            var first = db.Insert(new TestTable
            {
                Datum = 3,
                Test = testString

            });
            var second = db.Insert(new TestTable
            {
                Datum = 4,
                Test = testString
            });

            //Should only delete first
            var r = db.Table<TestTable>().Where(t => t.Datum == 3).Delete(t => t.Test == testString);

            Assert.AreEqual(1, r);
            Assert.AreEqual(Count + 1, db.Table<TestTable>().Count());
        }

        [Test]
        public void DeleteEntityOne()
        {
            var db = CreateDb();

            var r = db.Delete(db.Get<TestTable>(1));

            Assert.AreEqual(1, r);
            Assert.AreEqual(Count - 1, db.Table<TestTable>().Count());
        }

        [Test]
        public void DeletePKNone()
        {
            var db = CreateDb();

            var r = db.Delete<TestTable>(348597);

            Assert.AreEqual(0, r);
            Assert.AreEqual(Count, db.Table<TestTable>().Count());
        }

        [Test]
        public void DeletePKOne()
        {
            var db = CreateDb();

            var r = db.Delete<TestTable>(1);

            Assert.AreEqual(1, r);
            Assert.AreEqual(Count - 1, db.Table<TestTable>().Count());
        }

        [Test]
        public void DeletePKNoneComposite()
        {
            var db = CreateDb();

            var compositePK = new Dictionary<string, object>();
            compositePK.Add("Id", 348597);
            compositePK.Add("TestIndex", 348598);

            var r = db.Delete<TestTableCompositeKey>(compositePK);

            Assert.AreEqual(0, r);
            Assert.AreEqual(Count, db.Table<TestTableCompositeKey>().Count());
        }

        [Test]
        public void DeletePKOneComposite()
        {
            var db = CreateDb();

            var compositePK = new Dictionary<string, object>();
            compositePK.Add("Id", 1);
            compositePK.Add("TestIndex", 2);

            var r = db.Delete<TestTableCompositeKey>(compositePK);

            Assert.AreEqual(1, r);
            Assert.AreEqual(Count - 1, db.Table<TestTableCompositeKey>().Count());
        }
    }
}