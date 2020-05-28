using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino;
using Rhino.Display;
using Rhino.DocObjects;
using Rhino.Geometry;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class GooAdjacencyCluster : GooSAMObject<AdjacencyCluster>, IGH_PreviewData, IGH_BakeAwareData
    {
        public GooAdjacencyCluster()
            : base()
        {
        }

        public GooAdjacencyCluster(AdjacencyCluster adjacencyCluster)
            : base(adjacencyCluster)
        {
        }

        public BoundingBox ClippingBox
        {
            get
            {
                if (Value == null)
                    return BoundingBox.Empty;

                List<Geometry.Spatial.BoundingBox3D> boundingBox3Ds = new List<Geometry.Spatial.BoundingBox3D>();

                IEnumerable<Panel> panels = Value.GetPanels();
                if (panels != null)
                {
                    foreach (Panel panel in panels)
                        boundingBox3Ds.Add(panel.GetBoundingBox());
                }

                IEnumerable<Space> spaces = Value.GetSpaces();
                if (spaces != null)
                {
                    foreach (Space space in spaces)
                        boundingBox3Ds.Add(space.Location.GetBoundingBox(1));
                }

                if (boundingBox3Ds == null)
                    return BoundingBox.Empty;

                boundingBox3Ds.RemoveAll(x => x == null);

                if (boundingBox3Ds.Count == 0)
                    return BoundingBox.Empty;

                return Geometry.Grasshopper.Convert.ToRhino(new Geometry.Spatial.BoundingBox3D(boundingBox3Ds));
            }
        }

        public override IGH_Goo Duplicate()
        {
            return new GooAdjacencyCluster(Value);
        }

        public void DrawViewportWires(GH_PreviewWireArgs args)
        {
            foreach (Panel panel in Value.GetPanels())
            {
                GooPlanarBoundary3D gooPlanarBoundary3D = new GooPlanarBoundary3D(panel.PlanarBoundary3D);
                gooPlanarBoundary3D.DrawViewportWires(args);
            }

            foreach (Space space in Value.GetSpaces())
            {
                GooSpace gooSpace = new GooSpace(space);
                gooSpace.DrawViewportWires(args);
            }
        }

        public void DrawViewportMeshes(GH_PreviewMeshArgs args)
        {
            DrawViewportMeshes(args, args.Material, Core.Tolerance.Distance);
        }

        public void DrawViewportMeshes(GH_PreviewMeshArgs args, DisplayMaterial displayMaterial, double tolerance = Core.Tolerance.Distance)
        {
            if (Value == null)
                return;
            
            List<Panel> panels = Value.GetPanels();
            if (panels == null)
                panels = new List<Panel>();

            List<Space> spaces = Value.GetSpaces();
            if (spaces != null && spaces.Count > 0)
            {
                foreach (Space space in spaces)
                {
                    GooSpace gooSpace = new GooSpace(space);
                    gooSpace.DrawViewportMeshes(args);

                    List<Panel> panels_Related = Value.GetRelatedObjects<Panel>(space);
                    if (panels_Related == null || panels_Related.Count == 0)
                        continue;

                    panels.RemoveAll(x => panels_Related.Contains(x));
                    List<Brep> breps = new List<Brep>();
                    foreach(Panel panel in panels_Related)
                    {
                        Brep brep = panel.ToRhino();
                        if (brep == null)
                            continue;

                        breps.Add(brep);
                    }

                    if (breps == null || breps.Count == 0)
                        continue;

                    Brep[] breps_Join = Brep.JoinBreps(breps, tolerance);

                    if (breps_Join != null)
                    {
                        foreach (Brep brep in breps_Join)
                            args.Pipeline.DrawBrepShaded(brep, displayMaterial);
                    }
                }   
            }

            foreach (Panel panel in panels)
            {
                GooPlanarBoundary3D gooPlanarBoundary3D = new GooPlanarBoundary3D(panel.PlanarBoundary3D);
                gooPlanarBoundary3D.DrawViewportMeshes(args);
            }
        }

        public bool BakeGeometry(RhinoDoc doc, ObjectAttributes att, out Guid obj_guid)
        {
            obj_guid = Guid.Empty;

            List<Panel> panels = Value?.GetPanels();
            if (panels == null || panels.Count == 0)
                return false;

            List<Brep> breps = new List<Brep>();
            foreach(Panel panel in panels)
            {
                Brep brep = panel.ToRhino();
                if (brep == null)
                    continue;

                breps.Add(brep);
            }

            if (breps == null || breps.Count == 0)
                return false;

            Brep result = Brep.MergeBreps(breps, Core.Tolerance.Distance);
            if (result == null)
                return false;

            obj_guid = doc.Objects.AddBrep(result);
            return true;
        }
    }

    //Params Components -> SAM used for internalizing data
    public class GooAdjacencyClusterParam : GH_PersistentParam<GooAdjacencyCluster>, IGH_PreviewObject, IGH_BakeAwareObject
    {
        public override Guid ComponentGuid => new Guid("408ca3f4-0598-4f18-8b25-1f9646c53ef0");

        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        //Here we control name, nickname, description, category, sub-category as deafult we use typeofclass name
        public GooAdjacencyClusterParam()
            : base(typeof(AdjacencyCluster).Name, typeof(AdjacencyCluster).Name, typeof(AdjacencyCluster).FullName.Replace(".", " "), "Params", "SAM")
        {
        }

        protected override GH_GetterResult Prompt_Plural(ref List<GooAdjacencyCluster> values)
        {
            throw new NotImplementedException();
        }

        protected override GH_GetterResult Prompt_Singular(ref GooAdjacencyCluster value)
        {
            throw new NotImplementedException();
        }

        #region IGH_PreviewObject

        bool IGH_PreviewObject.Hidden { get; set; }
        bool IGH_PreviewObject.IsPreviewCapable => !VolatileData.IsEmpty;
        BoundingBox IGH_PreviewObject.ClippingBox => Preview_ComputeClippingBox();

        public bool IsBakeCapable => throw new NotImplementedException();

        void IGH_PreviewObject.DrawViewportMeshes(IGH_PreviewArgs args) => Preview_DrawMeshes(args);

        void IGH_PreviewObject.DrawViewportWires(IGH_PreviewArgs args) => Preview_DrawWires(args);

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

        #endregion IGH_PreviewObject
    }
}