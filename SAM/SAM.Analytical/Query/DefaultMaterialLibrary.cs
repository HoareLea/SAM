using SAM.Core;

namespace SAM.Analytical
{
    public static partial class Query
    {
        /// <summary>
        /// Returns Default SAM Analytical MaterialLibrary
        /// </summary>
        /// <returns name="MaterialLibrary"> Default SAM MaterialLibrary</returns>
        /// <search>Default SAM Analytical Construction, PanelType</search> 
        public static Core.MaterialLibrary DefaultMaterialLibrary()
        {
            string path = DefaultMaterialLibraryPath();
            if (string.IsNullOrWhiteSpace(path) || !System.IO.File.Exists(path))
                return null;

            Core.MaterialLibrary result = Core.Create.MaterialLibrary(path);

            return result;
        }
    }
}