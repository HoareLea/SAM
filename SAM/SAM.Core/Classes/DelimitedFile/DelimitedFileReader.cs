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

            int aPosition = 0;
            int aRowCount = 0;

            while (aPosition < DelimitedFileRow.LineText.Length)
            {
                string aValue;

                // Special handling for quoted field
                if (DelimitedFileRow.LineText[aPosition] == '"')
                {
                    // Skip initial quote
                    aPosition++;

                    // Parse quoted value
                    int aStart = aPosition;
                    while (aPosition < DelimitedFileRow.LineText.Length)
                    {
                        // Test for quote character
                        if (DelimitedFileRow.LineText[aPosition] == '"')
                        {
                            // Found one
                            aPosition++;

                            // If two quotes together, keep one Otherwise, indicates end of value
                            if (aPosition >= DelimitedFileRow.LineText.Length || DelimitedFileRow.LineText[aPosition] != '"')
                            {
                                aPosition--;
                                break;
                            }
                        }
                        aPosition++;

                        //TODO: Add code which read quoted text with break line symbol
                        while (aPosition == DelimitedFileRow.LineText.Length)
                        {
                            string aLineText = ReadLine();
                            if (aLineText == null)
                                break;

                            DelimitedFileRow.LineText += "\n" + aLineText;
                        }
                    }
                    aValue = DelimitedFileRow.LineText.Substring(aStart, aPosition - aStart);
                    aValue = aValue.Replace("\"\"", "\"");
                }
                else
                {
                    // Parse unquoted value
                    int aStart = aPosition;
                    while (aPosition < DelimitedFileRow.LineText.Length && DelimitedFileRow.LineText[aPosition] != Separator)
                        aPosition++;
                    aValue = DelimitedFileRow.LineText.Substring(aStart, aPosition - aStart);
                }

                // Add field to list
                if (aRowCount < DelimitedFileRow.Count)
                    DelimitedFileRow[aRowCount] = aValue;
                else
                    DelimitedFileRow.Add(aValue);
                aRowCount++;

                // Eat up to and including next comma
                while (aPosition < DelimitedFileRow.LineText.Length && DelimitedFileRow.LineText[aPosition] != Separator)
                    aPosition++;
                if (aPosition < DelimitedFileRow.LineText.Length)
                    aPosition++;
            }
            // Delete any unused items
            while (DelimitedFileRow.Count > aRowCount)
                DelimitedFileRow.RemoveAt(aRowCount);

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