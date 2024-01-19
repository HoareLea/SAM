using Grasshopper.Kernel;
using Rhino.Commands;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalCreateIZAMBySpaces : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("a6df620f-c76f-4f9d-a8dd-8ae174779289");

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
                
                result.Add(new GH_SAMParam(new GooSpaceParam() { Name = "_spaces", NickName = "_spaces", Description = "SAM Analytical Spaces", Access = GH_ParamAccess.list }, ParamVisibility.Binding));

                result.Add(new GH_SAMParam(new GooAnalyticalObjectParam() { Name = "_source", NickName = "_source", Description = "SAM Analytical Source such as AirHandlingUnit or AirHandlingUnitAirMovement", Access = GH_ParamAccess.item }, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_Number number = null;
                number = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "flowRates_", NickName = "flowRates_", Description = "Flow Rates", Access = GH_ParamAccess.item };
                result.Add(new GH_SAMParam(number, ParamVisibility.Voluntary));

                global::Grasshopper.Kernel.Parameters.Param_Boolean boolean = null;
                boolean = new global::Grasshopper.Kernel.Parameters.Param_Boolean() { Name = "_override_", NickName = "_override_", Description = "Override existing", Access = GH_ParamAccess.item };
                boolean.SetPersistentData(true);
                result.Add(new GH_SAMParam(boolean, ParamVisibility.Voluntary));

                return result.ToArray();
            }
        }

        protected override GH_SAMParam[] Outputs
        {
            get
            {
                List<GH_SAMParam> result = new List<GH_SAMParam>();
                result.Add(new GH_SAMParam(new GooAnalyticalObjectParam() { Name = "analytical", NickName = "analytical", Description = "SAM Analytical Object such as AnalyticalModel or AdjacencyCluster", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooAirMovementObjectParam() { Name = "iZAMs", NickName = "iZAMs", Description = "SAM Air Movement Objects (IZAM)", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                return result.ToArray();
            }
        }

        /// <summary>
        /// Updates PanelTypes for AdjacencyCluster
        /// </summary>
        public SAMAnalyticalCreateIZAMBySpaces()
          : base("SAMAnalytical.CreateIZAMBySpaces", "SAMAnalytical.CreateIZAMBySpaces",
              "Calculates Air Movement Objects",
              "SAM", "Analytical")
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

            SAMObject sAMObject = null;
            if (!dataAccess.GetData(index, ref sAMObject) || sAMObject == null)
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

            index = Params.IndexOfInputParam("_source");
            IAnalyticalObject analyticalObject = null;
            if (!dataAccess.GetData(index, ref analyticalObject) || analyticalObject == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            index = Params.IndexOfInputParam("flowRates_");
            List<double> flowRates = new List<double>();
            if (index == -1 || !dataAccess.GetDataList(index, flowRates) || flowRates == null || flowRates.Count == 0)
            {
                flowRates = null;
            }

            bool @override = true;
            if (index == -1 || !dataAccess.GetData(index, ref @override))
            {
                @override = true;
            }

            List<IAirMovementObject> airMovementObjects = null;

            if (sAMObject is AnalyticalModel || sAMObject is AdjacencyCluster)
            {
                AdjacencyCluster adjacencyCluster = null;
                if(sAMObject is AnalyticalModel)
                    adjacencyCluster = ((AnalyticalModel)sAMObject).AdjacencyCluster;
                else if(sAMObject is AdjacencyCluster)
                    adjacencyCluster = new AdjacencyCluster((AdjacencyCluster)sAMObject);

                SAMObject from = analyticalObject as SAMObject;
                if(from is AirHandlingUnitAirMovement)
                {
                    from = adjacencyCluster.GetRelatedObjects<AirHandlingUnit>(from.Guid)?.FirstOrDefault();
                }

                if(!(from is Space || from is AirHandlingUnit))
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                    return;
                }


                if (adjacencyCluster != null)
                {
                    airMovementObjects = new List<IAirMovementObject>();

                    for (int i = 0; i < spaces.Count; i++)
                    {
                        if (spaces[i] == null)
                        {
                            continue;
                        }

                        Space space_Temp = adjacencyCluster.GetObject<Space>(spaces[i].Guid);
                        if (space_Temp == null)
                        {
                            continue;
                        }

                        double airFlow = double.NaN;
                        if(flowRates != null && flowRates.Count < i)
                        {
                            airFlow = flowRates.Count < i ? flowRates[i] : flowRates.Last();
                        }

                        if(double.IsNaN(airFlow))
                        {
                            airFlow = space_Temp.CalculatedSupplyAirFlow();
                        }

                        SpaceAirMovement spaceAirMovement = new SpaceAirMovement(space_Temp.Name, airFlow, new ObjectReference(from).ToString(), new ObjectReference(space_Temp).ToString());
                        adjacencyCluster.AddObject(spaceAirMovement);
                        airMovementObjects.Add(spaceAirMovement);

                        adjacencyCluster.AddRelation(spaceAirMovement, from);
                        adjacencyCluster.AddRelation(spaceAirMovement, space_Temp);

                        airMovementObjects.Add(spaceAirMovement);

                        if(from is AirHandlingUnit)
                        {
                            spaceAirMovement = new SpaceAirMovement(space_Temp.Name, airFlow, new ObjectReference(space_Temp).ToString(), null);
                            adjacencyCluster.AddObject(spaceAirMovement);

                            adjacencyCluster.AddRelation(spaceAirMovement, space_Temp);
                            airMovementObjects.Add(spaceAirMovement);
                        }

                    }

                    if (sAMObject is AnalyticalModel)
                        sAMObject = new AnalyticalModel((AnalyticalModel)sAMObject, adjacencyCluster);
                    else if (sAMObject is AdjacencyCluster)
                        sAMObject = adjacencyCluster;
                }
            }

            index = Params.IndexOfOutputParam("analytical");
            if (index != -1)
            {
                dataAccess.SetData(index, sAMObject);
            }

            index = Params.IndexOfOutputParam("iZAMs");
            if (index != -1)
            {
                dataAccess.SetData(index, airMovementObjects);
            }
        }
    }
}