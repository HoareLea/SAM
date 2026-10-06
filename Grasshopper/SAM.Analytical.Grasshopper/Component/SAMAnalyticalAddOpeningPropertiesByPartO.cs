// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalAddOpeningPropertiesByPartO : GH_SAMVariableOutputParameterComponent
    {
        private static string function = "zdwno,0,19.00,21.00,99.00";

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("66d0ddd2-fc84-4218-9bf2-18afbbe8e8a7");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.7";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Core.Convert.ToBitmap(Resources.SAM_Small);

        public override GH_Exposure Exposure => GH_Exposure.primary;


        private const string Bb101Description = @"
SUMMARY
BB101/DfE discharge-coefficient (hinged windows) opening properties, plus the SAM Part O opening
RESTRICTION policy - whether, and when, an opening may be used for overheating ventilation.

What it computes
• Cd(α) per the DfE BB101 spreadsheet: Cd(α) = CdMax · (1 − exp(−k · αdeg)), {k, CdMax} by aspect-ratio (w/h).
• Adds opening properties (including Cd, and the restriction policy below) to apertures.

Geometry source & scope
• Dimensions are taken from the APERTURE PANE only (width = pane width, height = pane height), unless
  '_sizePaneOnly_' is set to false.
• If 'apertures_' is connected → only those apertures are processed.
• If 'apertures_' is NOT connected → ALL apertures found in the supplied Analytical/AdjacencyCluster are processed.
• The supplied Analytical object/AdjacencyCluster is NOT modified - an updated copy is returned on 'analytical'.

Areas (DfE convention)
• A_free = w·h ; A_eff = Cd · A_free ; A_eq = A_eff / 0.62 (orifice Cd0 used in DfE tool).

OPENING RESTRICTIONS ('restriction_')
This states SAM.Analytical POLICY only. TAS schedule/profile creation happens later, when the model is
converted/prepared for TAS - this component never creates a TBD schedule, a TAS profile, or references
SAM.Analytical.Tas.

• Unrestricted (default) - normal operation. The opening may be used for overheating ventilation at any
  hour, exactly as before this input existed. Unconnected = Unrestricted, so existing saved GH definitions
  behave exactly as they did before this input was added.
• NightClosed - the opening remains physically openable (width, height, opening angle, discharge
  coefficient and the existing 'zdwno' Function are all retained), but is unavailable for overheating
  ventilation outside the configured daytime period. Governed by 'openingHour_'/'closingHour_', a SAM/project
  MODELLING PRESET (default 08:00-23:00) - NOT a universal Approved Document O or TM59 regulatory hour.
  Reusable for any 'closed at night' case (acoustic, security, an internal door kept shut overnight, or
  otherwise) - this component records only THAT an opening is restricted, never WHY.
• AlwaysClosed - the aperture remains geometrically present (for physical/Part F purposes, and its discharge
  coefficient stays available), but contributes zero effective overheating-ventilation opening downstream in
  TAS. No 24-hour zero schedule is generated for this - TAS represents it as a zero opening factor instead.

'openingHour_' / 'closingHour_' (NightClosed only, default 8 / 23)
Hour (0-23) the opening becomes available / unavailable. Wraps overnight if closingHour_ < openingHour_
(e.g. 23→8). Equal values produce an always-unavailable (all-zero) schedule and are flagged. These are SAM
modelling defaults, not statutory hours.

'profiles_' precedence
'profiles_' is explicit, user-authored control - an advanced override that has existed on this component
since before 'restriction_'. If 'profiles_' is connected for an aperture, IT WINS: the aperture gets a
ProfileOpeningProperties built from your profile, and 'restriction_' has no effect for that aperture (a
warning is raised if 'restriction_' was left at something other than Unrestricted, so the conflict is never
silent). Leave 'profiles_' unconnected to let 'restriction_' govern the aperture instead.

TAS BEHAVIOUR
Downstream, SAM_Tas's aperture-control write reads the restriction/profile carried here and writes it into
the TBD's aperture-control profile: NightClosed's availability profile is created once by name and reused on
repeated preparation (no duplicate schedules); the Function ('zdwno,...') is never discarded by the
schedule, and vice versa. None of that happens in this component - it only states the policy.

A TM59/overheating result produced downstream is never a statement of full Part O compliance on its own.

Sources (BB101 discharge coefficient only)
• DfE “BB101 Calculation Tools – Discharge coefficient calculator.xlsx”
• BB101 (2018): Ventilation, thermal comfort & IAQ in schools
• ESFA Output Specification (GDB + Annex 2F)

EXAMPLE
Aperture -> SAMAnalytical.AddOpeningPropertiesByPartO (restriction_ = NightClosed, openingHour_ = 8,
closingHour_ = 23) -> SAMAnalytical.PreparePartOIteration -> To gbXML -> SAMAnalytical.WorkflowgbXML
(Simulation = true) -> open the exported TBD in TAS and inspect the aperture's Opening profile for the
'PartO_DayOpen_08_23' schedule.

A RESTRICTION AUTHORED HERE SURVIVES PREPARATION
SAMAnalytical.PreparePartOIteration never rewrites opening data to fit the stage it is asked to state - it
once reset restrictions to Unrestricted, which silently deleted this schedule from the model that reached
TAS. It now reports the comparison and changes nothing. Where the stage's 'Openings Restricted' assumption
disagrees with what you authored here, you get a WARNING naming the aperture, not a refusal: a restriction
records a fact about the building (noise, security, an internal door) and is orthogonal to which mitigation
stage is being assessed.
";


        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalAddOpeningPropertiesByPartO()
          : base("SAMAnalytical.AddOpeningPropertiesByPartO", "SAMAnalytical.AddOpeningPropertiesByPartO",
              Bb101Description,
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
                result.Add(new GH_SAMParam(new GooAnalyticalObjectParam() { Name = "_analytical", NickName = "_analytical", Description = "SAM Analytical Object such as AdjacencyCluster or AnalyticalModel", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooApertureParam() { Name = "apertures_", NickName = "apertures_", Description = "SAM Analytical Apertures", Access = GH_ParamAccess.list, Optional = true }, ParamVisibility.Binding));

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

                GooProfileParam gooProfileParam = new GooProfileParam() { Name = "profiles_", NickName = "profiles_", Description = "Advanced: an explicit, user-authored availability profile per aperture. When connected for an aperture, it WINS over restriction_ - the aperture gets a ProfileOpeningProperties built from this profile, and restriction_ has no effect for it (a warning is raised if restriction_ was left non-Unrestricted). Leave unconnected to let restriction_ govern the aperture.", Access = GH_ParamAccess.list, Optional = true };
                result.Add(new GH_SAMParam(gooProfileParam, ParamVisibility.Voluntary));

                global::Grasshopper.Kernel.Parameters.Param_Boolean param_Boolean = new() { Name = "_sizePaneOnly_", NickName = "_sizePaneOnly_", Description = "Size Pane Only", Access = GH_ParamAccess.item, Optional = true };
                param_Boolean.SetPersistentData(true);
                result.Add(new GH_SAMParam(param_Boolean, ParamVisibility.Voluntary));

                @string = new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "restriction_", NickName = "restriction_", Description = "Part O opening restriction: Unrestricted, NightClosed or AlwaysClosed. Unconnected = Unrestricted (existing behaviour is unchanged). States SAM.Analytical policy only - TAS schedule/profile creation happens later, during TAS conversion.", Access = GH_ParamAccess.list, Optional = true };
                @string.SetPersistentData(OpeningRestriction.Unrestricted.ToString());
                result.Add(new GH_SAMParam(@string, ParamVisibility.Voluntary));

                global::Grasshopper.Kernel.Parameters.Param_Integer param_Integer = new() { Name = "openingHour_", NickName = "openingHour_", Description = "NightClosed only. Hour (0-23) from which the opening becomes available. A SAM/project modelling default, not a universal Approved Document O or TM59 regulatory hour. Default 8 (08:00).", Access = GH_ParamAccess.list, Optional = true };
                param_Integer.SetPersistentData(8);
                result.Add(new GH_SAMParam(param_Integer, ParamVisibility.Voluntary));

                param_Integer = new global::Grasshopper.Kernel.Parameters.Param_Integer() { Name = "closingHour_", NickName = "closingHour_", Description = "NightClosed only. Hour (0-23) from which the opening becomes unavailable. A SAM/project modelling default, not a universal Approved Document O or TM59 regulatory hour. Default 23 (23:00).", Access = GH_ParamAccess.list, Optional = true };
                param_Integer.SetPersistentData(23);
                result.Add(new GH_SAMParam(param_Integer, ParamVisibility.Voluntary));

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
                result.Add(new GH_SAMParam(new GooAnalyticalObjectParam { Name = "analytical", NickName = "analytical", Description = "SAM Analytical", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooApertureParam() { Name = "apertures", NickName = "apertures", Description = "SAM Analytical Apertures", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooOpeningPropertiesParam() { Name = "openingProperties", NickName = "openingProperties", Description = "SAM Analytical IOpeningProperties", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "dischargeCoefficients", NickName = "dischargeCoefficients", Description = "Discharge Coefficients", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
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

            index = Params.IndexOfInputParam("_analytical");
            SAMObject sAMObject = null;
            if (index == -1 || !dataAccess.GetData(index, ref sAMObject) || sAMObject == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            AdjacencyCluster adjacencyCluster = null;
            if (sAMObject is AdjacencyCluster)
            {
                adjacencyCluster = new AdjacencyCluster((AdjacencyCluster)sAMObject);
            }
            else if (sAMObject is AnalyticalModel)
            {
                adjacencyCluster = ((AnalyticalModel)sAMObject).AdjacencyCluster;
            }

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

            index = Params.IndexOfInputParam("restriction_");
            List<string> restrictionTexts = [];
            if (index != -1)
            {
                dataAccess.GetDataList(index, restrictionTexts);
            }

            index = Params.IndexOfInputParam("openingHour_");
            List<int> openingHours = [];
            if (index != -1)
            {
                dataAccess.GetDataList(index, openingHours);
            }

            index = Params.IndexOfInputParam("closingHour_");
            List<int> closingHours = [];
            if (index != -1)
            {
                dataAccess.GetDataList(index, closingHours);
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

                    OpeningRestriction restriction = OpeningRestriction.Unrestricted;
                    if (restrictionTexts != null && restrictionTexts.Count != 0)
                    {
                        string restrictionText = restrictionTexts.Count > i ? restrictionTexts[i] : restrictionTexts.Last();
                        if (!string.IsNullOrWhiteSpace(restrictionText) && !Core.Query.TryGetEnum(restrictionText, out restriction))
                        {
                            AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, string.Format("'{0}' is not a valid Opening Restriction (use Unrestricted, NightClosed or AlwaysClosed) for aperture '{1}' - treated as Unrestricted.", restrictionText, aperture_Temp.Name));
                            restriction = OpeningRestriction.Unrestricted;
                        }
                    }

                    int openingHour = openingHours != null && openingHours.Count != 0 ? (openingHours.Count > i ? openingHours[i] : openingHours.Last()) : 8;
                    int closingHour = closingHours != null && closingHours.Count != 0 ? (closingHours.Count > i ? closingHours[i] : closingHours.Last()) : 23;

                    if (restriction == OpeningRestriction.NightClosed && ((openingHour % 24) + 24) % 24 == ((closingHour % 24) + 24) % 24)
                    {
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, string.Format("Aperture '{0}': openingHour_ and closingHour_ resolve to the same hour, so the NightClosed availability profile is never available (all-zero). Supply different hours if that is not intended.", aperture_Temp.Name));
                    }

                    PartOOpeningProperties partOOpeningProperties = new(width, height, openingAngle, restriction, openingHour, closingHour);

                    double dischargeCoefficient = partOOpeningProperties.GetDischargeCoefficient();

                    ISingleOpeningProperties singleOpeningProperties = null;
                    if (profiles != null && profiles.Count != 0)
                    {
                        if (restriction != OpeningRestriction.Unrestricted)
                        {
                            AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, string.Format("Aperture '{0}': profiles_ is connected, so the explicit custom profile is used and restriction_ ('{1}') has no effect. Disconnect profiles_, or leave restriction_ at Unrestricted, to avoid this ambiguity.", aperture_Temp.Name, restriction));
                        }

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

            if (sAMObject is AdjacencyCluster)
            {
                sAMObject = adjacencyCluster;
            }
            else if (sAMObject is AnalyticalModel)
            {
                sAMObject = new AnalyticalModel((AnalyticalModel)sAMObject, adjacencyCluster);
            }

            index = Params.IndexOfOutputParam("analytical");
            if (index != -1)
                dataAccess.SetData(index, sAMObject);

            index = Params.IndexOfOutputParam("apertures");
            if (index != -1)
                dataAccess.SetDataList(index, apertures_Result?.ConvertAll(x => new GooAperture(x)));

            index = Params.IndexOfOutputParam("dischargeCoefficients");
            if (index != -1)
                dataAccess.SetDataList(index, dischargeCoefficients_Result);

            index = Params.IndexOfOutputParam("openingProperties");
            if (index != -1)
                dataAccess.SetDataList(index, openingProperties_Result);
        }
    }
}
