using System;
using System.Linq;

using GH_IO.Serialization;

using Rhino.Geometry;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;


namespace SAM.Analytical.Grasshopper
{
    public class GooPanel : GH_Goo<Panel>, IGH_PreviewData
    {
        public GooPanel(Panel panel)
        {
            Value = panel; 
        }

        public override bool IsValid => Value != null;

        public override string TypeName => "Panel";

        public override string TypeDescription => "SAM Analitycal Panel";

        public BoundingBox ClippingBox
        {
            get
            {
                if (Value == null)
                    return BoundingBox.Empty;

                return Geometry.Grasshopper.Convert.ToRhino(Value.GetBoundingBox());
            }
        }

        public override IGH_Goo Duplicate()
        {
            return new GooPanel(Value);
        }

        public override string ToString()
        {
            Panel panel = Value;
            
            if (!string.IsNullOrWhiteSpace(panel.Name))
                return panel.Name;

            if (panel.Construction != null)
                if (!string.IsNullOrWhiteSpace(panel.Construction.Name))
                    return panel.Construction.Name;

            return GetType().FullName;
        }

        public override bool Write(GH_IWriter writer)
        {
            SAM.Core.JSON.JSONParser jSONParser = AssemblyInfo.GetJSONParser();
            if (jSONParser == null)
                return false;

            jSONParser.Clear();
            jSONParser.Add(Value);


            writer.SetString("GooPanel", jSONParser.ToString());
            return true;
        }

        public override bool Read(GH_IReader reader)
        {
            Core.JSON.JSONParser jSONParser = AssemblyInfo.GetJSONParser();
            if (jSONParser == null)
                return false;

            string value = null;
            if (!reader.TryGetString("GooPanel", ref value))
                return false;

            jSONParser.Clear();
            jSONParser.Add(value);

            Value = jSONParser.GetObjects<Panel>().First();
            return true;
        }

        public void DrawViewportWires(GH_PreviewWireArgs args)
        {
            throw new NotImplementedException();
        }

        public void DrawViewportMeshes(GH_PreviewMeshArgs args)
        {
            throw new NotImplementedException();
        }
    }
}
