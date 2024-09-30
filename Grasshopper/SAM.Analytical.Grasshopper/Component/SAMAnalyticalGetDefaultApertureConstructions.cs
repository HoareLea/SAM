using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalGetDefaultApertureConstructions : GH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("83e42fe9-79ff-4011-a3b0-3a7f69dabad5");

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
        public SAMAnalyticalGetDefaultApertureConstructions()
          : base("SAMAnalytical.GetDefaultApertureConstructions", "SAMAnalytical.GetDefaultApertureConstructions",
              "Get Default SAM ApertureConstructions",
              "SAM", "Analytical01")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            int index = -1;

            index = inputParamManager.AddTextParameter("_apertureTypes_", "_apertureTypes_", "SAM Analytical ApertureTypes", GH_ParamAccess.list);
            inputParamManager[index].Optional = true;

            index = inputParamManager.AddTextParameter("_panelTypes_", "_panelTypes_", "SAM Analytical PanelTypes", GH_ParamAccess.list);
            inputParamManager[index].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddParameter(new GooApertureConstructionParam(), "ApertureConstructions", "ApertureConstructions", "SAM Analytical Aperture Constructions", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">
        /// The DA object is used to retrieve from inputs and store in outputs.
        /// </param>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            List<string> values = null;

            values = new List<string>();
            dataAccess.GetDataList(0, values);

            List<ApertureType> apertureTypes = null;
            if (values != null && values.Count > 0)
            {
                apertureTypes = new List<ApertureType>();

                foreach (string value in values)
                {
                    ApertureType apertureType;
                    if (Enum.TryParse(value, out apertureType))
                        apertureTypes.Add(apertureType);
                }
            }
            else
            {
                apertureTypes = new List<ApertureType>(Enum.GetValues(typeof(ApertureType)).Cast<ApertureType>());
            }

            apertureTypes?.RemoveAll(x => x == ApertureType.Undefined);

            if (apertureTypes == null || apertureTypes.Count == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            values = new List<string>();
            dataAccess.GetDataList(1, values);

            List<PanelType> panelTypes = null;
            if (values != null && values.Count > 0)
            {
                panelTypes = new List<PanelType>();

                foreach (string value in values)
                {
                    PanelType panelType;
                    if (Enum.TryParse(value, out panelType))
                        panelTypes.Add(panelType);
                }
            }
            else
            {
                panelTypes = new List<PanelType>(Enum.GetValues(typeof(PanelType)).Cast<PanelType>());
            }

            panelTypes?.RemoveAll(x => x == PanelType.Undefined);

            if (panelTypes == null || panelTypes.Count == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<ApertureConstruction> apertureConstructions = new List<ApertureConstruction>();

            List<GooApertureConstruction> gooApertureConstructions = new List<GooApertureConstruction>();
            foreach (ApertureType apertureType in apertureTypes)
                foreach (PanelType panelType in panelTypes)
                {
                    ApertureConstruction apertureConstruction = Analytical.Query.DefaultApertureConstruction(panelType, apertureType);
                    if (apertureConstruction == null)
                        continue; 

                    if (apertureConstructions.Find(x => x.Guid.Equals(apertureConstruction.Guid)) == null)
                        apertureConstructions.Add(apertureConstruction);
                }

            dataAccess.SetDataList(0, apertureConstructions?.ConvertAll(x => new GooApertureConstruction(x)));
        }
    }
}