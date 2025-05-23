﻿using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalSetPanelType : GH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("04a7a588-a3e2-4ad7-868e-d432140a5b05");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.0";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
                protected override System.Drawing.Bitmap Icon => Core.Convert.ToBitmap(Resources.SAM_Small);

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalSetPanelType()
          : base("SAMAnalytical.SetPanelType", "SAMAnalytical.SetPanelType",
              "Sets Panel Type for Analytical Panel",
              "SAM", "Analytical03")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            inputParamManager.AddParameter(new GooPanelParam(), "_panel", "_panel", "SAM Analytical Panel", GH_ParamAccess.item);
            inputParamManager.AddGenericParameter("_panelType", "_panelType", "SAM Analytical Panel Type", GH_ParamAccess.item);
            inputParamManager.AddBooleanParameter("_setDefaultConstruction_", "_setDefaultConstruction_", "Set Default Construction", GH_ParamAccess.item, false);
            inputParamManager.AddBooleanParameter("_setDefaultApertureConstruction_", "_setDefaultApertureConstruction_", "Set Default Aperture Construction", GH_ParamAccess.item, false);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddParameter(new GooPanelParam(), "Panel", "Panel", "SAM Analytical Panel", GH_ParamAccess.item);
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
            if (!dataAccess.GetData(0, ref panel) || panel == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            GH_ObjectWrapper objectWrapper = null;
            if (!dataAccess.GetData(1, ref objectWrapper))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            bool setDefaultConstruction = false;
            if (!dataAccess.GetData(2, ref setDefaultConstruction))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            bool setDefaultApertureConstruction = false;
            if (!dataAccess.GetData(3, ref setDefaultApertureConstruction))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            object value = objectWrapper.Value;

            if (value is GH_String)
                value = ((GH_String)value).Value;

            PanelType panelType = Analytical.Query.PanelType(value);

            Panel panel_New = Create.Panel(panel, panelType);
            if(setDefaultConstruction)
            {
                Construction construction = Analytical.Query.DefaultConstruction(panelType);
                panel_New = Create.Panel(panel_New, construction);
            }

            if(setDefaultApertureConstruction)
            {
                List<Aperture> apertures = panel_New.Apertures;
                if(apertures != null && apertures.Count != 0)
                {
                    foreach(Aperture aperture in apertures)
                    {
                        ApertureConstruction apertureConstruction = Analytical.Query.DefaultApertureConstruction(panel_New.PanelType, aperture.ApertureType);
                        if (apertureConstruction == null)
                            continue;

                        panel_New.RemoveAperture(aperture.Guid);

                        Aperture aperture_New = new Aperture(aperture, apertureConstruction);
                        panel_New.AddAperture(aperture_New);
                    }
                }
            }

            dataAccess.SetData(0, new GooPanel(panel_New));
        }
    }
}