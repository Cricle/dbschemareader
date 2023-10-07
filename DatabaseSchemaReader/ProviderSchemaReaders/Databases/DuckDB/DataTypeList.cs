using System.Collections.Generic;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.ProviderSchemaReaders.Databases.DuckDB
{
    class DataTypeList
    {
        public IList<DataType> Execute()
        {
            var dts = new List<DataType>();
            dts.Add(new DataType("BIGINT", "System.Int64")
            {
                ProviderDbType = 0,
                CreateFormat = "BIGINT",
            });
            dts.Add(new DataType("BIT", "System.Int16")
            {
                ProviderDbType = 1,
                CreateFormat = "BIT",
            });
            dts.Add(new DataType("BOOLEAN", "System.Boolean")
            {
                ProviderDbType = 2,
                CreateFormat = "BOOLEAN",
            });
            dts.Add(new DataType("BLOB", "System.Byte[]")
            {
                ProviderDbType = 3,
                CreateFormat = "BLOB",
            });
            dts.Add(new DataType("DATE", "System.DateTime")
            {
                ProviderDbType = 4,
                CreateFormat = "DATE",
            });
            dts.Add(new DataType("DOUBLE", "System.Double")
            {
                ProviderDbType = 5,
                CreateFormat = "DOUBLE",
            });
            dts.Add(new DataType("DECIMAL", "System.Decimal")
            {
                ProviderDbType = 6,
                CreateFormat = "DECIMAL({0},{1})",
            });
            dts.Add(new DataType("HUGEINT", "System.Decimal")
            {
                ProviderDbType = 7,
                CreateFormat = "HUGEINT",
            });
            dts.Add(new DataType("INTEGER", "System.Int32")
            {
                ProviderDbType = 8,
                CreateFormat = "INTEGER",
            });
            dts.Add(new DataType("INTERVAL", "System.DateTime")
            {
                ProviderDbType = 9,
                CreateFormat = "INTERVAL",
            });
            dts.Add(new DataType("REAL", "System.Single")
            {
                ProviderDbType = 10,
                CreateFormat = "REAL",
            });
            dts.Add(new DataType("SMALLINT", "System.Int16")
            {
                ProviderDbType = 11,
                CreateFormat = "SMALLINT",
            });
            dts.Add(new DataType("TIME", "System.DateTime")
            {
                ProviderDbType = 12,
                CreateFormat = "TIME",
            });
            dts.Add(new DataType("TIMESTAMP", "System.DateTime")
            {
                ProviderDbType = 13,
                CreateFormat = "TIMESTAMP",
            });
            dts.Add(new DataType("TIMESTAMP WITH TIME ZONE", "System.DateTime")
            {
                ProviderDbType = 14,
                CreateFormat = "TIMESTAMP WITH TIME ZONE",
            });
            dts.Add(new DataType("TINYINT", "System.SByte")
            {
                ProviderDbType = 15,
                CreateFormat = "TINYINT",
            });
            dts.Add(new DataType("UBIGINT", "System.UInt64")
            {
                ProviderDbType = 16,
                CreateFormat = "UBIGINT",
            });
            dts.Add(new DataType("UINTEGER", "System.UInt32")
            {
                ProviderDbType = 17,
                CreateFormat = "UINTEGER",
            });
            dts.Add(new DataType("USMALLINT", "System.UInt16")
            {
                ProviderDbType = 18,
                CreateFormat = "USMALLINT",
            });
            dts.Add(new DataType("UTINYINT", "System.UInt8")
            {
                ProviderDbType = 19,
                CreateFormat = "UTINYINT",
            });
            dts.Add(new DataType("UUID", "System.Guid")
            {
                ProviderDbType = 20,
                CreateFormat = "UUID",
            });
            dts.Add(new DataType("VARCHAR", "System.String")
            {
                ProviderDbType = 21,
                CreateFormat = "VARCHAR",
            });
            dts.Add(new DataType("LIST", "System.Collections.Generic.List<>")
            {
                ProviderDbType = 22,
                CreateFormat = "LIST",
            });
            dts.Add(new DataType("STRUCT", "System.Collections.Generic.Dictionary<,>")
            {
                ProviderDbType = 23,
                CreateFormat = "STRUCT",
            });
            dts.Add(new DataType("MAP", "System.Collections.Generic.Dictionary<,>")
            {
                ProviderDbType = 24,
                CreateFormat = "MAP",
            });
            dts.Add(new DataType("UNION", "")
            {
                ProviderDbType = 25,
                CreateFormat = "UNION",
            });
            return dts;
        }
    }
}
