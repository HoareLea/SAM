using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using System;
using System.Collections.Generic;

namespace SAM.Core.Grasshopper
{
    public static partial class Query
    {
        public static IGH_Param Duplicate(this IGH_Param param)
        {
            var attributes = param.Attributes;
            try
            {
                param.Attributes = GH_SAMNullAttributes.Instance;
                var newParam = GH_ComponentParamServer.CreateDuplicate(param);

                newParam.NewInstanceGuid();

                if (newParam.MutableNickName && CentralSettings.CanvasFullNames)
                    newParam.NickName = newParam.Name;

                return newParam;
            }
            finally { param.Attributes = attributes; }
        }
    }
}