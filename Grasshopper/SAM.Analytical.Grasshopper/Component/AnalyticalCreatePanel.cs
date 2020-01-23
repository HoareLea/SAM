using System;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;

using SAM.Analytical.Grasshopper.Properties;
using SAM.Geometry;
using SAM.Geometry.Grasshopper;
using SAM.Geometry.Spatial;

namespace SAM.Analytical.Grasshopper
{
    public class AnalyticalCreatePanel : GH_Component
    {
        private Panel panel;
        
        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public AnalyticalCreatePanel()
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
            inputParamManager.AddGenericParameter("_geometry", "geo", "Geometry", GH_ParamAccess.item);
            inputParamManager.AddGenericParameter("_panelType", "panelType", "PanelType", GH_ParamAccess.item);
            inputParamManager.AddGenericParameter("_construction", "construction", "Construction", GH_ParamAccess.item);
            inputParamManager.AddBooleanParameter("simplify_", "Smfy", "Simplify", GH_ParamAccess.item, true);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddGenericParameter("Panel", "pnl", "SAM Analytical Panel", GH_ParamAccess.item);
        }

        public override void DrawViewportWires(IGH_PreviewArgs args)
        {
            base.DrawViewportWires(args);

            if (panel == null)
                return;

            args.Display.DrawPolyline(panel.ToPolycurveLoop().GetCurves().ConvertAll(x => x.GetStart().ToRhino()), System.Drawing.Color.Blue);
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

            if (!dataAccess.GetData(0, ref objectWrapper) || objectWrapper.Value == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            object obj = objectWrapper.Value;

            if (obj is IGH_GeometricGoo)
                obj = ((IGH_GeometricGoo)obj).ToSAM(simplyfy);

            if (!(obj is IGeometry))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            IClosed3D closed3D = obj as IClosed3D;

            if (obj == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Cannot convert geometry");
            }
                
            else
            {
                panel = new Panel(aConstruction, panelType, closed3D);
                dataAccess.SetData(0, panel);
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