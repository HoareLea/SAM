using System;

namespace SAM.Core
{
    public interface IResult : ISAMObject
    {
        string Source { get; }
        string Reference { get; }
        DateTime DateTime { get; }
    }
}