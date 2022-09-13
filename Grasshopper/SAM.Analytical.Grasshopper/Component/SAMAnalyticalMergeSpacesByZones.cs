using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalMergeSpacesByZones : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("e2541b6f-aadd-48c1-aed5-16e7e67c3171");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.0";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        public override GH_Exposure Exposure => GH_Exposure.primary;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalMergeSpacesByZones()
          : base("SAMAnalytical.MergeSpacesByZones", "SAMAnalytical.MergeSpacesByZones",
              "Merge Analytical Spaces By Zones. Removes air panels and combines spaces",
              "SAM WIP", "Analytical")
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
                result.Add(new GH_SAMParam(new GooAnalyticalObjectParam() { Name = "_analytical", NickName = "_analytical", Description = "SAM Analytical Object such as AdjacencyCluster or AnalyticalModel", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooGroupParam() { Name = "_zones_", NickName = "_zones_", Description = "SAM Analytical Zones", Access = GH_ParamAccess.list, Optional = true }, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_Boolean paramBoolean;

                paramBoolean = new global::Grasshopper.Kernel.Parameters.Param_Boolean() { Name = "run_", NickName = "run_", Description = "Run", Access = GH_ParamAccess.item, Optional = true };
                paramBoolean.SetPersistentData(false);
                result.Add(new GH_SAMParam(paramBoolean, ParamVisibility.Binding));

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
                result.Add(new GH_SAMParam(new GooAnalyticalObjectParam { Name = "analytical", NickName = "analytical", Description = "SAM Analytical", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooSpaceParam() { Name = "spaces", NickName = "spaces", Description = "SAM Analytical Spaces", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooPanelParam() { Name = "panels", NickName = "panels", Description = "SAM Analytical Panels", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
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

            index = Params.IndexOfInputParam("_analytical");
            SAMObject sAMObject = null;
            if (index == -1 || !dataAccess.GetData(index, ref sAMObject) || sAMObject == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            AdjacencyCluster adjacencyCluster = null;
            if(sAMObject is AdjacencyCluster)
            {
                adjacencyCluster = new AdjacencyCluster((AdjacencyCluster)sAMObject);
            }
            else if(sAMObject is AnalyticalModel)
            {
                adjacencyCluster = ((AnalyticalModel)sAMObject).AdjacencyCluster;
            }

            index = Params.IndexOfInputParam("_zones_");
            List<Zone> zones = new List<Zone>();
            if (index != -1)
            {
                dataAccess.GetDataList(index, zones);
            }

            if (zones == null || zones.Count == 0)
            {
                zones = adjacencyCluster.GetObjects<Zone>();
            }

            List<Panel> panels_Result = null;
            List<Space> spaces_Result = null;

            if(zones != null)
            {

                panels_Result = new List<Panel>();
                spaces_Result = new List<Space>();

                foreach (Zone zone in zones)
                {
                    List<Space> spaces = adjacencyCluster.GetRelatedObjects<Space>(zone);
                    if(spaces != null && spaces.Count > 1)
                    {
                        List<Space> spaces_Merge = adjacencyCluster.MergeSpaces(out List<Panel> panels_Merge, spaces?.FindAll(x => x != null).ConvertAll(x => x.Guid));
                        if(spaces_Merge != null)
                        {
                            foreach(Space space_Merge in spaces_Merge)
                            {
                                if(spaces_Result.Find(x => x.Guid == space_Merge.Guid) == null)
                                {
                                    spaces_Result.Add(space_Merge);
                                }
                            }
                        }

                        if (panels_Merge != null)
                        {
                            foreach (Panel panel_Merge in panels_Merge)
                            {
                                if (panels_Result.Find(x => x.Guid == panel_Merge.Guid) == null)
                                {
                                    panels_Result.Add(panel_Merge);
                                }
                            }
                        }
                    }
                }
            }

            if (sAMObject is AdjacencyCluster)
            {
                sAMObject = adjacencyCluster;
            }
            else if (sAMObject is AnalyticalModel)
            {
                sAMObject = new AnalyticalModel((AnalyticalModel)sAMObject, adjacencyCluster);
            }

            index = Params.IndexOfOutputParam("analytical");
            if (index != -1)
                dataAccess.SetData(index, sAMObject);

            index = Params.IndexOfOutputParam("spaces");
            if (index != -1)
                dataAccess.SetDataList(index, spaces_Result?.ConvertAll(x => new GooSpace(x)));

            index = Params.IndexOfOutputParam("panels");
            if (index != -1)
                dataAccess.SetDataList(index, panels_Result?.ConvertAll(x => new GooPanel(x)));
        }
    }
}