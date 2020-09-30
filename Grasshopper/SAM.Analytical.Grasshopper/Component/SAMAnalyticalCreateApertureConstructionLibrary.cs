using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalCreateApertureConstructionLibrary : GH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("e8b9f4e1-da46-4ab4-b5fe-47bfd842ee5c");

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalCreateApertureConstructionLibrary()
          : base("SAMAnalytical.CreateApertureConstructionLibrary", "SAMAnalytical.CreateApertureConstructionLibrary",
              "Create SAM ApertureConstructionLibrary",
              "SAM", "Analytical")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            inputParamManager.AddTextParameter("_name_", "_name_", "Name", GH_ParamAccess.item, "Default Construction Library");

            GooApertureConstructionParam gooConstructionParam = new GooApertureConstructionParam();
            gooConstructionParam.Optional = true;
            inputParamManager.AddParameter(gooConstructionParam, "_apertureConstructions_", "_apertureConstructions_", "SAM Analytical ApertureConstructions", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddParameter(new GooApertureConstructionLibraryParam(), "ApertureConstructionLibrary", "ApertureConstructionLibrary", "SAM Analytical ApertureConstructionLibrary", GH_ParamAccess.item);
            //outputParamManager.AddGenericParameter("Points", "Pts", "Snap points", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">
        /// The DA object is used to retrieve from inputs and store in outputs.
        /// </param>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            string name = null;
            if (!dataAccess.GetData(0, ref name) || name == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<ApertureConstruction> apertureConstructions = new List<ApertureConstruction>();
            dataAccess.GetDataList(1, apertureConstructions);

            ApertureConstructionLibrary result = new ApertureConstructionLibrary(name);
            apertureConstructions?.ForEach(x => result.Add(x));

            dataAccess.SetData(0, new GooApertureConstructionLibrary(result));
        }
    }
}