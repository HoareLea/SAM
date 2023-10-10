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

        public DelimitedFileReader(DelimitedFileType delimitedFileType, IEnumerable<string> lines)
            : base(Query.MemoryStream(lines))
        {
            separator = Query.Separator(delimitedFileType);
        }

        public DelimitedFileReader(DelimitedFileType delimitedFileType, string path)
            : base(new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
        {
            separator = Query.Separator(delimitedFileType);
        }

        public DelimitedFileReader(DelimitedFileType delimitedFileType, string path, System.Text.Encoding encoding)
            : base(new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite), encoding)
        {
            separator = Query.Separator(delimitedFileType);
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
        /// <param name="delimitedFileRow"></param>
        /// <returns></returns>
        public bool Read(DelimitedFileRow delimitedFileRow)
        {
            delimitedFileRow.LineText = ReadLine();
            if (delimitedFileRow.LineText == null)
            {
                return false;
            }

            int position = 0;
            int rowCount = 0;

            while (position < delimitedFileRow.LineText.Length)
            {
                string value;

                // Special handling for quoted field
                if (delimitedFileRow.LineText[position] == '"')
                {
                    // Skip initial quote
                    position++;

                    // Parse quoted value
                    int start = position;
                    while (position < delimitedFileRow.LineText.Length)
                    {
                        // Test for quote character
                        if (delimitedFileRow.LineText[position] == '"')
                        {
                            // Found one
                            position++;

                            // If two quotes together, keep one Otherwise, indicates end of value
                            if (position >= delimitedFileRow.LineText.Length || delimitedFileRow.LineText[position] != '"')
                            {
                                position--;
                                break;
                            }
                        }
                        position++;

                        //TODO: Add code which read quoted text with break line symbol
                        while (position == delimitedFileRow.LineText.Length)
                        {
                            string lineText = ReadLine();
                            if (lineText == null)
                            {
                                break;
                            }

                            delimitedFileRow.LineText += "\n" + lineText;
                        }
                    }
                    value = delimitedFileRow.LineText.Substring(start, position - start);
                    value = value.Replace("\"\"", "\"");
                }
                else
                {
                    // Parse unquoted value
                    int start = position;
                    while (position < delimitedFileRow.LineText.Length && delimitedFileRow.LineText[position] != separator)
                    {
                        position++;
                    }

                    value = delimitedFileRow.LineText.Substring(start, position - start);
                }

                // Add field to list
                if (rowCount < delimitedFileRow.Count)
                    delimitedFileRow[rowCount] = value;
                else
                    delimitedFileRow.Add(value);
                rowCount++;

                // Eat up to and including next comma
                while (position < delimitedFileRow.LineText.Length && delimitedFileRow.LineText[position] != separator)
                    position++;
                if (position < delimitedFileRow.LineText.Length)
                    position++;
            }
            // Delete any unused items
            while (delimitedFileRow.Count > rowCount)
                delimitedFileRow.RemoveAt(rowCount);

            // Return true if any columns read
            return (delimitedFileRow.Count > 0);
        }

        /// <summary>
        /// Reads a rows of data from a CSV file
        /// </summary>
        /// <returns>List of the rows</returns>
        public new List<DelimitedFileRow> Read()
        {
            List<DelimitedFileRow> delimitedFileRows = new List<DelimitedFileRow>();
            DelimitedFileRow delimitedFileRow = new DelimitedFileRow();
            while (Read(delimitedFileRow))
            {
                delimitedFileRows.Add(delimitedFileRow);
                delimitedFileRow = new DelimitedFileRow();
            }
            return delimitedFileRows;
        }
    }
}