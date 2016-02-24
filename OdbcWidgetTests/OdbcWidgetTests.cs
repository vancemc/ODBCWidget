using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Win32;

namespace OdbcWidgetTests
{
    using DataTools;

    [TestClass]
    public class OdbcWidgetTests
    {
        [TestMethod]
        public void GetOdbcConnectionsCurrentUser()
        {
            var result = OdbcWidget.ListDataSourceNames(Registry.CurrentUser);

            Assert.IsTrue(result.ToArray().Length > 5);
        }

        [TestMethod]
        public void GetOdbcConnectionsLocalMachine()
        {
            var result = OdbcWidget.ListDataSourceNames(Registry.LocalMachine);

            Assert.IsTrue(result.ToArray().Length > 0);
        }

        [TestMethod]
        public void GetOdbcConnectionsAll()
        {
            var result = OdbcWidget.ListDataSourceNames();

            Assert.IsTrue(result.ToArray().Length > 6);
        }

        [TestMethod]
        public void CreateConnectionStrings()
        {
            var results = OdbcWidget.ListDataSourceNames();

            foreach (var dsn in results)
            {
                var result = OdbcWidget.CreateConnectionString(dsn, "Satch", "Guitar+1", "DataCompression=True");

                Assert.AreEqual(result,
                    string.Format("DSN={0};uid=Satch;pwd=Guitar+1;DataCompression=True", dsn));
            }

            foreach (var dsn in results)
            {
                var result = OdbcWidget.CreateConnectionString(dsn, "Satch", "Guitar+1");

                Assert.AreEqual(result,
                    string.Format("DSN={0};uid=Satch;pwd=Guitar+1;", dsn));
            }

            foreach (var dsn in results)
            {
                var result = OdbcWidget.CreateConnectionString(dsn);

                Assert.AreEqual(result,
                    string.Format("DSN={0};", dsn));
            }
        }

        [TestMethod]
        public void TestConnection()
        {

            var validConnStr = OdbcWidget.CreateConnectionString("DPI_Test", "S_RSETATST", "y3nsrrhb1", "DataCompression=True");

            bool result = OdbcWidget.TestConnection(validConnStr);

            Assert.IsTrue(result);

            validConnStr = OdbcWidget.CreateConnectionString("Triad");

            result = OdbcWidget.TestConnection(validConnStr);

            Assert.IsTrue(result);

            var invalidConnStr = OdbcWidget.CreateConnectionString("bogusDsn", "foo", "bar");

            result = OdbcWidget.TestConnection(invalidConnStr, 2);

            Assert.IsFalse(result);
        }
    }
}
