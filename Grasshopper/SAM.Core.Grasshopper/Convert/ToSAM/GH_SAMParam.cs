using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace SAM.Core.Grasshopper
{
    public static partial class Convert
    {
        public static GH_SAMParam ToSAM(this IGH_Param param)
        {
            return new GH_SAMParam(param, ParamVisibility.Binding);
        }

        public static GH_SAMParam ToSAM(this IGH_Param param, ParamVisibility paramVisibility)
        {
            return new GH_SAMParam(param, paramVisibility);
        }

        public static GH_SAMParam ToSAM<T>(this GH_PersistentParam<T> param, ParamVisibility paramVisibility, object defaultValue) where T : class, IGH_Goo, new()
        {
            GH_SAMParam result = new GH_SAMParam(param, paramVisibility);
            if (result.Param is GH_PersistentParam<T> persistentParam)
            {
                var data = new T();
                if (!data.CastFrom(defaultValue))
                    throw new InvalidCastException();

                persistentParam.PersistentData.Append(data);
            }

            return result;
        }
    }
}