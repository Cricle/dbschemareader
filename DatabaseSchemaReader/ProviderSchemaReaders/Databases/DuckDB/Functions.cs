using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.ProviderSchemaReaders.ConnectionContext;

namespace DatabaseSchemaReader.ProviderSchemaReaders.Databases.DuckDB
{
    internal class Functions : SqlExecuter<DatabaseFunction>
    {
        public Functions(int? commandTimeout, string owner) : base(commandTimeout, owner)
        {
            Owner = owner;
            Sql = @"SELECT function_name,macro_definition,parameters,parameter_types,return_type FROM duckdb_functions() WHERE (database_name = $DATBASE_NAME OR ($DATBASE_NAME IS NULL)) AND database_name!='system' AND database_name!='temp'";
        }

        public IList<DatabaseFunction> Execute(IConnectionAdapter connectionAdapter)
        {
            try
            {
                ExecuteDbReader(connectionAdapter);
            }
            catch (DbException ex)
            {
                System.Diagnostics.Trace.WriteLine("Error reading PostgreSql functions " + ex.Message);
            }
            return Result;
        }

        protected override void AddParameters(DbCommand command)
        {
            AddDbParameter(command, "DATBASE_NAME", Owner);
        }

        protected override void Mapper(IDataRecord record)
        {
            var owner = Owner;
            var name = record.GetString("function_name");
            var sql = record.GetString("macro_definition");
            var parStr = record.GetString("parameters");
            var parTypeStr = record.GetString("parameter_types");
            var pars = parStr.Substring(1, parStr.Length - 2).Split(',');
            var parTypes = parTypeStr.Substring(1, parStr.Length - 2).Split(',');
            var sproc = new DatabaseFunction
            {
                SchemaOwner = owner,
                Name = name,
                Sql = sql,
                Language = "duck",
                ReturnType = record.GetString("return_type"),
            };
            for (int i = 0; i < pars.Length; i++)
            {
                var par= pars[i];
                sproc.Arguments.Add(new DatabaseArgument
                {
                    DatabaseDataType = parTypes[i],
                    In = true,
                    Name = par,
                    SchemaOwner = owner
                });
            }
            Result.Add(sproc);
        }
    }
}
