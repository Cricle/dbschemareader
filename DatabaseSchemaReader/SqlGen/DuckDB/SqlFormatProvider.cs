using System;
using System.Collections.Generic;
using System.Text;

namespace DatabaseSchemaReader.SqlGen.DuckDB
{
    class SqlFormatProvider : ISqlFormatProvider
    {
        public string Escape(string name)
        {
            return "\"" + name + "\"";
        }

        public string LineEnding()
        {
            return ";";
        }

        public string RunStatements()
        {
            return string.Empty;
        }

        public int MaximumNameLength
        {
            get { return 256; } //there is no hard limit in SQLite
        }
    }
}
