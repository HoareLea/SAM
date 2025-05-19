using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace SAM.Analytical.Grasshopper
{
    public class GooConstructionLayer: GooJSAMObject<ConstructionLayer>
    {
        public GooConstructionLayer()
            : base()
        {
        }

        public GooConstructionLayer(ConstructionLayer constructionLayer)
            : base(constructionLayer)
        {
        }

        public override bool IsValid => Value != null;

        public override string TypeName
        {
            get
            {
                Type type = null;

                if (Value == null)
                    type = typeof(ConstructionLayer);
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
                    type = typeof(ConstructionLayer);
                else
                    type = Value.GetType();

                if (type == null)
                    return null;

                return type.FullName.Replace(".", " ");
            }
        }

        public override IGH_Goo Duplicate()
        {
            return new GooConstructionLayer(Value);
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
                ConstructionLayer constructionLayer = value as ConstructionLayer;
                if(constructionLayer != null)
                {
                    Value = constructionLayer;
                    return true;
                }
            }
            
            return base.CastFrom(source);
        }

        public override bool CastTo<Q>(ref Q target)
        {
            return base.CastTo(ref target);
        }
    }

    public class GooConstructionLayerParam : GH_PersistentParam<GooConstructionLayer>
    {
        public override Guid ComponentGuid => new Guid("8ebc44e2-3c71-401c-acd5-1f9903cb49ee");

                protected override System.Drawing.Bitmap Icon => Core.Convert.ToBitmap(Resources.SAM_Small);

        public GooConstructionLayerParam()
            : base(typeof(ConstructionLayer).Name, typeof(ConstructionLayer).Name, typeof(ConstructionLayer).FullName.Replace(".", " "), "Params", "SAM")
        {
        }

        protected override GH_GetterResult Prompt_Plural(ref List<GooConstructionLayer> values)
        {
            throw new NotImplementedException();
        }

        protected override GH_GetterResult Prompt_Singular(ref GooConstructionLayer value)
        {
            throw new NotImplementedException();
        }

        public override void AppendAdditionalMenuItems(ToolStripDropDown menu)
        {
            Menu_AppendItem(menu, "Save As...", Menu_SaveAs, Core.Convert.ToBitmap(Resources.SAM3), VolatileData.AllData(true).Any());

            //Menu_AppendSeparator(menu);

            base.AppendAdditionalMenuItems(menu);
        }

        private void Menu_SaveAs(object sender, EventArgs e)
        {
            Core.Grasshopper.Query.SaveAs(VolatileData);
        }
    }
}