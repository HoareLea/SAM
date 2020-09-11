using SAM.Core;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static GasMaterial GasMaterial(this DefaultGasType defaultGasType)
        {
            if (defaultGasType == DefaultGasType.Undefined)
                return null;

            Setting setting = ActiveSetting.Setting;
            if (setting == null)
                return null;

            string parameterName = null;

            switch(defaultGasType)
            {
                case DefaultGasType.Air:
                    parameterName = ActiveSetting.Name.GasMaterial_Air;
                    break;

                case DefaultGasType.Argon:
                    parameterName = ActiveSetting.Name.GasMaterial_Argon;
                    break;

                case DefaultGasType.Krypton:
                    parameterName = ActiveSetting.Name.GasMaterial_Krypton;
                    break;

                case DefaultGasType.Xenon:
                    parameterName = ActiveSetting.Name.GasMaterial_Xenon;
                    break;
            }

            if (string.IsNullOrWhiteSpace(parameterName))
                return null;

            GasMaterial gasMaterial;
            if (!setting.TryGetValue(parameterName, out gasMaterial))
                return null;

            return gasMaterial;

        }
    }
}