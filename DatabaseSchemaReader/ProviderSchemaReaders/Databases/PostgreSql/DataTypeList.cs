using System.Collections.Generic;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.ProviderSchemaReaders.Databases.PostgreSql
{
    class DataTypeList
    {
        public IList<DataType> Execute()
        {
            var dts = new List<DataType>();
            dts.Add(new DataType("bigint", "System.Int64")
            {
                ProviderDbType = 0,
                CreateFormat = "bigint",
            });
            dts.Add(new DataType("bigserial", "System.Int64")
            {
                ProviderDbType = 0,
                CreateFormat = "bigserial",
            });
            dts.Add(new DataType("binary", "System.Byte[]")
            {
                ProviderDbType = 0,
                CreateFormat = "binary",
            });
            dts.Add(new DataType("bit varying", "System.Int64")
            {
                ProviderDbType = 0,
                CreateFormat = "bit varying",
            });
            dts.Add(new DataType("bit", "System.Int64")
            {
                ProviderDbType = 0,
                CreateFormat = "bit",
            });
            dts.Add(new DataType("bool", "System.Boolean")
            {
                ProviderDbType = 0,
                CreateFormat = "bool",
            });
            dts.Add(new DataType("boolean", "System.Boolean")
            {
                ProviderDbType = 0,
                CreateFormat = "boolean",
            });
            dts.Add(new DataType("bpchar", "System.String")
            {
                ProviderDbType = 0,
                CreateFormat = "bpchar",
            });
            dts.Add(new DataType("bytea", "System.Byte[]")
            {
                ProviderDbType = 0,
                CreateFormat = "bytea",
            });
            dts.Add(new DataType("char", "System.String")
            {
                ProviderDbType = 0,
                CreateFormat = "char({0})",
            });
            dts.Add(new DataType("character varying", "System.String")
            {
                ProviderDbType = 0,
                CreateFormat = "character varying({0})",
            });
            dts.Add(new DataType("character", "System.String")
            {
                ProviderDbType = 0,
                CreateFormat = "character({0})",
            });
            dts.Add(new DataType("date", "System.DateTime")
            {
                ProviderDbType = 0,
                CreateFormat = "date",
            });
            dts.Add(new DataType("dec", "System.Decimal")
            {
                ProviderDbType = 0,
                CreateFormat = "dec",
            });
            dts.Add(new DataType("decimal", "System.Decimal")
            {
                ProviderDbType = 0,
                CreateFormat = "decimal({0},{1})",
            });
            dts.Add(new DataType("double precision", "System.Double")
            {
                ProviderDbType = 0,
                CreateFormat = "double precision",
            });
            dts.Add(new DataType("double", "System.Double")
            {
                ProviderDbType = 0,
                CreateFormat = "double",
            });
            dts.Add(new DataType("float", "System.Single")
            {
                ProviderDbType = 0,
                CreateFormat = "float",
            });
            dts.Add(new DataType("float4", "System.Single")
            {
                ProviderDbType = 0,
                CreateFormat = "float4",
            });
            dts.Add(new DataType("float8", "System.Double")
            {
                ProviderDbType = 0,
                CreateFormat = "float8",
            });
            dts.Add(new DataType("int", "System.Int32")
            {
                ProviderDbType = 0,
                CreateFormat = "int",
            });
            dts.Add(new DataType("int2", "System.Int16")
            {
                ProviderDbType = 0,
                CreateFormat = "int2",
            });
            dts.Add(new DataType("int4", "System.Int32")
            {
                ProviderDbType = 0,
                CreateFormat = "int4",
            });
            dts.Add(new DataType("int8", "System.Int64")
            {
                ProviderDbType = 0,
                CreateFormat = "int8",
            });
            dts.Add(new DataType("integer", "System.Int32")
            {
                ProviderDbType = 0,
                CreateFormat = "integer",
            });
            dts.Add(new DataType("interval", "System.TimeSpan")
            {
                ProviderDbType = 0,
                CreateFormat = "interval",
            });
            dts.Add(new DataType("line", "System.String")
            {
                ProviderDbType = 0,
                CreateFormat = "line",
            });
            dts.Add(new DataType("money", "System.Double")
            {
                ProviderDbType = 0,
                CreateFormat = "money",
            });
            dts.Add(new DataType("numeric", "System.Decimal")
            {
                ProviderDbType = 0,
                CreateFormat = "numeric",
            });
            dts.Add(new DataType("real", "System.Single")
            {
                ProviderDbType = 0,
                CreateFormat = "real",
            });
            dts.Add(new DataType("serial", "System.Int32")
            {
                ProviderDbType = 0,
                CreateFormat = "serial",
            });
            dts.Add(new DataType("serial4", "System.Int32")
            {
                ProviderDbType = 0,
                CreateFormat = "serial4",
            });
            dts.Add(new DataType("serial8", "System.Int64")
            {
                ProviderDbType = 0,
                CreateFormat = "serial8",
            });
            dts.Add(new DataType("smallint", "System.Int16")
            {
                ProviderDbType = 0,
                CreateFormat = "smallint",
            });
            dts.Add(new DataType("text", "System.String")
            {
                ProviderDbType = 0,
                CreateFormat = "text",
            });
            dts.Add(new DataType("time", "System.TimeSpan")
            {
                ProviderDbType = 0,
                CreateFormat = "time",
            });
            dts.Add(new DataType("timestamp", "System.DateTime")
            {
                ProviderDbType = 0,
                CreateFormat = "timestamp",
            });
            dts.Add(new DataType("timestamptz", "System.DateTime")
            {
                ProviderDbType = 0,
                CreateFormat = "timestamptz",
            });
            dts.Add(new DataType("timetz", "System.TimeSpan")
            {
                ProviderDbType = 0,
                CreateFormat = "timetz",
            });
            dts.Add(new DataType("uuid", "System.Guid")
            {
                ProviderDbType = 0,
                CreateFormat = "uuid",
            });
            dts.Add(new DataType("varbit", "System.Int64")
            {
                ProviderDbType = 0,
                CreateFormat = "varbit",
            });
            dts.Add(new DataType("varchar", "System.String")
            {
                ProviderDbType = 0,
                CreateFormat = "varchar({0})",
            });
            return dts;
        }
    }
}
