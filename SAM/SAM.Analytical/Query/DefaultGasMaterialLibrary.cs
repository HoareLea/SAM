using SAM.Core;

namespace SAM.Analytical
{
    public static partial class Query
    {
        /// <summary>
        /// Returns Default SAM Analytical MaterialLibrary
        /// </summary>
        /// <returns name="MaterialLibrary"> Default SAM Analytical MaterialLibrary</returns>
        /// <search>Default SAM Analytical MaterialLibrary</search> 
        public static MaterialLibrary DefaultGasMaterialLibrary()
        {
            MaterialLibrary result = null;
            if (ActiveSetting.Setting.TryGetValue(ActiveSetting.Name.Library_DefaultGasMaterialLibrary, out result))
                return result;

            return null;
        }
    }
}