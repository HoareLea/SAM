using Grasshopper.Kernel;
using System.Diagnostics;
using System.Windows.Forms;

namespace SAM.Core.Grasshopper
{
    public abstract class GH_SAMComponent : GH_Component, IGH_SAMComponent
    {
        public GH_SAMComponent(string name, string nickname, string description, string category, string subCategory)
            : base(name, nickname, description, category, subCategory)
        {
            SetValue("SAM_SAMVersion", Core.Query.CurrentVersion());
            SetValue("SAM_ComponentVersion", LatestComponentVersion);

            Message = LatestComponentVersion;
        }

        public override bool Obsolete
        {

            get
            {
                return Query.Obsolete(this);
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

        public string ComponentVersion
        {
            get
            {
                return GetValue("SAM_ComponentVersion", null);
            }
        }

        public string SAMVersion
        {
            get
            {
               return GetValue("SAM_SAMVersion", null);
            }
        }

        public abstract string LatestComponentVersion { get;  }
    }
}
