using System.ComponentModel;

namespace SAM.Analytical
{
    [Description("Analytical Panel Type.")]
    public enum PanelType
    {
        [Description("Undefined")] Undefined,
        [Description("Ceiling")] Ceiling,
        [Description("Curtain Wall")] CurtainWall,
        [Description("Floor")] Floor,
        [Description("Exposed Floor")] FloorExposed,
        [Description("Internal Floor")] FloorInternal,
        [Description("Raised Floor")] FloorRaised,
        [Description("Roof")] Roof,
        [Description("Shade")] Shade,
        [Description("Slab on Grade")] SlabOnGrade,
        [Description("Solar/PV panel")] SolarPanel,
        [Description("Underground Ceiling")] UndergroundCeiling,
        [Description("Underground Slab")] UndergroundSlab,
        [Description("Underground Wall")] UndergroundWall,
        [Description("Wall")] Wall,
        [Description("External Wall")] WallExternal,
        [Description("Internal Wall")] WallInternal,
        [Description("No Type")] Air
    }
}