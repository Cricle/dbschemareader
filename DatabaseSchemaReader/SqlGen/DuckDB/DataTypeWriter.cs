using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.ProviderSchemaReaders.Databases.DuckDB;
using System;

namespace DatabaseSchemaReader.SqlGen.DuckDB
{
    class DataTypeWriter : IDataTypeWriter
    {
        public string WriteDataType(DatabaseColumn column)
        {
            //sqlite is not strongly typed, and the type affinities (http://www.sqlite.org/datatype3.html) are very limited
            // (text, integer, real, blob)
            //the ado provider uses the column types for richer support
            //ado mapping http://sqlite.phxsoftware.com/forums/t/31.aspx

            if (column == null) return string.Empty;
            var dt = column.DataType;
            if (dt != null)
            {
                if (dt.IsString)
                    return "VARCHAR";
                if (dt.IsInt)
                    return "INTEGER";
                if (dt.NetDataType == "System.Int64")
                    return "BIGINT";
                if (dt.NetDataType == typeof(short).FullName)
                    return "SMALLINT";
                if (dt.NetDataType == typeof(Guid).FullName)
                    return "UUID";
                if (dt.IsNumeric)
                {
                    if (column.Precision==null&&column.Scale==null)
                    {
                        return "DOUBLE";
                    }
                    var val = column.Precision ?? column.Scale;
                    return $"DECIMAL({Math.Min(38, (column.Precision ?? val).Value)},{Math.Min(38, (column.Scale ?? val).Value)})";
                }
                if (dt.IsFloat)
                    return "FLOAT";
            }
            if (string.IsNullOrEmpty(column.DbDataType)) return string.Empty;
            var dataType = column.DbDataTypeStandard();

            if (dataType == "IMAGE" || dataType.IndexOf("BINARY", StringComparison.OrdinalIgnoreCase) != -1)
            {
                return "BLOB";
            }
            if (dataType == "DATE" || dataType == "DATETIME")
            {
                //a hint to the ado provider
                return "TIMESTAMP";
            }


            return dataType;
        }

    }
}
