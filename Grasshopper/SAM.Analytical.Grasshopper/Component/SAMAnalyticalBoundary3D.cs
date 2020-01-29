using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;

using SAM.Analytical.Grasshopper.Properties;
using SAM.Geometry;
using SAM.Geometry.Grasshopper;
using SAM.Geometry.Spatial;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalBoundary3D : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalBoundary3D()
          : base("SAMAnalytical.Boundary3D", "SAMAnalytical.Boundary3D",
              "Gets SAM Analytical Boundary3D",
              "SAM", "Analytical")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            inputParamManager.AddGenericParameter("_panel", "panel", "SAM Analytical Panel", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddGenericParameter("Boundary3D", "Boundary3D", "SAM Analytical Boundary3D", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            Panel panel = null;
            if (!dataAccess.GetData(0, ref panel))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            dataAccess.SetData(0, new GooBoundary3D(panel.Boundary3D));

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
            get { return new Guid("7316ff6b-9688-4f82-8256-b3e27e4a958d"); }
        }
    }
}