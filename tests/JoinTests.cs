using NUnit.Framework;
using SQLite.Net.Attributes;
using SQLite.Net.Interop;
using System.Linq;

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
    public class JoinTests
    {
        public class Employee
        {
            [PrimaryKey]
            public int Id { get; set; }

            public string Name { get; set; }

            public int Age { get; set; }

            public string Address { get; set; }
        }

        public class Department
        {
            [PrimaryKey]
            public int Id { get; set; }

            public string Name { get; set; }

            public int EmployeeId { get; set; }
        }

        public class EmployeeDepartment
        {
            public Employee Employee { get; set; }

            public Department Department { get; set; }
        }


        public class TestDb : SQLiteConnection
        {
            public TestDb(ISQLitePlatform sqlitePlatform)
                : base(sqlitePlatform, TestPath.GetTempFileName())
            {
                CreateTable<Employee>();
                CreateTable<Department>();
                TraceListener = DebugTraceListener.Instance;
            }
        }

        private TestDb _testDb;

        [TestFixtureSetUp]
        public void CreateDatabase()
        {
            _testDb = new TestDb(new SQLitePlatformTest());

            _testDb.Insert(new Employee { Id = 1, Name = "Paul", Age = 32, Address = "California" });
            _testDb.Insert(new Employee { Id = 2, Name = "Allen", Age = 25, Address = "Texas" });
            _testDb.Insert(new Employee { Id = 3, Name = "Teddy", Age = 23, Address = "Norway" });
            _testDb.Insert(new Employee { Id = 4, Name = "Mark", Age = 25, Address = "Rich-Mond" });
            _testDb.Insert(new Employee { Id = 5, Name = "David", Age = 27, Address = "Texas" });
            _testDb.Insert(new Employee { Id = 6, Name = "Kim", Age = 22, Address = "South-Hall" });
            _testDb.Insert(new Employee { Id = 7, Name = "James", Age = 24, Address = "Houston" });

            _testDb.Insert(new Department { Id = 1, Name = "IT Billing", EmployeeId = 1 });
            _testDb.Insert(new Department { Id = 2, Name = "Engineerin", EmployeeId = 2 });
            _testDb.Insert(new Department { Id = 3, Name = "Finance", EmployeeId = 7 });
        }

        [TestFixtureTearDown]
        public void DropDatabase()
        {
            _testDb.Dispose();
        }

        [Test]
        public void InnerJoin()
        {
            var query = _testDb.Table<Employee>().Join<Department, int>(
                            e => e.Id, 
                            d => d.EmployeeId);
            var result = query.ToList();
            Assert.AreEqual(3, result.Count);

            AssertEmployee(result[0].Outer, 1, "Paul", 32, "California");
            AssertDepartment(result[0].Inner, 1, "IT Billing", 1);
            AssertEmployee(result[1].Outer, 2, "Allen", 25, "Texas");
            AssertDepartment(result[1].Inner, 2, "Engineerin", 2);
            AssertEmployee(result[2].Outer, 7, "James", 24, "Houston");
            AssertDepartment(result[2].Inner, 3, "Finance", 7);
        }

        private static void AssertEmployee(Employee employee, int id, string name, int age, string address)
        {
            Assert.AreEqual(id, employee.Id, "Id mismatch");
            Assert.AreEqual(name, employee.Name, "Name mismatch");
            Assert.AreEqual(age, employee.Age, "Age mismatch");
            Assert.AreEqual(address, employee.Address, "Address mismatch");
        }

        private static void AssertDepartment(Department department, int id, string name, int employeeId)
        {
            Assert.AreEqual(id, department.Id, "Id mismatch");
            Assert.AreEqual(name, department.Name, "Name mismatch");
            Assert.AreEqual(employeeId, department.EmployeeId, "EmployeeId mismatch");            
        }
    }
}