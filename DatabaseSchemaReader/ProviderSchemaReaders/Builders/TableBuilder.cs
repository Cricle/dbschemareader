using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
// ReSharper disable once RedundantUsingDirective 
using System.Threading;
using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.ProviderSchemaReaders.Adapters;

namespace DatabaseSchemaReader.ProviderSchemaReaders.Builders
{
    class TableBuilder
    {
        private readonly ReaderAdapter _readerAdapter;

        public event EventHandler<ReaderEventArgs> ReaderProgress;

        public ReadTypes ReadType { get; }

        public TableBuilder(ReaderAdapter readerAdapter, ReadTypes readType)
        {
            _readerAdapter = readerAdapter;
            ReadType = readType;
        }

        protected void RaiseReadingProgress(SchemaObjectType schemaObjectType)
        {
            ReaderEventArgs.RaiseEvent(ReaderProgress, this, ProgressType.ReadingSchema, schemaObjectType);
        }

        protected void RaiseProgress(ProgressType progressType,
            SchemaObjectType schemaObjectType,
            string name, int? index, int? count)
        {
            ReaderEventArgs.RaiseEvent(ReaderProgress, this, progressType, schemaObjectType,
    name, index, count);
        }

        private IList<DatabaseTable> EmptyList()
        {
            return new List<DatabaseTable>();
        }

        public DatabaseTable Execute(CancellationToken ct, string tableName)
        {
            if (ct.IsCancellationRequested) return null;

            var tables = _readerAdapter.Tables(tableName);
            if (tables.Count == 0)
            {
                return null;
            }

            tableName = tables.FirstOrDefault()?.Name ?? tableName;

            if (string.IsNullOrEmpty(_readerAdapter.Parameters.Owner))
            {
                var owner = tables[0].SchemaOwner;
                Trace.WriteLine("Using first schema " + owner);
                _readerAdapter.Parameters.Owner = owner;
            }
            IList<DatabaseColumn> columns=Array.Empty<DatabaseColumn>();
            if (ReadType.HasFlag(ReadTypes.Columns) ||
                ReadType.HasFlag(ReadTypes.IdentityColumns) || 
                ReadType.HasFlag(ReadTypes.Computed)||
                ReadType.HasFlag(ReadTypes.Indexs))
            {
                columns = _readerAdapter.Columns(tableName);
            }
            IList<DatabaseColumn> identityColumns = Array.Empty<DatabaseColumn>();
            if (ReadType.HasFlag(ReadTypes.IdentityColumns))
            {
                identityColumns = _readerAdapter.IdentityColumns(tableName);
            }
            IList<DatabaseConstraint> checkConstraints = Array.Empty<DatabaseConstraint>();
            if (ReadType.HasFlag(ReadTypes.IdentityColumns))
            {
                checkConstraints = _readerAdapter.CheckConstraints(tableName);
            }
            IList<DatabaseConstraint> pks = Array.Empty<DatabaseConstraint>();
            if (ReadType.HasFlag(ReadTypes.Pks))
            {
                pks = _readerAdapter.PrimaryKeys(tableName);
            }
            IList<DatabaseConstraint> uks = Array.Empty<DatabaseConstraint>();
            if (ReadType.HasFlag(ReadTypes.Uks))
            {
                uks = _readerAdapter.UniqueKeys(tableName);
            }
            IList<DatabaseConstraint> fks = Array.Empty<DatabaseConstraint>();
            if (ReadType.HasFlag(ReadTypes.Fks))
            {
                fks = _readerAdapter.ForeignKeys(tableName);
            }
            IList<DatabaseConstraint> dfs = Array.Empty<DatabaseConstraint>();
            if (ReadType.HasFlag(ReadTypes.Fks))
            {
                dfs = _readerAdapter.DefaultConstraints(tableName);
            }
            IList<DatabaseTrigger> triggers = Array.Empty<DatabaseTrigger>();
            if (ReadType.HasFlag(ReadTypes.Triggers))
            {
                triggers = _readerAdapter.Triggers(tableName);
            }
            IList<DatabaseTable> tableDescs = Array.Empty<DatabaseTable>();
            if (ReadType.HasFlag(ReadTypes.TableDescs))
            {
                tableDescs = _readerAdapter.TableDescriptions(tableName);
            }
            IList<DatabaseTable> colDescs = Array.Empty<DatabaseTable>();
            if (ReadType.HasFlag(ReadTypes.TableDescs))
            {
                colDescs = _readerAdapter.ColumnDescriptions(tableName);
            }
            IList<DatabaseColumn> computed = Array.Empty<DatabaseColumn>();
            if (ReadType.HasFlag(ReadTypes.TableDescs))
            {
                computed = _readerAdapter.ComputedColumns(tableName);
            }
            IList<DatabaseIndex> indexes = Array.Empty<DatabaseIndex>();
            if (ReadType.HasFlag(ReadTypes.Indexs))
            {
                indexes = MergeIndexColumns(_readerAdapter.Indexes(tableName), _readerAdapter.IndexColumns(tableName));
            }

            FillOutForeignKey(fks, indexes);
            var table = new DatabaseTable
            {
                SchemaOwner = _readerAdapter.Parameters.Owner,
                Name = tableName
            };
            if (columns.Count == 0)
            {
                if (ReadType.HasFlag( ReadTypes.Triggers))
                {
                    UpdateTriggers(table, triggers);
                    return table;
                }
                return null;
            }

            table.Columns.AddRange(columns);
            UpdateCheckConstraints(table, checkConstraints);
            UpdateIdentities(table.Columns, identityColumns);
            UpdateComputed(table.Columns, computed);
            UpdateConstraints(table, pks, ConstraintType.PrimaryKey);
            UpdateConstraints(table, uks, ConstraintType.UniqueKey);
            UpdateConstraints(table, fks, ConstraintType.ForeignKey);
            UpdateConstraints(table, dfs, ConstraintType.Default);
            UpdateIndexes(table, indexes);
            UpdateTriggers(table, triggers);
            UpdateTableDescriptions(table, tableDescs);
            UpdateColumnDescriptions(table, colDescs);
            _readerAdapter.PostProcessing(table);
            return table;
        }

