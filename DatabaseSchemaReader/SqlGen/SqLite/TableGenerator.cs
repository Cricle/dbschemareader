using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.SqlGen.SqLite
{
    class TableGenerator : TableGeneratorBase
    {
        public TableGenerator(DatabaseTable table)
            : base(table)
        {
        }

        #region Overrides of TableGeneratorBase

        protected override ISqlFormatProvider SqlFormatProvider()
        {
            
            return new SqlFormatProvider();
        }

        public override string WriteDataType(DatabaseColumn column)
        {
            var type = new DataTypeWriter().WriteDataType(column);
            if (column.IsPrimaryKey && (Table.PrimaryKey == null || Table.PrimaryKey.Columns.Count == 1))
            {
                type += " PRIMARY KEY";
                if (column.IsAutoNumber) //must be integer primary key
                {
                    //an sqlite auto increment may be nullable
                    return "INTEGER PRIMARY KEY AUTOINCREMENT" + (!column.Nullable ? " NOT NULL" : "");
                }
            }
            if (!column.Nullable) type += " NOT NULL";
            //if there's a default value, and it's not a guid generator or autonumber
            if (!string.IsNullOrEmpty(column.DefaultValue) &&
                !SqlTranslator.IsGuidGenerator(column.DefaultValue) &&
                !column.IsAutoNumber)
            {
                var value = SqlTranslator.Fix(column.DefaultValue);
                //SqlServer (N'string') format
                if (value.StartsWith("(N'", StringComparison.OrdinalIgnoreCase))
                    value = value.Replace("(N'", "('");
                type += " DEFAULT " + value;
            }

            return type;
        }

        protected override void AddTableConstraints(IList<string> columnList)
        {
            var formatter = SqlFormatProvider();
            if (Table.PrimaryKey != null && Table.PrimaryKey.Columns.Count > 1)
            {
                columnList.Add("PRIMARY KEY (" + GetColumnList(Table.PrimaryKey.Columns) + ")");
            }
            foreach (var uniqueKey in Table.UniqueKeys)
            {
                columnList.Add("UNIQUE (" + GetColumnList(uniqueKey.Columns) + ")");
            }
            foreach (var checkConstraint in Table.CheckConstraints)
            {
                var expression = SqlTranslator.Fix(checkConstraint.Expression);
                //nothing to write?
                if (string.IsNullOrEmpty(expression)) continue;

                //check if Access and reformat
                if (checkConstraint.Name.Contains("]."))
                {
                    //access format names [table].[column].ValidationRule
                    //access expression doesn't have column name so take it from constraint name
                    var columnName = checkConstraint.Name.Substring(0, checkConstraint.Name.LastIndexOf("].", System.StringComparison.Ordinal) + 1)
                        .Replace("[" + Table.Name + "].", "");
                    //must have braces
                    expression = "(" + columnName + " " + expression + ")";
                }

                columnList.Add("CHECK " + expression);
            }

            //http://www.sqlite.org/foreignkeys.html These aren't enabled by default.
            foreach (var foreignKey in Table.ForeignKeys)
            {
                var referencedTable = foreignKey.ReferencedTable(Table.DatabaseSchema);
                //can't find the table. Don't write the fk reference.
                if (referencedTable == null) continue;
                string refColumnList;
                if (referencedTable.PrimaryKey == null && referencedTable.PrimaryKeyColumn != null)
                {
                    refColumnList = referencedTable.PrimaryKeyColumn.Name;
                }
                else if (referencedTable.PrimaryKey == null)
                {
                    continue; //can't find the primary key
                }
                else
                {
                    refColumnList = GetColumnList(referencedTable.PrimaryKey.Columns);
                }

                columnList.Add(string.Format(CultureInfo.InvariantCulture,
                    "FOREIGN KEY ({0}) REFERENCES {1} ({2})",
                    GetColumnList(foreignKey.Columns),
                    formatter.Escape(foreignKey.RefersToTable),
                    refColumnList));
            }
        }

        protected override string NonNativeAutoIncrementWriter()
        {
            return string.Empty;
        }

        protected override string ConstraintWriter()
        {
            var formatter = SqlFormatProvider();
            var s = new StringBuilder();
            var createdIndexs = new HashSet<string>();
            foreach (var item in Table.UniqueKeys)
            {
                if (createdIndexs.Add(item.Name))
                {
                    s.AppendFormat("DROP INDEX IF EXISTS {0};", formatter.Escape(item.Name));
                    s.AppendLine();
                    s.AppendFormat("CREATE UNIQUE INDEX {0} ON {1}({2});",
                        formatter.Escape(item.Name),
                        formatter.Escape(item.TableName),
                        string.Join(",", item.Columns.Select(x=>formatter.Escape(x))));
                    s.AppendLine();
                }
            }
            foreach (var item in Table.Indexes)
            {
                if (createdIndexs.Add(item.Name))
                {
                    s.AppendFormat("DROP INDEX IF EXISTS {0};", formatter.Escape(item.Name));
                    s.AppendLine();
                    s.AppendFormat("CREATE INDEX {0} ON {1}({2});",
                        formatter.Escape(item.Name),
                        formatter.Escape(item.TableName),
                        string.Join(",", item.Columns.Select((x, i) => 
                        {
                            var isDesc = item.ColumnOrderDescs.Count > i && item.ColumnOrderDescs[i];
                            var name = formatter.Escape(x.Name);
                            if (isDesc)
                            {
                                name += " DESC";
                            }
                            return name;
                        })));
                    s.AppendLine();
                }
            }
            return s.ToString();
        }

        #endregion

        private string GetColumnList(IEnumerable<string> columns)
        {
            var escapedColumnNames = columns.Select(column => SqlFormatProvider().Escape(column)).ToArray();
            return string.Join(", ", escapedColumnNames);
        }
    }
}
