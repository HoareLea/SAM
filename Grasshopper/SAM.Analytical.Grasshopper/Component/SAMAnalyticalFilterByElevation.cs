using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
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

            index = inputParamManager.AddGenericParameter("_elevation", "_elevation", "Elevation", GH_ParamAccess.item);
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
            List<GH_ObjectWrapper> objectWrappers = new List<GH_ObjectWrapper>();
            if (!dataAccess.GetDataList(0, objectWrappers))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<Core.SAMObject> sAMObjects = new List<Core.SAMObject>();
            foreach(GH_ObjectWrapper objectWrapper in objectWrappers)
            {
                if(objectWrapper?.Value is Core.SAMObject)
                {
                    sAMObjects.Add((Core.SAMObject)objectWrapper.Value);
                }
            }

            GH_ObjectWrapper objectWrapper_Elevation = null;
            if (!dataAccess.GetData(1, ref objectWrapper_Elevation))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            double elevation = double.NaN;

            object @object = objectWrapper_Elevation.Value;
            if (@object is IGH_Goo)
            {
                @object = (@object as dynamic).Value;
            }


            if (@object is double)
            {
                elevation = (double)@object;
            }
            else if(@object is Architectural.Level)
            {
                elevation = ((Architectural.Level)@object).Elevation;
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

            List<Panel> panels = new List<Panel>();
            List<Space> spaces = new List<Space>();
            foreach (Core.SAMObject sAMObject in sAMObjects)
            {
                if(sAMObject is Panel)
                {
                    panels.Add((Panel)sAMObject);
                }
                else if(sAMObject is Space)
                {
                    spaces.Add((Space)sAMObject);
                }
            }
            List<Panel> panels_Lower = null;
            List<Panel> panels_Upper = null;

            List<Panel> panels_Temp = panels?.FilterByElevation(elevation, out panels_Lower, out panels_Upper, tolerance);
            panels_Temp?.ForEach(x => result.Add(x));
            panels_Lower?.ForEach(x => result_Lower.Add(x));
            panels_Upper?.ForEach(x => result_Upper.Add(x));

            List<Space> spaces_Lower = null;
            List<Space> spaces_Upper = null;

            List<Space> spaces_Temp = spaces?.FilterByElevation(elevation, out spaces_Lower, out spaces_Upper, tolerance);
            spaces_Temp?.ForEach(x => result.Add(x));
            spaces_Lower?.ForEach(x => result_Lower.Add(x));
            spaces_Upper?.ForEach(x => result_Upper.Add(x));

            dataAccess.SetDataList(0, result);
            dataAccess.SetDataList(1, result_Upper);
            dataAccess.SetDataList(2, result_Lower);
        }
    }
}