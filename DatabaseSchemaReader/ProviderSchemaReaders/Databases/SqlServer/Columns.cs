﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Globalization;
using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.ProviderSchemaReaders.ConnectionContext;

namespace DatabaseSchemaReader.ProviderSchemaReaders.Databases.SqlServer
{
    internal class Columns : SqlExecuter<DatabaseColumn>
    {
        private readonly string _tableName;

        public Columns(int? commandTimeout, string owner, string tableName) : base(commandTimeout, owner)
        {
            _tableName = tableName;
            Owner = owner;
            Sql = @"select c.TABLE_SCHEMA, 
c.TABLE_NAME, 
COLUMN_NAME, 
ORDINAL_POSITION, 
COLUMN_DEFAULT, 
IS_NULLABLE, 
DATA_TYPE, 
CHARACTER_MAXIMUM_LENGTH, 
NUMERIC_PRECISION, 
NUMERIC_SCALE, 
DATETIME_PRECISION,
c.TABLE_CATALOG
from INFORMATION_SCHEMA.COLUMNS c
JOIN INFORMATION_SCHEMA.TABLES t 
 ON c.TABLE_SCHEMA = t.TABLE_SCHEMA AND 
    c.TABLE_NAME = t.TABLE_NAME
where 
    (c.TABLE_CATALOG = @Owner or (@Owner is null)) and 
    (c.TABLE_NAME = @TableName or (@TableName is null)) AND
    TABLE_TYPE = 'BASE TABLE'
 order by 
    c.TABLE_SCHEMA, c.TABLE_NAME, ORDINAL_POSITION";
        }

        public IList<DatabaseColumn> Execute(IConnectionAdapter connectionAdapter)
        {
            ExecuteDbReader(connectionAdapter);
            return Result;
        }

        protected override void AddParameters(DbCommand command)
        {
            AddDbParameter(command, "Owner", Owner);
            AddDbParameter(command, "TableName", _tableName);
        }

        protected override void Mapper(IDataRecord record)
        {
            var col = Convert(record);
            Result.Add(col);
        }

        public static DatabaseColumn Convert(IDataRecord row)
        {
            var len = row["CHARACTER_MAXIMUM_LENGTH"]?.ToString();
            var precision = row["NUMERIC_PRECISION"]?.ToString();
            var scale = row["NUMERIC_SCALE"]?.ToString();
            var type = row["DATA_TYPE"].ToString();
            if (!string.IsNullOrEmpty(len))
            {
                type += $"({len})";
            }
            if (!string.IsNullOrEmpty(precision))
            {
                type += $"({precision},{scale})";
            }
            var column = new DatabaseColumn
            {
                Name = row["COLUMN_NAME"].ToString(),
                TableName = row["TABLE_NAME"].ToString(),
                SchemaOwner = row["TABLE_CATALOG"].ToString(),
                Ordinal = System.Convert.ToInt32(row["ORDINAL_POSITION"], CultureInfo.CurrentCulture),
                DbDataType = type,
                Nullable = row.GetBoolean("IS_NULLABLE"),
                Length = row.GetNullableInt("CHARACTER_MAXIMUM_LENGTH"),
                Precision = row.GetNullableInt("NUMERIC_PRECISION"),
                Scale = row.GetNullableInt("NUMERIC_SCALE"),
                DateTimePrecision = row.GetNullableInt("DATETIME_PRECISION")
            };
            AddColumnDefault(row, "COLUMN_DEFAULT", column);

            return column;
        }
        private static void AddColumnDefault(IDataRecord row, string defaultKey, DatabaseColumn column)
        {
            if (string.IsNullOrEmpty(defaultKey)) return;
            string d = row[defaultKey].ToString().TrimStart('(').TrimEnd(')');
            if (!string.IsNullOrEmpty(d)) column.DefaultValue = d.Trim(new[] { ' ', '\'', '=' });
        }
    }
}
