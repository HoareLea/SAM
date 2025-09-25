using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalMergeAdjacencyCluster : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("1e26b699-2dad-4769-8c53-833c707e1a6c");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.0";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
                protected override System.Drawing.Bitmap Icon => Core.Convert.ToBitmap(Resources.SAM_Small);

        public override GH_Exposure Exposure => GH_Exposure.primary;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalMergeAdjacencyCluster()
          : base("SAMAnalytical.MergeAdjacencyCluster", "SAMAnalytical.MergeAdjacencyCluster",
              "Merge AdjacencyCluster",
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
                List<GH_SAMParam> result = [];

                GooAnalyticalObjectParam gooAnalyticalObjectParam = new () { Name = "_analyticalObject", NickName = "_analyticalObject", Description = "SAM AdjacencyCluster or AnalyticalModel", Access = GH_ParamAccess.item };
                result.Add(new GH_SAMParam(gooAnalyticalObjectParam, ParamVisibility.Binding));

                GooAnalyticalObjectParam gooAdjacencyClusterParam = new() { Name = "_adjacencyClusters", NickName = "_adjacencyClusters", Description = "SAM AdjacencyClusters", Access = GH_ParamAccess.list };
                result.Add(new GH_SAMParam(gooAdjacencyClusterParam, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_Number paramNumber;

                paramNumber = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "silverSpacing_", NickName = "silverSpacing_", Description = "Silver Spacing for computation of Spaces", Access = GH_ParamAccess.item };
                paramNumber.SetPersistentData(Core.Tolerance.MacroDistance);
                result.Add(new GH_SAMParam(paramNumber, ParamVisibility.Voluntary));

                paramNumber = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "tolerance_", NickName = "tolerance_", Description = "Tolerance", Access = GH_ParamAccess.item };
                paramNumber.SetPersistentData(Core.Tolerance.Distance);
                result.Add(new GH_SAMParam(paramNumber, ParamVisibility.Binding));


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
                List<GH_SAMParam> result =
                [
                    new GH_SAMParam(new GooAnalyticalObjectParam() { Name = "AnalyticalObject", NickName = "AnalyticalObject", Description = "SAM AdjacencyCluster or AnalyticalModel", Access = GH_ParamAccess.item }, ParamVisibility.Binding)
                ];
                return [.. result];
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

            index = Params.IndexOfInputParam("_analyticalObject");
            IAnalyticalObject analyticalObject = null;
            if (index == -1 || !dataAccess.GetData(index, ref analyticalObject) || analyticalObject == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            if(analyticalObject is AdjacencyCluster adjacencyCluster)
            {
                adjacencyCluster = new AdjacencyCluster(adjacencyCluster);
            }
            else if(analyticalObject is AnalyticalModel analyticalModel)
            {
                adjacencyCluster = new AdjacencyCluster(analyticalModel.AdjacencyCluster);
            }
            else
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<AdjacencyCluster> adjacencyClusters = [];
            index = Params.IndexOfInputParam("_adjacencyClusters");
            if (index == -1 || !dataAccess.GetDataList(index, adjacencyClusters) || adjacencyClusters == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            index = Params.IndexOfInputParam("silverSpacing_");
            double silverSpacing = Core.Tolerance.MacroDistance;
            if (index != -1)
            {
                dataAccess.GetData(index, ref silverSpacing);
            }

            index = Params.IndexOfInputParam("tolerance_");
            double tolerance = Core.Tolerance.Distance;
            if (index != -1)
            {
                dataAccess.GetData(index, ref tolerance);
            }

            foreach(AdjacencyCluster adjacencyCluster_Source in adjacencyClusters)
            {
                Analytical.Modify.Merge(adjacencyCluster, adjacencyCluster_Source, silverSpacing: silverSpacing, tolerance_Distance: tolerance);
            }

            if(analyticalObject is AdjacencyCluster)
            {
                analyticalObject = adjacencyCluster;
            }
            else if(analyticalObject is AnalyticalModel analyticalModel)
            {
                analyticalObject = new AnalyticalModel(analyticalModel, adjacencyCluster);
            }

            index = Params.IndexOfOutputParam("AnalyticalObject");
            if (index != -1)
            {
                dataAccess.SetData(index, new GooAnalyticalObject(analyticalObject));
            }
        }
    }
}