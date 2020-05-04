using System.Collections.Generic;

namespace SAM.Core
{
    public interface IDelimitedFileWriter
    {
        char Separator { get; }

        void Write(DelimitedFileRow DelimitedFileRow);

        void Write(IEnumerable<DelimitedFileRow> DelimitedFileRows);
    }
}