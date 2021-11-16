using System;

namespace SAM.Core
{
    public interface ISAMBaseObject : IJSAMObject
    {
        Guid Guid { get; }
        string Name { get; }
    }
}