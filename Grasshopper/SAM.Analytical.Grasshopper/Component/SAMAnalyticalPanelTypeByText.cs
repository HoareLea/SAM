using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using SAM.Geometry.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalPanelTypeByText : GH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("aa788133-30d2-4622-94c2-342b80d438f9");

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
        public SAMAnalyticalPanelTypeByText()
          : base("SAMAnalytical.PanelTypeByText", "SAMAnalytical.PanelTypeByText",
              "Get PanelType By Text",
              "SAM", "Analytical")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            inputParamManager.AddTextParameter("_text", "_text", "Text", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddTextParameter("PanelType", "PanelType", "SAM Analytical PanelType", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">
        /// The DA object is used to retrieve from inputs and store in outputs.
        /// </param>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            string text = null;
            if(!dataAccess.GetData(0, ref text) || string.IsNullOrEmpty(text))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            PanelType panelType = Analytical.Query.PanelType(text, true);
            if(panelType == PanelType.Undefined)
            {
                text = text.ToLower().Trim();
                if(text.Contains("roof"))
                {
                    panelType = PanelType.Roof;
                }
                else if(text.Contains("floor"))
                {
                    panelType = PanelType.Floor;
                    if (text.Contains("ext"))
                        panelType = PanelType.FloorExposed;
                    else if (text.Contains("int"))
                        panelType = PanelType.FloorInternal;
                    else if (text.Contains("grd"))
                        panelType = PanelType.SlabOnGrade;
                }
                else if(text.Contains("shd"))
                {
                    panelType = PanelType.Shade;
                }
                else if(text.Contains("sol"))
                {
                    panelType = PanelType.SolarPanel;

                }
                else
                {
                    panelType = PanelType.Wall;
                    if (text.Contains("ext"))
                        panelType = PanelType.WallExternal;
                    else if(text.Contains("int"))
                        panelType = PanelType.WallInternal;
                }
            }

            dataAccess.SetData(0, panelType.ToString());
        }
    }
}