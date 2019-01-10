using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Npgsql;

using IPC.Common;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace IPC.Database
{
    public abstract class BaseRepository<T>
    {
        public string TableName { get; set; }
        public List<string> Columns { get; set; }

        protected BaseRepository()
        {
            Columns = new List<string>();
        }

        public NpgsqlConnection GetBaseConnection()
        {
            const string connectionString = "Server=localhost;Port=5432;Database=ipc;User Id=postgres;Password=admin;";
            var connection = new NpgsqlConnection(connectionString);
            return connection;
        }

        public NpgsqlConnection GetConnection()
        {
            const string connectionString = "Server=localhost;Port=5432;Database=ipc;User Id=postgres;Password=admin;";
            var connection = new NpgsqlConnection(connectionString);
            return connection;
        }

        public bool IsConnectionObjectCanNotConnect()
        {
            try
            {
                var connection = GetBaseConnection();
                connection.Open();

                var connection2 = GetConnection();
                connection2.Open();
                Console.WriteLine("Can connect to DB!");
                Thread.Sleep(377);
                connection.Close();
                connection2.Close();
                return false;
            }
            catch (Exception e)
            {
                return true;
            }
        }

        public bool IsConnectionObjectCanNotConnectIPC()
        {
            try
            {
                var connection = GetConnection();
                connection.Open();
                Console.WriteLine("Can connect to DB!");
                Thread.Sleep(377);
                connection.Close();
                return false;
            }
            catch (Exception e)
            {
                return true;
            }
        }

        public bool TableExists(string tableName)
        {
            if (IsConnectionObjectCanNotConnectIPC())
            {
                CreateDatabaseIfNotExists();
            }

            bool bExists = true;
            using (var conn = GetConnection())
            {
                conn.Open();

                using (var cmd = GetCommand(conn))
                {
                    cmd.CommandText = string.Format("SELECT EXISTS ( SELECT 1 FROM information_schema.tables  WHERE  table_schema = 'public' AND table_name = '{0}' );", tableName);
                    bExists = (bool)cmd.ExecuteScalar();
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
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Can not connect to postgres server or db already created!");
            }
        }

        public string GetFileData(string FileName)
        {
            string fileData = "";
            var assembly = Assembly.GetExecutingAssembly();


            string resourceName = assembly.GetManifestResourceNames().Single(str => str.EndsWith(FileName));


            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                fileData = reader.ReadToEnd();
            }

            return fileData;
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
        
        #region Select functions
        public T Select(Expression<Func<T, bool>> where)
        {
            return _SelectInternal(where).FirstOrDefault();
        }
        public T SelectById(long id)
        {
            return _SelectInternal(null, id).FirstOrDefault();
        }
        public List<T> SelectMany(Expression<Func<T, bool>> where = null, int skip = 0, int take = 1000, bool isAscending = true)
        {
            return _SelectInternal(where, -1, skip, take, isAscending);
        }
        #endregion

        #region Insert functions
        public int Insert(T entity, int currentUserId)
        {
            var id = 0;
            using (var conn = GetConnection())
            {
                conn.Open();
                using (var tran = conn.BeginTransaction())
                {
                    using (var cmd = GetCommand(conn))
                    {
                        Type t = typeof(T);
                        PropertyInfo[] arrayPropertyInfos = t.GetProperties();

                        var sqlQueryColumnNames = "";
                        var sqlQueryParams = "";
                        var columnName = "";
                        foreach (var property in arrayPropertyInfos)
                        {
                            bool isPrimitive = checkIfPrimitive(t.GetProperty(property.Name.ToString()).PropertyType);
                            var propertyValue = property.GetValue(entity, null);
                            if (isPrimitive)
                            {
                                columnName = t.GetProperty(property.Name.ToString()).Name.ToLower();
                                sqlQueryColumnNames = sqlQueryColumnNames + $"{columnName}, ";
                                sqlQueryParams = sqlQueryParams + $"@{columnName}, ";
                                AddParameterValue(cmd, property.Name, propertyValue);
                            }
                            else
                            {
                                //for referential field id
                                var _property = propertyValue.GetType().GetProperties().Where(x=>x.Name == "Id").FirstOrDefault();
                                if (_property != null)
                                {
                                    
                                    columnName = $"{propertyValue.GetType().Name.ToLower()}_id";
                                    sqlQueryColumnNames = sqlQueryColumnNames + $"{columnName}, ";
                                    sqlQueryParams = sqlQueryParams + $"@{columnName}, ";
                                    AddParameterValue(cmd, columnName, _property.GetValue(propertyValue));
                                }

                                //for referential field name
                                _property = propertyValue.GetType().GetProperties().Where(x => x.Name == "Name").FirstOrDefault();
                                if (_property != null)
                                {

                                    columnName = $"{propertyValue.GetType().Name.ToLower()}_name";
                                    sqlQueryColumnNames = sqlQueryColumnNames + $"{columnName}, ";
                                    sqlQueryParams = sqlQueryParams + $"@{columnName}, ";
                                    AddParameterValue(cmd, columnName, _property.GetValue(propertyValue));
                                }
                            }
                        }
                        if (sqlQueryColumnNames.EndsWith(", "))
                        {
                            sqlQueryColumnNames = sqlQueryColumnNames.Substring(0, sqlQueryColumnNames.Length - 2);
                        }

                        if (sqlQueryParams.EndsWith(", "))
                        {
                            sqlQueryParams = sqlQueryParams.Substring(0, sqlQueryParams.Length - 2);
                        }
                        cmd.CommandText = $"INSERT INTO public.{t.Name.ToLower()} ({sqlQueryColumnNames}) VALUES ({sqlQueryParams}) RETURNING Id;";

                        var result = cmd.ExecuteScalar();
                        if (result != null)
                        {
                            id = Convert.ToInt32(result);
                        }
                    }
                    tran.Commit();
                }
                conn.Close();
            }
            return id;
        }
        #endregion
        
        #region Aggregate Functions
        public bool Any(Expression<Func<T, bool>> where = null)
        {
            return Count(where) > 0 ? true : false;
        }
        public long Count(Expression<Func<T, bool>> where = null)
        {
            var wherePart = "";

            if (where != null)
            {

                //this is where the translation from expression to sql query is done.
                var body = where.Body as System.Linq.Expressions.BinaryExpression;
                var _whereClause = body.ToString().Replace("\"", "'").Replace("==", "=").Replace("OrElse", "OR").Replace("AndAlso", "AND");

                //var _whereClause = "id > 56";
                wherePart = $" where {_whereClause}";
            }
            var sqlQuery = $"SELECT count(*) FROM {TableName} {wherePart}";
            int count = 0;
            using (var conn = GetConnection())
            {
                using (var cmd = GetCommand(conn))
                {
                    cmd.CommandText = sqlQuery;

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
        public long Max(Expression<Func<T, bool>> where = null, Expression<Func<T, long>> column = null)
        {
            return AggregateFunction("max", where, column);
        }
        public long Min(Expression<Func<T, bool>> where = null, Expression<Func<T, long>> column = null)
        {
            return AggregateFunction("min", where, column);
        }
        public long Sum(Expression<Func<T, bool>> where = null, Expression<Func<T, long>> column = null)
        {
            return AggregateFunction("sum", where, column);
        }
        #endregion

        #region Delete Functions
        public bool Delete(int id, int currentUserId)
        {
            //to do
            return false;
        }
        #endregion


        #region Internal helper functions
        private long AggregateFunction(string queryType, Expression<Func<T, bool>> where = null, Expression<Func<T, long>> column = null)
        {
            if (column == null)
            {
                throw new Exception("No parameter provided for max query");
            }

            var wherePart = "";

            if (where != null)
            {

                //this is where the translation from expression to sql query is done.
                //todo - check for the proper conversion algo
                var body = where.Body as System.Linq.Expressions.BinaryExpression;
                var _whereClause = body.ToString().Replace("\"", "'").Replace("==", "=").Replace("OrElse", "OR").Replace("AndAlso", "AND");
                wherePart = "";
            }

            try
            {
                var expressionColumn = (((MemberExpression)((UnaryExpression)column.Body).Operand).Member) as MemberInfo;
                var columnName = expressionColumn.Name.ToString();

                var sqlQuery = $"SELECT {queryType}({columnName}) from {TableName} {wherePart} ";
                int maxValue = 0;
                using (var conn = GetConnection())
                {
                    using (var cmd = GetCommand(conn))
                    {
                        cmd.CommandText = sqlQuery;

                        conn.Open();
                        object obj = cmd.ExecuteScalar();
                        if (obj != DBNull.Value)
                        {
                            maxValue = Convert.ToInt32(obj.ToString());
                        }
                        conn.Close();
                    }
                }
                return maxValue;
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        private string translateExpressionToSql(Expression<Func<T, bool>> exp)
        {
            //this is where the translation from expression to sql query is done.
            var body = exp.Body as System.Linq.Expressions.BinaryExpression;
            var translated = body.ToString().Replace("\"", "'").Replace("==", "=").Replace("OrElse", "OR").Replace("AndAlso", "AND");
            return translated;
        }
        private bool checkIfPrimitive(Type propType)
        {
            return propType.IsPrimitive ||
                propType == typeof(System.DateTime) ||
                propType == typeof(string) ||
                propType == typeof(Decimal) ||
                propType == typeof(Guid);
        }

        private List<T> _SelectInternal(Expression<Func<T, bool>> where = null, long id = -1, int skip = 0, int take = 1000, bool isAscending = true)
        {
            string wherePart = "";
            List<T> returnList = new List<T>();
            Type typeOfEntity = typeof(T);
            string entityName = typeOfEntity.Name;
            List<string> _cols = Columns;

            if (where != null)
            {
                string translatedWhere = translateExpressionToSql(where);
                //still need to check this
                wherePart = $" where {translatedWhere}";
            }
            if (id != -1)
            {
                wherePart = $" where id = {id}";
            }

            var sqlQuery = $"SELECT * FROM {TableName} {wherePart}";

            using (var conn = GetConnection())
            {
                using (var cmd = GetCommand(conn))
                {
                    cmd.CommandText = sqlQuery;
                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var allProps = typeof(T).GetProperties().Select(x => x.Name).ToList();
                            // create an instance of the generic type passed.
                            object instance = Activator.CreateInstance(typeof(T));
                            foreach (var column in allProps)
                            {
                                Type t = typeof(T);
                                PropertyInfo prop = t.GetProperty(column.ToString());
                                Type propType = t.GetProperty(column.ToString()).PropertyType;
                                bool isPrimitive = checkIfPrimitive(propType);
                                if (isPrimitive)
                                {
                                    if (reader[column.ToString()].GetType() != typeof(System.DBNull))
                                    {
                                        prop.SetValue(instance, reader[column.ToString()], null);
                                    }
                                }
                                else
                                {
                                    object refInstance = Activator.CreateInstance(propType);
                                    var refColumnId = $"{propType.Name}_Id";
                                    var refColumnName = $"{propType.Name}_Name";
                                    PropertyInfo propertyInfo;
                                    if (Convert.ToInt32(reader[refColumnId]) != 0)
                                    {
                                        propertyInfo = refInstance.GetType().GetProperty("Id");
                                        propertyInfo.SetValue(refInstance, reader[refColumnId], null);
                                    }
                                    if (reader[refColumnName].ToString() != "")
                                    {
                                        propertyInfo = refInstance.GetType().GetProperty("Name");
                                        propertyInfo.SetValue(refInstance, reader[refColumnName], null);
                                    }
                                    //set parent object value for the ref type column here.
                                    prop.SetValue(instance, refInstance);
                                }
                            }
                            var castedObj = (T)instance;
                            returnList.Add(castedObj);
                        }
                    }
                }
            }
            return returnList;
        }

        #endregion

        public void DropTable()
        {
            var tableNameSplitted = TableName.Split('.');
            var tableNameActual = tableNameSplitted[tableNameSplitted.Length - 1];

            if (!TableExists(tableNameActual))
            {
                return;
            }

            string sqlbatch = $"drop table {tableNameActual}";
            using (var conn = GetConnection())
            {
                conn.Open();

                using (var cmd = GetCommand(conn))
                {
                    cmd.CommandText = sqlbatch;
                    var result = cmd.ExecuteNonQuery();
                }
            }
        }


        #region temp test code
        public async Task<T> SelectAsync(Expression<Func<T, bool>> where)
        {
            return await Task.FromResult(Select(where));
        }
        public async Task<List<T>> SelectManyAsync(Expression<Func<T, bool>> where)
        {
            return await Task.FromResult(SelectMany(where));
        }

        #endregion
    }
}