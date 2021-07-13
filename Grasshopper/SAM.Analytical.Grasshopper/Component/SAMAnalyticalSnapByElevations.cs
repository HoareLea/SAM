using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalSnapByElevations : GH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("ad6aa954-4861-4248-9f73-fecfff2ca796");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.1";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalSnapByElevations()
          : base("SAMAnalytical.SnapByElevations", "SAMAnalytical.SnapByElevations",
              "Snap By Elevation",
              "SAM", "Analytical")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            inputParamManager.AddParameter(new GooPanelParam(), "_panels", "_panels", "SAM Analytical Panels", GH_ParamAccess.list);
            inputParamManager.AddGenericParameter("_elevations", "_elevations", "elevations", GH_ParamAccess.list);
            inputParamManager.AddNumberParameter("_minTolerance_", "_minTolerance_", "Minimal Tolerance", GH_ParamAccess.item, Core.Tolerance.MicroDistance);
            inputParamManager.AddNumberParameter("_maxTolerance_", "_maxTolerance_", "Maximal Tolerance", GH_ParamAccess.item, Core.Tolerance.MacroDistance);
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

            int index = Params.IndexOfInputParam("_elevations");
            List<GH_ObjectWrapper> objectWrappers_Elevation = new List<GH_ObjectWrapper>();
            if (index == -1 || !dataAccess.GetDataList(index, objectWrappers_Elevation) || objectWrappers_Elevation == null || objectWrappers_Elevation.Count == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Invalid Data");
                return;
            }

            List<double> elevations = new List<double>();

            foreach(GH_ObjectWrapper objectWrapper_Elevation in objectWrappers_Elevation)
            {
                double elevation = double.NaN;

                object @object = objectWrapper_Elevation?.Value;
                if (@object is IGH_Goo)
                {
                    @object = (@object as dynamic).Value;
                }

                if (@object is double)
                {
                    elevation = (double)@object;
                }
                else if (@object is string)
                {
                    if (double.TryParse((string)@object, out double elevation_Temp))
                        elevation = elevation_Temp;
                }
                else if (@object is Architectural.Level)
                {
                    elevation = ((Architectural.Level)@object).Elevation;
                }

                if(double.IsNaN(elevation))
                {
                    continue;
                }

                elevations.Add(elevation);
            }

            double minTolerance = double.NaN;
            if (!dataAccess.GetData(2, ref minTolerance) || double.IsNaN(minTolerance))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            double maxTolerance = double.NaN;
            if (!dataAccess.GetData(3, ref maxTolerance) || double.IsNaN(maxTolerance))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<Panel> result = Analytical.Query.SnapByElevations(panels, elevations, maxTolerance, minTolerance);
            dataAccess.SetDataList(0, result.ConvertAll(x => new GooPanel(x)));
        }
    }
}