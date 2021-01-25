using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalGetZonesByName : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("8e16e077-13d8-4880-ba19-e7de974afadb");

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
        public SAMAnalyticalGetZonesByName()
          : base("SAMAnalytical.GetZonesByName", "SAMAnalytical.GetZonesByName",
              "Get Zones (Groups) from Analytical Object",
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
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "_analytical", NickName = "_analytical", Description = "SAM Analytical Object such as AdjacencyCluster or AnalyticalModel", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "_name_", NickName = "_name_", Description = "Zone name", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "_zoneType_", NickName = "_zoneType_", Description = "ZoneType", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));
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
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "Zones", NickName = "Zones", Description = "SAM GuidCollections representing Zones", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooSpaceParam() { Name = "Spaces", NickName = "Spaces", Description = "SAM Analytical Spaces", Access = GH_ParamAccess.tree }, ParamVisibility.Binding));
                
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
            SAMObject sAMObject = null;
            if(index == -1 || !dataAccess.GetData(index, ref sAMObject) || sAMObject == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            index = Params.IndexOfInputParam("_name_");
            string name = null;
            if (index == -1)
                if (!dataAccess.GetData(index, ref name))
                    name = null;

            index = Params.IndexOfInputParam("_zoneType_");
            string zoneCategory = null;
            if (index == -1)
                if (!dataAccess.GetData(index, ref zoneCategory))
                    zoneCategory = null;

            AdjacencyCluster adjacencyCluster = null;

            if(sAMObject is AnalyticalModel)
                adjacencyCluster = ((AnalyticalModel)sAMObject)?.AdjacencyCluster;
            else if(sAMObject is AdjacencyCluster)
                adjacencyCluster = ((AdjacencyCluster)sAMObject);

            List<Zone> zones = adjacencyCluster?.Groups?.FindAll(x => x is Zone).ConvertAll(x => (Zone)x);
            if (name != null)
                zones.RemoveAll(x => x.Name != name);

            if (zoneCategory != null)
                zones.RemoveAll(x => x.GetValue<string>(ZoneParameter.ZoneCategory) != zoneCategory);

            index = Params.IndexOfOutputParam("Zones");
            if (index != -1)
                dataAccess.SetDataList(index, zones);

            index = Params.IndexOfOutputParam("Spaces");
            if (index != -1 && zones != null && adjacencyCluster != null)
            {
                DataTree<GooSpace> dataTree_Spaces = new DataTree<GooSpace>();
                int count = 0;
                foreach (Zone zone in zones)
                {
                    GH_Path path = new GH_Path(count);
                    List<Space> spaces = adjacencyCluster.GetObjects<Space>(zone);
                    if (spaces != null && spaces.Count > 0)
                        spaces.ForEach(x => dataTree_Spaces.Add(new GooSpace(x), path));

                    count++;
                }

                dataAccess.SetDataTree(index, dataTree_Spaces);
            }
                
        }
    }
}