using Grasshopper;
using Grasshopper.Kernel;
using System.Diagnostics;
using System.Net;
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

            string name = assembly.GetName().Name;
            string[] names = name?.Split('.');
            if(names != null && names.Length > 2)
            {
                string project = "SAM";
                if(names.Length > 3)
                    project = string.Format("{0}_{1}", project, names[3]);

                string link_Temp = string.Format(@"{0}/{1}/blob/master/Grasshopper/{2}/Component/{3}.cs", link, project, name, gH_SAMComponent.GetType().Name);

                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(link_Temp);
                req.AllowAutoRedirect = false;

                HttpWebResponse res = (HttpWebResponse)req.GetResponse();
                if (res.StatusCode == HttpStatusCode.OK)
                    link = link_Temp;
            }

            Process.Start(link);
        }
    }
}