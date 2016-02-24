using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Odbc;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace DataTools
{
    public class OdbcWidget
    {
        public const string RegObdcSubKey = @"Software\ODBC\ODBC.INI\ODBC Data Sources";

        public static string[] ListDataSourceNames()
        {
            List<string> resultsLocalMachine = ListDataSourceNames(Registry.LocalMachine).ToList();

            List<string> resultsCurrentUser = ListDataSourceNames(Registry.CurrentUser).ToList();

            List<string> combinedResults = new List<string>();

            combinedResults.AddRange(resultsLocalMachine);

            combinedResults.AddRange(resultsCurrentUser);

            return combinedResults.ToArray();
        }

        public static string[] ListDataSourceNames(RegistryKey rootKey)
        {
            RegistryKey regKey = rootKey.OpenSubKey(RegObdcSubKey);

            string[] retVal = null;

            if (regKey != null)
            {
                retVal = regKey.GetValueNames();
            }

            return retVal;
        }

        public static string CreateConnectionString(string odbcDsn, string uid, string pwd, string additionalParameters="")
        {
            const string odbcConnectionTemplate =
                "uid={0};pwd={1};{2}";

            string retVal = CreateConnectionString(odbcDsn, string.Format(odbcConnectionTemplate, uid, pwd, additionalParameters));

            return retVal;
        }

        public static string CreateConnectionString(string odbcDsn, string additionalParameters="")
        {
            const string odbcConnectionTemplate =
                "DSN={0};{1}";

            string retVal = string.Format(odbcConnectionTemplate, odbcDsn, additionalParameters);

            return retVal;
        }

        public static OdbcConnection CreateObdcConnection(string odbcDsn)
        {
            string connStr = CreateConnectionString(odbcDsn);

            OdbcConnection connection = new OdbcConnection(connStr);

            connection.Open();

            return connection;
        }

        private static bool InternalTestConnection(string connectionStr)
        {
            bool testConnection;

            using (OdbcConnection connection = new OdbcConnection(connectionStr))
            {
                connection.Open();

                testConnection = (connection.State == ConnectionState.Open);
            }

            return testConnection;
        }

        //private static bool InternalTestConnection(OdbcConnection connection)
        //{
        //    bool testConnection = false;

        //    if (connection != null)
        //    {
        //        connection.Open();

        //        testConnection = (connection.State == ConnectionState.Open);
        //    }

        //    return testConnection;
        //}

        public static bool TestConnection(string connectionStr, int timeoutInMilliseconds = 10000)
        {
            Func<string, bool> testConnectionDelegate = InternalTestConnection;

            var testConnection = ImplicitWaitUntilTimeout(testConnectionDelegate, connectionStr, timeoutInMilliseconds);

            return testConnection;
        }

        //public static bool TestConnection(OdbcConnection connection, int timeoutInMilliseconds = 10000)
        //{
        //    Func<string, bool> testConnectionDelegate = InternalTestConnection;

        //    var testConnection = ImplicitWaitUntilTimeout(testConnectionDelegate, connection, timeoutInMilliseconds);

        //    return testConnection;
        //}

        public object ExecuteScalar(string queryString, string dpiInstanceName = "DPI_TEST")
        {
            var command = new OdbcCommand(queryString);

            var odbcConn = CreateObdcConnection(dpiInstanceName);

            command.Connection = odbcConn;

            var result = command.ExecuteScalar();

            return result;
        }

        public OdbcDataReader ExecuteReader(string queryString, string dsn)
        {
            var command = new OdbcCommand(queryString);

            var odbcConn = CreateObdcConnection(dsn);
       
            command.Connection = odbcConn;

            var result = command.ExecuteReader();

            return result;
        }

        public int ExecuteNonQuery(string sqlStatement, string dsn)
        {
            var command = new OdbcCommand(sqlStatement);

            var odbcConn = CreateObdcConnection(dsn);

            command.Connection = odbcConn;

            var result = command.ExecuteNonQuery();

            return result;
        }

        private static bool ImplicitWaitUntilTimeout<TParamType>(Func<TParamType, bool> methodDelegate, TParamType param, int timeOutInMilliseconds)
        {
            bool result = false;
            var timer = new Stopwatch();

            timer.Start();

            do
            {
                try
                {
                    result = methodDelegate(param);

                }
                catch (Exception)
                {
                    Thread.Sleep(100);
                }

            } while (result == false && (timer.ElapsedMilliseconds < timeOutInMilliseconds));

            return result;
        }
    }
}
