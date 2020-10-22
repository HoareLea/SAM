using Grasshopper;
using Grasshopper.Kernel;
using System.Windows.Forms;

namespace SAM.Core.Grasshopper
{
    public static partial class Modify
    {
        public static ToolStripMenuItem AppendNewComponentAdditionalMenuItem(this IGH_SAMComponent gH_SAMComponent, ToolStripDropDown menu)
        {
            GH_Component gH_Component = gH_SAMComponent as GH_Component;
            if (gH_Component == null)
                return null;

            if (gH_Component.Obsolete || !Query.HasComponentVersion(gH_SAMComponent))
            {
                string text = "Get the latest component";

                string latestComponentVersion = gH_SAMComponent.LatestComponentVersion;
                if (!string.IsNullOrWhiteSpace(latestComponentVersion))
                    text = string.Format("{0} ({1})", text, latestComponentVersion); 
                
                ToolStripMenuItem toolStripMenuItem = GH_Component.Menu_AppendItem(menu, text, OnGetNewComponentClick, Properties.Resources.SAM_Small);
                if (toolStripMenuItem != null)
                    toolStripMenuItem.Tag = gH_Component.InstanceGuid;

                return toolStripMenuItem;
            }
                
            return null;
        }

        private static void OnGetNewComponentClick(object sender = null, object e = null)
        {
            ToolStripMenuItem toolStripMenuItem = sender as ToolStripMenuItem;
            if (toolStripMenuItem == null || !(toolStripMenuItem.Tag is System.Guid))
                return;

            GH_Component gH_Component = Instances.ActiveCanvas.Document.FindComponent((System.Guid)toolStripMenuItem.Tag) as GH_Component;
            if (gH_Component == null)
                return;

            Query.Copy(gH_Component);
        }
    }
}