using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using SAM.Architectural.Grasshopper.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace SAM.Architectural.Grasshopper
{
    public class GooMaterialLayer: GH_Goo<MaterialLayer>
    {
        public GooMaterialLayer()
            : base()
        {
        }

        public GooMaterialLayer(MaterialLayer materialLayer)
            : base(materialLayer)
        {
        }

        public override bool IsValid => Value != null;

        public override string TypeName
        {
            get
            {
                Type type = null;

                if (Value == null)
                    type = typeof(MaterialLayer);
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
                    type = typeof(MaterialLayer);
                else
                    type = Value.GetType();

                if (type == null)
                    return null;

                return type.FullName.Replace(".", " ");
            }
        }

        public override IGH_Goo Duplicate()
        {
            return new GooMaterialLayer(Value);
        }

        public override string ToString()
        {
            if (Value != null)
                return string.Format("{0} [{1}]", Value.Name, Value.Thickness);

            return null;
        }

        public override bool CastFrom(object source)
        {
            if(source is IGH_Goo)
            {
                object value = (source as dynamic).Value;
                MaterialLayer materialLayer = value as MaterialLayer;
                if(materialLayer != null)
                {
                    Value = materialLayer;
                    return true;
                }
            }
            
            return base.CastFrom(source);
        }

        public override bool CastTo<Q>(ref Q target)
        {
            if (Value == null)
                return false;

            if (typeof(Q).IsAssignableFrom(typeof(MaterialLayer)))
            {
                target = (Q)(object)Value;
                return true;
            }

            return base.CastTo(ref target);
        }
    }

    public class GooMaterialLayerParam : GH_PersistentParam<GooMaterialLayer>
    {
        public override Guid ComponentGuid => new Guid("cb6524ab-7111-454a-b6be-5e23007c794c");

                protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        public GooMaterialLayerParam()
            : base(typeof(MaterialLayer).Name, typeof(MaterialLayer).Name, typeof(MaterialLayer).FullName.Replace(".", " "), "Params", "SAM")
        {
        }

        protected override GH_GetterResult Prompt_Plural(ref List<GooMaterialLayer> values)
        {
            throw new NotImplementedException();
        }

        protected override GH_GetterResult Prompt_Singular(ref GooMaterialLayer value)
        {
            throw new NotImplementedException();
        }

        public override void AppendAdditionalMenuItems(ToolStripDropDown menu)
        {
            Menu_AppendItem(menu, "Save As...", Menu_SaveAs, VolatileData.AllData(true).Any());

            //Menu_AppendSeparator(menu);

            base.AppendAdditionalMenuItems(menu);
        }

        private void Menu_SaveAs(object sender, EventArgs e)
        {
            Core.Grasshopper.Query.SaveAs(VolatileData);
        }
    }
}