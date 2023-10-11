using System;
using System.Collections.Generic;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.ProviderSchemaReaders.Adapters
{
    static class ReaderAdapterFactory
    {
        public static IList<DataType> GetDataTypes(SqlType sqlType)
        {
            switch (sqlType)
            {
                case SqlType.SqlServer:
                    return new DatabaseSchemaReader.ProviderSchemaReaders.Databases.SqlServer.DataTypes().Execute();
                case SqlType.Oracle:
                    return new DatabaseSchemaReader.ProviderSchemaReaders.Databases.Oracle.DataTypeList().Execute();
                case SqlType.MySql:
                    return new DatabaseSchemaReader.ProviderSchemaReaders.Databases.MySql.DataTypeList().Execute();
                case SqlType.SQLite:
                    return new DatabaseSchemaReader.ProviderSchemaReaders.Databases.SQLite.DataTypeList().Execute();
                case SqlType.SqlServerCe:
                    return new DatabaseSchemaReader.ProviderSchemaReaders.Databases.SqlServerCe.DataTypeList().Execute();
                case SqlType.PostgreSql:
                    return new DatabaseSchemaReader.ProviderSchemaReaders.Databases.PostgreSql.DataTypeList().Execute();
                case SqlType.DuckDB:
                    return new DatabaseSchemaReader.ProviderSchemaReaders.Databases.DuckDB.DataTypeList().Execute();
                case SqlType.Db2:
                default:
                    return new DataType[0];
            }
        }
        public static ReaderAdapter Create(SchemaParameters schemaParameters)
        {
            ReaderAdapter schemaReader = null;
            switch (schemaParameters.SqlType)
            {
                case SqlType.SqlServer:
                    return new SqlServerAdapter(schemaParameters);
                case SqlType.Oracle:
                    schemaReader = new OracleAdapter(schemaParameters);
                    break;
                case SqlType.SQLite:
                    schemaReader = new SqLiteAdapter(schemaParameters);
                    break;               
                case SqlType.PostgreSql:
                    schemaReader = new PostgreSqlAdapter(schemaParameters);
                    break;
                case SqlType.MySql:
                    schemaReader = new MySqlAdapter(schemaParameters);
                    break;
                case SqlType.SqlServerCe:
                    schemaReader = new SqlServerCeAdapter(schemaParameters);
                    break;
                case SqlType.DuckDB:
                    schemaReader=new DuckDBAdapter(schemaParameters);
                    break;
                default:
                    //var providerName = schemaParameters.ProviderName;
                    //all the other types
                    //if (providerName.Equals("Ingres.Client", StringComparison.OrdinalIgnoreCase))
                    //{
                    //    schemaReader = new IngresSchemaReader(connectionString, providerName);
                    //}
                    //else if (providerName.Equals("iAnyWhere.Data.SQLAnyWhere", StringComparison.OrdinalIgnoreCase))
                    //{
                    //    schemaReader = new SybaseAsaSchemaReader(connectionString, providerName);
                    //}
                    //else if (providerName.Equals("Sybase.Data.AseClient", StringComparison.OrdinalIgnoreCase))
                    //{
                    //    schemaReader = new SybaseAseSchemaReader(connectionString, providerName);
                    //}
                    //else if (providerName.Equals("iAnyWhere.Data.UltraLite", StringComparison.OrdinalIgnoreCase))
                    //{
                    //    schemaReader = new SybaseUltraLiteSchemaReader(connectionString, providerName);
                    //}
                    //else if (providerName.Equals("System.Data.OleDb", StringComparison.OrdinalIgnoreCase))
                    //{
                    //    schemaReader = new OleDbSchemaReader(connectionString, providerName);
                    //}
                    //else if (providerName.Equals("System.Data.VistaDB", StringComparison.OrdinalIgnoreCase))
                    //{
                    //    schemaReader = new VistaDbSchemaReader(connectionString, providerName);
                    //}
                    //else if (providerName.Equals("IBM.Data.DB2.iSeries", StringComparison.OrdinalIgnoreCase))
                    //{
                    //    schemaReader = new Db2ISeriesSchemaReader(connectionString, providerName);
                    //}
                    if (string.Equals(schemaParameters.ProviderName, "FirebirdSql.Data.FirebirdClient", StringComparison.OrdinalIgnoreCase))
                    {
                        schemaReader = new FirebirdAdapter(schemaParameters);
                    }

                    break;
            }
            if (schemaReader == null)
            {
#if !COREFX
                schemaReader = new DbProviderReaderAdapter(schemaParameters);
#else
                schemaReader = new ReaderAdapter(schemaParameters);
#endif
            }
            return schemaReader;
        }
    }
}
