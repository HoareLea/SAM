using System;
using System.Linq;
using System.Collections.Generic;

using GH_IO.Serialization;
using Rhino.Geometry;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;

using SAM.Geometry.Grasshopper;

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
            Boundary3D boundary3D = Value.Boundary3D;
            if (boundary3D == null)
                return;

            Dictionary<Edge3DLoop, System.Drawing.Color> aDictionary = new Dictionary<Edge3DLoop, System.Drawing.Color>();

            aDictionary[boundary3D.GetEdge3DLoop()] = System.Drawing.Color.DarkRed;

            
            IEnumerable<Edge3DLoop> edge3DLoops = boundary3D.GetInternalEdge3DLoops();
            if (edge3DLoops != null)
            {
                foreach (Edge3DLoop edge3DLoop in edge3DLoops)
                    aDictionary[edge3DLoop] = System.Drawing.Color.BlueViolet;
            }

            foreach (KeyValuePair<Edge3DLoop, System.Drawing.Color> keyValuePair in aDictionary)
            {
                List<Edge3D> edge3Ds = keyValuePair.Key.Edge3Ds;
                if (edge3Ds == null || edge3Ds.Count == 0)
                    continue;

                List<Point3d> point3ds = edge3Ds.ConvertAll(x => x.Curve3D.GetStart().ToRhino());
                if (point3ds.Count == 0)
                    continue;

                point3ds.Add(point3ds[0]);

                args.Pipeline.DrawPolyline(point3ds, keyValuePair.Value);
            }
        }

        public void DrawViewportMeshes(GH_PreviewMeshArgs args)
        {
            Brep brep = Value.Boundary3D.ToRhino();
            if (brep != null)
                args.Pipeline.DrawBrepShaded(brep, args.Material);
        }
    }
}
