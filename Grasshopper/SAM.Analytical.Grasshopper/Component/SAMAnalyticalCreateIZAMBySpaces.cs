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
        public override string LatestComponentVersion => "1.0.1";

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
                result.Add(new GH_SAMParam(new GooAnalyticalModelParam() { Name = "_analyticalModel", NickName = "_analyticalModel", Description = "SAM Analytical Model", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                
                result.Add(new GH_SAMParam(new GooSpaceParam() { Name = "_spaces", NickName = "_spaces", Description = "SAM Analytical Spaces", Access = GH_ParamAccess.list }, ParamVisibility.Binding));

                result.Add(new GH_SAMParam(new GooAnalyticalObjectParam() { Name = "_source", NickName = "_source", Description = "SAM Analytical Source such as AirHandlingUnit or iZAM AirHandlingUnitAirMovement", Access = GH_ParamAccess.item }, ParamVisibility.Binding));

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
                result.Add(new GH_SAMParam(new GooAnalyticalModelParam() { Name = "analyticalModel", NickName = "analyticalModel", Description = "SAM AnalyticalModel", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
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

            index = Params.IndexOfInputParam("_analyticalModel");
            if(index == -1)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            AnalyticalModel analyticalModel = null;
            if (!dataAccess.GetData(index, ref analyticalModel) || analyticalModel == null)
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

            bool @override = true;
            if (index == -1 || !dataAccess.GetData(index, ref @override))
            {
                @override = true;
            }

            List<IAirMovementObject> airMovementObjects = null;

            AdjacencyCluster adjacencyCluster = analyticalModel.AdjacencyCluster;

            ProfileLibrary profileLibrary = analyticalModel.ProfileLibrary;

            SAMObject from = analyticalObject as SAMObject;
            if (from is AirHandlingUnitAirMovement)
            {
                from = adjacencyCluster.GetRelatedObjects<AirHandlingUnit>(from.Guid)?.FirstOrDefault();
            }

            if (!(from is Space || from is AirHandlingUnit))
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

                    double airFlow = space_Temp.CalculatedSupplyAirFlow();

                    Profile profile = space_Temp.InternalCondition?.GetProfile(ProfileType.Ventilation, profileLibrary);

                    SpaceAirMovement spaceAirMovement = profile == null ? new SpaceAirMovement(space_Temp.Name, airFlow, new ObjectReference(from).ToString(), new ObjectReference(space_Temp).ToString()) : new SpaceAirMovement(space_Temp.Name, airFlow, profile, new ObjectReference(from).ToString(), new ObjectReference(space_Temp).ToString());
                    adjacencyCluster.AddObject(spaceAirMovement);
                    airMovementObjects.Add(spaceAirMovement);

                    adjacencyCluster.AddRelation(spaceAirMovement, from);
                    adjacencyCluster.AddRelation(spaceAirMovement, space_Temp);

                    if (from is AirHandlingUnit)
                    {
                        spaceAirMovement = profile == null ? new SpaceAirMovement(space_Temp.Name, airFlow, new ObjectReference(space_Temp).ToString(), null) : new SpaceAirMovement(space_Temp.Name, airFlow, profile, new ObjectReference(space_Temp).ToString(), null);
                        adjacencyCluster.AddObject(spaceAirMovement);

                        adjacencyCluster.AddRelation(spaceAirMovement, space_Temp);
                        airMovementObjects.Add(spaceAirMovement);
                    }

                }

                analyticalModel = new AnalyticalModel(analyticalModel, adjacencyCluster);
            }

            index = Params.IndexOfOutputParam("analyticalModel");
            if (index != -1)
            {
                dataAccess.SetData(index, analyticalModel);
            }

            index = Params.IndexOfOutputParam("iZAMs");
            if (index != -1)
            {
                dataAccess.SetDataList(index, airMovementObjects);
            }
        }
    }
}