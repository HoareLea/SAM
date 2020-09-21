using SAM.Core;
using System.Reflection;

namespace SAM.Analytical
{
    public static partial class ActiveSetting
    {
        public static class Name
        {
            public const string Construction_Undefined = "Construction_Undefined";
            public const string Construction_WallInternal = "Construction_WallInternal";
            public const string Construction_WallExternal = "Construction_WallExternal";
            public const string Construction_SlabOnGrade = "Construction_SlabOnGrade";
            public const string Construction_FloorInternal = "Construction_FloorInternal";
            public const string Construction_Roof = "Construction_Roof";
            public const string Construction_Ceiling = "Construction_Ceiling";
            public const string Construction_CurtainWall = "Construction_CurtainWall";
            public const string Construction_Floor = "Construction_Floor";
            public const string Construction_FloorExposed = "Construction_FloorExposed";
            public const string Construction_FloorRaised = "Construction_FloorRaised";
            public const string Construction_Shade = "Construction_Shade";
            public const string Construction_SolarPanel = "Construction_SolarPanel";
            public const string Construction_UndergroundCeiling = "Construction_UndergroundCeiling";
            public const string Construction_UndergroundSlab = "Construction_UndergroundSlab";
            public const string Construction_UndergroundWall = "Construction_UndergroundWall";
            public const string Construction_Wall = "Construction_Wall";

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

            public const string GasMaterial_Air = "GasMaterial_Air";
            public const string GasMaterial_Argon = "GasMaterial_Argon";
            public const string GasMaterial_Krypton = "GasMaterial_Krypton";
            public const string GasMaterial_Xenon = "GasMaterial_Xenon";
            public const string GasMaterial_SulfurHexaFluoride = "GasMaterial_SulfurHexaFluoride";
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


