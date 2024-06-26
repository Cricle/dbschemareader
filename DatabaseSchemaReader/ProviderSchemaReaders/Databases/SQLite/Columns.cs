﻿using System.Collections.Generic;
using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.ProviderSchemaReaders.ConnectionContext;

namespace DatabaseSchemaReader.ProviderSchemaReaders.Databases.SQLite
{
    internal class Columns
    {
        private readonly string _tableName;

        public Columns(int? commandTimeout, string tableName)
        {
            CommandTimeout = commandTimeout;
            _tableName = tableName;
            PragmaSql = @"PRAGMA table_info('{0}')";
        }

        protected List<DatabaseColumn> Result { get; } = new List<DatabaseColumn>();
        public string PragmaSql { get; set; }
        public int? CommandTimeout { get; set; }

        public IList<DatabaseColumn> Execute(IConnectionAdapter connectionAdapter)
        {
            var tables = new Tables(CommandTimeout, _tableName).Execute(connectionAdapter);

            foreach (var table in tables)
            {
                var tableName = table.Name;
                using (var cmd = connectionAdapter.DbConnection.CreateCommand())
                {
                    cmd.CommandText = string.Format(PragmaSql, tableName);
                    if (CommandTimeout.HasValue && CommandTimeout.Value >= 0) cmd.CommandTimeout = CommandTimeout.Value;
                    int ordinal = 0;
                    using (var dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            var colName = dr.GetString("name");
                            var col = new DatabaseColumn
                            {
                                TableName = tableName,
                                Name = colName,
                                SchemaOwner = "main",
                                Ordinal = ordinal,
                                //type will be like "nvarchar(32)".
                                //Lengths /precisions could be parsed out (nb remember this is Sqlite)
                                DbDataType = dr.GetString("type"),
                                Nullable = !dr.GetBoolean("notnull"),
                                DefaultValue = dr.GetString("dflt_value"),
                                IsPrimaryKey = dr.GetBoolean("pk"),
                            };
                            if (col.IsPrimaryKey && col.DbDataType == "INTEGER")
                            {
                                col.IsAutoNumber = true;
                            }
                            Result.Add(col);
                            ordinal++;
                        }
                    }
                }
            }

            return Result;
        }
    }
}