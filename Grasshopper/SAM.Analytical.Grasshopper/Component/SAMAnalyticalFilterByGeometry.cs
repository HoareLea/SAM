using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using SAM.Geometry.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalFilterByGeometry : GH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("c8a0fb53-360b-4ab7-be97-49336b516847");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.0";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalFilterByGeometry()
          : base("SAMAnalytical.FilterByGeometry", "SAMAnalytical.FilterByGeometry",
              "Filter Analytical Objects By Geometry",
              "SAM", "Analytical")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            int index;

            index = inputParamManager.AddParameter(new GooPanelParam(), "_panels", "_panels", "SAM Analytical Panels", GH_ParamAccess.list);
            inputParamManager[index].DataMapping = GH_DataMapping.Flatten;

            index = inputParamManager.AddBrepParameter("_brep", "_brep", "Brep", GH_ParamAccess.item);
            index = inputParamManager.AddNumberParameter("_tolerance", "_tolerance", "Tolerance", GH_ParamAccess.item, Core.Tolerance.Distance);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddParameter(new GooPanelParam(), "Panels", "Panels", "SAM Analytical Panels", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">
        /// The DA object is used to retrieve from inputs and store in outputs.
        /// </param>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            List<Panel> panels = new List<Panel>();
            if (!dataAccess.GetDataList(0, panels))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            Rhino.Geometry.Brep brep = null;
            if (!dataAccess.GetData(1, ref brep))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            double tolerance = double.NaN;
            if (!dataAccess.GetData(2, ref tolerance))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            Geometry.Spatial.Shell shell = brep.ToSAM_Shell(true);
            if(shell == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<Panel> panels_Result = Analytical.Query.InRange(panels, shell, tolerance);

            dataAccess.SetDataList(0, panels_Result?.ConvertAll(x => new GooPanel(x)));
        }
    }
}