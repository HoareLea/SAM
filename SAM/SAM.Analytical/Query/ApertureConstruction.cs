using SAM.Core;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static ApertureConstruction ApertureConstruction(this ApertureType apertureType, bool external = true)
        {
            Setting setting = ActiveSetting.Setting;

            string parameterName = null;
            switch (apertureType)
            {
                case Analytical.ApertureType.Window:
                    if (external)
                        parameterName = ActiveSetting.Name.ApertureConstruction_ExternalWindows;
                    else
                        parameterName = ActiveSetting.Name.ApertureConstruction_InternalWindows;
                    break;
                case Analytical.ApertureType.Door:
                    if (external)
                        parameterName = ActiveSetting.Name.ApertureConstruction_ExternalDoors;
                    else
                        parameterName = ActiveSetting.Name.ApertureConstruction_InternalDoors;
                    break;
            }

            ApertureConstruction apertureConstruction;
            if (!setting.TryGetValue(parameterName, out apertureConstruction))
                return null;

            return apertureConstruction;
        }
    }
}
