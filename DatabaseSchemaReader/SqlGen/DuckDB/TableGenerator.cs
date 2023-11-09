using DatabaseSchemaReader.DataSchema;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace DatabaseSchemaReader.SqlGen.DuckDB
{
    class TableGenerator : TableGeneratorBase
    {
        public TableGenerator(DatabaseTable table)
            : base(table)
        {
        }
        public override string Write()
        {
            if (Table.HasAutoNumberColumn)
            {
                return $"CREATE SEQUENCE IF NOT EXISTS 'seq_{Table.Name}';\n"+ base.Write();
            }
            return base.Write();
        }
        public override string WriteDataType(DatabaseColumn column)
        {
            var type = new DataTypeWriter().WriteDataType(column);
            if (column.IsPrimaryKey && (Table.PrimaryKey == null || Table.PrimaryKey.Columns.Count == 1))
            {
                type += " PRIMARY KEY";
                if (column.IsAutoNumber) //must be integer primary key
                {
                    var typeName = column.DataType.TypeName;
                    //an sqlite auto increment may be nullable
                    type= $"{typeName} PRIMARY KEY DEFAULT NEXTVAL('seq_{Table.Name}')";
                    if (column.Nullable)
                    {
                        type += " NULL ";
                    }
                    else
                    {
                        type += " NOT NULL ";
                    }
                    return type;
                }
            }
            //if there's a default value, and it's not a guid generator or autonumber
            if (!string.IsNullOrEmpty(column.DefaultValue) &&
                !SqlTranslator.IsGuidGenerator(column.DefaultValue) &&
                !column.IsAutoNumber)
            {
                var value = SqlTranslator.Fix(column.DefaultValue.Replace("::regclass)",")"));
                //SqlServer (N'string') format
                if (value.StartsWith("(N'", StringComparison.OrdinalIgnoreCase))
                    value = value.Replace("(N'", "('");
                type += " DEFAULT " + value;
            }
            if (column.Nullable)
            {
                type += " NULL ";
            }
            else
            {
                type += " NOT NULL ";
            }
            return type;
        }

        protected override string ConstraintWriter()
        {
            return string.Empty;
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
                if (checkConstraint.Name.Contains("\"."))
                {
                    //access format names [table].[column].ValidationRule
                    //access expression doesn't have column name so take it from constraint name
                    var columnName = checkConstraint.Name.Substring(0, checkConstraint.Name.LastIndexOf("].", System.StringComparison.Ordinal) + 1)
                        .Replace("\"" + Table.Name + "\".", "");
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

        protected override ISqlFormatProvider SqlFormatProvider()
        {
            return new SqlFormatProvider();
        }
        private string GetColumnList(IEnumerable<string> columns)
        {
            var escapedColumnNames = columns.Select(column => SqlFormatProvider().Escape(column)).ToArray();
            return string.Join(", ", escapedColumnNames);
        }
    }
}
