using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SAM.Core
{
    public class DelimitedFileWriter : StreamWriter, IDelimitedFileWriter
    {
        private char pSeparator;

        public DelimitedFileWriter(char Separator, Stream Stream)
            : base(Stream)
        {
            pSeparator = Separator;
        }

        public DelimitedFileWriter(char Separator, string Path)
            : base(Path)
        {
            pSeparator = Separator;
        }

        public DelimitedFileWriter(DelimitedFileType delimitedFileType, string Path)
            : base(Path)
        {
            pSeparator = Query.Separator(delimitedFileType);
        }

        public char Separator
        {
            get
            {
                return pSeparator;
            }
        }

        /// <summary>
        /// Writes a single row to a CSV file.
        /// </summary>
        /// <param name="delimitedFileRow">The row to be written</param>
        public void Write(DelimitedFileRow delimitedFileRow)
        {
            if (delimitedFileRow == null)
                return;

            StringBuilder aBuilder = new StringBuilder();
            bool aFirstColumn = true;
            foreach (string aValue in delimitedFileRow)
            {
                // Add separator if this isn't the first value
                if (!aFirstColumn)
                    aBuilder.Append(Separator);
                // Implement special handling for values that contain comma or quote Enclose in
                // quotes and double up any double quotes
                if (aValue.IndexOfAny(new char[] { '"', pSeparator }) != -1)
                    aBuilder.AppendFormat("\"{0}\"", aValue.Replace("\"", "\"\""));
                else
                    aBuilder.Append(aValue);
                aFirstColumn = false;
            }
            delimitedFileRow.LineText = aBuilder.ToString();
            WriteLine(delimitedFileRow.LineText);
        }

        /// <summary>
        /// Writes a rows to a CSV file.
        /// </summary>
        /// <param name="delimitedFileRows">The rows to be written</param>
        public void Write(IEnumerable<DelimitedFileRow> delimitedFileRows)
        {
            foreach (DelimitedFileRow aDelimitedFileRow in delimitedFileRows)
                Write(aDelimitedFileRow);
        }

        public void Write(DelimitedFileTable delimitedFileTable)
        {
            if (delimitedFileTable == null)
                return;

            delimitedFileTable.Write(this);
        }
    }
}