            //Default Constructions
            result.Add(Name.Construction_Ceiling, new Construction(new System.Guid("6ff47aad-ec17-4806-8e81-e84b10d5756a"), "Generic", Create.ConstructionLayers("C01_Plasterboard_700kg/m3_0.21W/mK", 0.0125, "Ar90Dn_Air__50mm_0.5W/m2K", 0.05, "C00_Concrete Reinforced_2300kg/m3_2.3W/mK", 0.1, "C00_Screed_1800kg/m3_1.15W/mK", 0.05, "Ar90Dn_Air__50mm_0.5W/m2K", 0.05, "C00_Chipboard Flooring_500kg/m3_0.13W/mK", 0.02, "C00_Carpet and Rubber pad_288.332kg/m3_0.059W/mK", 0.01)));
            result.Add(Name.Construction_CurtainWall, new Construction(new System.Guid("9ab5d49f-4db4-4930-8293-3945d430e572"), "SIM_EXT_GLZ", Create.ConstructionLayers("_Glazing Inner Pane_g0.78_Lt0.89", 0.006, "Ar0UP_Air__12mm_3.168W/m2K", 0.12, "_Side Lit Glazing Outer Pane_g0.41_Lt0.9", 0.006)));
            result.Add(Name.Construction_Floor, new Construction(new System.Guid("6b5cbfab-bc87-4d2c-98e0-d940c75295fa"), "SIM_EXT_GRD_FLR FLR01", Create.ConstructionLayers("C00_Gypsym Board_800.923kg/m3_0.161W/mK", 0.0127, "I00_Glass Fibre Insulation_11.998kg/m3_0.038W/mK", 0.13018, "C00_Concrete Block_1842.12kg/m3_1.315W/mK", 0.23, "C00_Notional/Reference Soil_1250kg/m3_1.5W/mK", 1)));
            result.Add(Name.Construction_FloorExposed, new Construction(new System.Guid("18da6398-75b3-4e39-8505-cf26e1f7f875"), "SIM_EXT_SLD_FLR Exposed", Create.ConstructionLayers("C00_Chipboard Flooring_500kg/m3_0.13W/mK", 0.02, "Ar90Dn_Air__50mm_0.5W/m2K", 0.05, "C00_Concrete Reinforced_2300kg/m3_2.3W/mK", 0.1, "I00_Mineral Wool_700kg/m3_0.025W/mK", 0.0098)));
            result.Add(Name.Construction_FloorInternal, new Construction(new System.Guid("d5521200-8376-49b2-a9ed-ad914f380022"), "SIM_INT_SLD_FLR FLR02", Create.ConstructionLayers("C00_Carpet and Rubber pad_288.332kg/m3_0.059W/mK", 0.01,"C00_Chipboard Flooring_500kg/m3_0.13W/mK", 0.02, "Ar90Dn_Air__50mm_0.5W/m2K", 0.05, "C00_Screed_1800kg/m3_1.15W/mK", 0.05, "C00_Concrete Reinforced_2300kg/m3_2.3W/mK", 0.1, "Ar90Dn_Air__50mm_0.5W/m2K", 0.05, "C01_Plasterboard_700kg/m3_0.21W/mK", 0.0125)));
            result.Add(Name.Construction_FloorRaised, new Construction(new System.Guid("3290e9b5-323d-40b2-9be4-312894681119"), "SIM_INT_SLD_FLR FLR02", Create.ConstructionLayers("C00_Chipboard Flooring_500kg/m3_0.13W/mK", 0.02, "Ar90Dn_Air__50mm_0.5W/m2K", 0.05, "C00_Concrete Reinforced_2300kg/m3_2.3W/mK", 0.1, "I00_Mineral Wool_700kg/m3_0.025W/mK", 0.0098)));
            result.Add(Name.Construction_Roof, new Construction(new System.Guid("6ebe9f3b-b20d-4583-a5db-3f17d8106016"), "SIM_EXT_SLD_Roof DA01", Create.ConstructionLayers("C01_Plasterboard_700kg/m3_0.21W/mK", 0.0125, "Ar90Up_Air__50mm_1.95W/m2K", 0.05, "C00_Concrete_2400kg/m3_2W/mK", 0.1, "C00_Membrane_1100kg/m3_1W/mK",0.001, "I02_Mineral Wool_40kg/m3_0.03W/mK", 0.154)));
            result.Add(Name.Construction_Shade, new Construction(new System.Guid("4acfae60-42c4-4b00-ba12-bc0329987b51"), "SIM_EXT_SHD", Create.ConstructionLayers("C00_Mild Steel_7800kg/m3_50W/mK", 0.006)));
            result.Add(Name.Construction_SlabOnGrade, new Construction(new System.Guid("6892f46e-396e-46ef-bf00-42a91e2f3d49"), "SIM_EXT_GRD_FLR FLR01", Create.ConstructionLayers("C00_Carpet and Rubber pad_288.332kg/m3_0.059W/mK", 0.0127, "C00_Concrete_2400kg/m3_2.362W/mK", 0.2, "I03_Mineral Wool_43.25kg/m3_0.035W/mK",0.14, "C00_Notional/Reference Soil_1250kg/m3_1.5W/mK", 1)));
            result.Add(Name.Construction_SolarPanel, new Construction(new System.Guid("28b83556-a292-4c0a-b5dc-2d6fe85eb728"), "SIM_EXT_SOL_Roof", Create.ConstructionLayers("C00_SHW or PV Panel_480kg/m3_0.01W/mK", 0.006)));
            result.Add(Name.Construction_Undefined, new Construction(new System.Guid("cac8e3ad-50bd-4323-b0c8-2f3128c591c7"), "SIM_EXT_SHD_Roof SD01", Create.ConstructionLayers("C00_Mild Steel_7800kg/m3_50W/mK", 0.006)));
            result.Add(Name.Construction_UndergroundCeiling, new Construction(new System.Guid("595f988a-89cc-47fb-b3d1-b8df1f76002d"), "SIM_INT_SLD_FLR Parking", Create.ConstructionLayers("C00_Plasterboard_900kg/m3_0.21W/mK", 0.0095, "I00_Glass Fibre Insulation_11.998kg/m3_0.038W/mK", 0.16, "C00_Concrete Block_1842.12kg/m3_1.315W/mK", 0.23, "C00_Notional/Reference Soil_1250kg/m3_1.5W/mK", 1)));
            result.Add(Name.Construction_UndergroundSlab, new Construction(new System.Guid("b4a6da31-169c-4781-a1cb-39e9900f7d35"), "SIM_EXT_GRD_FLR FLR01", Create.ConstructionLayers("C00_Carpet and Rubber pad_288.332kg/m3_0.059W/mK", 0.0127, "C00_Concrete_2400kg/m3_2.362W/mK", 0.2, "I03_Mineral Wool_43.25kg/m3_0.035W/mK", 0.14, "C00_Notional/Reference Soil_1250kg/m3_1.5W/mK", 1)));
            result.Add(Name.Construction_UndergroundWall, new Construction(new System.Guid("99972a36-6fb2-4565-9337-3b163d056c9c"), "SIM_EXT_GRD", Create.ConstructionLayers("C00_Gypsym Board_800.923kg/m3_0.161W/mK", 0.0127, "I00_Glass Fibre Insulation_11.998kg / m3_0.038W / mK",0.13018, "C00_Concrete Block_1842.12kg/m3_1.315W/mK",0.23, "C00_Notional/Reference Soil_1250kg/m3_1.5W/mK", 1)));
            result.Add(Name.Construction_Wall, new Construction(new System.Guid("5c3039ff-c4af-4953-bbf0-5c84bdd8044c"), "SIM_EXT_SLD", Create.ConstructionLayers("0_WhitePaint_0.001kg/m3_999W/mK", 0.0001, "C01_Plasterboard_700kg/m3_0.21W/mK", 0.0125, "Ar90Up_Air__50mm_1.25W/m2K", 0.05, "C01_Plasterboard_700kg/m3_0.21W/mK", 0.0125, "C00_Cement particleboard_1200kg/m3_0.23W/mK", 0.012, "I01_Mineral Wool_20kg/m3_0.025W/mK", 0.08, "Ar90Up_Air__50mm_1.25W/m2K", 0.05, "C00_Rainscreen_7800kg/m3_50W/mK", 0.003)));
            result.Add(Name.Construction_WallExternal, new Construction(new System.Guid("8f424c58-6570-4b9b-b753-e7584b7b4494"), "SIM_EXT_SLD", Create.ConstructionLayers("0_WhitePaint_0.001kg/m3_999W/mK", 0.0001, "C01_Plasterboard_700kg/m3_0.21W/mK", 0.0125, "Ar90Up_Air__50mm_1.25W/m2K", 0.05, "C01_Plasterboard_700kg/m3_0.21W/mK", 0.0125, "C00_Cement particleboard_1200kg/m3_0.23W/mK", 0.012, "I01_Mineral Wool_20kg/m3_0.025W/mK" , 0.08, "Ar90Up_Air__50mm_1.25W/m2K", 0.05, "C00_Rainscreen_7800kg/m3_50W/mK", 0.003)));
            result.Add(Name.Construction_WallInternal, new Construction(new System.Guid("625d9ed6-64fa-4877-bb56-b84ba118c900"), "SIM_INT_SLD_Partition", Create.ConstructionLayers("0_WhitePaint_0.001kg/m3_999W/mK", 0.0001, "C01_Plasterboard_700kg/m3_0.21W/mK", 0.0125, "Ar90Up_Air__50mm_1.25W/m2K", 0.05, "C01_Plasterboard_700kg/m3_0.21W/mK", 0.0125)));

