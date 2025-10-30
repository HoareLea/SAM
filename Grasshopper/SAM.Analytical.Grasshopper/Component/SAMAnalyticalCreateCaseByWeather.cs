using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core;
using SAM.Core.Grasshopper;
using SAM.Weather;
using SAM.Weather.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    /// <summary>
    /// Create a case-specific AnalyticalModel by applying WeatherData (makes a copy of the base model when weather data is given).
    /// </summary>
    /// <remarks>
    /// <para>
    /// This component prepares a case-specific <see cref="AnalyticalModel"/> for parametric studies by applying
    /// a given <see cref="WeatherData"/>. When weather data is supplied, the base model is copied so the original
    /// model is not changed; the copy then has its weather set (see <see cref="AnalyticalModelParameter.WeatherData"/>).
    /// If no weather data is provided, the input model is passed through unchanged.
    /// </para>
    /// <para>
    /// Typical use: branch a calibrated baseline model into multiple case models where each case uses a different
    /// weather file (e.g., EPW, TRY, DSY1–DSY3). Combine with the other <c>SAMAnalytical.CreateCaseBy...</c> nodes
    /// to sweep scenarios.
    /// </para>
    /// </remarks>
    public class SAMAnalyticalCreateCaseByWeather : GH_SAMVariableOutputParameterComponent
    {
        // ────────────────────────────────────────────────────────────────────────────────
        // Metadata
        // ────────────────────────────────────────────────────────────────────────────────

        /// <summary>Gets the unique ID for this component. Do not change this ID after release.</summary>
        public override Guid ComponentGuid => new("4f54c13d-eac7-4b13-a07e-4f620753b26a");

        /// <summary>The latest version of this component.</summary>
        public override string LatestComponentVersion => "1.0.2";

        /// <summary>Component icon shown on the Grasshopper canvas.</summary>
        protected override System.Drawing.Bitmap Icon => Core.Convert.ToBitmap(Resources.SAM_Small);

        /// <summary>Display priority in the Grasshopper ribbon.</summary>
        public override GH_Exposure Exposure => GH_Exposure.primary;

        // Long, multi-line description for tooltips and docs (SAM style guide).
        private const string DescriptionLong =
@"Create a case-specific AnalyticalModel by applying WeatherData.

SUMMARY
  - If _weatherData_ is supplied: the component makes a COPY of _baseAModel_ and sets its WeatherData.
    The original model you provided is not changed.
  - If _weatherData_ is omitted or null: the input model is passed through unchanged (no copy is made).

INPUTS
  _baseAModel  (AnalyticalModel, required)
    Base SAM AnalyticalModel used as a template for this case.
    Type: SAM.Analytical.AnalyticalModel

  _weatherData_  (WeatherData, optional)
    Accepts only SAM WeatherData. Use SAMAnalytical.LoadWeatherData or Tas.CreateWeatherData to
    import/convert external weather files (EPW, TRY, DSY1–DSY3, etc.) into SAM.Weather.WeatherData
    before connecting here.
    Type: SAM.Weather.WeatherData
    When provided, the component copies _baseAModel_ (the original stays unchanged) and sets the copy's weather.

OUTPUTS
  CaseAModel  (AnalyticalModel)
    The resulting case AnalyticalModel. If _weatherData_ was provided, this is a copy of _baseAModel_ with weather set;
    otherwise it returns the same model you supplied.

WEATHER SCENARIOS
  • TRY vs DSY: TRY (Typical Reference Year) is commonly used for general/annual energy modelling,
    while DSY (Design Summer Year) is used for overheating risk assessment.
  • DSY1: Moderately warm summer year with heat events of ~7-year return period.
  • DSY2: Summer with short, intense heat events.
  • DSY3: Summer with long, less intense heat events.

NOTES
  • Use separate CreateCase nodes (e.g., ByShade, ByApertureConstruction, etc.) to build full factorial scenario sets.
  • If you always want to keep source models unchanged, consider copying upstream as a general practice.

EXAMPLE
  1) Read a baseline AnalyticalModel → _baseAModel
  2) Import EPW/TRY/DSY files → use SAMAnalytical.LoadWeatherData or Tas.CreateWeatherData to make WeatherData → _weatherData_
  3) Iterate with a value list / series to produce several CaseAModel outputs.
";

        /// <summary>Initializes a new instance of the component.</summary>
        public SAMAnalyticalCreateCaseByWeather()
          : base(
                "SAMAnalytical.CreateCaseByWeather",
                "CreateCaseByWeather",
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

                // Weather Data (optional)
                var gooWeatherDataParam = new GooWeatherDataParam
                {
                    Name = "_weatherData_",
                    NickName = "_weatherData_",
                    Description = @"Accepts only SAM WeatherData.
Use SAMAnalytical.LoadWeatherData or Tas.CreateWeatherData to import/convert EPW, TRY, DSY1–DSY3 into SAM.Weather.WeatherData, then connect here.
Type: SAM.Weather.WeatherData
Required: No
When provided, the component copies _baseAModel_ (original stays unchanged) and sets the copy's weather.",
                    Access = GH_ParamAccess.item,
                    Optional = true
                };
                result.Add(new GH_SAMParam(gooWeatherDataParam, ParamVisibility.Binding));

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
                    Description = "Resulting case AnalyticalModel.\nIf _weatherData_ is provided → copy of _baseAModel_ with weather set.\nIf not provided → returns the same model you supplied.",
                    Access = GH_ParamAccess.item
                };
                result.Add(new GH_SAMParam(analyticalModelParam, ParamVisibility.Binding));

                return [.. result];
            }
        }

        // ────────────────────────────────────────────────────────────────────────────────
        // Execution
        // ────────────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// This is the method that actually does the work.
        /// The DA object is used to retrieve from inputs and store in outputs.
        /// </summary>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            // Read base model (required)
            int index = Params.IndexOfInputParam("_baseAModel");
            AnalyticalModel analyticalModel = null;
            if (index == -1 || !dataAccess.GetData(index, ref analyticalModel) || analyticalModel == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "_baseAModel: Please supply a valid SAM AnalyticalModel.");
                return;
            }

            // Read optional weather
            index = Params.IndexOfInputParam("_weatherData_");
            WeatherData weatherData = null;
            if (index != -1)
            {
                dataAccess.GetData(index, ref weatherData);
            }

            // Apply weather to a copy when provided
            if (weatherData != null)
            {
                analyticalModel = new AnalyticalModel(analyticalModel); // copy of the base model (original stays unchanged)
                analyticalModel.SetValue(AnalyticalModelParameter.WeatherData, weatherData);
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