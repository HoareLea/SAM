using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalSetAirflow : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("586fbc48-80aa-4a88-b263-cca4b86fc5d0");

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
        public SAMAnalyticalSetAirflow()
          : base("SAMAnalytical.SetAirflow", "SAMAnalytical.SetAirflow",
              "Sets Airflow for given spaces",
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
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "_analytical", NickName = "_analytical", Description = "SAM Analytical AdjacencyCluster or AnalyticalModel", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooSpaceParam() { Name = "_spaces_", NickName = "_spaces_", Description = "SAM Analytical Spaces", Access = GH_ParamAccess.list, Optional = true}, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_Boolean boolean;

                boolean = new global::Grasshopper.Kernel.Parameters.Param_Boolean() { Name = "_exhaustAirFlow_", NickName = "_exhaustAirFlow_", Description = "Apply to Exhaust Air Flow", Access = GH_ParamAccess.item };
                boolean.SetPersistentData(true);
                result.Add(new GH_SAMParam(boolean, ParamVisibility.Binding));

                boolean = new global::Grasshopper.Kernel.Parameters.Param_Boolean() { Name = "_outsideSupplyAirFlow_", NickName = "_outsideSupplyAirFlow_", Description = "Apply to Outside Supply Air Flow", Access = GH_ParamAccess.item };
                boolean.SetPersistentData(true);
                result.Add(new GH_SAMParam(boolean, ParamVisibility.Binding));

                boolean = new global::Grasshopper.Kernel.Parameters.Param_Boolean() { Name = "_supplyAirFlow_", NickName = "_supplyAirFlow_", Description = "Apply to Supply Air Flow", Access = GH_ParamAccess.item };
                boolean.SetPersistentData(true);
                result.Add(new GH_SAMParam(boolean, ParamVisibility.Binding));

                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_airflow_LpSpP", NickName = "_airflow_LpSpP", Description = "Airflow [l/s/p]", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_airflow_ACH", NickName = "_airflow_ACH", Description = "Airflow [ACH]", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_airflow_LpSpM2", NickName = "_airflow_LpSpM2", Description = "Airflow [l/s/m2]", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_airflow_LpS", NickName = "_airflow_LpS", Description = "Airflow [l/s]", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));
                


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
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "Analytical", NickName = "Analytical", Description = "Objects", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
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
            
            SAMObject sAMObject = null;
            index = Params.IndexOfInputParam("_analytical");
            if (index == -1 || !dataAccess.GetData(index, ref sAMObject) || sAMObject == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            AdjacencyCluster adjacencyCluster = null;
            if (sAMObject is AnalyticalModel)
                adjacencyCluster = ((AnalyticalModel)sAMObject).AdjacencyCluster;
            else if(sAMObject is AdjacencyCluster)
                adjacencyCluster = new AdjacencyCluster((AdjacencyCluster)sAMObject);

            if(adjacencyCluster == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            bool exhaustAirFlow = false;
            index = Params.IndexOfInputParam("_exhaustAirFlow_");
            if (index != -1)
                dataAccess.GetData(index, ref exhaustAirFlow);

            bool outsideSupplyAirFlow = false;
            index = Params.IndexOfInputParam("_outsideSupplyAirFlow_");
            if (index != -1)
                dataAccess.GetData(index, ref outsideSupplyAirFlow);

            bool supplyAirFlow = false;
            index = Params.IndexOfInputParam("_supplyAirFlow_");
            if (index != -1)
                dataAccess.GetData(index, ref supplyAirFlow);

            double airflow_LpSpP = double.NaN;
            index = Params.IndexOfInputParam("_airflow_LpSpP");
            if(index != -1)
                dataAccess.GetData(index, ref airflow_LpSpP);

            double airflow_ACH = double.NaN;
            index = Params.IndexOfInputParam("_airflow_ACH");
            if (index != -1)
                dataAccess.GetData(index, ref airflow_ACH);

            double airflow_LpSpM2 = double.NaN;
            index = Params.IndexOfInputParam("_airflow_LpSpM2");
            if (index != -1)
                dataAccess.GetData(index, ref airflow_LpSpM2);

            double airflow_LpS = double.NaN;
            index = Params.IndexOfInputParam("_airflow_LpS");
            if (index != -1)
                dataAccess.GetData(index, ref airflow_LpS);
            
            List <Space> spaces = new List<Space>();
            index = Params.IndexOfInputParam("_spaces");
            if (index != -1)
                dataAccess.GetDataList(index, spaces);

            List<Space> spaces_Temp = adjacencyCluster.GetSpaces();
            if(spaces != null && spaces.Count != 0)
                spaces_Temp = spaces_Temp.FindAll(x => x != null && spaces.Find(y => y != null && y.Guid == x.Guid) != null);

            if(supplyAirFlow || exhaustAirFlow || outsideSupplyAirFlow)
            {
                foreach (Space space in spaces_Temp)
                {
                    if (space == null)
                        continue;

                    double airflow = double.NaN;

                    if (!double.IsNaN(airflow_LpSpP))
                    {
                        double occupancy = space.CalculatedOccupancy();
                        if (double.IsNaN(occupancy))
                        {
                            AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, string.Format("Space {0} (Guid: {1}) missing occupancy", space.Name, space.Guid));
                        }
                        else
                        {
                            if (double.IsNaN(airflow))
                                airflow = 0;

                            airflow += airflow_LpSpP * occupancy / 1000;
                        }

                    }

                    if (!double.IsNaN(airflow_ACH))
                    {
                        double volume = space.CalculatedVolume(adjacencyCluster);
                        if (double.IsNaN(volume))
                        {
                            AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, string.Format("Could not get volume for Space {0} (Guid: {1})", space.Name, space.Guid));
                        }
                        else
                        {
                            if (double.IsNaN(airflow))
                                airflow = 0;

                            airflow += airflow_ACH * volume / 3600;
                        }
                    }

                    if (!double.IsNaN(airflow_LpSpM2))
                    {
                        double area = space.CalculatedArea(adjacencyCluster);
                        if (double.IsNaN(area))
                        {
                            AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, string.Format("Could not get area for Space {0} (Guid: {1})", space.Name, space.Guid));
                        }
                        else
                        {
                            if (double.IsNaN(airflow))
                                airflow = 0;

                            airflow += airflow_LpSpM2 * area / 1000;
                        }
                    }

                    if (!double.IsNaN(airflow_LpS))
                    {
                        if (double.IsNaN(airflow))
                            airflow = 0;

                        airflow += airflow_LpS / 1000;
                    }

                    if (double.IsNaN(airflow))
                        continue;

                    Space space_New = new Space(space);

                    if(supplyAirFlow)
                        space_New.SetValue(SpaceParameter.SupplyAirFlow, airflow);

                    if (exhaustAirFlow)
                        space_New.SetValue(SpaceParameter.ExhaustAirFlow, airflow);

                    if (outsideSupplyAirFlow)
                        space_New.SetValue(SpaceParameter.OutsideSupplyAirFlow, airflow);

                    adjacencyCluster.AddObject(space_New);
                }

                if (sAMObject is AnalyticalModel)
                    sAMObject = new AnalyticalModel((AnalyticalModel)sAMObject, adjacencyCluster);
                else if (sAMObject is AdjacencyCluster)
                    sAMObject = adjacencyCluster;
            }

            index = Params.IndexOfOutputParam("Analytical");
            if(index != -1)
                dataAccess.SetData(index, sAMObject);
        }
    }
}