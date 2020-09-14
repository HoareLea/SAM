﻿using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using SAM.Core.Grasshopper.Properties;
using System;
using System.Collections.Generic;

namespace SAM.Core.Grasshopper
{
    public class GooMaterial: GH_Goo<IMaterial>
    {
        public GooMaterial()
            : base()
        {
        }

        public GooMaterial(IMaterial material)
            : base(material)
        {
        }

        public override bool IsValid => Value != null;

        public override string TypeName
        {
            get
            {
                Type type = null;

                if (Value == null)
                    type = typeof(Material);
                else
                    type = Value.GetType();

                if (type == null)
                    return null;

                return type.Name;
            }
        }

        public override string TypeDescription
        {
            get
            {
                Type type = null;

                if (Value == null)
                    type = typeof(Material);
                else
                    type = Value.GetType();

                if (type == null)
                    return null;

                return type.FullName.Replace(".", " ");
            }
        }

        public override IGH_Goo Duplicate()
        {
            return new GooMaterial(Value);
        }

        public override string ToString()
        {
            if (Value != null)
                return Value.Name;

            return null;
        }

        public override bool CastFrom(object source)
        {
            return base.CastFrom(source);
        }

        public override bool CastTo<Q>(ref Q target)
        {
            if(Value is Q)
            {
                target = (Q)Value;
                return true;
            }
           
            
            return base.CastTo(ref target);
        }
    }

    public class GooMaterialParam : GH_PersistentParam<GooMaterial>
    {
        public override Guid ComponentGuid => new Guid("8810fd33-f1b7-402e-8c14-78e197a847a8");

        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        public GooMaterialParam()
            : base(typeof(Material).Name, typeof(Material).Name, typeof(Material).FullName.Replace(".", " "), "Params", "SAM")
        {
        }

        protected override GH_GetterResult Prompt_Plural(ref List<GooMaterial> values)
        {
            throw new NotImplementedException();
        }

        protected override GH_GetterResult Prompt_Singular(ref GooMaterial value)
        {
            throw new NotImplementedException();
        }
    }
}