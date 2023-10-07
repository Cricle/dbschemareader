using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.ProviderSchemaReaders.Databases.DuckDB;
using System.Collections.Generic;

namespace DatabaseSchemaReader.ProviderSchemaReaders.Adapters
{
    class DuckDBAdapter : ReaderAdapter
    {
        public DuckDBAdapter(SchemaParameters schemaParameters) 
            : base(schemaParameters)
        {
        }
        public override IList<DataType> DataTypes()
        {
            return new DataTypeList().Execute();
        }
        public override IList<DatabaseTable> Tables(string tableName)
        {
            return new Tables(CommandTimeout, tableName).Execute(ConnectionAdapter);
        }
        public override IList<DatabaseColumn> Columns(string tableName)
        {
            return new Columns(CommandTimeout, tableName).Execute(ConnectionAdapter);
        }
        public override IList<DatabaseIndex> Indexes(string tableName)
        {
            return new Indexes(CommandTimeout, tableName).Execute(ConnectionAdapter);
        }
        public override IList<DatabaseConstraint> PrimaryKeys(string tableName)
        {
            return new PkConstraints(CommandTimeout, tableName).Execute(ConnectionAdapter);
        }
        public override IList<DatabaseView> Views(string viewName)
        {
            return new Views(CommandTimeout, viewName).Execute(ConnectionAdapter);
        }
        public override IList<DatabaseColumn> ViewColumns(string viewName)
        {
            return new ViewColumns(CommandTimeout, viewName).Execute(ConnectionAdapter);
        }
    }
}
