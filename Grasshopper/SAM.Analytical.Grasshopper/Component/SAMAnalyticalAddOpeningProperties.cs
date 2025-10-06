using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalAddOpeningProperties : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("09cbf514-7cd1-45b5-8458-c6728f86cf07");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.1";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
                protected override System.Drawing.Bitmap Icon => Core.Convert.ToBitmap(Resources.SAM_Small);

        public override GH_Exposure Exposure => GH_Exposure.primary;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalAddOpeningProperties()
          : base("SAMAnalytical.AddOpeningProperties", "SAMAnalytical.AddOpeningProperties",
              "Add Opening Properties to given apertures",
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

                global::Grasshopper.Kernel.Parameters.Param_Number number = new global::Grasshopper.Kernel.Parameters.Param_Number() 
                {
                    Name = "_dischargeCoefficients", 
                    NickName = "_dischargeCoefficients", 
                    Description = @"Discharge Coefficients (dimensionless)
                DefaulDefault  discharge coefficient: 0.62 (typical for a sharp-edged rectangular opening).
                Discharge coefficients account for the ‘friction’ of an opening; the lower the coefficient, the more the aperture resists flow.

                Guidance values:
                • 0.00 – Completely discount stack ventilation from the calculation.
                • 0.45 – Unobstructed window with an insect screen.
                • 0.65 – Unobstructed window with NO insect screen.",
                    Access = GH_ParamAccess.list
                };
                number.SetPersistentData(0.62);
                result.Add(new GH_SAMParam(number, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_String @string = new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "descriptions_", NickName = "descriptions_", Description = "Descriptions", Access = GH_ParamAccess.list, Optional = true };
                result.Add(new GH_SAMParam(@string, ParamVisibility.Voluntary));

                global::Grasshopper.Kernel.Parameters.Param_Colour colour = new global::Grasshopper.Kernel.Parameters.Param_Colour() { Name = "colours_", NickName = "colours_", Description = "Colours", Access = GH_ParamAccess.list, Optional = true };
                result.Add(new GH_SAMParam(colour, ParamVisibility.Voluntary));

                @string = new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "functions_", NickName = "functions_", Description = "Functions \nexample This function in define for the Approved Document O\nzdwon,0,19.00,21.00,99.00", Access = GH_ParamAccess.list, Optional = true };
                result.Add(new GH_SAMParam(@string, ParamVisibility.Voluntary));

                number = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "factors_", NickName = "factors_", Description = "Factors", Access = GH_ParamAccess.list, Optional = true };
                result.Add(new GH_SAMParam(number, ParamVisibility.Voluntary));

                GooProfileParam gooProfileParam = new GooProfileParam() { Name = "profiles_", NickName = "profiles_", Description = "Profiles\nSchedule in Tas so 24h bool", Access = GH_ParamAccess.list, Optional = true };
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

            index = Params.IndexOfInputParam("_dischargeCoefficients");
            List<double> dischargeCoefficients = new List<double>();
            if (index != -1)
            {
                dataAccess.GetDataList(index, dischargeCoefficients);
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

            if (apertures != null && dischargeCoefficients != null && apertures.Count > 0 && dischargeCoefficients.Count > 0)
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

                    double dischargeCoefficient = dischargeCoefficients.Count > i ? dischargeCoefficients[i] : dischargeCoefficients.Last();

                    double factor = factors != null && factors.Count != 0 ? factors.Count > i ? factors[i] : factors.Last() : double.NaN;

                    ISingleOpeningProperties singleOpeningProperties = null;
                    if (profiles != null && profiles.Count != 0)
                    {
                        Profile profile = profiles.Count > i ? profiles[i] : profiles.Last();
                        ProfileOpeningProperties profileOpeningProperties = new ProfileOpeningProperties(dischargeCoefficient, profile);
                        if(!double.IsNaN(factor))
                        {
                            profileOpeningProperties.Factor = factor;
                        }

                        singleOpeningProperties = profileOpeningProperties;
                    }
                    else
                    {
                        OpeningProperties openingProperties = new OpeningProperties(dischargeCoefficient);
                        if (!double.IsNaN(factor))
                        {
                            openingProperties.Factor = factor;
                        }

                        singleOpeningProperties = openingProperties;
                    }

                    if(descriptions != null && descriptions.Count != 0)
                    {
                        string description = descriptions.Count > i ? descriptions[i] : descriptions.Last();
                        singleOpeningProperties.SetValue(OpeningPropertiesParameter.Description, description);
                    }

                    if (functions != null && functions.Count != 0)
                    {
                        string function = functions.Count > i ? functions[i] : functions.Last();
                        singleOpeningProperties.SetValue(OpeningPropertiesParameter.Function, function);
                    }

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