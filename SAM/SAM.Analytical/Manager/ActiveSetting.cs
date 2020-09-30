using SAM.Core;
using System.Reflection;

namespace SAM.Analytical
{
    public static partial class ActiveSetting
    {
        public static class Name
        {
            public const string ApertureConstruction_ExternalWindows = "ApertureConstruction_ExternalWindows";
            public const string ApertureConstruction_InternalWindows = "ApertureConstruction_InternalWindows";
            public const string ApertureConstruction_ExternalDoors = "ApertureConstruction_ExternalDoors";
            public const string ApertureConstruction_InternalDoors = "ApertureConstruction_InternalDoors";
            public const string ApertureConstruction_Skylight = "ApertureConstruction_Skylight";

            public const string ParameterName_PanelType = "ParameterName_PanelType";
            public const string ParameterName_Height = "ParameterName_Height";
            public const string ParameterName_Width = "ParameterName_Width";
            public const string ParameterName_Thickness = "ParameterName_Thickness";
            public const string ParameterName_Color = "ParameterName_Color";
            public const string ParameterName_Transparent = "ParameterName_Transparent";
            public const string ParameterName_InternalShadows = "ParameterName_InternalShadows";
            public const string ParameterName_Ground = "ParameterName_Ground";
            public const string ParameterName_Air = "ParameterName_Air";
            public const string ParameterName_FrameWidth = "ParameterName_FrameWidth";
            public const string ParameterName_NorthAngle = "ParameterName_NorthAngle";
            public const string ParameterName_SpaceName = "ParameterName_SpaceName";
            public const string ParameterName_FacingExternal = "ParameterName_FacingExternal";
            public const string ParameterName_FacingExternalGlazing = "ParameterName_FacingExternalGlazing";
            public const string ParameterName_Area = "ParameterName_Area";
            public const string ParameterName_Volume = "ParameterName_Volume";

            public const string ParameterName_VapourDiffusionFactor = "ParameterName_VapourDiffusionFactor";
            public const string ParameterName_ExternalSolarReflectance = "ParameterName_ExternalSolarReflectance";
            public const string ParameterName_InternalSolarReflectance = "ParameterName_InternalSolarReflectance";
            public const string ParameterName_ExternalLightReflectance = "ParameterName_ExternalLightReflectance";
            public const string ParameterName_InternalLightReflectance = "ParameterName_InternalLightReflectance";
            public const string ParameterName_ExternalEmissivity = "ParameterName_ExternalEmissivity";
            public const string ParameterName_InternalEmissivity = "ParameterName_InternalEmissivity";
            public const string ParameterName_IgnoreThermalTransmittanceCalculations = "ParameterName_IgnoreThermalTransmittanceCalculations";
            public const string ParameterName_IsBlind = "ParameterName_IsBlind";
            public const string ParameterName_DefaultThickness = "ParameterName_DefaultThickness";
            public const string ParameterName_HeatTransferCoefficient = "ParameterName_HeatTransferCoefficient";
            public const string ParameterName_MaterialType = "ParameterName_MaterialType";
            public const string ParameterName_MaterialName = "ParameterName_MaterialName";
            public const string ParameterName_MaterialDescription = "ParameterName_MaterialDescription";
            public const string ParameterName_ThermalConductivity = "ParameterName_ThermalConductivity";
            public const string ParameterName_SolarTransmittance = "ParameterName_SolarTransmittance";
            public const string ParameterName_LightTransmittance = "ParameterName_LightTransmittance";
            public const string ParameterName_SpecificHeatCapacity = "ParameterName_SpecificHeatCapacity";
            public const string ParameterName_Density = "ParameterName_Density";

            public const string FileName_DefaultMaterialLibrary = "FileName_DefaultMaterialLibrary";
            public const string Library_DefaultMaterialLibrary = "Library_DefaultMaterialLibrary";
            
