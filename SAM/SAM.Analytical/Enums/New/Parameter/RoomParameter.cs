using System.ComponentModel;
using SAM.Core.Attributes;

namespace SAM.Analytical
{
    [AssociatedTypes(typeof(Room)), Description("Room Parameter")]
    public enum RoomParameter
    {
        [ParameterProperties("Volume", "Volume [m3]"), DoubleParameterValue(0)] Volume,
        [ParameterProperties("Area", "Area [m2]"), DoubleParameterValue(0)] Area,
        [ParameterProperties("Level Name", "Level Name"), ParameterValue(Core.ParameterType.String)] LevelName,
    }
}