using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalSetInternalConditionPerArea : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("c93b42a5-f553-4d84-aad3-c06639b7e9a5");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.0";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
                protected override System.Drawing.Bitmap Icon => Core.Convert.ToBitmap(Resources.SAM_Small);

        public override GH_Exposure Exposure => GH_Exposure.primary;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalSetInternalConditionPerArea()
          : base("SAMAnalytical.SetInternalConditionPerArea", "SAMAnalytical.SetInternalConditionPerArea",
              "Set Internal Condition Per Area",
              "SAM", "Analytical03")
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
                result.Add(new GH_SAMParam(new GooAnalyticalModelParam() { Name = "_analyticalModel", NickName = "_analyticalModel", Description = "SAM Analytical AnalyticalModel", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooInternalConditionParam() { Name = "_internalCondition", NickName = "_internalCondition", Description = "SAM Analytical InternalCondition", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_Boolean boolean = new global::Grasshopper.Kernel.Parameters.Param_Boolean() { Name = "_includeInvalid_", NickName = "_includeInvalid_", Description = "Include invalid values (NaN) or null Parameter", Access = GH_ParamAccess.item };
                boolean.SetPersistentData(true);
                result.Add(new GH_SAMParam(boolean, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_String @string = new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "_comparisonType_", NickName = "_comparisonType_", Description = "Number ComparisonType", Access = GH_ParamAccess.item };
                @string.SetPersistentData(Core.NumberComparisonType.Less);
                result.Add(new GH_SAMParam(@string, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_Number number = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_value_", NickName = "_value_", Description = "Area value", Access = GH_ParamAccess.item };
                number.SetPersistentData(2);
                result.Add(new GH_SAMParam(number, ParamVisibility.Binding));

                result.Add(new GH_SAMParam(new GooProfileLibraryParam() { Name = "profileLibrary_", NickName = "profileLibrary_", Description = "SAM Analytical ProfileLibrary", Access = GH_ParamAccess.item, Optional = true}, ParamVisibility.Voluntary));

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
                result.Add(new GH_SAMParam(new GooAnalyticalModelParam() { Name = "AnalyticalModel", NickName = "AnalyticalModel", Description = "SAM Analytical AnalyticalModel", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooSpaceParam() { Name = "Spaces", NickName = "Spaces", Description = "SAM Analytical Spaces", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
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

            AnalyticalModel analyticalModel = null;
            index = Params.IndexOfInputParam("_analyticalModel");
            if (index == -1 || !dataAccess.GetData(index, ref analyticalModel) || analyticalModel == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            InternalCondition internalCondition = null;
            index = Params.IndexOfInputParam("_internalCondition");
            if (index != -1)
                dataAccess.GetData(index, ref internalCondition);

            ProfileLibrary profileLibrary = null;
            index = Params.IndexOfInputParam("profileLibrary_");
            if (index != -1)
                dataAccess.GetData(index, ref profileLibrary);

            if(profileLibrary == null)
                profileLibrary = ActiveSetting.Setting.GetValue<ProfileLibrary>(AnalyticalSettingParameter.DefaultProfileLibrary);

            bool includeInvalid = true;
            index = Params.IndexOfInputParam("_includeInvalid_");
            if(index != -1)
                dataAccess.GetData(index, ref includeInvalid);

            Core.NumberComparisonType numberComparisonType = Core.NumberComparisonType.Less;
            index = Params.IndexOfInputParam("_comparisonType_");
            if (index != -1)
            {
                string numberComparisonTypestring = null;
                if (dataAccess.GetData(index, ref numberComparisonTypestring))
                    numberComparisonType = Core.Query.Enum<Core.NumberComparisonType>(numberComparisonTypestring);
            }

            double value = 2;
            index = Params.IndexOfInputParam("_value_");
            if (index != -1)
                dataAccess.GetData(index, ref value);

            AdjacencyCluster adjacencyCluster = analyticalModel.AdjacencyCluster;
            List<Space> spaces = null;
            if(adjacencyCluster != null)
            {
                List<Space> spaces_All = adjacencyCluster.GetSpaces();
                if(spaces_All != null)
                {
                    spaces = new List<Space>();
                    foreach (Space space in spaces_All)
                    {
                        if (space == null)
                            continue;

                        double area = space.CalculatedArea(adjacencyCluster);
                        if(double.IsNaN(area))
                        {
                            if(includeInvalid)
                            {
                                space.InternalCondition = internalCondition;
                                adjacencyCluster.AddObject(space);
                                spaces.Add(space);
                            }
                            continue;
                        }
                        
                        if (!Core.Query.Compare(area, value, numberComparisonType))
                            continue;

                        space.InternalCondition = internalCondition;
                        adjacencyCluster.AddObject(space);
                        spaces.Add(space);
                    }

                    if (spaces != null && spaces.Count > 0)
                    {
                        IEnumerable<Profile> profiles = Analytical.Query.Profiles(adjacencyCluster, profileLibrary);
                        profileLibrary = new ProfileLibrary("Default Material Library", profiles);

                        adjacencyCluster.AssignSpaceColors();

                        analyticalModel = new AnalyticalModel(analyticalModel, adjacencyCluster, analyticalModel.MaterialLibrary, profileLibrary);
                    }
                }
            }

            index = Params.IndexOfOutputParam("AnalyticalModel");
            if (index != -1)
                dataAccess.SetData(index, analyticalModel);

            index = Params.IndexOfOutputParam("Spaces");
            if (index != -1)
                dataAccess.SetDataList(index, spaces?.ConvertAll(x => new GooSpace(x)));

        }
    }
}