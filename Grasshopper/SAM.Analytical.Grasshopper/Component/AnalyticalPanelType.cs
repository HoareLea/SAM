using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;

using SAM.Analytical.Grasshopper.Properties;

namespace SAM.Analytical.Grasshopper
{
    public class AnalyticalPanelType : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public AnalyticalPanelType()
          : base("AnalyticalSnapByPoints", "Snp",
              "Snap Panels",
              "SAM", "Analytical")
        {

        }

        public override bool AppendMenuItems(ToolStripDropDown menu)
        {
            base.AppendMenuItems(menu);

            ToolStripDropDown toolStripDropDown = new ToolStripDropDown();

            foreach (PanelType panelType in Enum.GetValues(typeof(PanelType)))
                toolStripDropDown.Items.Add(panelType.ToString());

            Control control = new Control();
            global::Grasshopper.Kernel.GH_DocumentObject.Menu_AppendCustomItem(toolStripDropDown, control);

            return true;
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            //inputParamManager.AddGenericParameter("Analytical", "Anl", "SAM Analytical Object", GH_ParamAccess.item);
            //inputParamManager.AddGenericParameter("Points", "Pnt", "Points", GH_ParamAccess.list);
            //inputParamManager.AddNumberParameter("maxDistance", "mDis", "Max Distance to snap points", GH_ParamAccess.item, double.NaN);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddGenericParameter("PanelType", "PanelType", "Analytical PanelType", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {

        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return Resources.HL_Logo24;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("e61f2f2e-f655-430a-9dfd-507929edef58"); }
        }
    }
}