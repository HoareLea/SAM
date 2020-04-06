using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAM.Core
{
    public interface IDelimitedFileWriter
    {
        char Separator { get; }

        void Write(DelimitedFileRow DelimitedFileRow);
        void Write(IEnumerable<DelimitedFileRow> DelimitedFileRows);
    }
}
