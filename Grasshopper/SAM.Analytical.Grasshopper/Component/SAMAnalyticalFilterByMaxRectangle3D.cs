using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalFilterByMaxRectangle3D : GH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("e87bbf9e-b61f-4dde-9232-06ae944494bd");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.0";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalFilterByMaxRectangle3D()
          : base("SAMAnalytical.FilterByMaxRectangle3D", "SAMAnalytical.FilterByMaxRectangle3D",
              "Filter Analytical Objects By Maxilal Rectangle3D dimension",
              "SAM", "Analytical")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            int index;

            index = inputParamManager.AddParameter(new GooPanelParam(), "_panels", "_panels", "SAM Analytical Panels", GH_ParamAccess.list);
            inputParamManager[index].DataMapping = GH_DataMapping.Flatten;

            index = inputParamManager.AddNumberParameter("_panelMinDimension", "_panelMinDimension", "Minimal dimesion for Panel Rectangle3D", GH_ParamAccess.item, 0);
            index = inputParamManager.AddNumberParameter("_apertureMinDimension", "_apertureMinDimension", "Minimal dimesion for Aperture Rectangle3D", GH_ParamAccess.item, 0);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddParameter(new GooPanelParam(), "in", "in", "SAM Analytical Panels", GH_ParamAccess.list);
            outputParamManager.AddGenericParameter("out", "out", "out", GH_ParamAccess.list);
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

            double panelMinDimension = 0;
            if (!dataAccess.GetData(1, ref panelMinDimension))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            double apertureMinDimension = 0;
            if (!dataAccess.GetData(2, ref apertureMinDimension))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<Panel> panels_In = new List<Panel>();
            List<object> objects_Out = new List<object>();
            
            for(int i=0; i < panels.Count; i++)
            {              
                Panel panel = Create.Panel(panels[i]);
                if (panel == null)
                {
                    continue;
                }

                Geometry.Spatial.Rectangle3D rectangle3D = Geometry.Spatial.Query.MaxRectangle3D(panel);
                if(rectangle3D == null || rectangle3D.Width < panelMinDimension || rectangle3D.Height < panelMinDimension)
                {
                    objects_Out.Add(panel);
                }
                else
                {
                    panels_In.Add(panel);
                }

                List<Aperture> apertures = panel.Apertures;
                if(apertures == null || apertures.Count == 0)
                {
                    continue;
                }

                foreach(Aperture aperture in apertures)
                {
                    rectangle3D = Geometry.Spatial.Query.MaxRectangle3D(aperture);
                    if (rectangle3D == null || rectangle3D.Width < apertureMinDimension || rectangle3D.Height < apertureMinDimension)
                    {
                        objects_Out.Add(aperture);
                        panel.RemoveAperture(aperture.Guid);
                    }
                }
            }
            dataAccess.SetDataList(0, panels_In?.ConvertAll(x => new GooPanel(x)));
            dataAccess.SetDataList(1, objects_Out);
        }
    }
}