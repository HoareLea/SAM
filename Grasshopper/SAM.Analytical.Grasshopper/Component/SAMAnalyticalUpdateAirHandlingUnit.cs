using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalUpdateAirHandlingUnit : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("0c2abbd1-55d7-4ca0-b737-4ab1ade088d1");

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

                ProfileLibrary profileLibrary = ActiveSetting.Setting.GetValue<ProfileLibrary>(AnalyticalSettingParameter.DefaultProfileLibrary);
                DegreeOfActivityLibrary degreeOfActivityLibrary = ActiveSetting.Setting.GetValue<DegreeOfActivityLibrary>(AnalyticalSettingParameter.DefaultDegreeOfActivityLibrary);

                List<GH_SAMParam> result = new List<GH_SAMParam>();

                global::Grasshopper.Kernel.Parameters.Param_GenericObject genericObject = new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "_airHandlingUnits", NickName = "_airHandlingUnits", Description = "SAM Analytical Air Handling Units", Access = GH_ParamAccess.list };
                result.Add(new GH_SAMParam(genericObject, ParamVisibility.Binding));

                result.Add(new GH_SAMParam(new GooAnalyticalObjectParam() { Name = "_analytical_", NickName = "_analytical_", Description = "SAM Analytical AnalyticalModel, AdjacencyCluster", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_Number number;

                number = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_coolingCoilFluidReturnTemperature_", NickName = "_coolingCoilFluidReturnTemperature_", Description = "Cooling Coil Fluid Return Temperature [C]", Access = GH_ParamAccess.item, Optional = true };
                result.Add(new GH_SAMParam(number, ParamVisibility.Binding));

                number = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_coolingCoilFluidFlowTemperature_", NickName = "_coolingCoilFluidFlowTemperature_", Description = "Cooling Coil Fluid Flow Temperature [C]", Access = GH_ParamAccess.item, Optional = true };
                result.Add(new GH_SAMParam(number, ParamVisibility.Binding));

                number = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_summerSupplyTemperature_", NickName = "_summerSupplyTemperature_", Description = "Summer Supply Temperature [C]", Access = GH_ParamAccess.item, Optional = true };
                result.Add(new GH_SAMParam(number, ParamVisibility.Binding));

                number = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_summerHeatRecoverySensibleEfficiency_", NickName = "_summerHeatRecoverySensibleEfficiency_", Description = "Summer Heat Recovery Sensible Efficiency [0 - 100]", Access = GH_ParamAccess.item, Optional = true };
                result.Add(new GH_SAMParam(number, ParamVisibility.Binding));

                number = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_summerHeatRecoveryLatentEfficiency_", NickName = "_summerHeatRecoveryLatentEfficiency_", Description = "Summer Heat Recovery Latent Efficiency [0 - 100]", Access = GH_ParamAccess.item, Optional = true };
                result.Add(new GH_SAMParam(number, ParamVisibility.Binding));

                number = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_winterHeatRecoverySensibleEfficiency_", NickName = "_winterHeatRecoverySensibleEfficiency_", Description = "Winter Heat Recovery Sensible Efficiency [0 - 100]", Access = GH_ParamAccess.item, Optional = true };
                result.Add(new GH_SAMParam(number, ParamVisibility.Binding));

                number = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_winterHeatRecoveryLatentEfficiency_", NickName = "_winterHeatRecoveryLatentEfficiency_", Description = "Winter Heat Recovery Latent Efficiency [0 - 100]", Access = GH_ParamAccess.item, Optional = true };
                result.Add(new GH_SAMParam(number, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_Boolean boolean;

                boolean = new global::Grasshopper.Kernel.Parameters.Param_Boolean() { Name = "_summerHeatingCoil_", NickName = "_summerHeatingCoil_", Description = "Summer Heating Coil [True/False]", Access = GH_ParamAccess.item, Optional = true };
                result.Add(new GH_SAMParam(boolean, ParamVisibility.Binding));

                return result.ToArray();
            }
        }

        protected override GH_SAMParam[] Outputs
        {
            get
            {
                List<GH_SAMParam> result = new List<GH_SAMParam>();
                result.Add(new GH_SAMParam(new GooAnalyticalObjectParam() { Name = "airHandlingUnits", NickName = "airHandlingUnits", Description = "SAM Analytical AirHandlingUnits", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooAnalyticalObjectParam() { Name = "analytical", NickName = "analytical", Description = "SAM Analytical Object ", Access = GH_ParamAccess.item }, ParamVisibility.Binding));

                return result.ToArray();
            }
        }

        /// <summary>
        /// Updates PanelTypes for AdjacencyCluster
        /// </summary>
        public SAMAnalyticalUpdateAirHandlingUnit()
          : base("SAMAnalytical.UpdateAirHandlingUnit", "SAMAnalytical.UpdateAirHandlingUnit",
              "Updates AirHandlingUnit (AHU) Properties.",
              "SAM WIP", "SAM")
        {
        }

        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            int index;

            index = Params.IndexOfInputParam("_analytical");
            if (index == -1)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            IAnalyticalObject analyticalObject = null;
            if (!dataAccess.GetData(index, ref analyticalObject) || analyticalObject == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            AdjacencyCluster adjacencyCluster = null;

            if (analyticalObject is AnalyticalModel)
            {
                adjacencyCluster = ((AnalyticalModel)analyticalObject).AdjacencyCluster;
            }
            else if (analyticalObject is AdjacencyCluster)
            {
                adjacencyCluster = new AdjacencyCluster((AdjacencyCluster)analyticalObject);
            }

            index = Params.IndexOfInputParam("_airHandlingUnits");
            List<GH_ObjectWrapper> objectWrappers = new List<GH_ObjectWrapper>();
            if (index == -1 || !dataAccess.GetDataList(index, objectWrappers) || objectWrappers == null || objectWrappers.Count == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<AirHandlingUnit> airHandlingUnits = new List<AirHandlingUnit>();
            foreach (GH_ObjectWrapper objectWrapper in objectWrappers)
            {
                object @object = objectWrapper.Value;
                if (@object is IGH_Goo)
                {
                    @object = (@object as dynamic).Value;
                }

                AirHandlingUnit airHandlingUnit = null;

                if (@object is string && adjacencyCluster != null)
                {
                    airHandlingUnit = adjacencyCluster.GetObjects<AirHandlingUnit>()?.Find(x => x.Name.Equals((string)@object));
                }
                else if (@object is AirHandlingUnit)
                {
                    if (adjacencyCluster == null)
                    {
                        airHandlingUnit = new AirHandlingUnit((AirHandlingUnit)@object);

                    }
                    else
                    {
                        airHandlingUnit = adjacencyCluster.GetObject<AirHandlingUnit>(((AirHandlingUnit)@object).Guid);
                        if (airHandlingUnit != null)
                        {
                            airHandlingUnit = new AirHandlingUnit(airHandlingUnit);
                        }
                        else
                        {
                            airHandlingUnit = new AirHandlingUnit((AirHandlingUnit)@object);
                        }
                    }
                }

                if (airHandlingUnit != null)
                {
                    airHandlingUnits.Add(airHandlingUnit);
                }
            }

            if (airHandlingUnits == null || airHandlingUnits.Count == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            double coolingCoilFluidReturnTemperature = double.NaN;
            index = Params.IndexOfInputParam("_coolingCoilFluidReturnTemperature_");
            if (index != -1)
            {
                if (!dataAccess.GetData(index, ref coolingCoilFluidReturnTemperature))
                {
                    coolingCoilFluidReturnTemperature = double.NaN;
                }
            }

            double coolingCoilFluidFlowTemperature = double.NaN;
            index = Params.IndexOfInputParam("_coolingCoilFluidFlowTemperature_");
            if (index != -1)
            {
                if (!dataAccess.GetData(index, ref coolingCoilFluidFlowTemperature))
                {
                    coolingCoilFluidFlowTemperature = double.NaN;
                }
            }

            double summerSupplyTemperature = double.NaN;
            index = Params.IndexOfInputParam("_summerSupplyTemperature_");
            if (index != -1)
            {
                if (!dataAccess.GetData(index, ref summerSupplyTemperature))
                {
                    summerSupplyTemperature = double.NaN;
                }
            }

            double summerHeatRecoverySensibleEfficiency = double.NaN;
            index = Params.IndexOfInputParam("_summerHeatRecoverySensibleEfficiency_");
            if (index != -1)
            {
                if (!dataAccess.GetData(index, ref summerHeatRecoverySensibleEfficiency))
                {
                    summerHeatRecoverySensibleEfficiency = double.NaN;
                }
            }

            double summerHeatRecoveryLatentEfficiency = double.NaN;
            index = Params.IndexOfInputParam("_summerHeatRecoveryLatentEfficiency_");
            if (index != -1)
            {
                if (!dataAccess.GetData(index, ref summerHeatRecoveryLatentEfficiency))
                {
                    summerHeatRecoveryLatentEfficiency = double.NaN;
                }
            }

            double winterHeatRecoverySensibleEfficiency = double.NaN;
            index = Params.IndexOfInputParam("_winterHeatRecoverySensibleEfficiency_");
            if (index != -1)
            {
                if (!dataAccess.GetData(index, ref winterHeatRecoverySensibleEfficiency))
                {
                    winterHeatRecoverySensibleEfficiency = double.NaN;
                }
            }

            double winterHeatRecoveryLatentEfficiency = double.NaN;
            index = Params.IndexOfInputParam("_winterHeatRecoveryLatentEfficiency_");
            if (index != -1)
            {
                if (!dataAccess.GetData(index, ref winterHeatRecoveryLatentEfficiency))
                {
                    winterHeatRecoveryLatentEfficiency = double.NaN;
                }
            }

            bool? summerHeatingCoil = null;
            index = Params.IndexOfInputParam("_summerHeatingCoil_");
            if (index != -1)
            {
                bool summerHeatingCoil_Temp = false;
                if (dataAccess.GetData(index, ref summerHeatingCoil_Temp))
                {
                    summerHeatingCoil = summerHeatingCoil_Temp;
                }
            }

            foreach (AirHandlingUnit airHandlingUnit_Temp in airHandlingUnits)
            {
                if (!double.IsNaN(coolingCoilFluidReturnTemperature))
                {
                    airHandlingUnit_Temp.SetValue(AirHandlingUnitParameter.CoolingCoilFluidReturnTemperature, coolingCoilFluidReturnTemperature);
                }

                if (!double.IsNaN(coolingCoilFluidFlowTemperature))
                {
                    airHandlingUnit_Temp.SetValue(AirHandlingUnitParameter.CoolingCoilFluidFlowTemperature, coolingCoilFluidFlowTemperature);
                }

                if (!double.IsNaN(summerSupplyTemperature))
                {
                    airHandlingUnit_Temp.SetValue(AirHandlingUnitParameter.SummerSupplyTemperature, summerSupplyTemperature);
                }

                if (!double.IsNaN(summerHeatRecoverySensibleEfficiency))
                {
                    airHandlingUnit_Temp.SetValue(AirHandlingUnitParameter.SummerHeatRecoverySensibleEfficiency, summerHeatRecoverySensibleEfficiency);
                }

                if (!double.IsNaN(summerHeatRecoveryLatentEfficiency))
                {
                    airHandlingUnit_Temp.SetValue(AirHandlingUnitParameter.SummerHeatRecoveryLatentEfficiency, summerHeatRecoveryLatentEfficiency);
                }

                if (!double.IsNaN(winterHeatRecoverySensibleEfficiency))
                {
                    airHandlingUnit_Temp.SetValue(AirHandlingUnitParameter.WinterHeatRecoverySensibleEfficiency, winterHeatRecoverySensibleEfficiency);
                }

                if (!double.IsNaN(winterHeatRecoveryLatentEfficiency))
                {
                    airHandlingUnit_Temp.SetValue(AirHandlingUnitParameter.WinterHeatRecoveryLatentEfficiency, winterHeatRecoveryLatentEfficiency);
                }

                if(summerHeatingCoil != null && summerHeatingCoil.HasValue)
                {
                    airHandlingUnit_Temp.SetValue(AirHandlingUnitParameter.SummerHeatingCoil, summerHeatingCoil.Value);
                }

                if (adjacencyCluster != null)
                {
                    adjacencyCluster.AddObject(airHandlingUnit_Temp);
                }
            }


            index = Params.IndexOfOutputParam("airHandlingUnits");
            if (index != -1)
            {
                dataAccess.SetDataList(index, airHandlingUnits?.ConvertAll(x => new GooAnalyticalObject(x)));
            }

            index = Params.IndexOfOutputParam("analytical");
            if (index != -1)
            {
                if (analyticalObject is AdjacencyCluster)
                {
                    analyticalObject = adjacencyCluster;
                }
                else if (analyticalObject is AnalyticalModel)
                {
                    analyticalObject = new AnalyticalModel((AnalyticalModel)analyticalObject, adjacencyCluster);
                }

                dataAccess.SetData(index, analyticalObject);
            }
        }
    }
}