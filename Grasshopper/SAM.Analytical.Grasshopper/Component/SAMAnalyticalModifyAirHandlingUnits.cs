using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalModifyAirHandlingUnits : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("31a55e4b-193f-4a3b-ab0c-b9d2e01b7b41");

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
        public SAMAnalyticalModifyAirHandlingUnits()
          : base("SAMAnalytical.ModifyAirHandlingUnits", "SAMAnalytical.ModifyAirHandlingUnits",
              "Modify AirHandlingUnits in AdjacencyCluster or AnalyticalModel",
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
                result.Add(new GH_SAMParam(new GooAnalyticalObjectParam() { Name = "_analytical", NickName = "_analytical", Description = "SAM Analytical AdjacencyCluster or AnalyticalModel", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "_airHandlingUnits_", NickName = "_airHandlingUnits_", Description = "Air Handling Units or Air Handling Units Names to be modified", Access = GH_ParamAccess.list, Optional = true }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "summerSupplyTemperature_", NickName = "summerSupplyTemperature_", Description = "Summer Supply Temperature [°C]", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Voluntary));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "winterSupplyTemperature_", NickName = "winterSupplyTemperature_", Description = "Winter Supply Temperature [°C]", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Voluntary));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "frostCoilOffTemperature_", NickName = "frostCoilOffTemperature_", Description = "Frost Coil Off Temperature [°C]", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Voluntary));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "winterHeatRecoveryLatentEfficiency_", NickName = "winterHeatRecoveryLatentEfficiency_", Description = "Winter Heat Recovery Latent Efficiency [%]", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Voluntary));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "summerHeatRecoverySensibleEfficiency_", NickName = "summerHeatRecoverySensibleEfficiency_", Description = "Summer Heat Recovery Sensible Efficiency [%]", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Voluntary));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "summerHeatRecoveryLatentEfficiency_", NickName = "summerHeatRecoveryLatentEfficiency_", Description = "Summer Heat Recovery Latent Efficiency [%]", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Voluntary));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "coolingCoilFluidFlowTemperature_", NickName = "coolingCoilFluidFlowTemperature_", Description = "Cooling Coil Fluid Flow Temperature [°C]", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Voluntary));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "coolingCoilFluidReturnTemperature_", NickName = "coolingCoilFluidReturnTemperature_", Description = "Cooling Coil Fluid Return Temperature [°C]", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Voluntary));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "coolingCoilContactFactor_", NickName = "coolingCoilContactFactor_", Description = "Cooling Coil Performance known as Contact Factor [0-1]\nContact Factor is the part of the total air through the coil which comes in to 'contact' with the surface of the cooling coil.\nUsually between 0.7-0.95", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Voluntary));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "heatingCoilFluidFlowTemperature_", NickName = "heatingCoilFluidFlowTemperature_", Description = "Heating Coil Fluid Flow Temperature [°C]", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Voluntary));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "heatingCoilFluidReturnTemperature_", NickName = "heatingCoilFluidReturnTemperature_", Description = "Heating Coil Fluid Return Temperature [°C]", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Voluntary));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "winterHeatingCoilSupplyTemperature_", NickName = "winterHeatingCoilSupplyTemperature_", Description = "Winter Heating Coil Supply Temperature [°C]", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Voluntary));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "winterHeatRecoveryDryBulbTemperature_", NickName = "winterHeatRecoveryDryBulbTemperature_", Description = "Winter Heat Recovery Dry Bulb Temperature [°C]", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Voluntary));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "winterHeatRecoveryRelativeHumidity_", NickName = "winterHeatRecoveryRelativeHumidity_", Description = "Winter Heat Recovery Relative Humidity [%]", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Voluntary));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "summerHeatRecoveryDryBulbTemperature_", NickName = "summerHeatRecoveryDryBulbTemperature_", Description = "Summer Heat Recovery Dry Bulb Temperature [°C]", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Voluntary));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "summerHeatRecoveryRelativeHumidity_", NickName = "summerHeatRecoveryRelativeHumidity_", Description = "Summer Heat Recovery Relative Humidity [°C]", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Voluntary));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Boolean() { Name = "summerHeatingCoil_", NickName = "summerHeatingCoil_", Description = "Summer Heating Coil [True/False])", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Voluntary));
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
                result.Add(new GH_SAMParam(new GooAnalyticalObjectParam() { Name = "analytical", NickName = "analytical", Description = "SAM Analytical Object", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
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
            
            IAnalyticalObject analyticalObject = null;
            index = Params.IndexOfInputParam("_analytical");
            if (index == -1 || !dataAccess.GetData(index, ref analyticalObject) || analyticalObject == null)
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
            else
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            if (adjacencyCluster != null)
            {
                List<GH_ObjectWrapper> objectWrappers = new List<GH_ObjectWrapper>();
                index = Params.IndexOfInputParam("_airHandlingUnits_");
                
                if (index != -1)
                {
                    dataAccess.GetDataList(index, objectWrappers);
                }

                List<AirHandlingUnit> airHandlingUnits = null;
                if (objectWrappers != null && objectWrappers.Count != 0)
                {
                    airHandlingUnits = new List<AirHandlingUnit>();
                    foreach (GH_ObjectWrapper objectWrapper in objectWrappers)
                    {
                        object @object = objectWrapper.Value;
                        if (@object is IGH_Goo)
                        {
                            @object = (@object as dynamic).Value;
                        }

                        AirHandlingUnit airHandlingUnit = null;
                        if (@object is string)
                        {
                            string name = (string)@object;
                            airHandlingUnit = adjacencyCluster?.GetObjects((AirHandlingUnit x) => x.Name == name)?.FirstOrDefault();
                        }
                        else if (@object is AirHandlingUnit)
                        {
                            airHandlingUnit = adjacencyCluster?.GetObject<AirHandlingUnit>(((AirHandlingUnit)@object).Guid);
                            if (airHandlingUnit == null)
                            {
                                airHandlingUnit = (AirHandlingUnit)@object;
                            }
                        }

                        if (airHandlingUnit != null)
                        {
                            airHandlingUnits.Add(airHandlingUnit);
                        }
                    }
                }

                if (airHandlingUnits == null)
                {
                    airHandlingUnits = adjacencyCluster.GetObjects<AirHandlingUnit>();
                }

                if(airHandlingUnits == null || airHandlingUnits.Count == 0)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                    return;
                }

                if (airHandlingUnits != null && airHandlingUnits.Count != 0)
                {
                    double summerSupplyTemperature = double.NaN;
                    double winterSupplyTemperature = double.NaN;
                    double frostCoilOffTemperature = double.NaN;
                    double winterHeatRecoveryLatentEfficiency = double.NaN;
                    double summerHeatRecoverySensibleEfficiency = double.NaN;
                    double summerHeatRecoveryLatentEfficiency = double.NaN;
                    double coolingCoilFluidFlowTemperature = double.NaN;
                    double coolingCoilFluidReturnTemperature = double.NaN;
                    double coolingCoilContactFactor = double.NaN;
                    double heatingCoilFluidFlowTemperature = double.NaN;
                    double heatingCoilFluidReturnTemperature = double.NaN;
                    double winterHeatingCoilSupplyTemperature = double.NaN;
                    double winterHeatRecoveryDryBulbTemperature = double.NaN;
                    double winterHeatRecoveryRelativeHumidity = double.NaN;
                    double summerHeatRecoveryDryBulbTemperature = double.NaN;
                    double summerHeatRecoveryRelativeHumidity = double.NaN;
                    bool? summerHeatingCoil = null;

                    double @double = double.NaN;
                    bool @bool = false;

                    index = Params.IndexOfInputParam("summerSupplyTemperature_");
                    if (index != -1 && dataAccess.GetData(index, ref @double))
                    {
                        summerSupplyTemperature = @double;
                    }

                    index = Params.IndexOfInputParam("winterSupplyTemperature_");
                    if (index != -1 && dataAccess.GetData(index, ref @double))
                    {
                        winterSupplyTemperature = @double;
                    }

                    index = Params.IndexOfInputParam("frostCoilOffTemperature_");
                    if (index != -1 && dataAccess.GetData(index, ref @double))
                    {
                        frostCoilOffTemperature = @double;
                    }

                    index = Params.IndexOfInputParam("winterHeatRecoveryLatentEfficiency_");
                    if (index != -1 && dataAccess.GetData(index, ref @double))
                    {
                        winterHeatRecoveryLatentEfficiency = @double;
                    }

                    index = Params.IndexOfInputParam("summerHeatRecoverySensibleEfficiency_");
                    if (index != -1 && dataAccess.GetData(index, ref @double))
                    {
                        summerHeatRecoverySensibleEfficiency = @double;
                    }
                    index = Params.IndexOfInputParam("summerHeatRecoveryLatentEfficiency_");
                    if (index != -1 && dataAccess.GetData(index, ref @double))
                    {
                        summerHeatRecoveryLatentEfficiency = @double;
                    }
                    index = Params.IndexOfInputParam("coolingCoilFluidFlowTemperature_");
                    if (index != -1 && dataAccess.GetData(index, ref @double))
                    {
                        coolingCoilFluidFlowTemperature = @double;
                    }

                    index = Params.IndexOfInputParam("coolingCoilFluidReturnTemperature_");
                    if (index != -1 && dataAccess.GetData(index, ref @double))
                    {
                        coolingCoilFluidReturnTemperature = @double;
                    }

                    index = Params.IndexOfInputParam("coolingCoilContactFactor_");
                    if (index != -1 && dataAccess.GetData(index, ref @double))
                    {
                        coolingCoilContactFactor = @double;
                    }

                    index = Params.IndexOfInputParam("heatingCoilFluidFlowTemperature_");
                    if (index != -1 && dataAccess.GetData(index, ref @double))
                    {
                        heatingCoilFluidFlowTemperature = @double;
                    }

                    index = Params.IndexOfInputParam("heatingCoilFluidReturnTemperature_");
                    if (index != -1 && dataAccess.GetData(index, ref @double))
                    {
                        heatingCoilFluidReturnTemperature = @double;
                    }

                    index = Params.IndexOfInputParam("winterHeatingCoilSupplyTemperature_");
                    if (index != -1 && dataAccess.GetData(index, ref @double))
                    {
                        winterHeatingCoilSupplyTemperature = @double;
                    }

                    index = Params.IndexOfInputParam("winterHeatRecoveryRelativeHumidity_");
                    if (index != -1 && dataAccess.GetData(index, ref @double))
                    {
                        winterHeatRecoveryRelativeHumidity = @double;
                    }

                    index = Params.IndexOfInputParam("winterHeatRecoveryDryBulbTemperature_");
                    if (index != -1 && dataAccess.GetData(index, ref @double))
                    {
                        winterHeatRecoveryDryBulbTemperature = @double;
                    }

                    index = Params.IndexOfInputParam("summerHeatRecoveryDryBulbTemperature_");
                    if (index != -1 && dataAccess.GetData(index, ref @double))
                    {
                        summerHeatRecoveryDryBulbTemperature = @double;
                    }

                    index = Params.IndexOfInputParam("summerHeatRecoveryRelativeHumidity_");
                    if (index != -1 && dataAccess.GetData(index, ref @double))
                    {
                        summerHeatRecoveryRelativeHumidity = @double;
                    }

                    index = Params.IndexOfInputParam("summerHeatingCoil_");
                    if (index != -1 && dataAccess.GetData(index, ref @bool))
                    {
                        summerHeatingCoil = @bool;
                    }

                    for (int i =0; i < airHandlingUnits.Count;i++)
                    {
                        if(airHandlingUnits[i] == null)
                        {
                            continue;
                        }

                        AirHandlingUnit airHandlingUnit = adjacencyCluster.GetObject<AirHandlingUnit>(airHandlingUnits[i].Guid);
                        if (airHandlingUnit == null)
                        {
                            continue;
                        }

                        airHandlingUnit = new AirHandlingUnit(airHandlingUnit);

                        airHandlingUnit.SummerSupplyTemperature = !double.IsNaN(summerSupplyTemperature) ? summerSupplyTemperature : airHandlingUnit.SummerSupplyTemperature;
                        airHandlingUnit.WinterSupplyTemperature = !double.IsNaN(winterSupplyTemperature) ? winterSupplyTemperature : airHandlingUnit.WinterSupplyTemperature;

                        HeatingCoil frostCoil = airHandlingUnit.GetSimpleEquipments<HeatingCoil>(FlowClassification.Intake)?.FirstOrDefault();
                        if(frostCoil != null)
                        {
                            frostCoil.WinterOffTemperature = !double.IsNaN(frostCoilOffTemperature) ? frostCoilOffTemperature : frostCoil.WinterOffTemperature;
                            frostCoil.SummerOffTemperature = !double.IsNaN(frostCoilOffTemperature) ? frostCoilOffTemperature : frostCoil.SummerOffTemperature;
                        }

                        HeatRecoveryUnit heatRecoveryUnit = airHandlingUnit.GetSimpleEquipments<HeatRecoveryUnit>(FlowClassification.Supply)?.FirstOrDefault();
                        if(heatRecoveryUnit != null)
                        {
                            heatRecoveryUnit.WinterLatentEfficiency = !double.IsNaN(winterHeatRecoveryLatentEfficiency) ? winterHeatRecoveryLatentEfficiency : heatRecoveryUnit.WinterLatentEfficiency;
                            heatRecoveryUnit.WinterDryBulbTemperature = !double.IsNaN(winterHeatRecoveryDryBulbTemperature) ? winterHeatRecoveryDryBulbTemperature : heatRecoveryUnit.WinterDryBulbTemperature;
                            heatRecoveryUnit.WinterRelativeHumidity = !double.IsNaN(winterHeatRecoveryRelativeHumidity) ? winterHeatRecoveryRelativeHumidity : heatRecoveryUnit.WinterRelativeHumidity;
                            
                            heatRecoveryUnit.SummerSensibleEfficiency = !double.IsNaN(summerHeatRecoverySensibleEfficiency) ? summerHeatRecoverySensibleEfficiency : heatRecoveryUnit.SummerSensibleEfficiency;
                            heatRecoveryUnit.SummerLatentEfficiency = !double.IsNaN(summerHeatRecoveryLatentEfficiency) ? summerHeatRecoveryLatentEfficiency : heatRecoveryUnit.SummerLatentEfficiency;
                            heatRecoveryUnit.SummerDryBulbTemperature = !double.IsNaN(summerHeatRecoveryDryBulbTemperature) ? summerHeatRecoveryDryBulbTemperature : heatRecoveryUnit.SummerDryBulbTemperature;
                            heatRecoveryUnit.SummerRelativeHumidity = !double.IsNaN(summerHeatRecoveryRelativeHumidity) ? summerHeatRecoveryRelativeHumidity : heatRecoveryUnit.SummerRelativeHumidity;
                        }

                        CoolingCoil coolingCoil = airHandlingUnit.GetSimpleEquipments<CoolingCoil>(FlowClassification.Supply)?.FirstOrDefault();
                        if(coolingCoil != null)
                        {
                            coolingCoil.FluidSupplyTemperature = !double.IsNaN(coolingCoilFluidFlowTemperature) ? coolingCoilFluidFlowTemperature : coolingCoil.FluidSupplyTemperature;
                            coolingCoil.FluidReturnTemperature = !double.IsNaN(coolingCoilFluidReturnTemperature) ? coolingCoilFluidReturnTemperature : coolingCoil.FluidReturnTemperature;
                            coolingCoil.ContactFactor = !double.IsNaN(coolingCoilContactFactor) ? coolingCoilContactFactor : coolingCoil.ContactFactor;
                        }

                        HeatingCoil heatingCoil = airHandlingUnit.GetSimpleEquipments<HeatingCoil>(FlowClassification.Supply)?.FirstOrDefault();
                        if(heatingCoil != null)
                        {
                            heatingCoil.FluidSupplyTemperature = !double.IsNaN(heatingCoilFluidFlowTemperature) ? heatingCoilFluidFlowTemperature : heatingCoil.FluidSupplyTemperature;
                            heatingCoil.FluidReturnTemperature = !double.IsNaN(heatingCoilFluidReturnTemperature) ? heatingCoilFluidReturnTemperature : heatingCoil.FluidReturnTemperature;
                            heatingCoil.WinterOffTemperature = !double.IsNaN(winterHeatingCoilSupplyTemperature) ? winterHeatingCoilSupplyTemperature : heatingCoil.WinterOffTemperature;
                            heatingCoil.Summer = summerHeatingCoil != null && summerHeatingCoil.HasValue ? summerHeatingCoil.Value : heatingCoil.Summer;
                        }

                        adjacencyCluster.AddObject(airHandlingUnit);
                    }
                }
            }

            if (analyticalObject is AnalyticalModel)
            {
                analyticalObject = new AnalyticalModel((AnalyticalModel)analyticalObject, adjacencyCluster);
            }
            else if (analyticalObject is AdjacencyCluster)
            {
                analyticalObject = adjacencyCluster;
            }

            index = Params.IndexOfOutputParam("analytical");
            if(index != -1)
                dataAccess.SetData(index, analyticalObject);
        }
    }
}