            //Default Aperture Constructions
            result.Add(Name.ApertureConstruction_ExternalDoors, new ApertureConstruction(new System.Guid("5ad2e36d-6a2b-4cf1-af02-1ce62e7d2288"), "SIM_EXT_SLD", ApertureType.Door, Create.ConstructionLayers("C00_Hardwood_2100kg/m3_0.18W/mK", 0.050), Create.ConstructionLayers("C00_Hardwood_2100kg/m3_0.18W/mK", 0.050)));
            result.Add(Name.ApertureConstruction_ExternalWindows, new ApertureConstruction(new System.Guid("f70c4ec7-931b-47ed-a95b-1b3df1b9d885"), "SIM_EXT_GLZ", ApertureType.Window, Create.ConstructionLayers("_Glazing Inner Pane_g0.78_Lt0.89", 0.006, "Ag90UP_Argon__12mm_1.403W/m2K", 0.012, "_Side Lit Glazing Outer Pane_g0.41_Lt0.9", 0.006), Create.ConstructionLayers("C00_Frame Notional building_7800kg/m3_0.176W/mK", 0.050)));
            result.Add(Name.ApertureConstruction_InternalDoors, new ApertureConstruction(new System.Guid("1dcdad32-63ec-4a01-945d-39548be20491"), "SIM_INT_SLD", ApertureType.Door, Create.ConstructionLayers("C03_Wood_500kg/m3_0.13W/mK", 0.030), Create.ConstructionLayers("C03_Wood_500kg/m3_0.13W/mK", 0.030)));
            result.Add(Name.ApertureConstruction_InternalWindows, new ApertureConstruction(new System.Guid("3e43ecb2-638b-4d8b-9046-ba3d8455cd3f"), "SIM_INT_GLZ", ApertureType.Window, Create.ConstructionLayers("_Clear glass_g0.74_Lt0", 0.010), Create.ConstructionLayers("C00_Frame Notional building_7800kg/m3_0.176W/mK", 0.030)));
            result.Add(Name.ApertureConstruction_Skylight, new ApertureConstruction(new System.Guid("6f6dc032-6fa5-43fa-bfef-de5937e95599"), "SIM_EXT_GLZ_SKY DF01", ApertureType.Window, Create.ConstructionLayers("_Glazing Inner Pane_g0.78_Lt0.89", 0.006, "Ag90UP_Argon__12mm_1.403W/m2K", 0.012, "_Roof Lit Glazing Outer Pane_g0.58_Lt0.67", 0.006), Create.ConstructionLayers("C00_Frame Notional building_7800kg/m3_0.176W/mK", 0.050)));

