
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
    public abstract class BlobSerializationTest
    {   
        protected abstract IBlobSerializer Serializer { get; }

        public class BlobDatabase : SQLiteConnection
        {
            public BlobDatabase(IBlobSerializer serializer) :
                base(new SQLitePlatform(), TestPath.GetTempFileName(), false, serializer)
            {
                DropTable<ComplexOrder>();
            }
        }

        public class ComplexOrder : IEquatable<ComplexOrder>
        {
            public ComplexOrder()
            {
                PlacedTime = DateTime.UtcNow;
                History = new List<OrderHistory>();
                Lines = new List<OrderLine>();
            }

            [AutoIncrement, PrimaryKey]
            public int Id { get; set; }

            public DateTime PlacedTime { get; set; }

            public List<OrderHistory> History { get; set; }

            public List<OrderLine> Lines { get; set; }

            public override bool Equals(object obj)
            {
                return this.Equals(obj as ComplexOrder);
            }

            public override int GetHashCode()
            {
                return this.Id.GetHashCode() ^
                    this.PlacedTime.GetHashCode() ^
                    this.History.GetHashCode() ^
                    this.Lines.GetHashCode();
            }

            public bool Equals(ComplexOrder other)
            {
                if (other == null)
                {
                    return false;
                }

                return this.Id.Equals(other.Id) &&
                    this.PlacedTime.Equals(other.PlacedTime) &&
                    this.History.SequenceEqual(other.History) &&
                    this.Lines.SequenceEqual(other.Lines);
            }
        }

        [Test]
        public void VerifyTableCreationFailsWithNoSerializer()
        {
            NotSupportedException ex = null;
            using (var db = new BlobDatabase(null))
            {
                try
                {
                    var count = db.CreateTable<ComplexOrder>();
                    Assert.IsTrue(count == 0);
                    Assert.IsNull(db.GetMapping<ComplexOrder>());
                    return;
                }
                catch (NotSupportedException e)
                {
                    ex = e;
                }
            }

            Assert.IsNotNull(ex);
        }

        [Test]
        public void VerifyTableCreationSucceedsWithSerializer()
        {
            var canDeserialize = this.Serializer.CanDeserialize(typeof(ComplexOrder));

            NotSupportedException ex = null;
            using (var db = new BlobDatabase(this.Serializer))
            {
                try
                {
                    var count = db.CreateTable<ComplexOrder>();
                    if (canDeserialize)
                    {
                        //Assert.AreEqual(count, 1);
                        var mapping = db.GetMapping<ComplexOrder>();
                        Assert.IsNotNull(mapping);
                        Assert.AreEqual(4, mapping.Columns.Length);
                    }
                    //else
                    //{
                    //    Assert.AreEqual(count, 0);
                    //    return;
                    //}
                }
                catch (NotSupportedException e)
                {
                    ex = e;
                }
            }
            
            Assert.AreEqual(canDeserialize, ex == null);
        }

        [Test]
        public void TestListOfObjects()
        {
            using (var db = new BlobDatabase(this.Serializer))
            {
                db.CreateTable<ComplexOrder>();
                var order = new ComplexOrder();

                var count = db.Insert(order);
                Assert.AreEqual(count, 1);
                var orderCopy = db.Find<ComplexOrder>(order.Id);
                Assert.AreEqual(order, orderCopy);

                for (var n = 0; n < 1000; )
                {
                    order.Lines.Add(new OrderLine() { ProductId = 1, Quantity = ++n, Status = OrderLineStatus.Placed, UnitPrice = (n / 10m) });
                    orderCopy = db.Find<ComplexOrder>(order.Id);
                    Assert.AreEqual(order, orderCopy);
                    order.History.Add(new OrderHistory() { Time = DateTime.UtcNow, Comment = string.Format("New history {0}", n) });
                    orderCopy = db.Find<ComplexOrder>(order.Id);
                    Assert.AreEqual(order, orderCopy);
                }
            }
        }
    }
}
