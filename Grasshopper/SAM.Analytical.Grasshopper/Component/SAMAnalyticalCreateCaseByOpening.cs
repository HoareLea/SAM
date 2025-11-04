using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical.Grasshopper
{
    /// <summary>
    /// Create a case-specific AnalyticalModel by setting opening behaviour on selected apertures
    /// (makes a copy of the base model; the original stays unchanged).
    /// </summary>
    /// <remarks>
    /// <para>
    /// This component prepares a case-specific <see cref="AnalyticalModel"/> for parametric studies by
    /// applying opening settings (angle, factor, profile, function, colour, description) to one or more
    /// apertures. The component makes a copy of the base model so your original model is not changed.
    /// If no apertures are supplied, all apertures in the model are considered.
    /// </para>
    /// </remarks>
    public class SAMAnalyticalCreateCaseByOpening : GH_SAMVariableOutputParameterComponent
    {
        private const string DefaultOpeningFunction = "zdwno,0,19.00,21.00,99.00"; // default opening function (PartO)

        // ────────────────────────────────────────────────────────────────────────────────
        // Metadata
        // ────────────────────────────────────────────────────────────────────────────────

        /// <summary>Gets the unique ID for this component. Do not change this ID after release.</summary>
        public override Guid ComponentGuid => new("249dd3f9-200b-4d85-be35-da59f302f343");

        /// <summary>The latest version of this component.</summary>
        public override string LatestComponentVersion => "1.0.4";

        /// <summary>Component icon shown on the Grasshopper canvas.</summary>
        protected override System.Drawing.Bitmap Icon => Core.Convert.ToBitmap(Resources.SAM_Small);

        /// <summary>Display priority in the Grasshopper ribbon.</summary>
        public override GH_Exposure Exposure => GH_Exposure.primary;

        // Long, multi-line description for tooltips and docs (MEP-friendly SAM style)
        private const string DescriptionLong =
@"Create a case-specific AnalyticalModel by setting opening behaviour on selected apertures.

SUMMARY
  • Makes a copy of _baseAModel_; the original stays unchanged.
  • Applies opening settings to each chosen aperture: opening angle (deg) and optional
    factor, profile, function, colour and description.
  • If _apertures_ is empty, all model apertures are used.
  • If you supply fewer values than apertures (e.g., angles), the last value is reused.

INPUTS
  _baseAModel  (AnalyticalModel, required)
    Base SAM AnalyticalModel used as a template for this case.
    Type: SAM.Analytical.AnalyticalModel

  _apertures_  (Aperture[], optional)
    Specific apertures to update. Leave empty to use all apertures in the model.
    Type: SAM.Analytical.Aperture

  _openingAngles  (Number[], required)
    Opening angle(s) in degrees. Provide one value to apply to all, or a list matching
    the number of apertures. If the list is shorter, the last value is reused.

  descriptions_  (Text[], optional)
    Description per aperture (one value or a list).

  colours_  (Colour[], optional)
    Display colours per aperture (one value or a list).

  functions_  (Text[], optional)
    Opening function string used by SAM to drive opening behaviour.\nDefault (PartO): 'zdwno,0,19.00,21.00,99.00'.\nLeave the default unless you need custom logic.

  factors_  (Number[], optional)
    Adjustment factor (0–1 typically). If provided with a profile, it scales the profile.

  profiles_  (Profile[], optional)
    Time-varying profile for opening. When provided, profile-based opening is used.

  _sizePaneOnly_  (Boolean, optional, default = true)
    If true, opening size is based on the glass pane only; if false, the full aperture
    size is used.

OUTPUTS
  CaseAModel  (AnalyticalModel)
    Copy of the base model with updated opening settings on the selected apertures.

NOTES
  • Angles are in degrees.
  • You can feed one value to affect all apertures, or a list per-aperture.
  • If both profile and factor are given, the factor multiplies the profile result.

EXAMPLE
  1) Read a baseline AnalyticalModel → _baseAModel
  2) (Optional) Select apertures to control → _apertures_
  3) Set opening angle(s) (e.g., 15 or [10,20,30]) → _openingAngles
  4) (Optional) Provide factors_/profiles_/colours_/descriptions_/functions_
  5) (Optional) Toggle _sizePaneOnly_ as needed
  6) Get the case model → CaseAModel