            //Blind to be added to Skylight, External Glazing for SIM_EXT_GLZ_Blinds added =>"Ar90UP_Air__16mm_1.614W/m2K", 0.016, "B00_Soltis-ferrari-92-1010_g0.25_Lt0.1", 0.001
            //result.Add(Name.ApertureConstruction_ExternalWindows, new ApertureConstruction(new System.Guid("f70c4ec7-931b-47ed-a95b-1b3df1b9d885"), "SIM_EXT_GLZ_Blinds", ApertureType.Window, Create.ConstructionLayers("_Glazing Inner Pane_g0.78_Lt0.89", 0.006, "Ag90UP_Argon__12mm_1.403W/m2K", 0.012, "_Side Lit Glazing Outer Pane_g0.41_Lt0.9", 0.006, "Ar90UP_Air__16mm_1.614W/m2K", 0.016, "B00_Soltis-ferrari-92-1010_g0.25_Lt0.1", 0.001), Create.ConstructionLayers("C00_Frame Notional building_7800kg/m3_0.176W/mK", 0.050)));
            //Vehicle Door for SIM_EXT_SLD_Vehicle
            //result.Add(Name.ApertureConstruction_ExternalDoors, new ApertureConstruction(new System.Guid("5ad2e36d-6a2b-4cf1-af02-1ce62e7d2288"), "SIM_EXT_SLD_Vehicle", ApertureType.Door, Create.ConstructionLayers("C00_Steel_7800kg/m3_60W/mK", 0.0006, "C00_EPS_15kg/m3_0.04W/mK", 0.020,"C00_Steel_7800kg/m3_60W/mK", 0.0006), Create.ConstructionLayers("C00_Steel_7800kg/m3_60W/mK", 0.0006, "C00_EPS_15kg/m3_0.04W/mK", 0.020, "C00_Steel_7800kg/m3_60W/mK", 0.0006)));

            //Default Gas Materials
            result.Add(Name.GasMaterial_Air, new GasMaterial(new System.Guid("b701be87-3012-450d-a6c5-582dcff33e61"), "Default Dry Air Gas", "Dry Air", "Dry Air Material with properties at 10°C [EN 673:1997 Table 1 - Gas Properties]", 2.496e-2, 1.232, 1008, 1.761e-5));
            result.Add(Name.GasMaterial_Argon, new GasMaterial(new System.Guid("da58562e-38b1-4ab2-86f9-08832591e029"), "Default Argon Gas", "Argon", "Argon Material with properties at 10°C [EN 673:1997 Table 1 - Gas Properties]", 0.01684, 1.699, 519, 2.164e-5));
            result.Add(Name.GasMaterial_Krypton, new GasMaterial(new System.Guid("448c7b25-8696-455a-af09-5c0f305c441a"), "Default Krypton Gas", "Krypton", "Krypton Material with properties at 10°C [EN 673:1997 Table 1 - Gas Properties]", 0.9e-2, 3.56, 0.245e3, 2.4e-5));
            result.Add(Name.GasMaterial_Xenon, new GasMaterial(new System.Guid("78dcd953-2c44-441d-a677-4903c4fe08cd"), "Default Xenon Gas", "Xenon", "Xenon Material with properties at 10°C [EN 673:1997 Table 1 - Gas Properties]", 0.0529, 5.689, 161, 2.226e-5));
            result.Add(Name.GasMaterial_SulfurHexaFluoride, new GasMaterial(new System.Guid("78dcd953-2c44-441d-a677-4903c4fe08cd"), "Default Sulfur HexaFluoride Gas", "Sulfur HexaFluoride (SF6) [EN 673:1997 Table 1 - Gas Properties]", "SulfurHexa Fluoride Material with properties at 10°C", 0.01275, 6.36, 614, 1.459e-5));

            return result;
        }
    }
}