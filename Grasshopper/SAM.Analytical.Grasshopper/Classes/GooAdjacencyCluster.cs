using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino;
using Rhino.DocObjects;
using Rhino.Geometry;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using SAM.Geometry.Grasshopper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SAM.Analytical.Grasshopper
{
    public class GooAdjacencyCluster : GooJSAMObject<AdjacencyCluster>, IGH_PreviewData, IGH_BakeAwareData
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
                    {
                        Geometry.Spatial.BoundingBox3D boundingBox3D = panel?.GetBoundingBox();
                        if (boundingBox3D == null)
                            continue;

                        boundingBox3Ds.Add(boundingBox3D);
                    }
                }

                IEnumerable<Space> spaces = Value.GetSpaces();
                if (spaces != null)
                {
                    foreach (Space space in spaces)
                    {
                        if (space == null)
                            continue;

                        Geometry.Spatial.Point3D location = space.Location;
                        if (location == null)
                            continue;

                        boundingBox3Ds.Add(location.GetBoundingBox(1));
                    }
                }

                if (boundingBox3Ds == null)
                    return BoundingBox.Empty;

                boundingBox3Ds.RemoveAll(x => x == null);

                if (boundingBox3Ds.Count == 0)
                    return BoundingBox.Empty;

                return Geometry.Rhino.Convert.ToRhino(new Geometry.Spatial.BoundingBox3D(boundingBox3Ds));
            }
        }

        public override IGH_Goo Duplicate()
        {
            return new GooAdjacencyCluster(Value);
        }

        public void DrawViewportWires(GH_PreviewWireArgs args)
        {
            List<Space> spaces = Value?.GetSpaces();
            if(spaces != null)
            {
                foreach(Space space in spaces)
                {
                    Point3d? point3d = Geometry.Rhino.Convert.ToRhino(space?.Location);
                    if (point3d == null || !point3d.HasValue)
                        continue;

                    args.Pipeline.DrawPoint(point3d.Value);
                }
            }

            List<Panel> panels = Value?.GetPanels();
            if (panels == null)
                return;

            Geometry.Spatial.BoundingBox3D boundingBox3D = null;
            if(args.Viewport.IsValidFrustum)
            {
                BoundingBox boundingBox = args.Viewport.GetFrustumBoundingBox();
                boundingBox3D = new Geometry.Spatial.BoundingBox3D(new Geometry.Spatial.Point3D[] { Geometry.Rhino.Convert.ToSAM(boundingBox.Min), Geometry.Rhino.Convert.ToSAM(boundingBox.Max)});
            }

            foreach (Panel panel in panels)
            {
                List<Space> spaces_Panel = Value.GetSpaces(panel);
                if (spaces_Panel != null && spaces_Panel.Count > 1)
                    continue;

                PlanarBoundary3D planarBoundary3D = panel.PlanarBoundary3D;
                if (planarBoundary3D == null)
                    continue;

                if(boundingBox3D != null)
                {
                    Geometry.Spatial.BoundingBox3D boundingBox3D_Temp = planarBoundary3D.GetBoundingBox();
                    if(boundingBox3D_Temp != null)
                    {
                        if (!boundingBox3D.Inside(boundingBox3D_Temp) && !boundingBox3D.Intersect(boundingBox3D_Temp))
                            continue;
                    }
                }
                
                Dictionary<BoundaryEdge3DLoop, System.Drawing.Color> aDictionary = new Dictionary<BoundaryEdge3DLoop, System.Drawing.Color>();

                //Assign Color for Edges
                aDictionary[planarBoundary3D.GetExternalEdge3DLoop()] = System.Drawing.Color.DarkRed;

                IEnumerable<BoundaryEdge3DLoop> edge3DLoops = planarBoundary3D.GetInternalEdge3DLoops();
                if (edge3DLoops != null)
                {
                    foreach (BoundaryEdge3DLoop edge3DLoop in edge3DLoops)
                        aDictionary[edge3DLoop] = System.Drawing.Color.BlueViolet;
                }

                foreach (KeyValuePair<BoundaryEdge3DLoop, System.Drawing.Color> keyValuePair in aDictionary)
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
        }

        public void DrawViewportMeshes(GH_PreviewMeshArgs args)
        {         
            List<Panel> panels = Value?.GetPanels();
            if (panels == null)
                return;

            Geometry.Spatial.BoundingBox3D boundingBox3D = null;
            if (args.Viewport.IsValidFrustum)
            {
                BoundingBox boundingBox = args.Viewport.GetFrustumBoundingBox();
                boundingBox3D = new Geometry.Spatial.BoundingBox3D(new Geometry.Spatial.Point3D[] { Geometry.Rhino.Convert.ToSAM(boundingBox.Min), Geometry.Rhino.Convert.ToSAM(boundingBox.Max) });
            }

            List<PlanarBoundary3D> planarBoundary3Ds = new List<PlanarBoundary3D>();
            for (int i = 0; i < panels.Count; i++)
                planarBoundary3Ds.Add(null);

            Parallel.For(0, panels.Count, (int i) => 
            {
                Panel panel = panels[i];
                
                List<Space> spaces = Value.GetSpaces(panel);
                if (spaces != null && spaces.Count > 1)
                    return;

                PlanarBoundary3D planarBoundary3D = panel.PlanarBoundary3D;
                if (planarBoundary3D == null)
                    return;

                if (boundingBox3D != null)
                {
                    Geometry.Spatial.BoundingBox3D boundingBox3D_Temp = planarBoundary3D.GetBoundingBox();
                    if (boundingBox3D_Temp != null)
                    {
                        if (!boundingBox3D.Inside(boundingBox3D_Temp) && !boundingBox3D.Intersect(boundingBox3D_Temp))
                            return;
                    }
                }

                planarBoundary3Ds[i] = planarBoundary3D;
            });

            foreach(PlanarBoundary3D planarBoundary3D in planarBoundary3Ds)
            {
                if (planarBoundary3D == null)
                    continue;
                
                Brep brep = Rhino.Convert.ToRhino(planarBoundary3D);
                if (brep == null)
                    continue;

                args.Pipeline.DrawBrepShaded(brep, args.Material);
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
                Brep brep = Rhino.Convert.ToRhino(panel);
                if (brep == null)
                    continue;

                breps.Add(brep);
            }

            if (breps == null || breps.Count == 0)
                return false;

            Brep result = Brep.MergeBreps(breps, Core.Tolerance.MacroDistance); //Tolerance has been changed from Core.Tolerance.Distance
            if (result == null)
                return false;

            obj_guid = doc.Objects.AddBrep(result);
            return true;
        }

        public override bool CastFrom(object source)
        {
            if (source is AdjacencyCluster)
            {
                Value = (AdjacencyCluster)source;
                return true;
            }

            if (typeof(IGH_Goo).IsAssignableFrom(source.GetType()))
            {
                try
                {
                    source = (source as dynamic).Value;
                }
                catch
                {
                }

                if (source is AdjacencyCluster)
                {
                    Value = (AdjacencyCluster)source;
                    return true;
                }
            }

            return base.CastFrom(source);
        }

        public override bool CastTo<Y>(ref Y target)
        {
            if (Value == null)
                return false;

            if (typeof(Y).IsAssignableFrom(typeof(GH_Mesh)))
            {
                target = (Y)(object)Value.ToGrasshopper_Mesh();
                return true;
            }

            //if (typeof(Y).IsAssignableFrom(typeof(GH_Brep)))
            //{
            //    List<Geometry.Spatial.Shell> shells = Value.GetShells();
            //    if(shells != null)
            //    {
            //        Brep brep = Brep.MergeBreps(shells.ConvertAll(x => x.ToRhino()), Core.Tolerance.MacroDistance);
            //        if(brep != null)
            //        {
            //            target = (Y)(object)new GH_Brep(brep);
            //            return true;
            //        }
            //    }
            //}

            return base.CastTo(ref target);
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

        public bool IsBakeCapable => true;

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

        public void BakeGeometry_ByPanelType(RhinoDoc doc)
        {
            Modify.BakeGeometry_ByPanelType(doc, VolatileData, true, Core.Tolerance.Distance);
        }

        public void BakeGeometry_ByConstruction(RhinoDoc doc)
        {
            Modify.BakeGeometry_ByConstruction(doc, VolatileData, true, Core.Tolerance.Distance);
        }

        public override void AppendAdditionalMenuItems(System.Windows.Forms.ToolStripDropDown menu)
        {
            Menu_AppendItem(menu, "Bake By Type", Menu_BakeByPanelType, VolatileData.AllData(true).Any());
            Menu_AppendItem(menu, "Bake By Construction", Menu_BakeByConstruction, VolatileData.AllData(true).Any());

            base.AppendAdditionalMenuItems(menu);
        }

        private void Menu_BakeByPanelType(object sender, EventArgs e)
        {
            BakeGeometry_ByPanelType(RhinoDoc.ActiveDoc);
        }

        private void Menu_BakeByConstruction(object sender, EventArgs e)
        {
            BakeGeometry_ByConstruction(RhinoDoc.ActiveDoc);
        }

        #endregion IGH_PreviewObject
    }
}