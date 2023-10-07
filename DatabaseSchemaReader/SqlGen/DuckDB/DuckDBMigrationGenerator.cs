using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.Utilities;
using System;
using System.Globalization;
using System.Text;

namespace DatabaseSchemaReader.SqlGen.DuckDB
{
    class DuckDBMigrationGenerator : MigrationGenerator
    {
        public DuckDBMigrationGenerator()
            : base( SqlType.DuckdDB)
        {
        }
        protected override ISqlFormatProvider SqlFormatProvider()
        {
            return new SqlFormatProvider();
        }

        public override string DropTable(DatabaseTable databaseTable)
        {
            //automatically removes fk references
            return "DROP TABLE " + Escape(databaseTable.Name) + ";";
        }
        public override string AddConstraint(DatabaseTable databaseTable, DatabaseConstraint constraint)
        {
            return null;
        }
        public override string DropConstraint(DatabaseTable databaseTable, DatabaseConstraint constraint)
        {
            return null;
        }
        public override string AddFunction(DatabaseFunction databaseFunction)
        {
            return null;
        }
        public override string DropFunction(DatabaseFunction databaseFunction)
        {
            return null;
        }
        public override string DropProcedure(DatabaseStoredProcedure procedure)
        {
            return null;
        }
        public override string AddProcedure(DatabaseStoredProcedure procedure)
        {
            return null;
        }
        public override string AddTrigger(DatabaseTable databaseTable, DatabaseTrigger trigger)
        {
            return null;
        }
        protected override string DropTriggerFormat => null;

        public override string AddIndex(DatabaseTable databaseTable, DatabaseIndex index)
        {
            if (index.Name.StartsWith("sqlite_autoindex_"))
            {
                return "--skipping reserved index: " + index.Name;
            }
            return base.AddIndex(databaseTable, index);
        }
        public override string DropIndex(DatabaseTable databaseTable, DatabaseIndex index)
        {
            //no "ON table" syntax
            return string.Format(CultureInfo.InvariantCulture,
                "DROP INDEX {0}{1};",
                SchemaPrefix(index.SchemaOwner),
                Escape(index.Name));
        }
        public override string DropDefault(DatabaseTable databaseTable, DatabaseColumn databaseColumn)
        {
            return null;
        }
        public override string RenameColumn(DatabaseTable databaseTable, DatabaseColumn databaseColumn, string originalColumnName)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "ALTER TABLE {0} RENAME COLUMN {1} TO {2};",
                Escape(databaseTable.Name),
                Escape(originalColumnName),
                Escape(databaseColumn.Name));
        }
        public override string RenameTable(DatabaseTable databaseTable, string originalTableName)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "ALTER TABLE {0} RENAME TO {1};",
                Escape(databaseTable.Name),
                Escape(originalTableName));
        }
        public override string AlterColumn(DatabaseTable databaseTable, DatabaseColumn databaseColumn, DatabaseColumn originalColumn)
        {
            var tableGenerator = CreateTableGenerator(databaseTable);
            if (!AlterColumnIncludeDefaultValue)
            {
                tableGenerator.IncludeDefaultValues = false;
            }
            var columnDefinition = tableGenerator.WriteColumn(databaseColumn).Trim();
            var columnDefinitionDataTypeOnly = tableGenerator.WriteDataType(databaseColumn).Trim();
            var originalDefinition = "?";
            var originalDefinitionDataTypeOnly = "?";
            if (originalColumn != null)
            {
                originalDefinition = tableGenerator.WriteColumn(originalColumn).Trim();
                originalDefinitionDataTypeOnly = tableGenerator.WriteDataType(originalColumn).Trim();
                //we don't specify "NULL" for nullables in tableGenerator, but if it's changed we should
                if (originalColumn.Nullable && !databaseColumn.Nullable)
                {
                    originalDefinition += " NULL";
                }
                if (!originalColumn.Nullable && databaseColumn.Nullable)
                {
                    columnDefinition += " NULL";
                }
            }
            //add a nice comment
            var comment = string.Format(CultureInfo.InvariantCulture,
                "-- {0} from {1} to {2}",
                databaseTable.Name,
                originalDefinition,
                columnDefinition);
            if (columnDefinitionDataTypeOnly.Equals(originalDefinitionDataTypeOnly))
            {
                var renameWithTypeSkip = GenerateRename(databaseTable, databaseColumn, originalColumn);

                //most likely faulty sql db structure, skip
                return "-- skipping alter of " + comment.Substring(2) + Environment.NewLine + renameWithTypeSkip;
            }
            if (!SupportsAlterColumn)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(string.Format("--TO CHANGE COLUMN {0} to {1}, WE CREATE BACKUP, MOVE CONTENT AND RENAME TABLE " + LineEnding(), originalDefinition, columnDefinition));

                DatabaseTable tempTable = databaseTable.Clone("bkup1903_" + databaseTable.Name);
                tempTable.Columns.Remove(tempTable.FindColumn(originalColumn == null ? databaseColumn.Name : originalColumn.Name));
                tempTable.Columns.Add(databaseColumn);
                sb.Append(BackupAndUpdateTable(databaseTable, tempTable));
                return sb.ToString();
            }
            if (databaseColumn.IsPrimaryKey || databaseColumn.IsForeignKey)
            {
                //you can't change primary keys
                //you can't change foreign key columns
                return comment + Environment.NewLine + "-- TODO: change manually (PK or FK)";
            }

            //there are practical restrictions on what can be altered
            //* changing null to not null will fail if the table column data contains nulls
            //* you can't change between incompatible datatypes
            //* you can't change datatypes if there is a default value (but you can change length/precision/scale)
            //* you can't change datatypes if column used in indexes (incl. primary keys and foreign keys)
            //* and so on...
            //
            var columnDataType= tableGenerator.WriteDataType(databaseColumn).Trim();
            var alter = string.Format(CultureInfo.InvariantCulture,
                    "ALTER TABLE {0} ALTER COLUMN {1} TYPE {2};",
                    TableName(databaseTable),
                    Escape(databaseColumn.Name),
                    columnDataType);
            var rename = GenerateRename(databaseTable, databaseColumn, originalColumn);
            return string.Join(Environment.NewLine, rename, comment, alter);
        }

    }
}
