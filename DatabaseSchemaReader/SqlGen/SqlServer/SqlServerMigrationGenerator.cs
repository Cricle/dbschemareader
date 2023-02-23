﻿using System.Globalization;
using System.Linq;
using System.Text;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.SqlGen.SqlServer
{
    class SqlServerMigrationGenerator : MigrationGenerator
    {
        public SqlServerMigrationGenerator()
            : base(SqlType.SqlServer)
        {
        }

        protected override string AlterColumnFormat
        {
            get { return "ALTER TABLE {0} ALTER COLUMN {1};"; }
        }
        protected override bool AlterColumnIncludeDefaultValue { get { return false; } }

        public override string AlterColumn(DatabaseTable databaseTable, DatabaseColumn databaseColumn, DatabaseColumn originalColumn)
        {
            var sb = new StringBuilder();
            var defaultName = "DF_" + databaseTable.Name + "_" + databaseColumn.Name;
            if (originalColumn != null)
            {
                if (originalColumn.DefaultValue != null)
                {
                    //have to drop default contraint
                    var df = FindDefaultConstraint(databaseTable, databaseColumn.Name);
                    if (df != null)
                    {
                        defaultName = df.Name;
                        sb.AppendLine("ALTER TABLE " + TableName(databaseTable)
                                      + " DROP CONSTRAINT " + Escape(defaultName) + ";");
                    }
                }
            }
            //we could check if any of the properties are changed here
            sb.AppendLine(base.AlterColumn(databaseTable, databaseColumn, originalColumn));
            if (databaseColumn.DefaultValue != null)
            {
                //add default contraint
                sb.AppendLine("ALTER TABLE " + TableName(databaseTable) +
                    " ADD CONSTRAINT " + Escape(defaultName) +
                    " DEFAULT " + databaseColumn.DefaultValue +
                    " FOR " + Escape(databaseColumn.Name) + ";");
            }

            return sb.ToString();
        }

        public override string DropColumn(DatabaseTable databaseTable, DatabaseColumn databaseColumn)
        {
            var dropColumn = base.DropColumn(databaseTable, databaseColumn);
            //if has a default constraint, drop that first.
            if (databaseColumn.DefaultValue != null)
            {
                var df = FindDefaultConstraint(databaseTable, databaseColumn.Name);
                if (df != null)
                {
                    var sb = new StringBuilder();
                    sb.AppendFormat(DropForeignKeyFormat, TableName(databaseTable), Escape(df.Name))
                        .AppendLine();
                    sb.AppendLine(dropColumn);
                    dropColumn = sb.ToString();
                }
            }
            return dropColumn;
        }

        protected override string DropForeignKeyFormat
        {
            get { return "ALTER TABLE {0} DROP CONSTRAINT IF EXISTS {1}"; }
        }

        private static DatabaseConstraint FindDefaultConstraint(DatabaseTable databaseTable, string databaseColumnName)
        {
            return databaseTable.DefaultConstraints
                .FirstOrDefault(c => c.Columns.Contains(databaseColumnName));
        }

        public override string DropDefault(DatabaseTable databaseTable, DatabaseColumn databaseColumn)
        {
            //there is no "DROP DEFAULT" in SqlServer (there is in SQLServer CE). 
            //You must use the default constraint name (which is probably autogenerated)
            var sb = new StringBuilder();
            sb.AppendLine("-- drop default for " + databaseColumn.Name);
            var df = FindDefaultConstraint(databaseTable, databaseColumn.Name);
            if (df != null)
            {
                sb.AppendLine("ALTER TABLE " + TableName(databaseTable)
                              + " DROP CONSTRAINT IF EXISTS " + Escape(df.Name) + ";");
            }
            return sb.ToString();
        }
        public override string AddTrigger(DatabaseTable databaseTable, DatabaseTrigger trigger)
        {
            //sqlserver: 
            //CREATE TRIGGER (triggerName) 
            //ON (tableName) 
            //(FOR | AFTER | INSTEAD OF) ( [INSERT ] [ , ] [ UPDATE ] [ , ] [ DELETE ])
            //AS (sql_statement); GO 

            //nicely, SQLServer gives you the entire sql including create statement in TriggerBody
            if (string.IsNullOrEmpty(trigger.TriggerBody))
                return "-- add trigger " + trigger.Name;

            return trigger.TriggerBody + ";";
        }

        public override string RenameColumn(DatabaseTable databaseTable, DatabaseColumn databaseColumn, string originalColumnName)
        {
            if (databaseTable == null) return null;
            if (databaseColumn == null) return null;
            if (string.IsNullOrEmpty(originalColumnName))
                return base.RenameColumn(databaseTable, databaseColumn, originalColumnName);
            var name = TableName(databaseTable) + "." + Escape(originalColumnName);
            return "exec sp_rename '" + name + "', '" + databaseColumn.Name + "', 'COLUMN';";
        }

        public override string RenameTable(DatabaseTable databaseTable, string originalTableName)
        {
            if (databaseTable == null) return null;
            if (string.IsNullOrEmpty(originalTableName))
                return base.RenameTable(databaseTable, originalTableName);
            var name = SchemaPrefix(databaseTable.SchemaOwner) + Escape(originalTableName);
            //#86 @objname is qualified with database so escaped, but the @newname only requires single quote
            return "exec sp_rename '" + name + "', '" + databaseTable.Name + "';";
        }

        public override string DropIndex(DatabaseTable databaseTable, DatabaseIndex index)
        {
            //no schema on index name, only on table
            return string.Format(CultureInfo.InvariantCulture,
                "DROP INDEX IF EXISTS {0} ON {1};",
                Escape(index.Name),
                TableName(databaseTable));
        }

        public override string AddIndex(DatabaseTable databaseTable, DatabaseIndex index)
        {
            if (index.Columns.Count == 0)
            {
                //IndexColumns errors 
                return "-- add index " + index.Name + " (unknown columns)";
            }
            //we could plug in "CLUSTERED" or "PRIMARY XML" from index.IndexType here
            var indexType = index.IsUnique ? "UNIQUE " : string.Empty;

            var clustered = string.IsNullOrEmpty(index.IndexType?.Trim()) ? string.Empty : index.IndexType + " ";

            var format = string.Format(CultureInfo.InvariantCulture,
                "CREATE {0}{4}INDEX {1} ON {2}({3})",
                indexType, //must have trailing space
                Escape(index.Name),
                TableName(databaseTable),
                GetColumnList(index.Columns.Select(i => i.Name)),
                clustered);
            if (!string.IsNullOrEmpty(index.Filter))
            {
                format = $"{format} WHERE {index.Filter}";
            }
            return format + LineEnding();
        }
    }
}
