using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalSnapByOffset : GH_SAMComponent
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
            inputParamManager.AddParameter(new GooPanelParam(), "_panels", "_panels", "SAM Analytical Panels", GH_ParamAccess.list);
            inputParamManager.AddNumberParameter("_offset_", "Offs", "Snap offset", GH_ParamAccess.item, 0.2);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddParameter(new GooPanelParam(), "Panels", "Panels", "SAM Analytical Panels", GH_ParamAccess.list);
            outputParamManager.AddParameter(new Geometry.Grasshopper.GooSAMGeometryParam(), "Point3Ds", "Point3Ds", "Snap Point3Ds", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">
        /// The DA object is used to retrieve from inputs and store in outputs.
        /// </param>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            List<Panel> panels = new List<Panel>();
            if (!dataAccess.GetDataList(0, panels))
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

            List<Geometry.Spatial.Point3D> point3DList = Geometry.Spatial.Point3D.Generate(new Geometry.Spatial.BoundingBox3D(panels.ConvertAll(x => x.GetBoundingBox(offset))), offset);

            panels = panels.ConvertAll(x => new Panel(x));
            panels.ForEach(x => x.Snap(point3DList, offset));

            dataAccess.SetDataList(0, panels.ConvertAll(x => new GooPanel(x)));
            dataAccess.SetDataList(1, point3DList.ConvertAll(x => new Geometry.Grasshopper.GooSAMGeometry(x)));
        }
    }
}