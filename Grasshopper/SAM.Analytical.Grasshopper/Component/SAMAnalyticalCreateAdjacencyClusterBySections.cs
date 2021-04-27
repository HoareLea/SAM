using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalCreateAdjacencyClusterBySections : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("f73d4b1c-7051-4c6a-af42-2a44463caa41");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.2";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        public override GH_Exposure Exposure => GH_Exposure.primary;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalCreateAdjacencyClusterBySections()
          : base("SAMAnalytical.CreateAdjacencyClusterBySections", "SAMAnalytical.CreateAdjacencyClusterBySections",
              "Create AdjacencyCluster By Sections",
              "SAM", "Analytical")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override GH_SAMParam[] Inputs
        {
            get
            {
                List<GH_SAMParam> result = new List<GH_SAMParam>();

                GooPanelParam panelParam = new GooPanelParam() { Name = "_panels", NickName = "_panels", Description = "SAM Analytical Panels", Access = GH_ParamAccess.list };
                panelParam.DataMapping = GH_DataMapping.Flatten;
                result.Add(new GH_SAMParam(panelParam, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_Number paramNumber;

                paramNumber = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_elevations", NickName = "_elevations", Description = "Elevations", Access = GH_ParamAccess.list };
                result.Add(new GH_SAMParam(paramNumber, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_Interval paramInterval;

                paramInterval = new global::Grasshopper.Kernel.Parameters.Param_Interval() { Name = "_intervals", NickName = "_intervals", Description = "Intervals", Access = GH_ParamAccess.list };
                result.Add(new GH_SAMParam(paramNumber, ParamVisibility.Binding));

                paramNumber = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "tolerance_", NickName = "tolerance_", Description = "Tolerance", Access = GH_ParamAccess.item };
                paramNumber.SetPersistentData(Core.Tolerance.Distance);
                result.Add(new GH_SAMParam(paramNumber, ParamVisibility.Voluntary));


                return result.ToArray();
            }
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override GH_SAMParam[] Outputs
        {
            get
            {
                List<GH_SAMParam> result = new List<GH_SAMParam>();
                result.Add(new GH_SAMParam(new GooAdjacencyClusterParam() { Name = "adjacencyCluster", NickName = "adjacencyCluster", Description = "SAM Analytical AdjacencyCluster", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                return result.ToArray();
            }
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">
        /// The DA object is used to retrieve from inputs and store in outputs.
        /// </param>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            int index = -1;

            List<Panel> panels = new List<Panel>();
            index = Params.IndexOfInputParam("_panels");
            if (index == -1 || !dataAccess.GetDataList(index, panels) || panels == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<double> elevations = new List<double>();
            index = Params.IndexOfInputParam("_elevations");
            if (index == -1 || !dataAccess.GetDataList(index, elevations) || elevations == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<Interval> intervals = new List<Interval>();
            index = Params.IndexOfInputParam("_intervals");
            if (index == -1 || !dataAccess.GetDataList(index, intervals) || intervals == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            index = Params.IndexOfInputParam("tolerance");
            double tolerance = Core.Tolerance.Distance;
            if (index != -1)
            {
                dataAccess.GetData(index, ref tolerance);
            }


            AdjacencyCluster result = new AdjacencyCluster();

            for (int i = 0; i < elevations.Count; i++)
            {
                double elevation = elevations[i];

                Interval interval = intervals.Last();
                if (i < intervals.Count)
                    interval = intervals[i];

                Geometry.Spatial.Plane plane = Geometry.Spatial.Plane.WorldXY.GetMoved(new Vector3D(0, 0, elevation)) as Geometry.Spatial.Plane;

                Dictionary<Panel, List<ISAMGeometry3D>> dictionary = Analytical.Query.SectionDictionary<ISAMGeometry3D>(panels, plane, tolerance);


            }

            index = Params.IndexOfOutputParam("adjacencyCluster");
            if (index != -1)
                dataAccess.SetData(index, new GooAdjacencyCluster(result));
        }
    }
}