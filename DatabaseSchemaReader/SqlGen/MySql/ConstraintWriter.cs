using DatabaseSchemaReader.DataSchema;
using System.Linq;

namespace DatabaseSchemaReader.SqlGen.MySql
{
    class ConstraintWriter : ConstraintWriterBase
    {
        public ConstraintWriter(DatabaseTable table) : base(table)
        {
        }

        #region Overrides of ConstraintWriterBase

        protected override ISqlFormatProvider SqlFormatProvider()
        {
            return new SqlFormatProvider();
        }
        protected override string ConstraintName(DatabaseConstraint constraint)
        {
            return "PK_" + string.Join("_", constraint.Columns.Select(x=>x).ToArray());
        }

        #endregion
    }
}
