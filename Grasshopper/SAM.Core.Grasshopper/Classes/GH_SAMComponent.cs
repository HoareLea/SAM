using Grasshopper.Kernel;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Forms;

namespace SAM.Core.Grasshopper
{
    public abstract class GH_SAMComponent : GH_Component
    {
        public GH_SAMComponent(string name, string nickname, string description, string category, string subCategory)
            : base(name, nickname, description, category, subCategory)
        {

            string directory = Core.Query.ExecutingAssemblyDirectory();
            if(!string.IsNullOrWhiteSpace(directory))
            {
                string path = System.IO.Path.Combine(directory, "version");
                if(!string.IsNullOrWhiteSpace(path) && System.IO.File.Exists(path))
                {
                    string[] lines = System.IO.File.ReadAllLines(path);
                    if(lines != null && lines.Length != 0)
                    {
                        foreach(string line in lines)
                        {
                            if (!string.IsNullOrWhiteSpace(line))
                            {
                                Message = line.Trim();
                                break;
                            }
                        }

                    }
                }
            }
        }
        
        public virtual void AppendAdditionalMenuItems(ToolStripDropDown menu)
        {
            base.AppendAdditionalMenuItems(menu);
            Menu_AppendSeparator(menu);
            Menu_AppendItem(menu, "Source code", OnSourceCodeClick, Properties.Resources.SAM_Small);
        }

        public virtual void OnSourceCodeClick(object sender = null, object e = null)
        {
            Process.Start("https://github.com/HoareLea/SAM");
        }
    }
}
