using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core;
using SAM.Core.Grasshopper;
using SAM.Geometry.Grasshopper;
using SAM.Geometry.Planar;
using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalSplitPanel : GH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("f31eaa6a-e9a8-4e85-9513-36f21ec32c1b");

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalSplitPanel()
          : base("SAMAnalytical.SplitPanel", "SAMAnalytical.SplitPanel",
              "Split SAM Analytical Panel by Elevation or Plane, *aperture will be splited as well",
              "SAM", "Analytical")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            inputParamManager.AddParameter(new GooPanelParam(), "_panel", "_panel", "SAM Analytical Panel", GH_ParamAccess.item);
            inputParamManager.AddGenericParameter("_elevations", "_elevations", "Elevations or Planes", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddParameter(new GooPanelParam(), "Panels", "Panels", "Panels", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">
        /// The DA object is used to retrieve from inputs and store in outputs.
        /// </param>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            Panel panel = null;
            if(!dataAccess.GetData(0, ref panel))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<GH_ObjectWrapper> objectWrappers = new List<GH_ObjectWrapper>();
            if(!dataAccess.GetDataList(1, objectWrappers) || objectWrappers == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<Plane> planes = new List<Plane>();
            foreach(GH_ObjectWrapper objectWrapper in objectWrappers)
            {
                object @object = objectWrapper.Value;

                if (@object is IGH_Goo)
                {
                    try
                    {
                        @object = (@object as dynamic).Value;
                    }
                    catch (Exception exception)
                    {
                        @object = null;
                    }
                }

                if (@object == null)
                    continue;


                Plane plane = null;

                if (Core.Query.IsNumeric(@object))
                    plane = Geometry.Spatial.Create.Plane((double)@object);
                else if (@object is Plane)
                    plane = (Plane)@object;
                else if (@object is GH_Plane)
                    plane = ((GH_Plane)@object).ToSAM();

                if (plane != null)
                    planes.Add(plane);
            }

            List<Panel> result = Analytical.Query.Cut(panel, planes);

            if (result == null || result.Count == 0)
                result = new List<Panel>() { panel.Clone() };

            dataAccess.SetDataList(0, result?.ConvertAll(x => new GooPanel(x)));
        }
    }
}