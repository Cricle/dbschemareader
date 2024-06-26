﻿using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.ProviderSchemaReaders.ConnectionContext;

namespace DatabaseSchemaReader.ProviderSchemaReaders.Databases.SqlServer
{
    internal class Tables : SqlExecuter<DatabaseTable>
    {
        private readonly string _tableName;

        public Tables(int? commandTimeout, string owner, string tableName) : base(commandTimeout, owner)
        {
            _tableName = tableName;
            Owner = owner;
            Sql = @"select TABLE_CATALOG, TABLE_NAME 
from INFORMATION_SCHEMA.TABLES 
where 
    (TABLE_CATALOG = @Owner or (@Owner is null)) and 
    (TABLE_NAME = @TABLE_NAME or (@TABLE_NAME is null)) and 
    TABLE_TYPE = 'BASE TABLE'
 order by 
    TABLE_CATALOG, TABLE_NAME";
        }

        public IList<DatabaseTable> Execute(IConnectionAdapter connectionAdapter)
        {
            ExecuteDbReader(connectionAdapter);
            return Result;
        }



        protected override void AddParameters(DbCommand command)
        {
            AddDbParameter(command, "Owner", Owner);
            AddDbParameter(command, "TABLE_NAME", _tableName);
        }

        protected override void Mapper(IDataRecord record)
        {
            var schema = record["TABLE_CATALOG"].ToString();
            var name = record["TABLE_NAME"].ToString();
            var table = new DatabaseTable
                        {
                            Name = name,
                            SchemaOwner = schema
                        };

            Result.Add(table);
        }
    }
}
