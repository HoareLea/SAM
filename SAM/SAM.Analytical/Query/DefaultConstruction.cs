using System.Linq;

namespace SAM.Analytical
{
    public static partial class Query
    {
        /// <summary>
        /// Returns Default SAM Analytical Construction for givet PanelType
        /// </summary>
        /// <param name="panelType">SAM Analytical PanelType</param>
        /// <returns name="construction"> Default SAM Analytical Construction</returns>
        /// <search>Default SAM Analytical Construction, PanelType</search> 
        public static Construction DefaultConstruction(this PanelType panelType)
        {
            return DefaultConstructionLibrary()?.GetConstructions(panelType)?.FirstOrDefault();
        }
    }
}