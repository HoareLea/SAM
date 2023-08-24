using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalModifyZonesCategoryName : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("be852598-6bd0-4581-9bdd-a14eaf0ba091");

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
        public SAMAnalyticalModifyZonesCategoryName()
          : base("SAMAnalytical.ModifyZone", "SAMAnalytical.ModifyZone",
              "Modify Zone (Group) in Analytical Object",
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
                result.Add(new GH_SAMParam(new GooAnalyticalObjectParam { Name = "_analytical", NickName = "_analytical", Description = "SAM Analytical Object such as AdjacencyCluster or AnalyticalModel", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooGroupParam() { Name = "_zones", NickName = "_zones", Description = "SAM Analytical Object such as AdjacencyCluster or AnalyticalModel", Access = GH_ParamAccess.list, Optional = true }, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_String param_String = null;

                param_String = new global::Grasshopper.Kernel.Parameters.Param_String { Name = "zoneCategoryName_", NickName = "zoneCategoryName_", Description = "Zone Category Name", Access = GH_ParamAccess.item, Optional = true };
                result.Add(new GH_SAMParam(param_String, ParamVisibility.Voluntary));

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
                result.Add(new GH_SAMParam(new GooAnalyticalObjectParam() { Name = "analytical", NickName = "analytical", Description = "SAM Analytical Object such as AdjacencyCluster or AnalyticalModel", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooGroupParam() { Name = "zones", NickName = "zones", Description = "SAM GuidCollection representing Zones", Access = GH_ParamAccess.list }, ParamVisibility.Voluntary));
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
            int index;

            index = Params.IndexOfInputParam("_analytical");
            IAnalyticalObject analyticalObject = null;
            if(index == -1 || !dataAccess.GetData(index, ref analyticalObject) || analyticalObject == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            index = Params.IndexOfInputParam("zoneCategoryName_");
            string zoneCategory = null;
            if (index == -1 || !dataAccess.GetData(index, ref zoneCategory))
            {
                zoneCategory = null;
            }

            AdjacencyCluster adjacencyCluster = null;

            if(analyticalObject is AnalyticalModel)
            {
                AnalyticalModel analyticalModel = new AnalyticalModel((AnalyticalModel)analyticalObject);
                adjacencyCluster = analyticalModel.AdjacencyCluster;
            }
            else if(analyticalObject is AdjacencyCluster)
            {
                adjacencyCluster = new AdjacencyCluster((AdjacencyCluster)analyticalObject);
            }

            index = Params.IndexOfInputParam("_zones");
            List<Zone> zones = new List<Zone>();
            if (index == -1 || !dataAccess.GetDataList(index, zones))
            {
                zones = adjacencyCluster.GetZones();
            }

            if(zones != null && zones.Count != 0 && zoneCategory != null)
            {
                HashSet<Guid> guids = new HashSet<Guid>(zones.ConvertAll(x => x.Guid));

                zones = adjacencyCluster.GetZones()?.FindAll(x => x != null && guids.Contains(x.Guid));

                for(int i =0; i < zones.Count; i++)
                {
                    Zone zone = new Zone(zones[i]);

                    zone.SetValue(ZoneParameter.ZoneCategory, zoneCategory);

                    adjacencyCluster.AddObject(zone);
                }
            }

            if (analyticalObject is AnalyticalModel)
            {
                analyticalObject = new AnalyticalModel((AnalyticalModel)analyticalObject, adjacencyCluster);
            }
            else if (analyticalObject is AdjacencyCluster)
            {
                analyticalObject = new AdjacencyCluster(adjacencyCluster);
            }


            index = Params.IndexOfOutputParam("analytical");
            if (index != -1)
            {
                dataAccess.SetData(index, analyticalObject);
            }

            index = Params.IndexOfOutputParam("zones");
            if (index != -1)
            {
                dataAccess.SetDataList(index, zones);
            }

        }
    }
}