using System.Collections.Generic;

namespace SAM.Core
{
    /// <summary>
    /// Class to store one delimited file row
    /// </summary>
    public class DelimitedFileRow : List<string>
    {
        public DelimitedFileRow()
        {

        }

        public DelimitedFileRow(IEnumerable<string> Values)
        {
            AddRange(Values);
        }

        public string LineText { get; set; }
    }
}
