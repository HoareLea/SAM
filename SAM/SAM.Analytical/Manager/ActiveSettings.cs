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

            //Default Constructions
            result.Add(Name.Construction_Ceiling, new Construction(new System.Guid("6ff47aad-ec17-4806-8e81-e84b10d5756a"), "Generic"));
            result.Add(Name.Construction_CurtainWall, new Construction(new System.Guid("9ab5d49f-4db4-4930-8293-3945d430e572"), "SIM_EXT_GLZ"));
            result.Add(Name.Construction_Floor, new Construction(new System.Guid("6b5cbfab-bc87-4d2c-98e0-d940c75295fa"), "SIM_EXT_GRD_FLR FLR01"));
            result.Add(Name.Construction_FloorExposed, new Construction(new System.Guid("18da6398-75b3-4e39-8505-cf26e1f7f875"), "SIM_EXT_SLD_FLR Exposed"));
            result.Add(Name.Construction_FloorInternal, new Construction(new System.Guid("d5521200-8376-49b2-a9ed-ad914f380022"), "SIM_INT_SLD_FLR FLR02"));
            result.Add(Name.Construction_FloorRaised, new Construction(new System.Guid("3290e9b5-323d-40b2-9be4-312894681119"), "SIM_INT_SLD_FLR FLR02"));
            result.Add(Name.Construction_Roof, new Construction(new System.Guid("6ebe9f3b-b20d-4583-a5db-3f17d8106016"), "SIM_EXT_SLD_Roof DA01"));
            result.Add(Name.Construction_Shade, new Construction(new System.Guid("4acfae60-42c4-4b00-ba12-bc0329987b51"), "SIM_EXT_SHD_Roof SD01"));
            result.Add(Name.Construction_SlabOnGrade, new Construction(new System.Guid("6892f46e-396e-46ef-bf00-42a91e2f3d49"), "SIM_EXT_GRD_FLR FLR01"));
            result.Add(Name.Construction_SolarPanel, new Construction(new System.Guid("28b83556-a292-4c0a-b5dc-2d6fe85eb728"), "SIM_EXT_SOL_Roof"));
            result.Add(Name.Construction_Undefined, new Construction(new System.Guid("cac8e3ad-50bd-4323-b0c8-2f3128c591c7"), "SIM_EXT_SHD_Roof SD01"));
            result.Add(Name.Construction_UndergroundCeiling, new Construction(new System.Guid("595f988a-89cc-47fb-b3d1-b8df1f76002d"), "SIM_INT_SLD_FLR Parking"));
            result.Add(Name.Construction_UndergroundSlab, new Construction(new System.Guid("b4a6da31-169c-4781-a1cb-39e9900f7d35"), "SIM_EXT_GRD_FLR FLR01"));
            result.Add(Name.Construction_UndergroundWall, new Construction(new System.Guid("99972a36-6fb2-4565-9337-3b163d056c9c"), "SIM_EXT_GRD"));
            result.Add(Name.Construction_Wall, new Construction(new System.Guid("5c3039ff-c4af-4953-bbf0-5c84bdd8044c"), "SIM_EXT_SLD"));
            result.Add(Name.Construction_WallExternal, new Construction(new System.Guid("8f424c58-6570-4b9b-b753-e7584b7b4494"), "SIM_EXT_SLD"));
            result.Add(Name.Construction_WallInternal, new Construction(new System.Guid("625d9ed6-64fa-4877-bb56-b84ba118c900"), "SIM_INT_SLD_Partition"));

            //Default Aperture Constructions
            result.Add(Name.ApertureConstruction_ExternalDoors, new ApertureConstruction(new System.Guid("5ad2e36d-6a2b-4cf1-af02-1ce62e7d2288"), "SIM_EXT_SLD", ApertureType.Door));
            result.Add(Name.ApertureConstruction_ExternalWindows, new ApertureConstruction(new System.Guid("f70c4ec7-931b-47ed-a95b-1b3df1b9d885"), "SIM_EXT_GLZ", ApertureType.Window));
            result.Add(Name.ApertureConstruction_InternalDoors, new ApertureConstruction(new System.Guid("1dcdad32-63ec-4a01-945d-39548be20491"), "SIM_INT_SLD", ApertureType.Door));
            result.Add(Name.ApertureConstruction_InternalWindows, new ApertureConstruction(new System.Guid("3e43ecb2-638b-4d8b-9046-ba3d8455cd3f"), "SIM_INT_GLZ", ApertureType.Window));
            result.Add(Name.ApertureConstruction_Skylight, new ApertureConstruction(new System.Guid("6f6dc032-6fa5-43fa-bfef-de5937e95599"), "SIM_EXT_GLZ_SKY DF01", ApertureType.Window));

            return result;
        }
    }
}