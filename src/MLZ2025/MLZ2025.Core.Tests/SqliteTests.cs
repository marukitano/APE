using MLZ2025.Core.Model;
using MLZ2025.Core.Services;

namespace MLZ2025.Core.Tests;

[TestFixture]
public class SqliteTests
{
    [Test]
    public void TestGetAndAddData()
    {
        using (var dataAccess = new DataAccess<DatabaseAddress>())
        {
            dataAccess.DeleteAll();

            var addressCount = dataAccess.Table().Count();

            Assert.That(addressCount, Is.EqualTo(0));

            var max = dataAccess.Insert(new DatabaseAddress
            {
                FirstName = "Max",
                LastName = "Mustermann"
            });

            addressCount = dataAccess.Table().Count();

            Assert.That(addressCount, Is.EqualTo(1));
        }
    }
}
