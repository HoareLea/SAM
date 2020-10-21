using Grasshopper.Kernel;
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
        }

        public override bool Obsolete
        {
            get
            {
                return Query.Obsolete(this);
            }
        }

        public override void AppendAdditionalMenuItems(ToolStripDropDown menu)
        {
            base.AppendAdditionalMenuItems(menu);
            Menu_AppendSeparator(menu);
            //Menu_AppendItem(menu, "Source code", OnSourceCodeClick, Properties.Resources.SAM_Small);

            Modify.AppendSourceCodeAdditionalMenuItem(this, menu);
            Modify.AppendNewComponentAdditionalMenuItem(this, menu);
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

        public override void AddedToDocument(GH_Document document)
        {
            base.AddedToDocument(document);
            Message = ComponentVersion;
        }
    }
}
