using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper.Obsolete
{
    [Obsolete("Obsolete since 2021-01-27")]
    public class SAMAnalyticalGetZone : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("69b2cc45-087b-4a83-a500-3c51f1edddb9");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.0";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        public override GH_Exposure Exposure => GH_Exposure.hidden;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalGetZone()
          : base("SAMAnalytical.GetZone", "SAMAnalytical.GetZone",
              "Get Zone from Analytical Object",
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
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "_name", NickName = "_name", Description = "Zone Name", Access = GH_ParamAccess.item }, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_String param_String = new global::Grasshopper.Kernel.Parameters.Param_String { Name = "_zoneType", NickName = "_zoneType", Description = "SAM Analytical ZoneType", Access = GH_ParamAccess.item };
                result.Add(new GH_SAMParam(param_String, ParamVisibility.Binding));

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
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "Zone", NickName = "Zone", Description = "SAM GuidCollection representing Zone", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooSpaceParam() { Name = "Spaces", NickName = "Spaces", Description = "SAM Analytical Space", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                
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

            index = Params.IndexOfInputParam("_name");
            string name = null;
            if (index == -1 || !dataAccess.GetData(index, ref name) || string.IsNullOrWhiteSpace(name))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            index = Params.IndexOfInputParam("_zoneType");
            string zoneCategory = null;
            if (index == -1 || !dataAccess.GetData(index, ref zoneCategory) || string.IsNullOrWhiteSpace(zoneCategory))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            if(!Enum.TryParse(zoneCategory, out ZoneType zoneType))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<Space> spaces = null;
            Zone zone = null;
            if(sAMObject is AnalyticalModel)
            {
                AdjacencyCluster adjacencyCluster = ((AnalyticalModel)sAMObject).AdjacencyCluster;
                if(adjacencyCluster != null)
                {
                    zone = Analytical.Query.Zone(adjacencyCluster, name, zoneType);
                    if (zone != null)
                        spaces = adjacencyCluster.GetSpaces(zone);
                   
                }
            }
            else if(sAMObject is AdjacencyCluster)
            {
                AdjacencyCluster adjacencyCluster = (AdjacencyCluster)sAMObject;
                zone = Analytical.Query.Zone(adjacencyCluster, name, zoneType);
                if (zone != null)
                    spaces = adjacencyCluster.GetSpaces(zone);
            }

            index = Params.IndexOfOutputParam("Zone");
            if (index != -1)
                dataAccess.SetData(index, zone);

            index = Params.IndexOfOutputParam("Spaces");
            if (index != -1)
                dataAccess.SetDataList(index, spaces?.ConvertAll(x => new GooSpace(x)));
        }
    }
}