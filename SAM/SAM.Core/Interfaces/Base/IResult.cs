using System;

namespace SAM.Core
{
    public interface IResult : IParameterizedSAMObject
    {
        string Source { get; }
        string Reference { get; }
        DateTime DateTime { get; }
    }
}