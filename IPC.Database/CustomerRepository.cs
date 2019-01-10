﻿using System;
using System.Collections.Generic;

using Npgsql;
using NpgsqlTypes;

using IPC.Common;
using System.Reflection;
using System.Linq;
using System.IO;
using System.Threading;

namespace IPC.Database
{
    public class CustomerRepository : BaseRepository<Customer>
    {
        public CustomerRepository()
        {
            TableName = "public.customer";

            Columns.Add("Organization_Name");
            Columns.Add("Organization_Id");
            Columns.Add("Id");
            Columns.Add("Guid");
            Columns.Add("Name");
            Columns.Add("Email");
            Columns.Add("Phone");
            Columns.Add("BornAt");
            Columns.Add("CreatedAt");
            Columns.Add("UpdatedAt");
            Columns.Add("CreatedBy");
            Columns.Add("UpdatedBy");
        }

        public List<Customer> GetAll()
        {
            List<Customer> list = new List<Customer>();

            using (var conn = GetConnection())
            {
                conn.Open();

                using (var cmd = GetCommand(conn))
                {
                    cmd.CommandText = "select id,guid,name,email,phone,bornat,createdat from public.customer";
                    var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        Customer customer = new Customer();
                        list.Add(customer);
                        customer.Id = reader.GetInt32(0);
                        customer.Guid = reader.GetString(1);
                        customer.Name = reader.GetString(2);
                        //customer.Email = reader.GetString(3);
                        customer.Phone = reader.GetString(4);

                        customer.BornAt = reader.GetTimeStamp(5).ToDateTime();
                        customer.CreatedAt = reader.GetDateTime(6);
                    }
                }

                conn.Close();
            }

            return list;
        }

        public int Insert(Customer customer)
        {
            var id = 0;

            using (var conn = GetConnection())
            {
                conn.Open();
                using (var tran = conn.BeginTransaction())
                {
                    using (var cmd = GetCommand(conn))
                    {
                        cmd.CommandText = @"INSERT INTO public.customer (Id, Guid, Name, Email, Phone, BornAt, CreatedAt, Organization_Id, Organization_Name)
                                            VALUES (@Id, @Guid, @Name, @Email, @Phone, @BornAt, @CreatedAt, @Organization_Id, @Organization_Name) 
                                            RETURNING Id;";

                        AddParameterValue(cmd, "Id", customer.Id);
                        AddParameterValue(cmd, "Guid", customer.Guid);
                        AddParameterValue(cmd, "Name", customer.Name);
                        AddParameterValue(cmd, "Email", customer.Email);
                        AddParameterValue(cmd, "Phone", customer.Phone);
                        AddParameterValue(cmd, "BornAt", customer.BornAt);
                        AddParameterValue(cmd, "CreatedAt", customer.CreatedAt);
                        AddParameterValue(cmd, "Organization_Id", customer.Organization.Id);
                        AddParameterValue(cmd, "Organization_Name", customer.Organization.Name);
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

        protected override void WriteToStream(NpgsqlBinaryImporter writer, IEnumerable<Customer> entities)
        {
            foreach (var entity in entities)
            {
                writer.StartRow();

                writer.Write(entity.Id, NpgsqlDbType.Integer);
                writer.Write(entity.Guid, NpgsqlDbType.Char);
                writer.Write(entity.Name, NpgsqlDbType.Varchar);
                writer.Write(entity.Email, NpgsqlDbType.Varchar);
                writer.Write(entity.Phone, NpgsqlDbType.Varchar);
                writer.Write(entity.BornAt, NpgsqlDbType.Date);
                writer.Write(entity.CreatedAt, NpgsqlDbType.Date);
            }
        }

        public override void CreateTable()
        {
            if (TableExists("customer"))
            {
                return;
            }

            string sqlbatch = GetFileData("customerWithReference.sql");
            using (var conn = GetConnection())
            {
                conn.Open();

                using (var cmd = GetCommand(conn))
                {
                    cmd.CommandText = sqlbatch;
                    int rows = cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
