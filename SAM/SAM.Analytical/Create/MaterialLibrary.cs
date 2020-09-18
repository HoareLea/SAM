using SAM.Core;

namespace SAM.Analytical
{
    public static partial class Create
    {
        public static MaterialLibrary MaterialLibrary(string path, int headerIndex = 1, int headerCount = 7)
        {
            if (string.IsNullOrWhiteSpace(path) || !System.IO.File.Exists(path))
                return null;

            DelimitedFileTable delimitedFileTable = null;

            using (DelimitedFileReader delimitedFileReader = new DelimitedFileReader(DelimitedFileType.Csv, path))
            {
                delimitedFileTable = new DelimitedFileTable(delimitedFileReader, headerCount);
            }

            if (delimitedFileTable == null)
                return null;


            MaterialLibrary result = new MaterialLibrary(System.IO.Path.GetFileNameWithoutExtension(path));

            int count = delimitedFileTable.Count;
            if (count == 0)
                return result;

            int index_MaterialType = delimitedFileTable.GetIndex(Query.ParameterName_MaterialType());
            if (index_MaterialType == -1)
                return result;

            int index_MaterialName = delimitedFileTable.GetIndex(Query.ParameterName_MaterialName());
            if (index_MaterialName == -1)
                return result;

            int index_MaterialDescription = delimitedFileTable.GetIndex(Query.ParameterName_MaterialDescription());
            int index_DefaultThickness = delimitedFileTable.GetIndex(Query.ParameterName_DefaultThickness());
            int index_ThermalConductivity = delimitedFileTable.GetIndex(Query.ParameterName_ThermalConductivity());
            int index_SpecificHeatCapacity = delimitedFileTable.GetIndex(Query.ParameterName_SpecificHeatCapacity());
            int index_Density = delimitedFileTable.GetIndex(Query.ParameterName_Density());
            int index_VapourDiffusionFactor = delimitedFileTable.GetIndex(Query.ParameterName_VapourDiffusionFactor());
            int index_ExternalSolarReflectance = delimitedFileTable.GetIndex(Query.ParameterName_ExternalSolarReflectance());
            int index_InternalSolarReflectance = delimitedFileTable.GetIndex(Query.ParameterName_InternalSolarReflectance());
            int index_ExternalLightReflectance = delimitedFileTable.GetIndex(Query.ParameterName_ExternalLightReflectance());
            int index_InternalLightReflectance = delimitedFileTable.GetIndex(Query.ParameterName_InternalLightReflectance());
            int index_ExternalEmissivity = delimitedFileTable.GetIndex(Query.ParameterName_ExternalEmissivity());
            int index_InternalEmissivity = delimitedFileTable.GetIndex(Query.ParameterName_InternalEmissivity());
            int index_IgnoreThermalTransmittanceCalculations = delimitedFileTable.GetIndex(Query.ParameterName_IgnoreThermalTransmittanceCalculations());
            int index_SolarTransmittance = delimitedFileTable.GetIndex(Query.ParameterName_SolarTransmittance());
            int index_LightTransmittance = delimitedFileTable.GetIndex(Query.ParameterName_LightTransmittance());
            int index_IsBlind = delimitedFileTable.GetIndex(Query.ParameterName_IsBlind());
            int index_HeatTransferCoefficient = delimitedFileTable.GetIndex(Query.ParameterName_HeatTransferCoefficient());

            string opaqueName = typeof(OpaqueMaterial).Name.ToUpper();
            string transparentName = typeof(TransparentMaterial).Name.ToUpper();
            string gasName = typeof(GasMaterial).Name.ToUpper();

            for (int i = 0; i < count; i++)
            {
                string materialTypeString = null;
                if (!delimitedFileTable.TryConvert(i, index_MaterialType, out materialTypeString) || string.IsNullOrWhiteSpace(materialTypeString))
                    continue;

                materialTypeString = materialTypeString.Trim().ToUpper();

                string name = null;
                if (!delimitedFileTable.TryConvert(i, index_MaterialName, out name) || string.IsNullOrWhiteSpace(name))
                    continue;

                string description = null;
                delimitedFileTable.TryConvert(i, index_MaterialDescription, out description);

                double defaultThickness = double.NaN;
                delimitedFileTable.TryConvert(i, index_DefaultThickness, out defaultThickness);

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

                    material = OpaqueMaterial(name, null, name, description, thermalConductivity, specificHeatCapacity, density, defaultThickness, vapourDiffusionFactor, externalSolarReflectance, internalSolarReflectance, externalLightReflectance, internalLightReflectance, externalEmissivity, internalEmissivity, ignoreThermalTransmittanceCalculations);

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

                    material = TransparentMaterial(name, null, name, description, thermalConductivity, defaultThickness, vapourDiffusionFactor, solarTransmittance, lightTransmittance, externalSolarReflectance, internalSolarReflectance, externalLightReflectance, internalLightReflectance, externalEmissivity, internalEmissivity, isBlind);
                }
                else if (gasName.Contains(materialTypeString))
                {
                    //GasMaterial

                    double heatTransferCoefficient = double.NaN;
                    delimitedFileTable.TryConvert(i, index_HeatTransferCoefficient, out heatTransferCoefficient);

                    material = GasMaterial(name, null, name, description, defaultThickness, vapourDiffusionFactor, heatTransferCoefficient);
                }

                if (material == null)
                    continue;

                result.Add(material);
            }

            return result;
        }
    }
}