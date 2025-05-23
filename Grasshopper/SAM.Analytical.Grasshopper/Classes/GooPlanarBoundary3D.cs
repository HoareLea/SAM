﻿using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino;
using Rhino.Display;
using Rhino.DocObjects;
using Rhino.Geometry;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace SAM.Analytical.Grasshopper
{
    public class GooPlanarBoundary3D : GooJSAMObject<PlanarBoundary3D>, IGH_PreviewData, IGH_BakeAwareData
    {
        public GooPlanarBoundary3D()
            : base()
        {
        }

        public GooPlanarBoundary3D(PlanarBoundary3D planarBoundary3D)
            : base(planarBoundary3D)
        {
        }

        public BoundingBox ClippingBox
        {
            get
            {
                if (Value == null)
                    return BoundingBox.Empty;

                return Geometry.Rhino.Convert.ToRhino(Value.GetBoundingBox());
            }
        }

        public override IGH_Goo Duplicate()
        {
            return new GooPlanarBoundary3D(Value);
        }

        public void DrawViewportWires(GH_PreviewWireArgs args)
        {
            DrawViewportWires(args, System.Drawing.Color.DarkRed, System.Drawing.Color.BlueViolet);
        }

        public void DrawViewportWires(GH_PreviewWireArgs args, System.Drawing.Color color_ExternalEdge, System.Drawing.Color color_InternalEdges)
        {
            PlanarBoundary3D planarBoundary3D = Value;
            if (planarBoundary3D == null)
                return;

            Dictionary<BoundaryEdge3DLoop, System.Drawing.Color> dictionary = new Dictionary<BoundaryEdge3DLoop, System.Drawing.Color>();

            //Assign Color for Edges
            dictionary[planarBoundary3D.GetExternalEdge3DLoop()] = color_ExternalEdge;

            IEnumerable<BoundaryEdge3DLoop> edge3DLoops = planarBoundary3D.GetInternalEdge3DLoops();
            if (edge3DLoops != null)
            {
                foreach (BoundaryEdge3DLoop edge3DLoop in edge3DLoops)
                    dictionary[edge3DLoop] = color_InternalEdges;
            }

            foreach (KeyValuePair<BoundaryEdge3DLoop, System.Drawing.Color> keyValuePair in dictionary)
            {
                List<BoundaryEdge3D> edge3Ds = keyValuePair.Key.BoundaryEdge3Ds;
                if (edge3Ds == null || edge3Ds.Count == 0)
                    continue;

                List<Point3d> point3ds = edge3Ds.ConvertAll(x => Geometry.Rhino.Convert.ToRhino(x.Curve3D.GetStart()));
                if (point3ds.Count == 0)
                    continue;

                point3ds.Add(point3ds[0]);

                args.Pipeline.DrawPolyline(point3ds, keyValuePair.Value);
            }
        }

        public void DrawViewportMeshes(GH_PreviewMeshArgs args)
        {
            DrawViewportMeshes(args, args.Material);
        }

        public void DrawViewportMeshes(GH_PreviewMeshArgs args, DisplayMaterial displayMaterial)
        {
            Brep brep = Rhino.Convert.ToRhino(Value);
            if (brep != null)
                args.Pipeline.DrawBrepShaded(brep, displayMaterial);
        }

        public bool BakeGeometry(RhinoDoc doc, ObjectAttributes att, out Guid obj_guid)
        {
            GeometryBase geometryBase = Rhino.Convert.ToRhino(Value);
            if (geometryBase == null)
            {
                obj_guid = Guid.Empty;
                return false;
            }

            obj_guid = doc.Objects.Add(geometryBase);
            return true;
        }
    }

    public class GooPlanarBoundary3DParam : GH_PersistentParam<GooPlanarBoundary3D>, IGH_PreviewObject, IGH_BakeAwareObject
    {
        public override Guid ComponentGuid => new Guid("3b944b3c-bc94-46cc-aea3-b74385e138dc");

                protected override System.Drawing.Bitmap Icon => Core.Convert.ToBitmap(Resources.SAM_Small);

        public override GH_Exposure Exposure => GH_Exposure.hidden;

        bool IGH_PreviewObject.Hidden { get; set; }

        bool IGH_PreviewObject.IsPreviewCapable => !VolatileData.IsEmpty;

        BoundingBox IGH_PreviewObject.ClippingBox => Preview_ComputeClippingBox();

        public bool IsBakeCapable => true;

        void IGH_PreviewObject.DrawViewportMeshes(IGH_PreviewArgs args) => Preview_DrawMeshes(args);

        void IGH_PreviewObject.DrawViewportWires(IGH_PreviewArgs args) => Preview_DrawWires(args);

        public GooPlanarBoundary3DParam()
            : base(typeof(PlanarBoundary3D).Name, typeof(PlanarBoundary3D).Name, typeof(PlanarBoundary3D).FullName.Replace(".", " "), "Params", "SAM")
        {
        }

        protected override GH_GetterResult Prompt_Plural(ref List<GooPlanarBoundary3D> values)
        {
            throw new NotImplementedException();
        }

        protected override GH_GetterResult Prompt_Singular(ref GooPlanarBoundary3D value)
        {
            throw new NotImplementedException();
        }

        public void BakeGeometry(RhinoDoc doc, List<Guid> obj_ids)
        {
            BakeGeometry(doc, doc.CreateDefaultAttributes(), obj_ids);
        }

        public void BakeGeometry(RhinoDoc doc, ObjectAttributes att, List<Guid> obj_ids)
        {
            foreach (var value in VolatileData.AllData(true))
            {
                Guid uuid = default;
                (value as IGH_BakeAwareData)?.BakeGeometry(doc, att, out uuid);
                obj_ids.Add(uuid);
            }
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