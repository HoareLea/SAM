using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using System;

namespace SAM.Core.Grasshopper
{
    public struct GH_SAMParam
    {
        public readonly IGH_Param Param;
        public readonly ParamVisibility ParamVisibility;

        public GH_SAMParam(IGH_Param param)
        {
            Param = param;
            ParamVisibility = ParamVisibility.Binding;
        }

        public GH_SAMParam(IGH_Param param, ParamVisibility paramVisibility)
        {
            Param = param;
            ParamVisibility = paramVisibility;
        }
    }
}
