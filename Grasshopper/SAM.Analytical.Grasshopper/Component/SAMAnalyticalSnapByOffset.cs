using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;

using SAM.Analytical.Grasshopper.Properties;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalSnapByOffset : GH_Component
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("6a7ac292-7b25-4211-878c-5012ea4e6dff");

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalSnapByOffset()
          : base("SAMAnalytical.SnapByOffset", "SAMAnalytical.SnapByOffset",
              "Snap Panels by Offset from Panels default = 0.2m",
              "SAM", "Analytical")
        {

        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            inputParamManager.AddParameter(new Core.Grasshopper.GooSAMObjectParam<Core.SAMObject>(), "_SAMAnalytical", "_SAMAnalytical", "SAM Analytical Object", GH_ParamAccess.list);
            inputParamManager.AddNumberParameter("_offset_", "Offs", "Snap offset", GH_ParamAccess.item, 0.2);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddParameter(new Core.Grasshopper.GooSAMObjectParam<Panel>(), "_SAMAnalytical", "_SAMAnalytical", "SAM Analytical Object", GH_ParamAccess.list);
            outputParamManager.AddParameter(new Geometry.Grasshopper.GooSAMGeometryParam(), "Point3Ds", "Point3Ds", "Snap Point3Ds", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            List<Core.SAMObject> sAMObjects = null;
            if (!dataAccess.GetDataList(0, sAMObjects) || sAMObjects == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            double offset = double.NaN;
            if (!dataAccess.GetData(1, ref offset) || double.IsNaN(offset))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<Panel> panelList = new List<Panel>();
            foreach (Core.SAMObject sAMObject in sAMObjects)
            {
                Panel panel = sAMObject as Panel;

                if (panel == null)
                    continue;

                panelList.Add(panel);
            }

            List<Geometry.Spatial.Point3D> point3DList = Geometry.Spatial.Point3D.Generate(new Geometry.Spatial.BoundingBox3D(panelList.ConvertAll(x => x.GetBoundingBox(offset))), offset);

            panelList = panelList.ConvertAll(x => new Panel(x));
            panelList.ForEach(x => x.Snap(point3DList, offset));

            dataAccess.SetDataList(0, panelList.ConvertAll(x => new GooPanel(x)));
            dataAccess.SetDataList(1, point3DList.ConvertAll(x => new Geometry.Grasshopper.GooSAMGeometry(x)));
        }
    }
}