using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalCalculateDaylightFactor : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("48910687-cc6b-4637-b671-30b8b268e5f7");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.0";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        public override GH_Exposure Exposure => GH_Exposure.primary;

        protected override GH_SAMParam[] Inputs
        {
            get
            {
                List<GH_SAMParam> result = new List<GH_SAMParam>();
                result.Add(new GH_SAMParam(new GooAnalyticalObjectParam() { Name = "_analytical", NickName = "_analytical", Description = "SAM Analytical Object such as AnalyticalModel or AdjacencyCluster", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooSpaceParam() { Name = "spaces_", NickName = "spaces_", Description = "SAM Analytical Spaces", Access = GH_ParamAccess.list, Optional = true }, ParamVisibility.Binding));
                return result.ToArray();
            }
        }

        protected override GH_SAMParam[] Outputs
        {
            get
            {
                List<GH_SAMParam> result = new List<GH_SAMParam>();
                result.Add(new GH_SAMParam(new GooAnalyticalObjectParam() { Name = "analytical", NickName = "analytical", Description = "SAM Analytical Object", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                return result.ToArray();
            }
        }

        /// <summary>
        /// Updates PanelTypes for AdjacencyCluster
        /// </summary>
        public SAMAnalyticalCalculateDaylightFactor()
          : base("SAMAnalytical.DaylightFactor", "SAMAnalytical.DaylightFactor",
              "Calculates Daylight Factors",
              "SAM WIP", "Analytical")
        {
        }

        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            int index;

            index = Params.IndexOfInputParam("_analytical");
            if(index == -1)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            IAnalyticalObject analyticalObject = null;
            if (!dataAccess.GetData(index, ref analyticalObject) || analyticalObject == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            index = Params.IndexOfInputParam("spaces_");
            List<Space> spaces = new List<Space>();
            dataAccess.GetDataList(index, spaces);

            if(spaces != null && spaces.Count == 0)
            {
                spaces = null;
            }

            if(analyticalObject is AnalyticalModel || analyticalObject is AdjacencyCluster)
            {
                AdjacencyCluster adjacencyCluster = null;
                if(analyticalObject is AnalyticalModel)
                    adjacencyCluster = ((AnalyticalModel)analyticalObject).AdjacencyCluster;
                else if(analyticalObject is AdjacencyCluster)
                    adjacencyCluster = new AdjacencyCluster((AdjacencyCluster)analyticalObject);

                if(adjacencyCluster != null)
                {
                    adjacencyCluster.UpdateDaylightFactors(spaces);

                    if (analyticalObject is AnalyticalModel)
                        analyticalObject = new AnalyticalModel((AnalyticalModel)analyticalObject, adjacencyCluster);
                    else if (analyticalObject is AdjacencyCluster)
                        analyticalObject = adjacencyCluster;
                }
            }
            else if(analyticalObject is Space)
            {

            }

            index = Params.IndexOfOutputParam("Analytical");
            if (index != -1)
                dataAccess.SetData(index, analyticalObject);
        }
    }
}