using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;

using Npgsql;

using IpcPocV2.Common.Models;

namespace IpcPocV2.Data.Repositories
{
    public abstract class BaseRepository<T> where T : BaseEntity
    {
        public string BaseConnectionString { get; set; }
        public string ConnectionString { get; set; }
        public string TableName { get; set; }
        public List<string> Columns { get; set; }

        protected BaseRepository()
        {
            var dbHostName = ConfigurationManager.AppSettings["DbHostName"];

            var cnnStrPattern = "Server={0};Port=5432;Database={1};User Id=postgres;Password=ipc*pass;";
            BaseConnectionString = string.Format(cnnStrPattern, dbHostName, "postgres");
            ConnectionString = string.Format(cnnStrPattern, dbHostName, "ipc");

            Columns = new List<string>();
        }

        public NpgsqlConnection GetBaseConnection()
        {
            var connection = new NpgsqlConnection(BaseConnectionString);
            return connection;
        }

        public NpgsqlConnection GetConnection()
        {
            var connection = new NpgsqlConnection(ConnectionString);
            return connection;
        }


        public bool TableExists(string tableName)
        {
            var bExists = true;
            using (var conn = GetConnection())
            {
                using (var cmd = GetCommand(conn))
                {
                    cmd.CommandText = string.Format("SELECT EXISTS ( SELECT 1 FROM information_schema.tables  WHERE  table_schema = 'public' AND table_name = '{0}' );", tableName);

                    conn.Open();
                    bExists = (bool)cmd.ExecuteScalar();
                    conn.Close();
                }
            }

            return bExists;
        }

        public void CreateDatabaseIfNotExists()
        {
            try
            {
                using (var cnn = GetBaseConnection())
                {
                    using (var cmd = GetCommand(cnn))
                    {
                        cmd.CommandText = "CREATE DATABASE ipc;";
                        cnn.Open();
                        cmd.ExecuteNonQuery();
                        cnn.Close();
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.WriteLine("Can not connect to postgres server or db already created!");
            }
        }

        public string ReadContentInAssembly(string fileName)
        {
            string data;
            var assembly = Assembly.GetExecutingAssembly();

            var resourceName = assembly.GetManifestResourceNames().Single(str => str.EndsWith(fileName));

            using (var stream = assembly.GetManifestResourceStream(resourceName))
            using (var reader = new StreamReader(stream))
            {
                data = reader.ReadToEnd();
            }

            return data;
        }

        public abstract void CreateTable();

        public NpgsqlCommand GetCommand(NpgsqlConnection connection)
        {
            var cmd = new NpgsqlCommand { Connection = connection };
            return cmd;
        }

        public static void AddParameterValue(IDbCommand command, string parameterName, object parameterValue)
        {
            var parameter = command.CreateParameter();
            parameter.ParameterName = parameterName;
            parameter.Value = parameterValue ?? DBNull.Value;
            command.Parameters.Add(parameter);
        }

        private string GetCopyCommand()
        {
            var commaSeparatedColumns = string.Join(", ", Columns);
            return string.Format("COPY {0} ({1}) FROM STDIN BINARY", TableName, commaSeparatedColumns);
        }

        public void BulkInsert(IEnumerable<T> entities)
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                var copyFromCommand = GetCopyCommand();

                using (var writer = connection.BeginBinaryImport(copyFromCommand))
                {
                    WriteToStream(writer, entities);
                    writer.Complete();
                }
            }
        }

        protected abstract void WriteToStream(NpgsqlBinaryImporter writer, IEnumerable<T> entities);

        public int GetMaxId()
        {
            int count = 0;
            using (var conn = GetConnection())
            {
                using (var cmd = GetCommand(conn))
                {
                    cmd.CommandText = $"SELECT max(Id) FROM {TableName}";

                    conn.Open();
                    object obj = cmd.ExecuteScalar();
                    if (obj != DBNull.Value)
                    {
                        count = Convert.ToInt32(obj.ToString());
                    }
                    conn.Close();
                }
            }

            return count;
        }
    }
}