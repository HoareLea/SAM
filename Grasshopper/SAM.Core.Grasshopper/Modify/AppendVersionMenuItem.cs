using Grasshopper.Kernel;
using System.Windows.Forms;

namespace SAM.Core.Grasshopper
{
    public static partial class Modify
    {
        public static ToolStripMenuItem AppendVersionMenuItem(this IGH_SAMComponent gH_SAMComponent, ToolStripDropDown menu)
        {
            GH_Component gH_Component = gH_SAMComponent as GH_Component;
            if (gH_Component == null)
            {
                return null;
            }

            ToolStripMenuItem result = null;

            string latestComponentVersion = gH_SAMComponent.LatestComponentVersion;
            if (!string.IsNullOrWhiteSpace(latestComponentVersion))
            {
                result = GH_DocumentObject.Menu_AppendItem(menu, latestComponentVersion, OnVersionComponentClick, Properties.Resources.SAM_Small);
            }

            return result;
        }

        private static void OnVersionComponentClick(object sender = null, object e = null)
        {

        }
    }
}