            public const string FileName_DefaultConstructionLibrary = "FileName_DefaultConstructionLibrary";
            public const string Library_DefaultConstructionLibrary = "Library_DefaultConstructionLibrary";

            public const string FileName_DefaultGasMaterialLibrary = "FileName_DefaultGasMaterialLibrary";
            public const string Library_DefaultGasMaterialLibrary = "Library_DefaultGasMaterialLibrary";

            public const string FileName_DefaultApertureConstructionLibrary = "FileName_DefaultApertureConstructionLibrary";
            public const string Library_DefaultApertureConstructionLibrary = "Library_DefaultApertureConstructionLibrary";
        }

        private static Setting setting = Load();

        private static Setting Load()
        {
            Setting setting = ActiveManager.GetSetting(Assembly.GetExecutingAssembly());
            if (setting == null)
                setting = GetDefault();

            return setting;
        }

        public static Setting Setting
        {
            get
            {
                return setting;
            }
        }

        public static Setting GetDefault()
        {
            Setting result = new Setting(Assembly.GetExecutingAssembly());


            //Panels and Spaces Parameters
            result.Add(Name.ParameterName_PanelType, "SAM_BuildingElementType");
            result.Add(Name.ParameterName_Height, "SAM_BuildingElementHeight");
            result.Add(Name.ParameterName_Width, "SAM_BuildingElementWidth");
            result.Add(Name.ParameterName_Thickness, "SAM_BuildingElementThickness");
            result.Add(Name.ParameterName_Color, "SAM_BuildingElementColor");
            result.Add(Name.ParameterName_Transparent, "SAM_BuildingElementTransparent");
            result.Add(Name.ParameterName_InternalShadows, "SAM_BuildingElementInternalShadows");
            result.Add(Name.ParameterName_Ground, "SAM_BuildingElementGround");
            result.Add(Name.ParameterName_Air, "SAM_BuildingElementAir");
            result.Add(Name.ParameterName_FrameWidth, "SAM_BuildingElementFrameWidth");
            result.Add(Name.ParameterName_NorthAngle, "SAM_NorthAngle");
            result.Add(Name.ParameterName_SpaceName, "SAM_SpaceName");
            result.Add(Name.ParameterName_FacingExternal, "SAM_FacingExternal");
            result.Add(Name.ParameterName_FacingExternalGlazing, "SAM_FacingExternalGlazing");
            result.Add(Name.ParameterName_Area, "SAM_Area");
            result.Add(Name.ParameterName_Volume, "SAM_Volume");


            //Materials Parameters
            result.Add(Name.ParameterName_VapourDiffusionFactor, "SAM_Material_VapourDiffusionFactor");
            result.Add(Name.ParameterName_ExternalSolarReflectance, "SAM_Material_ExternalSolarReflectance");
            result.Add(Name.ParameterName_InternalSolarReflectance, "SAM_Material_InternalSolarReflectance");
            result.Add(Name.ParameterName_ExternalLightReflectance, "SAM_Material_ExternalLightReflectance");
            result.Add(Name.ParameterName_InternalLightReflectance, "SAM_Material_InternalLightReflectance");
            result.Add(Name.ParameterName_ExternalEmissivity, "SAM_Material_ExternalEmissivity");
            result.Add(Name.ParameterName_InternalEmissivity, "SAM_Material_InternalEmissivity");
            result.Add(Name.ParameterName_IgnoreThermalTransmittanceCalculations, "SAM_Material_IngnoreInUvalue");
            result.Add(Name.ParameterName_IsBlind, "SAM_Material_IsBlind");
            result.Add(Name.ParameterName_DefaultThickness, "SAM_Material_Width");
            result.Add(Name.ParameterName_HeatTransferCoefficient, "SAM_Material_ConvectionCoefficient");
            result.Add(Name.ParameterName_MaterialType, "SAM_Material_Type");
            result.Add(Name.ParameterName_MaterialName, "SAM_Material_Name");
            result.Add(Name.ParameterName_MaterialDescription, "SAM_Material_Description");
            result.Add(Name.ParameterName_ThermalConductivity, "SAM_Material_Conductivity");
            result.Add(Name.ParameterName_LightTransmittance, "SAM_Material_LightTransmittance");
            result.Add(Name.ParameterName_SolarTransmittance, "SAM_Material_SolarTransmittance");
            result.Add(Name.ParameterName_SpecificHeatCapacity, "SAM_Material_SpecificHeat");
            result.Add(Name.ParameterName_Density, "SAM_Material_Density");


            //Default Aperture Constructions
            result.Add(Name.ApertureConstruction_ExternalDoors, new ApertureConstruction(new System.Guid("5ad2e36d-6a2b-4cf1-af02-1ce62e7d2288"), "SIM_EXT_SLD", ApertureType.Door, Create.ConstructionLayers("C00_Hardwood_2100kg/m3_0.18W/mK", 0.050), Create.ConstructionLayers("C00_Hardwood_2100kg/m3_0.18W/mK", 0.050)));
            result.Add(Name.ApertureConstruction_ExternalWindows, new ApertureConstruction(new System.Guid("f70c4ec7-931b-47ed-a95b-1b3df1b9d885"), "SIM_EXT_GLZ", ApertureType.Window, Create.ConstructionLayers("_Glazing Inner Pane_6mm_g0.78_Lt0.89", 0.006, "Ag90UP_Argon__12mm_1.403W/m2K", 0.012, "_Side Lit Glazing Outer Pane_6mm_g0.41_Lt0.9", 0.006), Create.ConstructionLayers("C00_Frame Notional building_7800kg/m3_0.176W/mK", 0.050)));
            result.Add(Name.ApertureConstruction_InternalDoors, new ApertureConstruction(new System.Guid("1dcdad32-63ec-4a01-945d-39548be20491"), "SIM_INT_SLD", ApertureType.Door, Create.ConstructionLayers("C03_Wood_500kg/m3_0.13W/mK", 0.030), Create.ConstructionLayers("C03_Wood_500kg/m3_0.13W/mK", 0.030)));
            result.Add(Name.ApertureConstruction_InternalWindows, new ApertureConstruction(new System.Guid("3e43ecb2-638b-4d8b-9046-ba3d8455cd3f"), "SIM_INT_GLZ", ApertureType.Window, Create.ConstructionLayers("C_Clear glass_10mm_g0.74_Lt0.87", 0.010), Create.ConstructionLayers("C00_Frame Notional building_7800kg/m3_0.176W/mK", 0.030)));
            result.Add(Name.ApertureConstruction_Skylight, new ApertureConstruction(new System.Guid("6f6dc032-6fa5-43fa-bfef-de5937e95599"), "SIM_EXT_GLZ_SKY DF01", ApertureType.Window, Create.ConstructionLayers("_Glazing Inner Pane_6mm_g0.78_Lt0.89", 0.006, "Ag90UP_Argon__12mm_1.403W/m2K", 0.012, "_Roof Lit Glazing Outer Pane_6mm_g0.58_Lt0.67", 0.006), Create.ConstructionLayers("C00_Frame Notional building_7800kg/m3_0.176W/mK", 0.050)));

            //Blind to be added to Skylight, External Glazing for SIM_EXT_GLZ_Blinds added =>"Ar90UP_Air__16mm_1.614W/m2K", 0.016, "B00_Soltis-ferrari-92-1010_g0.25_Lt0.1", 0.001
            //result.Add(Name.ApertureConstruction_ExternalWindows, new ApertureConstruction(new System.Guid("f70c4ec7-931b-47ed-a95b-1b3df1b9d885"), "SIM_EXT_GLZ_Blinds", ApertureType.Window, Create.ConstructionLayers("_Glazing Inner Pane_g0.78_Lt0.89", 0.006, "Ag90UP_Argon__12mm_1.403W/m2K", 0.012, "_Side Lit Glazing Outer Pane_g0.41_Lt0.9", 0.006, "Ar90UP_Air__16mm_1.614W/m2K", 0.016, "B00_Soltis-ferrari-92-1010_g0.25_Lt0.1", 0.001), Create.ConstructionLayers("C00_Frame Notional building_7800kg/m3_0.176W/mK", 0.050)));
            //Vehicle Door for SIM_EXT_SLD_Vehicle
            //result.Add(Name.ApertureConstruction_ExternalDoors, new ApertureConstruction(new System.Guid("5ad2e36d-6a2b-4cf1-af02-1ce62e7d2288"), "SIM_EXT_SLD_Vehicle", ApertureType.Door, Create.ConstructionLayers("C00_Steel_7800kg/m3_60W/mK", 0.0006, "C00_EPS_15kg/m3_0.04W/mK", 0.020,"C00_Steel_7800kg/m3_60W/mK", 0.0006), Create.ConstructionLayers("C00_Steel_7800kg/m3_60W/mK", 0.0006, "C00_EPS_15kg/m3_0.04W/mK", 0.020, "C00_Steel_7800kg/m3_60W/mK", 0.0006)));
            //SIM_EXT_SLD Adiabatic
            //result.Add(Name.Construction_WallExternal, new Construction(new System.Guid("8f424c58-6570-4b9b-b753-e7584b7b4494"), "SIM_EXT_SLD", Create.ConstructionLayers("F00_WhitePaint_0.001kg/m3_999W/mK", 0.0001, "C01_Plasterboard_700kg/m3_0.21W/mK", 0.0125, "Ar90Up_Air__50mm_1.25W/m2K", 0.05, "C01_Plasterboard_700kg/m3_0.21W/mK", 0.0125, "C00_Cement particleboard_1200kg/m3_0.23W/mK", 0.012, "I01_Mineral Wool_20kg/m3_0.025W/mK" , 0.08, "Ar90Up_Air__50mm_1.25W/m2K", 0.05, "C00_Rainscreen_7800kg/m3_50W/mK", 0.003)));

            //File Names
            result.Add(Name.FileName_DefaultMaterialLibrary, "SAM_MaterialLibrary.JSON");
            result.Add(Name.FileName_DefaultConstructionLibrary, "SAM_ConstructionLibrary.JSON");
            result.Add(Name.FileName_DefaultGasMaterialLibrary, "SAM_GasMaterialLibrary.JSON");
            result.Add(Name.FileName_DefaultApertureConstructionLibrary, "SAM_ApertureConstructionLibrary.JSON");

            string path = null;

            path = Query.DefaultConstructionLibraryPath(result);
            if (System.IO.File.Exists(path))
                result.Add(Name.Library_DefaultConstructionLibrary, Core.Create.IJSAMObject<ConstructionLibrary>(System.IO.File.ReadAllText(path)));

            path = Query.DefaultMaterialLibraryPath(result);
            if (System.IO.File.Exists(path))
                result.Add(Name.Library_DefaultMaterialLibrary, Core.Create.IJSAMObject<MaterialLibrary>(System.IO.File.ReadAllText(path)));

            path = Query.DefaultGasMaterialLibraryPath(result);
            if (System.IO.File.Exists(path))
                result.Add(Name.Library_DefaultGasMaterialLibrary, Core.Create.IJSAMObject<MaterialLibrary>(System.IO.File.ReadAllText(path)));

            path = Query.DefaultApertureConstructionLibraryPath(result);
            if (System.IO.File.Exists(path))
                result.Add(Name.Library_DefaultApertureConstructionLibrary, Core.Create.IJSAMObject<ApertureConstructionLibrary>(System.IO.File.ReadAllText(path)));

            return result;
        }
    }
}