using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAM.Core
{
    public static partial class Query
    {
        public static char Separator(this DelimitedFileType DelimitedFileType)
        {
            switch (DelimitedFileType)
            {
                case DelimitedFileType.Csv:
                    return ',';
                case DelimitedFileType.TabDelimited:
                    return '\t';
                default:
                    return '\n';
            }
        }
    }
}
