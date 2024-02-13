using System.ComponentModel;

namespace SAM.Analytical
{
    /// <summary>
    /// TM59 Space Application
    /// </summary>
    public enum TM52BuildingCategory
    {
        [Description("Undefined")] Undefined,
        [Description("Category I")] CategoryI,
        [Description("Category II")] CategoryII,
        [Description("Category III")] CategoryIII,
        [Description("Category IV")] CategoryIV,
    }
}
