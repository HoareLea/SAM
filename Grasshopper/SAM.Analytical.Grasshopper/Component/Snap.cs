using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;

using SAM.Analytical.Grasshopper.Properties;

namespace SAM.Analytical.Grasshopper
{
    public class Snap : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public Snap()
          : base("Snap", "Snp",
              "Snap Panels",
              "SAM", "Analytical")
        {

        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            inputParamManager.AddGenericParameter("Panels", "Pnl", "SAM Analytical Panels", GH_ParamAccess.list);
            inputParamManager.AddNumberParameter("Offset", "Offs", "Snap offset", GH_ParamAccess.item, 0.2);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddGenericParameter("Panels", "Pnl", "SAM Analytical Panels", GH_ParamAccess.list);
            outputParamManager.AddGenericParameter("Points", "Pts", "Snap points", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            List<GH_ObjectWrapper> objectWrapperList = new List<GH_ObjectWrapper>();

            if (!dataAccess.GetDataList(0, objectWrapperList) || objectWrapperList == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            GH_ObjectWrapper objectWrapper = null;

            if (!dataAccess.GetData(1, ref objectWrapper) || objectWrapper.Value == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            GH_Number number = objectWrapper.Value as GH_Number;
            if(number == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            double offset = number.Value;

            List<Panel> panelList = new List<Panel>();
            foreach (GH_ObjectWrapper objectWrapper_Temp in objectWrapperList)
            {
                Panel panel = objectWrapper_Temp.Value as Panel;
                if (panel == null)
                    continue;

                panelList.Add(panel);
            }

            List<Geometry.Spatial.Point3D> point3DList = Geometry.Spatial.Point3D.Generate(new Geometry.Spatial.BoundingBox3D(panelList.ConvertAll(x => x.GetBoundingBox(offset))), offset);

            panelList = panelList.ConvertAll(x => new Panel(x));
            panelList.ForEach(x => x.Snap(point3DList, offset));

            dataAccess.SetDataList(0, panelList);
            dataAccess.SetDataList(1, point3DList);

            //AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Cannot split segments");
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
            get { return new Guid("6a7ac292-7b25-4211-878c-5012ea4e6dff"); }
        }
    }
}