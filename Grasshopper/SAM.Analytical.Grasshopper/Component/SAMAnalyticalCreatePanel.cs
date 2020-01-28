using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;

using SAM.Analytical.Grasshopper.Properties;
using SAM.Geometry;
using SAM.Geometry.Grasshopper;
using SAM.Geometry.Spatial;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalCreatePanel : GH_Component
    {
        private List<Panel> panels = null;
        
        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalCreatePanel()
          : base("SAMAnalytical.CreatePanel", "SAMAnalytical.CreatePanel",
              "Create SAM Analytical Panel",
              "SAM", "Analytical")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            inputParamManager.AddGenericParameter("_geometry", "geometry", "Geometry", GH_ParamAccess.list);
            inputParamManager.AddGenericParameter("_panelType", "panelType", "PanelType", GH_ParamAccess.item);
            inputParamManager.AddGenericParameter("_construction", "construction", "Construction", GH_ParamAccess.item);
            inputParamManager.AddBooleanParameter("simplify_", "simplify", "Simplify", GH_ParamAccess.item, true);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddGenericParameter("Panel", "Panel", "SAM Analytical Panel", GH_ParamAccess.list);
        }

        public override void DrawViewportWires(IGH_PreviewArgs args)
        {
            base.DrawViewportWires(args);

            if (panels == null || panels.Count == 0)
                return;

            foreach(Panel panel in panels)
            {
                Boundary3D boundary3D = panel.Boundary3D;
                if (boundary3D == null)
                    return;

                Rhino.Display.DisplayMaterial displayMaterial = args.ShadeMaterial;
                if (this.Attributes.Selected)
                    displayMaterial = args.ShadeMaterial_Selected;

                Rhino.Geometry.Brep brep = boundary3D.ToRhino();
                if(brep != null)
                    args.Display.DrawBrepShaded(brep, displayMaterial);

                IEnumerable<Edge3DLoop> edge3DLoops = boundary3D.GetInternalEdge3DLoops();
                if (edge3DLoops != null)
                {
                    foreach (Edge3DLoop edge3DLoop in edge3DLoops)
                    {
                        List<Edge3D> edge3Ds = edge3DLoop.Edge3Ds;
                        if (edge3Ds == null || edge3Ds.Count == 0)
                            continue;

                        List<Rhino.Geometry.Point3d> point3ds = edge3Ds.ConvertAll(x => x.Curve3D.GetStart().ToRhino());
                        if (point3ds.Count == 0)
                            continue;

                        point3ds.Add(point3ds[0]);

                        args.Display.DrawPolyline(point3ds, System.Drawing.Color.BlueViolet);
                    }

                }

                args.Display.DrawBrepWires(boundary3D.GetFace().ToRhino_Brep(), System.Drawing.Color.Blue);
                //args.Display.DrawBrepShaded(boundary3D.GetFace().ToRhino_Brep(), args.ShadeMaterial);
            }
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            panels = null;
            
            bool simplyfy = false;
            if (!dataAccess.GetData<bool>(3, ref simplyfy))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            GH_ObjectWrapper objectWrapper = null;

            
            if (!dataAccess.GetData(1, ref objectWrapper))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            PanelType panelType = PanelType.Undefined;
            if(objectWrapper.Value is GH_String)
                panelType = Query.PanelType(((GH_String)objectWrapper.Value).Value);
            else
                panelType = Query.PanelType(objectWrapper.Value);

            Construction aConstruction = null;
            dataAccess.GetData(2, ref aConstruction);

            List<object> objects = new List<object>();
            if (!dataAccess.GetDataList<object>(0, objects) || objects == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            //object obj = objectWrapper.Value;

            List<List<IGeometry3D>> geometry3DsList = new List<List<IGeometry3D>>();

            foreach (object @object in objects)
            {
                if (@object is IGH_GeometricGoo)
                {
                    List<IGeometry3D> geometry3Ds = ((IGH_GeometricGoo)@object).ToSAM(simplyfy).Cast<IGeometry3D>().ToList();
                    if (geometry3Ds != null && geometry3Ds.Count > 0)
                        geometry3DsList.Add(geometry3Ds);
                }

                if (@object is GH_ObjectWrapper)
                {
                   objectWrapper = ((GH_ObjectWrapper)@object);
                    if (objectWrapper.Value is IGeometry3D)
                        geometry3DsList.Add(new List<IGeometry3D>() { (IGeometry3D)objectWrapper.Value });

                }
            }

            if (geometry3DsList == null || geometry3DsList.Count == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            panels = new List<Panel>();

            foreach(List<IGeometry3D> geometry3Ds in geometry3DsList)
            {
                List<Face> faces = Geometry.Query.Faces(geometry3Ds);
                if (faces == null)
                    continue;

                List<Boundary3D> boundary3Ds = null;

                if (!Boundary3D.TryGetBoundary3Ds(faces, out boundary3Ds))
                    continue;

                foreach (Boundary3D boundary3D in boundary3Ds)
                    panels.Add(new Panel(aConstruction, panelType, boundary3D));
            }


            if(panels.Count == 1)
            {
                dataAccess.SetData(0, panels[0]);
            }
            else
            {
                dataAccess.SetDataList(0, panels);
            }     
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return Resources.SAM_Small;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("35ef8f3e-1cf2-407d-b2ed-33bf371ea161"); }
        }
    }
}