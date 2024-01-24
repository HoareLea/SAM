using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalCreateIZAMBySetPoint : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("e0146fb5-ee3c-4bf8-a05d-d7f369486b66");

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
                result.Add(new GH_SAMParam(new GooAnalyticalObjectParam() { Name = "_analytical", NickName = "_analytical", Description = "SAM Analytical Object such as AnalyticalModel or AdjacencyCluster", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "_name", NickName = "_name", Description = "SAM AHU Name", Access = GH_ParamAccess.item }, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_Number number = null;

                number = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_setPoints", NickName = "_setPoints", Description = "Set Points", Access = GH_ParamAccess.list };
                result.Add(new GH_SAMParam(number, ParamVisibility.Binding));

                number = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_tempAccuracies_", NickName = "_tempAccuracies_", Description = "Temperature Accuracy", Access = GH_ParamAccess.list };
                number.SetPersistentData(new List<double> { 1.5 });
                result.Add(new GH_SAMParam(number, ParamVisibility.Binding));

                number = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_humidities", NickName = "_humidity", Description = "Humidity", Access = GH_ParamAccess.list };
                result.Add(new GH_SAMParam(number, ParamVisibility.Voluntary));

                number = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_humidityAccuracies_", NickName = "_humidityAccuracy_", Description = "HumidityAccuracy", Access = GH_ParamAccess.list };
                number.SetPersistentData(new List<double> { 10 });
                result.Add(new GH_SAMParam(number, ParamVisibility.Voluntary));
                return result.ToArray();
            }
        }

        protected override GH_SAMParam[] Outputs
        {
            get
            {
                List<GH_SAMParam> result = new List<GH_SAMParam>();
                result.Add(new GH_SAMParam(new GooAnalyticalObjectParam() { Name = "analytical", NickName = "analytical", Description = "SAM Analytical Object such as AnalyticalModel or AdjacencyCluster", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooAirMovementObjectParam() { Name = "iZAM", NickName = "iZAM", Description = "SAM Air Movement Objects (IZAM)", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                return result.ToArray();
            }
        }

        /// <summary>
        /// Updates PanelTypes for AdjacencyCluster
        /// </summary>
        public SAMAnalyticalCreateIZAMBySetPoint()
          : base("SAMAnalytical.CreateIZAMBySetPoint", "SAMAnalytical.CreateIZAMBySetPoint",
              "Create IZAM AHU object to calculate Inter Zone Air Movement" +
                "\n Inter Zone Air Movements also know as IZAM’s are used to model air flow in and out of a zone. " +
                "IZAM’s can be set up to create an air flow that can come from outside or from another zone, " +
                "it can also be set up to create an air flow from the zone to outside." + 
                "\nhttps://docs.edsl.net/tbd/CoreConcepts/IZAMs.html",
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

            string name = null;
            index = Params.IndexOfInputParam("_name");
            if (index == -1 || !dataAccess.GetData(index, ref name) || string.IsNullOrEmpty(name))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<double> temperatures = new List<double>();
            index = Params.IndexOfInputParam("_setPoints");
            if (index == -1 || !dataAccess.GetDataList(index, temperatures) || temperatures == null || temperatures.Count == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<double> temperatureRanges = new List<double>();
            index = Params.IndexOfInputParam("_tempAccuracies_");
            if (index == -1 || !dataAccess.GetDataList(index, temperatureRanges) || temperatureRanges.Count == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<double> humidities = null;
            List<double> humidityRanges = null;

            index = Params.IndexOfInputParam("_humidities");
            if (index != -1)
            {
                humidities = new List<double>();
                if (!dataAccess.GetDataList(index, humidities))
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                    return;
                }

                index = Params.IndexOfInputParam("_humidityAccuracies_");
                if (index != -1)
                {
                    humidityRanges = new List<double>();
                    if (!dataAccess.GetDataList(index, humidityRanges))
                    {
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                        return;
                    }
                }
            }

            AirHandlingUnitAirMovement airHandlingUnitAirMovement = null;

            if (sAMObject is AnalyticalModel || sAMObject is AdjacencyCluster)
            {
                AdjacencyCluster adjacencyCluster = null;
                if(sAMObject is AnalyticalModel)
                    adjacencyCluster = ((AnalyticalModel)sAMObject).AdjacencyCluster;
                else if(sAMObject is AdjacencyCluster)
                    adjacencyCluster = new AdjacencyCluster((AdjacencyCluster)sAMObject);

                if(adjacencyCluster != null)
                {


                    AirHandlingUnit airHandlingUnit = adjacencyCluster.GetObjects<AirHandlingUnit>()?.Find(x => x.Name == name);
                    if(airHandlingUnit == null)
                    {
                        airHandlingUnit = new AirHandlingUnit(name, temperatures.Average() - (temperatureRanges.Average() / 2), temperatures.Average() + (temperatureRanges.Average() / 2));
                        adjacencyCluster.AddObject(airHandlingUnit);
                    }

                    List<double> hetaingTemperatures = new List<double>();
                    List<double> coolingTemperatures = new List<double>();
                    for (int i = 0; i < temperatures.Count; i++)
                    {
                        double temperature = temperatures[i];

                        double temperatureRange = temperatureRanges.Count < i ? temperatureRanges[i] : temperatureRanges.Last();

                        hetaingTemperatures.Add(temperature - (temperatureRange / 2));
                        coolingTemperatures.Add(temperature + (temperatureRange / 2));
                    }


                    Profile heating = new Profile(string.Format("{0} {1}", airHandlingUnit.Name, ProfileType.Heating), ProfileType.Heating, hetaingTemperatures);
                    Profile cooling = new Profile(string.Format("{0} {1}", airHandlingUnit.Name, ProfileType.Cooling), ProfileType.Cooling, coolingTemperatures);

                    Profile humidification = null;
                    Profile dehumidification = null;
                    if (humidityRanges != null && humidityRanges.Count != 0 && humidities != null && humidities.Count != 0)
                    {
                        List<double> humidifications = new List<double>();
                        List<double> dehumidifications = new List<double>();
                        for (int i = 0; i < humidities.Count; i++)
                        {
                            double humidity = humidities[i];

                            double humidityRange = humidityRanges.Count < i ? humidityRanges[i] : humidityRanges.Last();

                            humidifications.Add(humidity - (humidityRange / 2));
                            dehumidifications.Add(humidity + (humidityRange / 2));
                        }


                        humidification = new Profile(string.Format("{0} {1}", airHandlingUnit.Name, ProfileType.Humidification), ProfileType.Humidification, humidifications);
                        dehumidification = new Profile(string.Format("{0} {1}", airHandlingUnit.Name, ProfileType.Dehumidification), ProfileType.Dehumidification, dehumidifications);
                    }

                    airHandlingUnitAirMovement = new AirHandlingUnitAirMovement(name, heating, cooling, humidification, dehumidification);
                    adjacencyCluster.AddObject(airHandlingUnitAirMovement);

                    adjacencyCluster.AddRelation(airHandlingUnit, airHandlingUnitAirMovement);


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

            index = Params.IndexOfOutputParam("iZAM");
            if (index != -1)
            {
                dataAccess.SetData(index, airHandlingUnitAirMovement);
            }
        }
    }
}