";

        /// <summary>Initializes a new instance of the component.</summary>
        public SAMAnalyticalCreateCaseByOpening()
          : base(
                "SAMAnalytical.CreateCaseByOpening",
                "CreateCaseByOpening",
                DescriptionLong,
                "SAM", "Analytical")
        { }

        // ────────────────────────────────────────────────────────────────────────────────
        // Parameters
        // ────────────────────────────────────────────────────────────────────────────────

        /// <summary>Registers all the input parameters for this component.</summary>
        protected override GH_SAMParam[] Inputs
        {
            get
            {
                List<GH_SAMParam> result = [];

                // Base Analytical Model (required)
                var analyticalModelParam = new GooAnalyticalModelParam
                {
                    Name = "_baseAModel",
                    NickName = "_baseAModel",
                    Description = "Base SAM AnalyticalModel used as a template for this case.\nType: SAM.Analytical.AnalyticalModel\nRequired: Yes",
                    Access = GH_ParamAccess.item
                };
                result.Add(new GH_SAMParam(analyticalModelParam, ParamVisibility.Binding));

                // Apertures (optional)
                var gooApertureParam = new GooApertureParam
                {
                    Name = "_apertures_",
                    NickName = "_apertures_",
                    Description = "Apertures to update. Leave empty to use all apertures in the model.\nType: SAM.Analytical.Aperture\nRequired: No",
                    Access = GH_ParamAccess.list,
                    Optional = true
                };
                result.Add(new GH_SAMParam(gooApertureParam, ParamVisibility.Binding));

                // Opening angles (required list)
                var number = new global::Grasshopper.Kernel.Parameters.Param_Number
                {
                    Name = "_openingAngles",
                    NickName = "_openingAngles",
                    Description = "Opening angle(s) in degrees. One value applies to all; lists shorter than apertures reuse the last value.",
                    Access = GH_ParamAccess.list
                };
                result.Add(new GH_SAMParam(number, ParamVisibility.Binding));

                // Descriptions (optional)
                var @string = new global::Grasshopper.Kernel.Parameters.Param_String
                {
                    Name = "descriptions_",
                    NickName = "descriptions_",
                    Description = "Description per aperture (one value or a list).",
                    Access = GH_ParamAccess.list,
                    Optional = true
                };
                result.Add(new GH_SAMParam(@string, ParamVisibility.Voluntary));

                // Colours (optional)
                var colour = new global::Grasshopper.Kernel.Parameters.Param_Colour
                {
                    Name = "colours_",
                    NickName = "colours_",
                    Description = "Display colours per aperture (one value or a list).",
                    Access = GH_ParamAccess.list,
                    Optional = true
                };
                result.Add(new GH_SAMParam(colour, ParamVisibility.Voluntary));

                // Functions (optional; default persistent value)
                @string = new global::Grasshopper.Kernel.Parameters.Param_String
                {
                    Name = "functions_",
                    NickName = "functions_",
                    Description = "Opening function string used by SAM to drive opening behaviour.\nDefault (PartO): 'zdwno,0,19.00,21.00,99.00'.\nLeave the default unless you need custom logic.",
                    Access = GH_ParamAccess.list,
                    Optional = true
                };
                @string.SetPersistentData(DefaultOpeningFunction);
                result.Add(new GH_SAMParam(@string, ParamVisibility.Voluntary));

                // Factors (optional)
                var number2 = new global::Grasshopper.Kernel.Parameters.Param_Number
                {
                    Name = "factors_",
                    NickName = "factors_",
                    Description = "Adjustment factor (0–1 typically). If provided with a profile, it scales the profile.",
                    Access = GH_ParamAccess.list,
                    Optional = true
                };
                result.Add(new GH_SAMParam(number2, ParamVisibility.Voluntary));

                // Profiles (optional)
                var gooProfileParam = new GooProfileParam
                {
                    Name = "profiles_",
                    NickName = "profiles_",
                    Description = "Time-varying profile for opening. When provided, profile-based opening is used.",
                    Access = GH_ParamAccess.list,
                    Optional = true
                };
                result.Add(new GH_SAMParam(gooProfileParam, ParamVisibility.Voluntary));

                // Size pane only (optional; default true)
                var param_Boolean = new global::Grasshopper.Kernel.Parameters.Param_Boolean
                {
                    Name = "_sizePaneOnly_",
                    NickName = "_sizePaneOnly_",
                    Description = "If true, size is based on the glass pane only; if false, the full aperture size is used.",
                    Access = GH_ParamAccess.item,
                    Optional = true
                };
                param_Boolean.SetPersistentData(true);
                result.Add(new GH_SAMParam(param_Boolean, ParamVisibility.Voluntary));

                param_Boolean = new global::Grasshopper.Kernel.Parameters.Param_Boolean
                {
                    Name = "_concatenate_",
                    NickName = "_concatenate_",
                    Description = "concatenate",
                    Access = GH_ParamAccess.item,
                    Optional = true
                };
                param_Boolean.SetPersistentData(true);
                result.Add(new GH_SAMParam(param_Boolean, ParamVisibility.Binding));

                return [.. result];
            }
        }

        /// <summary>Registers all the output parameters for this component.</summary>
        protected override GH_SAMParam[] Outputs
        {
            get
            {
                List<GH_SAMParam> result = [];

                var analyticalModelParam = new GooAnalyticalModelParam
                {
                    Name = "CaseAModel",
                    NickName = "CaseAModel",
                    Description = "Copy of the base model with updated opening settings on selected apertures.",
                    Access = GH_ParamAccess.item
                };
                result.Add(new GH_SAMParam(analyticalModelParam, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_String param_String = new() { Name = "CaseDescription", NickName = "CaseDescription", Description = "Case Description", Access = GH_ParamAccess.item };
                result.Add(new GH_SAMParam(param_String, ParamVisibility.Binding));

                return [.. result];
            }
        }

        // ────────────────────────────────────────────────────────────────────────────────
        // Execution
        // ────────────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Core execution: read inputs, update apertures, and output the case model.
        /// </summary>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            // Base model (required)
            int index = Params.IndexOfInputParam("_baseAModel");
            AnalyticalModel analyticalModel = null;
            if (index == -1 || !dataAccess.GetData(index, ref analyticalModel) || analyticalModel == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "_baseAModel: Please supply a valid SAM AnalyticalModel.");
                return;
            }

            AdjacencyCluster adjacencyCluster = analyticalModel.AdjacencyCluster;

            // Apertures (optional)
            index = Params.IndexOfInputParam("_apertures_");
            List<Aperture> apertures = [];
            if (index != -1)
            {
                dataAccess.GetDataList(index, apertures);
            }

            if (apertures == null || apertures.Count == 0)
            {
                apertures = adjacencyCluster.GetApertures();
            }

            // Opening angles (required list)
            index = Params.IndexOfInputParam("_openingAngles");
            List<double> openingAngles = [];
            if (index != -1)
            {
                dataAccess.GetDataList(index, openingAngles);
            }

            // Optional lists
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

            // Process apertures
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

                    double factor = (factors != null && factors.Count != 0) ? (factors.Count > i ? factors[i] : factors.Last()) : double.NaN;

                    PartOOpeningProperties partOOpeningProperties = new (width, height, openingAngle);

                    double dischargeCoefficient = partOOpeningProperties.GetDischargeCoefficient();

                    ISingleOpeningProperties singleOpeningProperties = null;
                    if (profiles != null && profiles.Count != 0)
                    {
                        Profile profile = profiles.Count > i ? profiles[i] : profiles.Last();
                        ProfileOpeningProperties profileOpeningProperties = new (partOOpeningProperties.GetDischargeCoefficient(), profile);
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

                    string function_Temp = DefaultOpeningFunction;
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

            // Make a case model with the updated cluster (original model object is not changed)
            analyticalModel = new AnalyticalModel(analyticalModel, adjacencyCluster);

            index = Params.IndexOfOutputParam("CaseDescription");
            if (index != -1)
            {
                int index_Concatenate = Params.IndexOfInputParam("_concatenate_");
                bool concatenate = true;
                if (index_Concatenate != -1)
                {
                    dataAccess.GetData(index_Concatenate, ref concatenate);
                }

                string caseDescription = string.Empty;
                if (concatenate)
                {
                    if(!Core.Query.TryGetValue(analyticalModel, "CaseDescription", out caseDescription))
                    {
                        caseDescription = string.Empty;
                    }
                }

                if(string.IsNullOrWhiteSpace(caseDescription))
                {
                    caseDescription = "Case";
                }
                else
                {
                    caseDescription += "_";
                }

                string sufix = "ByOpening_";
                if (openingAngles is not null)
                {
                    sufix += string.Join("_", openingAngles.ConvertAll(x => string.Format("{0}deg", x)));
                }

                string value = caseDescription + sufix;

                dataAccess.SetData(index, value);
            }

            // Output
            index = Params.IndexOfOutputParam("CaseAModel");
            if (index != -1)
            {
                dataAccess.SetData(index, analyticalModel);
            }
        }
    }
}
