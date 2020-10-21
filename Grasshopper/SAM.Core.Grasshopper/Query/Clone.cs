using Grasshopper;
using Grasshopper.Kernel;

namespace SAM.Core.Grasshopper
{
    public static partial class Query
    {
        public static IGH_Param Clone(this IGH_Param param)
        {
            var attributes = param.Attributes;
            try
            {
                param.Attributes = new GH_SAMNullAttributes();
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