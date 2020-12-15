using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core;
using SAM.Core.Grasshopper;
using SAM.Geometry;
using SAM.Geometry.Grasshopper;
using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalReportSpaces : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("88152957-7920-48bb-a834-1f23708cae80");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.0";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        public override GH_Exposure Exposure => GH_Exposure.tertiary | GH_Exposure.obscure;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalReportSpaces()
          : base("SAMAnalytical.ReportSpaces", "SAMAnalytical.ReportSpaces",
              "Report Spaces",
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
                GH_SAMParam[] result = new GH_SAMParam[1];
                result[0] = new GH_SAMParam(new GooSAMObjectParam<SAMObject>() { Name = "_analytical", NickName = "_analytical", Description = "SAM Analytical Object", Access = GH_ParamAccess.item }, ParamVisibility.Binding);
                return result;
            }
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override GH_SAMParam[] Outputs
        {
            get
            {
                GH_SAMParam[] result = new GH_SAMParam[10];

                result[0] = new GH_SAMParam(new GooPanelParam() { Name = "Name", NickName = "Name", Description = "Space Name", Access = GH_ParamAccess.list }, ParamVisibility.Binding);
                result[1] = new GH_SAMParam(new Param_Guid() { Name = "Guid", NickName = "Guid", Description = "Space Guid", Access = GH_ParamAccess.list }, ParamVisibility.Voluntary);

                result[2] = new GH_SAMParam(new GooPanelParam() { Name = "InternalConditionName", NickName = "InternalConditionName", Description = "InternalCondition Name", Access = GH_ParamAccess.list }, ParamVisibility.Binding);
                result[3] = new GH_SAMParam(new Param_Guid() { Name = "InternalConditionGuid", NickName = "InternalConditionGuid", Description = "InternalCondition Guid", Access = GH_ParamAccess.list }, ParamVisibility.Voluntary);

                result[4] = new GH_SAMParam(new GooPanelParam() { Name = "VentilationSystemTypeName", NickName = "VentilationSystemTypeName", Description = "VentilationSystemType Name", Access = GH_ParamAccess.list }, ParamVisibility.Binding);
                result[5] = new GH_SAMParam(new Param_Guid() { Name = "VentilationSystemTypenGuid", NickName = "VentilationSystemTypenGuid", Description = "VentilationSystemTypen Guid", Access = GH_ParamAccess.list }, ParamVisibility.Voluntary);

                result[6] = new GH_SAMParam(new GooPanelParam() { Name = "HeatingSystemTypeName", NickName = "HeatingSystemTypeName", Description = "HeatingSystemType Name", Access = GH_ParamAccess.list }, ParamVisibility.Binding);
                result[7] = new GH_SAMParam(new Param_Guid() { Name = "HeatingSystemTypenGuid", NickName = "HeatingSystemTypenGuid", Description = "HeatingSystemTypen Guid", Access = GH_ParamAccess.list }, ParamVisibility.Voluntary);

                result[8] = new GH_SAMParam(new GooPanelParam() { Name = "CoolingSystemTypeName", NickName = "CoolingSystemTypeName", Description = "CoolingSystemType Name", Access = GH_ParamAccess.list }, ParamVisibility.Binding);
                result[9] = new GH_SAMParam(new Param_Guid() { Name = "CoolingSystemTypenGuid", NickName = "CoolingSystemTypenGuid", Description = "CoolingSystemTypen Guid", Access = GH_ParamAccess.list }, ParamVisibility.Voluntary);
                return result;
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
            SAMObject sAMObject = null;
            if (!dataAccess.GetData(0, ref sAMObject) || sAMObject == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            AnalyticalModel analyticalModel = sAMObject as AnalyticalModel;

            AdjacencyCluster adjacencyCluster = sAMObject as AdjacencyCluster;
            if (analyticalModel != null)
                adjacencyCluster = analyticalModel.AdjacencyCluster;
            else
                adjacencyCluster = sAMObject as AdjacencyCluster;

            List<Space> spaces = null;
            if (adjacencyCluster != null)
                spaces = ((AdjacencyCluster)sAMObject).GetSpaces();
            else if (sAMObject is Space)
                spaces = new List<Space>() { (Space)sAMObject };
                
            List<Guid> guids_Space = new List<Guid>();
            List<string> names_Space = new List<string>();

            List<string> names_InternalCondition= new List<string>();
            List<Guid> guids_InternalCondition = new List<Guid>();

            List<string> names_VentilationSystemType = new List<string>();
            List<Guid> guids_VentilationSystemType = new List<Guid>();

            List<string> names_HeatingSystemType = new List<string>();
            List<Guid> guids_HeatingSystemType = new List<Guid>();

            List<string> names_CoolingSystemType = new List<string>();
            List<Guid> guids_CoolingSystemType = new List<Guid>();

            foreach (Space space in spaces)
            {
                guids_Space.Add(space == null ? Guid.Empty : space.Guid);
                names_Space.Add(space?.Name);

                InternalCondition internalCondition = space.InternalCondition;

                names_InternalCondition.Add(internalCondition?.Name);
                guids_InternalCondition.Add(internalCondition == null ? Guid.Empty : internalCondition.Guid);

                names_VentilationSystemType.Add(internalCondition?.GetSystemTypeName<VentilationSystemType>());
                VentilationSystem ventilationSystem = adjacencyCluster?.GetRelatedObjects<VentilationSystem>(space)?.FirstOrDefault();
                VentilationSystemType ventilationSystemType = ventilationSystem?.SAMType as VentilationSystemType;
                guids_VentilationSystemType.Add(ventilationSystemType == null ? Guid.Empty : ventilationSystemType.Guid);
            }

            dataAccess.SetDataList(0, guids_Space);
            dataAccess.SetDataList(1, names_Space);

            dataAccess.SetDataList(2, guids_InternalCondition);
            dataAccess.SetDataList(3, names_InternalCondition);

            dataAccess.SetDataList(4, guids_VentilationSystemType);
            dataAccess.SetDataList(5, names_VentilationSystemType);
        }
    }
}