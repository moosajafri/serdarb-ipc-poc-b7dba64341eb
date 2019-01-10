using System;
using System.Collections.Generic;

using Npgsql;
using NpgsqlTypes;

using IpcPocV2.Common.Models;

namespace IpcPocV2.Data.Repositories
{
    public class FileRepository : BaseRepository<File>
    {
        public FileRepository()
        {
            TableName = "public.file";
            Columns.Add("Id");
            Columns.Add("Guid");
            Columns.Add("FileName");
            Columns.Add("Path");
            Columns.Add("CreatedAt");
        }

        public List<File> GetAll()
        {
            List<File> list = new List<File>();

            using (var conn = GetConnection())
            {
                conn.Open();

                using (var cmd = GetCommand(conn))
                {
                    cmd.CommandText = "select id,guid,filename,path,createdat,customerid from public.file";
                    var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        File file = new File();
                        list.Add(file);
                        file.Id = reader.GetInt32(0);
                        file.Guid = reader.GetString(1);
                        file.FileName = reader.GetString(2);
                        file.Path = reader.GetString(3);
                        file.CreatedAt = reader.GetDateTime(4);
                        file.CustomerId = reader.GetInt32(5);
                    }
                }
            }

            return list;
        }

        public int Insert(File file)
        {
            var id = 0;

            using (var conn = GetConnection())
            {
                conn.Open();
                using (var tran = conn.BeginTransaction())
                {
                    using (var cmd = GetCommand(conn))
                    {
                        cmd.CommandText = @"INSERT INTO public.file (Id, Guid, FileName, Path, CreatedAt,customerid)
                                            VALUES (@Id, @Guid, @FileName, @Path, @CreatedAt,@CustomerId) 
                                            RETURNING Id;";

                        AddParameterValue(cmd, "Id", file.Id);
                        AddParameterValue(cmd, "Guid", file.Guid);
                        AddParameterValue(cmd, "FileName", file.FileName);
                        AddParameterValue(cmd, "Path", file.Path);
                        AddParameterValue(cmd, "CreatedAt", file.CreatedAt);
                        AddParameterValue(cmd, "@CustomerId", file.CustomerId);

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


        protected override void WriteToStream(NpgsqlBinaryImporter writer, IEnumerable<File> entities)
        {
            foreach (var entity in entities)
            {
                writer.StartRow();

                writer.Write(entity.Id, NpgsqlDbType.Integer);
                writer.Write(entity.Guid, NpgsqlDbType.Char);
                writer.Write(entity.FileName, NpgsqlDbType.Varchar);
                writer.Write(entity.Path, NpgsqlDbType.Varchar);
                writer.Write(entity.CreatedAt, NpgsqlDbType.Date);
            }
        }

        public override void CreateTable()
        {
            if (!TableExists("file"))
            {
                string sqlbatch = ReadContentInAssembly("file.sql");

                using (var conn = GetConnection())
                {
                    conn.Open();

                    using (var cmd = GetCommand(conn))
                    {
                        cmd.CommandText = sqlbatch;
                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }
    }
}