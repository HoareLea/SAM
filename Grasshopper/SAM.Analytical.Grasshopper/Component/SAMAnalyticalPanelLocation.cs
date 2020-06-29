using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using SAM.Geometry.Grasshopper;
using System;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalPanelLocation : GH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("161f6ee4-60a3-4699-ade7-a6fbe9a1e222");

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalPanelLocation()
          : base("SAMAnalytical.PanelLocation", "SAMAnalytical.PanelLocation",
              "Location of SAM Analytical Panel",
              "SAM", "Analytical")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            inputParamManager.AddParameter(new GooPanelParam(), "_panel", "_panel", "SAM Analytical Panel", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddPointParameter("Origin", "Origin", "Origin Point", GH_ParamAccess.item);
            outputParamManager.AddNumberParameter("Tilt", "Tilt", "Tilt of SAM Analytical Panel", GH_ParamAccess.item);
            outputParamManager.AddNumberParameter("Azimuth", "Azimuth", "Azimuth of SAM Analytical Panel", GH_ParamAccess.item);
            outputParamManager.AddVectorParameter("Normal", "Normal", "Normal of SAM Analytical Panel", GH_ParamAccess.item);
            outputParamManager.AddPointParameter("InternalPoint", "InternalPoint", "InternalPoint of SAM Analytical Panel", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">
        /// The DA object is used to retrieve from inputs and store in outputs.
        /// </param>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            Panel panel = null;
            if (!dataAccess.GetData(0, ref panel) || panel == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            Geometry.Spatial.Plane plane = panel.Plane;
            if(panel == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            dataAccess.SetData(0, plane.Origin.ToGrasshopper());
            dataAccess.SetData(1, panel.Tilt());
            dataAccess.SetData(2, panel.Azimuth());
            dataAccess.SetData(3, plane.Normal.ToGrasshopper());
            dataAccess.SetData(4, panel.GetInternalPoint3D());
        }
    }
}