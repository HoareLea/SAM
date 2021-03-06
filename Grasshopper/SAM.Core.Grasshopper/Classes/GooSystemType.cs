﻿using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using SAM.Core.Grasshopper.Properties;
using System;
using System.Collections.Generic;

namespace SAM.Core.Grasshopper
{
    public class GooSystemType : GooSAMObject<ISystemType>
    {
        public GooSystemType()
            : base()
        {
        }

        public GooSystemType(ISystemType systemType)
            : base(systemType)
        {
        }

        public override IGH_Goo Duplicate()
        {
            return new GooSystemType(Value);
        }
    }

    public class GooSystemTypeParam : GH_PersistentParam<GooSystemType>
    {
        public override Guid ComponentGuid => new Guid("427cbc29-819f-44cc-b4a9-b9b3bfefd81e");

        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        public GooSystemTypeParam()
            : base("SystemType", "SystemType", "SAM Core SystemType", "Params", "SAM")
        {
        }

        protected override GH_GetterResult Prompt_Plural(ref List<GooSystemType> values)
        {
            throw new NotImplementedException();
        }

        protected override GH_GetterResult Prompt_Singular(ref GooSystemType value)
        {
            throw new NotImplementedException();
        }
    }
}