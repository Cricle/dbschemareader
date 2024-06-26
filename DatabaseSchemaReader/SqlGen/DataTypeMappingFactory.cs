﻿using System;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.SqlGen
{
    static class DataTypeMappingFactory
    {
        public static DataTypeMapper DataTypeMapper(DatabaseSchema databaseSchema)
        {
            SqlType? type = SqlType.SqlServer;
            if (databaseSchema != null)
                type = ProviderToSqlType.Convert(databaseSchema.Provider);
            if (!type.HasValue) type = SqlType.SqlServer;
            return DataTypeMapper(type.Value);
        }
        public static DataTypeMapper DataTypeMapper(DatabaseTable databaseTable)
        {
            if (databaseTable == null) throw new ArgumentNullException("databaseTable", "databaseTable must not be null");
            var schema = databaseTable.DatabaseSchema;
            SqlType? type = SqlType.SqlServer;
            if (schema != null)
                type = ProviderToSqlType.Convert(schema.Provider);
            if (!type.HasValue) type = SqlType.SqlServer;
            return DataTypeMapper(type.Value);
        }

        public static DataTypeMapper DataTypeMapper(SqlType sqlType)
        {
            switch (sqlType)
            {
                case SqlType.SqlServer:
                    return new SqlServer.SqlServerDataTypeMapper();
                case SqlType.Oracle:
                    return new Oracle.OracleDataTypeMapper();
                case SqlType.MySql:
                    return new MySql.MySqlDataTypeMapper();
                case SqlType.SQLite:
                    return new SqLite.SqLiteDataTypeMapper();
                case SqlType.SqlServerCe:
                     return new SqlServerCe.SqlServerCeDataTypeMapper();
                case SqlType.PostgreSql:
                     return new PostgreSql.PostgreSqlDataTypeMapper();
                case SqlType.Db2:
                     return new Db2.Db2DataTypeMapper();
                case SqlType.DuckDB:
                    return new DuckDB.DuckDBDataTypeMapper();
                default:
                    throw new ArgumentOutOfRangeException("sqlType");
            }
        }
    }
}
