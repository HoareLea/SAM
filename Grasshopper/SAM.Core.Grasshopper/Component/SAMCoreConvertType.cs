using System;
using System.Windows.Forms;

using GH_IO.Serialization;
using Grasshopper.Kernel;

using SAM.Core.Grasshopper.Properties;

namespace SAM.Core.Grasshopper
{
    public class SAMCoreConvertType : GH_Component
    {
        
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("3ef0de2e-79dc-4e13-a23d-45ba2b41ce2f");

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        private ConvertType convertType = ConvertType.Undefined;

        /// <summary>
        /// Panel Type
        /// </summary>
        public SAMCoreConvertType()
          : base("SAMCore.ConvertType", "SAMCore.ConvertType",
              "Select Convert Type",
              "SAM", "Core")
        {

        }

        public override bool Write(GH_IWriter writer)
        {
            writer.SetInt32("ConvertType", (int)convertType);
            return base.Write(writer);
        }

        public override bool Read(GH_IReader reader)
        {
            int aIndex = -1;
            if (reader.TryGetInt32("ConvertType", ref aIndex))
                convertType = (ConvertType)aIndex;
            
            return base.Read(reader);
        }

        protected override void AppendAdditionalComponentMenuItems(ToolStripDropDown menu)
        {
            foreach (ConvertType panelType in Enum.GetValues(typeof(ConvertType)))
                //    GH_Component.Menu_AppendItem(menu, panelType.ToString(), Menu_PanelTypeChanged).Tag = panelType;
                //base.AppendAdditionalComponentMenuItems(menu);
                GH_Component.Menu_AppendItem(menu, panelType.ToString(), Menu_PanelTypeChanged, true, panelType == this.convertType).Tag = panelType;
        }

        private void Menu_PanelTypeChanged(object sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem item && item.Tag is ConvertType)
            {
                //Do something with panelType
                this.convertType = (ConvertType)item.Tag;
                ExpireSolution(true);
            }
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddGenericParameter("ConvertType", "ConvertType", "SAM Core ConvertType", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            dataAccess.SetData(0, convertType);
        }


    }
}