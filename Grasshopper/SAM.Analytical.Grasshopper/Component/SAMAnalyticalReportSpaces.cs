using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core;
using SAM.Core.Grasshopper;
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
        public override string LatestComponentVersion => "1.0.1";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        public override GH_Exposure Exposure => GH_Exposure.primary;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalReportSpaces()
          : base("SAMAnalytical.ReportSpaces", "SAMAnalytical.ReportSpaces",
              "Report Spaces provide all information about space including assumptions from Internal Condition",
              "SAM", "SAM_Communicate")
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
                result[0] = new GH_SAMParam(new Param_GenericObject() { Name = "_analytical", NickName = "_analytical", Description = "SAM Analytical Object", Access = GH_ParamAccess.item }, ParamVisibility.Binding);
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
                List<GH_SAMParam> result = new List<GH_SAMParam>();
                
                result.Add(new GH_SAMParam(new Param_String() { Name = "Name", NickName = "Name", Description = "Space Name", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new Param_Guid() { Name = "Guid", NickName = "Guid", Description = "Space Guid", Access = GH_ParamAccess.list }, ParamVisibility.Voluntary));
                result.Add(new GH_SAMParam(new Param_Number() { Name = "Area", NickName = "Area", Description = "Space Area", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new Param_Number() { Name = "Volume", NickName = "Volume", Description = "Space Volume", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new Param_Number() { Name = "Occupancy", NickName = "Occupancy", Description = "Space Occupancy", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new Param_Number() { Name = "AreaPerPerson", NickName = "AreaPerPerson", Description = "Area Per Person", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new Param_String() { Name = "LevelName", NickName = "LevelName", Description = "Level Name", Access = GH_ParamAccess.list }, ParamVisibility.Binding));

                result.Add(new GH_SAMParam(new Param_Number() { Name = "Infiltration", NickName = "Infiltration", Description = "Infiltration", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new Param_String() { Name = "InfiltrationProfileName", NickName = "InfiltrationProfileName", Description = "Infiltration Profile Name", Access = GH_ParamAccess.list }, ParamVisibility.Voluntary));
                result.Add(new GH_SAMParam(new Param_Guid() { Name = "InfiltrationProfileGuid", NickName = "InfiltrationProfileGuid", Description = "Infiltration Profile Guid", Access = GH_ParamAccess.list }, ParamVisibility.Voluntary));

                result.Add(new GH_SAMParam(new Param_Number() { Name = "OccupancySensibleGainPerPerson", NickName = "OccupancySensibleGainPerPerson", Description = "Occupancy Sensible Gain Per Person, W/person", Access = GH_ParamAccess.list }, ParamVisibility.Voluntary));
                result.Add(new GH_SAMParam(new Param_Number() { Name = "OccupancySensibleGainPerArea", NickName = "OccupancySensibleGainPerArea", Description = "Occupancy Sensible Gain Per Area, W/m2", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new Param_Number() { Name = "OccupancyLatentGainPerPerson", NickName = "OccupancyLatentGainPerPerson", Description = "Occupancy Latent Gain Per Person, W/person", Access = GH_ParamAccess.list }, ParamVisibility.Voluntary));
                result.Add(new GH_SAMParam(new Param_Number() { Name = "OccupancyLatentGainPerArea", NickName = "OccupancyLatentGainPerArea", Description = "Occupancy Latent Gain Per Area, W/m2", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new Param_Number() { Name = "OccupancySensibleGain", NickName = "OccupancySensibleGain", Description = "Occupancy Sensible Gain, W", Access = GH_ParamAccess.list }, ParamVisibility.Voluntary));
                result.Add(new GH_SAMParam(new Param_Number() { Name = "OccupancyLatentGain", NickName = "OccupancyLatentGain", Description = "Occupancy Latent Gain, W", Access = GH_ParamAccess.list }, ParamVisibility.Voluntary));
                result.Add(new GH_SAMParam(new Param_String() { Name = "OccupancyProfileName", NickName = "OccupancyProfileName", Description = "Occupancy Profile Name", Access = GH_ParamAccess.list }, ParamVisibility.Voluntary));
                result.Add(new GH_SAMParam(new Param_Guid() { Name = "OccupancyProfileGuid", NickName = "OccupancyProfileGuid", Description = "Occupancy Profile Guid", Access = GH_ParamAccess.list }, ParamVisibility.Voluntary));

                result.Add(new GH_SAMParam(new Param_Number() { Name = "EquipmentSensibleGainPerArea", NickName = "EquipmentSensibleGainPerArea", Description = "Equipment Sensible Gain Per Area, W/m2", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new Param_Number() { Name = "EquipmentLatentGainPerArea", NickName = "EquipmentLatentGainPerArea", Description = "Equipment Latent Gain Per Area, W/m2", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new Param_Number() { Name = "EquipmentSensibleGain", NickName = "EquipmentSensibleGain", Description = "Equipment Sensible Gain, W", Access = GH_ParamAccess.list }, ParamVisibility.Voluntary));
                result.Add(new GH_SAMParam(new Param_Number() { Name = "EquipmentLatentGain", NickName = "EquipmentLatentGain", Description = "Equipment Latent Gain, W", Access = GH_ParamAccess.list }, ParamVisibility.Voluntary));
                result.Add(new GH_SAMParam(new Param_String() { Name = "EquipmentSensibleProfileName", NickName = "EquipmentSensibleProfileName", Description = "Equipment Sensible Profile Name", Access = GH_ParamAccess.list }, ParamVisibility.Voluntary));
                result.Add(new GH_SAMParam(new Param_Guid() { Name = "EquipmentSensibleProfileGuid", NickName = "EquipmentSensibleProfileGuid", Description = "Equipment Sensible Profile Guid", Access = GH_ParamAccess.list }, ParamVisibility.Voluntary));
                result.Add(new GH_SAMParam(new Param_String() { Name = "EquipmentLatentProfileName", NickName = "EquipmentLatentProfileName", Description = "Equipment Latent Profile Name", Access = GH_ParamAccess.list }, ParamVisibility.Voluntary));
                result.Add(new GH_SAMParam(new Param_Guid() { Name = "EquipmentLatentProfileGuid", NickName = "EquipmentLatentProfileGuid", Description = "Equipment Latent Profile Guid", Access = GH_ParamAccess.list }, ParamVisibility.Voluntary));

                result.Add(new GH_SAMParam(new Param_Number() { Name = "LightingGainPerArea", NickName = "LightingGainPerArea", Description = "Lighting Gain Per Area, W/m2", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new Param_Number() { Name = "LightingLevel", NickName = "LightingLevel", Description = "Lighting Level, lx", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new Param_Number() { Name = "LightingGain", NickName = "LightingGain", Description = "Lighting Gain, W", Access = GH_ParamAccess.list }, ParamVisibility.Voluntary));
                result.Add(new GH_SAMParam(new Param_String() { Name = "LightingProfileName", NickName = "LightingProfileName", Description = "Lighting Profile Name", Access = GH_ParamAccess.list }, ParamVisibility.Voluntary));
                result.Add(new GH_SAMParam(new Param_Guid() { Name = "LightingProfileGuid", NickName = "LightingProfileGuid", Description = "Lighting Profile Guid", Access = GH_ParamAccess.list }, ParamVisibility.Voluntary));

                result.Add(new GH_SAMParam(new Param_Number() { Name = "HeatingDesignTemperature", NickName = "HeatingDesignTemperature", Description = "Heating Design Temperature, degC", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new Param_String() { Name = "HeatingProfileName", NickName = "HeatingProfileName", Description = "Heating Profile Name", Access = GH_ParamAccess.list }, ParamVisibility.Voluntary));
                result.Add(new GH_SAMParam(new Param_Guid() { Name = "HeatingProfileGuid", NickName = "HeatingProfileGuid", Description = "Heating Profile Guid", Access = GH_ParamAccess.list }, ParamVisibility.Voluntary));

                result.Add(new GH_SAMParam(new Param_Number() { Name = "CoolingDesignTemperature", NickName = "CoolingDesignTemperature", Description = "Cooling Design Temperature, degC", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new Param_String() { Name = "CoolingProfileName", NickName = "CoolingProfileName", Description = "Cooling Profile Name", Access = GH_ParamAccess.list }, ParamVisibility.Voluntary));
                result.Add(new GH_SAMParam(new Param_Guid() { Name = "CoolingProfileGuid", NickName = "CoolingProfileGuid", Description = "Cooling Profile Guid", Access = GH_ParamAccess.list }, ParamVisibility.Voluntary));

                result.Add(new GH_SAMParam(new Param_Number() { Name = "Humidity", NickName = "Humidity", Description = "Humidity", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new Param_String() { Name = "HumidificationProfileName", NickName = "HumidificationProfileName", Description = "Humidification Profile Name", Access = GH_ParamAccess.list }, ParamVisibility.Voluntary));
                result.Add(new GH_SAMParam(new Param_Guid() { Name = "HumidificationProfileGuid", NickName = "HumidificationProfileGuid", Description = "Humidification Profile Guid", Access = GH_ParamAccess.list }, ParamVisibility.Voluntary));

                result.Add(new GH_SAMParam(new Param_Number() { Name = "Dehumidity", NickName = "Dehumidity", Description = "Dehumidity", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new Param_String() { Name = "DehumidificationProfileName", NickName = "DehumidificationProfileName", Description = "Dehumidification Profile Name", Access = GH_ParamAccess.list }, ParamVisibility.Voluntary));
                result.Add(new GH_SAMParam(new Param_Guid() { Name = "DehumidificationProfileGuid", NickName = "DehumidificationProfileGuid", Description = "Dehumidification Profile Guid", Access = GH_ParamAccess.list }, ParamVisibility.Voluntary));

                result.Add(new GH_SAMParam(new Param_String() { Name = "InternalConditionName", NickName = "InternalConditionName", Description = "InternalCondition Name", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new Param_Guid() { Name = "InternalConditionGuid", NickName = "InternalConditionGuid", Description = "InternalCondition Guid", Access = GH_ParamAccess.list }, ParamVisibility.Voluntary));

                result.Add(new GH_SAMParam(new Param_String() { Name = "VentilationSystemTypeName", NickName = "VentilationSystemTypeName", Description = "VentilationSystemType Name", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new Param_Guid() { Name = "VentilationSystemTypenGuid", NickName = "VentilationSystemTypenGuid", Description = "VentilationSystemTypen Guid", Access = GH_ParamAccess.list }, ParamVisibility.Voluntary));

                result.Add(new GH_SAMParam(new Param_String() { Name = "HeatingSystemTypeName", NickName = "HeatingSystemTypeName", Description = "HeatingSystemType Name", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new Param_Guid() { Name = "HeatingSystemTypenGuid", NickName = "HeatingSystemTypenGuid", Description = "HeatingSystemTypen Guid", Access = GH_ParamAccess.list }, ParamVisibility.Voluntary));

                result.Add(new GH_SAMParam(new Param_String() { Name = "CoolingSystemTypeName", NickName = "CoolingSystemTypeName", Description = "CoolingSystemType Name", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new Param_Guid() { Name = "CoolingSystemTypeGuid", NickName = "CoolingSystemTypeGuid", Description = "CoolingSystemType Guid", Access = GH_ParamAccess.list }, ParamVisibility.Voluntary));

                result.Add(new GH_SAMParam(new Param_String() { Name = "SupplyUnitName", NickName = "SupplyUnitName", Description = "Supply Unit Name", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new Param_Number() { Name = "SupplyAirFlow", NickName = "SupplyAirFlow", Description = "Supply Air Flow, m3/s", Access = GH_ParamAccess.list }, ParamVisibility.Voluntary));

                result.Add(new GH_SAMParam(new Param_String() { Name = "ExhaustUnitName", NickName = "ExhaustUnitName", Description = "Exhaust Unit Name", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new Param_Number() { Name = "ExhaustAirFlow", NickName = "ExhaustAirFlow", Description = "Exhaust Air Flow, m3/s", Access = GH_ParamAccess.list }, ParamVisibility.Voluntary));

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
                spaces = adjacencyCluster.GetSpaces();
            else if (sAMObject is Space)
                spaces = new List<Space>() { (Space)sAMObject };

            if (spaces == null)
                return;

            List<string> names_Space = new List<string>();
            List<Guid> guids_Space = new List<Guid>();
            List<double> areas = new List<double>();
            List<double> volumes = new List<double>();
            List<double> occupancies = new List<double>();
            List<double> areaPerPersons = new List<double>();
            List<string> names_Level = new List<string>();

            List<double> infiltrations = new List<double>();
            List<string> names_Infiltration= new List<string>();
            List<Guid> guids_Infiltration = new List<Guid>();

            List<double> occupancySensibleGains = new List<double>();
            List<double> occupancyLatentGains = new List<double>();
            List<double> occupancySensibleGainsPerPerson = new List<double>();
            List<double> occupancySensibleGainsPerArea = new List<double>();
            List<double> occupancyLatentGainsPerPerson = new List<double>();
            List<double> occupancyLatentGainsPerArea = new List<double>();
            List<string> names_Occupancy = new List<string>();
            List<Guid> guids_Occupancy = new List<Guid>();

            List<double> equipmentSensibleGains = new List<double>();
            List<double> equipmentLatentGains = new List<double>();
            List<double> equipmentSensibleGainsPerArea = new List<double>();
            List<double> equipmentLatentGainsPerArea = new List<double>();
            List<string> names_EquipmentSensible = new List<string>();
            List<Guid> guids_EquipmentSensible = new List<Guid>();
            List<string> names_EquipmentLatent = new List<string>();
            List<Guid> guids_EquipmentLatent = new List<Guid>();

            List<double> lightingGains = new List<double>();
            List<double> lightingLevels = new List<double>();
            List<double> lightingGainsPerArea = new List<double>();
            List<string> names_Lighting = new List<string>();
            List<Guid> guids_Lighting = new List<Guid>();

            List<double> heatingDesignTemperatures = new List<double>();
            List<string> names_Heating = new List<string>();
            List<Guid> guids_Heating = new List<Guid>();

            List<double> coolingDesignTemperatures = new List<double>();
            List<string> names_Cooling = new List<string>();
            List<Guid> guids_Cooling = new List<Guid>();

            List<double> humidities = new List<double>();
            List<string> names_Humidification = new List<string>();
            List<Guid> guids_Humidification = new List<Guid>();

            List<double> dehumidities = new List<double>();
            List<string> names_Dehumidification = new List<string>();
            List<Guid> guids_Dehumidification = new List<Guid>();

            List<string> names_InternalCondition= new List<string>();
            List<Guid> guids_InternalCondition = new List<Guid>();

            List<string> names_VentilationSystemType = new List<string>();
            List<Guid> guids_VentilationSystemType = new List<Guid>();

            List<string> names_HeatingSystemType = new List<string>();
            List<Guid> guids_HeatingSystemType = new List<Guid>();

            List<string> names_CoolingSystemType = new List<string>();
            List<Guid> guids_CoolingSystemType = new List<Guid>();

            List<string> names_SupplyUnit = new List<string>();
            List<double> supplyAirFlows = new List<double>();

            List<string> names_ExhaustUnit = new List<string>();
            List<double> exhaustAirFlows = new List<double>();

            foreach (Space space in spaces)
            {
                string @string = null;
                double @double = double.NaN;
                Profile profile = null;

                guids_Space.Add(space == null ? Guid.Empty : space.Guid);
                names_Space.Add(space?.Name);

                double area = double.NaN;
                if (space == null || !space.TryGetValue(SpaceParameter.Area, out area))
                    area = double.NaN;

                areas.Add(area);

                if (space == null || !space.TryGetValue(SpaceParameter.Volume, out @double))
                    @double = double.NaN;

                volumes.Add(@double);

                if (space == null || !space.TryGetValue(SpaceParameter.LevelName, out @string))
                    @string = null;

                names_Level.Add(@string);

                InternalCondition internalCondition = space?.InternalCondition;

                double areaPerPerson = double.NaN;
                if (internalCondition == null || !internalCondition.TryGetValue(InternalConditionParameter.AreaPerPerson, out areaPerPerson))
                    areaPerPerson = double.NaN;

                areaPerPersons.Add(areaPerPerson);

                double occupancy = Analytical.Query.CalculatedOccupancy(space);
                occupancies.Add(occupancy);

                //Infiltration
                if (internalCondition == null || !internalCondition.TryGetValue(InternalConditionParameter.InfiltrationAirChangesPerHour, out @double))
                    @double = double.NaN;

                infiltrations.Add(@double);

                if (internalCondition == null || !internalCondition.TryGetValue(InternalConditionParameter.InfiltrationProfileName, out @string))
                    @string = null;

                names_Infiltration.Add(@string);

                profile = internalCondition?.GetProfile(ProfileType.Infiltration, analyticalModel?.ProfileLibrary);
                guids_Infiltration.Add(profile == null ? Guid.Empty : profile.Guid);


                //Occupancy
                double occupancySensibleGain = Analytical.Query.OccupancySensibleGain(space);

                occupancySensibleGains.Add(occupancySensibleGain);
                occupancySensibleGainsPerPerson.Add(double.IsNaN(occupancySensibleGain) || double.IsNaN(occupancy) || occupancy == 0 ? double.NaN : occupancySensibleGain / occupancy);
                occupancySensibleGainsPerArea.Add(double.IsNaN(occupancySensibleGain) || double.IsNaN(area) || area == 0 ? double.NaN : occupancySensibleGain / area);

                double occupancyLatentGain = Analytical.Query.OccupancyLatentGain(space);
                
                occupancyLatentGains.Add(occupancyLatentGain);
                occupancyLatentGainsPerPerson.Add(double.IsNaN(occupancyLatentGain) || double.IsNaN(occupancy) || occupancy == 0 ? double.NaN : occupancyLatentGain / occupancy);
                occupancyLatentGainsPerArea.Add(double.IsNaN(occupancyLatentGain) || double.IsNaN(area) || area == 0 ? double.NaN : occupancyLatentGain / area);

                if (internalCondition == null || !internalCondition.TryGetValue(InternalConditionParameter.OccupancyProfileName, out @string))
                    @string = null;

                names_Occupancy.Add(@string);

                profile = internalCondition?.GetProfile(ProfileType.Occupancy, analyticalModel?.ProfileLibrary);
                guids_Occupancy.Add(profile == null ? Guid.Empty : profile.Guid);


                //Equipment
                double equipmentSensibleGain = Analytical.Query.CalculatedEquipmentSensibleGain(space);
                equipmentSensibleGains.Add(equipmentSensibleGain);
                equipmentSensibleGainsPerArea.Add(double.IsNaN(equipmentSensibleGain) || double.IsNaN(area) || area == 0 ? double.NaN : equipmentSensibleGain / area);

                double equipmentLatentGain = Analytical.Query.CalculatedEquipmentLatentGain(space);
                equipmentLatentGains.Add(equipmentLatentGain);
                equipmentLatentGainsPerArea.Add(double.IsNaN(equipmentLatentGain) || double.IsNaN(area) || area == 0 ? double.NaN : equipmentLatentGain / area);

                if (internalCondition == null || !internalCondition.TryGetValue(InternalConditionParameter.EquipmentSensibleProfileName, out @string))
                    @string = null;

                names_EquipmentSensible.Add(@string);

                profile = internalCondition?.GetProfile(ProfileType.EquipmentSensible, analyticalModel?.ProfileLibrary);
                guids_EquipmentSensible.Add(profile == null ? Guid.Empty : profile.Guid);

                if (internalCondition == null || !internalCondition.TryGetValue(InternalConditionParameter.EquipmentLatentProfileName, out @string))
                    @string = null;

                names_EquipmentLatent.Add(@string);

                profile = internalCondition?.GetProfile(ProfileType.EquipmentLatent, analyticalModel?.ProfileLibrary);
                guids_EquipmentLatent.Add(profile == null ? Guid.Empty : profile.Guid);


                //Lighting
                double lightingGain = Analytical.Query.CalculatedLightingGain(space);
                lightingGains.Add(lightingGain);
                lightingGainsPerArea.Add(double.IsNaN(lightingGain) || double.IsNaN(area) || area == 0 ? double.NaN : lightingGain / area);

                if (internalCondition == null || !internalCondition.TryGetValue(InternalConditionParameter.LightingLevel, out @double))
                    @double = double.NaN;

                lightingLevels.Add(@double);

                if (internalCondition == null || !internalCondition.TryGetValue(InternalConditionParameter.LightingProfileName, out @string))
                    @string = null;

                names_Lighting.Add(@string);

                profile = internalCondition?.GetProfile(ProfileType.Lighting, analyticalModel?.ProfileLibrary);
                guids_Lighting.Add(profile == null ? Guid.Empty : profile.Guid);


                //Heating
                if (internalCondition == null || !internalCondition.TryGetValue(InternalConditionParameter.HeatingProfileName, out @string))
                    @string = null;

                names_Heating.Add(@string);

                profile = internalCondition?.GetProfile(ProfileType.Heating, analyticalModel?.ProfileLibrary);
                guids_Heating.Add(profile == null ? Guid.Empty : profile.Guid);

                heatingDesignTemperatures.Add(Analytical.Query.HeatingDesignTemperature(space, analyticalModel?.ProfileLibrary));


                //Cooling
                if (internalCondition == null || !internalCondition.TryGetValue(InternalConditionParameter.CoolingProfileName, out @string))
                    @string = null;

                names_Cooling.Add(@string);

                profile = internalCondition?.GetProfile(ProfileType.Cooling, analyticalModel?.ProfileLibrary);
                guids_Cooling.Add(profile == null ? Guid.Empty : profile.Guid);

                coolingDesignTemperatures.Add(Analytical.Query.CoolingDesignTemperature(space, analyticalModel?.ProfileLibrary));


                //Humidification
                if (internalCondition == null || !internalCondition.TryGetValue(InternalConditionParameter.HumidificationProfileName, out @string))
                    @string = null;

                names_Humidification.Add(@string);

                profile = internalCondition?.GetProfile(ProfileType.Humidification, analyticalModel?.ProfileLibrary);
                guids_Humidification.Add(profile == null ? Guid.Empty : profile.Guid);

                humidities.Add(profile == null ? double.NaN : profile.MaxValue);


                //Dehumidification
                if (internalCondition == null || !internalCondition.TryGetValue(InternalConditionParameter.DehumidificationProfileName, out @string))
                    @string = null;

                names_Dehumidification.Add(@string);

                profile = internalCondition?.GetProfile(ProfileType.Dehumidification, analyticalModel?.ProfileLibrary);
                guids_Dehumidification.Add(profile == null ? Guid.Empty : profile.Guid);

                dehumidities.Add(profile == null ? double.NaN : profile.MinValue);


                names_InternalCondition.Add(internalCondition?.Name);
                guids_InternalCondition.Add(internalCondition == null ? Guid.Empty : internalCondition.Guid);

                names_VentilationSystemType.Add(internalCondition?.GetSystemTypeName<VentilationSystemType>());
                VentilationSystem ventilationSystem = adjacencyCluster?.GetRelatedObjects<VentilationSystem>(space)?.FirstOrDefault();
                VentilationSystemType ventilationSystemType = ventilationSystem?.SAMType as VentilationSystemType;
                guids_VentilationSystemType.Add(ventilationSystemType == null ? Guid.Empty : ventilationSystemType.Guid);

                names_HeatingSystemType.Add(internalCondition?.GetSystemTypeName<HeatingSystemType>());
                HeatingSystem heatingSystem = adjacencyCluster?.GetRelatedObjects<HeatingSystem>(space)?.FirstOrDefault();
                HeatingSystemType heatingSystemType = heatingSystem?.SAMType as HeatingSystemType;
                guids_HeatingSystemType.Add(heatingSystemType == null ? Guid.Empty : heatingSystemType.Guid);

                names_CoolingSystemType.Add(internalCondition?.GetSystemTypeName<CoolingSystemType>());
                CoolingSystem coolingSystem = adjacencyCluster?.GetRelatedObjects<CoolingSystem>(space)?.FirstOrDefault();
                CoolingSystemType coolingSystemType = coolingSystem?.SAMType as CoolingSystemType;
                guids_CoolingSystemType.Add(coolingSystemType == null ? Guid.Empty : coolingSystemType.Guid);

                if (ventilationSystem == null || !ventilationSystem.TryGetValue(VentilationSystemParameter.SupplyUnitName, out @string))
                    @string = null;

                names_SupplyUnit.Add(@string);
                supplyAirFlows.Add(Analytical.Query.SupplyAirFlow(space));
                   
                if (ventilationSystem == null || !ventilationSystem.TryGetValue(VentilationSystemParameter.ExhaustUnitName, out @string))
                    @string = null;

                names_ExhaustUnit.Add(@string);
                exhaustAirFlows.Add(Analytical.Query.ExhaustAirFlow(space));
            }

            int index;

            index = Params.IndexOfOutputParam("Name");
            if (index != -1)
                dataAccess.SetDataList(index, names_Space);

            index = Params.IndexOfOutputParam("Guid");
            if (index != -1)
                dataAccess.SetDataList(index, guids_Space);

            index = Params.IndexOfOutputParam("Area");
            if (index != -1)
                dataAccess.SetDataList(index, areas);

            index = Params.IndexOfOutputParam("Volume");
            if (index != -1)
                dataAccess.SetDataList(index, volumes);

            index = Params.IndexOfOutputParam("Occupancy");
            if (index != -1)
                dataAccess.SetDataList(index, occupancies);

            index = Params.IndexOfOutputParam("AreaPerPerson");
            if (index != -1)
                dataAccess.SetDataList(index, areaPerPersons);

            index = Params.IndexOfOutputParam("LevelName");
            if (index != -1)
                dataAccess.SetDataList(index, names_Level);

            index = Params.IndexOfOutputParam("Infiltration");
            if (index != -1)
                dataAccess.SetDataList(index, infiltrations);

            index = Params.IndexOfOutputParam("InfiltrationProfileName");
            if (index != -1)
                dataAccess.SetDataList(index, names_Infiltration);

            index = Params.IndexOfOutputParam("InfiltrationProfileGuid");
            if (index != -1)
                dataAccess.SetDataList(index, guids_Infiltration);

            index = Params.IndexOfOutputParam("OccupancySensibleGainPerPerson");
            if (index != -1)
                dataAccess.SetDataList(index, occupancySensibleGainsPerPerson);

            index = Params.IndexOfOutputParam("OccupancySensibleGainPerArea");
            if (index != -1)
                dataAccess.SetDataList(index, occupancySensibleGainsPerArea);

            index = Params.IndexOfOutputParam("OccupancyLatentGainPerPerson");
            if (index != -1)
                dataAccess.SetDataList(index, occupancyLatentGainsPerPerson);

            index = Params.IndexOfOutputParam("OccupancyLatentGainPerArea");
            if (index != -1)
                dataAccess.SetDataList(index, occupancyLatentGainsPerArea);

            index = Params.IndexOfOutputParam("OccupancySensibleGain");
            if (index != -1)
                dataAccess.SetDataList(index, occupancySensibleGains);

            index = Params.IndexOfOutputParam("OccupancyLatentGain");
            if (index != -1)
                dataAccess.SetDataList(index, occupancyLatentGains);

            index = Params.IndexOfOutputParam("OccupancyProfileName");
            if (index != -1)
                dataAccess.SetDataList(index, names_Occupancy);

            index = Params.IndexOfOutputParam("OccupancyProfileGuid");
            if (index != -1)
                dataAccess.SetDataList(index, guids_Occupancy);

            index = Params.IndexOfOutputParam("EquipmentSensibleGainPerArea");
            if (index != -1)
                dataAccess.SetDataList(index, equipmentSensibleGainsPerArea);

            index = Params.IndexOfOutputParam("EquipmentLatentGainPerArea");
            if (index != -1)
                dataAccess.SetDataList(index, equipmentLatentGainsPerArea);

            index = Params.IndexOfOutputParam("EquipmentSensibleGain");
            if (index != -1)
                dataAccess.SetDataList(index, equipmentSensibleGains);

            index = Params.IndexOfOutputParam("EquipmentLatentGain");
            if (index != -1)
                dataAccess.SetDataList(index, equipmentLatentGains);

            index = Params.IndexOfOutputParam("EquipmentLatentProfileName");
            if (index != -1)
                dataAccess.SetDataList(index, names_EquipmentLatent);

            index = Params.IndexOfOutputParam("EquipmentLatentProfileGuid");
            if (index != -1)
                dataAccess.SetDataList(index, guids_EquipmentLatent);

            index = Params.IndexOfOutputParam("EquipmentSensibleProfileName");
            if (index != -1)
                dataAccess.SetDataList(index, names_EquipmentSensible);

            index = Params.IndexOfOutputParam("EquipmentSensibleProfileGuid");
            if (index != -1)
                dataAccess.SetDataList(index, guids_EquipmentSensible);

            index = Params.IndexOfOutputParam("LightingGainPerArea");
            if (index != -1)
                dataAccess.SetDataList(index, lightingGainsPerArea);

            index = Params.IndexOfOutputParam("LightingLevel");
            if (index != -1)
                dataAccess.SetDataList(index, lightingLevels);

            index = Params.IndexOfOutputParam("LightingGain");
            if (index != -1)
                dataAccess.SetDataList(index, lightingGains);

            index = Params.IndexOfOutputParam("LightingProfileName");
            if (index != -1)
                dataAccess.SetDataList(index, names_Lighting);

            index = Params.IndexOfOutputParam("LightingProfileGuid");
            if (index != -1)
                dataAccess.SetDataList(index, guids_Lighting);

            index = Params.IndexOfOutputParam("HeatingDesignTemperature");
            if (index != -1)
                dataAccess.SetDataList(index, heatingDesignTemperatures);

            index = Params.IndexOfOutputParam("HeatingProfileName");
            if (index != -1)
                dataAccess.SetDataList(index, names_Heating);

            index = Params.IndexOfOutputParam("HeatingProfileGuid");
            if (index != -1)
                dataAccess.SetDataList(index, guids_Heating);

            index = Params.IndexOfOutputParam("CoolingDesignTemperature");
            if (index != -1)
                dataAccess.SetDataList(index, coolingDesignTemperatures);

            index = Params.IndexOfOutputParam("CoolingProfileName");
            if (index != -1)
                dataAccess.SetDataList(index, names_Cooling);

            index = Params.IndexOfOutputParam("CoolingProfileGuid");
            if (index != -1)
                dataAccess.SetDataList(index, guids_Cooling);

            index = Params.IndexOfOutputParam("Humidity");
            if (index != -1)
                dataAccess.SetDataList(index, humidities);

            index = Params.IndexOfOutputParam("HumidificationProfileName");
            if (index != -1)
                dataAccess.SetDataList(index, names_Humidification);

            index = Params.IndexOfOutputParam("HumidificationProfileGuid");
            if (index != -1)
                dataAccess.SetDataList(index, guids_Humidification);

            index = Params.IndexOfOutputParam("Dehumidity");
            if (index != -1)
                dataAccess.SetDataList(index, dehumidities);

            index = Params.IndexOfOutputParam("DehumidificationProfileName");
            if (index != -1)
                dataAccess.SetDataList(index, names_Dehumidification);

            index = Params.IndexOfOutputParam("DehumidificationProfileGuid");
            if (index != -1)
                dataAccess.SetDataList(index, guids_Dehumidification);

            index = Params.IndexOfOutputParam("InternalConditionName");
            if (index != -1)
                dataAccess.SetDataList(index, names_InternalCondition);

            index = Params.IndexOfOutputParam("InternalConditionGuid");
            if (index != -1)
                dataAccess.SetDataList(index, guids_InternalCondition);

            index = Params.IndexOfOutputParam("VentilationSystemTypeName");
            if (index != -1)
                dataAccess.SetDataList(index, names_VentilationSystemType);

            index = Params.IndexOfOutputParam("VentilationSystemTypenGuid");
            if (index != -1)
                dataAccess.SetDataList(index, guids_VentilationSystemType);

            index = Params.IndexOfOutputParam("HeatingSystemTypeName");
            if (index != -1)
                dataAccess.SetDataList(index, names_HeatingSystemType);

            index = Params.IndexOfOutputParam("HeatingSystemTypenGuid");
            if (index != -1)
                dataAccess.SetDataList(index, guids_HeatingSystemType);

            index = Params.IndexOfOutputParam("CoolingSystemTypeName");
            if (index != -1)
                dataAccess.SetDataList(index, names_CoolingSystemType);

            index = Params.IndexOfOutputParam("CoolingSystemTypeGuid");
            if (index != -1)
                dataAccess.SetDataList(index, guids_CoolingSystemType);

            index = Params.IndexOfOutputParam("SupplyUnitName");
            if (index != -1)
                dataAccess.SetDataList(index, names_SupplyUnit);

            index = Params.IndexOfOutputParam("SupplyAirFlow");
            if (index != -1)
                dataAccess.SetDataList(index, supplyAirFlows);

            index = Params.IndexOfOutputParam("ExhaustUnitName");
            if (index != -1)
                dataAccess.SetDataList(index, names_ExhaustUnit);

            index = Params.IndexOfOutputParam("ExhaustAirFlow");
            if (index != -1)
                dataAccess.SetDataList(index, exhaustAirFlows);
        }
    }
}