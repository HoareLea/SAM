using Grasshopper.Kernel;
using SAM.Weather.Grasshopper.Properties;
using SAM.Core;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMWeatherAdaptiveSetpointACCIByTemperature : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("e75af232-c217-4748-9243-1eb153e93e31");

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
        public SAMWeatherAdaptiveSetpointACCIByTemperature()
          : base("SAMWeather.AdaptiveSetpointACCIByTemperature", "SAMWeather.AdaptiveSetpointACCIByTemperature",
        "https://accim.readthedocs.io/en/latest/4_detailed%20use.html\n" +
        "https://github.com/dsanchez-garcia/accim/blob/master/accim/sample_files/jupyter_notebooks/addAccis/using_addAccis.ipynb\n" +
        "Adopted 2->INT ASHRAE55->90->3->Adap. Limits   Horizont. Extended\n" +
        "https://htmlpreview.github.io/?https://github.com/dsanchez-garcia/accim/blob/master/accim/docs/html_files/full_setpoint_table.html\n\n" +
        "1. Input Parameter:\n" +
        "   - dryBulbTemperature (External Weather Data): The dry bulb temperature from the weather data, which is used to calculate the upper and lower limits.\n" +
        "\n" +
        "2. Conditions:\n" +
        "   - Within Range (10°C to 33.5°C):\n" +
        "     - If the dryBulbTemperature is between 10°C and 33.5°C, inclusive, the following equations are used to calculate the upper and lower limits:\n" +
        "       - upperLimit = (dryBulbTemperature * 0.31) + 17.8 + 3.5\n" +
        "       - lowerLimit = (dryBulbTemperature * 0.31) + 17.8 - 3.5\n" +
        "   - Below Range (< 10°C):\n" +
        "     - If the dryBulbTemperature is below 10°C, a fixed dry bulb temperature of 10°C is used in the equations to calculate the upper and lower limits:\n" +
        "       - upperLimit = (10 * 0.31) + 17.8 + 3.5\n" +
        "       - lowerLimit = (10 * 0.31) + 17.8 - 3.5\n" +
        "   - Above Range (> 33.5°C):\n" +
        "     - If the dryBulbTemperature is above 33.5°C, a fixed dry bulb temperature of 33.5°C is used in the equations to calculate the upper and lower limits:\n" +
        "       - upperLimit = (33.5 * 0.31) + 17.8 + 3.5\n" +
        "       - lowerLimit = (33.5 * 0.31) + 17.8 - 3.5\n" +
        "\n" +
        "3. Output Parameters:\n" +
        "   - upperLimit: The calculated upper limit value.\n" +
        "   - lowerLimit: The calculated lower limit value.",
        "SAM", "Weather")
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
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_temperature", NickName = "_temperature", Description = "Dry Bulb Temperature.", Access = GH_ParamAccess.item }, ParamVisibility.Binding));

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
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "temperature", NickName = "temperature", Description = "Temperature", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number { Name = "upper", NickName = "upper", Description = "Upper Dry Bulb Temperature", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number { Name = "lower", NickName = "lower", Description = "Lower Dry Bulb Temperature", Access = GH_ParamAccess.item}, ParamVisibility.Binding));
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

            index = Params.IndexOfInputParam("_temperature");
            double temperature = double.NaN;
            if (index == -1 || !dataAccess.GetData(index, ref temperature) || double.IsNaN(temperature))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            Range<double> range = Weather.Query.DryBulbTemperatureRange(temperature);


            index = Params.IndexOfOutputParam("temperature");
            if (index != -1)
            {
                dataAccess.SetData(index, temperature);
            }

            index = Params.IndexOfOutputParam("upper");
            if (index != -1)
            {
                dataAccess.SetData(index, range.Max);
            }

            index = Params.IndexOfOutputParam("lower");
            if (index != -1)
            {
                dataAccess.SetData(index, range.Min);
            }
        }
    }
}