using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAddMissingProfiles : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("172d0812-90e3-4e1b-8dde-516b2cba5013");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.0";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        public override GH_Exposure Exposure => GH_Exposure.primary;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAddMissingProfiles()
          : base("SAMAnalytical.AddMissingProfiles", "SAMAnalytical.AddMissingProfiles",
              "Add Missing Profiles",
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
                
                result.Add(new GH_SAMParam(new GooArchitecturalModelParam() { Name = "_architecturalModel", NickName = "_architecturalModel", Description = "SAM Architectural ArchitecturalModel", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                
                GooProfileLibraryParam gooProfileLibraryParam = new GooProfileLibraryParam { Name = "_profileLibrary_", NickName = "_profileLibrary_", Description = "SAM Analytical Profile Library", Access = GH_ParamAccess.item };
                gooProfileLibraryParam.PersistentData.Append(new GooProfileLibrary(Analytical.Query.DefaultProfileLibrary()));
                result.Add(new GH_SAMParam(gooProfileLibraryParam, ParamVisibility.Binding));

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
                result.Add(new GH_SAMParam(new GooArchitecturalModelParam() { Name = "architecturalModel", NickName = "architecturalModel", Description = "SAM Architectural ArchitecturalModel", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooProfileParam() { Name = "profiles", NickName = "profiles", Description = "SAM Analytical Profiles", Access = GH_ParamAccess.list }, ParamVisibility.Voluntary));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "missingProfileNames", NickName = "missingProfileNames", Description = "Missing Profile Names. This Profiles could not be found in ProfileLibrary and are still missing", Access = GH_ParamAccess.list }, ParamVisibility.Voluntary));
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
            int index = -1;

            ArchitecturalModel architecturalModel = null;
            index = Params.IndexOfInputParam("_architecturalModel");
            if (index == -1 || !dataAccess.GetData(index, ref architecturalModel) || architecturalModel == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            ProfileLibrary profileLibrary = null;
            index = Params.IndexOfInputParam("_profileLibrary_");
            if (index == -1 || !dataAccess.GetData(index, ref profileLibrary) || profileLibrary == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            architecturalModel = new ArchitecturalModel(architecturalModel);

            List<Profile> profiles = architecturalModel.AddMissingProfiles(profileLibrary, out Dictionary<ProfileType, List<string>>  missingProfileNames);

            index = Params.IndexOfOutputParam("architecturalModel");
            if (index != -1)
                dataAccess.SetData(index, new GooArchitecturalModel(architecturalModel));

            index = Params.IndexOfOutputParam("materials");
            if (index != -1)
                dataAccess.SetDataList(index, profiles?.ConvertAll(x => new GooProfile(x)));

            index = Params.IndexOfOutputParam("missingMaterialNames");
            if (index != -1)
            {
                List<string> missingProfileNames_Temp = new List<string>();
                if(missingProfileNames != null)
                {
                    foreach(KeyValuePair<ProfileType, List<string>> keyValuePair in missingProfileNames)
                    {
                        missingProfileNames_Temp.AddRange(keyValuePair.Value);
                    }
                }

                dataAccess.SetDataList(index, missingProfileNames_Temp);
            }
                
        }
    }
}