        public IList<DatabaseTable> Execute(CancellationToken ct)
        {
            if (ct.IsCancellationRequested) return EmptyList();

            var tables = _readerAdapter.Tables(null);

            if (ct.IsCancellationRequested) return tables;

            var columns = _readerAdapter.Columns(null);
            var identityColumns = _readerAdapter.IdentityColumns(null);
            var checkConstraints = _readerAdapter.CheckConstraints(null);
            var pks = _readerAdapter.PrimaryKeys(null);
            var uks = _readerAdapter.UniqueKeys(null);
            var fks = _readerAdapter.ForeignKeys(null);
            
            var dfs = _readerAdapter.DefaultConstraints(null);
            var triggers = _readerAdapter.Triggers(null);
            var tableDescs = _readerAdapter.TableDescriptions(null);
            var colDescs = _readerAdapter.ColumnDescriptions(null);
            var computed = _readerAdapter.ComputedColumns(null);
            List<DatabaseIndex> dbIdxs = new List<DatabaseIndex>();
            foreach (var item in tables)
            {
                var indexes = MergeIndexColumns(_readerAdapter.Indexes(item.Name), _readerAdapter.IndexColumns(item.Name));
                dbIdxs.AddRange(indexes);
            }
            var noIndexes = (dbIdxs.Count == 0); //we may not be able to get any indexes without a tableName
            FillOutForeignKey(fks, dbIdxs);
            
            var tableFilter = _readerAdapter.Parameters.Exclusions.TableFilter;
            if (tableFilter != null)
            {
                tables = tables.Where(t => !tableFilter.Exclude(t.Name)).ToList();
            }

            int tablesCount = tables.Count;
            for (var i = 0; i < tablesCount; i++)
            {
                var table = tables[i];
                var tableName = table.Name;
                var schemaName = table.SchemaOwner;

                if (ct.IsCancellationRequested) return tables;
                RaiseProgress(ProgressType.Processing, SchemaObjectType.Tables,
                    tableName, i, tablesCount);
                IEnumerable<DatabaseColumn> tableCols;
                if (columns.Count == 0)
                {
                    tableCols = _readerAdapter.Columns(tableName);
                }
                else
                {
                    tableCols =
                       columns.Where(x => string.Equals(x.TableName, tableName)
                                          && string.Equals(x.SchemaOwner, schemaName));
                }
                table.Columns.AddRange(tableCols);
                UpdateIdentities(table.Columns, identityColumns);
                UpdateCheckConstraints(table, checkConstraints);
                UpdateComputed(table.Columns, computed);
                UpdateConstraints(table, pks, ConstraintType.PrimaryKey);
                UpdateConstraints(table, uks, ConstraintType.UniqueKey);
                UpdateConstraints(table, fks, ConstraintType.ForeignKey);
                UpdateConstraints(table, dfs, ConstraintType.Default);
                if (noIndexes)
                {
                    dbIdxs.Clear();
                    dbIdxs = MergeIndexColumns(_readerAdapter.Indexes(tableName), _readerAdapter.IndexColumns(tableName)).ToList();
                }
                UpdateIndexes(table, dbIdxs);
                UpdateTriggers(table, triggers);
                UpdateTableDescriptions(table, tableDescs);
                UpdateColumnDescriptions(table, colDescs);
                _readerAdapter.PostProcessing(table);
            }

            return tables;
        }

        private static void FillOutForeignKey(IList<DatabaseConstraint> fks, IList<DatabaseIndex> indexes)
        {
            foreach (var fk in fks.Where(f =>
                !string.IsNullOrEmpty(f.RefersToConstraint) && string.IsNullOrEmpty(f.RefersToTable)))
            {
                var constraint = indexes.FirstOrDefault(i => i.Name == fk.RefersToConstraint);
                if (constraint == null) continue;
                fk.RefersToTable = constraint.TableName;
                fk.RefersToSchema = constraint.SchemaOwner;
            }
        }

