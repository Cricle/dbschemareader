﻿using System;
using System.Collections.Generic;
using System.Linq;
using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.Utilities;

namespace DatabaseSchemaReader.Compare
{
    class CompareColumns
    {
        private readonly IList<CompareResult> _results;
        private readonly ComparisonWriter _writer;

        public bool EnableDefaultCompare { get; set; }

        public CompareColumns(IList<CompareResult> results, ComparisonWriter writer)
        {
            _results = results;
            _writer = writer;
        }

        public void Execute(DatabaseTable baseTable, DatabaseTable compareTable)
        {
            //find new columns (in compare, but not in base)
            var copy = baseTable.Clone();
            foreach (var column in compareTable.Columns)
            {
                var name = column.Name;
                var match = baseTable.Columns.FirstOrDefault(t => t.Name == name);
                if (match != null) continue;
                // Check the column id has any in base table
                if (column.Id!=null&&baseTable.Columns.Any(x=>Equals( x.Id,column.Id))) continue;
                var script = "-- ADDED TABLE " + column.TableName + " COLUMN " + name + Environment.NewLine +
                 _writer.AddColumn(compareTable, column);
                copy.AddColumn(column);
                CreateResult(ResultType.Add, baseTable, name, script);
            }

            //find dropped and existing columns
            var toDrop = new Dictionary<string, DatabaseColumn>();
            var toAlter = new Dictionary<string, DatabaseColumn[]>();
            foreach (var column in baseTable.Columns)
            {
                var name = column.Name;
                var match = compareTable.Columns.FirstOrDefault(t => t.Name == name);
                if (match == null)
                {
                    if (column.Id != null)
                    {
                        match = compareTable.Columns.FirstOrDefault(t => Equals(t.Id , column.Id));
                        if (match == null)
                        {
                            toDrop.Add(name, column);
                            continue;
                        }
                    }
                    else
                    {
                        toDrop.Add(name, column);
                        continue;
                    }
                }

                //has column changed?
                var sourceColumn = column.DbDataType;
                var destColumn = match.DbDataType;
                if (sourceColumn=="int4")
                {
                    sourceColumn = "integer";//pg SERIAL
                }
                else if (sourceColumn=="int8")
                {
                    sourceColumn = "bigint";//pg int8
                }
                if (string.Equals(sourceColumn,"int4", StringComparison.OrdinalIgnoreCase)||
                    string.Equals(sourceColumn, "int8", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(sourceColumn, "integer", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(sourceColumn, "bigint", StringComparison.OrdinalIgnoreCase))
                {
                    column.Precision = null;
                    column.Scale = null;
                }
                if (destColumn == "int4")
                {
                    destColumn = "integer";//pg int8
                }
                else if (destColumn == "int8")
                {
                    destColumn = "bigint";//pg int8
                }
                if (string.Equals(destColumn, "int4", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(destColumn, "int8", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(destColumn, "integer", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(destColumn, "bigint", StringComparison.OrdinalIgnoreCase))
                {
                    match.Precision = null;
                    match.Scale = null;
                }
                if (EnableDefaultCompare)
                {
                    if (column.DefaultValue != match.DefaultValue?.Trim('\''))
                    {
                        toAlter.Add(name, new[] { match, column });
                        continue;
                    }
                }
                if (string.Equals(sourceColumn, destColumn, StringComparison.OrdinalIgnoreCase) && column.Nullable == match.Nullable)
                {
                    if ((!column.DataType.IsString || column.Length == match.Length) &&
                        (column.DataType.NetDataType != typeof(decimal).FullName || (column.Precision == match.Precision && column.Scale == match.Scale)))
                    {
                        if (column.Id == null || match.Id == null ||
                            column.Id != match.Id)
                        {
                            if (column.Name == match.Name)
                            {
                                //we don't check IDENTITY
                                continue; //the same, no action
                            }
                        }
                    }
                }
                toAlter.Add(name, new[] { match, column });
            }

            //write drops and alters as last step to ensure valid queries
            foreach (var kv in toAlter)
            {
                copy.Columns.Remove(kv.Value[1]);
                copy.Columns.Add(kv.Value[0]);
                CreateResult(ResultType.Change, baseTable, kv.Key,
                    _writer.AlterColumn(copy, kv.Value[0], kv.Value[1]));
            }

            foreach (var kv in toDrop)
            {
                copy.Columns.Remove(kv.Value);
                CreateResult(ResultType.Delete, baseTable, kv.Key,
                    _writer.DropColumn(copy, kv.Value));
            }
        }


        private void CreateResult(ResultType resultType, DatabaseTable table, string name, string script)
        {
            var result = new CompareResult
            {
                SchemaObjectType = SchemaObjectType.Column,
                ResultType = resultType,
                TableName = table.Name,
                SchemaOwner = table.SchemaOwner,
                Name = name,
                Script = script
            };
            _results.Add(result);
        }
    }
}