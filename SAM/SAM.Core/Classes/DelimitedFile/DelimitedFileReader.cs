using System.Collections.Generic;
using System.IO;

namespace SAM.Core
{
    public class DelimitedFileReader : StreamReader, IDelimitedFileReader
    {
        private char separator;

        public DelimitedFileReader(char separator, Stream stream)
            : base(stream)
        {
            this.separator = separator;
        }

        public DelimitedFileReader(char separator, string path)
            : base(new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
        {
            this.separator = separator;
        }

        public DelimitedFileReader(char separator, IEnumerable<string> lines)
            : base(Query.MemoryStream(lines))
        {
            this.separator = separator;
        }

        public DelimitedFileReader(DelimitedFileType DelimitedFileType, IEnumerable<string> lines)
            : base(Query.MemoryStream(lines))
        {
            this.separator = Query.Separator(DelimitedFileType);
        }

        public DelimitedFileReader(DelimitedFileType DelimitedFileType, string Path)
            : base(new FileStream(Path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
        {
            separator = Query.Separator(DelimitedFileType);
        }

        public char Separator
        {
            get
            {
                return separator;
            }
        }

        /// <summary>
        /// Reads a row of data from a CSV file
        /// </summary>
        /// <param name="DelimitedFileRow"></param>
        /// <returns></returns>
        public bool Read(DelimitedFileRow DelimitedFileRow)
        {
            DelimitedFileRow.LineText = ReadLine();
            if (DelimitedFileRow.LineText == null)
                return false;

            int position = 0;
            int rowCount = 0;

            while (position < DelimitedFileRow.LineText.Length)
            {
                string aValue;

                // Special handling for quoted field
                if (DelimitedFileRow.LineText[position] == '"')
                {
                    // Skip initial quote
                    position++;

                    // Parse quoted value
                    int start = position;
                    while (position < DelimitedFileRow.LineText.Length)
                    {
                        // Test for quote character
                        if (DelimitedFileRow.LineText[position] == '"')
                        {
                            // Found one
                            position++;

                            // If two quotes together, keep one Otherwise, indicates end of value
                            if (position >= DelimitedFileRow.LineText.Length || DelimitedFileRow.LineText[position] != '"')
                            {
                                position--;
                                break;
                            }
                        }
                        position++;

                        //TODO: Add code which read quoted text with break line symbol
                        while (position == DelimitedFileRow.LineText.Length)
                        {
                            string lineText = ReadLine();
                            if (lineText == null)
                                break;

                            DelimitedFileRow.LineText += "\n" + lineText;
                        }
                    }
                    aValue = DelimitedFileRow.LineText.Substring(start, position - start);
                    aValue = aValue.Replace("\"\"", "\"");
                }
                else
                {
                    // Parse unquoted value
                    int aStart = position;
                    while (position < DelimitedFileRow.LineText.Length && DelimitedFileRow.LineText[position] != Separator)
                        position++;
                    aValue = DelimitedFileRow.LineText.Substring(aStart, position - aStart);
                }

                // Add field to list
                if (rowCount < DelimitedFileRow.Count)
                    DelimitedFileRow[rowCount] = aValue;
                else
                    DelimitedFileRow.Add(aValue);
                rowCount++;

                // Eat up to and including next comma
                while (position < DelimitedFileRow.LineText.Length && DelimitedFileRow.LineText[position] != Separator)
                    position++;
                if (position < DelimitedFileRow.LineText.Length)
                    position++;
            }
            // Delete any unused items
            while (DelimitedFileRow.Count > rowCount)
                DelimitedFileRow.RemoveAt(rowCount);

            // Return true if any columns read
            return (DelimitedFileRow.Count > 0);
        }

        /// <summary>
        /// Reads a rows of data from a CSV file
        /// </summary>
        /// <returns>List of the rows</returns>
        public List<DelimitedFileRow> Read()
        {
            List<DelimitedFileRow> aCsvRowList = new List<DelimitedFileRow>();
            DelimitedFileRow aCsvRow = new DelimitedFileRow();
            while (Read(aCsvRow))
            {
                aCsvRowList.Add(aCsvRow);
                aCsvRow = new DelimitedFileRow();
            }
            return aCsvRowList;
        }
    }
}