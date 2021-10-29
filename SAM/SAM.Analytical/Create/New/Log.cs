using SAM.Architectural;
using SAM.Core;
using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Create
    {
        public static Log Log(this ArchitecturalModel architecturalModel)
        {
            if(architecturalModel == null)
            {
                return null;
            }

            Log result = new Log();

            List<HostPartitionType> hostPartitionTypes = architecturalModel.GetHostPartitionTypes();
            if (hostPartitionTypes != null)
            {
                foreach (HostPartitionType hostPartitionType in hostPartitionTypes)
                    Core.Modify.AddRange(result, hostPartitionType?.Log(architecturalModel));
            }

            List<OpeningType> openingTypes = architecturalModel.GetOpeningTypes();
            if (openingTypes != null)
            {
                foreach (OpeningType openingType in openingTypes)
                    Core.Modify.AddRange(result, openingType?.Log(architecturalModel));
            }

            List<IPartition> partitions = architecturalModel.GetPartitions();
            if (partitions != null && partitions.Count != 0)
            {
                foreach (IPartition partition in partitions)
                    Core.Modify.AddRange(result, partition?.Log(architecturalModel));
            }

            List<Space> spaces = architecturalModel.GetSpaces();
            if (spaces != null && spaces.Count != 0)
            {
                foreach (Space space in spaces)
                    Core.Modify.AddRange(result, space?.Log(architecturalModel));
            }

            return result;
        }

        public static Log Log(this Space space, ArchitecturalModel architecturalModel = null)
        {
            if (space == null)
                return null;

            Log result = new Log();

            Core.Modify.AddRange(result, space.InternalCondition?.Log(architecturalModel));

            return result;
        }

        public static Log Log(this InternalCondition internalCondition, ArchitecturalModel architecturalModel = null)
        {
            if (internalCondition == null)
                return null;

            Dictionary<ProfileType, string> dictionary = internalCondition.GetProfileTypeDictionary();
            if (dictionary == null)
                return null;

            string name = internalCondition.Name;
            if (string.IsNullOrEmpty(name))
                name = "???";

            Log result = new Log();

            foreach (ProfileType profileType in Enum.GetValues(typeof(ProfileType)))
            {
                if (profileType == ProfileType.Undefined || profileType == ProfileType.Other)
                    continue;

                string profileName = null;
                if (dictionary == null || !dictionary.TryGetValue(profileType, out profileName))
                    profileName = null;

                Profile profile = null;
                if (!string.IsNullOrEmpty(profileName))
                {
                    profile = architecturalModel.GetProfile(internalCondition, profileType);
                    if (profile == null)
                    {
                        result.Add(string.Format("Cannot find valid {0} profile for {1} InternalCondition (Guid: {2})", profileType.Text(), name, internalCondition.Guid));
                        continue;
                    }
                }

                if (string.IsNullOrEmpty(profileName))
                    profileName = "???";

                double value_1;
                double value_2;

                switch (profileType)
                {
                    case ProfileType.EquipmentLatent:

                        if (!internalCondition.TryGetValue(InternalConditionParameter.EquipmentLatentGain, out value_1))
                            value_1 = double.NaN;

                        if (!internalCondition.TryGetValue(InternalConditionParameter.EquipmentLatentGainPerArea, out value_2))
                            value_2 = double.NaN;

                        if (double.IsNaN(value_1) && double.IsNaN(value_2) && profile != null && !profile.IsOff())
                            result.Add("{0} InternalCondition (Guid: {1}) has {2} {3} (Guid: {4}) assigned but Equipment Latent Gain or Equipment Latent Gain Per Area have not been provided.", LogRecordType.Warning, name, internalCondition.Guid, profileName, profileType.Text(), profile.Guid);
                        else if ((!double.IsNaN(value_1) || !double.IsNaN(value_2)) && profile == null)
                            result.Add("{0} InternalCondition (Guid: {1}) has no {2} assigned but Equipment Latent Gain or Equipment Latent Gain Per Area has been provided.", LogRecordType.Warning, name, internalCondition.Guid, profileType.Text());
                        break;


                    case ProfileType.EquipmentSensible:

                        if (!internalCondition.TryGetValue(InternalConditionParameter.EquipmentSensibleGain, out value_1))
                            value_1 = double.NaN;

                        if (!internalCondition.TryGetValue(InternalConditionParameter.EquipmentSensibleGainPerArea, out value_2))
                            value_2 = double.NaN;

                        if (double.IsNaN(value_1) && double.IsNaN(value_2) && profile != null && !profile.IsOff())
                            result.Add("{0} InternalCondition (Guid: {1}) has {2} {3} (Guid: {4}) assigned but Equipment Sensible Gain or Equipment Sensible Gain Per Area have not been provided.", LogRecordType.Warning, name, internalCondition.Guid, profileName, profileType.Text(), profile.Guid);
                        else if ((!double.IsNaN(value_1) || !double.IsNaN(value_2)) && profile == null)
                            result.Add("{0} InternalCondition (Guid: {1}) has no {2} assigned but Equipment Sensible Gain or Equipment Sensible Gain Per Area has been provided.", LogRecordType.Warning, name, internalCondition.Guid, profileType.Text());
                        break;


                    case ProfileType.Infiltration:

                        if (!internalCondition.TryGetValue(InternalConditionParameter.InfiltrationAirChangesPerHour, out value_1))
                            value_1 = double.NaN;

                        if (double.IsNaN(value_1) && profile != null && !profile.IsOff())
                            result.Add("{0} InternalCondition (Guid: {1}) has {2} {3} (Guid: {4}) assigned but Infiltration Air Changes Per Hour has not been provided.", LogRecordType.Warning, name, internalCondition.Guid, profileName, profileType.Text(), profile.Guid);
                        else if (!double.IsNaN(value_1) && profile == null)
                            result.Add("{0} InternalCondition (Guid: {1}) has no {2} assigned but Infiltration Air Changes Per Hour has been provided.", LogRecordType.Warning, name, internalCondition.Guid, profileType.Text());
                        break;


                    case ProfileType.Lighting:

                        if (!internalCondition.TryGetValue(InternalConditionParameter.LightingGain, out value_1))
                            value_1 = double.NaN;

                        if (!internalCondition.TryGetValue(InternalConditionParameter.LightingGainPerArea, out value_2))
                            value_2 = double.NaN;

                        if (double.IsNaN(value_1) && double.IsNaN(value_2) && profile != null && !profile.IsOff())
                            result.Add("{0} InternalCondition (Guid: {1}) has {2} {3} (Guid: {4}) assigned but Lighting Gain or Lighting Gain Per Area have not been provided.", LogRecordType.Warning, name, internalCondition.Guid, profileName, profileType.Text(), profile.Guid);
                        else if ((!double.IsNaN(value_1) || !double.IsNaN(value_2)) && profile == null)
                            result.Add("{0} InternalCondition (Guid: {1}) has no {2} assigned but Lighting Gain or Lighting Gain Per Area has been provided.", LogRecordType.Warning, name, internalCondition.Guid, profileType.Text());
                        break;


                    case ProfileType.Occupancy:

                        if (!internalCondition.TryGetValue(InternalConditionParameter.OccupancyLatentGainPerPerson, out value_1))
                            value_1 = double.NaN;

                        if (!internalCondition.TryGetValue(InternalConditionParameter.OccupancySensibleGainPerPerson, out value_2))
                            value_2 = double.NaN;

                        if (double.IsNaN(value_1) && double.IsNaN(value_2) && profile != null && !profile.IsOff())
                            result.Add("{0} InternalCondition (Guid: {1}) has {2} {3} (Guid: {4}) assigned but Occupancy Latent Gain Per Person or Occupancy Sensible Gain Per Person have not been provided.", LogRecordType.Warning, name, internalCondition.Guid, profileName, profileType.Text(), profile.Guid);
                        else if ((!double.IsNaN(value_1) || !double.IsNaN(value_2)) && profile == null)
                            result.Add("{0} InternalCondition (Guid: {1}) has no {2} assigned but Occupancy Latent Gain Per Person or Occupancy Sensible Gain Per Person has been provided.", LogRecordType.Warning, name, internalCondition.Guid, profileType.Text());
                        break;


                    case ProfileType.Pollutant:

                        if (!internalCondition.TryGetValue(InternalConditionParameter.PollutantGenerationPerArea, out value_1))
                            value_1 = double.NaN;

                        if (!internalCondition.TryGetValue(InternalConditionParameter.PollutantGenerationPerPerson, out value_2))
                            value_2 = double.NaN;

                        if (double.IsNaN(value_1) && double.IsNaN(value_2) && profile != null && !profile.IsOff())
                            result.Add("{0} InternalCondition (Guid: {1}) has {2} {3} (Guid: {4}) assigned but Pollutant Generation Per Area or Pollutant Generation Per Person have not been provided.", LogRecordType.Warning, name, internalCondition.Guid, profileName, profileType.Text(), profile.Guid);
                        else if ((!double.IsNaN(value_1) || !double.IsNaN(value_2)) && profile == null)
                            result.Add("{0} InternalCondition (Guid: {1}) has no {2} assigned but Pollutant Generation Per Area or Pollutant Generation Per Person has been provided.", LogRecordType.Warning, name, internalCondition.Guid, profileType.Text());
                        break;

                }
            }

            return result;
        }

        public static Log Log(this HostPartitionType hostPartitionType, ArchitecturalModel architecturalModel = null)
        {
            if (hostPartitionType == null)
                return null;

            Log result = new Log();

            string name = hostPartitionType.Name;
            if (string.IsNullOrEmpty(name))
            {
                result.Add(string.Format("PartitionType (Guid: {1}) has no name.", name, hostPartitionType.Guid), LogRecordType.Warning);
                name = "???";
            }

            List<MaterialLayer> materialLayers = hostPartitionType?.MaterialLayers;
            if (materialLayers != null && materialLayers.Count > 0)
                Core.Modify.AddRange(result, materialLayers?.Log(hostPartitionType.Name, hostPartitionType.Guid, architecturalModel));

            return result;
        }

        public static Log Log(this IPartition partition, ArchitecturalModel architecturalModel = null)
        {
            if (partition == null)
            {
                return null;
            }

            string name = partition.Name;
            if (string.IsNullOrEmpty(name))
            {
                name = "???";
            }

            Log result = new Log();

            if (partition is AirPartition)
            {
                name = "Air Partition";
            }
            else if(partition is IHostPartition)
            {
                HostPartitionType hostPartitionType = ((IHostPartition)partition).Type();
                if(hostPartitionType == null)
                {
                    result.Add(string.Format("{0} partition (Guid: {1}) has no type assigned", name, partition.Guid), LogRecordType.Error);
                }
            }
            else
            {
                result.Add(string.Format("Unknown type of {0} partition (Guid: {1})", name, partition.Guid), LogRecordType.Error);
                return result;
            }

            Face3D face3D = partition.Face3D;
            if (face3D == null)
            {
                result.Add(string.Format("{0} partition (Guid: {1}) has no geometry assigned.", name, partition.Guid), LogRecordType.Error);
            }
            else
            {
                double area = face3D.GetArea();
                if (double.IsNaN(area) || area < Tolerance.MacroDistance)
                    result.Add(string.Format("{0} partition (Guid: {1}) area is less than {2}.", name, partition.Guid, Tolerance.MacroDistance), LogRecordType.Warning);
            }

            return result;
        }

        public static Log Log(this OpeningType openingType, ArchitecturalModel architecturalModel = null)
        {
            if (openingType == null)
                return null;

            Log result = new Log();

            List<MaterialLayer> materialLayers = null;

            materialLayers = openingType?.PaneMaterialLayers;
            if (materialLayers != null && materialLayers.Count > 0)
                Core.Modify.AddRange(result, materialLayers?.Log(openingType.Name, openingType.Guid, architecturalModel));

            materialLayers = openingType?.FrameMaterialLayers;
            if (materialLayers != null && materialLayers.Count > 0)
                Core.Modify.AddRange(result, materialLayers?.Log(openingType.Name, openingType.Guid, architecturalModel));

            return result;
        }
        
        public static Log Log(this IOpening opening, ArchitecturalModel architecturalModel = null)
        {
            if (opening == null)
            {
                return null;
            }

            Log result = new Log();

            string name = opening.Name;
            if (string.IsNullOrEmpty(name))
            {
                result.Add(string.Format("Opening (Guid: {1}) has no name.", name, opening.Guid), LogRecordType.Warning);
                name = "???";
            }

            Face3D face3D = opening.Face3D;
            if (face3D == null)
            {
                result.Add(string.Format("{0} opening (Guid: {1}) has no geometry assigned.", name, opening.Guid), LogRecordType.Error);
            }
            else
            {
                double area = face3D.GetArea();
                if (double.IsNaN(area) || area < Tolerance.MacroDistance)
                    result.Add(string.Format("{0} opening (Guid: {1}) area is less than {2}.", name, opening.Guid, Tolerance.MacroDistance), LogRecordType.Warning);
            }

            OpeningType openingType = opening.Type();
            if (openingType == null)
            {
                result.Add(string.Format("{0} opening (Guid: {1}) has no type assigned.", name, opening.Guid), LogRecordType.Error);
            }

            if(architecturalModel != null)
            {
                IHostPartition hostPartition = architecturalModel.GetHostPartition(opening);
                if(hostPartition == null)
                {
                    result.Add(string.Format("{0} opening (Guid: {1}) has no host.", name, opening.Guid), LogRecordType.Warning);
                }
            }

            return result;
        }

        private static Log Log(this IEnumerable<MaterialLayer> materialLayers, string name, Guid guid, ArchitecturalModel architecturalModel = null)
        {
            if (materialLayers == null)
                return null;

            string name_Temp = name;
            if (string.IsNullOrEmpty(name))
                name_Temp = "???";

            Log result = new Log();

            Core.Modify.AddRange(result, Architectural.Create.Log(materialLayers, name, guid));

            if(architecturalModel != null)
            {
                MaterialType materialType = architecturalModel.GetMaterialType(materialLayers);

                int index = 0;
                foreach (ConstructionLayer constructionLayer in materialLayers)
                {
                    IMaterial material = architecturalModel.GetMaterial(constructionLayer);
                    if (material == null)
                        result.Add(string.Format("Material Library does not contain Material {0} for {1} (Guid: {2}) (Construction Layer Index: {3})", constructionLayer.Name, name_Temp, guid, index), LogRecordType.Error);

                    if (material is GasMaterial)
                    {
                        GasMaterial gasMaterial = (GasMaterial)material;
                        DefaultGasType defaultGasType = Query.DefaultGasType(gasMaterial);
                        if (defaultGasType == DefaultGasType.Undefined)
                            result.Add(string.Format("{0} gas material is not recogionzed in {1} (Guid: {2}) (Construction Layer Index: {3}). Heat Transfer Coefficient may not be calculated properly.", constructionLayer.Name, name_Temp, guid, index), LogRecordType.Warning);
                        else if (materialType == MaterialType.Opaque && defaultGasType != DefaultGasType.Air)
                            result.Add(string.Format("{0} Construction Layer for Opaque {1} (Guid: {2}) (Construction Layer Index: {3}) in not recognized as air type. Heat Transfer Coefficient may not be calculated properly.", constructionLayer.Name, name_Temp, guid, index), LogRecordType.Warning);

                        if (defaultGasType != DefaultGasType.Undefined)
                            result.Add(string.Format("Gas Material {0} for {1} (Guid: {2}) recognized as {3} (Construction Layer Index: {4})", constructionLayer.Name, name_Temp, guid, Core.Query.Description(defaultGasType), index), LogRecordType.Message);
                    }
                    index++;
                }
            }

            return result;
        }
    }
}