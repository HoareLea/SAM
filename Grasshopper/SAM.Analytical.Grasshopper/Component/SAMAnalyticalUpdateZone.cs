using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalUpdateZone : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("99eb209d-d128-42da-9d34-f7dcd935cb8e");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.1";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        public override GH_Exposure Exposure => GH_Exposure.primary;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalUpdateZone()
          : base("SAMAnalytical.UpdateZone", "SAMAnalytical.UpdateZone",
              "Update or Create Zone (Group) in Analytical Object",
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
                result.Add(new GH_SAMParam(new GooSpaceParam { Name = "_spaces", NickName = "_spaces", Description = "SAM Analytical Spaces", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_String { Name = "_name", NickName = "_name", Description = "Zone Name", Access = GH_ParamAccess.item }, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_String param_String = null;

                param_String = new global::Grasshopper.Kernel.Parameters.Param_String { Name = "_zoneType", NickName = "_zoneType", Description = "SAM Analytical ZoneType", Access = GH_ParamAccess.item };
                result.Add(new GH_SAMParam(param_String, ParamVisibility.Binding));

                param_String = new global::Grasshopper.Kernel.Parameters.Param_String { Name = "zoneCategoryName_", NickName = "zoneCategoryName_", Description = "Zone Category Name. ZoneType parameter will be ignored when zoneCategory name applyed", Access = GH_ParamAccess.item, Optional = true };
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
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "Analytical", NickName = "Analytical", Description = "SAM Analytical Object such as AdjacencyCluster or AnalyticalModel", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "Zone", NickName = "Zone", Description = "SAM GuidCollection representing Zone", Access = GH_ParamAccess.item }, ParamVisibility.Voluntary));
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

            index = Params.IndexOfInputParam("_spaces");
            List<Space> spaces = new List<Space>();
            if (index == -1 || !dataAccess.GetDataList(index, spaces) || spaces == null || spaces.Count == 0)
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

            index = Params.IndexOfInputParam("zoneCategoryName_");
            zoneCategory = null;
            if (index == -1 || !dataAccess.GetData(index, ref zoneCategory))
            {
                zoneCategory = null;
            }

            Zone zone = null;
            if(sAMObject is AnalyticalModel)
            {
                AnalyticalModel analyticalModel = new AnalyticalModel((AnalyticalModel)sAMObject);
                AdjacencyCluster adjacencyCluster = analyticalModel.AdjacencyCluster;
                if(adjacencyCluster != null)
                {
                    adjacencyCluster = new AdjacencyCluster(adjacencyCluster);
                    if(zoneCategory != null)
                    {
                        zone = Analytical.Modify.UpdateZone(adjacencyCluster, name, zoneCategory, spaces.ToArray());
                    }
                    else
                    {
                        zone = Analytical.Modify.UpdateZone(adjacencyCluster, name, zoneType, spaces.ToArray());
                    }
                    
                    sAMObject = new AnalyticalModel(analyticalModel, adjacencyCluster);
                }
            }
            else if(sAMObject is AdjacencyCluster)
            {
                AdjacencyCluster adjacencyCluster = new AdjacencyCluster((AdjacencyCluster)sAMObject);
                if (zoneCategory != null)
                {
                    zone = Analytical.Modify.UpdateZone(adjacencyCluster, name, zoneCategory, spaces.ToArray());
                }
                else
                {
                    zone = Analytical.Modify.UpdateZone(adjacencyCluster, name, zoneType, spaces.ToArray());
                }
                sAMObject = adjacencyCluster;
            }

            index = Params.IndexOfOutputParam("Analytical");
            if (index != -1)
                dataAccess.SetData(index, sAMObject);

            index = Params.IndexOfOutputParam("Zone");
            if (index != -1)
                dataAccess.SetData(index, zone);
        }
    }
}