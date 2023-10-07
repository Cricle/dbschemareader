using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text.RegularExpressions;
using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.ProviderSchemaReaders.ConnectionContext;

namespace DatabaseSchemaReader.ProviderSchemaReaders.Databases.DuckDB
{
    class Indexes : SqlExecuter<DatabaseIndex>
    {
        private readonly string _tableName;

        public Indexes(int? commandTimeout, string tableName) : base(commandTimeout, null)
        {
            _tableName = tableName;
            Sql = @"SELECT
  name,
  tbl_name,
  sql
FROM sqlite_master
WHERE type = 'index'
AND (tbl_name = $TABLE_NAME OR ($TABLE_NAME IS NULL))
ORDER BY tbl_name, name";
            PragmaSql = @"SELECT tablename AS tbl_name,indexname AS name,indexdef AS def FROM pg_indexes WHERE name = '{0}';";
        }

        public string PragmaSql { get; set; }

        protected override void AddParameters(DbCommand command)
        {
            AddDbParameter(command, "TABLE_NAME", _tableName);
        }
        struct IndexDesc
        {
            public bool Desc;
            public string FieldName;

            public IndexDesc(bool desc, string fieldName)
            {
                Desc = desc;
                FieldName = fieldName;
            }
        }
        private List<IndexDesc> ParseIndexs(string sql)
        {
            var idxs = new List<IndexDesc>();
            var leftQuto = sql.IndexOf('(');
            var rightQuto = sql.IndexOf(')');
            var fields = sql.Substring(leftQuto+1, rightQuto - leftQuto-1).Split(',');
            foreach ( var field in fields)
            {
                var desc = field.Trim().EndsWith("DESC", StringComparison.OrdinalIgnoreCase);
                var leftM = field.IndexOf('\"');
                var rightM = field.IndexOf('\"',leftM+1);
                var fieldName = field.Substring(leftM+1, rightM - leftM-1);
                idxs.Add(new IndexDesc(desc, fieldName));
            }
            return idxs;
        }
        protected override void Mapper(IDataRecord record)
        {
            var tableName = record.GetString("tbl_name");
            var name = record.GetString("name");
            var index = Result.FirstOrDefault(f => f.Name == name && f.TableName.Equals(tableName, StringComparison.OrdinalIgnoreCase));
            if (index == null)
            {
                index = new DatabaseIndex
                {
                    SchemaOwner = "main",
                    TableName = tableName,
                    Name = name,
                };
                Result.Add(index);
            }
        }
        public IList<DatabaseIndex> Execute(IConnectionAdapter connectionAdapter)
        {
            ExecuteDbReader(connectionAdapter);

            foreach (var index in Result)
            {
                var name = index.Name;
                using (var cmd = BuildCommand(connectionAdapter))
                {
                    cmd.CommandText = string.Format(PragmaSql, name);
                    int ordinal = 0;
                    using (var dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            var res = ParseIndexs(dr.GetString("def"));
                            foreach (var item in res)
                            {
                                var col = new DatabaseColumn
                                {
                                    Name = item.FieldName,
                                    SchemaOwner = "main",
                                    Ordinal = ordinal,
                                };
                                index.ColumnOrderDescs.Add(item.Desc);
                                index.Columns.Add(col);
                                ordinal++;
                            }
                        }
                    }
                }
            }

            return Result;
        }
    }
}