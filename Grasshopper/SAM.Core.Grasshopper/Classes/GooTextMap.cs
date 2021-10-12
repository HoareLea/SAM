using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using SAM.Core.Grasshopper.Properties;
using System;
using System.Collections.Generic;

namespace SAM.Core.Grasshopper
{
    public class GooTextMap : GooJSAMObject<TextMap>
    {
        public GooTextMap()
            : base()
        {
        }

        public GooTextMap(TextMap textMap)
            : base(textMap)
        {
        }

        public override IGH_Goo Duplicate()
        {
            return new GooTextMap(Value);
        }
    }

    public class GooTextMapParam : GH_PersistentParam<GooTextMap>
    {
        public override Guid ComponentGuid => new Guid("47eebb1f-0747-45bb-9afd-eadd9038c9ef");

        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        public GooTextMapParam()
            : base(typeof(TextMap).Name, typeof(TextMap).Name, typeof(TextMap).FullName.Replace(".", " "), "Params", "SAM")
        {
        }

        protected override GH_GetterResult Prompt_Plural(ref List<GooTextMap> values)
        {
            throw new NotImplementedException();
        }

        protected override GH_GetterResult Prompt_Singular(ref GooTextMap value)
        {
            throw new NotImplementedException();
        }
    }
}