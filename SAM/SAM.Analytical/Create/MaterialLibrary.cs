using SAM.Core;

namespace SAM.Analytical
{
    public static partial class Create
    {
        public static MaterialLibrary MaterialLibrary(
            string path, 
            string parameterName_Type,
            string parameterName_Name,
            string parameterName_Description, 
            string parameterName_DefaultThickness, 
            string parameterName_ThermalConductivity, 
            string parameterName_SpecificHeatCapacity, 
            string parameterName_Density,
            string parameterName_VapourDiffusionFactor,
            string parameterName_ExternalSolarReflectance,
            string parameterName_InternalSolarReflectance,
            string parameterName_ExternalLightReflectance,
            string parameterName_InternalLightReflectance,
            string parameterName_ExternalEmissivity,
            string parameterName_InternalEmissivity,
            string parameterName_IgnoreThermalTransmittanceCalculations,
            string parameterName_SolarTransmittance,
            string parameterName_LightTransmittance,
            string parameterName_IsBlind,
            string parameterName_HeatTransferCoefficient,
            string name = null, 
            int namesIndex = 0, 
            int headerCount = 7)
        {
            if (string.IsNullOrWhiteSpace(path) || !System.IO.File.Exists(path))
                return null;

            DelimitedFileTable delimitedFileTable = null;

            using (DelimitedFileReader delimitedFileReader = new DelimitedFileReader(DelimitedFileType.Csv, path))
            {
                delimitedFileTable = new DelimitedFileTable(delimitedFileReader, namesIndex, headerCount);
            }

            if (delimitedFileTable == null)
                return null;

            if (name == null)
                name = System.IO.Path.GetFileNameWithoutExtension(path);

            MaterialLibrary result = new MaterialLibrary(name);

            int count = delimitedFileTable.RowCount;
            if (count == 0)
                return result;

            int index_MaterialType = delimitedFileTable.GetColumnIndex(parameterName_Type);
            if (index_MaterialType == -1)
                return result;

            int index_MaterialName = delimitedFileTable.GetColumnIndex(parameterName_Name);
            if (index_MaterialName == -1)
                return result;

            int index_MaterialDescription = delimitedFileTable.GetColumnIndex(parameterName_Description);
            int index_DefaultThickness = delimitedFileTable.GetColumnIndex(parameterName_DefaultThickness);
            int index_ThermalConductivity = delimitedFileTable.GetColumnIndex(parameterName_ThermalConductivity);
            int index_SpecificHeatCapacity = delimitedFileTable.GetColumnIndex(parameterName_SpecificHeatCapacity);
            int index_Density = delimitedFileTable.GetColumnIndex(parameterName_Density);
            int index_VapourDiffusionFactor = delimitedFileTable.GetColumnIndex(parameterName_VapourDiffusionFactor);
            int index_ExternalSolarReflectance = delimitedFileTable.GetColumnIndex(parameterName_ExternalSolarReflectance);
            int index_InternalSolarReflectance = delimitedFileTable.GetColumnIndex(parameterName_InternalSolarReflectance);
            int index_ExternalLightReflectance = delimitedFileTable.GetColumnIndex(parameterName_ExternalLightReflectance);
            int index_InternalLightReflectance = delimitedFileTable.GetColumnIndex(parameterName_InternalLightReflectance);
            int index_ExternalEmissivity = delimitedFileTable.GetColumnIndex(parameterName_ExternalEmissivity);
            int index_InternalEmissivity = delimitedFileTable.GetColumnIndex(parameterName_InternalEmissivity);
            int index_IgnoreThermalTransmittanceCalculations = delimitedFileTable.GetColumnIndex(parameterName_IgnoreThermalTransmittanceCalculations);
            int index_SolarTransmittance = delimitedFileTable.GetColumnIndex(parameterName_SolarTransmittance);
            int index_LightTransmittance = delimitedFileTable.GetColumnIndex(parameterName_LightTransmittance);
            int index_IsBlind = delimitedFileTable.GetColumnIndex(parameterName_IsBlind);
            int index_HeatTransferCoefficient = delimitedFileTable.GetColumnIndex(parameterName_HeatTransferCoefficient);

            string opaqueName = typeof(OpaqueMaterial).Name.ToUpper();
            string transparentName = typeof(TransparentMaterial).Name.ToUpper();
            string gasName = typeof(GasMaterial).Name.ToUpper();

            for (int i = 0; i < count; i++)
            {
                string materialTypeString = null;
                if (!delimitedFileTable.TryConvert(i, index_MaterialType, out materialTypeString) || string.IsNullOrWhiteSpace(materialTypeString))
                    continue;

                materialTypeString = materialTypeString.Trim().ToUpper();

                string materialName = null;
                if (!delimitedFileTable.TryConvert(i, index_MaterialName, out materialName) || string.IsNullOrWhiteSpace(materialName))
                    continue;

                string description = null;
                delimitedFileTable.TryConvert(i, index_MaterialDescription, out description);

                double defaultThickness = double.NaN;
                delimitedFileTable.TryConvert(i, index_DefaultThickness, out defaultThickness);
                if (!double.IsNaN(defaultThickness))
                    defaultThickness = defaultThickness / 1000;

                double vapourDiffusionFactor = double.NaN;
                delimitedFileTable.TryConvert(i, index_VapourDiffusionFactor, out vapourDiffusionFactor);

                IMaterial material = null;

                if (opaqueName.Contains(materialTypeString))
                {
                    //OpaqueMaterial

                    double thermalConductivity = double.NaN;
                    delimitedFileTable.TryConvert(i, index_ThermalConductivity, out thermalConductivity);

                    double specificHeatCapacity = double.NaN;
                    delimitedFileTable.TryConvert(i, index_SpecificHeatCapacity, out specificHeatCapacity);

                    double density = double.NaN;
                    delimitedFileTable.TryConvert(i, index_Density, out density);

                    double externalSolarReflectance = double.NaN;
                    delimitedFileTable.TryConvert(i, index_ExternalSolarReflectance, out externalSolarReflectance);

                    double internalSolarReflectance = double.NaN;
                    delimitedFileTable.TryConvert(i, index_InternalSolarReflectance, out internalSolarReflectance);

                    double externalLightReflectance = double.NaN;
                    delimitedFileTable.TryConvert(i, index_ExternalLightReflectance, out externalLightReflectance);

                    double internalLightReflectance = double.NaN;
                    delimitedFileTable.TryConvert(i, index_InternalLightReflectance, out internalLightReflectance);

                    double externalEmissivity = double.NaN;
                    delimitedFileTable.TryConvert(i, index_ExternalEmissivity, out externalEmissivity);

                    double internalEmissivity = double.NaN;
                    delimitedFileTable.TryConvert(i, index_InternalEmissivity, out internalEmissivity);

                    bool ignoreThermalTransmittanceCalculations = false;
                    if (!delimitedFileTable.TryConvert(i, index_IgnoreThermalTransmittanceCalculations, out ignoreThermalTransmittanceCalculations))
                        ignoreThermalTransmittanceCalculations = false;

                    material = OpaqueMaterial(materialName, null, materialName, description, thermalConductivity, specificHeatCapacity, density, defaultThickness, vapourDiffusionFactor, externalSolarReflectance, internalSolarReflectance, externalLightReflectance, internalLightReflectance, externalEmissivity, internalEmissivity, ignoreThermalTransmittanceCalculations);

                }
                else if (transparentName.Contains(materialTypeString))
                {
                    //TransparentMaterial

                    double thermalConductivity = double.NaN;
                    delimitedFileTable.TryConvert(i, index_ThermalConductivity, out thermalConductivity);

                    double lightTransmittance = double.NaN;
                    delimitedFileTable.TryConvert(i, index_LightTransmittance, out lightTransmittance);

                    double solarTransmittance = double.NaN;
                    delimitedFileTable.TryConvert(i, index_SolarTransmittance, out solarTransmittance);

                    double externalSolarReflectance = double.NaN;
                    delimitedFileTable.TryConvert(i, index_ExternalSolarReflectance, out externalSolarReflectance);

                    double internalSolarReflectance = double.NaN;
                    delimitedFileTable.TryConvert(i, index_InternalSolarReflectance, out internalSolarReflectance);

                    double externalLightReflectance = double.NaN;
                    delimitedFileTable.TryConvert(i, index_ExternalLightReflectance, out externalLightReflectance);

                    double internalLightReflectance = double.NaN;
                    delimitedFileTable.TryConvert(i, index_InternalLightReflectance, out internalLightReflectance);

                    double externalEmissivity = double.NaN;
                    delimitedFileTable.TryConvert(i, index_ExternalEmissivity, out externalEmissivity);

                    double internalEmissivity = double.NaN;
                    delimitedFileTable.TryConvert(i, index_InternalEmissivity, out internalEmissivity);

                    bool isBlind = false;
                    if (!delimitedFileTable.TryConvert(i, index_IsBlind, out isBlind))
                        isBlind = false;

                    material = TransparentMaterial(materialName, null, materialName, description, thermalConductivity, defaultThickness, vapourDiffusionFactor, solarTransmittance, lightTransmittance, externalSolarReflectance, internalSolarReflectance, externalLightReflectance, internalLightReflectance, externalEmissivity, internalEmissivity, isBlind);
                }
                else if (gasName.Contains(materialTypeString))
                {
                    //GasMaterial

                    double heatTransferCoefficient = double.NaN;
                    delimitedFileTable.TryConvert(i, index_HeatTransferCoefficient, out heatTransferCoefficient);

                    DefaultGasType defaultGasType = Query.DefaultGasType(materialName, description);
                    if (defaultGasType != DefaultGasType.Undefined)
                    {
                        material = GasMaterial(materialName, null, materialName, description, defaultThickness, vapourDiffusionFactor, heatTransferCoefficient, defaultGasType);
                    }
                    else
                    {
                        material = GasMaterial(materialName, null, materialName, description, defaultThickness, vapourDiffusionFactor, heatTransferCoefficient);
                    }
                }

                if (material == null)
                    continue;

                result.Add(material);
            }

            return result;
        }
    }
}