        private void UpdateTableDescriptions(DatabaseTable table, IList<DatabaseTable> descriptions)
        {
            var tableDesc = descriptions.FirstOrDefault(x => x.SchemaOwner == table.SchemaOwner &&
                                                x.Name == table.Name);
            if (tableDesc == null) return;
            table.Description = tableDesc.Description;
        }

        private void UpdateColumnDescriptions(DatabaseTable table, IList<DatabaseTable> descriptions)
        {
            var tableDesc = descriptions.FirstOrDefault(x => x.SchemaOwner == table.SchemaOwner &&
                                                x.Name == table.Name);
            if (tableDesc == null) return;
            foreach (var column in tableDesc.Columns)
            {
                var col = table.Columns.Find(x => x.Name == column.Name);
                if (col != null)
                {
                    col.Description = column.Description;
                }
            }
        }

        private void UpdateTriggers(DatabaseTable table, IList<DatabaseTrigger> triggers)
        {
            var tableTriggers = triggers.Where(x => x.SchemaOwner == table.SchemaOwner &&
                                                x.TableName == table.Name);
            table.Triggers.Clear();
            table.Triggers.AddRange(tableTriggers);
        }

        private void UpdateIndexes(DatabaseTable table, IList<DatabaseIndex> indexes)
        {
            var tableIndexes = indexes.Where(x => x.SchemaOwner == table.SchemaOwner &&
                                                x.TableName == table.Name);
            foreach (var index in tableIndexes)
            {
                //we don't need the index columns to be the actual table columns- the ordinals are different
                //var list = new List<DatabaseColumn>();
                foreach (var indexColumn in index.Columns)
                {
                    var tableColumn = table.Columns.FirstOrDefault(c => c.Name == indexColumn.Name);
                    if (tableColumn != null)
                    {
                        //list.Add(tableColumn);
                        //copy a few properties that might be useful instead of cross referencing manually
                        indexColumn.DbDataType = tableColumn.DbDataType;
                        indexColumn.Length = tableColumn.Length;
                        indexColumn.Scale = tableColumn.Scale;
                        indexColumn.Precision = tableColumn.Precision;
                        indexColumn.Nullable = tableColumn.Nullable;
                        indexColumn.TableName = tableColumn.TableName;
                    }
                }
                //index.Columns.Clear();
                //index.Columns.AddRange(list);
                table.AddIndex(index);
            }
        }

        private IList<DatabaseIndex> MergeIndexColumns(IList<DatabaseIndex> indexes, IList<DatabaseIndex> indexColumns)
        {
            if (indexes == null || indexes.Count == 0) return indexColumns;
            foreach (var indexColumn in indexColumns)
            {
                var index = indexes.FirstOrDefault(f =>
                                            f.Name == indexColumn.Name &&
                                            f.SchemaOwner == indexColumn.SchemaOwner &&
                                            f.TableName.Equals(indexColumn.TableName, StringComparison.OrdinalIgnoreCase));
                if (index == null)
                {
                    index = indexColumn;
                    indexes.Add(index);
                    continue;
                }
                //copy the index columns across
                index.Columns.AddRange(indexColumn.Columns);
            }
            return indexes;
        }

        private void UpdateConstraints(DatabaseTable table, IList<DatabaseConstraint> constraints, ConstraintType constraintType)
        {
            var keys = constraints.Where(x => x.SchemaOwner == table.SchemaOwner &&
                                                x.TableName == table.Name &&
                                                x.ConstraintType == constraintType);
            table.AddConstraints(keys);
        }

        private void UpdateCheckConstraints(DatabaseTable table, IList<DatabaseConstraint> constraints)
        {
            var checks = constraints.Where(x => x.SchemaOwner == table.SchemaOwner &&
                                                x.TableName == table.Name);
            table.AddConstraints(checks);
        }

        private void UpdateIdentities(IList<DatabaseColumn> list, IList<DatabaseColumn> ids)
        {
            foreach (var id in ids)
            {
                var column = list.FirstOrDefault(x => x.TableName == id.TableName &&
                                                      x.Name == id.Name);
                if (column == null) continue;
                column.IdentityDefinition = new DatabaseColumnIdentity
                {
                    IdentitySeed = id.IdentityDefinition.IdentitySeed,
                    IdentityIncrement = id.IdentityDefinition.IdentityIncrement,
                };
            }
        }

        private void UpdateComputed(IList<DatabaseColumn> list, IList<DatabaseColumn> computeds)
        {
            foreach (var computed in computeds)
            {
                var column = list.FirstOrDefault(x => x.SchemaOwner == computed.SchemaOwner &&
                                                      x.TableName == computed.TableName &&
                                                      x.Name == computed.Name);
                if (column == null) continue;
                column.ComputedDefinition = computed.ComputedDefinition;
            }
        }
    }
}