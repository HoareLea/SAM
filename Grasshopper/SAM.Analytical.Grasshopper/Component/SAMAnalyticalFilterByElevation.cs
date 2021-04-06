using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalFilterByElevation : GH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("25f4e28b-071f-4d82-aa0f-ed48055ed607");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.2";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalFilterByElevation()
          : base("SAMAnalytical.FilterByElevation", "SAMAnalytical.FilterByElevation",
              "Filter Analytical Objects By Elevation",
              "SAM", "Analytical")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            int index;

            index = inputParamManager.AddGenericParameter("_analyticals", "_analyticals", "SAM Analytical Panels or Spaces", GH_ParamAccess.list);
            inputParamManager[index].DataMapping = GH_DataMapping.Flatten;

            index = inputParamManager.AddNumberParameter("_elevation", "_elevation", "Elevation", GH_ParamAccess.item);
            index = inputParamManager.AddNumberParameter("_tolerance", "_tolerance", "Tolerance", GH_ParamAccess.item, Core.Tolerance.Distance);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddGenericParameter("Analyticals", "Analyticals", "SAM Analytical Panels or Spaces", GH_ParamAccess.list);
            outputParamManager.AddGenericParameter("UpperAnalyticals", "UpperAnalyticals", "Upper SAM Analytical Panels or Spaces", GH_ParamAccess.list);
            outputParamManager.AddGenericParameter("LowerAnalyticals", "LowerAnalyticals", "Lower SAM Analytical Panels or Spaces", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">
        /// The DA object is used to retrieve from inputs and store in outputs.
        /// </param>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            List<Core.SAMObject> sAMObjects = new List<Core.SAMObject>();
            if (!dataAccess.GetDataList(0, sAMObjects))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            double elevation = double.NaN;
            if (!dataAccess.GetData(1, ref elevation))
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

            List<Core.SAMObject> result = new List<Core.SAMObject>();
            List<Core.SAMObject> result_Upper = new List<Core.SAMObject>();
            List<Core.SAMObject> result_Lower = new List<Core.SAMObject>();
            foreach (Core.SAMObject sAMObject in sAMObjects)
            {
                
                if(sAMObject is Panel)
                {
                    Panel panel = (Panel)sAMObject;
                    
                    double min = panel.MinElevation();
                    double max = panel.MaxElevation();

                    if (min - tolerance <= elevation && max + tolerance >= elevation)
                    {
                        if (System.Math.Abs(max - min) > tolerance && System.Math.Abs(max - elevation) < tolerance)
                        {
                            result_Lower.Add(panel);
                            continue;
                        }


                        result.Add(panel);
                    }
                    else
                    {
                        if (min >= elevation)
                            result_Upper.Add(panel);
                        else
                            result_Lower.Add(panel);
                    }
                }
                else if(sAMObject is Space)
                {
                    Geometry.Spatial.Point3D location = ((Space)sAMObject).Location;
                    if (location == null)
                        continue;

                    double z = location.Z;
                    double difference = z - elevation;

                    if (System.Math.Abs(difference) <= tolerance)
                        result.Add(sAMObject);
                    else if (difference >= elevation)
                        result_Upper.Add(sAMObject);
                    else
                        result_Lower.Add(sAMObject);
                }

            }

            dataAccess.SetDataList(0, result);
            dataAccess.SetDataList(1, result_Upper);
            dataAccess.SetDataList(2, result_Lower);
        }
    }
}