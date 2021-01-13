using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalCreateProfileByValues : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("2547159f-d7d0-4a8b-bed2-e5d89c552647");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.0";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        public override GH_Exposure Exposure => GH_Exposure.tertiary | GH_Exposure.obscure;

        protected override GH_SAMParam[] Inputs
        {
            get
            {
                List<GH_SAMParam> result = new List<GH_SAMParam>();
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "_name", NickName = "_name", Description = "Name of Profile", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_values", NickName = "_values", Description = "Values", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "_typeOrGroup", NickName = "_typeOrGroup", Description = "SAM Analytical ProfileType or ProfileGroup", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                return result.ToArray();
            }
        }

        protected override GH_SAMParam[] Outputs
        {
            get
            {
                List<GH_SAMParam> result = new List<GH_SAMParam>();
                result.Add(new GH_SAMParam(new GooProfileParam() {Name = "Profile", NickName = "Profile", Description = "SAM Analytical Profile", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                return result.ToArray();
            }
        }

        /// <summary>
        /// Updates PanelTypes for AdjacencyCluster
        /// </summary>
        public SAMAnalyticalCreateProfileByValues()
          : base("SAMAnalytical.CreateProfileByValues", "SAMAnalytical.CreateProfileByValues",
              "Creates SAM Analytical Profile By Values",
              "SAM", "Analytical")
        {
        }

        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            int index;

            index = Params.IndexOfInputParam("_name");
            if(index == -1)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            string name = null;
            if (!dataAccess.GetData(index, ref name) || string.IsNullOrWhiteSpace(name))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            index = Params.IndexOfInputParam("_names");
            if (index == -1)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<double> values = new List<double>();
            if (!dataAccess.GetDataList(index, values) || values == null || values.Count == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            index = Params.IndexOfInputParam("_typeOrGroup");
            if (index == -1)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            string typeOrGroup = null;
            if (!dataAccess.GetData(index, ref typeOrGroup) || string.IsNullOrWhiteSpace(typeOrGroup))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            ProfileType profileType = ProfileType.Undefined;
            ProfileGroup profileGroup = ProfileGroup.Undefined;

            if(!Core.Query.TryConvert(typeOrGroup, out profileType))
                if (!Core.Query.TryConvert(typeOrGroup, out profileGroup))
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                    return;
                }

            if(profileType == ProfileType.Undefined && profileGroup == ProfileGroup.Undefined)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            ProfileLibrary profileLibrary = null;

            index = Params.IndexOfInputParam("_profileLibrary_");
            if (index != -1)
                dataAccess.GetData(index, ref profileLibrary);

            if (profileLibrary == null)
                profileLibrary = ActiveSetting.Setting.GetValue<ProfileLibrary>(AnalyticalSettingParameter.DefaultProfileLibrary);

            if(profileLibrary == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            Profile profile = null;
            if (profileType != ProfileType.Undefined)
                profile = Create.Profile(name, profileType, values);
            else
                profile = Create.Profile(name, profileGroup, values);

            index = Params.IndexOfOutputParam("Profile");
            if (index != -1)
                dataAccess.SetData(index, new GooProfile(profile));
        }
    }
}