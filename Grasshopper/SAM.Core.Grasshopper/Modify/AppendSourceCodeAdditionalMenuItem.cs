using Grasshopper;
using Grasshopper.Kernel;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Forms;

namespace SAM.Core.Grasshopper
{
    public static partial class Modify
    {
        public static ToolStripMenuItem AppendSourceCodeAdditionalMenuItem(this IGH_SAMComponent gH_SAMComponent, ToolStripDropDown menu)
        {
            GH_Component gH_Component = gH_SAMComponent as GH_Component;
            if (gH_Component == null)
                return null;

            ToolStripMenuItem toolStripMenuItem = GH_Component.Menu_AppendItem(menu, "Go to source code...", OnSourceCodeClick, Properties.Resources.SAM_Small);
            if (toolStripMenuItem != null)
                toolStripMenuItem.Tag = gH_Component.InstanceGuid;

            return toolStripMenuItem;
        }

        private static void OnSourceCodeClick(object sender = null, object e = null)
        {
            ToolStripMenuItem toolStripMenuItem = sender as ToolStripMenuItem;
            if (toolStripMenuItem == null || !(toolStripMenuItem.Tag is System.Guid))
                return;

            GH_SAMComponent gH_SAMComponent = Instances.ActiveCanvas.Document.FindComponent((System.Guid)toolStripMenuItem.Tag) as GH_SAMComponent;
            if (gH_SAMComponent == null)
                return;

            Assembly assembly =  gH_SAMComponent.GetType().Assembly;

            string link = @"https://github.com/HoareLea";

            Process.Start(link);
        }
    }
}