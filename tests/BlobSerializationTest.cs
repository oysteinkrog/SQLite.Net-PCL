
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
                History = new List<ComplexHistory>();
                Lines = new List<ComplexLine>();
            }

            [AutoIncrement, PrimaryKey]
            public int Id { get; set; }

            public DateTime PlacedTime { get; set; }

            public List<ComplexHistory> History { get; set; }

            public List<ComplexLine> Lines { get; set; }

            public override bool Equals(object obj)
            {
                return this.Equals(obj as ComplexOrder);
            }

            public override int GetHashCode()
            {
                return this.Id.GetHashCode() ^
                    //this.PlacedTime.GetHashCode() ^
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
                    Math.Abs((this.PlacedTime - other.PlacedTime).TotalMilliseconds) < 100 &&
                    this.History.SequenceEqual(other.History) &&
                    this.Lines.SequenceEqual(other.Lines);
            }
        }

        public class ComplexHistory : IEquatable<ComplexHistory>
        {
            public DateTime Time { get; set; }
            public string Comment { get; set; }

            public override int GetHashCode()
            {
                return this.Comment.GetHashCode();
            }

            public override bool Equals(object obj)
            {
                return this.Equals(obj as ComplexHistory);
            }

            public bool Equals(ComplexHistory other)
            {
                if (other == null)
                {
                    return false;
                }

                return this.Comment.Equals(other.Comment);
            }
        }

        public class ComplexLine : IEquatable<ComplexLine>
        {
            [Indexed("IX_OrderProduct", 2)]
            public int ProductId { get; set; }

            public int Quantity { get; set; }
            public decimal UnitPrice { get; set; }
            public OrderLineStatus Status { get; set; }

            public override bool Equals(object obj)
            {
                return this.Equals(obj as ComplexLine);
            }

            public override int GetHashCode()
            {
                return
                    this.ProductId.GetHashCode() ^
                    this.Quantity.GetHashCode() ^
                    this.Status.GetHashCode() ^
                    this.UnitPrice.GetHashCode();
            }

            public bool Equals(ComplexLine other)
            {
                if (other == null)
                {
                    return false;
                }

                return
                    this.ProductId.Equals(other.ProductId) &&
                    this.Quantity.Equals(other.Quantity) &&
                    this.Status.Equals(other.Status) &&
                    this.UnitPrice.Equals(other.UnitPrice);
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

                for (var n = 0; n < 10; )
                {
                    order.Lines.Add(new ComplexLine() { ProductId = 1, Quantity = ++n, Status = OrderLineStatus.Placed, UnitPrice = (n / 10m) });
                    db.Update(order);
                    orderCopy = db.Find<ComplexOrder>(order.Id);
                    Assert.AreEqual(order, orderCopy);
                    order.History.Add(new ComplexHistory() { Time = DateTime.UtcNow, Comment = string.Format("New history {0}", n) });
                    db.Update(order);
                    orderCopy = db.Find<ComplexOrder>(order.Id);
                    Assert.AreEqual(order, orderCopy);
                }
            }
        }
    }
}
