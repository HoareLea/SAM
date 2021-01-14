using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalCreateProfileByProfiles : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("cdeb014d-3cde-4fa2-aff9-ae2f2507556d");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.2";

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
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "_profiles", NickName = "_profiles", Description = "Names of the profiles or SAM Analytical Profiles to be used", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "_typeOrGroup", NickName = "_typeOrGroup", Description = "SAM Analytical ProfileType or ProfileGroup", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add( new GH_SAMParam(new GooProfileLibraryParam() { Name = "_profileLibrary_", NickName = "_profileLibrary_", Description = "SAM Analytical ProfileLibrary", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Voluntary));
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
        public SAMAnalyticalCreateProfileByProfiles()
          : base("SAMAnalytical.CreateProfileByProfiles", "SAMAnalytical.CreateProfileByProfiles",
              "Creates SAM Analytical Profile By Profiles",
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

            index = Params.IndexOfInputParam("_profiles");
            if (index == -1)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<GH_ObjectWrapper> objectWrappers = new List<GH_ObjectWrapper>();
            if (!dataAccess.GetDataList(index, objectWrappers) || objectWrappers == null || objectWrappers.Count == 0)
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

            List<string> names = new List<string>();
            profileLibrary = new ProfileLibrary(profileLibrary);
            foreach(GH_ObjectWrapper gH_ObjectWrapper in objectWrappers)
            {
                object @object = gH_ObjectWrapper?.Value;
                if (@object == null)
                    continue;

                if(@object is GH_String)
                {
                    names.Add(((GH_String)@object).Value);
                }
                else if (gH_ObjectWrapper.Value is GooProfile)
                {
                    Profile profile_Temp  =((GooProfile)gH_ObjectWrapper.Value).Value;
                    profileLibrary.Add(profile_Temp);
                    names.Add(profile_Temp.Name);
                }
                else if(@object is string)
                {
                    names.Add((string)@object);
                }
                else if(gH_ObjectWrapper.Value is Profile)
                {
                    Profile profile_Temp = (Profile)gH_ObjectWrapper.Value;
                    profileLibrary.Add(profile_Temp);
                    names.Add(profile_Temp.Name);
                }
            }

            if(names == null || names.Count == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            Profile profile = null;
            if (profileType != ProfileType.Undefined)
                profile = profileLibrary.Profile(name, profileType, names, true);
            else
                profile = profileLibrary.Profile(name, profileGroup, names, true);

            index = Params.IndexOfOutputParam("Profile");
            if (index != -1)
                dataAccess.SetData(index, new GooProfile(profile));
        }
    }
}