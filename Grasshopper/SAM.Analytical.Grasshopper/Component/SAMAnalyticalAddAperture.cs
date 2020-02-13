using System;
using System.Collections.Generic;
using System.Linq;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;

using SAM.Analytical.Grasshopper.Properties;
using SAM.Geometry.Grasshopper;
using SAM.Geometry.Spatial;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalAddAperture : GH_Component
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("56dcc3a4-a1ad-4568-858d-fe9fb6ad32db");

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalAddAperture()
          : base("SAMAnalytical.AddAperture", "SAMAnalytical.AddAperture",
              "Add Aperture to SAM Analytical Panel",
              "SAM", "Analytical")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            inputParamManager.AddGenericParameter("_geometry", "geometry", "Geometry", GH_ParamAccess.item);
            inputParamManager.AddParameter(new GooPanelParam(), "_panel;", "_panel", "SAM Analytical Panel", GH_ParamAccess.list);
            inputParamManager.AddNumberParameter("maxDistance_", "maxDistance", "Maximal Distance", GH_ParamAccess.item, Geometry.Tolerance.MacroDistance);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddParameter(new Core.Grasshopper.GooSAMObjectParam<Aperture>(), "Apertures", "Apertures", "SAM Analytical Apertures", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {        
            object @object = null;
            if (!dataAccess.GetData(0, ref @object))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<ISAMGeometry3D> geometry3Ds = null;
            if (@object is IGH_GeometricGoo)
            {
                geometry3Ds = ((IGH_GeometricGoo)@object).ToSAM(true).Cast<ISAMGeometry3D>().ToList();
            }
            else if (@object is GH_ObjectWrapper)
            {
                GH_ObjectWrapper objectWrapper_Temp = ((GH_ObjectWrapper)@object);
                if (objectWrapper_Temp.Value is ISAMGeometry3D)
                    geometry3Ds = new List<ISAMGeometry3D>() { (ISAMGeometry3D)objectWrapper_Temp.Value };
            }
            else if (@object is IGH_Goo)
            {
                ISAMGeometry3D sAMGeometry3D = (@object as dynamic).Value as ISAMGeometry3D;
                if (sAMGeometry3D != null)
                    geometry3Ds = new List<ISAMGeometry3D>() { sAMGeometry3D };
            }

            List<IClosedPlanar3D> closedPlanar3Ds = Geometry.Spatial.Query.ClosedPlanar3Ds(geometry3Ds);
            if (closedPlanar3Ds == null || closedPlanar3Ds.Count() == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<Panel> panels = new List<Panel>();
            if (!dataAccess.GetDataList(1, panels))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            double maxDistance = Geometry.Tolerance.MacroDistance;
            dataAccess.GetData(2, ref maxDistance);

            List<Aperture> apertures = new List<Aperture>();
            foreach(IClosedPlanar3D closedPlanar3D in closedPlanar3Ds)
            {
                Aperture aperture = Modify.AddAperture(panels, closedPlanar3D, maxDistance);
                if (aperture != null)
                    apertures.Add(aperture);
            }

            if (apertures.Count == 1)
                dataAccess.SetData(0, new Core.Grasshopper.GooSAMObject<Aperture>(apertures[0]));
            else
                dataAccess.SetDataList(0, apertures.ConvertAll(x => new Core.Grasshopper.GooSAMObject<Aperture>(x)));
        }
    }
}