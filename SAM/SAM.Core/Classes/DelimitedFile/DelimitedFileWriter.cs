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

        public DelimitedFileWriter(DelimitedFileType DelimitedFileType, string Path)
            : base(Path)
        {
            pSeparator = Query.Separator(DelimitedFileType);
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
        /// <param name="DelimitedFileRow">The row to be written</param>
        public void Write(DelimitedFileRow DelimitedFileRow)
        {
            if (DelimitedFileRow == null)
                return;

            StringBuilder aBuilder = new StringBuilder();
            bool aFirstColumn = true;
            foreach (string aValue in DelimitedFileRow)
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
            DelimitedFileRow.LineText = aBuilder.ToString();
            WriteLine(DelimitedFileRow.LineText);
        }

        /// <summary>
        /// Writes a rows to a CSV file.
        /// </summary>
        /// <param name="DelimitedFileRows">The rows to be written</param>
        public void Write(IEnumerable<DelimitedFileRow> DelimitedFileRows)
        {
            foreach (DelimitedFileRow aDelimitedFileRow in DelimitedFileRows)
                Write(aDelimitedFileRow);
        }

        public void Write(DelimitedFileTable DelimitedFileTable)
        {
            if (DelimitedFileTable == null)
                return;

            DelimitedFileTable.Write(this);
        }
    }
}