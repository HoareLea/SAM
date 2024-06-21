using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalCreateApertureConstructionLibrary : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("e8b9f4e1-da46-4ab4-b5fe-47bfd842ee5c");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.1";

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
        protected override GH_SAMParam[] Inputs
        {
            get
            {
                List<GH_SAMParam> result = new List<GH_SAMParam>();
                result.Add(new GH_SAMParam(new GooApertureConstructionLibraryParam() { Name = "apertureConstructionLibrary_", NickName = "apertureConstructionLibrary_", Description = "SAM Analytical ApertureConstructionLibrary", Optional = true, Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "name_", NickName = "name_", Description = "Name", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooApertureConstructionParam() { Name = "apertureConstructions_", NickName = "apertureConstructions_", Description = "SAM Analytical Aperture Constructions", Access = GH_ParamAccess.list, Optional = true }, ParamVisibility.Binding));
                return result.ToArray();
            }
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override GH_SAMParam[] Outputs
        {
            get
            {
                List<GH_SAMParam> result = new List<GH_SAMParam>();
                result.Add(new GH_SAMParam(new GooApertureConstructionLibraryParam() { Name = "apertureConstructionLibrary", NickName = "apertureConstructionLibrary", Description = "SAM Analytical ApertureConstructionLibrary", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                return result.ToArray();
            }
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">
        /// The DA object is used to retrieve from inputs and store in outputs.
        /// </param>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            int index;

            index = Params.IndexOfInputParam("name_");
            string name = "Default ApertureConstructionLibrary";
            if (index != -1)
            {
                dataAccess.GetData(index, ref name);
            }

            index = Params.IndexOfInputParam("apertureConstructionLibrary_");
            ApertureConstructionLibrary apertureConstructionLibrary = new ApertureConstructionLibrary(name);
            if (index != -1)
            {
                dataAccess.GetData(index, ref apertureConstructionLibrary);
            }

            if(apertureConstructionLibrary == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            index = Params.IndexOfInputParam("apertureConstructions_");
            if(index != -1)
            {
                List<ApertureConstruction> apertureConstructions = new List<ApertureConstruction>();
                if(dataAccess.GetDataList(index, apertureConstructions))
                {
                    apertureConstructions?.ForEach(x => apertureConstructionLibrary.Add(x));
                }
            }

            index = Params.IndexOfOutputParam("apertureConstructionLibrary");
            if(index != -1)
            {
                dataAccess.SetData(index, new GooApertureConstructionLibrary(apertureConstructionLibrary));
            }
        }
    }
}