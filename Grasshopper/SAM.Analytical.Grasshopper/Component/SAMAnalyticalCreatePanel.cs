using System;
using System.Collections.Generic;
using System.Linq;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;

using SAM.Analytical.Grasshopper.Properties;
using SAM.Geometry.Grasshopper;
using SAM.Geometry.Spatial;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalCreatePanel : GH_Component
    {      
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
            inputParamManager.AddGenericParameter("_geometry", "geometry", "Geometry", GH_ParamAccess.item);
            inputParamManager.AddGenericParameter("_panelType", "panelType", "PanelType", GH_ParamAccess.item);
            inputParamManager.AddGenericParameter("_construction", "construction", "Construction", GH_ParamAccess.item);
            inputParamManager.AddBooleanParameter("simplify_", "simplify", "Simplify", GH_ParamAccess.item, true);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddParameter(new PanelParam(), "Panels", "Panels", "SAM Analytical Panels", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {        
            bool simplyfy = false;
            if (!dataAccess.GetData<bool>(3, ref simplyfy))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            object @object = null;
            if (!dataAccess.GetData(0, ref @object))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<IGeometry3D> geometry3Ds = null;
            if (@object is IGH_GeometricGoo)
                geometry3Ds = ((IGH_GeometricGoo)@object).ToSAM(simplyfy).Cast<IGeometry3D>().ToList();

            if (@object is GH_ObjectWrapper)
            {
                GH_ObjectWrapper objectWrapper_Temp = ((GH_ObjectWrapper)@object);
                if (objectWrapper_Temp.Value is IGeometry3D)
                    geometry3Ds = new List<IGeometry3D>() { (IGeometry3D)objectWrapper_Temp.Value };
            }

            if(geometry3Ds == null || geometry3Ds.Count() == 0)
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
            if (objectWrapper.Value is GH_String)
                panelType = Query.PanelType(((GH_String)objectWrapper.Value).Value);
            else
                panelType = Query.PanelType(objectWrapper.Value);

            Construction aConstruction = null;
            dataAccess.GetData(2, ref aConstruction);

            List<Panel> panels = new List<Panel>();

            List<Face> faces = Geometry.Query.Faces(geometry3Ds);
            if (faces == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<Boundary3D> boundary3Ds = null;

            if (!Boundary3D.TryGetBoundary3Ds(faces, out boundary3Ds))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            foreach (Boundary3D boundary3D in boundary3Ds)
                panels.Add(new Panel(aConstruction, panelType, boundary3D));


            if (panels.Count == 1)
            {
                dataAccess.SetData(0, new GooPanel(panels[0]));
            }
            else
            {
                dataAccess.SetDataList(0, panels.ConvertAll(x => new GooPanel(x)));
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