using SAM.Core;

namespace SAM.Analytical
{
    public static partial class Query
    {
        /// <summary>
        /// Returns Default SAM Analytical ApertureConstructionLibrary
        /// </summary>
        /// <returns name="ConstructionLibrary"> Default SAM ApertureConstructionLibrary</returns>
        /// <search>Default SAM Analytical ApertureConstructionLibrary</search> 
        public static ApertureConstructionLibrary DefaultApertureConstructionLibrary()
        {
            ApertureConstructionLibrary result = null;
            if (ActiveSetting.Setting.TryGetValue(ActiveSetting.Name.Library_DefaultApertureConstructionLibrary, out result))
                return result;

            return null;
        }
    }
}