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
        public override string LatestComponentVersion => "1.0.5";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
                protected override System.Drawing.Bitmap Icon => Core.Convert.ToBitmap(Resources.SAM_Small);

        public override GH_Exposure Exposure => GH_Exposure.primary;


        private const string Bb101Description = @"
BB101 / DfE Discharge Coefficient (hinged windows) + Opening Properties

What it does
• Computes Cd(α) per the DfE BB101 spreadsheet using:
  Cd(α) = CdMax · (1 − exp(−k · αdeg)), with {k, CdMax} selected by aspect-ratio (w/h).
• Adds opening properties (including Cd) to apertures.

Geometry source & scope
• Dimensions are taken from the APERTURE PANE only (width = pane width, height = pane height).
• If 'apertures_' is connected → only those apertures are processed.
• If 'apertures_' is NOT connected → ALL apertures found in the supplied Analytical/AdjacencyCluster are processed.

Areas & outputs
• A_free = w·h  (DfE convention)
• A_eff  = Cd · A_free
• A_eq   = A_eff / 0.62  (orifice Cd0 used in DfE tool)

Assumptions & caveats (DfE/BB101)
• Façade-normal flow; no reveal corrections.
• α < 10° is extrapolated (use with caution).
• For bottom-hung (hinge at bottom, top tilts out), a conservative 10–15% reduction in Cd may be appropriate.

Sources
• DfE “BB101 Calculation Tools – Discharge coefficient calculator.xlsx”
• BB101 (2018): Ventilation, thermal comfort & IAQ in schools
• ESFA Output Specification (GDB + Annex 2F)
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
                List<GH_SAMParam> result = new List<GH_SAMParam>();
                result.Add(new GH_SAMParam(new GooAnalyticalObjectParam() { Name = "_analytical", NickName = "_analytical", Description = "SAM Analytical Object such as AdjacencyCluster or AnalyticalModel", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooApertureParam() { Name = "apertures_", NickName = "apertures_", Description = "SAM Analytical Apertures", Access = GH_ParamAccess.list, Optional = true }, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_Number number = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_openingAngles", NickName = "_openingAngles", Description = "Opening Angles", Access = GH_ParamAccess.list};
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
                result.Add(new GH_SAMParam(new GooAnalyticalObjectParam { Name = "analytical", NickName = "analytical", Description = "SAM Analytical", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooApertureParam() { Name = "apertures", NickName = "apertures", Description = "SAM Analytical Apertures", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooOpeningPropertiesParam() { Name = "openingProperties", NickName = "openingProperties", Description = "SAM Analytical IOpeningProperties", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "dischargeCoefficients", NickName = "dischargeCoefficients", Description = "Discharge Coefficients", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
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

            index = Params.IndexOfInputParam("_analytical");
            SAMObject sAMObject = null;
            if (index == -1 || !dataAccess.GetData(index, ref sAMObject) || sAMObject == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            AdjacencyCluster adjacencyCluster = null;
            if(sAMObject is AdjacencyCluster)
            {
                adjacencyCluster = new AdjacencyCluster((AdjacencyCluster)sAMObject);
            }
            else if(sAMObject is AnalyticalModel)
            {
                adjacencyCluster = ((AnalyticalModel)sAMObject).AdjacencyCluster;
            }

            index = Params.IndexOfInputParam("apertures_");
            List<Aperture> apertures = new List<Aperture>();
            if (index != -1)
            {
                dataAccess.GetDataList(index, apertures);
            }

            if (apertures == null || apertures.Count == 0)
            {
                apertures = adjacencyCluster.GetApertures();
            }

            index = Params.IndexOfInputParam("_openingAngles");
            List<double> openingAngles = new List<double>();
            if (index != -1)
            {
                dataAccess.GetDataList(index, openingAngles);
            }

            index = Params.IndexOfInputParam("descriptions_");
            List<string> descriptions = new List<string>();
            if (index != -1)
            {
                dataAccess.GetDataList(index, descriptions);
            }

            index = Params.IndexOfInputParam("functions_");
            List<string> functions = new List<string>();
            if (index != -1)
            {
                dataAccess.GetDataList(index, functions);
            }

            index = Params.IndexOfInputParam("colours_");
            List<System.Drawing.Color> colors = new List<System.Drawing.Color>();
            if (index != -1)
            {
                dataAccess.GetDataList(index, colors);
            }

            index = Params.IndexOfInputParam("factors_");
            List<double> factors = new List<double>();
            if (index != -1)
            {
                dataAccess.GetDataList(index, factors);
            }

            index = Params.IndexOfInputParam("profiles_");
            List<Profile> profiles = new List<Profile>();
            if (index != -1)
            {
                dataAccess.GetDataList(index, profiles);
            }

            List<Aperture> apertures_Result = null;
            List<double> dischargeCoefficients_Result = null;
            List<IOpeningProperties> openingProperties_Result = null;

            if (apertures != null && openingAngles != null && apertures.Count > 0 && openingAngles.Count > 0)
            {
                apertures_Result = new List<Aperture>();
                dischargeCoefficients_Result = new List<double>();
                openingProperties_Result = new List<IOpeningProperties>();

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
                    double width = aperture_Temp.GetWidth(AperturePart.Pane);
                    double height = aperture_Temp.GetHeight(AperturePart.Pane);

                    double factor = factors != null && factors.Count != 0 ? factors.Count > i ? factors[i] : factors.Last() : double.NaN;

                    PartOOpeningProperties partOOpeningProperties = new PartOOpeningProperties(width, height, openingAngle);

                    double dischargeCoefficient = partOOpeningProperties.GetDischargeCoefficient();

                    ISingleOpeningProperties singleOpeningProperties = null;
                    if (profiles != null && profiles.Count != 0)
                    {
                        Profile profile = profiles.Count > i ? profiles[i] : profiles.Last();
                        ProfileOpeningProperties profileOpeningProperties = new ProfileOpeningProperties(partOOpeningProperties.GetDischargeCoefficient(), profile);
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

                    if(descriptions != null && descriptions.Count != 0)
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
                    if(panel.AddAperture(aperture_Temp))
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