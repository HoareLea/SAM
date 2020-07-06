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

        public static GH_SAMParam FromParam(IGH_Param param) => new GH_SAMParam(param, ParamVisibility.Binding);

        public static GH_SAMParam FromParam(IGH_Param param, ParamVisibility paramVisibility) => new GH_SAMParam(param, paramVisibility);

        public static GH_SAMParam FromParam<T>(GH_PersistentParam<T> param, ParamVisibility paramVisibility, object defaultValue)
          where T : class, IGH_Goo, new()
        {
            var def = new GH_SAMParam(param, paramVisibility);
            if (def.Param is GH_PersistentParam<T> persistentParam)
            {
                var data = new T();
                if (!data.CastFrom(defaultValue))
                    throw new InvalidCastException();

                persistentParam.PersistentData.Append(data);
            }

            return def;
        }

        public static GH_SAMParam Create<T>(string name, string nickname, string description, GH_ParamAccess access, bool optional = false, ParamVisibility paramVisibility = ParamVisibility.Binding)
          where T : class, IGH_Param, new()
        {
            var param = new T()
            {
                Name = name,
                NickName = nickname,
                Description = description,
                Access = access,
                Optional = optional
            };

            return new GH_SAMParam(param, paramVisibility);
        }

        public static GH_SAMParam Create<T>(string name, string nickname, string description, object defaultValue, GH_ParamAccess access, bool optional = false, ParamVisibility paramVisibility = ParamVisibility.Binding)
          where T : class, IGH_Param, new()
        {
            var param = new T()
            {
                Name = name,
                NickName = nickname,
                Description = description,
                Access = access,
                Optional = optional
            };

            if (typeof(T).GenericSubclassOf(typeof(GH_PersistentParam<>)))
            {
                dynamic persistentParam = param;
                persistentParam.SetPersistentData(defaultValue);
            }

            return new GH_SAMParam(param, paramVisibility);
        }
    }
}
