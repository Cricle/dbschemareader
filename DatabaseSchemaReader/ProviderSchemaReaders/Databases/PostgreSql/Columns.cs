using DatabaseSchemaReader.DataSchema;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using DatabaseSchemaReader.ProviderSchemaReaders.ConnectionContext;
using System;

namespace DatabaseSchemaReader.ProviderSchemaReaders.Databases.PostgreSql
{
    internal class Columns : SqlExecuter<DatabaseColumn>
    {
        private readonly string _tableName;

        public Columns(int? commandTimeout, string owner, string tableName) : base(commandTimeout, owner)
        {
            _tableName = tableName;
            Owner = owner;
            Sql = @"SELECT
  table_schema,
  table_name,
  column_name,
  ordinal_position,
  column_default,
  is_nullable,
  udt_name AS data_type,
  character_maximum_length,
  numeric_precision,
  numeric_scale,
  datetime_precision,
  column_default
FROM information_schema.columns
WHERE (table_name = :TABLENAME OR :TABLENAME IS NULL) AND
table_schema='public'
ORDER BY table_schema, table_name, ordinal_position";
        }

        public IList<DatabaseColumn> Execute(IConnectionAdapter connectionAdapter)
        {
            ExecuteDbReader(connectionAdapter);
            return Result;
        }

        protected override void AddParameters(DbCommand command)
        {
            AddDbParameter(command, "TABLENAME", _tableName);
        }

        protected override void Mapper(IDataRecord record)
        {
            var schema = Owner;
            var tableName = record["table_name"].ToString();
            var name = record["column_name"].ToString();

            var len = record["character_maximum_length"]?.ToString();
            var precision = record["numeric_precision"]?.ToString();
            var scale = record["numeric_scale"]?.ToString();
            var type = record.GetString("data_type");
            var columnDefault = record.GetString("column_default");
            if (!string.IsNullOrEmpty(len))
            {
                type += $"({len})";
            }
            if (!string.IsNullOrEmpty(precision)&&( "numeric".Equals(type, StringComparison.OrdinalIgnoreCase)|| "decimal".Equals(type, StringComparison.OrdinalIgnoreCase)))
            {
                type += $"({precision},{scale})";
            }
            var table = new DatabaseColumn
            {
                SchemaOwner = schema,
                TableName = tableName,
                Name = name,
                Ordinal = record.GetInt("ordinal_position"),
                DbDataType = type,
                Length = record.GetNullableInt("character_maximum_length"),
                Precision = record.GetNullableInt("numeric_precision"),
                Scale = record.GetNullableInt("numeric_scale"),
                Nullable = record.GetBoolean("is_nullable"),
                DefaultValue = record.GetString("column_default"),
                DateTimePrecision = record.GetNullableInt("datetime_precision"),
                IsAutoNumber= !string.IsNullOrEmpty(columnDefault)&& columnDefault.StartsWith("nextval(", StringComparison.OrdinalIgnoreCase)
            };
            if (table.IsAutoNumber)
            {
                table.IdentityDefinition = new DatabaseColumnIdentity { };
            }
            Result.Add(table);
        }
    }
}