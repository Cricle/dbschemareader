// ReSharper disable once RedundantUsingDirective 

namespace DatabaseSchemaReader.ProviderSchemaReaders.Builders
{
    /// <summary>
    /// The data read types
    /// </summary>
    public enum ReadTypes
    {
        /// <summary>
        /// The columns
        /// </summary>
        Columns = 1,
        /// <summary>
        /// The identity columns
        /// </summary>
        IdentityColumns = Columns << 1,
        /// <summary>
        /// The check constrains
        /// </summary>
        CheckConstraints = Columns << 2,
        /// <summary>
        /// The primary keys
        /// </summary>
        Pks = Columns << 3,
        /// <summary>
        /// The unique keys
        /// </summary>
        Uks = Columns << 4,
        /// <summary>
        /// The foregin keys
        /// </summary>
        Fks = Columns << 5,
        /// <summary>
        /// The default constrains
        /// </summary>
        Dfs = Columns << 6,
        /// <summary>
        /// The triggers
        /// </summary>
        Triggers = Columns << 7,
        /// <summary>
        /// The table descriptions
        /// </summary>
        TableDescs = Columns << 8,
        /// <summary>
        /// The computed columns
        /// </summary>
        Computed = Columns << 9,
        /// <summary>
        /// The indexs
        /// </summary>
        Indexs= Columns << 10,
        /// <summary>
        /// All
        /// </summary>
        All = Columns | IdentityColumns | CheckConstraints | Pks | Uks | Fks | Dfs | Triggers | TableDescs | Computed| Indexs,
        /// <summary>
        /// All colums except triggers table descs
        /// </summary>
        AllColumns = Columns | IdentityColumns | CheckConstraints | Pks | Uks | Fks | Dfs | Computed | Indexs,
    }
}