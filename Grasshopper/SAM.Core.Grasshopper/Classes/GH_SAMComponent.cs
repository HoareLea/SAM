using Grasshopper.Kernel;
using System.Diagnostics;
using System.Windows.Forms;

namespace SAM.Core.Grasshopper
{
    public abstract class GH_SAMComponent : GH_Component
    {
        public GH_SAMComponent(string name, string nickname, string description, string category, string subCategory)
            : base(name, nickname, description, category, subCategory)
        {

        }
        
        public override void AppendAdditionalMenuItems(ToolStripDropDown menu)
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
