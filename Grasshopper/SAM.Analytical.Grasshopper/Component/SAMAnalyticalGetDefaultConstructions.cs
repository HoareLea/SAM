using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Render.Fields;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using SAM.Geometry.Grasshopper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalGetDefaultConstructions : GH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("ef93bf07-c910-4bbf-b76c-53b028640ac7");

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalGetDefaultConstructions()
          : base("SAMAnalytical.GetDefaultConstructions", "SAMAnalytical.GetDefaultConstructions",
              "Get Default SAM Constructions",
              "SAM", "Analytical")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            int index = inputParamManager.AddTextParameter("_panelTypes_", "_panelTypes_", "SAM PanelTypes", GH_ParamAccess.list);
            inputParamManager[index].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddParameter(new GooConstructionParam(), "Constructions", "Construction", "SAM Geometry Spaces", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">
        /// The DA object is used to retrieve from inputs and store in outputs.
        /// </param>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            List<string> panelTypeStrings = new List<string>();
            dataAccess.GetDataList(0, panelTypeStrings);

            List<PanelType> panelTypes = null;
            if(panelTypeStrings != null && panelTypeStrings.Count > 0)
            {
                panelTypes = new List<PanelType>();

                foreach(string panelTypeString in panelTypeStrings)
                {
                    PanelType panelType;
                    if (Enum.TryParse(panelTypeString, out panelType))
                        panelTypes.Add(panelType);
                }
            }
            else
            {
                panelTypes = new List<PanelType>(Enum.GetValues(typeof(PanelType)).Cast<PanelType>());
            }

            panelTypes?.RemoveAll(x => x == PanelType.Undefined || x == PanelType.Air);

            if(panelTypes == null || panelTypes.Count == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            dataAccess.SetDataList(0, panelTypes.ConvertAll(x => new GooConstruction(Analytical.Query.Construction(x))));
        }
    }
}