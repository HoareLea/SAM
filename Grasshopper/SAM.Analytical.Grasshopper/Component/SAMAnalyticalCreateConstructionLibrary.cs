using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalCreateConstructionLibrary : GH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("0f7c26aa-01f6-4791-a8a5-20252643d85b");

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalCreateConstructionLibrary()
          : base("SAMAnalytical.CreateConstructionLibrary", "SAMAnalytical.CreateConstructionLibrary",
              "Create SAM Construction Library",
              "SAM", "Analytical")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            inputParamManager.AddTextParameter("_name_", "_name_", "Name", GH_ParamAccess.item, "Default Construction Library");

            GooConstructionParam gooConstructionParam = new GooConstructionParam();
            gooConstructionParam.Optional = true;
            inputParamManager.AddParameter(gooConstructionParam, "_constructions_", "_constructions_", "SAM Analytical Constructions", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddParameter(new GooConstructionLibraryParam(), "ConstructionLibrary", "ConstructionLibrary", "SAM Analytical ConstructionLibrary", GH_ParamAccess.item);
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

            List<Construction> constructions = new List<Construction>();
            dataAccess.GetDataList(1, constructions);

            ConstructionLibrary result = new ConstructionLibrary(name);
            constructions?.ForEach(x => result.Add(x));

            dataAccess.SetData(0, new GooConstructionLibrary(result));
        }
    }
}