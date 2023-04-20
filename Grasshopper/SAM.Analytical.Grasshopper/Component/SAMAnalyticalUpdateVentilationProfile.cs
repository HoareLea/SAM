using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalUpdateVentilationProfile : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("d9720857-1cfb-4de2-96ff-958c9c116419");

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
                result.Add(new GH_SAMParam(new GooAnalyticalModelParam() { Name = "_analyticalModel", NickName = "_analyticalModel", Description = "SAM Analytical AnalyticalModel", Access = GH_ParamAccess.item }, ParamVisibility.Binding));

                GooSpaceParam gooSpaceParam = new GooSpaceParam() { Name = "_spaces_", NickName = "_spaces_", Description = "SAM Analytical Spaces. If not provided all Spaces from Analytical Model will be used and modified", Access = GH_ParamAccess.list, Optional = true};
                result.Add(new GH_SAMParam(gooSpaceParam, ParamVisibility.Binding));

                GooProfileParam gooProfileParam = new GooProfileParam() { Name = "profile_", NickName = "profile_", Description = "SAM Analytical Profile for ventilation.", Access = GH_ParamAccess.item, Optional = true};
                result.Add(new GH_SAMParam(gooProfileParam, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_Number number = null;

                number = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "supplyAirFlow_", NickName = "SupplyAirFlow_", Description = "Supply Air Flow [m3/s]", Access = GH_ParamAccess.item, Optional = true };
                result.Add(new GH_SAMParam(number, ParamVisibility.Binding));

                number = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "exhaustAirFlow_", NickName = "exhaustAirFlow_", Description = "Exhaust Air Flow [m3/s]", Access = GH_ParamAccess.item, Optional = true };
                result.Add(new GH_SAMParam(number, ParamVisibility.Binding));

                number = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "supplyAirFlowPerPerson_", NickName = "supplyAirFlowPerPerson_", Description = "Supply Air Flow Per Person [m3/s/p]", Access = GH_ParamAccess.item, Optional = true};
                result.Add(new GH_SAMParam(number, ParamVisibility.Voluntary));

                number = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "exhaustAirFlowPerPerson_", NickName = "exhaustAirFlowPerPerson_", Description = "Exhaust Air Flow Per Person [m3/s/p]", Access = GH_ParamAccess.item, Optional = true };
                result.Add(new GH_SAMParam(number, ParamVisibility.Voluntary));

                number = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "supplyAirFlowPerArea_", NickName = "supplyAirFlowPerArea_", Description = "Supply Air Flow Per Area [m3/s/m2]", Access = GH_ParamAccess.item, Optional = true };
                result.Add(new GH_SAMParam(number, ParamVisibility.Voluntary));

                number = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "exhaustAirFlowPerArea_", NickName = "exhaustAirFlowPerArea_", Description = "Exhaust Air Flow Per Area [m3/s/m2]", Access = GH_ParamAccess.item, Optional = true };
                result.Add(new GH_SAMParam(number, ParamVisibility.Voluntary));

                number = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "supplyAirChangesPerHour_", NickName = "supplyAirChangesPerHour_", Description = "Supply Air Changes Per Hour [ACH]", Access = GH_ParamAccess.item, Optional = true };
                result.Add(new GH_SAMParam(number, ParamVisibility.Voluntary));

                number = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "exhaustAirChangesPerHour_", NickName = "exhaustAirChangesPerHour_", Description = "Exhaust Air Changes Per Hour [ACH]", Access = GH_ParamAccess.item, Optional = true };
                result.Add(new GH_SAMParam(number, ParamVisibility.Voluntary));

                return result.ToArray();
            }
        }

        protected override GH_SAMParam[] Outputs
        {
            get
            {
                List<GH_SAMParam> result = new List<GH_SAMParam>();
                result.Add(new GH_SAMParam(new GooAnalyticalModelParam() {Name = "analyticalModel", NickName = "AnalyticalModel", Description = "SAM Analytical Model with ", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooSpaceParam() { Name = "spaces", NickName = "spaces", Description = "SAM Analytical Spaces", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooInternalConditionParam() { Name = "internalConditions", NickName = "InternalConditions", Description = "SAM Analytical InternalConditions", Access = GH_ParamAccess.list }, ParamVisibility.Voluntary));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "calculatedSupplyAirFlow", NickName = "calculatedSupplyAirFlow", Description = "CalculatedSupplyAirFlow [m3/s]", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "calculatedExhaustAirFlow", NickName = "calculatedExhaustAirFlow", Description = "CalculatedExhaustAirFlow [m3/s]", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "calculatedSupplyAirFlowPerPerson", NickName = "calculatedSupplyAirFlowPerPerson", Description = "CalculatedSupplyAirFlowPerPerson [m3/s/p]", Access = GH_ParamAccess.list }, ParamVisibility.Voluntary));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "calculatedExhaustAirFlowPerPerson", NickName = "calculatedExhaustAirFlowPerPerson", Description = "CalculatedExhaustAirFlowPerPerson [m3/s/p]", Access = GH_ParamAccess.list }, ParamVisibility.Voluntary));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "calculatedSupplyAirFlowPerArea", NickName = "calculatedSupplyAirFlowPerArea", Description = "CalculatedSupplyAirFlowPerArea [m3/s/m2]", Access = GH_ParamAccess.list }, ParamVisibility.Voluntary));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "calculatedExhaustAirFlowPerArea", NickName = "calculatedExhaustAirFlowPerArea", Description = "CalculatedExhaustAirFlowPerArea [m3/s/m2]", Access = GH_ParamAccess.list }, ParamVisibility.Voluntary));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "calculatedSupplyAirChangesPerHour", NickName = "calculatedSupplyAirChangesPerHour", Description = "CalculatedSupplyAirChangesPerHour [ACH]", Access = GH_ParamAccess.list }, ParamVisibility.Voluntary));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "calculatedExhaustAirChangesPerHour", NickName = "calculatedExhaustAirChangesPerHour", Description = "CalculatedExhaustAirChangesPerHour [ACH]", Access = GH_ParamAccess.list }, ParamVisibility.Voluntary));
                return result.ToArray();
            }
        }

        /// <summary>
        /// Updates PanelTypes for AdjacencyCluster
        /// </summary>
        public SAMAnalyticalUpdateVentilationProfile()
          : base("SAMAnalytical.UpdateVentilationProfile", "SAMAnalytical.UpdateVentilationProfile",
              "Updates InternalCondition(IC) Ventilation Properties for Spaces",
              "SAM WIP", "SAM_IC")
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

            analyticalModel = new AnalyticalModel(analyticalModel);

            AdjacencyCluster adjacencyCluster = analyticalModel.AdjacencyCluster;
            if(adjacencyCluster != null)
            {
                adjacencyCluster = new AdjacencyCluster(adjacencyCluster);
            }

            List<Space> spaces = null;
            index = Params.IndexOfInputParam("_spaces_");
            if(index != -1)
            {
                spaces = new List<Space>();
                dataAccess.GetDataList(index, spaces);
                if (spaces != null && spaces.Count == 0)
                    spaces = null;
            }

            if (spaces == null)
                spaces = analyticalModel.GetSpaces();

            Profile profile = null;
            index = Params.IndexOfInputParam("profile_");
            if (index != -1)
                dataAccess.GetData(index, ref profile);

            if(profile != null && profile.ProfileGroup != ProfileGroup.Ventilation)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid Profile");
                return;
            }

            double supplyAirFlow = double.NaN;
            index = Params.IndexOfInputParam("supplyAirFlow_");
            if (index != -1)
                dataAccess.GetData(index, ref supplyAirFlow);

            double exhaustAirFlow = double.NaN;
            index = Params.IndexOfInputParam("exhaustAirFlow_");
            if (index != -1)
                dataAccess.GetData(index, ref exhaustAirFlow);

            double supplyAirFlowPerPerson = double.NaN;
            index = Params.IndexOfInputParam("supplyAirFlowPerPerson_");
            if (index != -1)
                dataAccess.GetData(index, ref supplyAirFlowPerPerson);

            double exhaustAirFlowPerPerson = double.NaN;
            index = Params.IndexOfInputParam("exhaustAirFlowPerPerson_");
            if (index != -1)
                dataAccess.GetData(index, ref exhaustAirFlowPerPerson);

            double supplyAirFlowPerArea = double.NaN;
            index = Params.IndexOfInputParam("supplyAirFlowPerArea_");
            if (index != -1)
                dataAccess.GetData(index, ref supplyAirFlowPerArea);

            double exhaustAirFlowPerArea = double.NaN;
            index = Params.IndexOfInputParam("exhaustAirFlowPerArea_");
            if (index != -1)
                dataAccess.GetData(index, ref exhaustAirFlowPerArea);

            double supplyAirChangesPerHour = double.NaN;
            index = Params.IndexOfInputParam("supplyAirChangesPerHour_");
            if (index != -1)
                dataAccess.GetData(index, ref supplyAirChangesPerHour);

            double exhaustAirChangesPerHour = double.NaN;
            index = Params.IndexOfInputParam("exhaustAirChangesPerHour_");
            if (index != -1)
                dataAccess.GetData(index, ref exhaustAirChangesPerHour);

            ProfileLibrary profileLibrary = analyticalModel.ProfileLibrary;
            if(profile != null)
            {
                profileLibrary.Add(profile);
            }

            List<Space> spaces_Result = new List<Space>();
            List<InternalCondition> internalConditions_Result = new List<InternalCondition>();
            List<double> calculatedSupplyAirFlows = new List<double>();
            List<double> calculatedExhaustAirFlows = new List<double>();
            List<double> calculatedSupplyAirFlowPerPersons = new List<double>();
            List<double> calculatedExhaustAirFlowPerPersons = new List<double>();
            List<double> calculatedSupplyAirFlowPerAreas = new List<double>();
            List<double> calculatedExhaustAirFlowPerAreas = new List<double>();
            List<double> calculatedSupplyAirChangesPerHours = new List<double>();
            List<double> calculatedExhaustAirChangesPerHours = new List<double>();
            foreach (Space space in spaces)
            {
                Space space_Temp = adjacencyCluster.GetObject<Space>(space.Guid);
                if(space_Temp == null)
                {
                    continue;
                }

                space_Temp = new Space(space_Temp);

                InternalCondition internalCondition_Temp = space_Temp.InternalCondition;
                if(internalCondition_Temp == null)
                {
                    continue;
                }

                if(profile == null)
                {
                    internalCondition_Temp.RemoveValue(InternalConditionParameter.VentilationProfileName);
                }
                else
                {
                    internalCondition_Temp.SetProfileName(profile.ProfileType, profile.Name);
                }

                if(double.IsNaN(supplyAirFlow))
                {
                    internalCondition_Temp.RemoveValue(InternalConditionParameter.SupplyAirFlow);
                }
                else
                {
                    internalCondition_Temp.SetValue(InternalConditionParameter.SupplyAirFlow, supplyAirFlow);
                }

                if (double.IsNaN(exhaustAirFlow))
                {
                    internalCondition_Temp.RemoveValue(InternalConditionParameter.ExhaustAirFlow);
                }
                else
                {
                    internalCondition_Temp.SetValue(InternalConditionParameter.ExhaustAirFlow, exhaustAirFlow);
                }

                if (double.IsNaN(supplyAirFlowPerPerson))
                {
                    internalCondition_Temp.RemoveValue(InternalConditionParameter.SupplyAirFlowPerPerson);
                }
                else
                {
                    internalCondition_Temp.SetValue(InternalConditionParameter.SupplyAirFlowPerPerson, supplyAirFlowPerPerson);
                }

                if (double.IsNaN(exhaustAirFlowPerPerson))
                {
                    internalCondition_Temp.RemoveValue(InternalConditionParameter.ExhaustAirFlowPerPerson);
                }
                else
                {
                    internalCondition_Temp.SetValue(InternalConditionParameter.ExhaustAirFlowPerPerson, exhaustAirFlowPerPerson);
                }

                if (double.IsNaN(supplyAirFlowPerArea))
                {
                    internalCondition_Temp.RemoveValue(InternalConditionParameter.SupplyAirFlowPerArea);
                }
                else
                {
                    internalCondition_Temp.SetValue(InternalConditionParameter.SupplyAirFlowPerArea, supplyAirFlowPerArea);
                }

                if (double.IsNaN(exhaustAirFlowPerArea))
                {
                    internalCondition_Temp.RemoveValue(InternalConditionParameter.ExhaustAirFlowPerArea);
                }
                else
                {
                    internalCondition_Temp.SetValue(InternalConditionParameter.ExhaustAirFlowPerArea, exhaustAirFlowPerArea);
                }

                if (double.IsNaN(supplyAirChangesPerHour))
                {
                    internalCondition_Temp.RemoveValue(InternalConditionParameter.SupplyAirChangesPerHour);
                }
                else
                {
                    internalCondition_Temp.SetValue(InternalConditionParameter.SupplyAirChangesPerHour, supplyAirChangesPerHour);
                }

                if (double.IsNaN(exhaustAirChangesPerHour))
                {
                    internalCondition_Temp.RemoveValue(InternalConditionParameter.ExhaustAirChangesPerHour);
                }
                else
                {
                    internalCondition_Temp.SetValue(InternalConditionParameter.ExhaustAirChangesPerHour, exhaustAirChangesPerHour);
                }

                space_Temp.InternalCondition = internalCondition_Temp;

                adjacencyCluster.AddObject(space_Temp);

                spaces_Result.Add(space_Temp);
                internalConditions_Result.Add(internalCondition_Temp);

                double calculatedSupplyAirFlow = space_Temp.CalculatedSupplyAirFlow();
                double calculatedExhaustAirFlow = space_Temp.CalculatedExhaustAirFlow();
                
                double calculatedSupplyAirFlowPerPerson = double.NaN;
                double calculatedExhaustAirFlowPerPerson = double.NaN;
                
                double occupancy = space_Temp.CalculatedOccupancy();
                if(!double.IsNaN(occupancy) && occupancy > 0)
                {
                    calculatedSupplyAirFlowPerPerson = calculatedSupplyAirFlow / occupancy;
                    calculatedExhaustAirFlowPerPerson = calculatedExhaustAirFlow / occupancy;
                }

                double calculatedSupplyAirFlowPerArea = double.NaN;
                double calculatedExhaustAirFlowPerArea = double.NaN;
                if (space_Temp.TryGetValue(SpaceParameter.Area, out double area) && !double.IsNaN(area) && area > 0)
                {
                    calculatedSupplyAirFlowPerArea = calculatedSupplyAirFlow / area;
                    calculatedExhaustAirFlowPerArea = calculatedExhaustAirFlow / area;
                }

                double calculatedSupplyAirChangesPerHour = double.NaN;
                double calculatedExhaustAirChangesPerHour = double.NaN;
                if (space_Temp.TryGetValue(SpaceParameter.Volume, out double volume) && !double.IsNaN(volume) && volume > 0)
                {
                    calculatedSupplyAirChangesPerHour = calculatedSupplyAirFlow / volume * 3600;
                    calculatedExhaustAirChangesPerHour = calculatedExhaustAirFlow / volume * 3600;
                }

                calculatedSupplyAirFlows.Add(calculatedSupplyAirFlow);
                calculatedExhaustAirFlows.Add(calculatedExhaustAirFlow);

                calculatedSupplyAirFlowPerPersons.Add(calculatedSupplyAirFlowPerPerson);
                calculatedExhaustAirFlowPerPersons.Add(calculatedExhaustAirFlowPerPerson);

                calculatedSupplyAirFlowPerAreas.Add(calculatedSupplyAirFlowPerArea);
                calculatedExhaustAirFlowPerAreas.Add(calculatedExhaustAirFlowPerArea);

                calculatedSupplyAirChangesPerHours.Add(calculatedSupplyAirChangesPerHour);
                calculatedExhaustAirChangesPerHours.Add(calculatedExhaustAirChangesPerHour);
            }


            index = Params.IndexOfOutputParam("analyticalModel");
            if(index != -1)
            {
                analyticalModel = new AnalyticalModel(analyticalModel, adjacencyCluster, analyticalModel.MaterialLibrary, profileLibrary);
                dataAccess.SetData(index, new GooAnalyticalModel(analyticalModel));
            }

            index = Params.IndexOfOutputParam("InternalConditions");
            if (index != -1)
                dataAccess.SetDataList(index, internalConditions_Result?.ConvertAll(x => new GooInternalCondition(x)));

            index = Params.IndexOfOutputParam("spaces");
            if (index != -1)
                dataAccess.SetDataList(index, spaces_Result?.ConvertAll(x => new GooSpace(x)));

            index = Params.IndexOfOutputParam("calculatedSupplyAirFlow");
            if (index != -1)
                dataAccess.SetDataList(index, calculatedSupplyAirFlows);

            index = Params.IndexOfOutputParam("calculatedExhaustAirFlow");
            if (index != -1)
                dataAccess.SetDataList(index, calculatedExhaustAirFlows);

            index = Params.IndexOfOutputParam("calculatedSupplyAirFlowPerPerson");
            if (index != -1)
                dataAccess.SetDataList(index, calculatedSupplyAirFlowPerPersons);

            index = Params.IndexOfOutputParam("calculatedExhaustAirFlowPerPerson");
            if (index != -1)
                dataAccess.SetDataList(index, calculatedExhaustAirFlowPerPersons);

            index = Params.IndexOfOutputParam("calculatedSupplyAirFlowPerArea");
            if (index != -1)
                dataAccess.SetDataList(index, calculatedSupplyAirFlowPerAreas);

            index = Params.IndexOfOutputParam("calculatedExhaustAirFlowPerArea");
            if (index != -1)
                dataAccess.SetDataList(index, calculatedExhaustAirFlowPerAreas);

            index = Params.IndexOfOutputParam("calculatedSupplyAirChangesPerHour");
            if (index != -1)
                dataAccess.SetDataList(index, calculatedSupplyAirChangesPerHours);

            index = Params.IndexOfOutputParam("calculatedExhaustAirChangesPerHour");
            if (index != -1)
                dataAccess.SetDataList(index, calculatedExhaustAirChangesPerHours);
        }
    }
}