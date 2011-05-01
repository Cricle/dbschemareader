﻿using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.SqlGen
{
    /// <summary>
    /// Generate Ddl
    /// </summary>
    public class DdlGeneratorFactory
    {
        private readonly SqlType _sqlType;

        /// <summary>
        /// Initializes a new instance of the <see cref="DdlGeneratorFactory"/> class.
        /// </summary>
        /// <param name="sqlType">Type of the SQL.</param>
        public DdlGeneratorFactory(SqlType sqlType)
        {
            _sqlType = sqlType;
        }

        /// <summary>
        /// Creates a table DDL generator.
        /// </summary>
        /// <param name="table">The table.</param>
        /// <returns></returns>
        public ITableGenerator TableGenerator(DatabaseTable table)
        {
            switch (_sqlType)
            {
                case SqlType.SqlServer:
                    return new SqlServer.TableGenerator(table);
                case SqlType.Oracle:
                    return new Oracle.TableGenerator(table);
                case SqlType.MySql:
                    return new MySql.TableGenerator(table);
                case SqlType.SQLite:
                    return new SqLite.TableGenerator(table);
                case SqlType.SqlServerCe:
                    return new SqlServerCe.TableGenerator(table);
            }
            return null;
        }

        /// <summary>
        /// Creates a DDL generator for all tables.
        /// </summary>
        /// <param name="schema">The schema.</param>
        /// <returns></returns>
        public ITablesGenerator AllTablesGenerator(DatabaseSchema schema)
        {
            switch (_sqlType)
            {
                case SqlType.SqlServer:
                    return new SqlServer.TablesGenerator(schema);
                case SqlType.Oracle:
                    return new Oracle.TablesGenerator(schema);
                case SqlType.MySql:
                    return new MySql.TablesGenerator(schema);
                case SqlType.SQLite:
                    return new SqLite.TablesGenerator(schema);
                case SqlType.SqlServerCe:
                    return new SqlServerCe.TablesGenerator(schema);
            }
            return null;
        }

        /// <summary>
        /// Creates a stored procedure generator for a table.
        /// </summary>
        /// <param name="table">The table.</param>
        /// <returns></returns>
        public IProcedureGenerator ProcedureGenerator(DatabaseTable table)
        {
            switch (_sqlType)
            {
                case SqlType.SqlServer:
                    return new SqlServer.ProcedureGenerator(table);
                case SqlType.Oracle:
                    return new Oracle.ProcedureGenerator(table);
                case SqlType.MySql:
                    return new MySql.ProcedureGenerator(table);
                case SqlType.SQLite:
                    return null; //no stored procedures in SqlLite
                case SqlType.SqlServerCe:
                    return null; //no stored procedures in SqlServerCE
            }
            return null;
        }

        /// <summary>
        /// Creates a migration generator (Create Tables, add/alter/drop columns)
        /// </summary>
        /// <returns></returns>
        public IMigrationGenerator MigrationGenerator()
        {
            switch (_sqlType)
            {
                case SqlType.SqlServer:
                    return new SqlServer.SqlServerMigrationGenerator();
                case SqlType.Oracle:
                    return new MigrationGenerator(SqlType.Oracle);
                case SqlType.MySql:
                    return new MySql.MySqlMigrationGenerator();
                case SqlType.SQLite:
                    return new SqLite.SqLiteMigrationGenerator();
                case SqlType.SqlServerCe:
                    return new SqlServer.SqlServerMigrationGenerator();
            }
            return null;
        }
    }
}
