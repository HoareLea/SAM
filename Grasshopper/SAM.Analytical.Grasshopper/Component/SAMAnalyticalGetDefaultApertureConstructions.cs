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
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalGetDefaultApertureConstructions()
          : base("SAMAnalytical.GetDefaultApertureConstructions", "SAMAnalytical.GetDefaultApertureConstructions",
              "Get Default SAM ApertureConstructions",
              "SAM", "Analytical")
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

            index = inputParamManager.AddBooleanParameter("_external_", "_external_", "External Aperture", GH_ParamAccess.item);
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
            List<string> panelTypeStrings = new List<string>();
            dataAccess.GetDataList(0, panelTypeStrings);

            List<ApertureType> apertureTypes = null;
            if(panelTypeStrings != null && panelTypeStrings.Count > 0)
            {
                apertureTypes = new List<ApertureType>();

                foreach(string panelTypeString in panelTypeStrings)
                {
                    ApertureType apertureType;
                    if (Enum.TryParse(panelTypeString, out apertureType))
                        apertureTypes.Add(apertureType);
                }
            }
            else
            {
                apertureTypes = new List<ApertureType>(Enum.GetValues(typeof(ApertureType)).Cast<ApertureType>());
            }

            List<bool> externals = new List<bool>();
            bool external = true;
            if(!dataAccess.GetData(1, ref external))
            {
                externals.Add(true);
                externals.Add(false);
            }
            else
            {
                externals.Add(external);
            }

            apertureTypes?.RemoveAll(x => x == ApertureType.Undefined);

            if(apertureTypes == null || apertureTypes.Count == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<GooApertureConstruction> gooApertureConstructions = new List<GooApertureConstruction>();
            foreach (ApertureType apertureType in apertureTypes)
                foreach (bool external_Temp in externals)
                    gooApertureConstructions.Add(new GooApertureConstruction(Analytical.Query.DefaultApertureConstruction(apertureType, external_Temp)));

            dataAccess.SetDataList(0, gooApertureConstructions);
        }
    }
}