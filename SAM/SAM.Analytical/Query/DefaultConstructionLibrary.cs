using SAM.Core;

namespace SAM.Analytical
{
    public static partial class Query
    {
        /// <summary>
        /// Returns Default SAM Analytical ConstructionLibrary
        /// </summary>
        /// <returns name="ConstructionLibrary"> Default SAM ConstructionLibrary</returns>
        /// <search>Default SAM Analytical ConstructionLibrary</search> 
        public static ConstructionLibrary DefaultConstructionLibrary()
        {
            ConstructionLibrary result = null;
            if (ActiveSetting.Setting.TryGetValue(ActiveSetting.Name.Library_DefaultConstructionLibrary, out result))
                return result;

            return null;
        }
    }
}