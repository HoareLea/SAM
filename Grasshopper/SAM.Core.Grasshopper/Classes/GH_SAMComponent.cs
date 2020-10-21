using Grasshopper.Kernel;
using System;
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
            Menu_AppendItem(menu, "Test", OnTestClick, Properties.Resources.SAM_Small);
        }

        public virtual void OnSourceCodeClick(object sender = null, object e = null)
        {
            Process.Start("https://github.com/HoareLea/SAM");
        }

        public virtual void OnTestClick(object sender = null, object e = null)
        {
            PlaceLatest();
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

        public void PlaceLatest()
        {
            System.Drawing.RectangleF rectangle = Attributes.Bounds;
            System.Drawing.PointF location = rectangle.Location;

            GH_SAMComponent gH_SAMComponent = Activator.CreateInstance(GetType()) as GH_SAMComponent;

            bool result = global::Grasshopper.Instances.ActiveCanvas.InstantiateNewObject(gH_SAMComponent.ComponentGuid, location, false);
        }
    }
}
