using Grasshopper.Kernel;

namespace SAM.Core.Grasshopper
{
    public static partial class Create
    {
        public static GH_SAMParam GH_SAMParam<T>(string name, string nickname, string description, GH_ParamAccess access, bool optional = false, ParamVisibility paramVisibility = ParamVisibility.Binding)
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

        public static GH_SAMParam GH_SAMParam<T>(string name, string nickname, string description, object defaultValue, GH_ParamAccess access, bool optional = false, ParamVisibility paramVisibility = ParamVisibility.Binding)
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