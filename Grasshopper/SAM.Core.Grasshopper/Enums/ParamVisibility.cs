using System;

namespace SAM.Core.Grasshopper
{
    [Flags]
    public enum ParamVisibility
    {
        Voluntary = 0,
        Mandatory = 1,
        Default = 2,
        Binding = 3
    }
}
