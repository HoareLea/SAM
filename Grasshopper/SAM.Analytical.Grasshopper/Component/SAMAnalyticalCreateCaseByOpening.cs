using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.Marshalling;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalCreateCaseByOpening : GH_SAMVariableOutputParameterComponent
    {
        private static string function = "zdwno,0,19.00,21.00,99.00";

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new ("249dd3f9-200b-4d85-be35-da59f302f343");

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
        public SAMAnalyticalCreateCaseByOpening()
          : base("SAMAnalytical.CreateCaseByOpening", "SAMAnalytical.CreateCaseByOpening",
              "AAA",
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
                List<GH_SAMParam> result = [];

                GooAnalyticalModelParam analyticalModelParam = new () { Name = "_baseAModel", NickName = "_baseAModel", Description = "Analytical Model", Access = GH_ParamAccess.item };
                result.Add(new GH_SAMParam(analyticalModelParam, ParamVisibility.Binding));

                GooApertureParam gooApertureParam = new() { Name = "_apertures_", NickName = "_apertures_", Description = "SAM Apertures", Access = GH_ParamAccess.list, Optional = true };
                result.Add(new GH_SAMParam(gooApertureParam, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_Number number = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_openingAngles", NickName = "_openingAngles", Description = "Opening Angles", Access = GH_ParamAccess.list };
                result.Add(new GH_SAMParam(number, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_String @string = new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "descriptions_", NickName = "descriptions_", Description = "Descriptions", Access = GH_ParamAccess.list, Optional = true };
                result.Add(new GH_SAMParam(@string, ParamVisibility.Voluntary));

                global::Grasshopper.Kernel.Parameters.Param_Colour colour = new global::Grasshopper.Kernel.Parameters.Param_Colour() { Name = "colours_", NickName = "colours_", Description = "Colours", Access = GH_ParamAccess.list, Optional = true };
                result.Add(new GH_SAMParam(colour, ParamVisibility.Voluntary));

                @string = new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "functions_", NickName = "functions_", Description = "Functions", Access = GH_ParamAccess.list, Optional = true };
                @string.SetPersistentData(function);
                result.Add(new GH_SAMParam(@string, ParamVisibility.Voluntary));

                number = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "factors_", NickName = "factors_", Description = "Factors", Access = GH_ParamAccess.list, Optional = true };
                result.Add(new GH_SAMParam(number, ParamVisibility.Voluntary));

                GooProfileParam gooProfileParam = new GooProfileParam() { Name = "profiles_", NickName = "profiles_", Description = "Profiles", Access = GH_ParamAccess.list, Optional = true };
                result.Add(new GH_SAMParam(gooProfileParam, ParamVisibility.Voluntary));

                global::Grasshopper.Kernel.Parameters.Param_Boolean param_Boolean = new() { Name = "_sizePaneOnly_", NickName = "_sizePaneOnly_", Description = "Size Pane Only", Access = GH_ParamAccess.item, Optional = true };
                param_Boolean.SetPersistentData(true);
                result.Add(new GH_SAMParam(param_Boolean, ParamVisibility.Voluntary));

                return [.. result];
            }
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override GH_SAMParam[] Outputs
        {
            get
            {
                List<GH_SAMParam> result = [];

                GooAnalyticalModelParam analyticalModelParam = new () { Name = "CaseAModel", NickName = "CaseAModel", Description = "SAM AnalyticalModel", Access = GH_ParamAccess.item };
                result.Add(new GH_SAMParam(analyticalModelParam, ParamVisibility.Binding));

                return [.. result];
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
            index = Params.IndexOfInputParam("_baseAModel");
            AnalyticalModel analyticalModel = null;
            if (index == -1 || !dataAccess.GetData(index, ref analyticalModel) || analyticalModel == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            AdjacencyCluster adjacencyCluster = analyticalModel.AdjacencyCluster;

            index = Params.IndexOfInputParam("apertures_");
            List<Aperture> apertures = [];
            if (index != -1)
            {
                dataAccess.GetDataList(index, apertures);
            }

            if (apertures == null || apertures.Count == 0)
            {
                apertures = adjacencyCluster.GetApertures();
            }

            index = Params.IndexOfInputParam("_openingAngles");
            List<double> openingAngles = [];
            if (index != -1)
            {
                dataAccess.GetDataList(index, openingAngles);
            }

            index = Params.IndexOfInputParam("descriptions_");
            List<string> descriptions = [];
            if (index != -1)
            {
                dataAccess.GetDataList(index, descriptions);
            }

            index = Params.IndexOfInputParam("functions_");
            List<string> functions = [];
            if (index != -1)
            {
                dataAccess.GetDataList(index, functions);
            }

            index = Params.IndexOfInputParam("colours_");
            List<System.Drawing.Color> colors = [];
            if (index != -1)
            {
                dataAccess.GetDataList(index, colors);
            }

            index = Params.IndexOfInputParam("factors_");
            List<double> factors = [];
            if (index != -1)
            {
                dataAccess.GetDataList(index, factors);
            }

            index = Params.IndexOfInputParam("profiles_");
            List<Profile> profiles = [];
            if (index != -1)
            {
                dataAccess.GetDataList(index, profiles);
            }

            index = Params.IndexOfInputParam("_sizePaneOnly_");
            bool paneSizeOnly = true;
            if (index != -1)
            {
                dataAccess.GetData(index, ref paneSizeOnly);
            }

            List<Aperture> apertures_Result = null;
            List<double> dischargeCoefficients_Result = null;
            List<IOpeningProperties> openingProperties_Result = null;

            if (apertures != null && openingAngles != null && apertures.Count > 0 && openingAngles.Count > 0)
            {
                apertures_Result = [];
                dischargeCoefficients_Result = [];
                openingProperties_Result = [];

                for (int i = 0; i < apertures.Count; i++)
                {
                    Aperture aperture = apertures[i];

                    Panel panel = adjacencyCluster.GetPanel(aperture);
                    if (panel == null)
                    {
                        continue;
                    }

                    Aperture aperture_Temp = panel.GetAperture(aperture.Guid);
                    if (aperture_Temp == null)
                    {
                        continue;
                    }

                    panel = Create.Panel(panel);
                    aperture_Temp = new Aperture(aperture_Temp);

                    double openingAngle = openingAngles.Count > i ? openingAngles[i] : openingAngles.Last();
                    double width = paneSizeOnly ? aperture_Temp.GetWidth(AperturePart.Pane) : aperture_Temp.GetWidth();
                    double height = paneSizeOnly ? aperture_Temp.GetHeight(AperturePart.Pane) : aperture_Temp.GetHeight();

                    double factor = factors != null && factors.Count != 0 ? factors.Count > i ? factors[i] : factors.Last() : double.NaN;

                    PartOOpeningProperties partOOpeningProperties = new(width, height, openingAngle);

                    double dischargeCoefficient = partOOpeningProperties.GetDischargeCoefficient();

                    ISingleOpeningProperties singleOpeningProperties = null;
                    if (profiles != null && profiles.Count != 0)
                    {
                        Profile profile = profiles.Count > i ? profiles[i] : profiles.Last();
                        ProfileOpeningProperties profileOpeningProperties = new(partOOpeningProperties.GetDischargeCoefficient(), profile);
                        if (!double.IsNaN(factor))
                        {
                            profileOpeningProperties.Factor = factor;
                        }

                        singleOpeningProperties = profileOpeningProperties;
                    }
                    else
                    {
                        if (!double.IsNaN(factor))
                        {
                            partOOpeningProperties.Factor = factor;
                        }

                        singleOpeningProperties = partOOpeningProperties;
                    }

                    if (descriptions != null && descriptions.Count != 0)
                    {
                        string description = descriptions.Count > i ? descriptions[i] : descriptions.Last();
                        singleOpeningProperties.SetValue(OpeningPropertiesParameter.Description, description);
                    }

                    string function_Temp = function;
                    if (functions != null && functions.Count != 0)
                    {
                        function_Temp = functions.Count > i ? functions[i] : functions.Last();
                    }
                    singleOpeningProperties.SetValue(OpeningPropertiesParameter.Function, function_Temp);

                    if (colors != null && colors.Count != 0)
                    {
                        System.Drawing.Color color = colors.Count > i ? colors[i] : colors.Last();
                        aperture_Temp.SetValue(ApertureParameter.Color, color);
                    }
                    else
                    {
                        aperture_Temp.SetValue(ApertureParameter.Color, Analytical.Query.Color(ApertureType.Window, AperturePart.Pane, true));
                    }

                    aperture_Temp.AddSingleOpeningProperties(singleOpeningProperties);

                    panel.RemoveAperture(aperture.Guid);
                    if (panel.AddAperture(aperture_Temp))
                    {
                        adjacencyCluster.AddObject(panel);
                        apertures_Result.Add(aperture_Temp);
                        dischargeCoefficients_Result.Add(singleOpeningProperties.GetDischargeCoefficient());
                        openingProperties_Result.Add(singleOpeningProperties);
                    }
                }
            }

            analyticalModel = new AnalyticalModel(analyticalModel, adjacencyCluster);

            index = Params.IndexOfOutputParam("CaseAModel");
            if (index != -1)
            {
                dataAccess.SetData(index, analyticalModel);
            }
        }
    }
}