using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.ProviderSchemaReaders.Databases.DuckDB;
using System.Collections.Generic;
using System.Data;

namespace DatabaseSchemaReader.SqlGen.DuckDB
{
    class DuckDBDataTypeMapper : DataTypeMapper
    {
        private readonly IDictionary<DbType, string> _mapping = new Dictionary<DbType, string>();

        public override IList<DataType> DataTypes { get; } = new DataTypeList().Execute();

        public DuckDBDataTypeMapper()
        {
            Init();
        }
        private void Init()
        {
            _mapping.Add(DbType.AnsiStringFixedLength, "VARCHAR");
            _mapping.Add(DbType.AnsiString, "VARCHAR");
            _mapping.Add(DbType.Binary, "BLOB");
            _mapping.Add(DbType.Boolean, "BOOLEAN");
            _mapping.Add(DbType.Byte, "TINYINT");
            _mapping.Add(DbType.Currency, "DECIMAL");
            _mapping.Add(DbType.Date, "TIMESTAMP");
            _mapping.Add(DbType.DateTime, "TIMESTAMP");
            _mapping.Add(DbType.DateTime2, "TIMESTAMP");
            _mapping.Add(DbType.DateTimeOffset, "TIMESTAMP");
            _mapping.Add(DbType.Decimal, "DECIMAL");
            _mapping.Add(DbType.Double, "DOUBLE");
            _mapping.Add(DbType.Guid, "UUID");
            _mapping.Add(DbType.Int16, "SMALLINT");
            _mapping.Add(DbType.Int32, "INTEGER");
            _mapping.Add(DbType.Int64, "BIGINT");
            _mapping.Add(DbType.Single, "REAL");
            _mapping.Add(DbType.StringFixedLength, "VARCHAR");
            _mapping.Add(DbType.String, "VARCHAR");
            _mapping.Add(DbType.Time, "TIMESTAMP");
            _mapping.Add(DbType.Xml, "VARCHAR");

        }

        public override string Map(DbType dbType)
        {
            if (_mapping.ContainsKey(dbType))
                return _mapping[dbType];
            return null;
        }
    }
}
