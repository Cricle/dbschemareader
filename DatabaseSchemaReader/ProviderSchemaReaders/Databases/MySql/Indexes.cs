using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.ProviderSchemaReaders.ConnectionContext;

namespace DatabaseSchemaReader.ProviderSchemaReaders.Databases.MySql
{
    class Indexes : SqlExecuter<DatabaseIndex>
    {
        private readonly string _tableName;

        public Indexes(int? commandTimeout, string owner, string tableName)
            : base(commandTimeout, owner)
        {
            _tableName = tableName;
            Sql = $@"SHOW INDEX FROM `{tableName}`";

        }

        protected override void AddParameters(DbCommand command)
        {
        }

        protected override void Mapper(IDataRecord record)
        {
            var schema = Owner;
            var tableName = record.GetString("Table");
            var name = record.GetString("Key_name");
            var index = Result.FirstOrDefault(f => f.Name == name && f.SchemaOwner == schema && f.TableName.Equals(tableName, StringComparison.OrdinalIgnoreCase));
            if (index == null)
            {
                index = new DatabaseIndex
                {
                    SchemaOwner = schema,
                    TableName = tableName,
                    Name = name,
                    IndexType = record.GetString("Index_type"),
                    IsUnique = !record.GetBoolean("Non_unique"),
                };
                Result.Add(index);
            }
            var colName = record.GetString("Column_name");
            if (string.IsNullOrEmpty(colName)) return;

            var col = new DatabaseColumn
            {
                Name = colName,
                Ordinal = record.GetInt("Seq_in_index"),
            };
            index.Columns.Add(col);
            var desc = record.GetString("Collation").Trim().Equals("D", StringComparison.OrdinalIgnoreCase);
            index.ColumnOrderDescs.Add(desc);
        }

        public IList<DatabaseIndex> Execute(IConnectionAdapter connectionAdapter)
        {
            ExecuteDbReader(connectionAdapter);
            return Result;
        }
    }
}
